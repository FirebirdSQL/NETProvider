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

			FbConnectionPoolManager.Instance.ClearPool(connection._options);
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

		private FbConnectionInternal _innerConnection;
		private ConnectionState _state;
		private FbConnectionString _options;
		private bool _disposed;
		private string _connectionString;

		#endregion

		#region Properties

		[Category("Data")]
		[SettingsBindable(true)]
		[RefreshProperties(RefreshProperties.All)]
		[DefaultValue("")]
		public override string ConnectionString
		{
			get { return _connectionString; }
			set
			{
				lock (this)
				{
					if (_state == ConnectionState.Closed)
					{
						if (value == null)
						{
							value = string.Empty;
						}

						_options.Load(value);
						_options.Validate();
						_connectionString = value;
					}
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override int ConnectionTimeout
		{
			get { return _options.ConnectionTimeout; }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string Database
		{
			get { return _options.Database; }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string DataSource
		{
			get { return _options.DataSource; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string ServerVersion
		{
			get
			{
				if (_state == ConnectionState.Closed)
				{
					throw new InvalidOperationException("The connection is closed.");
				}

				if (_innerConnection != null)
				{
					return _innerConnection.Database.ServerVersion;
				}

				return string.Empty;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override ConnectionState State
		{
			get { return _state; }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int PacketSize
		{
			get { return _options.PacketSize; }
		}

		#endregion

		#region Internal Properties

		internal FbConnectionInternal InnerConnection
		{
			get { return _innerConnection; }
		}

		internal FbConnectionString ConnectionOptions
		{
			get { return _options; }
		}

		internal bool IsClosed
		{
			get { return _state == ConnectionState.Closed; }
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
			_options = new FbConnectionString();
			_state = ConnectionState.Closed;
			_connectionString = string.Empty;

			if (!string.IsNullOrEmpty(connectionString))
			{
				ConnectionString = connectionString;
			}
		}

		#endregion

		#region IDisposable methods

		protected override void Dispose(bool disposing)
		{
			lock (this)
			{
				if (!_disposed)
				{
					try
					{
						// release any unmanaged resources
						Close();

						if (disposing)
						{
							// release any managed resources
							_innerConnection = null;
							_options = null;
							_connectionString = null;
						}

						_disposed = true;
					}
					catch
					{ }
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
			return new FbConnection(ConnectionString);
		}

		#endregion

		#region Transaction Handling Methods

		public new FbTransaction BeginTransaction()
		{
			return BeginTransaction(IsolationLevel.ReadCommitted, null);
		}

		public new FbTransaction BeginTransaction(IsolationLevel level)
		{
			return BeginTransaction(level, null);
		}

		public FbTransaction BeginTransaction(string transactionName)
		{
			return BeginTransaction(IsolationLevel.ReadCommitted, transactionName);
		}

		public FbTransaction BeginTransaction(IsolationLevel level, string transactionName)
		{
			CheckClosed();

			return _innerConnection.BeginTransaction(level, transactionName);
		}

		public FbTransaction BeginTransaction(FbTransactionOptions options)
		{
			return BeginTransaction(options, null);
		}

		public FbTransaction BeginTransaction(FbTransactionOptions options, string transactionName)
		{
			CheckClosed();

			return _innerConnection.BeginTransaction(options, transactionName);
		}

		#endregion

		#region Transaction Enlistement

		public override void EnlistTransaction(System.Transactions.Transaction transaction)
		{
			CheckClosed();

			_innerConnection.EnlistTransaction(transaction);
		}

		#endregion

		#region DbConnection methods

		protected override DbCommand CreateDbCommand()
		{
			return new FbCommand(null, this);
		}

		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
		{
			return BeginTransaction(isolationLevel);
		}

		#endregion

		#region Database Schema Methods

		public override DataTable GetSchema()
		{
			return GetSchema("MetaDataCollections");
		}

		public override DataTable GetSchema(string collectionName)
		{
			return GetSchema(collectionName, null);
		}

		public override DataTable GetSchema(string collectionName, string[] restrictions)
		{
			CheckClosed();

			return _innerConnection.GetSchema(collectionName, restrictions);
		}

		#endregion

		#region Methods

		public new FbCommand CreateCommand()
		{
			return (FbCommand)CreateDbCommand();
		}

		public override void ChangeDatabase(string db)
		{
			lock (this)
			{
				CheckClosed();

				if (string.IsNullOrEmpty(db))
				{
					throw new InvalidOperationException("Database name is not valid.");
				}

				string cs = _connectionString;

				try
				{
					FbConnectionStringBuilder csb = new FbConnectionStringBuilder(_connectionString);

					/* Close current connection	*/
					Close();

					/* Set up the new Database	*/
					csb.Database = db;
					ConnectionString = csb.ToString();

					/* Open	new	connection	*/
					Open();
				}
				catch (IscException ex)
				{
					ConnectionString = cs;
					throw new FbException(ex.Message, ex);
				}
			}
		}

		public override void Open()
		{
			lock (this)
			{
				if (string.IsNullOrEmpty(_connectionString))
				{
					throw new InvalidOperationException("Connection String is not initialized.");
				}
				if (!IsClosed && _state != ConnectionState.Connecting)
				{
					throw new InvalidOperationException("Connection already Open.");
				}
				if (_options.Enlist && System.Transactions.Transaction.Current == null)
				{
					throw new InvalidOperationException("There is no active TransactionScope to enlist transactions.");
				}

				DemandPermission();

				try
				{
					OnStateChange(_state, ConnectionState.Connecting);

					if (_options.Pooling)
					{
						_innerConnection = FbConnectionPoolManager.Instance.Get(_options, this);
					}
					else
					{
						// Do not use Connection Pooling
						_innerConnection = new FbConnectionInternal(_options);
						_innerConnection.SetOwningConnection(this);
						_innerConnection.Connect();
					}

					try
					{
						_innerConnection.EnlistTransaction(System.Transactions.Transaction.Current);
					}
					catch
					{
						// if enlistment fails clean up innerConnection
						_innerConnection.DisposeTransaction();

						if (_options.Pooling)
						{
							// Send connection return back to the Pool
							FbConnectionPoolManager.Instance.Release(_innerConnection);
						}
						else
						{
							_innerConnection.Dispose();
							_innerConnection = null;
						}

						throw;
					}

					// Bind	Warning	messages event
					_innerConnection.Database.WarningMessage = new WarningMessageCallback(OnWarningMessage);

					// Update the connection state
					OnStateChange(_state, ConnectionState.Open);
				}
				catch (IscException ex)
				{
					OnStateChange(_state, ConnectionState.Closed);
					throw new FbException(ex.Message, ex);
				}
				catch
				{
					OnStateChange(_state, ConnectionState.Closed);
					throw;
				}
			}
		}

		public override void Close()
		{
			if (!IsClosed && _innerConnection != null)
			{
				lock (this)
				{
					try
					{
						lock (_innerConnection)
						{
							// Close the Remote	Event Manager
							_innerConnection.CloseEventManager();

							// Unbind Warning messages event
							if (_innerConnection.Database != null)
							{
								_innerConnection.Database.WarningMessage = null;
							}

							// Dispose Transaction
							_innerConnection.DisposeTransaction();

							// Dispose all active statemenets
							_innerConnection.ReleasePreparedCommands();

							// Close connection	or send	it back	to the pool
							if (_options.Pooling)
							{
								if (_innerConnection.CancelDisabled)
								{
									// Enable fb_cancel_operation if going into pool
									_innerConnection.EnableCancel();
								}

								// Send	connection to the Pool
								FbConnectionPoolManager.Instance.Release(_innerConnection);
							}
							else
							{
								if (!_innerConnection.IsEnlisted)
								{
									_innerConnection.Dispose();
								}
								_innerConnection = null;
							}
						}
					}
					catch
					{ }
					finally
					{
						// Update connection state
						OnStateChange(_state, ConnectionState.Closed);
					}
				}
			}
		}

		#endregion

		#region Private Methods

		internal void DemandPermission()
		{
			FirebirdClientPermission permission = new FirebirdClientPermission(_connectionString);
			permission.Demand();
		}

		private void CheckClosed()
		{
			if (IsClosed)
			{
				throw new InvalidOperationException("Operation requires an open and available connection.");
			}
		}

		#endregion

		#region Event Handlers

		private void OnWarningMessage(IscException warning)
		{
			if (InfoMessage != null)
			{
				InfoMessage(this, new FbInfoMessageEventArgs(warning));
			}
		}

		private void OnStateChange(ConnectionState originalState, ConnectionState currentState)
		{
			_state = currentState;
			if (StateChange != null)
			{
				StateChange(this, new StateChangeEventArgs(originalState, currentState));
			}
		}

		#endregion

		#region Cancelation
		public void EnableCancel()
		{
			CheckClosed();

			_innerConnection.EnableCancel();
		}

		public void DisableCancel()
		{
			CheckClosed();

			_innerConnection.DisableCancel();
		}

		internal void CancelCommand()
		{
			CheckClosed();

			_innerConnection.CancelCommand();
		}
		#endregion
	}
}
