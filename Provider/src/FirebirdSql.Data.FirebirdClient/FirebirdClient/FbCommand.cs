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
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
#if !NETSTANDARD1_6 && !NETSTANDARD2_0
using System.Runtime.Remoting.Messaging;
#endif

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.FirebirdClient
{
	public sealed class FbCommand : DbCommand
#if !NETSTANDARD1_6
		, ICloneable
#endif
	{
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
					Release();
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

		[Browsable(false)]
		public string CommandPlan
		{
			get
			{
				if (_statement != null)
				{
					return _statement.GetExecutionPlan();
				}
				return null;
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
					Release();
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
		{
		}

		public FbCommand(string cmdText)
			: this(cmdText, null, null)
		{
		}

		public FbCommand(string cmdText, FbConnection connection)
			: this(cmdText, connection, null)
		{
		}

		public FbCommand(string cmdText, FbConnection connection, FbTransaction transaction)
			: base()
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

		#region IDisposable methods

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!_disposed)
				{
					_disposed = true;
					Release();
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
#if NETSTANDARD1_6
				command.Parameters.Add(Parameters[i].Clone());
#else
				command.Parameters.Add(((ICloneable)Parameters[i]).Clone());
#endif
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

		public override void Prepare()
		{
			CheckCommand();

			try
			{
				Prepare(false);
			}
			catch (IscException ex)
			{
				RollbackImplicitTransaction();
				throw new FbException(ex.Message, ex);
			}
			catch
			{
				RollbackImplicitTransaction();
				throw;
			}
		}

		public override int ExecuteNonQuery()
		{
			CheckCommand();

			try
			{
				ExecuteCommand(CommandBehavior.Default);

				if (_statement.StatementType == DbStatementType.StoredProcedure)
				{
					SetOutputParameters();
				}

				CommitImplicitTransaction();
			}
			catch (IscException ex)
			{
				RollbackImplicitTransaction();
				throw new FbException(ex.Message, ex);
			}
			catch
			{
				RollbackImplicitTransaction();
				throw;
			}

			return RecordsAffected;
		}
#if !NETSTANDARD1_6 && !NETSTANDARD2_0
		public IAsyncResult BeginExecuteNonQuery(AsyncCallback callback, object objectState)
		{
			// BeginInvoke might be slow, but the query processing will make this irrelevant
			return ((Func<int>)ExecuteNonQuery).BeginInvoke(callback, objectState);
		}
		public int EndExecuteNonQuery(IAsyncResult asyncResult)
		{
			return ((Func<int>)(asyncResult as AsyncResult).AsyncDelegate).EndInvoke(asyncResult);
		}
#endif

		public new FbDataReader ExecuteReader()
		{
			return ExecuteReader(CommandBehavior.Default);
		}
		public new FbDataReader ExecuteReader(CommandBehavior behavior)
		{
			CheckCommand();

			try
			{
				ExecuteCommand(behavior, true);
			}
			catch (IscException ex)
			{
				RollbackImplicitTransaction();
				throw new FbException(ex.Message, ex);
			}
			catch
			{
				RollbackImplicitTransaction();
				throw;
			}

			_activeReader = new FbDataReader(this, _connection, behavior);

			return _activeReader;
		}
#if !NETSTANDARD1_6 && !NETSTANDARD2_0
		public IAsyncResult BeginExecuteReader(AsyncCallback callback, object objectState)
		{
			// BeginInvoke might be slow, but the query processing will make this irrelevant
			return ((Func<FbDataReader>)ExecuteReader).BeginInvoke(callback, objectState);
		}
		public IAsyncResult BeginExecuteReader(CommandBehavior behavior, AsyncCallback callback, object objectState)
		{
			// BeginInvoke might be slow, but the query processing will make this irrelevant
			return ((Func<CommandBehavior, FbDataReader>)ExecuteReader).BeginInvoke(behavior, callback, objectState);
		}
		public FbDataReader EndExecuteReader(IAsyncResult asyncResult)
		{
			if ((asyncResult as AsyncResult).AsyncDelegate is Func<FbDataReader>)
			{
				return ((Func<FbDataReader>)(asyncResult as AsyncResult).AsyncDelegate).EndInvoke(asyncResult);
			}
			else
			{
				return ((Func<CommandBehavior, FbDataReader>)(asyncResult as AsyncResult).AsyncDelegate).EndInvoke(asyncResult);
			}
		}
#endif

		public override object ExecuteScalar()
		{
			DbValue[] values = null;
			object val = null;

			CheckCommand();

			try
			{
				ExecuteCommand(CommandBehavior.Default);

				// Gets	only the values	of the first row or
				// the output parameters values if command is an Stored Procedure
				if (_statement.StatementType == DbStatementType.StoredProcedure)
				{
					values = _statement.GetOutputParameters();
					SetOutputParameters(values);
				}
				else
				{
					values = _statement.Fetch();
				}

				// Get the return value
				if (values != null && values.Length > 0)
				{
					val = values[0].Value;
				}

				CommitImplicitTransaction();
			}
			catch (IscException ex)
			{
				RollbackImplicitTransaction();
				throw new FbException(ex.Message, ex);
			}
			catch
			{
				RollbackImplicitTransaction();
				throw;
			}

			return val;
		}
#if !NETSTANDARD1_6 && !NETSTANDARD2_0
		public IAsyncResult BeginExecuteScalar(AsyncCallback callback, object objectState)
		{
			// BeginInvoke might be slow, but the query processing will make this irrelevant
			return ((Func<object>)ExecuteScalar).BeginInvoke(callback, objectState);
		}
		public object EndExecuteScalar(IAsyncResult asyncResult)
		{
			return ((Func<object>)(asyncResult as AsyncResult).AsyncDelegate).EndInvoke(asyncResult);
		}
#endif

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

		#endregion

		#region Internal Methods

		internal void DisposeReader()
		{
			if (_activeReader != null)
			{
				_activeReader.Dispose();
				_activeReader = null;
			}
		}

		internal DbValue[] Fetch()
		{
			try
			{
				if (_statement != null)
				{
					// Fetch the next row
					return _statement.Fetch();
				}
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
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

		internal void SetOutputParameters()
		{
			SetOutputParameters(null);
		}

		internal void SetOutputParameters(DbValue[] outputParameterValues)
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
								parameter.Value = values[i].Value;
								i++;
							}
						}
					}
				}
			}
		}

		internal void CommitImplicitTransaction()
		{
			if (HasImplicitTransaction &&
				_transaction != null &&
				_transaction.Transaction != null)
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
						_transaction.Dispose();
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
					_transaction.Dispose();
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

		internal void Release()
		{
			RollbackImplicitTransaction();

			DisposeReader();

			if (_connection != null && _connection.State == ConnectionState.Open)
			{
				_connection.InnerConnection.RemovePreparedCommand(this);
			}

			if (_statement != null)
			{
				_statement.Dispose();
				_statement = null;
			}
		}

		#endregion

		#region Input parameter descriptor generation methods

		private void DescribeInput()
		{
			if (Parameters.Count > 0)
			{
				var descriptor = BuildParametersDescriptor();
				if (descriptor == null)
				{
					_statement.DescribeParameters();
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

		private void UpdateParameterValues()
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
						statementParameter.Value = DBNull.Value;

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
									blob.Write((byte[])commandParameter.InternalValue);
									statementParameter.Value = blob.Id;
								}
								break;

							case DbDataType.Text:
								{
									var blob = _statement.CreateBlob();
									if (commandParameter.InternalValue is byte[])
										blob.Write((byte[])commandParameter.InternalValue);
									else
										blob.Write((string)commandParameter.InternalValue);
									statementParameter.Value = blob.Id;
								}
								break;

							case DbDataType.Array:
								{
									if (statementParameter.ArrayHandle == null)
									{
										statementParameter.ArrayHandle =
										_statement.CreateArray(
											statementParameter.Relation,
											statementParameter.Name);
									}
									else
									{
										statementParameter.ArrayHandle.Database = _statement.Database;
										statementParameter.ArrayHandle.Transaction = _statement.Transaction;
									}

									statementParameter.ArrayHandle.Handle = 0;
									statementParameter.ArrayHandle.Write((System.Array)commandParameter.InternalValue);
									statementParameter.Value = statementParameter.ArrayHandle.Handle;
								}
								break;

							case DbDataType.Guid:
								if (!(commandParameter.InternalValue is Guid) && !(commandParameter.InternalValue is byte[]))
								{
									throw new InvalidOperationException("Incorrect Guid value.");
								}
								statementParameter.Value = commandParameter.InternalValue;
								break;

							default:
								statementParameter.Value = commandParameter.InternalValue;
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
			LogCommand();

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
			}

			// Prepare the statement if	needed
			if (!_statement.IsPrepared)
			{
				// Close the inner DataReader if needed
				DisposeReader();

				// Reformat the SQL statement if needed
				var sql = _commandText;

				if (_commandType == CommandType.StoredProcedure)
				{
					sql = BuildStoredProcedureSql(sql, returnsSet);
				}

				try
				{
					// Try to prepare the command
					_statement.Prepare(ParseNamedParameters(sql));
				}
				catch
				{
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

		private void ExecuteCommand(CommandBehavior behavior)
		{
			ExecuteCommand(behavior, false);
		}

		private void ExecuteCommand(CommandBehavior behavior, bool returnsSet)
		{
			// Prepare statement
			Prepare(returnsSet);

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
						DescribeInput();
					}
					UpdateParameterValues();
				}

				// Execute statement
				_statement.Execute();
			}
		}

		private string BuildStoredProcedureSql(string spName, bool returnsSet)
		{
			var sql = spName == null ? string.Empty : spName.Trim();

			if (sql.Length > 0 &&
				!sql.ToLowerInvariant().StartsWith("execute procedure ") &&
				!sql.ToLowerInvariant().StartsWith("select "))
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
					if (Char.IsLetterOrDigit(sym) || sym == '_' || sym == '$')
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

		[Conditional(TraceHelper.ConditionalSymbol)]
		private void LogCommand()
		{
			if (TraceHelper.HasListeners)
			{
				var message = new StringBuilder();
				message.AppendLine("Command:");
				message.AppendLine(_commandText);
				message.AppendLine("Parameters:");
				if (_parameters != null)
				{
					foreach (FbParameter item in _parameters)
					{
						message.AppendLine(string.Format("Name:{0}\tType:{1}\tUsed Value:{2}", item.ParameterName, item.FbDbType, (!IsNullParameterValue(item.InternalValue) ? item.InternalValue : "<null>")));
					}
				}
				else
				{
					message.AppendLine("<no parameters>");
				}
				TraceHelper.Trace(TraceEventType.Information, message.ToString());
			}
		}

		private bool IsNullParameterValue(object value)
		{
			return (value == DBNull.Value || value == null);
		}

		#endregion
	}
}
