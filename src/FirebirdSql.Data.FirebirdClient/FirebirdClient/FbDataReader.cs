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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;
using FirebirdSql.Data.Types;

namespace FirebirdSql.Data.FirebirdClient;

public sealed class FbDataReader : DbDataReader
{
	#region Constants

	private const int StartPosition = -1;

	#endregion

	#region Fields

	private DataTable _schemaTable;
	private FbCommand _command;
	private FbConnection _connection;
	private DbValue[] _row;
	private Descriptor _fields;
	private CommandBehavior _commandBehavior;
	private bool _eof;
	private bool _isClosed;
	private int _position;
	private int _recordsAffected;
	private Dictionary<string, int> _columnsIndexesOrdinal;
	private Dictionary<string, int> _columnsIndexesOrdinalCI;

	#endregion

	#region DbDataReader Indexers

	public override object this[int i]
	{
		get { return GetValue(i); }
	}

	public override object this[string name]
	{
		get { return GetValue(GetOrdinal(name)); }
	}

	#endregion

	#region Constructors

	internal FbDataReader()
		: base()
	{ }

	internal FbDataReader(FbCommand command, FbConnection connection, CommandBehavior commandBehavior)
	{
		_position = StartPosition;
		_command = command;
		_connection = connection;
		_commandBehavior = commandBehavior;
		_fields = _command.GetFieldsDescriptor();

		UpdateRecordsAffected();
	}

	#endregion

	#region DbDataReader overriden Properties

	public override int Depth
	{
		get
		{
			CheckState();

			return 0;
		}
	}

	public override bool HasRows
	{
		get { return _command.HasFields; }
	}

	public override bool IsClosed
	{
		get { return _isClosed; }
	}

	public override int FieldCount
	{
		get
		{
			CheckState();

			return _fields.Count;
		}
	}

	public override int RecordsAffected
	{
		get { return _recordsAffected; }
	}

	public override int VisibleFieldCount
	{
		get
		{
			CheckState();

			return _fields.Count;
		}
	}

	#endregion

	#region DbDataReader overriden methods

	public override void Close()
	{
		if (!IsClosed)
		{
			_isClosed = true;
			if (_command != null && !_command.IsDisposed)
			{
				if (_command.CommandType == CommandType.StoredProcedure)
				{
					_command.SetOutputParameters();
				}
				if (_command.HasImplicitTransaction)
				{
					_command.CommitImplicitTransaction();
				}
				_command.ActiveReader = null;
			}
			if (_connection != null && IsCommandBehavior(CommandBehavior.CloseConnection))
			{
				_connection.Close();
			}
			_position = StartPosition;
			_command = null;
			_connection = null;
			_row = null;
			_schemaTable = null;
			_fields = null;
		}
	}
#if NET48 || NETSTANDARD2_0
	public async Task CloseAsync()
#else
	public override async Task CloseAsync()
#endif
	{
		if (!IsClosed)
		{
			_isClosed = true;
			if (_command != null && !_command.IsDisposed)
			{
				if (_command.CommandType == CommandType.StoredProcedure)
				{
					await _command.SetOutputParametersAsync(CancellationToken.None).ConfigureAwait(false);
				}
				if (_command.HasImplicitTransaction)
				{
					await _command.CommitImplicitTransactionAsync(CancellationToken.None).ConfigureAwait(false);
				}
				_command.ActiveReader = null;
			}
			if (_connection != null && IsCommandBehavior(CommandBehavior.CloseConnection))
			{
				await _connection.CloseAsync().ConfigureAwait(false);
			}
			_position = StartPosition;
			_command = null;
			_connection = null;
			_row = null;
			_schemaTable = null;
			_fields = null;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Close();
		}
	}
#if !(NET48 || NETSTANDARD2_0)
	public override async ValueTask DisposeAsync()
	{
		await CloseAsync().ConfigureAwait(false);
		await base.DisposeAsync().ConfigureAwait(false);
	}
#endif

