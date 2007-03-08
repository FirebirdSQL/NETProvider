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
 *	Copyright (c) 2002, 2006 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using FirebirdSql.Data.Common;
using FirebirdSql.Data.Schema;


namespace FirebirdSql.Data.FirebirdClient
{
	internal class FbConnectionInternal : MarshalByRefObject, IDisposable
	{
		#region · Fields ·

		private IDatabase			        db;
		private FbTransaction		        activeTransaction;
		private List<FbCommand>		        preparedCommands;
		private FbConnectionString	        options;
		private FbConnection		        owningConnection;
		private long				        created;
		private long				        lifetime;
		private bool				        pooled;
        private bool                        isDisposed;

#if (NET)

        private FbEnlistmentNotification enlistmentNotification;

#endif

		#endregion

		#region · Properties ·

		public IDatabase Database
		{
			get { return this.db; }
		}

		public long Lifetime
		{
			get { return this.lifetime; }
			set { this.lifetime = value; }
		}

		public long Created
		{
			get { return this.created; }
			set { this.created = value; }
		}

		public bool Pooled
		{
			get { return this.pooled; }
			set { this.pooled = value; }
		}

		public bool HasActiveTransaction
		{
			get
			{
				return this.activeTransaction != null && !this.activeTransaction.IsUpdated;
			}
		}

		public List<FbCommand> PreparedCommands
		{
			get
			{
				if (this.preparedCommands == null)
				{
					this.preparedCommands = new List<FbCommand>();
				}

				return this.preparedCommands;
			}
		}

		public FbTransaction ActiveTransaction
		{
			get { return this.activeTransaction; }
		}

		public FbConnectionString ConnectionOptions
		{
			get { return this.options; }
		}

		public FbConnection OwningConnection
		{
			get { return this.owningConnection; }
			set { this.owningConnection = value; }
		}

        public bool IsEnlisted
        {
#if (NET)
            get { return this.enlistmentNotification != null && !this.enlistmentNotification.IsCompleted; }
#else
            get { return false; }
#endif
        }


		#endregion

		#region · Constructors ·

		public FbConnectionInternal(FbConnectionString options) 
			: this(options, null)
		{
		}

		public FbConnectionInternal(FbConnectionString options, FbConnection owningConnection)
		{
			this.options = options;
			this.owningConnection = owningConnection;

            GC.SuppressFinalize(this);
		}

		#endregion

        #region · Finalizer ·

        ~FbConnectionInternal()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            this.Dispose(false);
        }

        #endregion

        #region · IDisposable Methods ·

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
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    // Release managed resources here
                    this.Disconnect();
                }

                // Call the appropriate methods to clean up 
                // unmanaged resources here.
                // If disposing is false, 
                // only the following code is executed.
            }

            this.isDisposed = true;
        }

        #endregion

		#region · Create and Drop database methods ·

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

		#region · Connect and Disconnect methods ·

		public void Connect()
		{
            if (Charset.GetCharset(this.options.Charset) == null)
            {
                throw new FbException("Invalid character set specified");
            }

			try
			{
				this.db             = ClientFactory.CreateDatabase(this.options);
				this.db.Charset     = Charset.GetCharset(this.options.Charset);
				this.db.Dialect     = this.options.Dialect;
				this.db.PacketSize  = this.options.PacketSize;

				DatabaseParameterBuffer dpb = this.BuildDpb(this.db, options);

				this.db.Attach(dpb, this.options.DataSource, this.options.Port, this.options.Database);

#if (NET)

                if (this.options.Enlist)
                {
                    this.EnlistTransaction(System.Transactions.Transaction.Current);
                }

#endif
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		public void Disconnect()
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
                this.db                 = null;
                this.owningConnection   = null;
                this.options            = null;
                this.lifetime           = 0;
                this.pooled             = false;
            }
		}

		#endregion

		#region · Transaction Handling Methods ·

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
			for (int i = 0; i < this.PreparedCommands.Count; i++)
			{
				FbCommand command = this.PreparedCommands[i];

				if (command.Transaction != null)
				{
					command.CloseReader();
					command.Transaction = null;
				}
			}
		}

		#endregion

        #region · Transaction Enlistement ·

