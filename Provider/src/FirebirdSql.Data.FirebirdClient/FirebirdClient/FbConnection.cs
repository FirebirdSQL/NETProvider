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
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

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
				throw new ArgumentNullException(nameof(connection));

			FbConnectionPoolManager.Instance.ClearPool(connection.ConnectionOptions);
		}

		public static void ClearPool(string connectionString)
		{
			if (connectionString == null)
				throw new ArgumentNullException(nameof(connectionString));

			FbConnectionPoolManager.Instance.ClearPool(new ConnectionString(connectionString));
		}

		#endregion

		#region Static Database Creation/Drop methods

		public static void CreateDatabase(string connectionString, int pageSize = 4096, bool forcedWrites = true, bool overwrite = false) => CreateDatabaseImpl(connectionString, pageSize, forcedWrites, overwrite, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public static Task CreateDatabaseAsync(string connectionString, int pageSize = 4096, bool forcedWrites = true, bool overwrite = false, CancellationToken cancellationToken = default) => CreateDatabaseImpl(connectionString, pageSize, forcedWrites, overwrite, new AsyncWrappingCommonArgs(true, cancellationToken));
		private static async Task CreateDatabaseImpl(string connectionString, int pageSize, bool forcedWrites, bool overwrite, AsyncWrappingCommonArgs async)
		{
			var options = new ConnectionString(connectionString);
			options.Validate();

			try
			{
				var db = new FbConnectionInternal(options);
				try
				{
					await db.CreateDatabase(pageSize, forcedWrites, overwrite, async).ConfigureAwait(false);
				}
				finally
				{
					await db.Disconnect(async).ConfigureAwait(false);
				}
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		public static void DropDatabase(string connectionString) => DropDatabaseImpl(connectionString, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public static Task DropDatabaseAsync(string connectionString, CancellationToken cancellationToken = default) => DropDatabaseImpl(connectionString, new AsyncWrappingCommonArgs(true, cancellationToken));
		static async Task DropDatabaseImpl(string connectionString, AsyncWrappingCommonArgs async)
		{
			var options = new ConnectionString(connectionString);
			options.Validate();

			try
			{
				var db = new FbConnectionInternal(options);
				try
				{
					await db.DropDatabase(async).ConfigureAwait(false);
				}
				finally
				{
					await db.Disconnect(async).ConfigureAwait(false);
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
		private ConnectionString _options;
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
				if (_state == ConnectionState.Closed)
				{
					if (value == null)
					{
						value = string.Empty;
					}

					_options = new ConnectionString(value);
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

		internal ConnectionString ConnectionOptions
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
		{
			_options = new ConnectionString();
			_state = ConnectionState.Closed;
			_connectionString = string.Empty;
		}

		public FbConnection(string connectionString)
			: this()
		{
			if (!string.IsNullOrEmpty(connectionString))
			{
				ConnectionString = connectionString;
			}
		}

		#endregion

		#region IDisposable, IAsyncDisposable methods

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				DisposeHelper(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
			}
			base.Dispose(disposing);
		}
#if !(NET48 || NETSTANDARD2_0)
		public override async ValueTask DisposeAsync()
		{
			await DisposeHelper(new AsyncWrappingCommonArgs(true)).ConfigureAwait(false);
			await base.DisposeAsync().ConfigureAwait(false);
		}
#endif
		private async Task DisposeHelper(AsyncWrappingCommonArgs async)
		{
			if (!_disposed)
			{
				_disposed = true;
				await CloseImpl(async).ConfigureAwait(false);
				_innerConnection = null;
				_options = null;
				_connectionString = null;
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

		public new FbTransaction BeginTransaction() => BeginTransaction(IsolationLevel.ReadCommitted, null);
#if NET48 || NETSTANDARD2_0
		public Task<FbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
#else
		public new Task<FbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
#endif
			=> BeginTransactionAsync(IsolationLevel.ReadCommitted, null, cancellationToken);
		public new FbTransaction BeginTransaction(IsolationLevel level) => BeginTransaction(level, null);
#if NET48 || NETSTANDARD2_0
		public Task<FbTransaction> BeginTransactionAsync(IsolationLevel level, CancellationToken cancellationToken = default)
#else
		public new Task<FbTransaction> BeginTransactionAsync(IsolationLevel level, CancellationToken cancellationToken = default)
#endif
			=> BeginTransactionAsync(level, null, cancellationToken);
		public FbTransaction BeginTransaction(string transactionName) => BeginTransaction(IsolationLevel.ReadCommitted, transactionName);
		public Task<FbTransaction> BeginTransactionAsync(string transactionName, CancellationToken cancellationToken = default) => BeginTransactionAsync(IsolationLevel.ReadCommitted, transactionName, cancellationToken);
		public FbTransaction BeginTransaction(IsolationLevel level, string transactionName) => BeginTransactionImpl(level, transactionName, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<FbTransaction> BeginTransactionAsync(IsolationLevel level, string transactionName, CancellationToken cancellationToken = default) => BeginTransactionImpl(level, transactionName, new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<FbTransaction> BeginTransactionImpl(IsolationLevel level, string transactionName, AsyncWrappingCommonArgs async)
		{
			CheckClosed();

			return _innerConnection.BeginTransaction(level, transactionName, async);
		}

		public FbTransaction BeginTransaction(FbTransactionOptions options) => BeginTransaction(options, null);
		public Task<FbTransaction> BeginTransactionAsync(FbTransactionOptions options, CancellationToken cancellationToken = default) => BeginTransactionAsync(options, null, cancellationToken);
		public FbTransaction BeginTransaction(FbTransactionOptions options, string transactionName) => BeginTransactionImpl(options, transactionName, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<FbTransaction> BeginTransactionAsync(FbTransactionOptions options, string transactionName, CancellationToken cancellationToken = default) => BeginTransactionImpl(options, transactionName, new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<FbTransaction> BeginTransactionImpl(FbTransactionOptions options, string transactionName, AsyncWrappingCommonArgs async)
		{
			CheckClosed();

			return _innerConnection.BeginTransaction(options, transactionName, async);
		}

		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => BeginTransaction(isolationLevel);
#if !(NET48 || NETSTANDARD2_0)
		protected override async ValueTask<DbTransaction> BeginDbTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken) => await BeginTransactionAsync(isolationLevel, cancellationToken).ConfigureAwait(false);
#endif

		#endregion

		#region Database Schema Methods

		public override DataTable GetSchema() => GetSchemaImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
#if NET48 || NETSTANDARD2_0 || NETSTANDARD2_1
		public Task<DataTable> GetSchemaAsync(CancellationToken cancellationToken = default)
#else
		public override Task<DataTable> GetSchemaAsync(CancellationToken cancellationToken = default)
#endif
			=> GetSchemaImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<DataTable> GetSchemaImpl(AsyncWrappingCommonArgs async)
		{
			return GetSchemaImpl("MetaDataCollections", async);
		}

		public override DataTable GetSchema(string collectionName) => GetSchemaImpl(collectionName, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
#if NET48 || NETSTANDARD2_0 || NETSTANDARD2_1
		public Task<DataTable> GetSchemaAsync(string collectionName, CancellationToken cancellationToken = default)
#else
		public override Task<DataTable> GetSchemaAsync(string collectionName, CancellationToken cancellationToken = default)
#endif
			=> GetSchemaImpl(collectionName, new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<DataTable> GetSchemaImpl(string collectionName, AsyncWrappingCommonArgs async)
		{
			return GetSchemaImpl(collectionName, null, async);
		}
		public override DataTable GetSchema(string collectionName, string[] restrictions) => GetSchemaImpl(collectionName, restrictions, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
#if NET48 || NETSTANDARD2_0 || NETSTANDARD2_1
		public Task<DataTable> GetSchemaAsync(string collectionName, string[] restrictions, CancellationToken cancellationToken = default)
#else
		public override Task<DataTable> GetSchemaAsync(string collectionName, string[] restrictions, CancellationToken cancellationToken = default)
#endif
			=> GetSchemaImpl(collectionName, restrictions, new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<DataTable> GetSchemaImpl(string collectionName, string[] restrictions, AsyncWrappingCommonArgs async)
		{
			CheckClosed();

			return _innerConnection.GetSchema(collectionName, restrictions, async);
		}

		#endregion

		#region Methods

		public new FbCommand CreateCommand()
		{
			return (FbCommand)CreateDbCommand();
		}

		protected override DbCommand CreateDbCommand()
		{
			return new FbCommand(null, this);
		}

		public override void ChangeDatabase(string db) => ChangeDatabaseImpl(db, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
#if NET48 || NETSTANDARD2_0
		public Task ChangeDatabaseAsync(string db, CancellationToken cancellationToken = default)
#else
		public override Task ChangeDatabaseAsync(string db, CancellationToken cancellationToken = default)
#endif
			=> ChangeDatabaseImpl(db, new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task ChangeDatabaseImpl(string db, AsyncWrappingCommonArgs async)
		{
			CheckClosed();

			if (string.IsNullOrEmpty(db))
			{
				throw new InvalidOperationException("Database name is not valid.");
			}

			var oldConnectionString = _connectionString;
			try
			{
				var csb = new FbConnectionStringBuilder(_connectionString);

				/* Close current connection	*/
				await CloseImpl(async).ConfigureAwait(false);

				/* Set up the new Database	*/
				csb.Database = db;
				ConnectionString = csb.ToString();

				/* Open	new	connection	*/
				await OpenImpl(async).ConfigureAwait(false);
			}
			catch (IscException ex)
			{
				ConnectionString = oldConnectionString;
				throw new FbException(ex.Message, ex);
			}
		}

		public override void Open() => OpenImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public override Task OpenAsync(CancellationToken cancellationToken) => OpenImpl(new AsyncWrappingCommonArgs(false, cancellationToken));
		private async Task OpenImpl(AsyncWrappingCommonArgs async)
		{
			if (string.IsNullOrEmpty(_connectionString))
			{
				throw new InvalidOperationException("Connection String is not initialized.");
			}
			if (!IsClosed && _state != ConnectionState.Connecting)
			{
				throw new InvalidOperationException("Connection already Open.");
			}

			try
			{
				OnStateChange(_state, ConnectionState.Connecting);

				var createdNew = default(bool);
				if (_options.Pooling)
				{
					_innerConnection = FbConnectionPoolManager.Instance.Get(_options, out createdNew);
				}
				else
				{
					_innerConnection = new FbConnectionInternal(_options);
					createdNew = true;
				}
				if (createdNew)
				{
					await _innerConnection.Connect(async).ConfigureAwait(false);
				}
				_innerConnection.SetOwningConnection(this);

				if (_options.Enlist)
				{
					try
					{
						var transaction = System.Transactions.Transaction.Current;
						if (transaction != null)
						{
							_innerConnection.EnlistTransaction(transaction);
						}
					}
					catch
					{
						// if enlistment fails clean up innerConnection
						await _innerConnection.DisposeTransaction(async).ConfigureAwait(false);

						if (_options.Pooling)
						{
							FbConnectionPoolManager.Instance.Release(_innerConnection, true);
						}
						else
						{
							await _innerConnection.Disconnect(async).ConfigureAwait(false);
							_innerConnection = null;
						}

						throw;
					}
				}

				// Bind	Warning	messages event
				_innerConnection.Database.WarningMessage = OnWarningMessage;

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

		public override void Close() => CloseImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
#if NET48 || NETSTANDARD2_0
		public Task CloseAsync()
#else
		public override Task CloseAsync()
#endif
			=> CloseImpl(new AsyncWrappingCommonArgs(true));
		internal async Task CloseImpl(AsyncWrappingCommonArgs async)
		{
			if (!IsClosed && _innerConnection != null)
			{
				try
				{
					await _innerConnection.CloseEventManager(async).ConfigureAwait(false);

					if (_innerConnection.Database != null)
					{
						_innerConnection.Database.WarningMessage = null;
					}

					await _innerConnection.DisposeTransaction(async).ConfigureAwait(false);

					await _innerConnection.ReleasePreparedCommands(async).ConfigureAwait(false);

					if (_options.Pooling)
					{
						if (_innerConnection.CancelDisabled)
						{
							await _innerConnection.EnableCancel(async).ConfigureAwait(false);
						}

						var broken = _innerConnection.Database.ConnectionBroken;
						FbConnectionPoolManager.Instance.Release(_innerConnection, !broken);
						if (broken)
						{
							await DisconnectEnlistedHelper().ConfigureAwait(false);
						}
					}
					else
					{
						await DisconnectEnlistedHelper().ConfigureAwait(false);
					}
				}
				catch
				{ }
				finally
				{
					OnStateChange(_state, ConnectionState.Closed);
				}
			}

			async Task DisconnectEnlistedHelper()
			{
				if (!_innerConnection.IsEnlisted)
				{
					await _innerConnection.Disconnect(async).ConfigureAwait(false);
				}
				_innerConnection = null;
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
		public void EnableCancel() => EnableCancelImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task EnableCancelAsync(CancellationToken cancellationToken = default) => EnableCancelImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task EnableCancelImpl(AsyncWrappingCommonArgs async)
		{
			CheckClosed();

			return _innerConnection.EnableCancel(async);
		}

		public void DisableCancel() => DisableCancelImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task DisableCancelAsync(CancellationToken cancellationToken = default) => DisableCancelImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task DisableCancelImpl(AsyncWrappingCommonArgs async)
		{
			CheckClosed();

			return _innerConnection.DisableCancel(async);
		}

		public void CancelCommand() => CancelCommandImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task CancelCommandAsync(CancellationToken cancellationToken = default) => CancelCommandImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task CancelCommandImpl(AsyncWrappingCommonArgs async)
		{
			CheckClosed();

			return _innerConnection.CancelCommand(async);
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