	public override bool Read()
	{
		CheckState();

		if (IsCommandBehavior(CommandBehavior.SchemaOnly))
		{
			return false;
		}
		else if (IsCommandBehavior(CommandBehavior.SingleRow) && _position != StartPosition)
		{
			return false;
		}
		else
		{
			using (var explicitCancellation = ExplicitCancellation.Enter(CancellationToken.None, _command.Cancel))
			{
				_row = _command.Fetch();
				if (_row != null)
				{
					_position++;
					return true;
				}
				else
				{
					_eof = true;
					return false;
				}
			}
		}
	}
	public override async Task<bool> ReadAsync(CancellationToken cancellationToken)
	{
		CheckState();

		if (IsCommandBehavior(CommandBehavior.SchemaOnly))
		{
			return false;
		}
		else if (IsCommandBehavior(CommandBehavior.SingleRow) && _position != StartPosition)
		{
			return false;
		}
		else
		{
			using (var explicitCancellation = ExplicitCancellation.Enter(cancellationToken, _command.Cancel))
			{
				_row = await _command.FetchAsync(explicitCancellation.CancellationToken).ConfigureAwait(false);
				if (_row != null)
				{
					_position++;
					return true;
				}
				else
				{
					_eof = true;
					return false;
				}
			}
		}
	}

	private FbCommand GetSchemaCommand()
	{
		var command = new FbCommand(string.Empty, _command.Connection, _command.Connection.InnerConnection.ActiveTransaction)
		{
			FetchSize = 32767        // Singlerequest all Fields, single Transaction
		};

		// all relations, which used
		foreach (var relation in Enumerable.Range(0, _fields.Count).Where(f => !string.IsNullOrEmpty(_fields[f].Relation)).Select(f => _fields[f].Relation).Distinct())
		{
			if (relation != string.Empty)
			{
				var par = new FbParameter
				{
					DbType = DbType.String,
					Value = relation
				};
				command.Parameters.Add(par);
			}
		}
		if (command.Parameters.Count > 0)
		{
			command.CommandText = string.Format(GetSchemaCommandSingleRequest(), string.Join(", ", Enumerable.Range(0, command.Parameters.Count).Select(p => "?")));
			return command;
		}
		command.Dispose();
		return null;
	}

	private async Task<DataTable> GetFieldsSchemaAsync(CancellationToken cancellationToken = default)
	{
		if (GetSchemaCommand() is FbCommand schemaCommand)
		{
			using (var reader = await schemaCommand.ExecuteReaderAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(false))
			{
				var fieldsSchema = new DataTable();
				fieldsSchema.Columns.AddRange(Enumerable.Range(0, reader.FieldCount).Select(i =>
					 new DataColumn(reader.GetName(i), reader.GetFieldType(i))
					 {
						 AllowDBNull = true
					 }).ToArray());
				var values = new object[reader.FieldCount];
				fieldsSchema.BeginLoadData();
				while (await reader.ReadAsync().ConfigureAwait(false))
				{
					reader.GetValues(values);
					fieldsSchema.Rows.Add(values);
				}
				fieldsSchema.DefaultView.Sort = "base_table, base_field";
				return fieldsSchema;
			}
		}
		return null;
	}

	private DataTable GetFieldsSchema()
	{
		if (GetSchemaCommand() is FbCommand schemaCommand)
		{
			using (schemaCommand)
			{
				using (var reader = schemaCommand.ExecuteReader())
				{
					var fieldsSchema = new DataTable();
					fieldsSchema.Columns.AddRange(Enumerable.Range(0, reader.FieldCount).Select(i =>
						 new DataColumn(reader.GetName(i), reader.GetFieldType(i))
						 {
							 AllowDBNull = true
						 }).ToArray());
					var values = new object[reader.FieldCount];
					fieldsSchema.BeginLoadData();
					while (reader.Read())
					{
						reader.GetValues(values);
						fieldsSchema.Rows.Add(values);
					}
					fieldsSchema.DefaultView.Sort = "base_table, base_field";
					return fieldsSchema;
				}
			}
		}
		return null;
	}

