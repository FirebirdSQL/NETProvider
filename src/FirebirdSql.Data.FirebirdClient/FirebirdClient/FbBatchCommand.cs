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

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;
using FirebirdSql.Data.Logging;

namespace FirebirdSql.Data.FirebirdClient;

public sealed class FbBatchCommand : IFbPreparedCommand, IDescriptorFiller, IDisposable
#if !(NET48 || NETSTANDARD2_0)
		, IAsyncDisposable
#endif
{
	static readonly IFbLogger Log = FbLogManager.CreateLogger(nameof(FbBatchCommand));

	private const int DefaultBatchBufferSize = 16 * 1024 * 1024;

	#region Fields

	private FbConnection _connection;
	private FbTransaction _transaction;
	private FbBatchParameterCollection _batchParameters;
	private StatementBase _statement;
	private BatchBase _batch;
	//private FbDataReader _activeReader;
	private IReadOnlyList<string> _namedParameters;
	private string _commandText;
	private bool _disposed;
	private bool _implicitTransaction;
	//private int? _commandTimeout;
	//private int _fetchSize;
	private bool _multiError;
	private int _batchBufferSize;

	#endregion

	#region Properties

	public string CommandText
	{
		get { return _commandText; }
		set
		{
			if (_commandText != value && _statement != null)
			{
				Release();
			}

			_commandText = value;
		}
	}

	//[Category("Behavior")]
	//[DefaultValue(ConnectionString.DefaultValueCommandTimeout)]
	//public int CommandTimeout
	//{
	//	get
	//	{
	//		if (_commandTimeout != null)
	//			return (int)_commandTimeout;
	//		if (_connection?.CommandTimeout >= 0)
	//			return (int)_connection?.CommandTimeout;
	//		return ConnectionString.DefaultValueCommandTimeout;
	//	}
	//	set
	//	{
	//		if (value < 0)
	//		{
	//			throw new ArgumentException("The property value assigned is less than 0.");
	//		}

	//		_commandTimeout = value;
	//	}
	//}

	public FbConnection Connection
	{
		get { return _connection; }
		set
		{
			//if (_activeReader != null)
			//{
			//	throw new InvalidOperationException("There is already an open DataReader associated with this Command which must be closed first.");
			//}

			if (_transaction != null && _transaction.IsCompleted)
			{
				_transaction = null;
			}

			if (_connection != null &&
				_connection != value &&
				_connection.State == ConnectionState.Open)
			{
				Release();
			}

			_connection = value;
		}
	}

	public FbBatchParameterCollection BatchParameters
	{
		get
		{
			if (_batchParameters == null)
			{
				_batchParameters = new FbBatchParameterCollection();
			}
			return _batchParameters;
		}
	}

	public FbTransaction Transaction
	{
		get { return _implicitTransaction ? null : _transaction; }
		set
		{
			//if (_activeReader != null)
			//{
			//	throw new InvalidOperationException("There is already an open DataReader associated with this Command which must be closed first.");
			//}

			RollbackImplicitTransaction();

			_transaction = value;

			if (_statement != null)
			{
				if (_transaction != null)
				{
					_statement.Transaction = _transaction.Transaction;
				}
				else
				{
					_statement.Transaction = null;
				}
			}
		}
	}

	//public int FetchSize
	//{
	//	get { return _fetchSize; }
	//	set
	//	{
	//		if (_activeReader != null)
	//		{
	//			throw new InvalidOperationException("There is already an open DataReader associated with this Command which must be closed first.");
	//		}
	//		_fetchSize = value;
	//	}
	//}

	public bool MultiError
	{
		get { return _multiError; }
		set { _multiError = value; }
	}

	public int BatchBufferSize
	{
		get { return _batchBufferSize; }
		set
		{
			if (!SizeHelper.IsValidBatchBufferSize(value))
				throw SizeHelper.InvalidSizeException("batch buffer");

			_batchBufferSize = value;
		}
	}

	/// <summary>
	/// Gets collection of parameters parsed from the query text during command prepare.
	/// </summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public IReadOnlyList<string> NamedParameters
	{
		get { return _namedParameters; }
	}

	#endregion

	#region Internal Properties

	internal bool IsDisposed
	{
		get { return _disposed; }
	}

	//internal FbDataReader ActiveReader
	//{
	//	get { return _activeReader; }
	//	set { _activeReader = value; }
	//}

	internal FbTransaction ActiveTransaction
	{
		get { return _transaction; }
	}

	internal bool HasImplicitTransaction
	{
		get { return _implicitTransaction; }
	}

	//internal bool HasFields
	//{
	//	get { return _statement?.Fields?.Count > 0; }
	//}

	internal bool HasParameters
	{
		get { return _batchParameters != null && _batchParameters.Count > 0 && _batchParameters[0].Count > 0; }
	}

	#endregion

	#region Constructors

	public FbBatchCommand()
		: this(null, null, null)
	{ }

	public FbBatchCommand(string cmdText)
		: this(cmdText, null, null)
	{ }

	public FbBatchCommand(string cmdText, FbConnection connection)
		: this(cmdText, connection, null)
	{ }

	public FbBatchCommand(string cmdText, FbConnection connection, FbTransaction transaction)
	{
		_namedParameters = Array.Empty<string>();
		//_commandTimeout = null;
		//_fetchSize = 200;
		_multiError = false;
		_batchBufferSize = DefaultBatchBufferSize;
		_commandText = string.Empty;

		if (connection != null)
		{
			//_fetchSize = connection.ConnectionOptions.FetchSize;
		}

		if (cmdText != null)
		{
			CommandText = cmdText;
		}

		Connection = connection;
		_transaction = transaction;
	}

	#endregion

	#region IDisposable, IAsyncDisposable methods

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			try
			{
				Release();
			}
			catch (IscException ex)
			{
				throw FbException.Create(ex);
			}
			_batchBufferSize = DefaultBatchBufferSize;
			_multiError = false;
			//_commandTimeout = null;
			//_fetchSize = 0;
			_implicitTransaction = false;
			_commandText = null;
			_connection = null;
			_transaction = null;
			_batchParameters = null;
			_batch = null;
			_statement = null;
			//_activeReader = null;
			_namedParameters = null;
		}
	}
