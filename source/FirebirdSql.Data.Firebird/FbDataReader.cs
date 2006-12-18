/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.ibphoenix.com/main.nfs?a=ibphoenix&l=;PAGES;NAME='ibp_idpl'
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Data;
using System.Data.Common;
using System.Collections;
using System.ComponentModel;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Firebird
{	
	/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/overview/*'/>
	public sealed class FbDataReader : MarshalByRefObject, IDataReader, IEnumerable, IDisposable, IDataRecord
	{		
		#region Fields
		
		private const int		STARTPOS = -1;
		private bool			disposed;
		private bool			isClosed;
        private bool            eof;
        private int				position;
		private int				recordsAffected;
		private DataTable		schemaTable;
		private FbCommand		command;
		private FbConnection	connection;
		private DbValue[]		row;
		private RowDescriptor	fields;
		private	CommandBehavior	behavior;

		#endregion

		#region Constructors

		internal FbDataReader(
			FbCommand		command, 
			FbConnection	connection,
			CommandBehavior	behavior)
		{
			this.isClosed		 = false;
			this.position		 = STARTPOS;
			this.recordsAffected = -1;
			this.command		 = command;
			this.behavior		 = behavior;
			this.connection		 = connection;
			this.fields          = this.command.GetFieldsDescriptor();
		}

		#endregion

		#region Finalizer

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/destructor[@name="Finalize"]/*'/>
		~FbDataReader()
		{
			this.Dispose(false);
		}

		#endregion

		#region IDisposable Methods

		void IDisposable.Dispose() 
		{
			this.Dispose(true);
			System.GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				try
				{
					if (disposing)
					{
						// release any managed resources
						this.Close();
					}

					// release any unmanaged resources
				}
				finally
				{
				}
							
				this.disposed = true;
			}
		}

		#endregion

		#region IDataReader Properties & Methods

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/property[@name="Depth"]/*'/>
		public int Depth 
		{
			get 
			{ 
				this.checkState();

				return 0; 
			}
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/property[@name="IsClosed"]/*'/>
		public bool IsClosed
		{
			get { return this.isClosed; }
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/property[@name="RecordsAffected"]/*'/>
		public int RecordsAffected 
		{
			get { return this.IsClosed ? this.recordsAffected : -1; }
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/property[@name="HasRows"]/*'/>
		public bool HasRows
		{
			get { return true; }
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="Close"]/*'/>
		public void Close()
		{
			bool closeConnection = false;

			if (this.isClosed)
			{
				return;
			}

			if (this.position == STARTPOS &&
				this.command.CommandType == CommandType.StoredProcedure)
			{
				// Read the first row, this is needed when using stored procedures
				// for update/delete/insert operations with the dataadapter
				// because DbDataAdapter.Update makes calls to FbCommand.ExecuteReader
				this.Read();
			}

			this.isClosed	= true;
			this.eof		= false;
			this.position	= STARTPOS;

			this.updateRecordsAffected();

			if (this.connection != null)
			{
				if ((this.behavior & CommandBehavior.CloseConnection) == CommandBehavior.CloseConnection)
				{
					closeConnection = true;
				}
			}

			if (this.command != null && !this.command.IsDisposed)
			{
				// Set values of output parameters
				this.command.InternalSetOutputParameters();

				// Commit implicit transaction if needed
				this.command.CommitImplicitTransaction();

                this.command.ActiveReader = null;
            }

			if (closeConnection)
			{
				this.connection.Close();
			}

			this.command		= null;
			this.connection		= null;
			this.row			= null;
			this.schemaTable	= null;
			this.fields			= null;
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="NextResult"]/*'/>
		public bool NextResult()
		{
			if (this.IsClosed)
			{
				throw new InvalidOperationException("The datareader must be opened.");
			}

			this.updateRecordsAffected();

			bool returnValue = this.command.NextResult(this.behavior);

			if (returnValue)
			{
				this.fields		= this.command.GetFieldsDescriptor();
				this.position	= STARTPOS;
				this.eof		= false;
			}
			else
			{
				this.row = null;
			}

			return returnValue;
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="Read"]/*'/>
		public bool Read()
		{
			this.checkState();

			bool retValue = false;

			if ((this.behavior & CommandBehavior.SingleRow) == CommandBehavior.SingleRow &&
				this.position != STARTPOS)
			{
			}
			else
			{
				if ((this.behavior & CommandBehavior.SchemaOnly) == CommandBehavior.SchemaOnly)
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

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetSchemaTable"]/*'/>
		public DataTable GetSchemaTable()
		{
			this.checkState();

			if (this.schemaTable != null)
			{
				return this.schemaTable;
			}

			DataRow	schemaRow;

			this.schemaTable = this.getSchemaTableStructure();

			/* Prepare statement for schema fields information	*/
			FbCommand schemaCmd = new FbCommand(
				this.getSchemaCommandText(),
				this.command.Connection,
				this.command.ActiveTransaction);

			schemaCmd.Parameters.Add("@TABLE_NAME", FbDbType.Char, 31);
			schemaCmd.Parameters.Add("@COLUMN_NAME", FbDbType.Char, 31);
			schemaCmd.InternalPrepare();
			
			schemaTable.BeginLoadData();			
			for (int i = 0; i < this.fields.Count; i++)
			{
				bool isKeyColumn	= false;
				bool isUnique		= false;
				bool isReadOnly		= false;
				int  precision      = -100;

                if (!this.fields[i].IsExpression())
                {
					/* Get Schema data for the field	*/
					schemaCmd.Parameters[0].Value = this.fields[i].Relation;
					schemaCmd.Parameters[1].Value = this.fields[i].Name;
					
					schemaCmd.InternalExecute();

					// Fetch data
					DbValue[] values = schemaCmd.Fetch();

					if (values != null)
					{
						isReadOnly	= (this.isReadOnly(values) || fields[i].IsExpression()) ? true : false;
						isKeyColumn	= (Convert.ToInt32(values[2].Value) == 1) ? true : false;
						isUnique	= (Convert.ToInt32(values[3].Value) == 1) ? true : false;
						precision	= values[4].IsDBNull() ? -100 : Convert.ToInt32(values[4].Value);
					}
				}

				/* Create new row for the Schema Table	*/
				schemaRow = schemaTable.NewRow();

				schemaRow["ColumnName"]		= this.GetName(i);
				schemaRow["ColumnOrdinal"]	= i;
				schemaRow["ColumnSize"]		= this.fields[i].GetSize();
				if (fields[i].IsDecimal())
				{
                    schemaRow["NumericPrecision"] = schemaRow["ColumnSize"];
                    if (precision != -100)
                    {
                        schemaRow["NumericPrecision"] = precision;
                    }
                    schemaRow["NumericScale"]	 = this.fields[i].NumericScale*(-1);
				}
				schemaRow["DataType"]		= this.GetFieldType(i);
				schemaRow["ProviderType"]	= this.getProviderType(i);
				schemaRow["IsLong"]			= this.fields[i].IsLong();
				schemaRow["AllowDBNull"]	= this.fields[i].AllowDBNull();
				schemaRow["IsRowVersion"]	= false;
				schemaRow["IsAutoIncrement"]= false;
				schemaRow["IsReadOnly"]		= isReadOnly;
				schemaRow["IsKey"]			= isKeyColumn;
				schemaRow["IsUnique"]		= isUnique;
				schemaRow["IsAliased"]		= this.fields[i].IsAliased();
				schemaRow["IsExpression"]	= this.fields[i].IsExpression();
				schemaRow["BaseSchemaName"]	= DBNull.Value;
				schemaRow["BaseCatalogName"]= DBNull.Value;
				schemaRow["BaseTableName"]	= this.fields[i].Relation;
				schemaRow["BaseColumnName"]	= this.fields[i].Name;

				schemaTable.Rows.Add(schemaRow);

				/* Close statement	*/
				schemaCmd.Close();				
			}
			schemaTable.EndLoadData();

			/* Dispose command	*/
			schemaCmd.Dispose();

			return schemaTable;
		}

		#endregion

		#region IDataRecord Properties & Methods

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/indexer[@name="Item(System.Int32)"]/*'/>
		public object this[int i]
		{
			get { return this.GetValue(i); }
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/indexer[@name="Item(System.String)"]/*'/>
		public object this[String name]
		{			
			get { return this.GetValue(this.GetOrdinal(name)); }
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/property[@name="FieldCount"]/*'/>
		public int FieldCount
		{			
			get 
			{
				this.checkState();

				return this.fields.Count;
			}
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetOrdinal(System.String)"]/*'/>
		public int GetOrdinal(string name)
		{
			this.checkState();

			for (int i = 0; i < this.fields.Count; i++)
			{
				if (GlobalizationHelper.CultureAwareCompare(name, this.fields[i].Alias))
				{
					return i;
				}
			}
						
			throw new IndexOutOfRangeException("Could not find specified column in results.");
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetName(System.Int32)"]/*'/>
		public string GetName(int i)
		{
			this.checkState();
			this.checkIndex(i);

			if (this.fields[i].Alias.Length > 0)
			{
				return this.fields[i].Alias;
			}
			else
			{
				return this.fields[i].Name;
			}
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetDataTypeName(System.Int32)"]/*'/>
		public String GetDataTypeName(int i)
		{
			this.checkState();
			this.checkIndex(i);

			return TypeHelper.GetDataTypeName(this.fields[i].DbDataType);
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetFieldType(System.Int32)"]/*'/>
		public Type GetFieldType(int i)
		{
			this.checkState();
			this.checkIndex(i);

			return this.fields[i].GetSystemType();
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetValue(System.Int32)"]/*'/>
		public object GetValue(int i)
		{
			this.checkState();
			this.checkPosition();
			this.checkIndex(i);

			return this.row[i].Value;
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetValues(System.Array)"]/*'/>
		public int GetValues(object[] values)
		{
			this.checkState();
			this.checkPosition();
			
			for (int i = 0; i < this.fields.Count; i++)
			{
				values[i] = this.GetValue(i);
			}

			return values.Length;
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetBoolean(System.Int32)"]/*'/>
		public bool GetBoolean(int i)
		{
			this.checkPosition();

			return (bool)this.GetValue(i);
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetByte(System.Int32)"]/*'/>
		public byte GetByte(int i)
		{
			this.checkPosition();
			this.checkIndex(i);

			return this.row[i].GetByte();
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetBytes(System.Int32,System.Int64,System.Byte[],System.Int32,System.Int32)"]/*'/>
		public long GetBytes(
			int		i, 
			long	dataIndex, 
			byte[]	buffer, 
			int		bufferIndex, 
			int		length)
		{
			this.checkPosition();
			this.checkIndex(i);

			int bytesRead	= 0;
			int realLength	= length;

			if (buffer == null)
			{
				if (this.IsDBNull(i))
				{
					return 0;
				}
				else
				{
					return this.row[i].GetBinary().Length;
				}
			}
			
			byte[] byteArray = (byte[])row[i].GetBinary();

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

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetChar(System.Int32)"]/*'/>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public char GetChar(int i)
		{
			this.checkPosition();
			this.checkIndex(i);

			return this.row[i].GetChar();
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetChars(System.Int32,System.Int64,System.Char[],System.Int32,System.Int32)"]/*'/>
		public long GetChars(
			int		i, 
			long	dataIndex, 
			char[]	buffer, 
			int		bufferIndex, 
			int		length)
		{
			this.checkPosition();
			this.checkIndex(i);

			if (buffer == null)
			{
				if (this.IsDBNull(i))
				{
					return 0;
				}
				else
				{
					char[] data = ((string)this.GetValue(i)).ToCharArray();

					return data.Length;
				}
			}
			
			int charsRead	= 0;
			int realLength	= length;
			
			char[] charArray = ((string)this.GetValue(i)).ToCharArray();

			if (length > (charArray.Length - dataIndex))
			{
				realLength = charArray.Length - (int)dataIndex;
			}
            					
			System.Array.Copy(charArray, (int)dataIndex, buffer, 
				bufferIndex, realLength);

			if ( (charArray.Length - dataIndex) < length)
			{
				charsRead = charArray.Length - (int)dataIndex;
			}
			else
			{
				charsRead = length;
			}

			return charsRead;
		}
		
		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetGuid(System.Int32)"]/*'/>
		public Guid GetGuid(int i)
		{
			throw new NotSupportedException("Guid data type is not supported.");
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetInt16(System.Int32)"]/*'/>
		public Int16 GetInt16(int i)
		{
			this.checkPosition();
			this.checkIndex(i);

			return this.row[i].GetInt16();
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetInt32(System.Int32)"]/*'/>
		public Int32 GetInt32(int i)
		{
			this.checkPosition();
			this.checkIndex(i);

			return this.row[i].GetInt32();
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetInt64(System.Int32)"]/*'/>		
		public Int64 GetInt64(int i)
		{
			this.checkPosition();
			this.checkIndex(i);

			return this.row[i].GetInt64();
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetFloat(System.Int32)"]/*'/>
		public float GetFloat(int i)
		{
			this.checkPosition();
			this.checkIndex(i);

			return this.row[i].GetFloat();
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetDouble(System.Int32)"]/*'/>
		public double GetDouble(int i)
		{
			this.checkPosition();
			this.checkIndex(i);

			return this.row[i].GetDouble();
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetString(System.Int32)"]/*'/>
		public String GetString(int i)
		{
			this.checkPosition();
			this.checkIndex(i);

			return this.row[i].GetString();
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetDecimal(System.Int32)"]/*'/>
		public Decimal GetDecimal(int i)
		{
			this.checkPosition();
			this.checkIndex(i);

			return this.row[i].GetDecimal();
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetDateTime(System.Int32)"]/*'/>
		public DateTime GetDateTime(int i)
		{
			this.checkPosition();

			this.checkIndex(i);

			return this.row[i].GetDateTime();
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetData(System.Int32)"]/*'/>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IDataReader GetData(int i)
		{
			throw new NotSupportedException("GetData not supported.");
		}		

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="IsDBNull(System.Int32)"]/*'/>
		public bool IsDBNull(int i)
		{
			this.checkPosition();
			this.checkIndex(i);
			
			return this.row[i].IsDBNull();
		}

		#endregion

		#region IEnumerable Methods

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="IEnumerable.GetEnumerator"]/*'/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new DbEnumerator(this);
		}

		#endregion

		#region Private Methods

        private void checkPosition()
        {
            if (this.eof || this.position == STARTPOS)
            {
                throw new InvalidOperationException("There are no data to read.");
            }
        }

        private void checkState()
		{
            if (this.isClosed)
            {
				throw new InvalidOperationException("Invalid attempt of read when the reader is closed.");
			}
		}

		private void checkIndex(int i)
		{
			if (i < 0 || i >= this.FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}
		}

		private FbDbType getProviderType(int i)
		{
			return (FbDbType)this.fields[i].DbDataType;
		}

		private bool isReadOnly(DbValue[] values)
		{
			/* row[8] = COMPUTED_BLR
			 * row[9] = COMPUTED_SOURCE
			 */
			if (values[0].Value != System.DBNull.Value || 
				values[1].Value != System.DBNull.Value)
			{
				return true;
			}

			return false;
		}

		private DataTable getSchemaTableStructure()
		{
			DataTable schema = new DataTable("Schema");			

			// Schema table structure
			schema.Columns.Add("ColumnName"		, System.Type.GetType("System.String"));
			schema.Columns.Add("ColumnOrdinal"	, System.Type.GetType("System.Int32"));
			schema.Columns.Add("ColumnSize"		, System.Type.GetType("System.Int32"));
			schema.Columns.Add("NumericPrecision", System.Type.GetType("System.Int32"));
			schema.Columns.Add("NumericScale"	, System.Type.GetType("System.Int32"));
			schema.Columns.Add("DataType"		, System.Type.GetType("System.Type"));
			schema.Columns.Add("ProviderType"	, System.Type.GetType("System.Int32"));
			schema.Columns.Add("IsLong"			, System.Type.GetType("System.Boolean"));
			schema.Columns.Add("AllowDBNull"	, System.Type.GetType("System.Boolean"));
			schema.Columns.Add("IsReadOnly"		, System.Type.GetType("System.Boolean"));
			schema.Columns.Add("IsRowVersion"	, System.Type.GetType("System.Boolean"));
			schema.Columns.Add("IsUnique"		, System.Type.GetType("System.Boolean"));
			schema.Columns.Add("IsKey"			, System.Type.GetType("System.Boolean"));
			schema.Columns.Add("IsAutoIncrement", System.Type.GetType("System.Boolean"));
			schema.Columns.Add("IsAliased"		, System.Type.GetType("System.Boolean"));
			schema.Columns.Add("IsExpression"	, System.Type.GetType("System.Boolean"));
			schema.Columns.Add("BaseSchemaName"	, System.Type.GetType("System.String"));
			schema.Columns.Add("BaseCatalogName", System.Type.GetType("System.String"));
			schema.Columns.Add("BaseTableName"	, System.Type.GetType("System.String"));
			schema.Columns.Add("BaseColumnName"	, System.Type.GetType("System.String"));
			
			return schema;
		}

		private string getSchemaCommandText()
		{
			System.Text.StringBuilder sql = new System.Text.StringBuilder();
			sql.AppendFormat(
				"SELECT \n" +
				    // "rfr.rdb$relation_name as table_name,\n"	+
				    // "rfr.rdb$field_name        as colum_name,\n"	+
				    // "rfr.rdb$field_position    as column_ordinal,\n"+
				    // "fld.rdb$field_length      as column_size,\n"	+
				    // "fld.rdb$field_precision   as numeric_precision,\n"	+
				    // "fld.rdb$field_scale       as numeric_scale,\n"	+
				    // "fld.rdb$field_type        as data_type,\n"		+
				    // "fld.rdb$field_sub_type    as data_sub_type,\n"	+
				    "fld.rdb$computed_blr	   as computed_blr,\n"	+
				    "fld.rdb$computed_source   as computed_source,\n"	+
				    // "rfr.rdb$null_flag         as nullable,\n"		+
				    // "rfr.rdb$update_flag       as is_readonly,\n"	+
				    // "rfr.rdb$default_value     as column_def,\n"	+				
				    "(select count(*)\n"							+
				    "from rdb$relation_constraints rel, rdb$indices idx, rdb$index_segments seg\n"	+
				    "where rel.rdb$constraint_type = 'PRIMARY KEY'\n"	+
				    "and rel.rdb$index_name = idx.rdb$index_name\n"	+
				    "and idx.rdb$index_name = seg.rdb$index_name\n"	+
				    "and rel.rdb$relation_name = rfr.rdb$relation_name\n"	+
				    "and seg.rdb$field_name = rfr.rdb$field_name) as primary_key,\n"	+
				    "(select count(*)\n"	+
				    "from rdb$relation_constraints rel, rdb$indices idx, rdb$index_segments seg\n"	+
				    "where rel.rdb$constraint_type = 'UNIQUE'\n"	+
				    "and rel.rdb$index_name = idx.rdb$index_name\n"	+
				    "and idx.rdb$index_name = seg.rdb$index_name\n"	+
				    "and rel.rdb$relation_name = rfr.rdb$relation_name\n"	+
				    "and seg.rdb$field_name = rfr.rdb$field_name) as unique_key,\n"	+
                    "fld.rdb$field_precision as numeric_precision\n"	+
                "from rdb$relation_fields rfr, rdb$fields fld\n"	+
				    "where rfr.rdb$field_source = fld.rdb$field_name");
				
			sql.Append("\n and rfr.rdb$relation_name = ?");	
			sql.Append("\n and rfr.rdb$field_name = ?");
			sql.Append("\n order by rfr.rdb$relation_name, rfr.rdb$field_position");

			return sql.ToString();			
		}

		private void updateRecordsAffected()
		{
			if (this.command != null && !this.command.IsDisposed)
			{
				if (this.command.RecordsAffected != -1)
				{
					this.recordsAffected = 
						this.recordsAffected == -1 ? 0 : this.recordsAffected;
					this.recordsAffected += this.command.RecordsAffected;
				}
			}
		}

		#endregion
	}
}