	private DataTable LoadSchemaTable(DataTable fieldsTable)
	{
		DataRow schemaRow = null;

		// no of relations excluding calculated columns with no relation
		var singleTable = Enumerable.Range(0, _fields.Count).Where(f => !string.IsNullOrEmpty(_fields[f].Relation)).Select(f => _fields[f].Relation).Distinct().Count() == 1;

		_schemaTable = GetSchemaTableStructure();

		try
		{
			_schemaTable.BeginLoadData();

			for (var i = 0; i < _fields.Count; i++)
			{
				var isKeyColumn = false;
				var isUnique = false;
				var isReadOnly = false;
				var precision = 0;
				var isExpression = false;

				if (fieldsTable != null)    // relation exists in result
				{
					var rows = fieldsTable.DefaultView.FindRows(new object[] { _fields[i].Relation, _fields[i].Name });
					if (rows.Length > 0)
					{
						isReadOnly = !TypeHelper.IsDBNull(rows[0][0]) || !TypeHelper.IsDBNull(rows[0][1]);
						isKeyColumn = Convert.ToInt32(rows[0][2]) == 1;
						isUnique = Convert.ToInt32(rows[0][3]) == 1;
						precision = TypeHelper.IsDBNull(rows[0][4]) ? -1 : Convert.ToInt32(rows[0][4]);

						// Same as readonly
						isExpression = !TypeHelper.IsDBNull(rows[0][0]) || !TypeHelper.IsDBNull(rows[0][1]);
					}
				}

				/* Create new row for the Schema Table	*/
				schemaRow = _schemaTable.NewRow();

				schemaRow["ColumnName"] = GetName(i);
				schemaRow["ColumnOrdinal"] = i;
				schemaRow["ColumnSize"] = _fields[i].GetSize();
				if (_fields[i].IsDecimal())
				{
					schemaRow["NumericPrecision"] = schemaRow["ColumnSize"];
					if (precision > 0)
					{
						schemaRow["NumericPrecision"] = precision;
					}
					schemaRow["NumericScale"] = _fields[i].NumericScale * (-1);
				}
				schemaRow["DataType"] = GetFieldType(i);
				schemaRow["ProviderType"] = GetProviderType(i);
				schemaRow["IsLong"] = _fields[i].IsLong();
				schemaRow["AllowDBNull"] = _fields[i].AllowDBNull();
				schemaRow["IsRowVersion"] = false;
				schemaRow["IsAutoIncrement"] = false;
				schemaRow["IsReadOnly"] = isReadOnly;
				schemaRow["IsKey"] = isKeyColumn;
				schemaRow["IsUnique"] = isUnique;
				schemaRow["IsAliased"] = _fields[i].IsAliased();
				schemaRow["IsExpression"] = isExpression;
				schemaRow["BaseSchemaName"] = DBNull.Value;
				schemaRow["BaseCatalogName"] = DBNull.Value;
				schemaRow["BaseTableName"] = _fields[i].Relation;
				schemaRow["BaseColumnName"] = _fields[i].Name;

				if (!singleTable)	// more than 1 or calculated columns
				{
					schemaRow["IsKey"] = false;
					schemaRow["IsUnique"] = false;
				}

				_schemaTable.Rows.Add(schemaRow);
			}
			_schemaTable.EndLoadData();
		}
		finally
		{
#if NET48 || NETSTANDARD2_0
			//schemaCmd.Dispose();
#else
			//schemaCmd.Dispose();
#endif
			fieldsTable?.Dispose();
		}

		return _schemaTable;
	}

	public override DataTable GetSchemaTable()
	{
		CheckState();

		if (_schemaTable != null)
		{
			return _schemaTable;
		}

		return LoadSchemaTable(GetFieldsSchema());
	}

#if NET48 || NETSTANDARD2_0 || NETSTANDARD2_1
	public async Task<DataTable> GetSchemaTableAsync(CancellationToken cancellationToken = default)
#else
	public override async Task<DataTable> GetSchemaTableAsync(CancellationToken cancellationToken = default)
#endif
	{
		CheckState();

		if (_schemaTable != null)
		{
			return _schemaTable;
		}

		return LoadSchemaTable(await GetFieldsSchemaAsync(cancellationToken).ConfigureAwait(false));
	}

 	public override int GetOrdinal(string name)
	{
		CheckState();

		return GetColumnIndex(name);
	}

	public override string GetName(int i)
	{
		CheckState();
		CheckIndex(i);

		if (_fields[i].Alias.Length > 0)
		{
			return _fields[i].Alias;
		}
		else
		{
			return _fields[i].Name;
		}
	}

	public override string GetDataTypeName(int i)
	{
		CheckState();
		CheckIndex(i);

		return TypeHelper.GetDataTypeName(_fields[i].DbDataType);
	}