#if (NET )

        public void EnlistTransaction(System.Transactions.Transaction transaction)
        {
            if (this.HasActiveTransaction)
            {
                throw new ArgumentException("Unable to enlist in transaction, a local transaction already exists");
            }
            if (this.enlistmentNotification != null)
            {
                throw new ArgumentException("Already enlisted in a transaction");
            }

            this.enlistmentNotification             = new FbEnlistmentNotification(this, transaction);
            this.enlistmentNotification.Completed   += new EventHandler(EnlistmentCompleted);
        }

        private void EnlistmentCompleted(object sender, EventArgs e)
        {
            this.enlistmentNotification = null;
        }

#endif

        #endregion

		#region · Schema Methods ·

		public DataTable GetSchema(string collectionName, string[] restrictions)
		{
			return FbSchemaFactory.GetSchema(this.owningConnection, collectionName, restrictions);
		}

		#endregion

		#region · Prepared Commands Methods ·

		public void AddPreparedCommand(FbCommand command)
		{
			if (!this.PreparedCommands.Contains(command))
			{
				this.PreparedCommands.Add(command);
			}
		}

		public void RemovePreparedCommand(FbCommand command)
		{
			this.PreparedCommands.Remove(command);
		}

		public void DisposePreparedCommands()
		{
			if (this.preparedCommands != null)
			{
				if (this.PreparedCommands.Count > 0)
				{
					FbCommand[] commands = this.PreparedCommands.ToArray();

					for (int i = 0; i < commands.Length; i++)
					{
                        try
                        {
                            // Release statement handle
                            commands[i].Release();
                        }
                        catch (System.IO.IOException)
                        {
                            // If an IO error occurs weh trying to release the command 
                            // avoid it. ( It maybe the connection to the server was down 
                            // for unknown reasons. )
                        }
                        catch (IscException iex)
                        {
                            if (iex.ErrorCode != IscCodes.isc_net_read_err &&
                                iex.ErrorCode != IscCodes.isc_net_write_err &&
                                iex.ErrorCode != IscCodes.isc_network_error)
                            {
                                throw;
                            }
                        }
					}
				}

				this.PreparedCommands.Clear();
				this.preparedCommands = null;
			}
		}

		#endregion

		#region · Firebird Events Methods ·

		public void CloseEventManager()
		{
			if (this.db.HasRemoteEventSupport)
			{
				lock (this.db)
				{
					this.db.CloseEventManager();
				}
			}
		}

		#endregion

		#region · Connection Verification ·

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

		#region · Private Methods ·

		private DatabaseParameterBuffer BuildDpb(IDatabase db, FbConnectionString options)
		{
			DatabaseParameterBuffer dpb = db.CreateDatabaseParameterBuffer();

			dpb.Append(IscCodes.isc_dpb_version1);
			dpb.Append(IscCodes.isc_dpb_dummy_packet_interval, new byte[] { 120, 10, 0, 0 });
			dpb.Append(IscCodes.isc_dpb_sql_dialect, new byte[] { Convert.ToByte(options.Dialect), 0, 0, 0 });
			dpb.Append(IscCodes.isc_dpb_lc_ctype, options.Charset);
			if (options.Role != null && options.Role.Length > 0)
			{
				dpb.Append(IscCodes.isc_dpb_sql_role_name, options.Role);
			}
			dpb.Append(IscCodes.isc_dpb_connect_timeout, options.ConnectionTimeout);
			dpb.Append(IscCodes.isc_dpb_user_name, options.UserID);
			dpb.Append(IscCodes.isc_dpb_password, options.Password);

			return dpb;
		}

		#endregion
	}
}
