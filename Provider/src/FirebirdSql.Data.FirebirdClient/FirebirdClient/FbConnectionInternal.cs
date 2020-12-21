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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FirebirdSql.Data.Client;
using FirebirdSql.Data.Common;
using FirebirdSql.Data.Schema;

namespace FirebirdSql.Data.FirebirdClient
{
	internal class FbConnectionInternal
	{
		#region Fields

		private IDatabase _db;
		private FbTransaction _activeTransaction;
		private HashSet<FbCommand> _preparedCommands;
		private ConnectionString _options;
		private FbConnection _owningConnection;
		private FbEnlistmentNotification _enlistmentNotification;

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
				return _enlistmentNotification != null && !_enlistmentNotification.IsCompleted;
			}
		}

		public ConnectionString Options
		{
			get { return _options; }
		}

		public bool CancelDisabled { get; private set; }

		#endregion

		#region Constructors

		public FbConnectionInternal(ConnectionString options)
		{
			_preparedCommands = new HashSet<FbCommand>();

			_options = options;
		}

		#endregion

		#region Create and Drop database methods

		public async Task CreateDatabase(int pageSize, bool forcedWrites, bool overwrite, AsyncWrappingCommonArgs async)
		{
			var db = await ClientFactory.CreateDatabase(_options, async).ConfigureAwait(false);

			var dpb = db.CreateDatabaseParameterBuffer();

			dpb.Append(IscCodes.isc_dpb_dummy_packet_interval, new byte[] { 120, 10, 0, 0 });
			dpb.Append(IscCodes.isc_dpb_sql_dialect, new byte[] { _options.Dialect, 0, 0, 0 });
			if (!string.IsNullOrEmpty(_options.UserID))
			{
				dpb.Append(IscCodes.isc_dpb_user_name, _options.UserID);
			}
			if (_options.Charset.Length > 0)
			{
				var charset = Charset.GetCharset(_options.Charset);
				if (charset == null)
				{
					throw new ArgumentException("Character set is not valid.");
				}
				else
				{
					dpb.Append(IscCodes.isc_dpb_set_db_charset, charset.Name);
				}
			}
			dpb.Append(IscCodes.isc_dpb_force_write, (short)(forcedWrites ? 1 : 0));
			dpb.Append(IscCodes.isc_dpb_overwrite, (overwrite ? 1 : 0));
			if (pageSize > 0)
			{
				dpb.Append(IscCodes.isc_dpb_page_size, pageSize);
			}

			try
			{
				if (string.IsNullOrEmpty(_options.UserID) && string.IsNullOrEmpty(_options.Password))
				{
					await db.CreateDatabaseWithTrustedAuth(dpb, _options.DataSource, _options.Port, _options.Database, _options.CryptKey, async).ConfigureAwait(false);
				}
				else
				{
					await db.CreateDatabase(dpb, _options.DataSource, _options.Port, _options.Database, _options.CryptKey, async).ConfigureAwait(false);
				}
			}
			finally
			{
				await db.Detach(async).ConfigureAwait(false);
			}
		}

		public async Task DropDatabase(AsyncWrappingCommonArgs async)
		{
			var db = await ClientFactory.CreateDatabase(_options, async).ConfigureAwait(false);
			try
			{
				if (string.IsNullOrEmpty(_options.UserID) && string.IsNullOrEmpty(_options.Password))
				{
					await db.AttachWithTrustedAuth(BuildDpb(db, _options), _options.DataSource, _options.Port, _options.Database, _options.CryptKey, async).ConfigureAwait(false);
				}
				else
				{
					await db.Attach(BuildDpb(db, _options), _options.DataSource, _options.Port, _options.Database, _options.CryptKey, async).ConfigureAwait(false);
				}
				await db.DropDatabase(async).ConfigureAwait(false);
			}
			finally
			{
				await db.Detach(async).ConfigureAwait(false);
			}
		}

		#endregion

		#region Connect and Disconnect methods

		public async Task Connect(AsyncWrappingCommonArgs async)
		{
			if (Charset.GetCharset(_options.Charset) == null)
			{
				throw new FbException("Invalid character set specified");
			}

			try
			{
				_db = await ClientFactory.CreateDatabase(_options, async).ConfigureAwait(false);
				_db.Charset = Charset.GetCharset(_options.Charset);
				_db.Dialect = _options.Dialect;
				_db.PacketSize = _options.PacketSize;

				var dpb = BuildDpb(_db, _options);

				if (string.IsNullOrEmpty(_options.UserID) && string.IsNullOrEmpty(_options.Password))
				{
					await _db.AttachWithTrustedAuth(dpb, _options.DataSource, _options.Port, _options.Database, _options.CryptKey, async).ConfigureAwait(false);
				}
				else
				{
					await _db.Attach(dpb, _options.DataSource, _options.Port, _options.Database, _options.CryptKey, async).ConfigureAwait(false);
				}
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		public async Task Disconnect(AsyncWrappingCommonArgs async)
		{
			if (_db != null)
			{
				try
				{
					await _db.Detach(async).ConfigureAwait(false);
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

		public async Task<FbTransaction> BeginTransaction(IsolationLevel level, string transactionName, AsyncWrappingCommonArgs async)
		{
			EnsureActiveTransaction();

			try
			{
				_activeTransaction = new FbTransaction(_owningConnection, level);
				await _activeTransaction.BeginTransaction(async).ConfigureAwait(false);

				if (transactionName != null)
				{
					_activeTransaction.Save(transactionName);
				}
			}
			catch (IscException ex)
			{
				await DisposeTransaction(async).ConfigureAwait(false);
				throw new FbException(ex.Message, ex);
			}

			return _activeTransaction;
		}

		public async Task<FbTransaction> BeginTransaction(FbTransactionOptions options, string transactionName, AsyncWrappingCommonArgs async)
		{
			EnsureActiveTransaction();

			try
			{
				_activeTransaction = new FbTransaction(_owningConnection, IsolationLevel.Unspecified);
				await _activeTransaction.BeginTransaction(options, async).ConfigureAwait(false);

				if (transactionName != null)
				{
					_activeTransaction.Save(transactionName);
				}
			}
			catch (IscException ex)
			{
				await DisposeTransaction(async).ConfigureAwait(false);
				throw new FbException(ex.Message, ex);
			}

			return _activeTransaction;
		}

		public async Task DisposeTransaction(AsyncWrappingCommonArgs async)
		{
			if (_activeTransaction != null && !IsEnlisted)
			{
#if NET48 || NETSTANDARD2_0
				_activeTransaction.Dispose();
				await Task.CompletedTask.ConfigureAwait(false);
#else
				await async.AsyncSyncCallNoCancellation(_activeTransaction.DisposeAsync, _activeTransaction.Dispose).ConfigureAwait(false);
#endif
				_activeTransaction = null;
			}
		}

		public async Task TransactionCompleted(AsyncWrappingCommonArgs async)
		{
			foreach (var command in _preparedCommands)
			{
				if (command.Transaction != null)
				{
					await command.DisposeReader(async).ConfigureAwait(false);
					command.Transaction = null;
				}
			}
		}

#endregion

#region Transaction Enlistment

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

		public Task<FbTransaction> BeginTransaction(System.Transactions.IsolationLevel isolationLevel, AsyncWrappingCommonArgs async)
		{
			var il = isolationLevel switch
			{
				System.Transactions.IsolationLevel.Chaos => IsolationLevel.Chaos,
				System.Transactions.IsolationLevel.ReadUncommitted => IsolationLevel.ReadUncommitted,
				System.Transactions.IsolationLevel.RepeatableRead => IsolationLevel.RepeatableRead,
				System.Transactions.IsolationLevel.Serializable => IsolationLevel.Serializable,
				System.Transactions.IsolationLevel.Snapshot => IsolationLevel.Snapshot,
				System.Transactions.IsolationLevel.Unspecified => IsolationLevel.Unspecified,
				_ => IsolationLevel.ReadCommitted,
			};
			return BeginTransaction(il, null, async);
		}

#endregion

#region Schema Methods

		public Task<DataTable> GetSchema(string collectionName, string[] restrictions, AsyncWrappingCommonArgs async)
		{
			return FbSchemaFactory.GetSchema(_owningConnection, collectionName, restrictions, async);
		}

#endregion

#region Prepared Commands Methods

		public void AddPreparedCommand(FbCommand command)
		{
			if (_preparedCommands.Contains(command))
				return;
			_preparedCommands.Add(command);
		}

		public void RemovePreparedCommand(FbCommand command)
		{
			_preparedCommands.Remove(command);
		}

		public async Task ReleasePreparedCommands(AsyncWrappingCommonArgs async)
		{
			// copy the data because the collection will be modified via RemovePreparedCommand from Release
			var data = _preparedCommands.ToList();
			foreach (var item in data)
			{
				try
				{
					await item.Release(async).ConfigureAwait(false);
				}
				catch (IOException)
				{
					// If an IO error occurs when trying to release the command
					// avoid it. (It maybe the connection to the server was down
					// for unknown reasons.)
				}
				catch (IscException ex) when (ex.ErrorCode == IscCodes.isc_network_error
					|| ex.ErrorCode == IscCodes.isc_net_read_err
					|| ex.ErrorCode == IscCodes.isc_net_write_err)
				{ }
			}
		}

#endregion

#region Firebird Events Methods

		public Task CloseEventManager(AsyncWrappingCommonArgs async)
		{
			if (_db != null && _db.HasRemoteEventSupport)
			{
				return _db.CloseEventManager(async);
			}
			return Task.CompletedTask;
		}

#endregion

#region Private Methods

		private void EnsureActiveTransaction()
		{
			if (HasActiveTransaction)
				throw new InvalidOperationException("A transaction is currently active. Parallel transactions are not supported.");
		}

		private static DatabaseParameterBufferBase BuildDpb(IDatabase db, ConnectionString options)
		{
			var dpb = db.CreateDatabaseParameterBuffer();

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
			dpb.Append(IscCodes.isc_dpb_client_version, GetClientVersion());
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

		private static string GetProcessName()
		{
			return GetSystemWebHostingPath() ?? GetRealProcessName() ?? string.Empty;
		}


		private static string GetSystemWebHostingPath()
		{
#if NETSTANDARD2_0 || NET5_0
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

		private static string GetRealProcessName()
		{
			static string FromProcess()
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

		private static int GetProcessId()
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

		private static string GetClientVersion()
		{
			return typeof(FbConnectionInternal).GetTypeInfo().Assembly.GetName().Version.ToString();
		}
#endregion

#region Cancelation
		public async Task EnableCancel(AsyncWrappingCommonArgs async)
		{
			await _db.CancelOperation(IscCodes.fb_cancel_enable, async).ConfigureAwait(false);
			CancelDisabled = false;
		}

		public async Task DisableCancel(AsyncWrappingCommonArgs async)
		{
			await _db.CancelOperation(IscCodes.fb_cancel_disable, async).ConfigureAwait(false);
			CancelDisabled = true;
		}

		public Task CancelCommand(AsyncWrappingCommonArgs async)
		{
			return _db.CancelOperation(IscCodes.fb_cancel_raise, async);
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