#if !(NET48 || NETSTANDARD2_0)
	public async ValueTask DisposeAsync()
	{
		if (!_disposed)
		{
			_disposed = true;
			try
			{
				await ReleaseAsync().ConfigureAwait(false);
			}
			catch (IscException ex)
			{
				throw FbException.Create(ex);
			}
			_batchBufferSize = DefaultBatchBufferSize;
			_multiError = false;
			//_commandTimeout = 0;
			//_fetchSize = 0;
			_implicitTransaction = false;
			_commandText = null;
			_connection = null;
			_transaction = null;
			_batchParameters = null;
			_batch = null;
			_statement = null;
			//_activeReader = null;
			_namedParameters = null;
		}
	}
#endif

	#endregion

	#region Methods

	public void Cancel()
	{
		_connection.CancelCommand();
	}

	public FbParameter CreateParameter()
	{
		return new FbParameter();
	}

	public FbParameterCollection AddBatchParameters()
	{
		var result = new FbParameterCollection();
		BatchParameters.Add(result);
		return result;
	}

	public void Prepare()
	{
		CheckCommand();

		using (var explicitCancellation = ExplicitCancellation.Enter(CancellationToken.None, Cancel))
		{
			try
			{
				Prepare(false);
			}
			catch (IscException ex)
			{
				RollbackImplicitTransaction();
				throw FbException.Create(ex);
			}
			catch
			{
				RollbackImplicitTransaction();
				throw;
			}
		}
	}
	public async Task PrepareAsync(CancellationToken cancellationToken = default)
	{
		CheckCommand();

		using (var explicitCancellation = ExplicitCancellation.Enter(cancellationToken, Cancel))
		{
			try
			{
				await PrepareAsync(false, explicitCancellation.CancellationToken).ConfigureAwait(false);
			}
			catch (IscException ex)
			{
				await RollbackImplicitTransactionAsync(explicitCancellation.CancellationToken).ConfigureAwait(false);
				throw FbException.Create(ex);
			}
			catch
			{
				await RollbackImplicitTransactionAsync(explicitCancellation.CancellationToken).ConfigureAwait(false);
				throw;
			}
		}
	}

	public FbBatchNonQueryResult ExecuteNonQuery()
	{
		CheckCommand();

		FbBatchNonQueryResult result;

		using (var explicitCancellation = ExplicitCancellation.Enter(CancellationToken.None, Cancel))
		{
			try
			{
				result = ExecuteCommand(false);

				//if (_statement.StatementType == DbStatementType.StoredProcedure)
				//{
				//	SetOutputParameters();
				//}

				CommitImplicitTransaction();
			}
			catch (IscException ex)
			{
				RollbackImplicitTransaction();
				throw FbException.Create(ex);
			}
			catch
			{
				RollbackImplicitTransaction();
				throw;
			}
		}

		return result;
	}
	public async Task<FbBatchNonQueryResult> ExecuteNonQueryAsync(CancellationToken cancellationToken = default)
	{
		CheckCommand();

		FbBatchNonQueryResult result;

		using (var explicitCancellation = ExplicitCancellation.Enter(cancellationToken, Cancel))
		{
			try
			{
				result = await ExecuteCommandAsync(false, explicitCancellation.CancellationToken).ConfigureAwait(false);

				//if (_statement.StatementType == DbStatementType.StoredProcedure)
				//{
				//	await SetOutputParametersAsync(explicitCancellation.CancellationToken).ConfigureAwait(false);
				//}

				await CommitImplicitTransactionAsync(explicitCancellation.CancellationToken).ConfigureAwait(false);
			}
			catch (IscException ex)
			{
				await RollbackImplicitTransactionAsync(explicitCancellation.CancellationToken).ConfigureAwait(false);
				throw FbException.Create(ex);
			}
			catch
			{
				await RollbackImplicitTransactionAsync(explicitCancellation.CancellationToken).ConfigureAwait(false);
				throw;
			}
		}

		return result;
	}

	//public new FbDataReader ExecuteReader() => ExecuteReader(CommandBehavior.Default);
	//public new Task<FbDataReader> ExecuteReaderAsync() => ExecuteReaderAsync(CommandBehavior.Default);
	//public new Task<FbDataReader> ExecuteReaderAsync(CancellationToken cancellationToken) => ExecuteReaderAsync(CommandBehavior.Default, cancellationToken);

	//public new FbDataReader ExecuteReader(CommandBehavior behavior)
	//{
	//	CheckCommand();

	//	using (var explicitCancellation = ExplicitCancellation.Enter(CancellationToken.None, Cancel))
	//	{
	//		try
	//		{
	//			ExecuteCommand(behavior, true);
	//		}
	//		catch (IscException ex)
	//		{
	//			RollbackImplicitTransaction();
	//			throw FbException.Create(ex);
	//		}
	//		catch
	//		{
	//			RollbackImplicitTransaction();
	//			throw;
	//		}
	//	}

	//	_activeReader = new FbDataReader(this, _connection, behavior);
	//	return _activeReader;
	//}
	//public new Task<FbDataReader> ExecuteReaderAsync(CommandBehavior behavior) => ExecuteReaderAsync(behavior, CancellationToken.None);
	//public new async Task<FbDataReader> ExecuteReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
	//{
	//	CheckCommand();

	//	using (var explicitCancellation = ExplicitCancellation.Enter(cancellationToken, Cancel))
	//	{
	//		try
	//		{
	//			await ExecuteCommandAsync(behavior, true, explicitCancellation.CancellationToken).ConfigureAwait(false);
	//		}
	//		catch (IscException ex)
	//		{
	//			await RollbackImplicitTransactionAsync(explicitCancellation.CancellationToken).ConfigureAwait(false);
	//			throw FbException.Create(ex);
	//		}
	//		catch
	//		{
	//			await RollbackImplicitTransactionAsync(explicitCancellation.CancellationToken).ConfigureAwait(false);
	//			throw;
	//		}
	//	}

	//	_activeReader = new FbDataReader(this, _connection, behavior);
	//	return _activeReader;
	//}

	public string GetCommandPlan()
	{
		if (_statement == null)
		{
			return null;
		}
		return _statement.GetExecutionPlan();
	}
	public Task<string> GetCommandPlanAsync(CancellationToken cancellationToken = default)
	{
		if (_statement == null)
		{
			return Task.FromResult<string>(null);
		}
		return _statement.GetExecutionPlanAsync(cancellationToken).AsTask();
	}

	public string GetCommandExplainedPlan()
	{
		if (_statement == null)
		{
			return null;
		}
		return _statement.GetExecutionExplainedPlan();
	}
	public Task<string> GetCommandExplainedPlanAsync(CancellationToken cancellationToken = default)
	{
		if (_statement == null)
		{
			return Task.FromResult<string>(null);
		}
		return _statement.GetExecutionExplainedPlanAsync(cancellationToken).AsTask();
	}

	public int ComputeCurrentBatchSize()
	{
		if (_batch == null)
		{
			throw new InvalidOperationException("Batch must be prepared.");
		}
		if (!HasParameters)
		{
			return 0;
		}
		return _batch.ComputeBatchSize(_batchParameters.Count, this);
	}
	public async Task<int> ComputeCurrentBatchSizeAsync(CancellationToken cancellationToken = default)
	{
		if (_batch == null)
		{
			throw new InvalidOperationException("Batch must be prepared.");
		}
		if (!HasParameters)
		{
			return 0;
		}
		return await _batch.ComputeBatchSizeAsync(_batchParameters.Count, this, cancellationToken).ConfigureAwait(false);
	}

	#endregion

	#region Internal Methods

	//		internal void DisposeReader()
	//		{
	//			if (_activeReader != null)
	//			{
	//#if NET48 || NETSTANDARD2_0
	//				_activeReader.Dispose();
	//#else
	//				_activeReader.Dispose();
	//#endif
	//				_activeReader = null;
	//			}
	//		}
	//		internal async Task DisposeReaderAsync(CancellationToken cancellationToken = default)
	//		{
	//			if (_activeReader != null)
	//			{
	//#if NET48 || NETSTANDARD2_0
	//				_activeReader.Dispose();
	//				await Task.CompletedTask.ConfigureAwait(false);
	//#else
	//				await _activeReader.DisposeAsync().ConfigureAwait(false);
	//#endif
	//				_activeReader = null;
	//			}
	//		}

	//internal DbValue[] Fetch()
	//{
	//	if (_statement != null)
	//	{
	//		try
	//		{
	//			return _statement.Fetch();
	//		}
	//		catch (IscException ex)
	//		{
	//			throw FbException.Create(ex);
	//		}
	//	}
	//	return null;
	//}
	//internal async Task<DbValue[]> FetchAsync(CancellationToken cancellationToken = default)
	//{
	//	if (_statement != null)
	//	{
	//		try
	//		{
	//			return await _statement.FetchAsync(cancellationToken).ConfigureAwait(false);
	//		}
	//		catch (IscException ex)
	//		{
	//			throw FbException.Create(ex);
	//		}
	//	}
	//	return null;
	//}

	//internal Descriptor GetFieldsDescriptor()
	//{
	//	if (_statement != null)
	//	{
	//		return _statement.Fields;
	//	}
	//	return null;
	//}

	//internal void SetOutputParameters()
	//{
	//	SetOutputParameters(null);
	//}
	//internal Task SetOutputParametersAsync(CancellationToken cancellationToken = default)
	//{
	//	return SetOutputParametersAsync(null, cancellationToken);
	//}

	//internal void SetOutputParameters(DbValue[] outputParameterValues)
	//{
	//	if (Parameters.Count > 0 && _statement != null)
	//	{
	//		if (_statement != null &&
	//			_statement.StatementType == DbStatementType.StoredProcedure)
	//		{
	//			var values = outputParameterValues;
	//			if (outputParameterValues == null)
	//			{
	//				values = _statement.GetOutputParameters();
	//			}

	//			if (values != null && values.Length > 0)
	//			{
	//				var i = 0;
	//				foreach (FbParameter parameter in Parameters)
	//				{
	//					if (parameter.Direction == ParameterDirection.Output ||
	//						parameter.Direction == ParameterDirection.InputOutput ||
	//						parameter.Direction == ParameterDirection.ReturnValue)
	//					{
	//						parameter.Value = values[i].GetValue();
	//						i++;
	//					}
	//				}
	//			}
	//		}
	//	}
	//}
	//internal async Task SetOutputParametersAsync(DbValue[] outputParameterValues, CancellationToken cancellationToken = default)
	//{
	//	if (Parameters.Count > 0 && _statement != null)
	//	{
	//		if (_statement != null &&
	//			_statement.StatementType == DbStatementType.StoredProcedure)
	//		{
	//			var values = outputParameterValues;
	//			if (outputParameterValues == null)
	//			{
	//				values = _statement.GetOutputParameters();
	//			}

	//			if (values != null && values.Length > 0)
	//			{
	//				var i = 0;
	//				foreach (FbParameter parameter in Parameters)
	//				{
	//					if (parameter.Direction == ParameterDirection.Output ||
	//						parameter.Direction == ParameterDirection.InputOutput ||
	//						parameter.Direction == ParameterDirection.ReturnValue)
	//					{
	//						parameter.Value = await values[i].GetValueAsync(cancellationToken).ConfigureAwait(false);
	//						i++;
	//					}
	//				}
	//			}
	//		}
	//	}
	//}

	internal void CommitImplicitTransaction()
	{
		if (HasImplicitTransaction && _transaction != null && _transaction.Transaction != null)
		{
			try
			{
				_transaction.Commit();
			}
			catch
			{
				RollbackImplicitTransaction();

				throw;
			}
			finally
			{
				if (_transaction != null)
				{
#if NET48 || NETSTANDARD2_0
					_transaction.Dispose();
#else
					_transaction.Dispose();
#endif
					_transaction = null;
					_implicitTransaction = false;
				}

				if (_statement != null)
				{
					_statement.Transaction = null;
				}
			}
		}
	}
	internal async Task CommitImplicitTransactionAsync(CancellationToken cancellationToken = default)
	{
		if (HasImplicitTransaction && _transaction != null && _transaction.Transaction != null)
		{
			try
			{
				await _transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
			}
			catch
			{
				await RollbackImplicitTransactionAsync(cancellationToken).ConfigureAwait(false);

				throw;
			}
			finally
			{
				if (_transaction != null)
				{
#if NET48 || NETSTANDARD2_0
					_transaction.Dispose();
#else
					await _transaction.DisposeAsync().ConfigureAwait(false);
#endif
					_transaction = null;
					_implicitTransaction = false;
				}

				if (_statement != null)
				{
					_statement.Transaction = null;
				}
			}
		}
	}

	internal void RollbackImplicitTransaction()
	{
		if (HasImplicitTransaction && _transaction != null && _transaction.Transaction != null)
		{
			var transactionCount = Connection.InnerConnection.Database.TransactionCount;

			try
			{
				_transaction.Rollback();
			}
			catch
			{
				if (Connection.InnerConnection.Database.TransactionCount == transactionCount)
				{
					Connection.InnerConnection.Database.TransactionCount--;
				}
			}
			finally
			{
#if NET48 || NETSTANDARD2_0
				_transaction.Dispose();
#else
				_transaction.Dispose();
#endif
				_transaction = null;
				_implicitTransaction = false;

				if (_statement != null)
				{
					_statement.Transaction = null;
				}
			}
		}
	}
	internal async Task RollbackImplicitTransactionAsync(CancellationToken cancellationToken = default)
	{
		if (HasImplicitTransaction && _transaction != null && _transaction.Transaction != null)
		{
			var transactionCount = Connection.InnerConnection.Database.TransactionCount;

			try
			{
				await _transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
			}
			catch
			{
				if (Connection.InnerConnection.Database.TransactionCount == transactionCount)
				{
					Connection.InnerConnection.Database.TransactionCount--;
				}
			}
			finally
			{
#if NET48 || NETSTANDARD2_0
				_transaction.Dispose();
#else
				await _transaction.DisposeAsync().ConfigureAwait(false);
#endif
				_transaction = null;
				_implicitTransaction = false;

				if (_statement != null)
				{
					_statement.Transaction = null;
				}
			}
		}
	}

	internal void Close()
	{
		if (_statement != null)
		{
			_statement.Close();
		}
	}
	internal Task CloseAsync(CancellationToken cancellationToken = default)
	{
		if (_statement != null)
		{
			return _statement.CloseAsync(cancellationToken).AsTask();
		}
		return Task.CompletedTask;
	}

	void IFbPreparedCommand.Release() => Release();
	internal void Release()
	{
		RollbackImplicitTransaction();

		//DisposeReader();

		if (_connection != null && _connection.State == ConnectionState.Open)
		{
			_connection.InnerConnection.RemovePreparedCommand(this);
		}

		_batch = null;

		if (_statement != null)
		{
			_statement.Dispose2();
			_statement = null;
		}
	}
	Task IFbPreparedCommand.ReleaseAsync(CancellationToken cancellationToken) => ReleaseAsync(cancellationToken);
	internal async Task ReleaseAsync(CancellationToken cancellationToken = default)
	{
		await RollbackImplicitTransactionAsync(cancellationToken).ConfigureAwait(false);

		//await DisposeReaderAsync(cancellationToken).ConfigureAwait(false);

		if (_connection != null && _connection.State == ConnectionState.Open)
		{
			_connection.InnerConnection.RemovePreparedCommand(this);
		}

		_batch = null;

		if (_statement != null)
		{
			await _statement.Dispose2Async(cancellationToken).ConfigureAwait(false);
			_statement = null;
		}
	}

	void IFbPreparedCommand.TransactionCompleted() => TransactionCompleted();
	internal void TransactionCompleted()
	{
		if (Transaction != null)
		{
			//DisposeReader();
			Transaction = null;
		}
	}
	Task IFbPreparedCommand.TransactionCompletedAsync(CancellationToken cancellationToken) => TransactionCompletedAsync(cancellationToken);
	internal async Task TransactionCompletedAsync(CancellationToken cancellationToken = default)
	{
		if (Transaction != null)
		{
			//await DisposeReaderAsync(cancellationToken).ConfigureAwait(false);
			await Task.CompletedTask.ConfigureAwait(false);
			Transaction = null;
		}
	}

	#endregion

	#region IDescriptorFiller

	void IDescriptorFiller.Fill(Descriptor descriptor, int index) => UpdateParameterValues(descriptor, index);
	private void UpdateParameterValues(Descriptor descriptor, int batchIndex)
	{
		if (!HasParameters)
			return;

		for (var i = 0; i < descriptor.Count; i++)
		{
			var parameter = descriptor[i];
			var index = i;

			if (_namedParameters.Count > 0)
			{
				index = _batchParameters[batchIndex].IndexOf(_namedParameters[i], i);
				if (index == -1)
				{
					throw FbException.Create($"Must declare the variable '{_namedParameters[i]}'.");
				}
			}

			if (index != -1)
			{
				var commandParameter = _batchParameters[batchIndex][index];
				if (commandParameter.InternalValue == DBNull.Value || commandParameter.InternalValue == null)
				{
					parameter.NullFlag = -1;
					parameter.DbValue.SetValue(DBNull.Value);

					if (!parameter.AllowDBNull())
					{
						parameter.DataType++;
					}
				}
				else
				{
					parameter.NullFlag = 0;

					switch (parameter.DbDataType)
					{
						case DbDataType.Binary:
							{
								var blob = _statement.CreateBlob();
								blob.Write((byte[])commandParameter.InternalValue);
								parameter.DbValue.SetValue(blob.Id);
							}
							break;

						case DbDataType.Text:
							{
								var blob = _statement.CreateBlob();
								if (commandParameter.InternalValue is byte[])
								{
									blob.Write((byte[])commandParameter.InternalValue);
								}
								else
								{
									blob.Write((string)commandParameter.InternalValue);
								}
								parameter.DbValue.SetValue(blob.Id);
							}
							break;

						case DbDataType.Array:
							{
								if (parameter.ArrayHandle == null)
								{
									parameter.ArrayHandle = _statement.CreateArray(parameter.Relation, parameter.Name);
								}
								else
								{
									parameter.ArrayHandle.Database = _statement.Database;
									parameter.ArrayHandle.Transaction = _statement.Transaction;
								}

								parameter.ArrayHandle.Handle = 0;
								parameter.ArrayHandle.Write((Array)commandParameter.InternalValue);
								parameter.DbValue.SetValue(parameter.ArrayHandle.Handle);
							}
							break;

						case DbDataType.Guid:
							if (!(commandParameter.InternalValue is Guid) && !(commandParameter.InternalValue is byte[]))
							{
								throw new InvalidOperationException("Incorrect Guid value.");
							}
							parameter.DbValue.SetValue(commandParameter.InternalValue);
							break;

						default:
							parameter.DbValue.SetValue(commandParameter.InternalValue);
							break;
					}
				}
			}
		}
	}
	ValueTask IDescriptorFiller.FillAsync(Descriptor descriptor, int index, CancellationToken cancellationToken) => UpdateParameterValuesAsync(descriptor, index, cancellationToken);
	private async ValueTask UpdateParameterValuesAsync(Descriptor descriptor, int batchIndex, CancellationToken cancellationToken = default)
	{
		if (!HasParameters)
			return;

		for (var i = 0; i < descriptor.Count; i++)
		{
			var batchParameter = descriptor[i];
			var index = i;

			if (_namedParameters.Count > 0)
			{
				index = _batchParameters[batchIndex].IndexOf(_namedParameters[i], i);
				if (index == -1)
				{
					throw FbException.Create($"Must declare the variable '{_namedParameters[i]}'.");
				}
			}

			if (index != -1)
			{
				var commandParameter = _batchParameters[batchIndex][index];
				if (commandParameter.InternalValue == DBNull.Value || commandParameter.InternalValue == null)
				{
					batchParameter.NullFlag = -1;
					batchParameter.DbValue.SetValue(DBNull.Value);

					if (!batchParameter.AllowDBNull())
					{
						batchParameter.DataType++;
					}
				}
				else
				{
					batchParameter.NullFlag = 0;

					switch (batchParameter.DbDataType)
					{
						case DbDataType.Binary:
							{
								var blob = _statement.CreateBlob();
								await blob.WriteAsync((byte[])commandParameter.InternalValue, cancellationToken).ConfigureAwait(false);
								batchParameter.DbValue.SetValue(blob.Id);
							}
							break;

						case DbDataType.Text:
							{
								var blob = _statement.CreateBlob();
								if (commandParameter.InternalValue is byte[])
								{
									await blob.WriteAsync((byte[])commandParameter.InternalValue, cancellationToken).ConfigureAwait(false);
								}
								else
								{
									await blob.WriteAsync((string)commandParameter.InternalValue, cancellationToken).ConfigureAwait(false);
								}
								batchParameter.DbValue.SetValue(blob.Id);
							}
							break;

						case DbDataType.Array:
							{
								if (batchParameter.ArrayHandle == null)
								{
									batchParameter.ArrayHandle =
									await _statement.CreateArrayAsync(batchParameter.Relation, batchParameter.Name, cancellationToken).ConfigureAwait(false);
								}
								else
								{
									batchParameter.ArrayHandle.Database = _statement.Database;
									batchParameter.ArrayHandle.Transaction = _statement.Transaction;
								}

								batchParameter.ArrayHandle.Handle = 0;
								await batchParameter.ArrayHandle.WriteAsync((Array)commandParameter.InternalValue, cancellationToken).ConfigureAwait(false);
								batchParameter.DbValue.SetValue(batchParameter.ArrayHandle.Handle);
							}
							break;

						case DbDataType.Guid:
							if (!(commandParameter.InternalValue is Guid) && !(commandParameter.InternalValue is byte[]))
							{
								throw new InvalidOperationException("Incorrect Guid value.");
							}
							batchParameter.DbValue.SetValue(commandParameter.InternalValue);
							break;

						default:
							batchParameter.DbValue.SetValue(commandParameter.InternalValue);
							break;
					}
				}
			}
		}
	}

	#endregion

	#region Private Methods

	private void Prepare(bool returnsSet)
	{
		var innerConn = _connection.InnerConnection;

		// Check if	we have	a valid	transaction
		if (_transaction == null)
		{
			if (innerConn.IsEnlisted)
			{
				_transaction = innerConn.ActiveTransaction;
			}
			else
			{
				_implicitTransaction = true;
				_transaction = new FbTransaction(_connection, _connection.ConnectionOptions.IsolationLevel);
				_transaction.BeginTransaction();

				// Update Statement	transaction
				if (_statement != null)
				{
					_statement.Transaction = _transaction.Transaction;
				}
			}
		}

		// Check if	we have	a valid	statement handle
		if (_statement == null)
		{
			_statement = innerConn.Database.CreateStatement(_transaction.Transaction);
			_batch = _statement.CreateBatch();
		}

		// Prepare the statement if	needed
		if (!_statement.IsPrepared)
		{
			// Close the inner DataReader if needed
			//DisposeReader();

			// Reformat the SQL statement if needed
			var sql = _commandText;

			try
			{
				(sql, _namedParameters) = NamedParametersParser.Parse(sql);
				// Try to prepare the command
				_statement.Prepare(sql);
			}
			catch
			{
				_batch = null;
				// Release the statement and rethrow the exception
				_statement.Release();
				_statement = null;

				throw;
			}

			// Add this	command	to the active command list
			innerConn.AddPreparedCommand(this);
		}
		else
		{
			// Close statement for subsequently	executions
			Close();
		}
	}
	private async Task PrepareAsync(bool returnsSet, CancellationToken cancellationToken = default)
	{
		var innerConn = _connection.InnerConnection;

		// Check if	we have	a valid	transaction
		if (_transaction == null)
		{
			if (innerConn.IsEnlisted)
			{
				_transaction = innerConn.ActiveTransaction;
			}
			else
			{
				_implicitTransaction = true;
				_transaction = new FbTransaction(_connection, _connection.ConnectionOptions.IsolationLevel);
				await _transaction.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

				// Update Statement	transaction
				if (_statement != null)
				{
					_statement.Transaction = _transaction.Transaction;
				}
			}
		}

		// Check if	we have	a valid	statement handle
		if (_statement == null)
		{
			_statement = innerConn.Database.CreateStatement(_transaction.Transaction);
			_batch = _statement.CreateBatch();
		}

		// Prepare the statement if	needed
		if (!_statement.IsPrepared)
		{
			// Close the inner DataReader if needed
			//await DisposeReaderAsync(cancellationToken).ConfigureAwait(false);

			// Reformat the SQL statement if needed
			var sql = _commandText;

			try
			{
				(sql, _namedParameters) = NamedParametersParser.Parse(sql);
				// Try to prepare the command
				await _statement.PrepareAsync(sql, cancellationToken).ConfigureAwait(false);
			}
			catch
			{
				_batch = null;
				// Release the statement and rethrow the exception
				await _statement.ReleaseAsync(cancellationToken).ConfigureAwait(false);
				_statement = null;

				throw;
			}

			// Add this	command	to the active command list
			innerConn.AddPreparedCommand(this);
		}
		else
		{
			// Close statement for subsequently	executions
			await CloseAsync(cancellationToken).ConfigureAwait(false);
		}
	}

	private FbBatchNonQueryResult ExecuteCommand(bool returnsSet)
	{
		LogMessages.CommandExecution(Log, this);

		Prepare(returnsSet);

		// Set the fetch size
		//_statement.FetchSize = _fetchSize;

		// Set if it's needed the Records Affected information
		_statement.ReturnRecordsAffected = _connection.ConnectionOptions.ReturnRecordsAffected;

		// Validate input parameter count
		if (_namedParameters.Count > 0 && !HasParameters)
		{
			throw FbException.Create("Must declare command parameters.");
		}

		try
		{
			_batch.MultiError = MultiError;
			_batch.BatchBufferSize = BatchBufferSize;
			// Execute
			return new FbBatchNonQueryResult(_batch.Execute(_batchParameters.Count, this));
		}
		finally
		{
			_batch.Release();
		}
	}
	private async Task<FbBatchNonQueryResult> ExecuteCommandAsync(bool returnsSet, CancellationToken cancellationToken = default)
	{
		LogMessages.CommandExecution(Log, this);

		await PrepareAsync(returnsSet, cancellationToken).ConfigureAwait(false);

		// Set the fetch size
		//_statement.FetchSize = _fetchSize;

		// Set if it's needed the Records Affected information
		_statement.ReturnRecordsAffected = _connection.ConnectionOptions.ReturnRecordsAffected;

		// Validate input parameter count
		if (_namedParameters.Count > 0 && !HasParameters)
		{
			throw FbException.Create("Must declare command parameters.");
		}

		try
		{
			_batch.MultiError = MultiError;
			_batch.BatchBufferSize = BatchBufferSize;
			// Execute
			return new FbBatchNonQueryResult(await _batch.ExecuteAsync(_batchParameters.Count, this, cancellationToken).ConfigureAwait(false));
		}
		finally
		{
			await _batch.ReleaseAsync(cancellationToken).ConfigureAwait(false);
		}
	}

	private void CheckCommand()
	{
		if (_transaction != null && _transaction.IsCompleted)
		{
			_transaction = null;
		}

		FbConnection.EnsureOpen(_connection);

		//if (_activeReader != null)
		//{
		//	throw new InvalidOperationException("There is already an open DataReader associated with this Command which must be closed first.");
		//}

		if (_transaction == null &&
			_connection.InnerConnection.HasActiveTransaction &&
			!_connection.InnerConnection.IsEnlisted)
		{
			throw new InvalidOperationException("Execute requires the Command object to have a Transaction object when the Connection object assigned to the command is in a pending local transaction. The Transaction property of the Command has not been initialized.");
		}

		if (_transaction != null && !_transaction.IsCompleted &&
			!_connection.Equals(_transaction.Connection))
		{
			throw new InvalidOperationException("Command Connection is not equal to Transaction Connection.");
		}

		if (_commandText == null || _commandText.Length == 0)
		{
			throw new InvalidOperationException("The command text for this Command has not been set.");
		}
	}

	#endregion
}
