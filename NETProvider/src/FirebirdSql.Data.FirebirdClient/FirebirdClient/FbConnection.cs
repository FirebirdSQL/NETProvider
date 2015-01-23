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
 * Contributors:
 *   Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using FirebirdSql.Data.Common;
using FirebirdSql.Data.Services;

namespace FirebirdSql.Data.FirebirdClient
{
	[DefaultEvent("InfoMessage")]
	public sealed class FbConnection : DbConnection, ICloneable
	{
		#region Static Pool Handling Methods

		public static void ClearAllPools()
		{
			FbConnectionPoolManager.Instance.ClearAllPools();
		}

		public static void ClearPool(FbConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException("connection");

			FbConnectionPoolManager.Instance.ClearPool(connection.options);
		}

		#endregion

		#region Static Database Creation/Drop methods

		public static void CreateDatabase(string connectionString)
		{
			FbConnection.CreateDatabase(connectionString, 4096, true, false);
		}

		public static void CreateDatabase(string connectionString, bool overwrite)
		{
			FbConnection.CreateDatabase(connectionString, 4096, true, overwrite);
		}

		public static void CreateDatabase(string connectionString, int pageSize, bool forcedWrites, bool overwrite)
		{
			FbConnectionString options = new FbConnectionString(connectionString);
			options.Validate();

			try
			{
				// DPB configuration
				DatabaseParameterBuffer dpb = new DatabaseParameterBuffer();

				// Dpb version
				dpb.Append(IscCodes.isc_dpb_version1);

				// Dummy packet	interval
				dpb.Append(IscCodes.isc_dpb_dummy_packet_interval, new byte[] { 120, 10, 0, 0 });

				// User	name
				dpb.Append(IscCodes.isc_dpb_user_name, options.UserID);

				// User	password
				dpb.Append(IscCodes.isc_dpb_password, options.Password);

				// Database	dialect
				dpb.Append(IscCodes.isc_dpb_sql_dialect, new byte[] { options.Dialect, 0, 0, 0 });

				// Character set
				if (options.Charset.Length > 0)
				{
					Charset charset = Charset.GetCharset(options.Charset);

					if (charset == null)
					{
						throw new ArgumentException("Character set is not valid.");
					}
					else
					{
						dpb.Append(IscCodes.isc_dpb_set_db_charset, charset.Name);
					}
				}

				// Forced writes
				dpb.Append(IscCodes.isc_dpb_force_write, (short)(forcedWrites ? 1 : 0));

				// Database overwrite
				dpb.Append(IscCodes.isc_dpb_overwrite, (overwrite ? 1 : 0));

				// Page	Size
				if (pageSize > 0)
				{
					dpb.Append(IscCodes.isc_dpb_page_size, pageSize);
				}

				// Create the new database
				FbConnectionInternal db = new FbConnectionInternal(options);
				db.CreateDatabase(dpb);
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		public static void DropDatabase(string connectionString)
		{
			// Configure Attachment
			FbConnectionString options = new FbConnectionString(connectionString);
			options.Validate();

			try
			{
				// Drop	the	database	
				FbConnectionInternal db = new FbConnectionInternal(options);
				db.DropDatabase();
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		#endregion

		#region Events

		public override event StateChangeEventHandler StateChange;

		public event FbInfoMessageEventHandler InfoMessage;

		#endregion

		#region Fields

		private FbConnectionInternal innerConnection;
		private ConnectionState state;
		private FbConnectionString options;
		private bool disposed;
		private string connectionString;

		#endregion

		#region Properties

		[Category("Data")]
		[SettingsBindable(true)]
		[RefreshProperties(RefreshProperties.All)]
		[DefaultValue("")]
		public override string ConnectionString
		{
			get { return this.connectionString; }
			set
			{
				lock (this)
				{
					if (this.state == ConnectionState.Closed)
					{
						if (value == null)
						{
							value = string.Empty;
						}

						this.options.Load(value);
						this.options.Validate();
						this.connectionString = value;
					}
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override int ConnectionTimeout
		{
			get { return this.options.ConnectionTimeout; }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string Database
		{
			get { return this.options.Database; }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string DataSource
		{
			get { return this.options.DataSource; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string ServerVersion
		{
			get
			{
				if (this.state == ConnectionState.Closed)
				{
					throw new InvalidOperationException("The connection is closed.");
				}

				if (this.innerConnection != null)
				{
					return this.innerConnection.Database.ServerVersion;
				}

				return string.Empty;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override ConnectionState State
		{
			get { return this.state; }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int PacketSize
		{
			get { return this.options.PacketSize; }
		}

		#endregion

		#region Internal Properties

		internal FbConnectionInternal InnerConnection
		{
			get { return this.innerConnection; }
		}

		internal FbConnectionString ConnectionOptions
		{
			get { return this.options; }
		}

		internal bool IsClosed
		{
			get { return this.state == ConnectionState.Closed; }
		}

		#endregion

		#region Protected Properties

		protected override DbProviderFactory DbProviderFactory
		{
			get { return FirebirdClientFactory.Instance; }
		}

		#endregion

		#region Constructors

		public FbConnection()
			: this(null)
		{
		}

		public FbConnection(string connectionString)
			: base()
		{
			this.options = new FbConnectionString();
			this.state = ConnectionState.Closed;
			this.connectionString = string.Empty;

			if (!string.IsNullOrEmpty(connectionString))
			{
				this.ConnectionString = connectionString;
			}
		}

		#endregion

		#region IDisposable methods

		protected override void Dispose(bool disposing)
		{
			lock (this)
			{
				if (!this.disposed)
				{
					try
					{
						// release any unmanaged resources
						this.Close();

						if (disposing)
						{
							// release any managed resources
							this.innerConnection = null;
							this.options = null;
							this.connectionString = null;
						}

						this.disposed = true;
					}
					catch
					{
					}
					finally
					{
						base.Dispose(disposing);
					}
				}
			}
		}

		#endregion

		#region ICloneable Methods

		object ICloneable.Clone()
		{
			return new FbConnection(this.ConnectionString);
		}

		#endregion

		#region Transaction Handling Methods

		public new FbTransaction BeginTransaction()
		{
			return this.BeginTransaction(IsolationLevel.ReadCommitted, null);
		}

		public new FbTransaction BeginTransaction(IsolationLevel level)
		{
			return this.BeginTransaction(level, null);
		}

		public FbTransaction BeginTransaction(string transactionName)
		{
			return this.BeginTransaction(IsolationLevel.ReadCommitted, transactionName);
		}

		public FbTransaction BeginTransaction(IsolationLevel level, string transactionName)
		{
			this.CheckClosed();

			return this.innerConnection.BeginTransaction(level, transactionName);
		}

		public FbTransaction BeginTransaction(FbTransactionOptions options)
		{
			return this.BeginTransaction(options, null);
		}

		public FbTransaction BeginTransaction(FbTransactionOptions options, string transactionName)
		{
			this.CheckClosed();

			return this.innerConnection.BeginTransaction(options, transactionName);
		}

		#endregion

		#region Transaction Enlistement

		public override void EnlistTransaction(System.Transactions.Transaction transaction)
		{
			this.CheckClosed();

			this.innerConnection.EnlistTransaction(transaction);
		}

		#endregion

		#region DbConnection methods

		protected override DbCommand CreateDbCommand()
		{
			return new FbCommand(null, this);
		}

		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
		{
			return this.BeginTransaction(isolationLevel);
		}

		#endregion

		#region Database Schema Methods

		public override DataTable GetSchema()
		{
			return this.GetSchema("MetaDataCollections");
		}

		public override DataTable GetSchema(string collectionName)
		{
			return this.GetSchema(collectionName, null);
		}

		public override DataTable GetSchema(string collectionName, string[] restrictions)
		{
			this.CheckClosed();

			return this.innerConnection.GetSchema(collectionName, restrictions);
		}

		#endregion

		#region Methods

		public new FbCommand CreateCommand()
		{
			return (FbCommand)this.CreateDbCommand();
		}

		public override void ChangeDatabase(string db)
		{
			lock (this)
			{
				this.CheckClosed();

				if (string.IsNullOrEmpty(db))
				{
					throw new InvalidOperationException("Database name is not valid.");
				}

				string cs = this.connectionString;

				try
				{
					FbConnectionStringBuilder csb = new FbConnectionStringBuilder(this.connectionString);

					/* Close current connection	*/
					this.Close();

					/* Set up the new Database	*/
					csb.Database = db;
					this.ConnectionString = csb.ToString();

					/* Open	new	connection	*/
					this.Open();
				}
				catch (IscException ex)
				{
					this.ConnectionString = cs;
					throw new FbException(ex.Message, ex);
				}
			}
		}

		public override void Open()
		{
			lock (this)
			{
				if (string.IsNullOrEmpty(this.connectionString))
				{
					throw new InvalidOperationException("Connection String is not initialized.");
				}
				if (!this.IsClosed && this.state != ConnectionState.Connecting)
				{
					throw new InvalidOperationException("Connection already Open.");
				}
				if (this.options.Enlist && System.Transactions.Transaction.Current == null)
				{
					throw new InvalidOperationException("There is no active TransactionScope to enlist transactions.");
				}

				this.DemandPermission();

				try
				{
					this.OnStateChange(this.state, ConnectionState.Connecting);

					if (this.options.Pooling)
					{
						this.innerConnection = FbConnectionPoolManager.Instance.Get(this.options, this);
					}
					else
					{
						// Do not use Connection Pooling
						this.innerConnection = new FbConnectionInternal(this.options);
						this.innerConnection.SetOwningConnection(this);
						this.innerConnection.Connect();
					}

					try
					{
						this.innerConnection.EnlistTransaction(System.Transactions.Transaction.Current);
					}
					catch
					{
						// if enlistment fails clean up innerConnection
						this.innerConnection.DisposeTransaction();

						if (this.options.Pooling)
						{
							// Send connection return back to the Pool
							FbConnectionPoolManager.Instance.Release(this.innerConnection);
						}
						else
						{
							this.innerConnection.Dispose();
							this.innerConnection = null;
						}

						throw;
					}

					// Bind	Warning	messages event
					this.innerConnection.Database.WarningMessage = new WarningMessageCallback(this.OnWarningMessage);

					// Update the connection state
					this.OnStateChange(this.state, ConnectionState.Open);
				}
				catch (IscException ex)
				{
					this.OnStateChange(this.state, ConnectionState.Closed);
					throw new FbException(ex.Message, ex);
				}
				catch
				{
					this.OnStateChange(this.state, ConnectionState.Closed);
					throw;
				}
			}
		}

		public override void Close()
		{
			if (!this.IsClosed && this.innerConnection != null)
			{
				lock (this)
				{
					try
					{
						lock (this.innerConnection)
						{
							// Close the Remote	Event Manager
							this.innerConnection.CloseEventManager();

							// Unbind Warning messages event
							if (this.innerConnection.Database != null)
							{
								this.innerConnection.Database.WarningMessage = null;
							}

							// Dispose Transaction
							this.innerConnection.DisposeTransaction();

							// Dispose all active statemenets
							this.innerConnection.ReleasePreparedCommands();

							// Close connection	or send	it back	to the pool
							if (this.options.Pooling)
							{
								if (this.innerConnection.CancelDisabled)
								{
									// Enable fb_cancel_operation if going into pool
									this.innerConnection.EnableCancel();
								}

								// Send	connection to the Pool
								FbConnectionPoolManager.Instance.Release(this.innerConnection);
							}
							else
							{
								if (!this.innerConnection.IsEnlisted)
								{
									this.innerConnection.Dispose();
								}
								this.innerConnection = null;
							}
						}
					}
					catch
					{
					}
					finally
					{
						// Update connection state
						this.OnStateChange(this.state, ConnectionState.Closed);
					}
				}
			}
		}

		#endregion

		#region Private Methods

		internal void DemandPermission()
		{
			FirebirdClientPermission permission = new FirebirdClientPermission(this.connectionString);
			permission.Demand();
		}

		private void CheckClosed()
		{
			if (this.IsClosed)
			{
				throw new InvalidOperationException("Operation requires an open and available connection.");
			}
		}

		#endregion

		#region Event Handlers

		private void OnWarningMessage(IscException warning)
		{
			if (this.InfoMessage != null)
			{
				this.InfoMessage(this, new FbInfoMessageEventArgs(warning));
			}
		}

		private void OnStateChange(ConnectionState originalState, ConnectionState currentState)
		{
			this.state = currentState;
			if (this.StateChange != null)
			{
				this.StateChange(this, new StateChangeEventArgs(originalState, currentState));
			}
		}

		#endregion

		#region Cancelation
		public void EnableCancel()
		{
			this.CheckClosed();

			this.innerConnection.EnableCancel();
		}

		public void DisableCancel()
		{
			this.CheckClosed();

			this.innerConnection.DisableCancel(); 
		}

		internal void CancelCommand()
		{
			this.CheckClosed();

			this.innerConnection.CancelCommand();
		}
		#endregion
	}
}