	public override Type GetFieldType(int i)
	{
		CheckState();
		CheckIndex(i);

		return _fields[i].GetSystemType();
	}

	public override Type GetProviderSpecificFieldType(int i)
	{
		return GetFieldType(i);
	}

	public override object GetProviderSpecificValue(int i)
	{
		return GetValue(i);
	}

	public override int GetProviderSpecificValues(object[] values)
	{
		return GetValues(values);
	}

	public override object GetValue(int i)
	{
		// type coercions for EF
		if (_command.ExpectedColumnTypes != null)
		{
			var type = _command.ExpectedColumnTypes.ElementAtOrDefault(i);
			var nullableUnderlying = Nullable.GetUnderlyingType(type);
			if (nullableUnderlying != null)
			{
				if (IsDBNull(i))
				{
					return null;
				}
				if (nullableUnderlying == typeof(bool))
				{
					return GetFieldValue<bool>(i);
				}
			}
			if (type == typeof(bool))
			{
				return GetFieldValue<bool>(i);
			}
		}

		return GetFieldValue<object>(i);
	}

	public override int GetValues(object[] values)
	{
		CheckState();
		CheckPosition();

		var count = Math.Min(_fields.Count, values.Length);
		try
		{
			for (var i = 0; i < count; i++)
			{
				values[i] = _command.ExpectedColumnTypes != null ? GetValue(i) : _row[i].GetValue();
			}
			return count;
		}
		catch (IscException ex)
		{
			throw FbException.Create(ex);
		}
	}

	public override T GetFieldValue<T>(int i)
	{
		CheckState();
		CheckPosition();
		CheckIndex(i);

		var type = typeof(T);
		type = Nullable.GetUnderlyingType(type) ?? type;
		try
		{
			if (type == typeof(bool))
			{
				return (T)(object)_row[i].GetBoolean();
			}
			else if (type == typeof(byte))
			{
				return (T)(object)_row[i].GetByte();
			}
			else if (type == typeof(char))
			{
				return (T)(object)_row[i].GetChar();
			}
			else if (type == typeof(Guid))
			{
				return (T)(object)_row[i].GetGuid();
			}
			else if (type == typeof(short))
			{
				return (T)(object)_row[i].GetInt16();
			}
			else if (type == typeof(int))
			{
				return (T)(object)_row[i].GetInt32();
			}
			else if (type == typeof(long))
			{
				return (T)(object)_row[i].GetInt64();
			}
			else if (type == typeof(float))
			{
				return (T)(object)_row[i].GetFloat();
			}
			else if (type == typeof(double))
			{
				return (T)(object)_row[i].GetDouble();
			}
			else if (type == typeof(string))
			{
				return (T)(object)_row[i].GetString();
			}
			else if (type == typeof(decimal))
			{
				return (T)(object)_row[i].GetDecimal();
			}
			else if (type == typeof(DateTime))
			{
				return (T)(object)_row[i].GetDateTime();
			}
			else if (type == typeof(TimeSpan))
			{
				return (T)(object)_row[i].GetTimeSpan();
			}
			else if (type == typeof(byte[]))
			{
				return (T)(object)_row[i].GetBinary();
			}
			else if (type == typeof(FbDecFloat))
			{
				return (T)(object)_row[i].GetDecFloat();
			}
			else if (type == typeof(BigInteger))
			{
				return (T)(object)_row[i].GetInt128();
			}
			else if (type == typeof(FbZonedDateTime))
			{
				return (T)(object)_row[i].GetZonedDateTime();
			}
			else if (type == typeof(FbZonedTime))
			{
				return (T)(object)_row[i].GetZonedTime();
			}
#if NET6_0_OR_GREATER
			else if (type == typeof(DateOnly))
			{
				return (T)(object)DateOnly.FromDateTime(_row[i].GetDateTime());
			}
#endif
#if NET6_0_OR_GREATER
			else if (type == typeof(TimeOnly))
			{
				return (T)(object)TimeOnly.FromTimeSpan(_row[i].GetTimeSpan());
			}
#endif
			else
			{
				return (T)_row[i].GetValue();
			}
		}
		catch (IscException ex)
		{
			throw FbException.Create(ex);
		}
	}

