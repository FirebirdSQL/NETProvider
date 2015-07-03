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

		private const int STARTPOS = -1;

		#endregion

		#region Fields

		private DataTable schemaTable;
		private FbCommand command;
		private FbConnection connection;
		private DbValue[] row;
		private Descriptor fields;
		private CommandBehavior commandBehavior;
		private bool eof;
		private bool isClosed;
		private int position;
		private int recordsAffected;
		private Dictionary<string, int> columnsIndexesOrdinal;
		private Dictionary<string, int> columnsIndexesOrdinalCI;
		private Dictionary<string, int> columnsIndexesInvariantCI;

		#endregion

		#region DbDataReader Indexers

		public override object this[int i]
		{
			get { return this.GetValue(i); }
		}

		public override object this[string name]
		{
			get { return this.GetValue(this.GetOrdinal(name)); }
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
			this.position = STARTPOS;
			this.command = command;
			this.connection = connection;
			this.commandBehavior = commandBehavior;
			this.fields = this.command.GetFieldsDescriptor();

			this.UpdateRecordsAffected();
		}

		#endregion

		#region Finalizer

		~FbDataReader()
		{
			this.Dispose(false);
		}

		#endregion

		#region IDisposable methods

		//protected override void Dispose(bool disposing)
		//{
		//    if (!this.disposed)
		//    {
		//        try
		//        {
		//            if (disposing)
		//            {
		//                // release any managed resources
		//                this.Close();
		//            }

		//            // release any unmanaged resources
		//        }
		//        finally
		//        {
		//        }

		//        this.disposed = true;
		//    }
		//}

		#endregion

		#region DbDataReader overriden Properties

		public override int Depth
		{
			get
			{
				this.CheckState();

				return 0;
			}
		}

		public override bool HasRows
		{
			get { return this.command.IsSelectCommand; }
		}

		public override bool IsClosed
		{
			get { return this.isClosed; }
		}

		public override int FieldCount
		{
			get
			{
				this.CheckState();

				return this.fields.Count;
			}
		}

		public override int RecordsAffected
		{
			get { return this.recordsAffected; }
		}

		public override int VisibleFieldCount
		{
			get
			{
				this.CheckState();

				return this.fields.Count;
			}
		}

		#endregion

		#region DbDataReader overriden methods

		public override void Close()
		{
			if (!this.IsClosed)
			{
				try
				{
					if (this.command != null && !this.command.IsDisposed)
					{
						if (this.command.CommandType == CommandType.StoredProcedure)
						{
							// Set values of output parameters
							this.command.SetOutputParameters();
						}

						if (this.command.HasImplicitTransaction)
						{
							// Commit implicit transaction if needed
							this.command.CommitImplicitTransaction();
						}

						// Set null the active reader of the command
						this.command.ActiveReader = null;
					}
				}
				catch
				{
				}
				finally
				{
					if (this.connection != null && this.IsCommandBehavior(CommandBehavior.CloseConnection))
					{
						this.connection.Close();
					}

					this.isClosed = true;
					this.position = STARTPOS;
					this.command = null;
					this.connection = null;
					this.row = null;
					this.schemaTable = null;
					this.fields = null;
				}
			}
		}

		public override bool Read()
		{
			this.CheckState();

			bool retValue = false;

			if (this.IsCommandBehavior(CommandBehavior.SingleRow) &&
				this.position != STARTPOS)
			{
			}
			else
			{
				if (this.IsCommandBehavior(CommandBehavior.SchemaOnly))
				{
				}
				else
				{
					this.row = this.command.Fetch();

					if (this.row != null)
					{
						this.position++;
						retValue = true;
					}
					else
					{
						this.eof = true;
					}
				}
			}

			return retValue;
		}

		public override DataTable GetSchemaTable()
		{
			this.CheckState();

			if (this.schemaTable != null)
			{
				return this.schemaTable;
			}

			DataRow schemaRow = null;
			int tableCount = 0;
			string currentTable = string.Empty;

			this.schemaTable = GetSchemaTableStructure();

			/* Prepare statement for schema fields information	*/
			FbCommand schemaCmd = new FbCommand(
				GetSchemaCommandText(),
				this.command.Connection,
				this.command.Connection.InnerConnection.ActiveTransaction);

			schemaCmd.Parameters.Add("@TABLE_NAME", FbDbType.Char, 31);
			schemaCmd.Parameters.Add("@COLUMN_NAME", FbDbType.Char, 31);
			schemaCmd.Prepare();

			schemaTable.BeginLoadData();

			for (int i = 0; i < this.fields.Count; i++)
			{
				bool isKeyColumn = false;
				bool isUnique = false;
				bool isReadOnly = false;
				int precision = 0;
				bool isExpression = false;

				/* Get Schema data for the field	*/
				schemaCmd.Parameters[0].Value = this.fields[i].Relation;
				schemaCmd.Parameters[1].Value = this.fields[i].Name;

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
				schemaRow = schemaTable.NewRow();

				schemaRow["ColumnName"] = this.GetName(i);
				schemaRow["ColumnOrdinal"] = i;
				schemaRow["ColumnSize"] = this.fields[i].GetSize();
				if (fields[i].IsDecimal())
				{
					schemaRow["NumericPrecision"] = schemaRow["ColumnSize"];
					if (precision > 0)
					{
						schemaRow["NumericPrecision"] = precision;
					}
					schemaRow["NumericScale"] = this.fields[i].NumericScale * (-1);
				}
				schemaRow["DataType"] = this.GetFieldType(i);
				schemaRow["ProviderType"] = this.GetProviderType(i);
				schemaRow["IsLong"] = this.fields[i].IsLong();
				schemaRow["AllowDBNull"] = this.fields[i].AllowDBNull();
				schemaRow["IsRowVersion"] = false;
				schemaRow["IsAutoIncrement"] = false;
				schemaRow["IsReadOnly"] = isReadOnly;
				schemaRow["IsKey"] = isKeyColumn;
				schemaRow["IsUnique"] = isUnique;
				schemaRow["IsAliased"] = this.fields[i].IsAliased();
				schemaRow["IsExpression"] = isExpression;
				schemaRow["BaseSchemaName"] = DBNull.Value;
				schemaRow["BaseCatalogName"] = DBNull.Value;
				schemaRow["BaseTableName"] = this.fields[i].Relation;
				schemaRow["BaseColumnName"] = this.fields[i].Name;

				schemaTable.Rows.Add(schemaRow);

				if (!String.IsNullOrEmpty(this.fields[i].Relation) && currentTable != this.fields[i].Relation)
				{
					tableCount++;
					currentTable = this.fields[i].Relation;
				}

				/* Close statement	*/
				schemaCmd.Close();
			}

			if (tableCount > 1)
			{
				foreach (DataRow row in schemaTable.Rows)
				{
					row["IsKey"] = false;
					row["IsUnique"] = false;
				}
			}

			schemaTable.EndLoadData();

			/* Dispose command	*/
			schemaCmd.Dispose();

			return schemaTable;
		}

		public override int GetOrdinal(string name)
		{
			this.CheckState();

			return this.GetColumnIndex(name);
		}

		public override string GetName(int i)
		{
			this.CheckState();
			this.CheckIndex(i);

			if (this.fields[i].Alias.Length > 0)
			{
				return this.fields[i].Alias;
			}
			else
			{
				return this.fields[i].Name;
			}
		}

		public override string GetDataTypeName(int i)
		{
			this.CheckState();
			this.CheckIndex(i);

			return TypeHelper.GetDataTypeName(this.fields[i].DbDataType);
		}

		public override Type GetFieldType(int i)
		{
			this.CheckState();
			this.CheckIndex(i);

			return this.fields[i].GetSystemType();
		}

		public override Type GetProviderSpecificFieldType(int i)
		{
			return this.GetFieldType(i);
		}

		public override object GetProviderSpecificValue(int i)
		{
			return this.GetValue(i);
		}

		public override int GetProviderSpecificValues(object[] values)
		{
			return this.GetValues(values);
		}

		public override object GetValue(int i)
		{
			// type coercions for EF
			if (this.command.ExpectedColumnTypes != null)
			{
				var type = this.command.ExpectedColumnTypes.ElementAtOrDefault(i);
				var nullableUnderlying = Nullable.GetUnderlyingType(type);
				if (nullableUnderlying != null)
				{
					if (this.IsDBNull(i))
					{
						return null;
					}
					if (nullableUnderlying == typeof(bool))
					{
						return this.GetBoolean(i);
					}
				}
				if (type == typeof(bool))
				{
					return this.GetBoolean(i);
				}
			}

			this.CheckState();
			this.CheckPosition();
			this.CheckIndex(i);

			return CheckedGetValue(() => this.row[i].Value);
		}

		public override int GetValues(object[] values)
		{
			this.CheckState();
			this.CheckPosition();

			for (int i = 0; i < this.fields.Count; i++)
			{
				values[i] = CheckedGetValue(() => this.GetValue(i));
			}

			return values.Length;
		}

		public override bool GetBoolean(int i)
		{
			this.CheckPosition();
			this.CheckIndex(i);

			return CheckedGetValue(() => this.row[i].GetBoolean());
		}

		public override byte GetByte(int i)
		{
			this.CheckPosition();
			this.CheckIndex(i);

			return CheckedGetValue(() => this.row[i].GetByte());
		}

		public override long GetBytes(
			int i,
			long dataIndex,
			byte[] buffer,
			int bufferIndex,
			int length)
		{
			this.CheckPosition();
			this.CheckIndex(i);

			int bytesRead = 0;
			int realLength = length;

			if (buffer == null)
			{
				if (this.IsDBNull(i))
				{
					return 0;
				}
				else
				{
					return CheckedGetValue(() => (byte[])row[i].GetBinary()).Length;
				}
			}
			else
			{
				byte[] byteArray = CheckedGetValue(() => (byte[])row[i].GetBinary());

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
			this.CheckPosition();
			this.CheckIndex(i);

			return CheckedGetValue(() => this.row[i].GetChar());
		}

		public override long GetChars(
			int i,
			long dataIndex,
			char[] buffer,
			int bufferIndex,
			int length)
		{
			this.CheckPosition();
			this.CheckIndex(i);

			if (buffer == null)
			{
				if (this.IsDBNull(i))
				{
					return 0;
				}
				else
				{
					return (CheckedGetValue(() => (string)this.GetValue(i))).ToCharArray().Length;
				}
			}
			else
			{

				char[] charArray = (CheckedGetValue(() => (string)this.GetValue(i))).ToCharArray();

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
			this.CheckPosition();
			this.CheckIndex(i);

			return CheckedGetValue(() => this.row[i].GetGuid());
		}

		public override Int16 GetInt16(int i)
		{
			this.CheckPosition();
			this.CheckIndex(i);

			return CheckedGetValue(() => this.row[i].GetInt16());
		}

		public override Int32 GetInt32(int i)
		{
			this.CheckPosition();
			this.CheckIndex(i);

			return CheckedGetValue(() => this.row[i].GetInt32());
		}

		public override Int64 GetInt64(int i)
		{
			this.CheckPosition();
			this.CheckIndex(i);

			return CheckedGetValue(() => this.row[i].GetInt64());
		}

		public override float GetFloat(int i)
		{
			this.CheckPosition();
			this.CheckIndex(i);

			return CheckedGetValue(() => this.row[i].GetFloat());
		}

		public override double GetDouble(int i)
		{
			this.CheckPosition();
			this.CheckIndex(i);

			return CheckedGetValue(() => this.row[i].GetDouble());
		}

		public override string GetString(int i)
		{
			this.CheckPosition();
			this.CheckIndex(i);

			return CheckedGetValue(() => this.row[i].GetString());
		}

		public override Decimal GetDecimal(int i)
		{
			this.CheckPosition();
			this.CheckIndex(i);

			return CheckedGetValue(() => this.row[i].GetDecimal());
		}

		public override DateTime GetDateTime(int i)
		{
			this.CheckPosition();
			this.CheckIndex(i);

			return CheckedGetValue(() => this.row[i].GetDateTime());
		}

		public override bool IsDBNull(int i)
		{
			this.CheckPosition();
			this.CheckIndex(i);

			return this.row[i].IsDBNull();
		}

		public override IEnumerator GetEnumerator()
		{
			return new DbEnumerator(this, this.IsCommandBehavior(CommandBehavior.CloseConnection));
		}

		public override bool NextResult()
		{
			return false;
		}

		#endregion

		#region Private Methods

		private void CheckPosition()
		{
			if (this.eof || this.position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}
		}

		private void CheckState()
		{
			if (this.IsClosed)
			{
				throw new InvalidOperationException("Invalid attempt of read when the reader is closed.");
			}
		}

		private void CheckIndex(int i)
		{
			if (i < 0 || i >= this.FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}
		}

		private FbDbType GetProviderType(int i)
		{
			return (FbDbType)this.fields[i].DbDataType;
		}

		private void UpdateRecordsAffected()
		{
			if (this.command != null && !this.command.IsDisposed)
			{
				if (this.command.RecordsAffected != -1)
				{
					this.recordsAffected = this.recordsAffected == -1 ? 0 : this.recordsAffected;
					this.recordsAffected += this.command.RecordsAffected;
				}
			}
		}

		private bool IsCommandBehavior(CommandBehavior behavior)
		{
			return this.commandBehavior.HasFlag(behavior);
		}

		private void InitializeColumnsIndexes()
		{
			columnsIndexesOrdinal = new Dictionary<string, int>(this.fields.Count, StringComparer.Ordinal);
			columnsIndexesOrdinalCI = new Dictionary<string, int>(this.fields.Count, StringComparer.OrdinalIgnoreCase);
			columnsIndexesInvariantCI = new Dictionary<string, int>(this.fields.Count, StringComparer.InvariantCultureIgnoreCase);
			for (int i = 0; i < this.fields.Count; i++)
			{
				string fieldName = this.fields[i].Alias;
				if (!columnsIndexesOrdinal.ContainsKey(fieldName))
					columnsIndexesOrdinal.Add(fieldName, i);
				if (!columnsIndexesOrdinalCI.ContainsKey(fieldName))
					columnsIndexesOrdinalCI.Add(fieldName, i);
				if (!columnsIndexesInvariantCI.ContainsKey(fieldName))
					columnsIndexesInvariantCI.Add(fieldName, i);
			}
		}

		private int GetColumnIndex(string name)
		{
			if (columnsIndexesOrdinal == null || columnsIndexesOrdinalCI == null || columnsIndexesInvariantCI == null)
			{
				this.InitializeColumnsIndexes();
			}
			int index;
			if (!columnsIndexesOrdinal.TryGetValue(name, out index))
				if (!columnsIndexesOrdinalCI.TryGetValue(name, out index))
					if (!columnsIndexesInvariantCI.TryGetValue(name, out index))
						throw new IndexOutOfRangeException("Could not find specified column in results.");
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
