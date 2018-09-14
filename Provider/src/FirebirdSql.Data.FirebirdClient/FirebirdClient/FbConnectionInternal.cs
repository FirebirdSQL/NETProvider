/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using FirebirdSql.Data.Common;
using System.Linq;
#if !NETSTANDARD1_6
using FirebirdSql.Data.Schema;
#endif

namespace FirebirdSql.Data.FirebirdClient
{
	internal class FbConnectionInternal : IDisposable
	{
		#region Fields

		private IDatabase _db;
		private FbTransaction _activeTransaction;
		private List<WeakReference<FbCommand>> _preparedCommands;
		private FbConnectionString _options;
		private FbConnection _owningConnection;
		private bool _disposed;
#if !NETSTANDARD1_6
		private FbEnlistmentNotification _enlistmentNotification;
#endif

		#endregion

		#region Properties

		public IDatabase Database
		{
			get { return _db; }
		}

		public bool HasActiveTransaction
		{
			get
			{
				return _activeTransaction != null && !_activeTransaction.IsCompleted;
			}
		}

		public FbTransaction ActiveTransaction
		{
			get { return _activeTransaction; }
		}

		public FbConnection OwningConnection
		{
			get { return _owningConnection; }
		}

		public bool IsEnlisted
		{
			get
			{
#if NETSTANDARD1_6
				return false;
#else
				return _enlistmentNotification != null && !_enlistmentNotification.IsCompleted;
#endif
			}
		}

		public FbConnectionString Options
		{
			get { return _options; }
		}

		public bool CancelDisabled { get; set; }

		#endregion

		#region Constructors

		public FbConnectionInternal(FbConnectionString options)
		{
			_preparedCommands = new List<WeakReference<FbCommand>>();

			_options = options;
		}

		#endregion

