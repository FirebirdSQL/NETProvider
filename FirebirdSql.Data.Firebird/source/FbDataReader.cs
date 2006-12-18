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
using System.Globalization;
using System.ComponentModel;
using FirebirdSql.Data.Firebird.Gds;

namespace FirebirdSql.Data.Firebird
{	
	/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/overview/*'/>
	public sealed class FbDataReader : MarshalByRefObject, IDataReader, IEnumerable, IDisposable, IDataRecord
	{		
		#region FIELDS
		
		private const int		STARTPOS = -1;
		private bool			disposed;
		private bool			open;
		private int				position;
		private int				recordsAffected;
		private int				fieldCount;
		private DataTable		schemaTable;
		private FbCommand		command;
		private FbConnection	connection;
		private GdsValue[]		row;
		private	CommandBehavior	behavior;

		#endregion

		#region PROPERTIES

		internal FbCommand Command
		{
			get { return command; }
		}

		#endregion

		#region CONSTRUCTORS

		private FbDataReader()
		{
			this.open				= true;
			this.position			= STARTPOS;
			this.recordsAffected	= -1;						
			this.fieldCount			= -1;
		}

		internal FbDataReader(FbCommand command, FbConnection connection) : this()
		{
			this.command				= command;
			this.behavior				= this.command.CommandBehavior;
			this.connection				= connection;
			this.connection.DataReader	= this;

			this.fieldCount = this.command.Statement.Fields.SqlD;
		}

		#endregion

		#region DESTRUCTORS

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/destructor[@name="Finalize"]/*'/>
		~FbDataReader()
		{
			Dispose(false);
		}

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

						this.command	= null;
						this.connection	= null;
						this.row		= null;
						this.schemaTable= null;
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

		#region IDATAREADER_PROPERTIES_METHODS

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/property[@name="Depth"]/*'/>
		public int Depth 
		{
			get { return 0; }
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/property[@name="IsClosed"]/*'/>
		public bool IsClosed
		{
			get { return !open; }
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/property[@name="RecordsAffected"]/*'/>
		public int RecordsAffected 
		{
			get { return IsClosed ? recordsAffected : -1; }
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/property[@name="HasRows"]/*'/>
		public bool HasRows
		{
			get 
			{ 
				return true;
			}
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="Close"]/*'/>
		public void Close()
		{
			bool closeConnection = false;

			if (!this.open)
			{
				return;
			}

			if (this.position == STARTPOS && this.command.CommandType == CommandType.StoredProcedure)
			{
				// Read the first row, this is needed when using stored procedures
				// for update/delete/insert operations with the dataadapter
				// because DbDataAdapter.Update makes calls to FbCommand.ExecuteReader
				this.Read();
			}

			this.open		= false;
			this.position	= STARTPOS;

			this.updateRecordsAffected();

			if (this.connection != null)
			{
				this.connection.DataReader = null;

				if ((this.behavior & CommandBehavior.CloseConnection) == CommandBehavior.CloseConnection)
				{
					closeConnection = true;
				}
			}

			if (this.command != null		&&
				!this.command.IsDisposed)
			{
				// Set values of output parameters
				this.command.InternalSetOutputParameters();

				// Try to commit the implcit transaction
				this.command.CommitImplicitTransaction();
			}

			if (closeConnection)
			{
				this.connection.Close();
			}
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="NextResult"]/*'/>
		public bool NextResult()
		{
			if (IsClosed)
			{
				throw new InvalidOperationException("The datareader must be opened.");
			}

			updateRecordsAffected();

			bool returnValue = command.NextResult();

			if (returnValue)
			{
				fieldCount	= this.command.Statement.Fields.SqlD;
				position	= STARTPOS;
			}
			else
			{
				row = null;
			}

			return returnValue;
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="Read"]/*'/>
		public bool Read()
		{
			bool retValue = false;

			if ((this.behavior & CommandBehavior.SingleRow) == CommandBehavior.SingleRow &&
				position != STARTPOS)
			{
			}
			else
			{
				if ((this.behavior & CommandBehavior.SchemaOnly) == CommandBehavior.SchemaOnly ||
					this.command.Statement == null)
				{
				}
				else
				{
					if (this.command.Statement.StatementType == GdsStatementType.Select ||
						this.command.Statement.StatementType == GdsStatementType.SelectForUpdate ||
						this.command.Statement.StatementType == GdsStatementType.StoredProcedure)
					{
						try
						{
							this.row = this.command.Statement.Fetch();
						}
						catch(GdsException ex)
						{
							throw new FbException(ex.Message, ex);
						}

						if (this.row == null)
						{
							if (this.position == STARTPOS)
							{
								this.fieldCount = 0;
							}
						}
						else
						{
							this.fieldCount = this.command.Statement.Fields.SqlD;

							this.position++;

							retValue = true;
						}
					}
				}
			}

			return retValue;
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetSchemaTable"]/*'/>
		public DataTable GetSchemaTable()
		{
			if (this.schemaTable != null)
			{
				return this.schemaTable;
			}

			DataRow	schemaRow;

			this.schemaTable = getSchemaTableStructure();

			/* Prepare statement for schema fields information	*/
			FbCommand schemaCmd = new FbCommand(
								this.getSchemaCommandText(),
								this.command.Connection,
								this.command.ActiveTransaction);