	public override async Task<T> GetFieldValueAsync<T>(int i, CancellationToken cancellationToken)
	{
		CheckState();
		CheckPosition();
		CheckIndex(i);

		var type = typeof(T);
		type = Nullable.GetUnderlyingType(type) ?? type;
		try
		{
			if (type == typeof(bool))
			{
				return (T)(object)_row[i].GetBoolean();
			}
			else if (type == typeof(byte))
			{
				return (T)(object)_row[i].GetByte();
			}
			else if (type == typeof(char))
			{
				return (T)(object)_row[i].GetChar();
			}
			else if (type == typeof(Guid))
			{
				return (T)(object)_row[i].GetGuid();
			}
			else if (type == typeof(short))
			{
				return (T)(object)_row[i].GetInt16();
			}
			else if (type == typeof(int))
			{
				return (T)(object)_row[i].GetInt32();
			}
			else if (type == typeof(long))
			{
				return (T)(object)_row[i].GetInt64();
			}
			else if (type == typeof(float))
			{
				return (T)(object)_row[i].GetFloat();
			}
			else if (type == typeof(double))
			{
				return (T)(object)_row[i].GetDouble();
			}
			else if (type == typeof(string))
			{
				return (T)(object)await _row[i].GetStringAsync(cancellationToken).ConfigureAwait(false);
			}
			else if (type == typeof(decimal))
			{
				return (T)(object)_row[i].GetDecimal();
			}
			else if (type == typeof(DateTime))
			{
				return (T)(object)_row[i].GetDateTime();
			}
			else if (type == typeof(TimeSpan))
			{
				return (T)(object)_row[i].GetTimeSpan();
			}
			else if (type == typeof(byte[]))
			{
				return (T)(object)await _row[i].GetBinaryAsync().ConfigureAwait(false);
			}
			else if (type == typeof(FbDecFloat))
			{
				return (T)(object)_row[i].GetDecFloat();
			}
			else if (type == typeof(BigInteger))
			{
				return (T)(object)_row[i].GetInt128();
			}
			else if (type == typeof(FbZonedDateTime))
			{
				return (T)(object)_row[i].GetZonedDateTime();
			}
			else if (type == typeof(FbZonedTime))
			{
				return (T)(object)_row[i].GetZonedTime();
			}
#if NET6_0_OR_GREATER
			else if (type == typeof(DateOnly))
			{
				return (T)(object)DateOnly.FromDateTime(_row[i].GetDateTime());
			}
#endif
#if NET6_0_OR_GREATER
			else if (type == typeof(TimeOnly))
			{
				return (T)(object)TimeOnly.FromTimeSpan(_row[i].GetTimeSpan());
			}
#endif
			else
			{
				return (T)await _row[i].GetValueAsync().ConfigureAwait(false);
			}
		}
		catch (IscException ex)
		{
			throw FbException.Create(ex);
		}
	}

	public override bool GetBoolean(int i)
	{
		return GetFieldValue<bool>(i);
	}

	public override byte GetByte(int i)
	{
		return GetFieldValue<byte>(i);
	}

	public override long GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
	{
		CheckState();
		CheckPosition();
		CheckIndex(i);

		var bytesRead = 0;
		var realLength = length;

		if (buffer == null)
		{
			if (IsDBNull(i))
			{
				return 0;
			}
			else
			{
				return GetFieldValue<byte[]>(i).Length;
			}
		}
		else
		{
			var byteArray = GetFieldValue<byte[]>(i);

			if (length > (byteArray.Length - dataIndex))
			{
				realLength = byteArray.Length - (int)dataIndex;
			}

			Array.Copy(byteArray, (int)dataIndex, buffer, bufferIndex, realLength);

			if ((byteArray.Length - dataIndex) < length)
			{
				bytesRead = byteArray.Length - (int)dataIndex;
			}
			else
			{
				bytesRead = length;
			}

			return bytesRead;
		}
	}

	public override char GetChar(int i)
	{
		return GetFieldValue<char>(i);
	}

