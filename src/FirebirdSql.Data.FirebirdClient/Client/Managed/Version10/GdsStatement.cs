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
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version10;

internal class GdsStatement : StatementBase
{
	#region Fields

	protected int _handle;
	private bool _disposed;
	protected GdsDatabase _database;
	private GdsTransaction _transaction;
	protected Descriptor _parameters;
	protected Descriptor _fields;
	protected bool _allRowsFetched;
	private Queue<DbValue[]> _rows;
	private int _fetchSize;

	#endregion

	#region Properties

	public override DatabaseBase Database
	{
		get { return _database; }
	}

	public override TransactionBase Transaction
	{
		get { return _transaction; }
		set
		{
			if (_transaction != value)
			{
				if (TransactionUpdate != null && _transaction != null)
				{
					_transaction.Update -= TransactionUpdate;
					TransactionUpdate = null;
				}

				if (value == null)
				{
					_transaction = null;
				}
				else
				{
					_transaction = (GdsTransaction)value;
					TransactionUpdate = new EventHandler(TransactionUpdated);
					_transaction.Update += TransactionUpdate;
				}
			}
		}
	}

	public override Descriptor Parameters
	{
		get { return _parameters; }
		set { _parameters = value; }
	}

	public override Descriptor Fields
	{
		get { return _fields; }
	}

	public override int FetchSize
	{
		get { return _fetchSize; }
		set { _fetchSize = value; }
	}

	public int Handle
	{
		get { return _handle; }
	}

	#endregion

	#region Constructors

	public GdsStatement(GdsDatabase database)
		: this(database, null)
	{
	}

	public GdsStatement(GdsDatabase database, GdsTransaction transaction)
	{
		_handle = IscCodes.INVALID_OBJECT;
		_fetchSize = 200;
		_rows = new Queue<DbValue[]>();
		OutputParameters = new Queue<DbValue[]>();

		_database = database;

		if (transaction != null)
		{
			Transaction = transaction;
		}
	}

	#endregion

	#region Dispose2

	public override void Dispose2()
	{
		if (!_disposed)
		{
			_disposed = true;
			Release();
			Clear();
			_rows = null;
			OutputParameters = null;
			_database = null;
			_fields = null;
			_parameters = null;
			_transaction = null;
			_allRowsFetched = false;
			_handle = 0;
			_fetchSize = 0;
			base.Dispose2();
		}
	}
	public override async ValueTask Dispose2Async(CancellationToken cancellationToken = default)
	{
		if (!_disposed)
		{
			_disposed = true;
			await ReleaseAsync(cancellationToken).ConfigureAwait(false);
			Clear();
			_rows = null;
			OutputParameters = null;
			_database = null;
			_fields = null;
			_parameters = null;
			_transaction = null;
			_allRowsFetched = false;
			_handle = 0;
			_fetchSize = 0;
			await base.Dispose2Async(cancellationToken).ConfigureAwait(false);
		}
	}

	#endregion

	#region Blob Creation Metods

	public override BlobBase CreateBlob()
	{
		return new GdsBlob(_database, _transaction);
	}

	public override BlobBase CreateBlob(long blobId)
	{
		return new GdsBlob(_database, _transaction, blobId);
	}

	#endregion

	#region Array Creation Methods

	public override ArrayBase CreateArray(ArrayDesc descriptor)
	{
		var array = new GdsArray(descriptor);
		return array;
	}
	public override ValueTask<ArrayBase> CreateArrayAsync(ArrayDesc descriptor, CancellationToken cancellationToken = default)
	{
		var array = new GdsArray(descriptor);
		return ValueTask2.FromResult<ArrayBase>(array);
	}

	public override ArrayBase CreateArray(string tableName, string fieldName)
	{
		var array = new GdsArray(_database, _transaction, tableName, fieldName);
		array.Initialize();
		return array;
	}
	public override async ValueTask<ArrayBase> CreateArrayAsync(string tableName, string fieldName, CancellationToken cancellationToken = default)
	{
		var array = new GdsArray(_database, _transaction, tableName, fieldName);
		await array.InitializeAsync(cancellationToken).ConfigureAwait(false);
		return array;
	}

	public override ArrayBase CreateArray(long handle, string tableName, string fieldName)
	{
		var array = new GdsArray(_database, _transaction, handle, tableName, fieldName);
		array.Initialize();
		return array;
	}
	public override async ValueTask<ArrayBase> CreateArrayAsync(long handle, string tableName, string fieldName, CancellationToken cancellationToken = default)
	{
		var array = new GdsArray(_database, _transaction, handle, tableName, fieldName);
		await array.InitializeAsync(cancellationToken).ConfigureAwait(false);
		return array;
	}

	#endregion

	#region Batch Creation Methods

	public override BatchBase CreateBatch()
	{
		throw new NotSupportedException("Batching is not supported on this Firebird version.");
	}

	public override BatchParameterBuffer CreateBatchParameterBuffer()
	{
		throw new NotSupportedException("Batching is not supported on this Firebird version.");
	}

	#endregion

	#region Methods

