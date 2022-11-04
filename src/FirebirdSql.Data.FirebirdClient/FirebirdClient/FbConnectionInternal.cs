/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
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
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Client;
using FirebirdSql.Data.Common;
using FirebirdSql.Data.Schema;

namespace FirebirdSql.Data.FirebirdClient;

internal class FbConnectionInternal
{
	#region Fields

	private DatabaseBase _db;
	private FbTransaction _activeTransaction;
	private HashSet<IFbPreparedCommand> _preparedCommands;
	private ConnectionString _connectionStringOptions;
	private FbConnection _owningConnection;
	private FbEnlistmentNotification _enlistmentNotification;

	#endregion

	#region Properties

	public DatabaseBase Database
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

	public ConnectionString ConnectionStringOptions
	{
		get { return _connectionStringOptions; }
	}

	public bool CancelDisabled { get; private set; }

	#endregion

	#region Constructors

	public FbConnectionInternal(ConnectionString options)
	{
		_preparedCommands = new HashSet<IFbPreparedCommand>();

		_connectionStringOptions = options;
	}

	#endregion

	#region Create and Drop database methods

	public void CreateDatabase(int pageSize, bool forcedWrites, bool overwrite)
	{
		var db = ClientFactory.CreateDatabase(_connectionStringOptions);

		var dpb = db.CreateDatabaseParameterBuffer();

		if (db.UseUtf8ParameterBuffer)
		{
			dpb.Append(IscCodes.isc_dpb_utf8_filename, 0);
		}
		dpb.Append(IscCodes.isc_dpb_dummy_packet_interval, new byte[] { 120, 10, 0, 0 });
		dpb.Append(IscCodes.isc_dpb_sql_dialect, new byte[] { (byte)_connectionStringOptions.Dialect, 0, 0, 0 });
		if (!string.IsNullOrEmpty(_connectionStringOptions.UserID))
		{
			dpb.Append(IscCodes.isc_dpb_user_name, _connectionStringOptions.UserID);
		}
		if (_connectionStringOptions.Charset.Length > 0)
		{
			if (!Charset.TryGetByName(_connectionStringOptions.Charset, out var charset))
				throw new ArgumentException("Invalid character set specified.");
			dpb.Append(IscCodes.isc_dpb_set_db_charset, charset.Name);
		}
		dpb.Append(IscCodes.isc_dpb_force_write, (short)(forcedWrites ? 1 : 0));
		dpb.Append(IscCodes.isc_dpb_overwrite, (overwrite ? 1 : 0));
		if (pageSize > 0)
		{
			if (!SizeHelper.IsValidPageSize(pageSize))
				throw SizeHelper.InvalidSizeException("page size");
			dpb.Append(IscCodes.isc_dpb_page_size, pageSize);
		}

		try
		{
			if (string.IsNullOrEmpty(_connectionStringOptions.UserID) && string.IsNullOrEmpty(_connectionStringOptions.Password))
			{
				db.CreateDatabaseWithTrustedAuth(dpb, _connectionStringOptions.Database, _connectionStringOptions.CryptKey);
			}
			else
			{
				db.CreateDatabase(dpb, _connectionStringOptions.Database, _connectionStringOptions.CryptKey);
			}
		}
		finally
		{
			db.Detach();
		}
	}
	public async Task CreateDatabaseAsync(int pageSize, bool forcedWrites, bool overwrite, CancellationToken cancellationToken = default)
	{
		var db = await ClientFactory.CreateDatabaseAsync(_connectionStringOptions, cancellationToken).ConfigureAwait(false);

		var dpb = db.CreateDatabaseParameterBuffer();

		if (db.UseUtf8ParameterBuffer)
		{
			dpb.Append(IscCodes.isc_dpb_utf8_filename, 0);
		}
		dpb.Append(IscCodes.isc_dpb_dummy_packet_interval, new byte[] { 120, 10, 0, 0 });
		dpb.Append(IscCodes.isc_dpb_sql_dialect, new byte[] { (byte)_connectionStringOptions.Dialect, 0, 0, 0 });
		if (!string.IsNullOrEmpty(_connectionStringOptions.UserID))
		{
			dpb.Append(IscCodes.isc_dpb_user_name, _connectionStringOptions.UserID);
		}
		if (_connectionStringOptions.Charset.Length > 0)
		{
			if (!Charset.TryGetByName(_connectionStringOptions.Charset, out var charset))
				throw new ArgumentException("Invalid character set specified.");
			dpb.Append(IscCodes.isc_dpb_set_db_charset, charset.Name);
		}
		dpb.Append(IscCodes.isc_dpb_force_write, (short)(forcedWrites ? 1 : 0));
		dpb.Append(IscCodes.isc_dpb_overwrite, (overwrite ? 1 : 0));
		if (pageSize > 0)
		{
			if (!SizeHelper.IsValidPageSize(pageSize))
				throw SizeHelper.InvalidSizeException("page size");
			dpb.Append(IscCodes.isc_dpb_page_size, pageSize);
		}

		try
		{
			if (string.IsNullOrEmpty(_connectionStringOptions.UserID) && string.IsNullOrEmpty(_connectionStringOptions.Password))
			{
				await db.CreateDatabaseWithTrustedAuthAsync(dpb, _connectionStringOptions.Database, _connectionStringOptions.CryptKey, cancellationToken).ConfigureAwait(false);
			}
			else
			{
				await db.CreateDatabaseAsync(dpb, _connectionStringOptions.Database, _connectionStringOptions.CryptKey, cancellationToken).ConfigureAwait(false);
			}
		}
		finally
		{
			await db.DetachAsync(cancellationToken).ConfigureAwait(false);
		}
	}