		#region IDisposable Methods

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				Disconnect();
			}
		}

		#endregion

		#region Create and Drop database methods

		public void CreateDatabase(DatabaseParameterBuffer dpb)
		{
			var db = ClientFactory.CreateDatabase(_options);
			db.CreateDatabase(dpb, _options.DataSource, _options.Port, _options.Database, _options.CryptKey);
		}

		public void DropDatabase()
		{
			var db = ClientFactory.CreateDatabase(_options);
			db.Attach(BuildDpb(db, _options), _options.DataSource, _options.Port, _options.Database, _options.CryptKey);
			db.DropDatabase();
		}

		#endregion

		#region Connect and Disconnect methods

		public void Connect()
		{
			if (Charset.GetCharset(_options.Charset) == null)
			{
				throw new FbException("Invalid character set specified");
			}

			try
			{
				_db = ClientFactory.CreateDatabase(_options);
				_db.Charset = Charset.GetCharset(_options.Charset);
				_db.Dialect = _options.Dialect;
				_db.PacketSize = _options.PacketSize;

				var dpb = BuildDpb(_db, _options);

				if (string.IsNullOrEmpty(_options.UserID) && string.IsNullOrEmpty(_options.Password))
				{
					_db.AttachWithTrustedAuth(dpb, _options.DataSource, _options.Port, _options.Database, _options.CryptKey);
				}
				else
				{
					_db.Attach(dpb, _options.DataSource, _options.Port, _options.Database, _options.CryptKey);
				}
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		public void Disconnect()
		{
			if (_db != null)
			{
				try
				{
					_db.Dispose();
				}
				catch
				{ }
				finally
				{
					_db = null;
					_owningConnection = null;
					_options = null;
				}
			}
		}

		#endregion

		#region Transaction Handling Methods

		public FbTransaction BeginTransaction(IsolationLevel level, string transactionName)
		{
			EnsureActiveTransaction();

			try
			{
				_activeTransaction = new FbTransaction(_owningConnection, level);
				_activeTransaction.BeginTransaction();

				if (transactionName != null)
				{
					_activeTransaction.Save(transactionName);
				}
			}
			catch (IscException ex)
			{
				DisposeTransaction();
				throw new FbException(ex.Message, ex);
			}

			return _activeTransaction;
		}

		public FbTransaction BeginTransaction(FbTransactionOptions options, string transactionName)
		{
			EnsureActiveTransaction();

			try
			{
				_activeTransaction = new FbTransaction(_owningConnection, IsolationLevel.Unspecified);
				_activeTransaction.BeginTransaction(options);

				if (transactionName != null)
				{
					_activeTransaction.Save(transactionName);
				}
			}
			catch (IscException ex)
			{
				DisposeTransaction();
				throw new FbException(ex.Message, ex);
			}

			return _activeTransaction;
		}

		public void DisposeTransaction()
		{
			if (_activeTransaction != null && !IsEnlisted)
			{
				_activeTransaction.Dispose();
				_activeTransaction = null;
			}
		}

		public void TransactionCompleted()
		{
			for (var i = 0; i < _preparedCommands.Count; i++)
			{
				if (!_preparedCommands[i].TryGetTarget(out var command))
					continue;

				if (command.Transaction != null)
				{
					command.DisposeReader();
					command.Transaction = null;
				}
			}
		}

		#endregion

		#region Transaction Enlistement

#if !NETSTANDARD1_6
		public void EnlistTransaction(System.Transactions.Transaction transaction)
		{
			if (_owningConnection != null)
			{
				if (_enlistmentNotification != null && _enlistmentNotification.SystemTransaction == transaction)
					return;

				if (HasActiveTransaction)
				{
					throw new ArgumentException("Unable to enlist in transaction, a local transaction already exists");
				}
				if (_enlistmentNotification != null)
				{
					throw new ArgumentException("Already enlisted in a transaction");
				}

				_enlistmentNotification = new FbEnlistmentNotification(this, transaction);
				_enlistmentNotification.Completed += new EventHandler(EnlistmentCompleted);
			}
		}

		private void EnlistmentCompleted(object sender, EventArgs e)
		{
			_enlistmentNotification = null;
		}

		public FbTransaction BeginTransaction(System.Transactions.IsolationLevel isolationLevel)
		{
			switch (isolationLevel)
			{
				case System.Transactions.IsolationLevel.Chaos:
					return BeginTransaction(System.Data.IsolationLevel.Chaos, null);

				case System.Transactions.IsolationLevel.ReadUncommitted:
					return BeginTransaction(System.Data.IsolationLevel.ReadUncommitted, null);

				case System.Transactions.IsolationLevel.RepeatableRead:
					return BeginTransaction(System.Data.IsolationLevel.RepeatableRead, null);

				case System.Transactions.IsolationLevel.Serializable:
					return BeginTransaction(System.Data.IsolationLevel.Serializable, null);

				case System.Transactions.IsolationLevel.Snapshot:
					return BeginTransaction(System.Data.IsolationLevel.Snapshot, null);

				case System.Transactions.IsolationLevel.Unspecified:
					return BeginTransaction(System.Data.IsolationLevel.Unspecified, null);

				case System.Transactions.IsolationLevel.ReadCommitted:
				default:
					return BeginTransaction(System.Data.IsolationLevel.ReadCommitted, null);
			}
		}
#endif

		#endregion

		#region Schema Methods

#if !NETSTANDARD1_6
		public DataTable GetSchema(string collectionName, string[] restrictions)
		{
			return FbSchemaFactory.GetSchema(_owningConnection, collectionName, restrictions);
		}
#endif

		#endregion

		#region Prepared Commands Methods

		public void AddPreparedCommand(FbCommand command)
		{
			var position = -1;
			for (var i = 0; i < _preparedCommands.Count; i++)
			{
				if (!_preparedCommands[i].TryGetTarget(out var current))
				{
					position = i;
				}
				else
				{
					if (current == command)
					{
						return;
					}
				}
			}
			if (position != -1)
			{
				_preparedCommands[position].SetTarget(command);
			}
			else
			{
				_preparedCommands.Add(new WeakReference<FbCommand>(command));
			}
		}

		public void RemovePreparedCommand(FbCommand command)
		{
			for (var i = _preparedCommands.Count - 1; i >= 0; i--)
			{
				var item = _preparedCommands[i];
				if (item.TryGetTarget(out var current) && current == command)
				{
					_preparedCommands.RemoveAt(i);
					return;
				}
			}
		}

		public void ReleasePreparedCommands()
		{
			for (var i = 0; i < _preparedCommands.Count; i++)
			{
				if (!_preparedCommands[i].TryGetTarget(out var current))
					continue;

				try
				{
					current.Release();
				}
				catch (IOException)
				{
					// If an IO error occurs weh trying to release the command
					// avoid it. ( It maybe the connection to the server was down
					// for unknown reasons. )
				}
				catch (IscException ex)
				{
					if (ex.ErrorCode != IscCodes.isc_net_read_err &&
						ex.ErrorCode != IscCodes.isc_net_write_err &&
						ex.ErrorCode != IscCodes.isc_network_error)
					{
						throw;
					}
				}
			}
			_preparedCommands.Clear();
		}

		#endregion

		#region Firebird Events Methods

		public void CloseEventManager()
		{
			if (_db != null && _db.HasRemoteEventSupport)
			{
				_db.CloseEventManager();
			}
		}

		#endregion

		#region Private Methods

		private DatabaseParameterBuffer BuildDpb(IDatabase db, FbConnectionString options)
		{
			var dpb = new DatabaseParameterBuffer();

			dpb.Append(IscCodes.isc_dpb_version1);
			dpb.Append(IscCodes.isc_dpb_dummy_packet_interval, new byte[] { 120, 10, 0, 0 });
			dpb.Append(IscCodes.isc_dpb_sql_dialect, new byte[] { options.Dialect, 0, 0, 0 });
			dpb.Append(IscCodes.isc_dpb_lc_ctype, options.Charset);
			if (options.DbCachePages > 0)
			{
				dpb.Append(IscCodes.isc_dpb_num_buffers, options.DbCachePages);
			}
			if (!string.IsNullOrEmpty(options.UserID))
			{
				dpb.Append(IscCodes.isc_dpb_user_name, options.UserID);
			}
			if (!string.IsNullOrEmpty(options.Role))
			{
				dpb.Append(IscCodes.isc_dpb_sql_role_name, options.Role);
			}
			dpb.Append(IscCodes.isc_dpb_connect_timeout, options.ConnectionTimeout);
			dpb.Append(IscCodes.isc_dpb_process_id, GetProcessId());
			dpb.Append(IscCodes.isc_dpb_process_name, GetProcessName());
			dpb.Append(IscCodes.isc_dpb_client_version, _VersionInfo.Version);
			if (options.NoDatabaseTriggers)
			{
				dpb.Append(IscCodes.isc_dpb_no_db_triggers, 1);
			}
			if (options.NoGarbageCollect)
			{
				dpb.Append(IscCodes.isc_dpb_no_garbage_collect, (byte)0);
			}

			return dpb;
		}

		private string GetProcessName()
		{
			return GetSystemWebHostingPath() ?? GetRealProcessName() ?? string.Empty;
		}


		private string GetSystemWebHostingPath()
		{
#if NETSTANDARD1_6 || NETSTANDARD2_0
			return null;
#else
			var assembly = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.Equals("System.Web", StringComparison.Ordinal)).FirstOrDefault();
			if (assembly == null)
				return null;
			// showing ApplicationPhysicalPath may be wrong because of connection pooling
			// better idea?
			return (string)assembly.GetType("System.Web.Hosting.HostingEnvironment").GetProperty("ApplicationPhysicalPath").GetValue(null, null);
#endif
		}

		private string GetRealProcessName()
		{
			string FromProcess()
			{
				try
				{
					return Process.GetCurrentProcess().MainModule.FileName;
				}
				catch (InvalidOperationException)
				{
					return null;
				}
			}
			return Assembly.GetEntryAssembly()?.Location ?? FromProcess();
		}

		private int GetProcessId()
		{
			try
			{
				return Process.GetCurrentProcess().Id;
			}
			catch (InvalidOperationException)
			{
				return -1;
			}
		}

		private void EnsureActiveTransaction()
		{
			if (HasActiveTransaction)
				throw new InvalidOperationException("A transaction is currently active. Parallel transactions are not supported.");
		}
		#endregion

		#region Cancelation
		public void EnableCancel()
		{
			_db.CancelOperation(IscCodes.fb_cancel_enable);
			CancelDisabled = false;
		}

		public void DisableCancel()
		{
			_db.CancelOperation(IscCodes.fb_cancel_disable);
			CancelDisabled = true;
		}

		public void CancelCommand()
		{
			_db.CancelOperation(IscCodes.fb_cancel_raise);
		}
		#endregion

		#region Infrastructure
		public FbConnectionInternal SetOwningConnection(FbConnection owningConnection)
		{
			_owningConnection = owningConnection;
			return this;
		}
		#endregion
	}
}