	public override void Prepare(string commandText)
	{
		ClearAll();

		try
		{
			if (State == StatementState.Deallocated)
			{
				SendAllocateToBuffer();
				_database.Xdr.Flush();
				ProcessAllocateResponse((GenericResponse)_database.ReadResponse());
			}

			SendPrepareToBuffer(commandText);
			_database.Xdr.Flush();
			ProcessPrepareResponse((GenericResponse)_database.ReadResponse());

			SendInfoSqlToBuffer(StatementTypeInfoItems, IscCodes.STATEMENT_TYPE_BUFFER_SIZE);
			_database.Xdr.Flush();
			StatementType = ProcessStatementTypeInfoBuffer(ProcessInfoSqlResponse((GenericResponse)_database.ReadResponse()));

			State = StatementState.Prepared;
		}
		catch (IOException ex)
		{
			State = State == StatementState.Allocated ? StatementState.Error : State;
			throw IscException.ForIOException(ex);
		}
	}
	public override async ValueTask PrepareAsync(string commandText, CancellationToken cancellationToken = default)
	{
		ClearAll();

		try
		{
			if (State == StatementState.Deallocated)
			{
				await SendAllocateToBufferAsync(cancellationToken).ConfigureAwait(false);
				await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
				await ProcessAllocateResponseAsync((GenericResponse)await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
			}

			await SendPrepareToBufferAsync(commandText, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
			await ProcessPrepareResponseAsync((GenericResponse)await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);

			await SendInfoSqlToBufferAsync(StatementTypeInfoItems, IscCodes.STATEMENT_TYPE_BUFFER_SIZE, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
			StatementType = ProcessStatementTypeInfoBuffer(await ProcessInfoSqlResponseAsync((GenericResponse)await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false));

			State = StatementState.Prepared;
		}
		catch (IOException ex)
		{
			State = State == StatementState.Allocated ? StatementState.Error : State;
			throw IscException.ForIOException(ex);
		}
	}

	public override void Execute(int timeout, IDescriptorFiller descriptorFiller)
	{
		EnsureNotDeallocated();

		Clear();

		try
		{
			SendExecuteToBuffer(timeout, descriptorFiller);

			_database.Xdr.Flush();

			if (StatementType == DbStatementType.StoredProcedure)
			{
				ProcessStoredProcedureExecuteResponse((SqlResponse)_database.ReadResponse());
			}

			var executeResponse = (GenericResponse)_database.ReadResponse();
			ProcessExecuteResponse(executeResponse);

			if (DoRecordsAffected)
			{
				SendInfoSqlToBuffer(RowsAffectedInfoItems, IscCodes.ROWS_AFFECTED_BUFFER_SIZE);
				_database.Xdr.Flush();
				RecordsAffected = ProcessRecordsAffectedBuffer(ProcessInfoSqlResponse((GenericResponse)_database.ReadResponse()));
			}
			else
			{
				RecordsAffected = -1;
			}

			State = StatementState.Executed;
		}
		catch (IOException ex)
		{
			State = StatementState.Error;
			throw IscException.ForIOException(ex);
		}
	}
	public override async ValueTask ExecuteAsync(int timeout, IDescriptorFiller descriptorFiller, CancellationToken cancellationToken = default)
	{
		EnsureNotDeallocated();

		Clear();

		try
		{
			await SendExecuteToBufferAsync(timeout, descriptorFiller, cancellationToken).ConfigureAwait(false);

			await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			if (StatementType == DbStatementType.StoredProcedure)
			{
				await ProcessStoredProcedureExecuteResponseAsync((SqlResponse)await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
			}

			var executeResponse = (GenericResponse)await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false);
			await ProcessExecuteResponseAsync(executeResponse, cancellationToken).ConfigureAwait(false);

			if (DoRecordsAffected)
			{
				await SendInfoSqlToBufferAsync(RowsAffectedInfoItems, IscCodes.ROWS_AFFECTED_BUFFER_SIZE, cancellationToken).ConfigureAwait(false);
				await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
				RecordsAffected = ProcessRecordsAffectedBuffer(await ProcessInfoSqlResponseAsync((GenericResponse)await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false));
			}
			else
			{
				RecordsAffected = -1;
			}

			State = StatementState.Executed;
		}
		catch (IOException ex)
		{
			State = StatementState.Error;
			throw IscException.ForIOException(ex);
		}
	}

	public override DbValue[] Fetch()
	{
		EnsureNotDeallocated();

		if (StatementType == DbStatementType.StoredProcedure && !_allRowsFetched)
		{
			_allRowsFetched = true;
			return GetOutputParameters();
		}
		else if (StatementType == DbStatementType.Insert && _allRowsFetched)
		{
			return null;
		}
		else if (StatementType != DbStatementType.Select && StatementType != DbStatementType.SelectForUpdate)
		{
			return null;
		}

		if (!_allRowsFetched && _rows.Count == 0)
		{
			try
			{
				_database.Xdr.Write(IscCodes.op_fetch);
				_database.Xdr.Write(_handle);
				_database.Xdr.WriteBuffer(_fields.ToBlr().Data);
				_database.Xdr.Write(0); // p_sqldata_message_number
				_database.Xdr.Write(_fetchSize); // p_sqldata_messages
				_database.Xdr.Flush();

				var operation = _database.ReadOperation();
				if (operation == IscCodes.op_fetch_response)
				{
					var hasOperation = true;
					while (!_allRowsFetched)
					{
						var response = hasOperation
							? _database.ReadResponse(operation)
							: _database.ReadResponse();
						hasOperation = false;
						if (response is FetchResponse fetchResponse)
						{
							if (fetchResponse.Count > 0 && fetchResponse.Status == 0)
							{
								_rows.Enqueue(ReadRow());
							}
							else if (fetchResponse.Status == 100)
							{
								_allRowsFetched = true;
							}
							else
							{
								break;
							}
						}
						else
						{
							break;
						}
					}
				}
				else
				{
					_database.ReadResponse(operation);
				}
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}

		if (_rows != null && _rows.Count > 0)
		{
			return _rows.Dequeue();
		}
		else
		{
			_rows.Clear();
			return null;
		}
	}
	public override async ValueTask<DbValue[]> FetchAsync(CancellationToken cancellationToken = default)
	{
		EnsureNotDeallocated();

		if (StatementType == DbStatementType.StoredProcedure && !_allRowsFetched)
		{
			_allRowsFetched = true;
			return GetOutputParameters();
		}
		else if (StatementType == DbStatementType.Insert && _allRowsFetched)
		{
			return null;
		}
		else if (StatementType != DbStatementType.Select && StatementType != DbStatementType.SelectForUpdate)
		{
			return null;
		}

		if (!_allRowsFetched && _rows.Count == 0)
		{
			try
			{
				await _database.Xdr.WriteAsync(IscCodes.op_fetch, cancellationToken).ConfigureAwait(false);
				await _database.Xdr.WriteAsync(_handle, cancellationToken).ConfigureAwait(false);
				await _database.Xdr.WriteBufferAsync(_fields.ToBlr().Data, cancellationToken).ConfigureAwait(false);
				await _database.Xdr.WriteAsync(0, cancellationToken).ConfigureAwait(false); // p_sqldata_message_number
				await _database.Xdr.WriteAsync(_fetchSize, cancellationToken).ConfigureAwait(false); // p_sqldata_messages
				await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

				var operation = await _database.ReadOperationAsync(cancellationToken).ConfigureAwait(false);
				if (operation == IscCodes.op_fetch_response)
				{
					var hasOperation = true;
					while (!_allRowsFetched)
					{
						var response = hasOperation
							? await _database.ReadResponseAsync(operation, cancellationToken).ConfigureAwait(false)
							: await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false);
						hasOperation = false;
						if (response is FetchResponse fetchResponse)
						{
							if (fetchResponse.Count > 0 && fetchResponse.Status == 0)
							{
								_rows.Enqueue(await ReadRowAsync(cancellationToken).ConfigureAwait(false));
							}
							else if (fetchResponse.Status == 100)
							{
								_allRowsFetched = true;
							}
							else
							{
								break;
							}
						}
						else
						{
							break;
						}
					}
				}
				else
				{
					await _database.ReadResponseAsync(operation, cancellationToken).ConfigureAwait(false);
				}
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}

		if (_rows != null && _rows.Count > 0)
		{
			return _rows.Dequeue();
		}
		else
		{
			_rows.Clear();
			return null;
		}
	}

	#endregion

	#region Protected Methods

	#region op_prepare methods
	protected void SendPrepareToBuffer(string commandText)
	{
		_database.Xdr.Write(IscCodes.op_prepare_statement);
		_database.Xdr.Write(_transaction.Handle);
		_database.Xdr.Write(_handle);
		_database.Xdr.Write((int)_database.Dialect);
		_database.Xdr.Write(commandText);
		_database.Xdr.WriteBuffer(DescribeInfoAndBindInfoItems, DescribeInfoAndBindInfoItems.Length);
		_database.Xdr.Write(IscCodes.PREPARE_INFO_BUFFER_SIZE);
	}
	protected async ValueTask SendPrepareToBufferAsync(string commandText, CancellationToken cancellationToken = default)
	{
		await _database.Xdr.WriteAsync(IscCodes.op_prepare_statement, cancellationToken).ConfigureAwait(false);
		await _database.Xdr.WriteAsync(_transaction.Handle, cancellationToken).ConfigureAwait(false);
		await _database.Xdr.WriteAsync(_handle, cancellationToken).ConfigureAwait(false);
		await _database.Xdr.WriteAsync((int)_database.Dialect, cancellationToken).ConfigureAwait(false);
		await _database.Xdr.WriteAsync(commandText, cancellationToken).ConfigureAwait(false);
		await _database.Xdr.WriteBufferAsync(DescribeInfoAndBindInfoItems, DescribeInfoAndBindInfoItems.Length, cancellationToken).ConfigureAwait(false);
		await _database.Xdr.WriteAsync(IscCodes.PREPARE_INFO_BUFFER_SIZE, cancellationToken).ConfigureAwait(false);
	}

	protected void ProcessPrepareResponse(GenericResponse response)
	{
		var descriptors = ParseSqlInfo(response.Data, DescribeInfoAndBindInfoItems, new Descriptor[] { null, null });
		_fields = descriptors[0];
		_parameters = descriptors[1];
	}
	protected async ValueTask ProcessPrepareResponseAsync(GenericResponse response, CancellationToken cancellationToken = default)
	{
		var descriptors = await ParseSqlInfoAsync(response.Data, DescribeInfoAndBindInfoItems, new Descriptor[] { null, null }, cancellationToken).ConfigureAwait(false);
		_fields = descriptors[0];
		_parameters = descriptors[1];
	}
	#endregion

	#region op_info_sql methods
	protected override byte[] GetSqlInfo(byte[] items, int bufferLength)
	{
		DoInfoSqlPacket(items, bufferLength);
		_database.Xdr.Flush();
		return ProcessInfoSqlResponse((GenericResponse)_database.ReadResponse());
	}
	protected override async ValueTask<byte[]> GetSqlInfoAsync(byte[] items, int bufferLength, CancellationToken cancellationToken = default)
	{
		await DoInfoSqlPacketAsync(items, bufferLength, cancellationToken).ConfigureAwait(false);
		await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
		return await ProcessInfoSqlResponseAsync((GenericResponse)await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
	}

	protected void DoInfoSqlPacket(byte[] items, int bufferLength)
	{
		try
		{
			SendInfoSqlToBuffer(items, bufferLength);
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	protected async ValueTask DoInfoSqlPacketAsync(byte[] items, int bufferLength, CancellationToken cancellationToken = default)
	{
		try
		{
			await SendInfoSqlToBufferAsync(items, bufferLength, cancellationToken).ConfigureAwait(false);
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	protected void SendInfoSqlToBuffer(byte[] items, int bufferLength)
	{
		_database.Xdr.Write(IscCodes.op_info_sql);
		_database.Xdr.Write(_handle);
		_database.Xdr.Write(0);
		_database.Xdr.WriteBuffer(items, items.Length);
		_database.Xdr.Write(bufferLength);
	}
	protected async ValueTask SendInfoSqlToBufferAsync(byte[] items, int bufferLength, CancellationToken cancellationToken = default)
	{
		await _database.Xdr.WriteAsync(IscCodes.op_info_sql, cancellationToken).ConfigureAwait(false);
		await _database.Xdr.WriteAsync(_handle, cancellationToken).ConfigureAwait(false);
		await _database.Xdr.WriteAsync(0, cancellationToken).ConfigureAwait(false);
		await _database.Xdr.WriteBufferAsync(items, items.Length, cancellationToken).ConfigureAwait(false);
		await _database.Xdr.WriteAsync(bufferLength, cancellationToken).ConfigureAwait(false);
	}

	protected byte[] ProcessInfoSqlResponse(GenericResponse response)
	{
		Debug.Assert(response.Data != null && response.Data.Length > 0);

		return response.Data;
	}
	protected ValueTask<byte[]> ProcessInfoSqlResponseAsync(GenericResponse response, CancellationToken cancellationToken = default)
	{
		Debug.Assert(response.Data != null && response.Data.Length > 0);

		return ValueTask2.FromResult(response.Data);
	}
	#endregion

	#region op_free_statement methods
	protected override void Free(int option)
	{
		if (FreeNotNeeded(option))
			return;

		DoFreePacket(option);
		ProcessFreeResponse(_database.ReadResponse());
	}
	protected override async ValueTask FreeAsync(int option, CancellationToken cancellationToken = default)
	{
		if (FreeNotNeeded(option))
			return;

		await DoFreePacketAsync(option, cancellationToken).ConfigureAwait(false);
		await ProcessFreeResponseAsync(await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
	}

	protected bool FreeNotNeeded(int option)
	{
		// does not seem to be possible or necessary to close an execute procedure statement
		if (StatementType == DbStatementType.StoredProcedure && option == IscCodes.DSQL_close)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	protected void DoFreePacket(int option)
	{
		try
		{
			_database.Xdr.Write(IscCodes.op_free_statement);
			_database.Xdr.Write(_handle);
			_database.Xdr.Write(option);
			_database.Xdr.Flush();

			if (option == IscCodes.DSQL_drop)
			{
				_parameters = null;
				_fields = null;
			}

			Clear();
		}
		catch (IOException ex)
		{
			State = StatementState.Error;
			throw IscException.ForIOException(ex);
		}
	}
	protected async ValueTask DoFreePacketAsync(int option, CancellationToken cancellationToken = default)
	{
		try
		{
			await _database.Xdr.WriteAsync(IscCodes.op_free_statement, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(_handle, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(option, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			if (option == IscCodes.DSQL_drop)
			{
				_parameters = null;
				_fields = null;
			}

			Clear();
		}
		catch (IOException ex)
		{
			State = StatementState.Error;
			throw IscException.ForIOException(ex);
		}
	}

	protected void ProcessFreeResponse(IResponse response)
	{ }
	protected ValueTask ProcessFreeResponseAsync(IResponse response, CancellationToken cancellationToken = default)
	{
		return ValueTask2.CompletedTask;
	}
	#endregion

	#region op_allocate_statement methods
	protected void SendAllocateToBuffer()
	{
		_database.Xdr.Write(IscCodes.op_allocate_statement);
		_database.Xdr.Write(_database.Handle);
	}
	protected async ValueTask SendAllocateToBufferAsync(CancellationToken cancellationToken = default)
	{
		await _database.Xdr.WriteAsync(IscCodes.op_allocate_statement, cancellationToken).ConfigureAwait(false);
		await _database.Xdr.WriteAsync(_database.Handle, cancellationToken).ConfigureAwait(false);
	}

	protected void ProcessAllocateResponse(GenericResponse response)
	{
		_handle = response.ObjectHandle;
		_allRowsFetched = false;
		State = StatementState.Allocated;
		StatementType = DbStatementType.None;
	}
	protected ValueTask ProcessAllocateResponseAsync(GenericResponse response, CancellationToken cancellationToken = default)
	{
		_handle = response.ObjectHandle;
		_allRowsFetched = false;
		State = StatementState.Allocated;
		StatementType = DbStatementType.None;
		return ValueTask2.CompletedTask;
	}
	#endregion

	#region op_execute/op_execute2 methods
	protected virtual void SendExecuteToBuffer(int timeout, IDescriptorFiller descriptorFiller)
	{
		// this may throw error, so it needs to be before any writing
		var parametersData = GetParameterData(descriptorFiller, 0);

		if (StatementType == DbStatementType.StoredProcedure)
		{
			_database.Xdr.Write(IscCodes.op_execute2);
		}
		else
		{
			_database.Xdr.Write(IscCodes.op_execute);
		}

		_database.Xdr.Write(_handle);
		_database.Xdr.Write(_transaction.Handle);

		if (_parameters != null)
		{
			_database.Xdr.WriteBuffer(_parameters.ToBlr().Data);
			_database.Xdr.Write(0); // Message number
			_database.Xdr.Write(1); // Number of messages
			_database.Xdr.WriteBytes(parametersData, parametersData.Length);
		}
		else
		{
			_database.Xdr.WriteBuffer(null);
			_database.Xdr.Write(0);
			_database.Xdr.Write(0);
		}

		if (StatementType == DbStatementType.StoredProcedure)
		{
			_database.Xdr.WriteBuffer(_fields?.ToBlr().Data);
			_database.Xdr.Write(0); // Output message number
		}
	}
	protected virtual async ValueTask SendExecuteToBufferAsync(int timeout, IDescriptorFiller descriptorFiller, CancellationToken cancellationToken = default)
	{
		// this may throw error, so it needs to be before any writing
		var parametersData = await GetParameterDataAsync(descriptorFiller, 0, cancellationToken).ConfigureAwait(false);

		if (StatementType == DbStatementType.StoredProcedure)
		{
			await _database.Xdr.WriteAsync(IscCodes.op_execute2, cancellationToken).ConfigureAwait(false);
		}
		else
		{
			await _database.Xdr.WriteAsync(IscCodes.op_execute, cancellationToken).ConfigureAwait(false);
		}

		await _database.Xdr.WriteAsync(_handle, cancellationToken).ConfigureAwait(false);
		await _database.Xdr.WriteAsync(_transaction.Handle, cancellationToken).ConfigureAwait(false);

		if (_parameters != null)
		{
			await _database.Xdr.WriteBufferAsync(_parameters.ToBlr().Data, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(0, cancellationToken).ConfigureAwait(false); // Message number
			await _database.Xdr.WriteAsync(1, cancellationToken).ConfigureAwait(false); // Number of messages
			await _database.Xdr.WriteBytesAsync(parametersData, parametersData.Length, cancellationToken).ConfigureAwait(false);
		}
		else
		{
			await _database.Xdr.WriteBufferAsync(null, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(0, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(0, cancellationToken).ConfigureAwait(false);
		}

		if (StatementType == DbStatementType.StoredProcedure)
		{
			await _database.Xdr.WriteBufferAsync(_fields?.ToBlr().Data, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(0, cancellationToken).ConfigureAwait(false); // Output message number
		}
	}

	protected void ProcessExecuteResponse(GenericResponse response)
	{ }
	protected ValueTask ProcessExecuteResponseAsync(GenericResponse response, CancellationToken cancellationToken = default)
	{
		return ValueTask2.CompletedTask;
	}

	protected void ProcessStoredProcedureExecuteResponse(SqlResponse response)
	{
		try
		{
			if (response.Count > 0)
			{
				OutputParameters.Enqueue(ReadRow());
			}
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	protected async ValueTask ProcessStoredProcedureExecuteResponseAsync(SqlResponse response, CancellationToken cancellationToken = default)
	{
		try
		{
			if (response.Count > 0)
			{
				OutputParameters.Enqueue(await ReadRowAsync(cancellationToken).ConfigureAwait(false));
			}
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	#endregion

	protected override void TransactionUpdated(object sender, EventArgs e)
	{
		if (Transaction != null && TransactionUpdate != null)
		{
			Transaction.Update -= TransactionUpdate;
		}

		State = StatementState.Closed;
		TransactionUpdate = null;
		_allRowsFetched = false;
	}

	protected Descriptor[] ParseSqlInfo(byte[] info, byte[] items, Descriptor[] rowDescs)
	{
		return ParseTruncSqlInfo(info, items, rowDescs);
	}
	protected ValueTask<Descriptor[]> ParseSqlInfoAsync(byte[] info, byte[] items, Descriptor[] rowDescs, CancellationToken cancellationToken = default)
	{
		return ParseTruncSqlInfoAsync(info, items, rowDescs, cancellationToken);
	}

	protected Descriptor[] ParseTruncSqlInfo(byte[] info, byte[] items, Descriptor[] rowDescs)
	{
		var currentPosition = 0;
		var currentDescriptorIndex = -1;
		var currentItemIndex = 0;
		while (info[currentPosition] != IscCodes.isc_info_end)
		{
			byte item;
			while ((item = info[currentPosition++]) != IscCodes.isc_info_sql_describe_end)
			{
				switch (item)
				{
					case IscCodes.isc_info_truncated:
						currentItemIndex--;

						var newItems = new List<byte>(items.Length);
						var part = 0;
						var chock = 0;
						for (var i = 0; i < items.Length; i++)
						{
							if (items[i] == IscCodes.isc_info_sql_describe_end)
							{
								newItems.Insert(chock, IscCodes.isc_info_sql_sqlda_start);
								newItems.Insert(chock + 1, 2);

								var processedItems = (rowDescs[part] != null ? rowDescs[part].Count : (short)0);
								newItems.Insert(chock + 2, (byte)((part == currentDescriptorIndex ? currentItemIndex : processedItems) & 255));
								newItems.Insert(chock + 3, (byte)((part == currentDescriptorIndex ? currentItemIndex : processedItems) >> 8));

								part++;
								chock = i + 4 + 1;
							}
							newItems.Add(items[i]);
						}

						info = GetSqlInfo(newItems.ToArray(), info.Length);

						currentPosition = 0;
						currentDescriptorIndex = -1;
						goto Break;

					case IscCodes.isc_info_sql_select:
					case IscCodes.isc_info_sql_bind:
						currentDescriptorIndex++;

						if (info[currentPosition] == IscCodes.isc_info_truncated)
							break;

						currentPosition++;
						var len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
						currentPosition += 2;
						if (rowDescs[currentDescriptorIndex] == null)
						{
							var n = IscHelper.VaxInteger(info, currentPosition, len);
							rowDescs[currentDescriptorIndex] = new Descriptor((short)n);
							if (n == 0)
							{
								currentPosition += len;
								goto Break;
							}
						}
						currentPosition += len;
						break;

					case IscCodes.isc_info_sql_sqlda_seq:
						len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
						currentPosition += 2;
						currentItemIndex = (int)IscHelper.VaxInteger(info, currentPosition, len);
						currentPosition += len;
						break;

					case IscCodes.isc_info_sql_type:
						len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
						currentPosition += 2;
						rowDescs[currentDescriptorIndex][currentItemIndex - 1].DataType = (short)IscHelper.VaxInteger(info, currentPosition, len);
						currentPosition += len;
						break;

					case IscCodes.isc_info_sql_sub_type:
						len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
						currentPosition += 2;
						rowDescs[currentDescriptorIndex][currentItemIndex - 1].SubType = (short)IscHelper.VaxInteger(info, currentPosition, len);
						currentPosition += len;
						break;

					case IscCodes.isc_info_sql_scale:
						len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
						currentPosition += 2;
						rowDescs[currentDescriptorIndex][currentItemIndex - 1].NumericScale = (short)IscHelper.VaxInteger(info, currentPosition, len);
						currentPosition += len;
						break;

					case IscCodes.isc_info_sql_length:
						len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
						currentPosition += 2;
						rowDescs[currentDescriptorIndex][currentItemIndex - 1].Length = (short)IscHelper.VaxInteger(info, currentPosition, len);
						currentPosition += len;
						break;

					case IscCodes.isc_info_sql_field:
						len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
						currentPosition += 2;
						rowDescs[currentDescriptorIndex][currentItemIndex - 1].Name = _database.Charset.GetString(info, currentPosition, len);
						currentPosition += len;
						break;

					case IscCodes.isc_info_sql_relation:
						len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
						currentPosition += 2;
						rowDescs[currentDescriptorIndex][currentItemIndex - 1].Relation = _database.Charset.GetString(info, currentPosition, len);
						currentPosition += len;
						break;

					case IscCodes.isc_info_sql_owner:
						len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
						currentPosition += 2;
						rowDescs[currentDescriptorIndex][currentItemIndex - 1].Owner = _database.Charset.GetString(info, currentPosition, len);
						currentPosition += len;
						break;

					case IscCodes.isc_info_sql_alias:
						len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
						currentPosition += 2;
						rowDescs[currentDescriptorIndex][currentItemIndex - 1].Alias = _database.Charset.GetString(info, currentPosition, len);
						currentPosition += len;
						break;

					default:
						throw IscException.ForErrorCode(IscCodes.isc_dsql_sqlda_err);
				}
			}
			// just to get out of the loop
			Break:
			{ }
		}
		return rowDescs;
	}
	protected async ValueTask<Descriptor[]> ParseTruncSqlInfoAsync(byte[] info, byte[] items, Descriptor[] rowDescs, CancellationToken cancellationToken = default)
	{
		var currentPosition = 0;
		var currentDescriptorIndex = -1;
		var currentItemIndex = 0;
		while (info[currentPosition] != IscCodes.isc_info_end)
		{
			byte item;
			while ((item = info[currentPosition++]) != IscCodes.isc_info_sql_describe_end)
			{
				switch (item)
				{
					case IscCodes.isc_info_truncated:
						currentItemIndex--;

						var newItems = new List<byte>(items.Length);
						var part = 0;
						var chock = 0;
						for (var i = 0; i < items.Length; i++)
						{
							if (items[i] == IscCodes.isc_info_sql_describe_end)
							{
								newItems.Insert(chock, IscCodes.isc_info_sql_sqlda_start);
								newItems.Insert(chock + 1, 2);

								var processedItems = (rowDescs[part] != null ? rowDescs[part].Count : (short)0);
								newItems.Insert(chock + 2, (byte)((part == currentDescriptorIndex ? currentItemIndex : processedItems) & 255));
								newItems.Insert(chock + 3, (byte)((part == currentDescriptorIndex ? currentItemIndex : processedItems) >> 8));

								part++;
								chock = i + 4 + 1;
							}
							newItems.Add(items[i]);
						}

						info = await GetSqlInfoAsync(newItems.ToArray(), info.Length, cancellationToken).ConfigureAwait(false);

						currentPosition = 0;
						currentDescriptorIndex = -1;
						goto Break;

					case IscCodes.isc_info_sql_select:
					case IscCodes.isc_info_sql_bind:
						currentDescriptorIndex++;

						if (info[currentPosition] == IscCodes.isc_info_truncated)
							break;

						currentPosition++;
						var len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
						currentPosition += 2;
						if (rowDescs[currentDescriptorIndex] == null)
						{
							var n = IscHelper.VaxInteger(info, currentPosition, len);
							rowDescs[currentDescriptorIndex] = new Descriptor((short)n);
							if (n == 0)
							{
								currentPosition += len;
								goto Break;
							}
						}
						currentPosition += len;
						break;

					case IscCodes.isc_info_sql_sqlda_seq:
						len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
						currentPosition += 2;
						currentItemIndex = (int)IscHelper.VaxInteger(info, currentPosition, len);
						currentPosition += len;
						break;

					case IscCodes.isc_info_sql_type:
						len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
						currentPosition += 2;
						rowDescs[currentDescriptorIndex][currentItemIndex - 1].DataType = (short)IscHelper.VaxInteger(info, currentPosition, len);
						currentPosition += len;
						break;

					case IscCodes.isc_info_sql_sub_type:
						len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
						currentPosition += 2;
						rowDescs[currentDescriptorIndex][currentItemIndex - 1].SubType = (short)IscHelper.VaxInteger(info, currentPosition, len);
						currentPosition += len;
						break;

					case IscCodes.isc_info_sql_scale:
						len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
						currentPosition += 2;
						rowDescs[currentDescriptorIndex][currentItemIndex - 1].NumericScale = (short)IscHelper.VaxInteger(info, currentPosition, len);
						currentPosition += len;
						break;

					case IscCodes.isc_info_sql_length:
						len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
						currentPosition += 2;
						rowDescs[currentDescriptorIndex][currentItemIndex - 1].Length = (short)IscHelper.VaxInteger(info, currentPosition, len);
						currentPosition += len;
						break;

					case IscCodes.isc_info_sql_field:
						len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
						currentPosition += 2;
						rowDescs[currentDescriptorIndex][currentItemIndex - 1].Name = _database.Charset.GetString(info, currentPosition, len);
						currentPosition += len;
						break;

					case IscCodes.isc_info_sql_relation:
						len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
						currentPosition += 2;
						rowDescs[currentDescriptorIndex][currentItemIndex - 1].Relation = _database.Charset.GetString(info, currentPosition, len);
						currentPosition += len;
						break;

					case IscCodes.isc_info_sql_owner:
						len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
						currentPosition += 2;
						rowDescs[currentDescriptorIndex][currentItemIndex - 1].Owner = _database.Charset.GetString(info, currentPosition, len);
						currentPosition += len;
						break;

					case IscCodes.isc_info_sql_alias:
						len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
						currentPosition += 2;
						rowDescs[currentDescriptorIndex][currentItemIndex - 1].Alias = _database.Charset.GetString(info, currentPosition, len);
						currentPosition += len;
						break;

					default:
						throw IscException.ForErrorCode(IscCodes.isc_dsql_sqlda_err);
				}
			}
			// just to get out of the loop
			Break:
			{ }
		}
		return rowDescs;
	}

	protected virtual byte[] WriteParameters()
	{
		if (_parameters == null)
			return null;

		using (var ms = new MemoryStream(256))
		{
			var xdr = new XdrReaderWriter(new DataProviderStreamWrapper(ms), _database.Charset);
			for (var i = 0; i < _parameters.Count; i++)
			{
				var field = _parameters[i];
				try
				{
					WriteRawParameter(xdr, field);
					xdr.Write(field.NullFlag);
				}
				catch (IOException ex)
				{
					throw IscException.ForIOException(ex);
				}
			}
			xdr.Flush();
			return ms.ToArray();
		}
	}
	protected virtual async ValueTask<byte[]> WriteParametersAsync(CancellationToken cancellationToken = default)
	{
		if (_parameters == null)
			return null;

		using (var ms = new MemoryStream(256))
		{
			var xdr = new XdrReaderWriter(new DataProviderStreamWrapper(ms), _database.Charset);
			for (var i = 0; i < _parameters.Count; i++)
			{
				var field = _parameters[i];
				try
				{
					await WriteRawParameterAsync(xdr, field, cancellationToken).ConfigureAwait(false);
					await xdr.WriteAsync(field.NullFlag, cancellationToken).ConfigureAwait(false);
				}
				catch (IOException ex)
				{
					throw IscException.ForIOException(ex);
				}
			}
			await xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
			return ms.ToArray();
		}
	}

	protected void WriteRawParameter(IXdrWriter xdr, DbField field)
	{
		if (field.DbDataType != DbDataType.Null)
		{
			field.FixNull();

			switch (field.DbDataType)
			{
				case DbDataType.Char:
					if (field.Charset.IsOctetsCharset)
					{
						xdr.WriteOpaque(field.DbValue.GetBinary(), field.Length);
					}
					else if (field.Charset.IsNoneCharset)
					{
						var bvalue = field.Charset.GetBytes(field.DbValue.GetString());
						if (bvalue.Length > field.Length)
						{
							throw IscException.ForErrorCodes(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
						}
						xdr.WriteOpaque(bvalue, field.Length);
					}
					else
					{
						var svalue = field.DbValue.GetString();
						if ((field.Length % field.Charset.BytesPerCharacter) == 0 && svalue.Length > field.CharCount)
						{
							throw IscException.ForErrorCodes(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
						}
						xdr.WriteOpaque(field.Charset.GetBytes(svalue), field.Length);
					}
					break;

				case DbDataType.VarChar:
					if (field.Charset.IsOctetsCharset)
					{
						xdr.WriteBuffer(field.DbValue.GetBinary());
					}
					else if (field.Charset.IsNoneCharset)
					{
						var bvalue = field.Charset.GetBytes(field.DbValue.GetString());
						if (bvalue.Length > field.Length)
						{
							throw IscException.ForErrorCodes(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
						}
						xdr.WriteBuffer(bvalue);
					}
					else
					{
						var svalue = field.DbValue.GetString();
						if ((field.Length % field.Charset.BytesPerCharacter) == 0 && svalue.Length > field.CharCount)
						{
							throw IscException.ForErrorCodes(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
						}
						xdr.WriteBuffer(field.Charset.GetBytes(svalue));
					}
					break;

				case DbDataType.SmallInt:
					xdr.Write(field.DbValue.GetInt16());
					break;

				case DbDataType.Integer:
					xdr.Write(field.DbValue.GetInt32());
					break;

				case DbDataType.BigInt:
				case DbDataType.Array:
				case DbDataType.Binary:
				case DbDataType.Text:
					xdr.Write(field.DbValue.GetInt64());
					break;

				case DbDataType.Decimal:
				case DbDataType.Numeric:
					xdr.Write(field.DbValue.GetDecimal(), field.DataType, field.NumericScale);
					break;

				case DbDataType.Float:
					xdr.Write(field.DbValue.GetFloat());
					break;

				case DbDataType.Guid:
					xdr.Write(field.DbValue.GetGuid(), field.SqlType);
					break;

				case DbDataType.Double:
					xdr.Write(field.DbValue.GetDouble());
					break;

				case DbDataType.Date:
					xdr.Write(field.DbValue.GetDate());
					break;

				case DbDataType.Time:
					xdr.Write(field.DbValue.GetTime());
					break;

				case DbDataType.TimeStamp:
					xdr.Write(field.DbValue.GetDate());
					xdr.Write(field.DbValue.GetTime());
					break;

				case DbDataType.Boolean:
					xdr.Write(field.DbValue.GetBoolean());
					break;

				case DbDataType.TimeStampTZ:
					xdr.Write(field.DbValue.GetDate());
					xdr.Write(field.DbValue.GetTime());
					xdr.Write(field.DbValue.GetTimeZoneId());
					break;

				case DbDataType.TimeStampTZEx:
					xdr.Write(field.DbValue.GetDate());
					xdr.Write(field.DbValue.GetTime());
					xdr.Write(field.DbValue.GetTimeZoneId());
					xdr.Write((short)0);
					break;

				case DbDataType.TimeTZ:
					xdr.Write(field.DbValue.GetTime());
					xdr.Write(field.DbValue.GetTimeZoneId());
					break;

				case DbDataType.TimeTZEx:
					xdr.Write(field.DbValue.GetTime());
					xdr.Write(field.DbValue.GetTimeZoneId());
					xdr.Write((short)0);
					break;

				case DbDataType.Dec16:
					xdr.Write(field.DbValue.GetDecFloat(), 16);
					break;

				case DbDataType.Dec34:
					xdr.Write(field.DbValue.GetDecFloat(), 34);
					break;

				case DbDataType.Int128:
					xdr.Write(field.DbValue.GetInt128());
					break;

				default:
					throw IscException.ForStrParam($"Unknown SQL data type: {field.DataType}.");
			}
		}
	}
	protected async ValueTask WriteRawParameterAsync(IXdrWriter xdr, DbField field, CancellationToken cancellationToken = default)
	{
		if (field.DbDataType != DbDataType.Null)
		{
			field.FixNull();

			switch (field.DbDataType)
			{
				case DbDataType.Char:
					if (field.Charset.IsOctetsCharset)
					{
						await xdr.WriteOpaqueAsync(await field.DbValue.GetBinaryAsync(cancellationToken).ConfigureAwait(false), field.Length, cancellationToken).ConfigureAwait(false);
					}
					else if (field.Charset.IsNoneCharset)
					{
						var bvalue = field.Charset.GetBytes(await field.DbValue.GetStringAsync(cancellationToken).ConfigureAwait(false));
						if (bvalue.Length > field.Length)
						{
							throw IscException.ForErrorCodes(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
						}
						await xdr.WriteOpaqueAsync(bvalue, field.Length, cancellationToken).ConfigureAwait(false);
					}
					else
					{
						var svalue = await field.DbValue.GetStringAsync(cancellationToken).ConfigureAwait(false);
						if ((field.Length % field.Charset.BytesPerCharacter) == 0 && svalue.Length > field.CharCount)
						{
							throw IscException.ForErrorCodes(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
						}
						await xdr.WriteOpaqueAsync(field.Charset.GetBytes(svalue), field.Length, cancellationToken).ConfigureAwait(false);
					}
					break;

				case DbDataType.VarChar:
					if (field.Charset.IsOctetsCharset)
					{
						await xdr.WriteBufferAsync(await field.DbValue.GetBinaryAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
					}
					else if (field.Charset.IsNoneCharset)
					{
						var bvalue = field.Charset.GetBytes(await field.DbValue.GetStringAsync(cancellationToken).ConfigureAwait(false));
						if (bvalue.Length > field.Length)
						{
							throw IscException.ForErrorCodes(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
						}
						await xdr.WriteBufferAsync(bvalue, cancellationToken).ConfigureAwait(false);
					}
					else
					{
						var svalue = await field.DbValue.GetStringAsync(cancellationToken).ConfigureAwait(false);
						if ((field.Length % field.Charset.BytesPerCharacter) == 0 && svalue.Length > field.CharCount)
						{
							throw IscException.ForErrorCodes(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
						}
						await xdr.WriteBufferAsync(field.Charset.GetBytes(svalue), cancellationToken).ConfigureAwait(false);
					}
					break;

				case DbDataType.SmallInt:
					await xdr.WriteAsync(field.DbValue.GetInt16(), cancellationToken).ConfigureAwait(false);
					break;

				case DbDataType.Integer:
					await xdr.WriteAsync(field.DbValue.GetInt32(), cancellationToken).ConfigureAwait(false);
					break;

				case DbDataType.BigInt:
				case DbDataType.Array:
				case DbDataType.Binary:
				case DbDataType.Text:
					await xdr.WriteAsync(field.DbValue.GetInt64(), cancellationToken).ConfigureAwait(false);
					break;

				case DbDataType.Decimal:
				case DbDataType.Numeric:
					await xdr.WriteAsync(field.DbValue.GetDecimal(), field.DataType, field.NumericScale, cancellationToken).ConfigureAwait(false);
					break;

				case DbDataType.Float:
					await xdr.WriteAsync(field.DbValue.GetFloat(), cancellationToken).ConfigureAwait(false);
					break;

				case DbDataType.Guid:
					await xdr.WriteAsync(field.DbValue.GetGuid(), field.SqlType, cancellationToken).ConfigureAwait(false);
					break;

				case DbDataType.Double:
					await xdr.WriteAsync(field.DbValue.GetDouble(), cancellationToken).ConfigureAwait(false);
					break;

				case DbDataType.Date:
					await xdr.WriteAsync(field.DbValue.GetDate(), cancellationToken).ConfigureAwait(false);
					break;

				case DbDataType.Time:
					await xdr.WriteAsync(field.DbValue.GetTime(), cancellationToken).ConfigureAwait(false);
					break;

				case DbDataType.TimeStamp:
					await xdr.WriteAsync(field.DbValue.GetDate(), cancellationToken).ConfigureAwait(false);
					await xdr.WriteAsync(field.DbValue.GetTime(), cancellationToken).ConfigureAwait(false);
					break;

				case DbDataType.Boolean:
					await xdr.WriteAsync(field.DbValue.GetBoolean(), cancellationToken).ConfigureAwait(false);
					break;

				case DbDataType.TimeStampTZ:
					await xdr.WriteAsync(field.DbValue.GetDate(), cancellationToken).ConfigureAwait(false);
					await xdr.WriteAsync(field.DbValue.GetTime(), cancellationToken).ConfigureAwait(false);
					await xdr.WriteAsync(field.DbValue.GetTimeZoneId(), cancellationToken).ConfigureAwait(false);
					break;

				case DbDataType.TimeStampTZEx:
					await xdr.WriteAsync(field.DbValue.GetDate(), cancellationToken).ConfigureAwait(false);
					await xdr.WriteAsync(field.DbValue.GetTime(), cancellationToken).ConfigureAwait(false);
					await xdr.WriteAsync(field.DbValue.GetTimeZoneId(), cancellationToken).ConfigureAwait(false);
					await xdr.WriteAsync((short)0, cancellationToken).ConfigureAwait(false);
					break;

				case DbDataType.TimeTZ:
					await xdr.WriteAsync(field.DbValue.GetTime(), cancellationToken).ConfigureAwait(false);
					await xdr.WriteAsync(field.DbValue.GetTimeZoneId(), cancellationToken).ConfigureAwait(false);
					break;

				case DbDataType.TimeTZEx:
					await xdr.WriteAsync(field.DbValue.GetTime(), cancellationToken).ConfigureAwait(false);
					await xdr.WriteAsync(field.DbValue.GetTimeZoneId(), cancellationToken).ConfigureAwait(false);
					await xdr.WriteAsync((short)0, cancellationToken).ConfigureAwait(false);
					break;

				case DbDataType.Dec16:
					await xdr.WriteAsync(field.DbValue.GetDecFloat(), 16, cancellationToken).ConfigureAwait(false);
					break;

				case DbDataType.Dec34:
					await xdr.WriteAsync(field.DbValue.GetDecFloat(), 34, cancellationToken).ConfigureAwait(false);
					break;

				case DbDataType.Int128:
					await xdr.WriteAsync(field.DbValue.GetInt128(), cancellationToken).ConfigureAwait(false);
					break;

				default:
					throw IscException.ForStrParam($"Unknown SQL data type: {field.DataType}.");
			}
		}
	}

	protected object ReadRawValue(IXdrReader xdr, DbField field)
	{
		var innerCharset = !_database.Charset.IsNoneCharset ? _database.Charset : field.Charset;

		switch (field.DbDataType)
		{
			case DbDataType.Char:
				if (field.Charset.IsOctetsCharset)
				{
					return xdr.ReadOpaque(field.Length);
				}
				else
				{
					var s = xdr.ReadString(innerCharset, field.Length);
					if ((field.Length % field.Charset.BytesPerCharacter) == 0 &&
						s.Length > field.CharCount)
					{
						return s.Substring(0, field.CharCount);
					}
					else
					{
						return s;
					}
				}

			case DbDataType.VarChar:
				if (field.Charset.IsOctetsCharset)
				{
					return xdr.ReadBuffer();
				}
				else
				{
					return xdr.ReadString(innerCharset);
				}

			case DbDataType.SmallInt:
				return xdr.ReadInt16();

			case DbDataType.Integer:
				return xdr.ReadInt32();

			case DbDataType.Array:
			case DbDataType.Binary:
			case DbDataType.Text:
			case DbDataType.BigInt:
				return xdr.ReadInt64();

			case DbDataType.Decimal:
			case DbDataType.Numeric:
				return xdr.ReadDecimal(field.DataType, field.NumericScale);

			case DbDataType.Float:
				return xdr.ReadSingle();

			case DbDataType.Guid:
				return xdr.ReadGuid(field.SqlType);

			case DbDataType.Double:
				return xdr.ReadDouble();

			case DbDataType.Date:
				return xdr.ReadDate();

			case DbDataType.Time:
				return xdr.ReadTime();

			case DbDataType.TimeStamp:
				return xdr.ReadDateTime();

			case DbDataType.Boolean:
				return xdr.ReadBoolean();

			case DbDataType.TimeStampTZ:
				return xdr.ReadZonedDateTime(false);

			case DbDataType.TimeStampTZEx:
				return xdr.ReadZonedDateTime(true);

			case DbDataType.TimeTZ:
				return xdr.ReadZonedTime(false);

			case DbDataType.TimeTZEx:
				return xdr.ReadZonedTime(true);

			case DbDataType.Dec16:
				return xdr.ReadDec16();

			case DbDataType.Dec34:
				return xdr.ReadDec34();

			case DbDataType.Int128:
				return xdr.ReadInt128();

			default:
				throw TypeHelper.InvalidDataType((int)field.DbDataType);
		}
	}
	protected async ValueTask<object> ReadRawValueAsync(IXdrReader xdr, DbField field, CancellationToken cancellationToken = default)
	{
		var innerCharset = !_database.Charset.IsNoneCharset ? _database.Charset : field.Charset;

		switch (field.DbDataType)
		{
			case DbDataType.Char:
				if (field.Charset.IsOctetsCharset)
				{
					return await xdr.ReadOpaqueAsync(field.Length, cancellationToken).ConfigureAwait(false);
				}
				else
				{
					var s = await xdr.ReadStringAsync(innerCharset, field.Length, cancellationToken).ConfigureAwait(false);
					if ((field.Length % field.Charset.BytesPerCharacter) == 0 &&
						s.Length > field.CharCount)
					{
						return s.Substring(0, field.CharCount);
					}
					else
					{
						return s;
					}
				}

			case DbDataType.VarChar:
				if (field.Charset.IsOctetsCharset)
				{
					return await xdr.ReadBufferAsync(cancellationToken).ConfigureAwait(false);
				}
				else
				{
					return await xdr.ReadStringAsync(innerCharset, cancellationToken).ConfigureAwait(false);
				}

			case DbDataType.SmallInt:
				return await xdr.ReadInt16Async(cancellationToken).ConfigureAwait(false);

			case DbDataType.Integer:
				return await xdr.ReadInt32Async(cancellationToken).ConfigureAwait(false);

			case DbDataType.Array:
			case DbDataType.Binary:
			case DbDataType.Text:
			case DbDataType.BigInt:
				return await xdr.ReadInt64Async(cancellationToken).ConfigureAwait(false);

			case DbDataType.Decimal:
			case DbDataType.Numeric:
				return await xdr.ReadDecimalAsync(field.DataType, field.NumericScale, cancellationToken).ConfigureAwait(false);

			case DbDataType.Float:
				return await xdr.ReadSingleAsync(cancellationToken).ConfigureAwait(false);

			case DbDataType.Guid:
				return await xdr.ReadGuidAsync(field.SqlType, cancellationToken).ConfigureAwait(false);

			case DbDataType.Double:
				return await xdr.ReadDoubleAsync(cancellationToken).ConfigureAwait(false);

			case DbDataType.Date:
				return await xdr.ReadDateAsync(cancellationToken).ConfigureAwait(false);

			case DbDataType.Time:
				return await xdr.ReadTimeAsync(cancellationToken).ConfigureAwait(false);

			case DbDataType.TimeStamp:
				return await xdr.ReadDateTimeAsync(cancellationToken).ConfigureAwait(false);

			case DbDataType.Boolean:
				return await xdr.ReadBooleanAsync(cancellationToken).ConfigureAwait(false);

			case DbDataType.TimeStampTZ:
				return await xdr.ReadZonedDateTimeAsync(false, cancellationToken).ConfigureAwait(false);

			case DbDataType.TimeStampTZEx:
				return await xdr.ReadZonedDateTimeAsync(true, cancellationToken).ConfigureAwait(false);

			case DbDataType.TimeTZ:
				return await xdr.ReadZonedTimeAsync(false, cancellationToken).ConfigureAwait(false);

			case DbDataType.TimeTZEx:
				return await xdr.ReadZonedTimeAsync(true, cancellationToken).ConfigureAwait(false);

			case DbDataType.Dec16:
				return await xdr.ReadDec16Async(cancellationToken).ConfigureAwait(false);

			case DbDataType.Dec34:
				return await xdr.ReadDec34Async(cancellationToken).ConfigureAwait(false);

			case DbDataType.Int128:
				return await xdr.ReadInt128Async(cancellationToken).ConfigureAwait(false);

			default:
				throw TypeHelper.InvalidDataType((int)field.DbDataType);
		}
	}

	protected void Clear()
	{
		if (_rows != null && _rows.Count > 0)
		{
			_rows.Clear();
		}
		if (OutputParameters != null && OutputParameters.Count > 0)
		{
			OutputParameters.Clear();
		}

		_allRowsFetched = false;
	}

	protected void ClearAll()
	{
		Clear();

		_parameters = null;
		_fields = null;
	}

	protected virtual DbValue[] ReadRow()
	{
		var row = new DbValue[_fields.Count];
		try
		{
			for (var i = 0; i < _fields.Count; i++)
			{
				var value = ReadRawValue(_database.Xdr, _fields[i]);
				var sqlInd = _database.Xdr.ReadInt32();
				if (sqlInd == -1)
				{
					row[i] = new DbValue(this, _fields[i], null);
				}
				else if (sqlInd == 0)
				{
					row[i] = new DbValue(this, _fields[i], value);
				}
				else
				{
					throw IscException.ForStrParam($"Invalid {nameof(sqlInd)} value: {sqlInd}.");
				}
			}
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
		return row;
	}
	protected virtual async ValueTask<DbValue[]> ReadRowAsync(CancellationToken cancellationToken = default)
	{
		var row = new DbValue[_fields.Count];
		try
		{
			for (var i = 0; i < _fields.Count; i++)
			{
				var value = await ReadRawValueAsync(_database.Xdr, _fields[i], cancellationToken).ConfigureAwait(false);
				var sqlInd = await _database.Xdr.ReadInt32Async(cancellationToken).ConfigureAwait(false);
				if (sqlInd == -1)
				{
					row[i] = new DbValue(this, _fields[i], null);
				}
				else if (sqlInd == 0)
				{
					row[i] = new DbValue(this, _fields[i], value);
				}
				else
				{
					throw IscException.ForStrParam($"Invalid {nameof(sqlInd)} value: {sqlInd}.");
				}
			}
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
		return row;
	}

	#endregion

	#region Protected Internal Methods

	protected internal byte[] GetParameterData(IDescriptorFiller descriptorFiller, int index)
	{
		descriptorFiller.Fill(_parameters, index);
		return WriteParameters();
	}
	protected internal async ValueTask<byte[]> GetParameterDataAsync(IDescriptorFiller descriptorFiller, int index, CancellationToken cancellationToken = default)
	{
		await descriptorFiller.FillAsync(_parameters, index, cancellationToken).ConfigureAwait(false);
		return WriteParameters();
	}

	#endregion
}