	public void DropDatabase()
	{
		var db = ClientFactory.CreateDatabase(_connectionStringOptions);
		try
		{
			if (string.IsNullOrEmpty(_connectionStringOptions.UserID) && string.IsNullOrEmpty(_connectionStringOptions.Password))
			{
				db.AttachWithTrustedAuth(BuildDpb(db, _connectionStringOptions), _connectionStringOptions.Database, _connectionStringOptions.CryptKey);
			}
			else
			{
				db.Attach(BuildDpb(db, _connectionStringOptions), _connectionStringOptions.Database, _connectionStringOptions.CryptKey);
			}
			db.DropDatabase();
		}
		finally
		{
			db.Detach();
		}
	}
	public async Task DropDatabaseAsync(CancellationToken cancellationToken = default)
	{
		var db = await ClientFactory.CreateDatabaseAsync(_connectionStringOptions, cancellationToken).ConfigureAwait(false);
		try
		{
			if (string.IsNullOrEmpty(_connectionStringOptions.UserID) && string.IsNullOrEmpty(_connectionStringOptions.Password))
			{
				await db.AttachWithTrustedAuthAsync(BuildDpb(db, _connectionStringOptions), _connectionStringOptions.Database, _connectionStringOptions.CryptKey, cancellationToken).ConfigureAwait(false);
			}
			else
			{
				await db.AttachAsync(BuildDpb(db, _connectionStringOptions), _connectionStringOptions.Database, _connectionStringOptions.CryptKey, cancellationToken).ConfigureAwait(false);
			}
			await db.DropDatabaseAsync(cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			await db.DetachAsync(cancellationToken).ConfigureAwait(false);
		}
	}

	#endregion

	#region Connect and Disconnect methods

	public void Connect()
	{
		if (!Charset.TryGetByName(_connectionStringOptions.Charset, out var charset))
			throw new ArgumentException("Invalid character set specified.");

		try
		{
			_db = ClientFactory.CreateDatabase(_connectionStringOptions);
			var dpb = BuildDpb(_db, _connectionStringOptions);
			if (string.IsNullOrEmpty(_connectionStringOptions.UserID) && string.IsNullOrEmpty(_connectionStringOptions.Password))
			{
				_db.AttachWithTrustedAuth(dpb, _connectionStringOptions.Database, _connectionStringOptions.CryptKey);
			}
			else
			{
				_db.Attach(dpb, _connectionStringOptions.Database, _connectionStringOptions.CryptKey);
			}
		}
		catch (IscException ex)
		{
			throw FbException.Create(ex);
		}
	}
	public async Task ConnectAsync(CancellationToken cancellationToken = default)
	{
		if (!Charset.TryGetByName(_connectionStringOptions.Charset, out var charset))
			throw new ArgumentException("Invalid character set specified.");

		try
		{
			_db = await ClientFactory.CreateDatabaseAsync(_connectionStringOptions, cancellationToken).ConfigureAwait(false);
			var dpb = BuildDpb(_db, _connectionStringOptions);
			if (string.IsNullOrEmpty(_connectionStringOptions.UserID) && string.IsNullOrEmpty(_connectionStringOptions.Password))
			{
				await _db.AttachWithTrustedAuthAsync(dpb, _connectionStringOptions.Database, _connectionStringOptions.CryptKey, cancellationToken).ConfigureAwait(false);
			}
			else
			{
				await _db.AttachAsync(dpb, _connectionStringOptions.Database, _connectionStringOptions.CryptKey, cancellationToken).ConfigureAwait(false);
			}
		}
		catch (IscException ex)
		{
			throw FbException.Create(ex);
		}
	}

	public void Disconnect()
	{
		if (_db != null)
		{
			try
			{
				_db.Detach();
			}
			catch
			{ }
			finally
			{
				_db = null;
				_owningConnection = null;
				_connectionStringOptions = null;
			}
		}
	}
	public async Task DisconnectAsync(CancellationToken cancellationToken = default)
	{
		if (_db != null)
		{
			try
			{
				await _db.DetachAsync(cancellationToken).ConfigureAwait(false);
			}
			catch
			{ }
			finally
			{
				_db = null;
				_owningConnection = null;
				_connectionStringOptions = null;
			}
		}
	}

	#endregion

	#region Transaction Handling Methods

	public FbTransaction BeginTransaction(IsolationLevel level, string transactionName)
	{
		EnsureNoActiveTransaction();

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
			throw FbException.Create(ex);
		}

		return _activeTransaction;
	}
	public async Task<FbTransaction> BeginTransactionAsync(IsolationLevel level, string transactionName, CancellationToken cancellationToken = default)
	{
		EnsureNoActiveTransaction();

		try
		{
			_activeTransaction = new FbTransaction(_owningConnection, level);
			await _activeTransaction.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

			if (transactionName != null)
			{
				_activeTransaction.Save(transactionName);
			}
		}
		catch (IscException ex)
		{
			await DisposeTransactionAsync(cancellationToken).ConfigureAwait(false);
			throw FbException.Create(ex);
		}

		return _activeTransaction;
	}

