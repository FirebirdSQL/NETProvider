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

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.FirebirdClient
{
	[DefaultEvent("InfoMessage")]
	public sealed class FbConnection : DbConnection
#if !NETSTANDARD1_6
		, ICloneable
#endif
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

		public static void CreateDatabase(string connectionString, bool overwrite)
		{
			CreateDatabaseImpl(connectionString, overwrite: overwrite);
		}

		public static void CreateDatabase(string connectionString, int pageSize = 4096, bool forcedWrites = true, bool overwrite = false)
		{
			CreateDatabaseImpl(connectionString, pageSize, forcedWrites, overwrite);
		}

		private static void CreateDatabaseImpl(string connectionString, int pageSize = 4096, bool forcedWrites = true, bool overwrite = false)
		{
			FbConnectionString options = new FbConnectionString(connectionString);
			options.Validate();

			try
			{
				DatabaseParameterBuffer dpb = new DatabaseParameterBuffer();

				dpb.Append(IscCodes.isc_dpb_version1);
				dpb.Append(IscCodes.isc_dpb_dummy_packet_interval, new byte[] { 120, 10, 0, 0 });
				dpb.Append(IscCodes.isc_dpb_sql_dialect, new byte[] { options.Dialect, 0, 0, 0 });
				if (!string.IsNullOrEmpty(options.UserID))
				{
					dpb.Append(IscCodes.isc_dpb_user_name, options.UserID);
				}
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
				dpb.Append(IscCodes.isc_dpb_force_write, (short)(forcedWrites ? 1 : 0));
				dpb.Append(IscCodes.isc_dpb_overwrite, (overwrite ? 1 : 0));
				if (pageSize > 0)
				{
					dpb.Append(IscCodes.isc_dpb_page_size, pageSize);
				}

				using (FbConnectionInternal db = new FbConnectionInternal(options))
				{
					db.CreateDatabase(dpb);
				}
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		public static void DropDatabase(string connectionString)
		{
			FbConnectionString options = new FbConnectionString(connectionString);
			options.Validate();

			try
			{
				using (FbConnectionInternal db = new FbConnectionInternal(options))
				{
					db.DropDatabase();
				}
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		#endregion

		#region Events

		public override event StateChangeEventHandler StateChange;

		public event EventHandler<FbInfoMessageEventArgs> InfoMessage;

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
#if !NETSTANDARD1_6
		[SettingsBindable(true)]
#endif
		[RefreshProperties(RefreshProperties.All)]
		[DefaultValue("")]
		public override string ConnectionString
		{
			get { return _connectionString; }
			set
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

#if !NETSTANDARD1_6
		protected override DbProviderFactory DbProviderFactory
		{
			get { return FirebirdClientFactory.Instance; }
		}
#endif

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
			if (disposing)
			{
				if (!_disposed)
				{
					_disposed = true;
					Close();
					_innerConnection = null;
					_options = null;
					_connectionString = null;
					base.Dispose(disposing);
				}
			}
		}

		#endregion

		#region ICloneable Methods
#if NETSTANDARD1_6
		internal object Clone()
#else
		object ICloneable.Clone()
#endif
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

#if !NETSTANDARD1_6
		public override void EnlistTransaction(System.Transactions.Transaction transaction)
		{
			CheckClosed();

			_innerConnection.EnlistTransaction(transaction);
		}
#endif

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

#if !NETSTANDARD1_6
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
#endif

		#endregion

		#region Methods

		public new FbCommand CreateCommand()
		{
			return (FbCommand)CreateDbCommand();
		}

		public override void ChangeDatabase(string db)
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

		public override void Open()
		{
			if (string.IsNullOrEmpty(_connectionString))
			{
				throw new InvalidOperationException("Connection String is not initialized.");
			}
			if (!IsClosed && _state != ConnectionState.Connecting)
			{
				throw new InvalidOperationException("Connection already Open.");
			}
#if !NETSTANDARD1_6
			if (_options.Enlist && System.Transactions.Transaction.Current == null)
			{
				throw new InvalidOperationException("There is no active TransactionScope to enlist transactions.");
			}
#endif

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

#if !NETSTANDARD1_6
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
#endif

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

		public override void Close()
		{
			if (!IsClosed && _innerConnection != null)
			{
				try
				{
					_innerConnection.CloseEventManager();

					if (_innerConnection.Database != null)
					{
						_innerConnection.Database.WarningMessage = null;
					}

					_innerConnection.DisposeTransaction();

					_innerConnection.ReleasePreparedCommands();

					if (_options.Pooling)
					{
						if (_innerConnection.CancelDisabled)
						{
							_innerConnection.EnableCancel();
						}

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
				catch
				{ }
				finally
				{
					OnStateChange(_state, ConnectionState.Closed);
				}
			}
		}

		#endregion

		#region Private Methods

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
			InfoMessage?.Invoke(this, new FbInfoMessageEventArgs(warning));
		}

		private void OnStateChange(ConnectionState originalState, ConnectionState currentState)
		{
			_state = currentState;
			StateChange?.Invoke(this, new StateChangeEventArgs(originalState, currentState));
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

		#region Internal Methods

		internal static void EnsureOpen(FbConnection connection)
		{
			if (connection == null || connection.State != ConnectionState.Open)
				throw new InvalidOperationException("Connection must be valid and open.");
		}

		#endregion
	}
}
