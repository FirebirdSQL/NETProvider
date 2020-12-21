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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;
using FirebirdSql.Data.Logging;

namespace FirebirdSql.Data.FirebirdClient
{
	public sealed class FbCommand : DbCommand, ICloneable
	{
		static readonly IFbLogger Log = FbLogManager.CreateLogger(nameof(FbCommand));

		#region Fields

		private CommandType _commandType;
		private UpdateRowSource _updatedRowSource;
		private FbConnection _connection;
		private FbTransaction _transaction;
		private FbParameterCollection _parameters;
		private StatementBase _statement;
		private FbDataReader _activeReader;
		private List<string> _namedParameters;
		private string _commandText;
		private bool _disposed;
		private bool _designTimeVisible;
		private bool _implicitTransaction;
		private int _commandTimeout;
		private int _fetchSize;
		private Type[] _expectedColumnTypes;

		#endregion

		#region Properties

		[Category("Data")]
		[DefaultValue("")]
		[RefreshProperties(RefreshProperties.All)]
		public override string CommandText
		{
			get { return _commandText; }
			set
			{
				if (_commandText != value && _statement != null)
				{
					Release(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
				}

				_commandText = value;
			}
		}

		[Category("Data")]
		[DefaultValue(CommandType.Text), RefreshProperties(RefreshProperties.All)]
		public override CommandType CommandType
		{
			get { return _commandType; }
			set { _commandType = value; }
		}

		public override int CommandTimeout
		{
			get { return _commandTimeout; }
			set
			{
				if (value < 0)
				{
					throw new ArgumentException("The property value assigned is less than 0.");
				}

				_commandTimeout = value;
			}
		}

		[Category("Behavior")]
		[DefaultValue(null)]
		public new FbConnection Connection
		{
			get { return _connection; }
			set
			{
				if (_activeReader != null)
				{
					throw new InvalidOperationException("There is already an open DataReader associated with this Command which must be closed first.");
				}

				if (_transaction != null && _transaction.IsCompleted)
				{
					_transaction = null;
				}

				if (_connection != null &&
					_connection != value &&
					_connection.State == ConnectionState.Open)
				{
					Release(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
				}

				_connection = value;
			}
		}

		[Category("Data")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public new FbParameterCollection Parameters
		{
			get
			{
				if (_parameters == null)
				{
					_parameters = new FbParameterCollection();
				}
				return _parameters;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new FbTransaction Transaction
		{
			get { return _implicitTransaction ? null : _transaction; }
			set
			{
				if (_activeReader != null)
				{
					throw new InvalidOperationException("There is already an open DataReader associated with this Command which must be closed first.");
				}

				RollbackImplicitTransaction(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();

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

		[Category("Behavior")]
		[DefaultValue(UpdateRowSource.Both)]
		public override UpdateRowSource UpdatedRowSource
		{
			get { return _updatedRowSource; }
			set { _updatedRowSource = value; }
		}

		[Category("Behavior")]
		[DefaultValue(200)]
		public int FetchSize
		{
			get { return _fetchSize; }
			set
			{
				if (_activeReader != null)
				{
					throw new InvalidOperationException("There is already an open DataReader associated with this Command which must be closed first.");
				}
				_fetchSize = value;
			}
		}

		#endregion

		#region Protected DbCommand Properties

		protected override DbConnection DbConnection
		{
			get { return Connection; }
			set { Connection = (FbConnection)value; }
		}

		protected override DbTransaction DbTransaction
		{
			get { return Transaction; }
			set { Transaction = (FbTransaction)value; }
		}

		protected override DbParameterCollection DbParameterCollection
		{
			get { return Parameters; }
		}

		#endregion

		#region Design-Time properties

		[Browsable(false)]
		[DesignOnly(true)]
		[DefaultValue(true)]
		public override bool DesignTimeVisible
		{
			get { return _designTimeVisible; }
			set
			{
				_designTimeVisible = value;
				TypeDescriptor.Refresh(this);
			}
		}

		#endregion

		#region Internal Properties

		internal int RecordsAffected
		{
			get
			{
				if (_statement != null && CommandType != CommandType.StoredProcedure)
				{
					return _statement.RecordsAffected;
				}
				return -1;
			}
		}

		internal bool IsDisposed
		{
			get { return _disposed; }
		}

		internal FbDataReader ActiveReader
		{
			get { return _activeReader; }
			set { _activeReader = value; }
		}

		internal FbTransaction ActiveTransaction
		{
			get { return _transaction; }
		}

		internal bool HasImplicitTransaction
		{
			get { return _implicitTransaction; }
		}

		internal bool HasFields
		{
			get { return _statement?.Fields?.Count > 0; }
		}

		internal bool IsDDLCommand
		{
			get { return _statement?.StatementType == DbStatementType.DDL; }
		}

		internal Type[] ExpectedColumnTypes
		{
			get { return _expectedColumnTypes; }
		}

		#endregion

		#region Constructors

		public FbCommand()
			: this(null, null, null)
		{ }

		public FbCommand(string cmdText)
			: this(cmdText, null, null)
		{ }

		public FbCommand(string cmdText, FbConnection connection)
			: this(cmdText, connection, null)
		{ }

		public FbCommand(string cmdText, FbConnection connection, FbTransaction transaction)
		{
			_namedParameters = new List<string>();
			_updatedRowSource = UpdateRowSource.Both;
			_commandType = CommandType.Text;
			_designTimeVisible = true;
			_commandTimeout = 30;
			_fetchSize = 200;
			_commandText = string.Empty;

			if (connection != null)
			{
				_fetchSize = connection.ConnectionOptions.FetchSize;
			}

			if (cmdText != null)
			{
				CommandText = cmdText;
			}

			Connection = connection;
			_transaction = transaction;
		}

		public static FbCommand CreateWithTypeCoercions(Type[] expectedColumnTypes)
		{
			var result = new FbCommand();
			result._expectedColumnTypes = expectedColumnTypes;
			return result;
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
				await Release(async).ConfigureAwait(false);
				_commandTimeout = 0;
				_fetchSize = 0;
				_implicitTransaction = false;
				_commandText = null;
				_connection = null;
				_transaction = null;
				_parameters = null;
				_statement = null;
				_activeReader = null;
				if (_namedParameters != null)
				{
					_namedParameters.Clear();
					_namedParameters = null;
				}
			}
		}

		#endregion

		#region ICloneable Methods

		object ICloneable.Clone()
		{
			var command = new FbCommand();

			command.CommandText = CommandText;
			command.Connection = Connection;
			command.Transaction = Transaction;
			command.CommandType = CommandType;
			command.UpdatedRowSource = UpdatedRowSource;
			command.CommandTimeout = CommandTimeout;
			command.FetchSize = FetchSize;
			command.UpdatedRowSource = UpdatedRowSource;

			if (_expectedColumnTypes != null)
			{
				command._expectedColumnTypes = (Type[])_expectedColumnTypes.Clone();
			}

			for (var i = 0; i < Parameters.Count; i++)
			{
				command.Parameters.Add(((ICloneable)Parameters[i]).Clone());
			}

			return command;
		}

		#endregion

		#region Methods

		public override void Cancel()
		{
			_connection.CancelCommand();
		}

		public new FbParameter CreateParameter()
		{
			return new FbParameter();
		}

		public override void Prepare() => PrepareImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
#if NET48 || NETSTANDARD2_0
		public Task PrepareAsync(CancellationToken cancellationToken = default)
#else
		public override Task PrepareAsync(CancellationToken cancellationToken = default)
#endif
			=> PrepareImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		internal async Task PrepareImpl(AsyncWrappingCommonArgs async)
		{
			CheckCommand();

			try
			{
				await Prepare(false, async).ConfigureAwait(false);
			}
			catch (IscException ex)
			{
				await RollbackImplicitTransaction(async).ConfigureAwait(false);
				throw new FbException(ex.Message, ex);
			}
			catch
			{
				await RollbackImplicitTransaction(async).ConfigureAwait(false);
				throw;
			}
		}

		public override int ExecuteNonQuery() => ExecuteNonQueryImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken) => ExecuteNonQueryImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		internal async Task<int> ExecuteNonQueryImpl(AsyncWrappingCommonArgs async)
		{
			CheckCommand();

			try
			{
				await ExecuteCommand(CommandBehavior.Default, false, async).ConfigureAwait(false);

				if (_statement.StatementType == DbStatementType.StoredProcedure)
				{
					await SetOutputParameters(async).ConfigureAwait(false);
				}

				await CommitImplicitTransaction(async).ConfigureAwait(false);
			}
			catch (IscException ex)
			{
				await RollbackImplicitTransaction(async).ConfigureAwait(false);
				throw new FbException(ex.Message, ex);
			}
			catch
			{
				await RollbackImplicitTransaction(async).ConfigureAwait(false);
				throw;
			}

			return _statement.StatementType switch
			{
				DbStatementType.Insert => RecordsAffected,
				DbStatementType.Update => RecordsAffected,
				DbStatementType.Delete => RecordsAffected,
				_ => -1,
			};
		}

		public new FbDataReader ExecuteReader() => ExecuteReader(CommandBehavior.Default);
		public new FbDataReader ExecuteReader(CommandBehavior behavior) => ExecuteReaderImpl(behavior, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public new Task<FbDataReader> ExecuteReaderAsync() => ExecuteReaderAsync(CommandBehavior.Default);
		public new Task<FbDataReader> ExecuteReaderAsync(CommandBehavior behavior) => ExecuteReaderAsync(behavior, CancellationToken.None);
		public new Task<FbDataReader> ExecuteReaderAsync(CancellationToken cancellationToken) => ExecuteReaderAsync(CommandBehavior.Default, cancellationToken);
		public new Task<FbDataReader> ExecuteReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken) => ExecuteReaderImpl(behavior, new AsyncWrappingCommonArgs(true, cancellationToken));
		internal async Task<FbDataReader> ExecuteReaderImpl(CommandBehavior behavior, AsyncWrappingCommonArgs async)
		{
			CheckCommand();

			try
			{
				await ExecuteCommand(behavior, true, async).ConfigureAwait(false);
			}
			catch (IscException ex)
			{
				await RollbackImplicitTransaction(async).ConfigureAwait(false);
				throw new FbException(ex.Message, ex);
			}
			catch
			{
				await RollbackImplicitTransaction(async).ConfigureAwait(false);
				throw;
			}

			_activeReader = new FbDataReader(this, _connection, behavior);

			return _activeReader;
		}

		public override object ExecuteScalar() => ExecuteScalarImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public override Task<object> ExecuteScalarAsync(CancellationToken cancellationToken) => ExecuteScalarImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		internal async Task<object> ExecuteScalarImpl(AsyncWrappingCommonArgs async)
		{
			DbValue[] values = null;
			object val = null;

			CheckCommand();

			try
			{
				await ExecuteCommand(CommandBehavior.Default, false, async).ConfigureAwait(false);

				// Gets	only the values	of the first row or
				// the output parameters values if command is an Stored Procedure
				if (_statement.StatementType == DbStatementType.StoredProcedure)
				{
					values = _statement.GetOutputParameters();
					await SetOutputParameters(values, async).ConfigureAwait(false);
				}
				else
				{
					values = await _statement.Fetch(async).ConfigureAwait(false);
				}

				// Get the return value
				if (values != null && values.Length > 0)
				{
					val = await values[0].GetValue(async).ConfigureAwait(false);
				}

				await CommitImplicitTransaction(async).ConfigureAwait(false);
			}
			catch (IscException ex)
			{
				await RollbackImplicitTransaction(async).ConfigureAwait(false);
				throw new FbException(ex.Message, ex);
			}
			catch
			{
				await RollbackImplicitTransaction(async).ConfigureAwait(false);
				throw;
			}

			return val;
		}

		public string GetCommandPlan() => GetCommandPlanImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<string> GetCommandPlanAsync(CancellationToken cancellationToken = default) => GetCommandPlanImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<string> GetCommandPlanImpl(AsyncWrappingCommonArgs async)
		{
			if (_statement == null)
			{
				return Task.FromResult<string>(null);
			}
			return _statement.GetExecutionPlan(async);
		}

		public string GetCommandExplainedPlan() => GetCommandExplainedPlanImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<string> GetCommanExplaineddPlanAsync(CancellationToken cancellationToken = default) => GetCommandExplainedPlanImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<string> GetCommandExplainedPlanImpl(AsyncWrappingCommonArgs async)
		{
			if (_statement == null)
			{
				return Task.FromResult<string>(null);
			}
			return _statement.GetExecutionExplainedPlan(async);
		}

		#endregion

		#region DbCommand Protected Methods

		protected override DbParameter CreateDbParameter()
		{
			return CreateParameter();
		}

		protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
		{
			return ExecuteReader(behavior);
		}

		protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
		{
			return await ExecuteReaderAsync(behavior, cancellationToken).ConfigureAwait(false);
		}

		#endregion

		#region Internal Methods

		internal async Task DisposeReader(AsyncWrappingCommonArgs async)
		{
			if (_activeReader != null)
			{
#if NET48 || NETSTANDARD2_0
				_activeReader.Dispose();
				await Task.CompletedTask.ConfigureAwait(false);
#else
				await async.AsyncSyncCallNoCancellation(_activeReader.DisposeAsync, _activeReader.Dispose).ConfigureAwait(false);
#endif
				_activeReader = null;
			}
		}

		internal async Task<DbValue[]> Fetch(AsyncWrappingCommonArgs async)
		{
			if (_statement != null)
			{
				try
				{
					// Fetch the next row
					return await _statement.Fetch(async).ConfigureAwait(false);
				}
				catch (IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}
			return null;
		}

		internal Descriptor GetFieldsDescriptor()
		{
			if (_statement != null)
			{
				return _statement.Fields;
			}
			return null;
		}

		internal Task SetOutputParameters(AsyncWrappingCommonArgs async)
		{
			return SetOutputParameters(null, async);
		}

		internal async Task SetOutputParameters(DbValue[] outputParameterValues, AsyncWrappingCommonArgs async)
		{
			if (Parameters.Count > 0 && _statement != null)
			{
				if (_statement != null &&
					_statement.StatementType == DbStatementType.StoredProcedure)
				{
					var values = outputParameterValues;
					if (outputParameterValues == null)
					{
						values = _statement.GetOutputParameters();
					}

					if (values != null && values.Length > 0)
					{
						var i = 0;
						foreach (FbParameter parameter in Parameters)
						{
							if (parameter.Direction == ParameterDirection.Output ||
								parameter.Direction == ParameterDirection.InputOutput ||
								parameter.Direction == ParameterDirection.ReturnValue)
							{
								parameter.Value = await values[i].GetValue(async).ConfigureAwait(false);
								i++;
							}
						}
					}
				}
			}
		}

		internal async Task CommitImplicitTransaction(AsyncWrappingCommonArgs async)
		{
			if (HasImplicitTransaction && _transaction != null && _transaction.Transaction != null)
			{
				try
				{
					await _transaction.CommitImpl(async).ConfigureAwait(false);
				}
				catch
				{
					await RollbackImplicitTransaction(async).ConfigureAwait(false);

					throw;
				}
				finally
				{
					if (_transaction != null)
					{
#if NET48 || NETSTANDARD2_0
						_transaction.Dispose();
#else
						await async.AsyncSyncCallNoCancellation(_transaction.DisposeAsync, _transaction.Dispose).ConfigureAwait(false);
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

		internal async Task RollbackImplicitTransaction(AsyncWrappingCommonArgs async)
		{
			if (HasImplicitTransaction && _transaction != null && _transaction.Transaction != null)
			{
				var transactionCount = Connection.InnerConnection.Database.TransactionCount;

				try
				{
					await _transaction.RollbackImpl(async).ConfigureAwait(false);
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
					await async.AsyncSyncCallNoCancellation(_transaction.DisposeAsync, _transaction.Dispose).ConfigureAwait(false);
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

		internal Task Close(AsyncWrappingCommonArgs async)
		{
			if (_statement != null)
			{
				return _statement.Close(async);
			}
			return Task.CompletedTask;
		}

		internal async Task Release(AsyncWrappingCommonArgs async)
		{
			await RollbackImplicitTransaction(async).ConfigureAwait(false);

			await DisposeReader(async).ConfigureAwait(false);

			if (_connection != null && _connection.State == ConnectionState.Open)
			{
				_connection.InnerConnection.RemovePreparedCommand(this);
			}

			if (_statement != null)
			{
				await _statement.Dispose2(async).ConfigureAwait(false);
				_statement = null;
			}
		}

		#endregion

		#region Input parameter descriptor generation methods

		private async Task DescribeInput(AsyncWrappingCommonArgs async)
		{
			if (Parameters.Count > 0)
			{
				var descriptor = BuildParametersDescriptor();
				if (descriptor == null)
				{
					await _statement.DescribeParameters(async).ConfigureAwait(false);
				}
				else
				{
					_statement.Parameters = descriptor;
				}
			}
		}

		private Descriptor BuildParametersDescriptor()
		{
			var count = ValidateInputParameters();

			if (count > 0)
			{
				if (_namedParameters.Count > 0)
				{
					count = (short)_namedParameters.Count;
					return BuildNamedParametersDescriptor(count);
				}
				else
				{
					return BuildPlaceHoldersDescriptor(count);
				}
			}

			return null;
		}

		private Descriptor BuildNamedParametersDescriptor(short count)
		{
			var descriptor = new Descriptor(count);
			var index = 0;

			for (var i = 0; i < _namedParameters.Count; i++)
			{
				var parametersIndex = Parameters.IndexOf(_namedParameters[i], i);
				if (parametersIndex == -1)
				{
					throw new FbException(string.Format("Must declare the variable '{0}'", _namedParameters[i]));
				}

				var parameter = Parameters[parametersIndex];

				if (parameter.Direction == ParameterDirection.Input ||
					parameter.Direction == ParameterDirection.InputOutput)
				{
					if (!BuildParameterDescriptor(descriptor, parameter, index++))
					{
						return null;
					}
				}
			}

			return descriptor;
		}

		private Descriptor BuildPlaceHoldersDescriptor(short count)
		{
			var descriptor = new Descriptor(count);
			var index = 0;

			for (var i = 0; i < Parameters.Count; i++)
			{
				var parameter = Parameters[i];

				if (parameter.Direction == ParameterDirection.Input ||
					parameter.Direction == ParameterDirection.InputOutput)
				{
					if (!BuildParameterDescriptor(descriptor, parameter, index++))
					{
						return null;
					}
				}
			}

			return descriptor;
		}

		private bool BuildParameterDescriptor(Descriptor descriptor, FbParameter parameter, int index)
		{
			if (!parameter.IsTypeSet)
			{
				return false;
			}

			var type = parameter.FbDbType;
			var charset = _connection.InnerConnection.Database.Charset;

			// Check the parameter character set
			if (parameter.Charset == FbCharset.Octets && !(parameter.InternalValue is byte[]))
			{
				throw new InvalidOperationException("Value for char octets fields should be a byte array");
			}
			else if (type == FbDbType.Guid)
			{
				charset = Charset.GetCharset(Charset.Octets);
			}
			else if (parameter.Charset != FbCharset.Default)
			{
				charset = Charset.GetCharset((int)parameter.Charset);
			}

			// Set parameter Data Type
			descriptor[index].DataType = (short)TypeHelper.GetSqlTypeFromDbDataType(TypeHelper.GetDbDataTypeFromFbDbType(type), parameter.IsNullable);

			// Set parameter Sub Type
			switch (type)
			{
				case FbDbType.Binary:
					descriptor[index].SubType = 0;
					break;

				case FbDbType.Text:
					descriptor[index].SubType = 1;
					break;

				case FbDbType.Guid:
					descriptor[index].SubType = (short)charset.Identifier;
					break;

				case FbDbType.Char:
				case FbDbType.VarChar:
					descriptor[index].SubType = (short)charset.Identifier;
					if (charset.IsOctetsCharset)
					{
						descriptor[index].Length = (short)parameter.Size;
					}
					else if (parameter.HasSize)
					{
						var len = (short)(parameter.Size * charset.BytesPerCharacter);
						descriptor[index].Length = len;
					}
					break;
			}

			// Set parameter length
			if (descriptor[index].Length == 0)
			{
				descriptor[index].Length = TypeHelper.GetSize((DbDataType)type) ?? 0;
			}

			// Verify parameter
			if (descriptor[index].SqlType == 0 || descriptor[index].Length == 0)
			{
				return false;
			}

			return true;
		}

		private short ValidateInputParameters()
		{
			short count = 0;

			for (var i = 0; i < Parameters.Count; i++)
			{
				if (Parameters[i].Direction == ParameterDirection.Input ||
					Parameters[i].Direction == ParameterDirection.InputOutput)
				{
					var type = Parameters[i].FbDbType;

					if (type == FbDbType.Array || type == FbDbType.Decimal || type == FbDbType.Numeric)
					{
						return -1;
					}
					else
					{
						count++;
					}
				}
			}

			return count;
		}

		private async Task UpdateParameterValues(AsyncWrappingCommonArgs async)
		{
			var index = -1;

			for (var i = 0; i < _statement.Parameters.Count; i++)
			{
				var statementParameter = _statement.Parameters[i];
				index = i;

				if (_namedParameters.Count > 0)
				{
					index = Parameters.IndexOf(_namedParameters[i], i);
					if (index == -1)
					{
						throw new FbException(string.Format("Must declare the variable '{0}'", _namedParameters[i]));
					}
				}

				if (index != -1)
				{
					var commandParameter = Parameters[index];
					if (commandParameter.InternalValue == DBNull.Value || commandParameter.InternalValue == null)
					{
						statementParameter.NullFlag = -1;
						statementParameter.DbValue.SetValue(DBNull.Value);

						if (!statementParameter.AllowDBNull())
						{
							statementParameter.DataType++;
						}
					}
					else
					{
						statementParameter.NullFlag = 0;

						switch (statementParameter.DbDataType)
						{
							case DbDataType.Binary:
								{
									var blob = _statement.CreateBlob();
									await blob.Write((byte[])commandParameter.InternalValue, async).ConfigureAwait(false);
									statementParameter.DbValue.SetValue(blob.Id);
								}
								break;

							case DbDataType.Text:
								{
									var blob = _statement.CreateBlob();
									if (commandParameter.InternalValue is byte[])
									{
										await blob.Write((byte[])commandParameter.InternalValue, async).ConfigureAwait(false);
									}
									else
									{
										await blob.Write((string)commandParameter.InternalValue, async).ConfigureAwait(false);
									}
									statementParameter.DbValue.SetValue(blob.Id);
								}
								break;

							case DbDataType.Array:
								{
									if (statementParameter.ArrayHandle == null)
									{
										statementParameter.ArrayHandle =
										await _statement.CreateArray(statementParameter.Relation, statementParameter.Name, async).ConfigureAwait(false);
									}
									else
									{
										statementParameter.ArrayHandle.Database = _statement.Database;
										statementParameter.ArrayHandle.Transaction = _statement.Transaction;
									}

									statementParameter.ArrayHandle.Handle = 0;
									await statementParameter.ArrayHandle.Write((Array)commandParameter.InternalValue, async).ConfigureAwait(false);
									statementParameter.DbValue.SetValue(statementParameter.ArrayHandle.Handle);
								}
								break;

							case DbDataType.Guid:
								if (!(commandParameter.InternalValue is Guid) && !(commandParameter.InternalValue is byte[]))
								{
									throw new InvalidOperationException("Incorrect Guid value.");
								}
								statementParameter.DbValue.SetValue(commandParameter.InternalValue);
								break;

							default:
								statementParameter.DbValue.SetValue(commandParameter.InternalValue);
								break;
						}
					}
				}
			}
		}

		#endregion

		#region Private Methods

		private async Task Prepare(bool returnsSet, AsyncWrappingCommonArgs async)
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
					await _transaction.BeginTransaction(async).ConfigureAwait(false);

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
			}

			// Prepare the statement if	needed
			if (!_statement.IsPrepared)
			{
				// Close the inner DataReader if needed
				await DisposeReader(async).ConfigureAwait(false);

				// Reformat the SQL statement if needed
				var sql = _commandText;

				if (_commandType == CommandType.StoredProcedure)
				{
					sql = BuildStoredProcedureSql(sql, returnsSet);
				}

				try
				{
					// Try to prepare the command
					await _statement.Prepare(ParseNamedParameters(sql), async).ConfigureAwait(false);
				}
				catch
				{
					// Release the statement and rethrow the exception
					await _statement.Release(async).ConfigureAwait(false);
					_statement = null;

					throw;
				}

				// Add this	command	to the active command list
				innerConn.AddPreparedCommand(this);
			}
			else
			{
				// Close statement for subsequently	executions
				await Close(async).ConfigureAwait(false);
			}
		}

		private async Task ExecuteCommand(CommandBehavior behavior, bool returnsSet, AsyncWrappingCommonArgs async)
		{
			LogCommandExecutionIfEnabled();

			await Prepare(returnsSet, async).ConfigureAwait(false);

			if ((behavior & CommandBehavior.SequentialAccess) == CommandBehavior.SequentialAccess ||
				(behavior & CommandBehavior.SingleResult) == CommandBehavior.SingleResult ||
				(behavior & CommandBehavior.SingleRow) == CommandBehavior.SingleRow ||
				(behavior & CommandBehavior.CloseConnection) == CommandBehavior.CloseConnection ||
				behavior == CommandBehavior.Default)
			{
				// Set the fetch size
				_statement.FetchSize = _fetchSize;

				// Set if it's needed the Records Affected information
				_statement.ReturnRecordsAffected = _connection.ConnectionOptions.ReturnRecordsAffected;

				// Validate input parameter count
				if (_namedParameters.Count > 0 && Parameters.Count == 0)
				{
					throw new FbException("Must declare command parameters.");
				}

				// Update input parameter values
				if (Parameters.Count > 0)
				{
					if (_statement.Parameters == null)
					{
						await DescribeInput(async).ConfigureAwait(false);
					}
					await UpdateParameterValues(async).ConfigureAwait(false);
				}

				// Execute statement
				await _statement.Execute(async).ConfigureAwait(false);
			}
		}

		private string BuildStoredProcedureSql(string spName, bool returnsSet)
		{
			var sql = spName == null ? string.Empty : spName.Trim();

			if (sql.Length > 0 &&
				!sql.ToUpperInvariant().StartsWith("EXECUTE PROCEDURE ") &&
				!sql.ToUpperInvariant().StartsWith("SELECT "))
			{
				var paramsText = new StringBuilder();

				// Append the stored proc parameter	name
				paramsText.Append(sql);
				if (Parameters.Count > 0)
				{
					paramsText.Append("(");
					for (var i = 0; i < Parameters.Count; i++)
					{
						if (Parameters[i].Direction == ParameterDirection.Input ||
							Parameters[i].Direction == ParameterDirection.InputOutput)
						{
							// Append parameter	name to parameter list
							paramsText.Append(Parameters[i].InternalParameterName);
							if (i != Parameters.Count - 1)
							{
								paramsText = paramsText.Append(",");
							}
						}
					}
					paramsText.Append(")");
					paramsText.Replace(",)", ")");
					paramsText.Replace("()", string.Empty);
				}

				if (returnsSet)
				{
					sql = "select * from " + paramsText.ToString();
				}
				else
				{
					sql = "execute procedure " + paramsText.ToString();
				}
			}

			return sql;
		}

		private string ParseNamedParameters(string sql)
		{
			var builder = new StringBuilder();
			var paramBuilder = new StringBuilder();
			var inSingleQuotes = false;
			var inDoubleQuotes = false;
			var inParam = false;

			_namedParameters.Clear();

			if (sql.IndexOf('@') == -1)
			{
				return sql;
			}

			for (var i = 0; i < sql.Length; i++)
			{
				var sym = sql[i];

				if (inParam)
				{
					if (char.IsLetterOrDigit(sym) || sym == '_' || sym == '$')
					{
						paramBuilder.Append(sym);
					}
					else
					{
						_namedParameters.Add(paramBuilder.ToString());
						paramBuilder.Length = 0;
						builder.Append('?');
						builder.Append(sym);
						inParam = false;
					}
				}
				else
				{
					if (sym == '\'' && !inDoubleQuotes)
					{
						inSingleQuotes = !inSingleQuotes;
					}
					else if (sym == '\"' && !inSingleQuotes)
					{
						inDoubleQuotes = !inDoubleQuotes;
					}
					else if (!(inSingleQuotes || inDoubleQuotes) && sym == '@')
					{
						inParam = true;
						paramBuilder.Append(sym);
						continue;
					}

					builder.Append(sym);
				}
			}

			if (inParam)
			{
				_namedParameters.Add(paramBuilder.ToString());
				builder.Append('?');
			}

			return builder.ToString();
		}

		private void CheckCommand()
		{
			if (_transaction != null && _transaction.IsCompleted)
			{
				_transaction = null;
			}

			FbConnection.EnsureOpen(_connection);

			if (_activeReader != null)
			{
				throw new InvalidOperationException("There is already an open DataReader associated with this Command which must be closed first.");
			}

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

		private void LogCommandExecutionIfEnabled()
		{
			if (Log.IsEnabled(FbLogLevel.Debug))
			{
				var sb = new StringBuilder();
				sb.AppendLine("Executing command:");
				sb.AppendLine(_commandText);
				if (FbLogManager.IsParameterLoggingEnabled)
				{
					sb.AppendLine("Parameters:");
					if (_parameters?.Count > 0)
					{
						foreach (FbParameter item in _parameters)
						{
							sb.AppendLine(string.Format("Name:{0}\tType:{1}\tUsed Value:{2}", item.ParameterName, item.FbDbType, (!IsNullParameterValue(item.InternalValue) ? item.InternalValue : "<null>")));
						}
					}
					else
					{
						sb.AppendLine("<no parameters>");
					}
				}
				Log.Debug(sb.ToString());
			}
		}

		private static bool IsNullParameterValue(object value)
		{
			return (value == DBNull.Value || value == null);
		}

		#endregion
	}
}