			schemaCmd.Parameters.Add("@TABLE_NAME", FbDbType.VarChar);
			schemaCmd.Parameters.Add("@COLUMN_NAME", FbDbType.VarChar);
			schemaCmd.InternalPrepare();
			
			schemaTable.BeginLoadData();			
			for (int i = 0; i < fieldCount; i++)
			{
				/* Get Schema data for the field	*/
				schemaCmd.Parameters[0].Value = getBaseTableName(i);
				schemaCmd.Parameters[1].Value = getBaseColumnName(i);
				
				schemaCmd.InternalExecute();

				GdsValue[] values = schemaCmd.Statement.Fetch();

				/* Create new row for the Schema Table	*/
				schemaRow = schemaTable.NewRow();

				schemaRow["ColumnName"]		= GetName(i);
				schemaRow["ColumnOrdinal"]	= i;
				schemaRow["ColumnSize"]		= getSize(i);
				
				if (isNumeric(i))
				{
					schemaRow["NumericPrecision"]= getSize(i);
					schemaRow["NumericScale"]	 = getScale(i);
				}
				else
				{
					schemaRow["NumericPrecision"]= System.DBNull.Value;
					schemaRow["NumericScale"]	 = System.DBNull.Value;
				}
				
				schemaRow["DataType"]		= GetFieldType(i);
				schemaRow["ProviderType"]	= getProviderType(i);
				schemaRow["IsLong"]			= isLong(i);
				
				if ((bool)schemaRow["IsLong"])
				{
					schemaRow["ColumnSize"]	= System.Int32.MaxValue;
				}

				schemaRow["AllowDBNull"]	= allowDBNull(i);
				schemaRow["IsRowVersion"]	= false;
				schemaRow["IsAutoIncrement"]= false;
				
				if (values != null)
				{
					// Is Table Field
					schemaRow["IsReadOnly"]	= isReadOnly(values) || isExpression(i) ? true : false;
					schemaRow["IsKey"]		= (Convert.ToInt32(values[13].Value) == 1) ? true : false;
					schemaRow["IsUnique"]	= (Convert.ToInt32(values[14].Value) == 1) ? true : false;
				}
				else
				{
					// Isn't Table Field
					schemaRow["IsReadOnly"]	= true;
					schemaRow["IsKey"]		= false;
					schemaRow["IsUnique"]	= false;
				}
				
				schemaRow["IsAliased"]		= isAliased(i);
				schemaRow["IsExpression"]	= isExpression(i);
				schemaRow["BaseSchemaName"]	= DBNull.Value;
				schemaRow["BaseCatalogName"]= DBNull.Value;
				schemaRow["BaseTableName"]	= getBaseTableName(i);
				schemaRow["BaseColumnName"]	= getBaseColumnName(i);

				schemaTable.Rows.Add(schemaRow);

				/* Close statement	*/
				schemaCmd.Statement.Close();				
			}

			schemaTable.EndLoadData();

			/* Dispose command	*/
			schemaCmd.Dispose();

