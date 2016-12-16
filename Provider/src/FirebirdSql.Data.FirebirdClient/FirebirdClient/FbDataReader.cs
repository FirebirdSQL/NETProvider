/*
 *  Firebird ADO.NET Data provider for .NET and Mono
 *
 *     The contents of this file are subject to the Initial
 *     Developer's Public License Version 1.0 (the "License");
 *     you may not use this file except in compliance with the
 *     License. You may obtain a copy of the License at
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *     express or implied.  See the License for the specific
 *     language governing rights and limitations under the License.
 *
 *  Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *  Copyright (c) 2008, 2013 Jiri Cincura (jiri@cincura.net)
 *  All Rights Reserved.
 *
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.FirebirdClient
{
	public sealed class FbDataReader : DbDataReader
	{
		#region Constants

		private const int StartPosition = -1;

		#endregion

		#region Fields

#if !NETCORE10
		private DataTable _schemaTable;
#endif
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
		{
		}

		internal FbDataReader(
			FbCommand command,
			FbConnection connection,
			CommandBehavior commandBehavior)
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
			get { return _command.IsSelectCommand; }
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

#if !NETCORE10
		public override void Close()
		{
			Dispose();
		}
#endif

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!IsClosed)
				{
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
					_isClosed = true;
					_position = StartPosition;
					_command = null;
					_connection = null;
					_row = null;
#if !NETCORE10
					_schemaTable = null;
#endif
					_fields = null;
				}
			}
		}

		public override bool Read()
		{
			CheckState();

			bool retValue = false;

			if (IsCommandBehavior(CommandBehavior.SingleRow) && _position != StartPosition)
			{
			}
			else
			{
				if (IsCommandBehavior(CommandBehavior.SchemaOnly))
				{
				}
				else
				{
					_row = _command.Fetch();

					if (_row != null)
					{
						_position++;
						retValue = true;
					}
					else
					{
						_eof = true;
					}
				}
			}

			return retValue;
		}

#if !NETCORE10
		public override DataTable GetSchemaTable()
		{
			CheckState();

			if (_schemaTable != null)
			{
				return _schemaTable;
			}

			DataRow schemaRow = null;
			int tableCount = 0;
			string currentTable = string.Empty;

			_schemaTable = GetSchemaTableStructure();

			/* Prepare statement for schema fields information	*/
			FbCommand schemaCmd = new FbCommand(
				GetSchemaCommandText(),
				_command.Connection,
				_command.Connection.InnerConnection.ActiveTransaction);

			schemaCmd.Parameters.Add("@TABLE_NAME", FbDbType.Char, 31);
			schemaCmd.Parameters.Add("@COLUMN_NAME", FbDbType.Char, 31);
			schemaCmd.Prepare();

			_schemaTable.BeginLoadData();

			for (int i = 0; i < _fields.Count; i++)
			{
				bool isKeyColumn = false;
				bool isUnique = false;
				bool isReadOnly = false;
				int precision = 0;
				bool isExpression = false;

				/* Get Schema data for the field	*/
				schemaCmd.Parameters[0].Value = _fields[i].Relation;
				schemaCmd.Parameters[1].Value = _fields[i].Name;

				using (FbDataReader r = schemaCmd.ExecuteReader())
				{
					if (r.Read())
					{
						isReadOnly = (IsReadOnly(r) || IsExpression(r)) ? true : false;
						isKeyColumn = (r.GetInt32(2) == 1) ? true : false;
						isUnique = (r.GetInt32(3) == 1) ? true : false;
						precision = r.IsDBNull(4) ? -1 : r.GetInt32(4);
						isExpression = IsExpression(r);
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

				_schemaTable.Rows.Add(schemaRow);

				if (!String.IsNullOrEmpty(_fields[i].Relation) && currentTable != _fields[i].Relation)
				{
					tableCount++;
					currentTable = _fields[i].Relation;
				}

				/* Close statement	*/
				schemaCmd.Close();
			}

			if (tableCount > 1)
			{
				foreach (DataRow row in _schemaTable.Rows)
				{
					row["IsKey"] = false;
					row["IsUnique"] = false;
				}
			}

			_schemaTable.EndLoadData();

			/* Dispose command	*/
			schemaCmd.Dispose();

			return _schemaTable;
		}
#endif

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
						return GetBoolean(i);
					}
				}
				if (type == typeof(bool))
				{
					return GetBoolean(i);
				}
			}

			CheckState();
			CheckPosition();
			CheckIndex(i);

			return CheckedGetValue(() => _row[i].Value);
		}

		public override int GetValues(object[] values)
		{
			CheckState();
			CheckPosition();

			for (int i = 0; i < _fields.Count; i++)
			{
				values[i] = CheckedGetValue(() => GetValue(i));
			}

			return values.Length;
		}

		public override bool GetBoolean(int i)
		{
			CheckPosition();
			CheckIndex(i);

			return CheckedGetValue(() => _row[i].GetBoolean());
		}

		public override byte GetByte(int i)
		{
			CheckPosition();
			CheckIndex(i);

			return CheckedGetValue(() => _row[i].GetByte());
		}

		public override long GetBytes(
			int i,
			long dataIndex,
			byte[] buffer,
			int bufferIndex,
			int length)
		{
			CheckPosition();
			CheckIndex(i);

			int bytesRead = 0;
			int realLength = length;

			if (buffer == null)
			{
				if (IsDBNull(i))
				{
					return 0;
				}
				else
				{
					return CheckedGetValue(() => _row[i].GetBinary()).Length;
				}
			}
			else
			{
				byte[] byteArray = CheckedGetValue(() => _row[i].GetBinary());

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

		[EditorBrowsable(EditorBrowsableState.Never)]
		public override char GetChar(int i)
		{
			CheckPosition();
			CheckIndex(i);

			return CheckedGetValue(() => _row[i].GetChar());
		}

		public override long GetChars(
			int i,
			long dataIndex,
			char[] buffer,
			int bufferIndex,
			int length)
		{
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
					return (CheckedGetValue(() => (string)GetValue(i))).ToCharArray().Length;
				}
			}
			else
			{

				char[] charArray = (CheckedGetValue(() => (string)GetValue(i))).ToCharArray();

				int charsRead = 0;
				int realLength = length;

				if (length > (charArray.Length - dataIndex))
				{
					realLength = charArray.Length - (int)dataIndex;
				}

				System.Array.Copy(charArray, (int)dataIndex, buffer,
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
			CheckPosition();
			CheckIndex(i);

			return CheckedGetValue(() => _row[i].GetGuid());
		}

		public override Int16 GetInt16(int i)
		{
			CheckPosition();
			CheckIndex(i);

			return CheckedGetValue(() => _row[i].GetInt16());
		}

		public override Int32 GetInt32(int i)
		{
			CheckPosition();
			CheckIndex(i);

			return CheckedGetValue(() => _row[i].GetInt32());
		}

		public override Int64 GetInt64(int i)
		{
			CheckPosition();
			CheckIndex(i);

			return CheckedGetValue(() => _row[i].GetInt64());
		}

		public override float GetFloat(int i)
		{
			CheckPosition();
			CheckIndex(i);

			return CheckedGetValue(() => _row[i].GetFloat());
		}

		public override double GetDouble(int i)
		{
			CheckPosition();
			CheckIndex(i);

			return CheckedGetValue(() => _row[i].GetDouble());
		}

		public override string GetString(int i)
		{
			CheckPosition();
			CheckIndex(i);

			return CheckedGetValue(() => _row[i].GetString());
		}

		public override Decimal GetDecimal(int i)
		{
			CheckPosition();
			CheckIndex(i);

			return CheckedGetValue(() => _row[i].GetDecimal());
		}

		public override DateTime GetDateTime(int i)
		{
			CheckPosition();
			CheckIndex(i);

			return CheckedGetValue(() => _row[i].GetDateTime());
		}

		public override bool IsDBNull(int i)
		{
			CheckPosition();
			CheckIndex(i);

			return _row[i].IsDBNull();
		}

		public override IEnumerator GetEnumerator()
		{
			return new DbEnumerator(this, IsCommandBehavior(CommandBehavior.CloseConnection));
		}

		public override bool NextResult()
		{
			return false;
		}

		#endregion

		#region Private Methods

		private void CheckPosition()
		{
			if (_eof || _position == StartPosition)
			{
				throw new InvalidOperationException("There are no data to read.");
			}
		}

		private void CheckState()
		{
			if (IsClosed)
			{
				throw new InvalidOperationException("Invalid attempt of read when the reader is closed.");
			}
		}

		private void CheckIndex(int i)
		{
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}
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
			for (int i = 0; i < _fields.Count; i++)
			{
				string fieldName = _fields[i].Alias;
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
			int index;
			if (!_columnsIndexesOrdinal.TryGetValue(name, out index))
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

#if !NETCORE10
		private static DataTable GetSchemaTableStructure()
		{
			DataTable schema = new DataTable("Schema");

			// Schema table structure
			schema.Columns.Add("ColumnName", System.Type.GetType("System.String"));
			schema.Columns.Add("ColumnOrdinal", System.Type.GetType("System.Int32"));
			schema.Columns.Add("ColumnSize", System.Type.GetType("System.Int32"));
			schema.Columns.Add("NumericPrecision", System.Type.GetType("System.Int32"));
			schema.Columns.Add("NumericScale", System.Type.GetType("System.Int32"));
			schema.Columns.Add("DataType", System.Type.GetType("System.Type"));
			schema.Columns.Add("ProviderType", System.Type.GetType("System.Int32"));
			schema.Columns.Add("IsLong", System.Type.GetType("System.Boolean"));
			schema.Columns.Add("AllowDBNull", System.Type.GetType("System.Boolean"));
			schema.Columns.Add("IsReadOnly", System.Type.GetType("System.Boolean"));
			schema.Columns.Add("IsRowVersion", System.Type.GetType("System.Boolean"));
			schema.Columns.Add("IsUnique", System.Type.GetType("System.Boolean"));
			schema.Columns.Add("IsKey", System.Type.GetType("System.Boolean"));
			schema.Columns.Add("IsAutoIncrement", System.Type.GetType("System.Boolean"));
			schema.Columns.Add("IsAliased", System.Type.GetType("System.Boolean"));
			schema.Columns.Add("IsExpression", System.Type.GetType("System.Boolean"));
			schema.Columns.Add("BaseSchemaName", System.Type.GetType("System.String"));
			schema.Columns.Add("BaseCatalogName", System.Type.GetType("System.String"));
			schema.Columns.Add("BaseTableName", System.Type.GetType("System.String"));
			schema.Columns.Add("BaseColumnName", System.Type.GetType("System.String"));

			return schema;
		}

		private static string GetSchemaCommandText()
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
					fld.rdb$field_precision AS numeric_precision
				  FROM rdb$relation_fields rfr
					INNER JOIN rdb$fields fld ON rfr.rdb$field_source = fld.rdb$field_name
				  WHERE rfr.rdb$relation_name = ?
					AND rfr.rdb$field_name = ?
				  ORDER BY rfr.rdb$relation_name, rfr.rdb$field_position";

			return sql;
		}
#endif

		private static T CheckedGetValue<T>(Func<T> f)
		{
			try
			{
				return f();
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		#endregion
	}
}