	public FbTransaction BeginTransaction(FbTransactionOptions options, string transactionName)
	{
		EnsureNoActiveTransaction();

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
			throw FbException.Create(ex);
		}

		return _activeTransaction;
	}
	public async Task<FbTransaction> BeginTransactionAsync(FbTransactionOptions options, string transactionName, CancellationToken cancellationToken = default)
	{
		EnsureNoActiveTransaction();

		try
		{
			_activeTransaction = new FbTransaction(_owningConnection, IsolationLevel.Unspecified);
			await _activeTransaction.BeginTransactionAsync(options, cancellationToken).ConfigureAwait(false);

			if (transactionName != null)
			{
				_activeTransaction.Save(transactionName);
			}
		}
		catch (IscException ex)
		{
			await DisposeTransactionAsync(cancellationToken).ConfigureAwait(false);
			throw FbException.Create(ex);
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
	public async Task DisposeTransactionAsync(CancellationToken cancellationToken = default)
	{
		if (_activeTransaction != null && !IsEnlisted)
		{
#if NET48 || NETSTANDARD2_0
			_activeTransaction.Dispose();
			await Task.CompletedTask.ConfigureAwait(false);
#else
			await _activeTransaction.DisposeAsync().ConfigureAwait(false);
#endif
			_activeTransaction = null;
		}
	}

	public void TransactionCompleted()
	{
		foreach (var command in _preparedCommands)
		{
			command.TransactionCompleted();
		}
	}
	public async Task TransactionCompletedAsync(CancellationToken cancellationToken = default)
	{
		foreach (var command in _preparedCommands)
		{
			await command.TransactionCompletedAsync(cancellationToken).ConfigureAwait(false);
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

	public FbTransaction BeginTransaction(System.Transactions.IsolationLevel isolationLevel)
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
		return BeginTransaction(il, null);
	}
	public Task<FbTransaction> BeginTransactionAsync(System.Transactions.IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
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
		return BeginTransactionAsync(il, null, cancellationToken);
	}

	#endregion

	#region Schema Methods

	public DataTable GetSchema(string collectionName, string[] restrictions)
	{
		return FbSchemaFactory.GetSchema(_owningConnection, collectionName, restrictions);
	}
	public Task<DataTable> GetSchemaAsync(string collectionName, string[] restrictions, CancellationToken cancellationToken = default)
	{
		return FbSchemaFactory.GetSchemaAsync(_owningConnection, collectionName, restrictions, cancellationToken);
	}

	#endregion

	#region Prepared Commands Methods

	public void AddPreparedCommand(IFbPreparedCommand command)
	{
		if (_preparedCommands.Contains(command))
			return;
		_preparedCommands.Add(command);
	}

	public void RemovePreparedCommand(IFbPreparedCommand command)
	{
		_preparedCommands.Remove(command);
	}

	public void ReleasePreparedCommands()
	{
		// copy the data because the collection will be modified via RemovePreparedCommand from Release
		var data = _preparedCommands.ToList();
		foreach (var item in data)
		{
			try
			{
				item.Release();
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
	public async Task ReleasePreparedCommandsAsync(CancellationToken cancellationToken = default)
	{
		// copy the data because the collection will be modified via RemovePreparedCommand from Release
		var data = _preparedCommands.ToList();
		foreach (var item in data)
		{
			try
			{
				await item.ReleaseAsync(cancellationToken).ConfigureAwait(false);
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

	public void CloseEventManager()
	{
		if (_db != null && _db.HasRemoteEventSupport)
		{
			_db.CloseEventManager();
		}
	}
	public Task CloseEventManagerAsync(CancellationToken cancellationToken = default)
	{
		if (_db != null && _db.HasRemoteEventSupport)
		{
			return _db.CloseEventManagerAsync(cancellationToken).AsTask();
		}
		return Task.CompletedTask;
	}

	#endregion

	#region Private Methods

	private void EnsureNoActiveTransaction()
	{
		if (HasActiveTransaction)
			throw new InvalidOperationException("A transaction is currently active. Parallel transactions are not supported.");
	}

	private static DatabaseParameterBufferBase BuildDpb(DatabaseBase db, ConnectionString options)
	{
		var dpb = db.CreateDatabaseParameterBuffer();

		if (db.UseUtf8ParameterBuffer)
		{
			dpb.Append(IscCodes.isc_dpb_utf8_filename, 0);
		}
		dpb.Append(IscCodes.isc_dpb_dummy_packet_interval, new byte[] { 120, 10, 0, 0 });
		dpb.Append(IscCodes.isc_dpb_sql_dialect, new byte[] { (byte)options.Dialect, 0, 0, 0 });
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
		dpb.Append(IscCodes.isc_dpb_process_name, GetProcessName(options));
		dpb.Append(IscCodes.isc_dpb_client_version, GetClientVersion());
		if (options.NoDatabaseTriggers)
		{
			dpb.Append(IscCodes.isc_dpb_no_db_triggers, 1);
		}
		if (options.NoGarbageCollect)
		{
			dpb.Append(IscCodes.isc_dpb_no_garbage_collect, (byte)0);
		}
		if (options.ParallelWorkers > 0)
		{
			dpb.Append(IscCodes.isc_dpb_parallel_workers, options.ParallelWorkers);
		}

		return dpb;
	}

	private static string GetProcessName(ConnectionString options)
	{
		if (!string.IsNullOrEmpty(options.ApplicationName))
		{
			return options.ApplicationName;
		}
		return GetSystemWebHostingPath() ?? GetRealProcessName() ?? string.Empty;
	}


	private static string GetSystemWebHostingPath()
	{
#if NET48
		var assembly = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.Equals("System.Web", StringComparison.Ordinal)).FirstOrDefault();
		if (assembly == null)
			return null;
		// showing ApplicationPhysicalPath may be wrong because of connection pooling
		// better idea?
		return (string)assembly.GetType("System.Web.Hosting.HostingEnvironment").GetProperty("ApplicationPhysicalPath").GetValue(null, null);
#else
		return null;
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
	public void EnableCancel()
	{
		_db.CancelOperation(IscCodes.fb_cancel_enable);
		CancelDisabled = false;
	}
	public async Task EnableCancelAsync(CancellationToken cancellationToken = default)
	{
		await _db.CancelOperationAsync(IscCodes.fb_cancel_enable, cancellationToken).ConfigureAwait(false);
		CancelDisabled = false;
	}

	public void DisableCancel()
	{
		_db.CancelOperation(IscCodes.fb_cancel_disable);
		CancelDisabled = true;
	}
	public async Task DisableCancelAsync(CancellationToken cancellationToken = default)
	{
		await _db.CancelOperationAsync(IscCodes.fb_cancel_disable, cancellationToken).ConfigureAwait(false);
		CancelDisabled = true;
	}

	public void CancelCommand()
	{
		_db.CancelOperation(IscCodes.fb_cancel_raise);
	}
	public Task CancelCommandAsync(CancellationToken cancellationToken = default)
	{
		return _db.CancelOperationAsync(IscCodes.fb_cancel_raise, cancellationToken).AsTask();
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