			return schemaTable;
		}

		#endregion

		#region IDATARECORD_PROPERTIES_METHODS

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/indexer[@name="Item(System.Int32)"]/*'/>
		public object this[int i]
		{
			get { return GetValue(i); }
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/indexer[@name="Item(System.String)"]/*'/>
		public object this[String name]
		{			
			get { return GetValue(GetOrdinal(name)); }
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/property[@name="FieldCount"]/*'/>
		public int FieldCount
		{			
			get { return fieldCount; }
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetName(System.Int32)"]/*'/>
		public string GetName(int i)
		{
			if (i < 0 || i >= fieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			if (command.Statement.Fields.SqlVar[i].AliasName.Length > 0)
			{
				return command.Statement.Fields.SqlVar[i].AliasName;
			}
			else
			{
				return command.Statement.Fields.SqlVar[i].SqlName;
			}
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetDataTypeName(System.Int32)"]/*'/>
		public String GetDataTypeName(int i)
		{
			if (i < 0 || i >= fieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			return command.Statement.Fields.SqlVar[i].GetDataTypeName();
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetFieldType(System.Int32)"]/*'/>
		public Type GetFieldType(int i)
		{
			checkIndex(i);

			return command.Statement.Fields.SqlVar[i].GetSystemType();
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetValue(System.Int32)"]/*'/>
		public Object GetValue(int i)
		{
			checkIndex(i);

			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			if (row[i] == null)
			{
				return System.DBNull.Value;
			}

			return row[i].Value;
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetValues(System.Array)"]/*'/>
		public int GetValues(object[] values)
		{
			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}
			
			for (int i = 0; i < fieldCount; i++)
			{
				values[i] = GetValue(i);
			}

			return values.Length;
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetOrdinal(System.String)"]/*'/>
		public int GetOrdinal(string name)
		{
			if (IsClosed)
			{
				throw new InvalidOperationException("Reader closed");
			}

			GdsRowDescription rowDesc = command.Statement.Fields;

			for (int i = 0; i < fieldCount; i++)
			{
				if (cultureAwareCompare(name, rowDesc.SqlVar[i].AliasName))
				{
					return i;
				}
			}
						
			throw new IndexOutOfRangeException("Could not find specified column in results.");
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetBoolean(System.Int32)"]/*'/>
		public bool GetBoolean(int i)
		{
			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			return Convert.ToBoolean(GetValue(i));
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetByte(System.Int32)"]/*'/>
		public byte GetByte(int i)
		{
			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			return Convert.ToByte(GetValue(i));
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetBytes(System.Int32,System.Int64,System.Byte[],System.Int32,System.Int32)"]/*'/>
		public long GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
		{
			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			checkIndex(i);

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
					byte[] data = (byte[])this.GetValue(i);

					return data.Length;
				}
			}
			
			byte[] byteArray = (byte[])GetValue(i);

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
			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}
			
			return Convert.ToChar(GetValue(i));
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetChars(System.Int32,System.Int64,System.Char[],System.Int32,System.Int32)"]/*'/>
		public long GetChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length)
		{
			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			checkIndex(i);

			if (buffer == null)
			{
				if (this.IsDBNull(i))
				{
					return 0;
				}
				else
				{
					char[] data = ((string)GetValue(i)).ToCharArray();

					return data.Length;
				}
			}
			
			int charsRead	= 0;
			int realLength	= length;
			
			char[] charArray = ((string)GetValue(i)).ToCharArray();

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
			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			return Convert.ToInt16(GetValue(i));
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetInt32(System.Int32)"]/*'/>
		public Int32 GetInt32(int i)
		{
			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			return Convert.ToInt32(GetValue(i));
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetInt64(System.Int32)"]/*'/>		
		public Int64 GetInt64(int i)
		{
			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			return Convert.ToInt64(GetValue(i));
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetFloat(System.Int32)"]/*'/>
		public float GetFloat(int i)
		{
			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			return Convert.ToSingle(GetValue(i));
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetDouble(System.Int32)"]/*'/>
		public double GetDouble(int i)
		{
			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}
			
			return Convert.ToDouble(GetValue(i));
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetString(System.Int32)"]/*'/>
		public String GetString(int i)
		{
			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			return Convert.ToString(GetValue(i));
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetDecimal(System.Int32)"]/*'/>
		public Decimal GetDecimal(int i)
		{
			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			return Convert.ToDecimal(GetValue(i));
		}

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="GetDateTime(System.Int32)"]/*'/>
		public DateTime GetDateTime(int i)
		{
			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			return Convert.ToDateTime(GetValue(i));
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
			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			if (i < 0 || i >= fieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}
						
			return (row[i].Value == null || 
					row[i].Value == System.DBNull.Value) ? true : false;
		}

		#endregion

		#region IENUMERABLE_METHODS

		/// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/method[@name="IEnumerable.GetEnumerator"]/*'/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new DbEnumerator(this);
		}

		#endregion

		#region PRIVATE_METHODS

		private void checkIndex(int i)
		{
			if (i < 0 || i >= fieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}
		}

		private FbDbType getProviderType(int i)
		{
			return command.Statement.Fields.SqlVar[i].GetFbDbType();
		}

		private int getSize(int i)
		{
			if (command.Statement.Fields.SqlVar[i].CharCount != -1)
			{
				return command.Statement.Fields.SqlVar[i].CharCount;
			}
			else
			{
				return command.Statement.Fields.SqlVar[i].SqlLen;
			}
		}

		private int getScale(int i)
		{
			return command.Statement.Fields.SqlVar[i].SqlScale;
		}

		private string getBaseTableName(int i)
		{
			return command.Statement.Fields.SqlVar[i].RelName;
		}

		private string getBaseColumnName(int i)
		{
			return command.Statement.Fields.SqlVar[i].SqlName;
		}

		private bool isNumeric(int i)
		{			
			return command.Statement.Fields.SqlVar[i].IsNumeric();
		}

		private bool isLong(int i)
		{			
			return command.Statement.Fields.SqlVar[i].IsLong();
		}

		private bool allowDBNull(int i)
		{	
			GdsField field = command.Statement.Fields.SqlVar[i];

			return (field.SqlType & 1) == 1 ? true : false;
		}

		private bool isAliased(int i)
		{
			GdsField field = command.Statement.Fields.SqlVar[i];

			return field.SqlName != field.AliasName ? true : false;
		}

		private bool isReadOnly(GdsValue[] values)
		{
			/* row[8] = COMPUTED_BLR
			 * row[9] = COMPUTED_SOURCE
			 */
			if (values[8].Value != System.DBNull.Value || 
				values[9].Value != System.DBNull.Value)
			{
				return true;
			}

			return false;
		}

		private bool isExpression(int i)
		{	
			GdsField field = command.Statement.Fields.SqlVar[i];

			return field.SqlName.Length == 0 ? true : false;
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
				"SELECT rfr.rdb$relation_name as table_name,\n"	+
				"rfr.rdb$field_name        as colum_name,\n"	+
				"rfr.rdb$field_position    as column_ordinal,\n"+
				"fld.rdb$field_length      as column_size,\n"	+
				"fld.rdb$field_precision   as numeric_precision,\n"	+
				"fld.rdb$field_scale       as numeric_scale,\n"	+
				"fld.rdb$field_type        as data_type,\n"		+
				"fld.rdb$field_sub_type    as data_sub_type,\n"	+
				"fld.rdb$computed_blr	   as computed_blr,\n"	+
				"fld.rdb$computed_source   as computed_source,\n"	+
				"rfr.rdb$null_flag         as nullable,\n"		+
				"rfr.rdb$update_flag       as is_readonly,\n"	+
				"rfr.rdb$default_value     as column_def,\n"	+				
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
				"and seg.rdb$field_name = rfr.rdb$field_name) as unique_key\n"	+
				"from rdb$relation_fields rfr, rdb$fields fld\n"	+
				"where rfr.rdb$field_source = fld.rdb$field_name");
				
			sql.Append("\n and rfr.rdb$relation_name = ?");	
			sql.Append("\n and rfr.rdb$field_name = ?");
			sql.Append("\n order by rfr.rdb$relation_name, rfr.rdb$field_position");

			return sql.ToString();			
		}

		private void updateRecordsAffected()
		{
			if (command != null && !command.IsDisposed)
			{
				if (command.RecordsAffected != -1)
				{
					recordsAffected = recordsAffected == -1 ? 0 : recordsAffected;
					recordsAffected += command.RecordsAffected;
				}
			}
		}

		private bool cultureAwareCompare(string strA, string strB)
		{
			return CultureInfo.CurrentCulture.CompareInfo.Compare(
				strA, 
				strB, 
				CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | 
				CompareOptions.IgnoreCase) == 0 ? true : false;
		}

		#endregion
	}
}
