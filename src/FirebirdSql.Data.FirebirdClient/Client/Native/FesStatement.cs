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
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Client.Native.Handles;
using FirebirdSql.Data.Client.Native.Marshalers;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Native;

internal sealed class FesStatement : StatementBase
{
	#region Fields

	private StatementHandle _handle;
	private bool _disposed;
	private FesDatabase _database;
	private FesTransaction _transaction;
	private Descriptor _parameters;
	private Descriptor _fields;
	private bool _allRowsFetched;
	private IntPtr[] _statusVector;
	private IntPtr _fetchSqlDa;

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
					_transaction = (FesTransaction)value;
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
		get { return 200; }
		set { }
	}

	#endregion

	#region Constructors

	public FesStatement(FesDatabase database)
		: this(database, null)
	{
	}

	public FesStatement(FesDatabase database, FesTransaction transaction)
	{
		_database = database;
		_handle = new StatementHandle();
		OutputParameters = new Queue<DbValue[]>();
		_statusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];
		_fetchSqlDa = IntPtr.Zero;

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
			_database = null;
			_fields = null;
			_parameters = null;
			_transaction = null;
			OutputParameters = null;
			_statusVector = null;
			_allRowsFetched = false;
			_handle.Dispose();
			FetchSize = 0;
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
			_database = null;
			_fields = null;
			_parameters = null;
			_transaction = null;
			OutputParameters = null;
			_statusVector = null;
			_allRowsFetched = false;
			_handle.Dispose();
			FetchSize = 0;
			await base.Dispose2Async(cancellationToken).ConfigureAwait(false);
		}
	}

	#endregion

	#region Blob Creation Metods

	public override BlobBase CreateBlob()
	{
		return new FesBlob(_database, _transaction);
	}

	public override BlobBase CreateBlob(long blobId)
	{
		return new FesBlob(_database, _transaction, blobId);
	}

	#endregion

	#region Array Creation Methods

	public override ArrayBase CreateArray(ArrayDesc descriptor)
	{
		var array = new FesArray(descriptor);
		return array;
	}
	public override ValueTask<ArrayBase> CreateArrayAsync(ArrayDesc descriptor, CancellationToken cancellationToken = default)
	{
		var array = new FesArray(descriptor);
		return ValueTask2.FromResult<ArrayBase>(array);
	}

	public override ArrayBase CreateArray(string tableName, string fieldName)
	{
		var array = new FesArray(_database, _transaction, tableName, fieldName);
		array.Initialize();
		return array;
	}
	public override async ValueTask<ArrayBase> CreateArrayAsync(string tableName, string fieldName, CancellationToken cancellationToken = default)
	{
		var array = new FesArray(_database, _transaction, tableName, fieldName);
		await array.InitializeAsync(cancellationToken).ConfigureAwait(false);
		return array;
	}

	public override ArrayBase CreateArray(long handle, string tableName, string fieldName)
	{
		var array = new FesArray(_database, _transaction, handle, tableName, fieldName);
		array.Initialize();
		return array;
	}
	public override async ValueTask<ArrayBase> CreateArrayAsync(long handle, string tableName, string fieldName, CancellationToken cancellationToken = default)
	{
		var array = new FesArray(_database, _transaction, handle, tableName, fieldName);
		await array.InitializeAsync(cancellationToken).ConfigureAwait(false);
		return array;
	}

	public override BatchBase CreateBatch()
	{
		throw new NotSupportedException("Batching isn't, yet, supported on Firebird Embedded.");
	}

	public override BatchParameterBuffer CreateBatchParameterBuffer()
	{
		throw new NotSupportedException("Batching isn't, yet, supported on Firebird Embedded.");
	}

	#endregion

	#region Methods

	public override void Release()
	{
		XsqldaMarshaler.CleanUpNativeData(ref _fetchSqlDa);

		base.Release();
	}
	public override ValueTask ReleaseAsync(CancellationToken cancellationToken = default)
	{
		XsqldaMarshaler.CleanUpNativeData(ref _fetchSqlDa);

		return base.ReleaseAsync(cancellationToken);
	}

	public override void Close()
	{
		XsqldaMarshaler.CleanUpNativeData(ref _fetchSqlDa);

		base.Close();
	}
	public override ValueTask CloseAsync(CancellationToken cancellationToken = default)
	{
		XsqldaMarshaler.CleanUpNativeData(ref _fetchSqlDa);

		return base.CloseAsync(cancellationToken);
	}

	public override void Prepare(string commandText)
	{
		ClearAll();

		ClearStatusVector();

		if (State == StatementState.Deallocated)
		{
			Allocate();
		}

		_fields = new Descriptor(1);

		var sqlda = XsqldaMarshaler.MarshalManagedToNative(_database.Charset, _fields);
		var trHandle = _transaction.HandlePtr;

		var buffer = _database.Charset.GetBytes(commandText);

		_database.FbClient.isc_dsql_prepare(
			_statusVector,
			ref trHandle,
			ref _handle,
			(short)buffer.Length,
			buffer,
			_database.Dialect,
			sqlda);

		var descriptor = XsqldaMarshaler.MarshalNativeToManaged(_database.Charset, sqlda);

		XsqldaMarshaler.CleanUpNativeData(ref sqlda);

		_database.ProcessStatusVector(_statusVector);

		_fields = descriptor;

		if (_fields.ActualCount > 0 && _fields.ActualCount != _fields.Count)
		{
			Describe();
		}
		else
		{
			if (_fields.ActualCount == 0)
			{
				_fields = new Descriptor(0);
			}
		}

		_fields.ResetValues();

		DescribeParameters();

		StatementType = GetStatementType();

		State = StatementState.Prepared;
	}
	public override async ValueTask PrepareAsync(string commandText, CancellationToken cancellationToken = default)
	{
		ClearAll();

		ClearStatusVector();

		if (State == StatementState.Deallocated)
		{
			Allocate();
		}

		_fields = new Descriptor(1);

		var sqlda = XsqldaMarshaler.MarshalManagedToNative(_database.Charset, _fields);
		var trHandle = _transaction.HandlePtr;

		var buffer = _database.Charset.GetBytes(commandText);

		_database.FbClient.isc_dsql_prepare(
			_statusVector,
			ref trHandle,
			ref _handle,
			(short)buffer.Length,
			buffer,
			_database.Dialect,
			sqlda);

		var descriptor = XsqldaMarshaler.MarshalNativeToManaged(_database.Charset, sqlda);

		XsqldaMarshaler.CleanUpNativeData(ref sqlda);

		_database.ProcessStatusVector(_statusVector);

		_fields = descriptor;

		if (_fields.ActualCount > 0 && _fields.ActualCount != _fields.Count)
		{
			Describe();
		}
		else
		{
			if (_fields.ActualCount == 0)
			{
				_fields = new Descriptor(0);
			}
		}

		_fields.ResetValues();

		DescribeParameters();

		StatementType = await GetStatementTypeAsync(cancellationToken).ConfigureAwait(false);

		State = StatementState.Prepared;
	}

	public override void Execute(int timeout, IDescriptorFiller descriptorFiller)
	{
		EnsureNotDeallocated();

		descriptorFiller.Fill(_parameters, 0);

		ClearStatusVector();
		NativeHelpers.CallIfExists(() =>
		{
			_database.FbClient.fb_dsql_set_timeout(_statusVector, ref _handle, (uint)timeout);
			_database.ProcessStatusVector(_statusVector);
		});

		ClearStatusVector();

		var inSqlda = IntPtr.Zero;
		var outSqlda = IntPtr.Zero;

		if (_parameters != null)
		{
			inSqlda = XsqldaMarshaler.MarshalManagedToNative(_database.Charset, _parameters);
		}
		if (StatementType == DbStatementType.StoredProcedure)
		{
			Fields.ResetValues();
			outSqlda = XsqldaMarshaler.MarshalManagedToNative(_database.Charset, _fields);
		}

		var trHandle = _transaction.HandlePtr;

		_database.FbClient.isc_dsql_execute2(
			_statusVector,
			ref trHandle,
			ref _handle,
			IscCodes.SQLDA_VERSION1,
			inSqlda,
			outSqlda);

		if (outSqlda != IntPtr.Zero)
		{
			var descriptor = XsqldaMarshaler.MarshalNativeToManaged(_database.Charset, outSqlda, true);

			var values = new DbValue[descriptor.Count];

			for (var i = 0; i < values.Length; i++)
			{
				var d = descriptor[i];
				var value = d.DbValue.GetValue();
				values[i] = new DbValue(this, d, value);
			}

			OutputParameters.Enqueue(values);
		}

		XsqldaMarshaler.CleanUpNativeData(ref inSqlda);
		XsqldaMarshaler.CleanUpNativeData(ref outSqlda);

		_database.ProcessStatusVector(_statusVector);

		if (DoRecordsAffected)
		{
			RecordsAffected = GetRecordsAffected();
		}
		else
		{
			RecordsAffected = -1;
		}

		State = StatementState.Executed;
	}
	public override async ValueTask ExecuteAsync(int timeout, IDescriptorFiller descriptorFiller, CancellationToken cancellationToken = default)
	{
		EnsureNotDeallocated();

		await descriptorFiller.FillAsync(_parameters, 0, cancellationToken).ConfigureAwait(false);

		ClearStatusVector();
		NativeHelpers.CallIfExists(() =>
		{
			_database.FbClient.fb_dsql_set_timeout(_statusVector, ref _handle, (uint)timeout);
			_database.ProcessStatusVector(_statusVector);
		});

		ClearStatusVector();

		var inSqlda = IntPtr.Zero;
		var outSqlda = IntPtr.Zero;

		if (_parameters != null)
		{
			inSqlda = XsqldaMarshaler.MarshalManagedToNative(_database.Charset, _parameters);
		}
		if (StatementType == DbStatementType.StoredProcedure)
		{
			Fields.ResetValues();
			outSqlda = XsqldaMarshaler.MarshalManagedToNative(_database.Charset, _fields);
		}

		var trHandle = _transaction.HandlePtr;

		_database.FbClient.isc_dsql_execute2(
			_statusVector,
			ref trHandle,
			ref _handle,
			IscCodes.SQLDA_VERSION1,
			inSqlda,
			outSqlda);

		if (outSqlda != IntPtr.Zero)
		{
			var descriptor = XsqldaMarshaler.MarshalNativeToManaged(_database.Charset, outSqlda, true);

			var values = new DbValue[descriptor.Count];

			for (var i = 0; i < values.Length; i++)
			{
				var d = descriptor[i];
				var value = await d.DbValue.GetValueAsync(cancellationToken).ConfigureAwait(false);
				values[i] = new DbValue(this, d, value);
			}

			OutputParameters.Enqueue(values);
		}

		XsqldaMarshaler.CleanUpNativeData(ref inSqlda);
		XsqldaMarshaler.CleanUpNativeData(ref outSqlda);

		_database.ProcessStatusVector(_statusVector);

		if (DoRecordsAffected)
		{
			RecordsAffected = await GetRecordsAffectedAsync(cancellationToken).ConfigureAwait(false);
		}
		else
		{
			RecordsAffected = -1;
		}

		State = StatementState.Executed;
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

		if (_allRowsFetched)
		{
			return null;
		}

		_fields.ResetValues();

		if (_fetchSqlDa == IntPtr.Zero)
		{
			_fetchSqlDa = XsqldaMarshaler.MarshalManagedToNative(_database.Charset, _fields);
		}

		ClearStatusVector();

		var status = _database.FbClient.isc_dsql_fetch(_statusVector, ref _handle, IscCodes.SQLDA_VERSION1, _fetchSqlDa);
		if (status == new IntPtr(100))
		{
			_allRowsFetched = true;

			XsqldaMarshaler.CleanUpNativeData(ref _fetchSqlDa);

			return null;
		}
		else
		{
			var rowDesc = XsqldaMarshaler.MarshalNativeToManaged(_database.Charset, _fetchSqlDa, true);

			if (_fields.Count == rowDesc.Count)
			{
				for (var i = 0; i < _fields.Count; i++)
				{
					if (_fields[i].IsArray() && _fields[i].ArrayHandle != null)
					{
						rowDesc[i].ArrayHandle = _fields[i].ArrayHandle;
					}
				}
			}

			_fields = rowDesc;

			_database.ProcessStatusVector(_statusVector);

			var row = new DbValue[_fields.ActualCount];
			for (var i = 0; i < row.Length; i++)
			{
				var d = _fields[i];
				var value = d.DbValue.GetValue();
				row[i] = new DbValue(this, d, value);
			}
			return row;
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

		if (_allRowsFetched)
		{
			return null;
		}

		_fields.ResetValues();

		if (_fetchSqlDa == IntPtr.Zero)
		{
			_fetchSqlDa = XsqldaMarshaler.MarshalManagedToNative(_database.Charset, _fields);
		}

		ClearStatusVector();

		var status = _database.FbClient.isc_dsql_fetch(_statusVector, ref _handle, IscCodes.SQLDA_VERSION1, _fetchSqlDa);
		if (status == new IntPtr(100))
		{
			_allRowsFetched = true;

			XsqldaMarshaler.CleanUpNativeData(ref _fetchSqlDa);

			return null;
		}
		else
		{
			var rowDesc = XsqldaMarshaler.MarshalNativeToManaged(_database.Charset, _fetchSqlDa, true);

			if (_fields.Count == rowDesc.Count)
			{
				for (var i = 0; i < _fields.Count; i++)
				{
					if (_fields[i].IsArray() && _fields[i].ArrayHandle != null)
					{
						rowDesc[i].ArrayHandle = _fields[i].ArrayHandle;
					}
				}
			}

			_fields = rowDesc;

			_database.ProcessStatusVector(_statusVector);

			var row = new DbValue[_fields.ActualCount];
			for (var i = 0; i < row.Length; i++)
			{
				var d = _fields[i];
				var value = await d.DbValue.GetValueAsync(cancellationToken).ConfigureAwait(false);
				row[i] = new DbValue(this, d, value);
			}
			return row;
		}
	}

	#endregion

	#region Protected Methods

	protected override void Free(int option)
	{
		// Does	not	seem to	be possible	or necessary to	close
		// an execute procedure	statement.
		if (StatementType == DbStatementType.StoredProcedure && option == IscCodes.DSQL_close)
		{
			return;
		}

		ClearStatusVector();

		_database.FbClient.isc_dsql_free_statement(
			_statusVector,
			ref _handle,
			(short)option);

		if (option == IscCodes.DSQL_drop)
		{
			_parameters = null;
			_fields = null;
		}

		Clear();
		_allRowsFetched = false;

		_database.ProcessStatusVector(_statusVector);
	}
	protected override ValueTask FreeAsync(int option, CancellationToken cancellationToken = default)
	{
		// Does	not	seem to	be possible	or necessary to	close
		// an execute procedure	statement.
		if (StatementType == DbStatementType.StoredProcedure && option == IscCodes.DSQL_close)
		{
			return ValueTask2.CompletedTask;
		}

		ClearStatusVector();

		_database.FbClient.isc_dsql_free_statement(
			_statusVector,
			ref _handle,
			(short)option);

		if (option == IscCodes.DSQL_drop)
		{
			_parameters = null;
			_fields = null;
		}

		Clear();
		_allRowsFetched = false;

		_database.ProcessStatusVector(_statusVector);

		return ValueTask2.CompletedTask;
	}

	protected override void TransactionUpdated(object sender, EventArgs e)
	{
		if (Transaction != null && TransactionUpdate != null)
		{
			Transaction.Update -= TransactionUpdate;
		}
		Clear();
		State = StatementState.Closed;
		TransactionUpdate = null;
		_allRowsFetched = false;
	}

	protected override byte[] GetSqlInfo(byte[] items, int bufferLength)
	{
		ClearStatusVector();

		var buffer = new byte[bufferLength];

		_database.FbClient.isc_dsql_sql_info(
			_statusVector,
			ref _handle,
			(short)items.Length,
			items,
			(short)bufferLength,
			buffer);

		_database.ProcessStatusVector(_statusVector);

		return buffer;
	}
	protected override ValueTask<byte[]> GetSqlInfoAsync(byte[] items, int bufferLength, CancellationToken cancellationToken = default)
	{
		ClearStatusVector();

		var buffer = new byte[bufferLength];

		_database.FbClient.isc_dsql_sql_info(
			_statusVector,
			ref _handle,
			(short)items.Length,
			items,
			(short)bufferLength,
			buffer);

		_database.ProcessStatusVector(_statusVector);

		return ValueTask2.FromResult(buffer);
	}

	#endregion

	#region Private Methods

	private void ClearStatusVector()
	{
		Array.Clear(_statusVector, 0, _statusVector.Length);
	}

	private void Clear()
	{
		OutputParameters?.Clear();
	}

	private void ClearAll()
	{
		Clear();

		_parameters = null;
		_fields = null;
	}

	private void Allocate()
	{
		ClearStatusVector();

		var dbHandle = _database.HandlePtr;

		_database.FbClient.isc_dsql_allocate_statement(
			_statusVector,
			ref dbHandle,
			ref _handle);

		_database.ProcessStatusVector(_statusVector);

		_allRowsFetched = false;
		State = StatementState.Allocated;
		StatementType = DbStatementType.None;
	}

	private void Describe()
	{
		ClearStatusVector();

		_fields = new Descriptor(_fields.ActualCount);

		var sqlda = XsqldaMarshaler.MarshalManagedToNative(_database.Charset, _fields);

		_database.FbClient.isc_dsql_describe(
			_statusVector,
			ref _handle,
			IscCodes.SQLDA_VERSION1,
			sqlda);

		var descriptor = XsqldaMarshaler.MarshalNativeToManaged(_database.Charset, sqlda);

		XsqldaMarshaler.CleanUpNativeData(ref sqlda);

		_database.ProcessStatusVector(_statusVector);

		_fields = descriptor;
	}

	private void DescribeParameters()
	{
		ClearStatusVector();

		_parameters = new Descriptor(1);

		var sqlda = XsqldaMarshaler.MarshalManagedToNative(_database.Charset, _parameters);


		_database.FbClient.isc_dsql_describe_bind(
			_statusVector,
			ref _handle,
			IscCodes.SQLDA_VERSION1,
			sqlda);

		var descriptor = XsqldaMarshaler.MarshalNativeToManaged(_database.Charset, sqlda);

		_database.ProcessStatusVector(_statusVector);

		if (descriptor.ActualCount != 0 && descriptor.Count != descriptor.ActualCount)
		{
			var n = descriptor.ActualCount;
			descriptor = new Descriptor(n);

			XsqldaMarshaler.CleanUpNativeData(ref sqlda);

			sqlda = XsqldaMarshaler.MarshalManagedToNative(_database.Charset, descriptor);

			_database.FbClient.isc_dsql_describe_bind(
				_statusVector,
				ref _handle,
				IscCodes.SQLDA_VERSION1,
				sqlda);

			descriptor = XsqldaMarshaler.MarshalNativeToManaged(_database.Charset, sqlda);

			XsqldaMarshaler.CleanUpNativeData(ref sqlda);

			_database.ProcessStatusVector(_statusVector);
		}
		else
		{
			if (descriptor.ActualCount == 0)
			{
				descriptor = new Descriptor(0);
			}
		}

		if (sqlda != IntPtr.Zero)
		{
			XsqldaMarshaler.CleanUpNativeData(ref sqlda);
		}

		_parameters = descriptor;
	}

	#endregion
}
