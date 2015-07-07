/*
 *	Firebird ADO.NET Data provider for .NET and Mono
 *
 *	   The contents of this file are subject to the Initial
 *	   Developer's Public License Version 1.0 (the "License");
 *	   you may not use this file except in compliance with the
 *	   License. You may obtain a copy of the License at
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *	   express or implied. See the License for the specific
 *	   language governing rights and limitations under the License.
 *
 *	Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *	All Rights Reserved.
 *
 *  Contributors:
 *      Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Diagnostics;
using System.IO;

using FirebirdSql.Data.Common;
using FirebirdSql.Data.Schema;


namespace FirebirdSql.Data.FirebirdClient
{
	internal class FbConnectionInternal : MarshalByRefObject, IDisposable
	{
		#region Fields

		private IDatabase db;
		private FbTransaction activeTransaction;
		private List<WeakReference> preparedCommands;
		private FbConnectionString options;
		private FbConnection owningConnection;
		private bool disposed;
		private object preparedCommandsCleanupSyncRoot;
		private FbEnlistmentNotification enlistmentNotification;

		#endregion

		#region Properties

		public IDatabase Database
		{
			get { return this.db; }
		}

		public bool HasActiveTransaction
		{
			get
			{
				return this.activeTransaction != null && !this.activeTransaction.IsUpdated;
			}
		}

		public FbTransaction ActiveTransaction
		{
			get { return this.activeTransaction; }
		}

		public FbConnection OwningConnection
		{
			get { return this.owningConnection; }
		}

		public bool IsEnlisted
		{
			get { return this.enlistmentNotification != null && !this.enlistmentNotification.IsCompleted; }
		}

		public FbConnectionString Options
		{
			get { return this.options; }
		}

		public bool CancelDisabled { get; set; }

		#endregion

		#region Constructors

		public FbConnectionInternal(FbConnectionString options)
		{
			this.preparedCommands = new List<WeakReference>();
			this.preparedCommandsCleanupSyncRoot = new object();

			this.options = options;

			GC.SuppressFinalize(this);
		}

		#endregion

		#region Finalizer

		~FbConnectionInternal()
		{
			// Do not re-create Dispose clean-up code here.
			// Calling Dispose(false) is optimal in terms of
			// readability and maintainability.
			this.Dispose(false);
		}

		#endregion

		#region IDisposable Methods

		public void Dispose()
		{
			this.Dispose(true);

			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			lock (this)
			{
				if (!this.disposed)
				{
					// release any unmanaged resources
					this.Disconnect();

					if (disposing)
					{
						// release managed resources here
					}

					this.disposed = true;
				}
			}
		}

		#endregion

		#region Create and Drop database methods

		public void CreateDatabase(DatabaseParameterBuffer dpb)
		{
			IDatabase db = ClientFactory.CreateDatabase(this.options);
			db.CreateDatabase(dpb, this.options.DataSource, this.options.Port, this.options.Database);
		}

		public void DropDatabase()
		{
			IDatabase db = ClientFactory.CreateDatabase(this.options);
			db.Attach(this.BuildDpb(db, this.options), this.options.DataSource, this.options.Port, this.options.Database);
			db.DropDatabase();
		}

		#endregion

		#region Connect and Disconnect methods

		public void Connect()
		{
			if (Charset.GetCharset(this.options.Charset) == null)
			{
				throw new FbException("Invalid character set specified");
			}

			try
			{
				this.db = ClientFactory.CreateDatabase(this.options);
				this.db.Charset = Charset.GetCharset(this.options.Charset);
				this.db.Dialect = this.options.Dialect;
				this.db.PacketSize = this.options.PacketSize;

				DatabaseParameterBuffer dpb = this.BuildDpb(this.db, options);

				if (options.FallIntoTrustedAuth)
				{
					this.db.AttachWithTrustedAuth(dpb, this.options.DataSource, this.options.Port, this.options.Database);
				}
				else
				{
					this.db.Attach(dpb, this.options.DataSource, this.options.Port, this.options.Database);
				}
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		public void Disconnect()
		{
			if (this.db != null)
			{
				try
				{
					this.db.Dispose();
				}
				catch
				{
				}
				finally
				{
					this.db = null;
					this.owningConnection = null;
					this.options = null;
				}
			}
		}

		#endregion

		#region Transaction Handling Methods

		public FbTransaction BeginTransaction(IsolationLevel level, string transactionName)
		{
			lock (this)
			{
				if (this.HasActiveTransaction)
				{
					throw new InvalidOperationException("A transaction is currently active. Parallel transactions are not supported.");
				}

				try
				{
					this.activeTransaction = new FbTransaction(this.owningConnection, level);
					this.activeTransaction.BeginTransaction();

					if (transactionName != null)
					{
						this.activeTransaction.Save(transactionName);
					}
				}
				catch (IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}

			return this.activeTransaction;
		}

		public FbTransaction BeginTransaction(FbTransactionOptions options, string transactionName)
		{
			lock (this)
			{
				if (this.HasActiveTransaction)
				{
					throw new InvalidOperationException("A transaction is currently active. Parallel transactions are not supported.");
				}

				try
				{
					this.activeTransaction = new FbTransaction(
						this.owningConnection, IsolationLevel.Unspecified);

					this.activeTransaction.BeginTransaction(options);

					if (transactionName != null)
					{
						this.activeTransaction.Save(transactionName);
					}
				}
				catch (IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}

			return this.activeTransaction;
		}

		public void DisposeTransaction()
		{
			if (this.activeTransaction != null && !this.IsEnlisted)
			{
				this.activeTransaction.Dispose();
				this.activeTransaction = null;
			}
		}

		public void TransactionUpdated()
		{
			for (int i = 0; i < this.preparedCommands.Count; i++)
			{
				FbCommand command;
				if (!this.preparedCommands[i].TryGetTarget<FbCommand>(out command))
					continue;

				if (command.Transaction != null)
				{
					command.CloseReader();
					command.Transaction = null;
				}
			}
		}

		#endregion

		#region Transaction Enlistement

		public void EnlistTransaction(System.Transactions.Transaction transaction)
		{
			if (this.owningConnection != null && this.options.Enlist)
			{
				if (this.enlistmentNotification != null && this.enlistmentNotification.SystemTransaction == transaction)
					return;

				if (this.HasActiveTransaction)
				{
					throw new ArgumentException("Unable to enlist in transaction, a local transaction already exists");
				}
				if (this.enlistmentNotification != null)
				{
					throw new ArgumentException("Already enlisted in a transaction");
				}

				this.enlistmentNotification = new FbEnlistmentNotification(this, transaction);
				this.enlistmentNotification.Completed += new EventHandler(EnlistmentCompleted);
			}
		}

		private void EnlistmentCompleted(object sender, EventArgs e)
		{
			this.enlistmentNotification = null;
		}

		public FbTransaction BeginTransaction(System.Transactions.IsolationLevel isolationLevel)
		{
			switch (isolationLevel)
			{
				case System.Transactions.IsolationLevel.Chaos:
					return this.BeginTransaction(System.Data.IsolationLevel.Chaos, null);

				case System.Transactions.IsolationLevel.ReadUncommitted:
					return this.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted, null);

				case System.Transactions.IsolationLevel.RepeatableRead:
					return this.BeginTransaction(System.Data.IsolationLevel.RepeatableRead, null);

				case System.Transactions.IsolationLevel.Serializable:
					return this.BeginTransaction(System.Data.IsolationLevel.Serializable, null);

				case System.Transactions.IsolationLevel.Snapshot:
					return this.BeginTransaction(System.Data.IsolationLevel.Snapshot, null);

				case System.Transactions.IsolationLevel.Unspecified:
					return this.BeginTransaction(System.Data.IsolationLevel.Unspecified, null);

				case System.Transactions.IsolationLevel.ReadCommitted:
				default:
					return this.BeginTransaction(System.Data.IsolationLevel.ReadCommitted, null);
			}
		}

		#endregion

		#region Schema Methods

		public DataTable GetSchema(string collectionName, string[] restrictions)
		{
			return FbSchemaFactory.GetSchema(this.owningConnection, collectionName, restrictions);
		}

		#endregion

		#region Prepared Commands Methods

		public void AddPreparedCommand(FbCommand command)
		{
			int position = this.preparedCommands.Count;
			for (int i = 0; i < this.preparedCommands.Count; i++)
			{
				FbCommand current;
				if (!this.preparedCommands[i].TryGetTarget<FbCommand>(out current))
				{
					position = i;
					break;
				}
				else
				{
					if (current == command)
					{
						return;
					}
				}
			}
			this.preparedCommands.Insert(position, new WeakReference(command));
		}

		public void RemovePreparedCommand(FbCommand command)
		{
			lock (preparedCommandsCleanupSyncRoot)
			{
				for (int i = this.preparedCommands.Count - 1; i >= 0; i--)
				{
					var item = this.preparedCommands[i];
					FbCommand current;
					if (item.TryGetTarget(out current) && current == command)
					{
						this.preparedCommands.RemoveAt(i);
						return;
					}
				}
			}
		}

		public void ReleasePreparedCommands()
		{
			WeakReference[] toProcess = new WeakReference[this.preparedCommands.Count];
			this.preparedCommands.CopyTo(toProcess);
			for (int i = 0; i < toProcess.Length; i++)
			{
				FbCommand current;
				if (!toProcess[i].TryGetTarget(out current))
					continue;

				try
				{
					// Release statement handle
					current.Release();
				}
				catch (System.IO.IOException)
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

			lock (preparedCommandsCleanupSyncRoot)
			{
				this.preparedCommands.Clear();
			}
		}

		#endregion

		#region Firebird Events Methods

		public void CloseEventManager()
		{
			if (this.db != null && this.db.HasRemoteEventSupport)
			{
				lock (this.db)
				{
					this.db.CloseEventManager();
				}
			}
		}

		#endregion

		#region Connection Verification

		public bool Verify()
		{
			// Do not actually ask for any information
			byte[] items = new byte[]
			{
				IscCodes.isc_info_end
			};

			try
			{
				this.db.GetDatabaseInfo(items, 16);

				return true;
			}
			catch
			{
				return false;
			}
		}

		#endregion

		#region Private Methods

		private DatabaseParameterBuffer BuildDpb(IDatabase db, FbConnectionString options)
		{
			DatabaseParameterBuffer dpb = new DatabaseParameterBuffer();

			dpb.Append(IscCodes.isc_dpb_version1);
			dpb.Append(IscCodes.isc_dpb_dummy_packet_interval, new byte[] { 120, 10, 0, 0 });
			dpb.Append(IscCodes.isc_dpb_sql_dialect, new byte[] { Convert.ToByte(options.Dialect), 0, 0, 0 });
			dpb.Append(IscCodes.isc_dpb_lc_ctype, options.Charset);
			if (options.DbCachePages > 0)
			{
				dpb.Append(IscCodes.isc_dpb_num_buffers, options.DbCachePages);
			}
			if (!string.IsNullOrEmpty(options.Role))
			{
				dpb.Append(IscCodes.isc_dpb_sql_role_name, options.Role);
			}
			dpb.Append(IscCodes.isc_dpb_connect_timeout, options.ConnectionTimeout);

			if (!options.FallIntoTrustedAuth)
			{
				dpb.Append(IscCodes.isc_dpb_user_name, options.UserID);
				dpb.Append(IscCodes.isc_dpb_password, options.Password);
			}
			dpb.Append(IscCodes.isc_dpb_process_id, GetProcessId());
			dpb.Append(IscCodes.isc_dpb_process_name, GetProcessName());
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
			// showing ApplicationPhysicalPath may be wrong because of connection pooling; better idea?
			return GetHostingPath() ?? GetRealProcessName();
		}


		private string GetHostingPath()
		{
			System.Reflection.Assembly assembly;
			try
			{
				assembly = System.Reflection.Assembly.Load(string.Format("System.Web, Version={0}.{1}.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", Environment.Version.Major, Environment.Version.Minor));
			}
			catch (FileNotFoundException)
			{
				return null;
			}
			catch (FileLoadException)
			{
				return null;
			}
			catch (BadImageFormatException)
			{
				return null;
			}
			return (string)assembly
				.GetType("System.Web.Hosting.HostingEnvironment")
				.GetProperty("ApplicationPhysicalPath")
				.GetValue(null, null);
		}
		private string GetRealProcessName()
		{
			System.Reflection.Assembly assembly = System.Reflection.Assembly.GetEntryAssembly();
			if (assembly != null)
			{
				return assembly.Location;
			}
			else // if we're not loaded from managed code
			{
				return Process.GetCurrentProcess().MainModule.FileName;
			}
		}

		private int GetProcessId()
		{
			System.Reflection.Assembly assembly = System.Reflection.Assembly.GetEntryAssembly();
			if (assembly != null)
			{
				if (assembly.IsFullyTrusted)
					return Process.GetCurrentProcess().Id;
				else
					return -1;
			}
			else // if we're not loaded from managed code
			{
				return Process.GetCurrentProcess().Id;
			}
		}
		#endregion

		#region Cancelation
		public void EnableCancel()
		{
			this.db.CancelOperation(IscCodes.fb_cancel_enable);
			this.CancelDisabled = false;
		}

		public void DisableCancel()
		{
			this.db.CancelOperation(IscCodes.fb_cancel_disable);
			this.CancelDisabled = true;
		}

		public void CancelCommand()
		{
			this.db.CancelOperation(IscCodes.fb_cancel_raise);
		}
		#endregion

		#region Infrastructure
		public FbConnectionInternal SetOwningConnection(FbConnection owningConnection)
		{
			this.owningConnection = owningConnection;
			return this;
		}
		#endregion
	}
}