	public override long GetChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length)
	{
		CheckState();
		CheckPosition();
		CheckIndex(i);

		if (buffer == null)
		{
			if (IsDBNull(i))
			{
				return 0;
			}
			else
			{
				return GetFieldValue<string>(i).ToCharArray().Length;
			}
		}
		else
		{

			var charArray = GetFieldValue<string>(i).ToCharArray();

			var charsRead = 0;
			var realLength = length;

			if (length > (charArray.Length - dataIndex))
			{
				realLength = charArray.Length - (int)dataIndex;
			}

			Array.Copy(charArray, (int)dataIndex, buffer,
				bufferIndex, realLength);

			if ((charArray.Length - dataIndex) < length)
			{
				charsRead = charArray.Length - (int)dataIndex;
			}
			else
			{
				charsRead = length;
			}

			return charsRead;
		}
	}

	public override Guid GetGuid(int i)
	{
		return GetFieldValue<Guid>(i);
	}

	public override short GetInt16(int i)
	{
		return GetFieldValue<short>(i);
	}

	public override int GetInt32(int i)
	{
		return GetFieldValue<int>(i);
	}

	public override long GetInt64(int i)
	{
		return GetFieldValue<long>(i);
	}

	public override float GetFloat(int i)
	{
		return GetFieldValue<float>(i);
	}

	public override double GetDouble(int i)
	{
		return GetFieldValue<double>(i);
	}

	public override string GetString(int i)
	{
		return GetFieldValue<string>(i);
	}

	public override decimal GetDecimal(int i)
	{
		return GetFieldValue<decimal>(i);
	}

	public override DateTime GetDateTime(int i)
	{
		return GetFieldValue<DateTime>(i);
	}

	public override Stream GetStream(int i)
	{
		CheckState();
		CheckPosition();
		CheckIndex(i);

		return _row[i].GetBinaryStream();
	}

	public override bool IsDBNull(int i)
	{
		CheckState();
		CheckPosition();
		CheckIndex(i);

		return _row[i].IsDBNull();
	}
	public override Task<bool> IsDBNullAsync(int i, CancellationToken cancellationToken)
	{
		CheckState();
		CheckPosition();
		CheckIndex(i);

		return Task.FromResult(_row[i].IsDBNull());
	}

	public override IEnumerator GetEnumerator()
	{
		return new DbEnumerator(this, IsCommandBehavior(CommandBehavior.CloseConnection));
	}

	public override bool NextResult()
	{
		return false;
	}
	public override Task<bool> NextResultAsync(CancellationToken cancellationToken)
	{
		return Task.FromResult(false);
	}

	#endregion

	#region Private Methods

	private void CheckPosition()
	{
		if (_eof || _position == StartPosition)
			throw new InvalidOperationException("There are no data to read.");
	}

	private void CheckState()
	{
		if (IsClosed)
			throw new InvalidOperationException("Invalid attempt of read when the reader is closed.");
	}

	private void CheckIndex(int i)
	{
		if (i < 0 || i >= FieldCount)
			throw new IndexOutOfRangeException("Could not find specified column in results.");
	}

	private FbDbType GetProviderType(int i)
	{
		return (FbDbType)_fields[i].DbDataType;
	}

	private void UpdateRecordsAffected()
	{
		if (_command != null && !_command.IsDisposed)
		{
			if (_command.RecordsAffected != -1)
			{
				_recordsAffected = _recordsAffected == -1 ? 0 : _recordsAffected;
				_recordsAffected += _command.RecordsAffected;
			}
		}
	}

	private bool IsCommandBehavior(CommandBehavior behavior)
	{
		return _commandBehavior.HasFlag(behavior);
	}

	private void InitializeColumnsIndexes()
	{
		_columnsIndexesOrdinal = new Dictionary<string, int>(_fields.Count, StringComparer.Ordinal);
		_columnsIndexesOrdinalCI = new Dictionary<string, int>(_fields.Count, StringComparer.OrdinalIgnoreCase);
		for (var i = 0; i < _fields.Count; i++)
		{
			var fieldName = _fields[i].Alias;
			if (!_columnsIndexesOrdinal.ContainsKey(fieldName))
				_columnsIndexesOrdinal.Add(fieldName, i);
			if (!_columnsIndexesOrdinalCI.ContainsKey(fieldName))
				_columnsIndexesOrdinalCI.Add(fieldName, i);
		}
	}

	private int GetColumnIndex(string name)
	{
		if (_columnsIndexesOrdinal == null || _columnsIndexesOrdinalCI == null)
		{
			InitializeColumnsIndexes();
		}
		if (!_columnsIndexesOrdinal.TryGetValue(name, out var index))
			if (!_columnsIndexesOrdinalCI.TryGetValue(name, out index))
				throw new IndexOutOfRangeException($"Could not find specified column '{name}' in results.");
		return index;
	}

	#endregion

	#region Static Methods

	private static bool IsReadOnly(FbDataReader r)
	{
		return IsExpression(r);
	}

	public static bool IsExpression(FbDataReader r)
	{
		/* [0] = COMPUTED_BLR
		 * [1] = COMPUTED_SOURCE
		 */
		if (!r.IsDBNull(0) || !r.IsDBNull(1))
		{
			return true;
		}

		return false;
	}

	private static DataTable GetSchemaTableStructure()
	{
		var schema = new DataTable("Schema");

		// Schema table structure
		schema.Columns.Add("ColumnName", Type.GetType("System.String"));
		schema.Columns.Add("ColumnOrdinal", Type.GetType("System.Int32"));
		schema.Columns.Add("ColumnSize", Type.GetType("System.Int32"));
		schema.Columns.Add("NumericPrecision", Type.GetType("System.Int32"));
		schema.Columns.Add("NumericScale", Type.GetType("System.Int32"));
		schema.Columns.Add("DataType", Type.GetType("System.Type"));
		schema.Columns.Add("ProviderType", Type.GetType("System.Int32"));
		schema.Columns.Add("IsLong", Type.GetType("System.Boolean"));
		schema.Columns.Add("AllowDBNull", Type.GetType("System.Boolean"));
		schema.Columns.Add("IsReadOnly", Type.GetType("System.Boolean"));
		schema.Columns.Add("IsRowVersion", Type.GetType("System.Boolean"));
		schema.Columns.Add("IsUnique", Type.GetType("System.Boolean"));
		schema.Columns.Add("IsKey", Type.GetType("System.Boolean"));
		schema.Columns.Add("IsAutoIncrement", Type.GetType("System.Boolean"));
		schema.Columns.Add("IsAliased", Type.GetType("System.Boolean"));
		schema.Columns.Add("IsExpression", Type.GetType("System.Boolean"));
		schema.Columns.Add("BaseSchemaName", Type.GetType("System.String"));
		schema.Columns.Add("BaseCatalogName", Type.GetType("System.String"));
		schema.Columns.Add("BaseTableName", Type.GetType("System.String"));
		schema.Columns.Add("BaseColumnName", Type.GetType("System.String"));

		return schema;
	}

	private static string GetSchemaCommandSingleRequest()
	{
		const string sql =
			@"SELECT
					fld.rdb$computed_blr AS computed_blr,
					fld.rdb$computed_source AS computed_source,
					(SELECT COUNT(*) FROM rdb$relation_constraints rel
					  INNER JOIN rdb$indices idx ON rel.rdb$index_name = idx.rdb$index_name
					  INNER JOIN rdb$index_segments seg ON idx.rdb$index_name = seg.rdb$index_name
					WHERE rel.rdb$constraint_type = 'PRIMARY KEY'
					  AND rel.rdb$relation_name = rfr.rdb$relation_name
					  AND seg.rdb$field_name = rfr.rdb$field_name) AS primary_key,
					(SELECT COUNT(*) FROM rdb$relation_constraints rel
					  INNER JOIN rdb$indices idx ON rel.rdb$index_name = idx.rdb$index_name
					  INNER JOIN rdb$index_segments seg ON idx.rdb$index_name = seg.rdb$index_name
					WHERE rel.rdb$constraint_type = 'UNIQUE'
					  AND rel.rdb$relation_name = rfr.rdb$relation_name
					  AND seg.rdb$field_name = rfr.rdb$field_name) AS unique_key,
					fld.rdb$field_precision AS numeric_precision,

                    rfr.rdb$relation_name   base_table,
                    rfr.rdb$field_name      base_field

				  FROM rdb$relation_fields rfr
					INNER JOIN rdb$fields fld ON rfr.rdb$field_source = fld.rdb$field_name

					WHERE rfr.rdb$relation_name in ({0})";
		// isn't necessary
		//ORDER BY rfr.rdb$relation_name, rfr.rdb$field_position";

		return sql;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static T CheckedGetValue2<T>(Func<T> getter)
	{
		try
		{
			return getter();
		}
		catch (IscException ex)
		{
			throw FbException.Create(ex);
		}
	}

	#endregion
}
