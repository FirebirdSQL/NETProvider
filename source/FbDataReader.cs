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
using System.Collections;
using System.Data;
using System.Data.Common;

using FirebirdSql.Data.INGDS;
using FirebirdSql.Data.NGDS;
using FirebirdSql.Logging;

namespace FirebirdSql.Data.Firebird
{	
	/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="T:FbDataReader"]/*'/>	
	public sealed class FbDataReader : MarshalByRefObject, IDataReader, 
										IEnumerable, IDisposable, IDataRecord
	{		
		#region FIELDS
		
		private const int	STARTPOS = -1;

		private bool		disposed;
		private Log4CSharp  log;
		private bool		open;
		private int			position;
		private int			recordsAffected;
		private int			fieldCount;
		private DataTable	schemaTable;
		private FbCommand	command;		

		#endregion

		#region CONSTRUCTORS

		private FbDataReader()
		{
			disposed		= false;
			log				= null;
			open			= true;		
			position		= STARTPOS;
			recordsAffected = -1;						
			fieldCount		= -1;
			schemaTable		= null;
			command			= null;
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:#ctor(FirebirdSql.Data.Firebird.FbCommand)"]/*'/>
		internal FbDataReader(FbCommand command) : this()
		{
			command.Connection.DataReader = this;

			this.command	 = command;

			updateRecordsAffected();

			#if (_DEBUG)
				log = new Log4CSharp(GetType(), "fbprov.log", Mode.APPEND);
			#endif
		}

		#endregion

		#region DESTRUCTORS

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:Finalize"]/*'/>
		~FbDataReader()
		{
			Dispose(false);
		}

		#endregion

		#region IDISPOSABLE_METHODS
		
		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:Dispose"]/*'/>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!disposed)
			{
				try
				{
					if (disposing)
					{
						// release any managed resources
						Close();
					}

					// release any unmanaged resources
				}
				finally
				{
				}
							
				disposed = true;
			}
		}

		#endregion

		#region IDATAREADER_PROPERTIES_METHODS

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="P:Depth"]/*'/>
		public int Depth 
		{
			get { return 0; }
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="P:IsClosed"]/*'/>
		public bool IsClosed
		{
			get { return !open; }
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="P:RecordsAffected"]/*'/>
		public int RecordsAffected 
		{
			get { return IsClosed ? recordsAffected : -1; }
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:Close"]/*'/>
		public void Close()
		{
			if (!open)
			{
				return;
			}

			if (command != null)
			{
				if (!command.IsDisposed)
				{
					command.Connection.DataReader = null;

					if (command.Statement != null)
					{
						// Set values of output parameters
						command.Statement.SetOutputParameterValues();

						// Close statement
						command.Statement.Close();
			
						if ((command.CommandBehavior & CommandBehavior.CloseConnection) == CommandBehavior.CloseConnection)
						{
							command.Connection.Close();
						}
					}
				}
			}

			open	 = false;			
			position = STARTPOS;
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:NextResult"]/*'/>
		public bool NextResult()
		{
			if (IsClosed)
			{
				throw new InvalidOperationException("The datareader must be opened.");
			}

			bool returnValue = command.NextResult();

			if (returnValue)
			{
				updateRecordsAffected();
				position  = STARTPOS;				
			}

			return returnValue;
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:Read"]/*'/>
		public bool Read()
		{
			if (log != null) log.Debug("read");

			if (command.CommandBehavior == CommandBehavior.SingleRow && 
				position != STARTPOS)
			{
				return false;
			}

			try
			{
				command.Statement.Resultset.Fetch();
			}
			catch(GDSException ex)
			{
				throw new FbException(ex.Message, ex);
			}

			if (command.Statement.Resultset.EOF)
			{
				if (position == STARTPOS)
				{
					fieldCount = 0;
				}

				return false;
			}
			else
			{
				fieldCount = command.Statement.Resultset.FieldCount;

				position++;

				return true;
			}
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetSchemaTable"]/*'/>
		public DataTable GetSchemaTable()
		{
			if (log != null) log.Debug("GetSchemaTable");

			if (schemaTable != null)
			{
				return schemaTable;
			}

			DataRow		schemaRow;

			schemaTable = getSchemaTableStructure();

			/* Prepare statement for schema fields information	*/
			FbStatement fbSchemaRow = FbMetaData.GetSchemaColumns(
												command.Transaction	,
												null				,
												null				,
												true				,
												true				);
			fbSchemaRow.Prepare();

			for (int i = 0; i < FieldCount; i++)
			{
				/* Get Schema data for the field	*/
				fbSchemaRow.Parameters[0].Value = getBaseTableName(i);
				fbSchemaRow.Parameters[1].Value = getBaseColumnName(i);
				fbSchemaRow.Execute();
				bool fetched = fbSchemaRow.Resultset.Fetch();

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
				if (fetched)
				{
					// Is Table Field
					schemaRow["IsReadOnly"]		= ((Int16)fbSchemaRow.Resultset["IS_READONLY"]) == 0 ? true : false;
					schemaRow["IsKey"]			= ((Int32)fbSchemaRow.Resultset["PRIMARY_KEY"]) != 0 ? true : false;
					schemaRow["IsUnique"]		= ((Int32)fbSchemaRow.Resultset["UNIQUE_KEY"]) != 0 ? true : false;
				}
				else
				{
					// Isn't Table Field
					schemaRow["IsReadOnly"]		= true;
					schemaRow["IsKey"]			= false;
					schemaRow["IsUnique"]		= false;
				}
				schemaRow["IsAliased"]		= isAliased(i);
				schemaRow["IsExpression"]	= isExpression(i);
				schemaRow["BaseSchemaName"]	= DBNull.Value;
				schemaRow["BaseCatalogName"]= DBNull.Value;
				schemaRow["BaseTableName"]	= getBaseTableName(i);
				schemaRow["BaseColumnName"]	= getBaseColumnName(i);

				schemaTable.Rows.Add(schemaRow);

				/* Close statement	*/
				fbSchemaRow.Close();
			}

			/* Drop statement	*/
			fbSchemaRow.DropStatement();

			return schemaTable;
		}

		#endregion

		#region IDATARECORD_PROPERTIES_METHODS

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="P:Item(System.Int32)"]/*'/>
		public object this[int i]
		{
			get { return GetValue(i); }
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="P:Item(System.String)"]/*'/>
		public object this[String name]
		{			
			get { return GetValue(GetOrdinal(name)); }
		}
		
		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="P:FieldCount"]/*'/>
		public int FieldCount
		{			
			get { return command.Statement.Resultset.FieldCount; }
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetName(System.Int32)"]/*'/>
		public String GetName(int i)
		{
			if (log != null) log.Debug("GetName");

			return command.Statement.Resultset.GetName(i);
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetDataTypeName(System.Int32)"]/*'/>
		public String GetDataTypeName(int i)
		{
			if (log != null) log.Debug("GetDataTypeName");

			return command.Statement.Resultset.GetDataTypeName(i);
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetFieldType(System.Int32)"]/*'/>
		public Type GetFieldType(int i)
		{			
			if (log != null) log.Debug("GetFieldType");

			return command.Statement.Resultset.GetFieldType(i);
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetValue(System.Int32)"]/*'/>
		public Object GetValue(int i)
		{
			if (log != null) log.Debug("GetValue");

			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			return command.Statement.Resultset.GetValue(i);
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetValues(System.Object[])"]/*'/>
		public int GetValues(object[] values)
		{
			if (log != null) log.Debug("GetValues");

			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}
			
			for(int i = 0; i < FieldCount; i++)
			{
				values[i] = GetValue(i);
			}

			return values.Length;
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetOrdinal(System.String)"]/*'/>
		public int GetOrdinal(string name)
		{
			if (log != null) log.Debug("GetOrdinal");

			if (IsClosed)
			{
				throw new InvalidOperationException("Reader closed");
			}

			return command.Statement.Resultset.GetOrdinal(name);
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetBoolean(System.Int32)"]/*'/>
		public bool GetBoolean(int i)
		{
			if (log != null) log.Debug("GetBoolean");

			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			// return command.Statement.Resultset.GetBoolean(i);
			return Convert.ToBoolean(GetValue(i));
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetByte(System.Int32)"]/*'/>
		public byte GetByte(int i)
		{
			if (log != null) log.Debug("GetByte");

			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			// return command.Statement.Resultset.GetByte(i);
			return Convert.ToByte(GetValue(i));
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetBytes(System.Int32,System.Int64,System.Byte[],System.Int32,System.Int32)"]/*'/>
		public long GetBytes(int i, long dataIndex, byte[] buffer, 
							int bufferIndex, int length)
		{
			if (log != null) log.Debug("GetBytes");

			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			return command.Statement.Resultset.GetBytes(i, dataIndex, buffer,
														bufferIndex, length);
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetChar(System.Int32)"]/*'/>
		public char GetChar(int i)
		{
			if (log != null) log.Debug("GetChar");

			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}
			
			// return command.Statement.Resultset.GetChar(i);
			return Convert.ToChar(GetValue(i));
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetChars(System.Int32,System.Int64,System.Char[],System.Int32,System.Int32)"]/*'/>
		public long GetChars(int i, long dataIndex, char[] buffer, 
							int bufferIndex, int length)
		{
			if (log != null) log.Debug("GetChars");

			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			return command.Statement.Resultset.GetChars(i, dataIndex, buffer, 
														bufferIndex, length);
		}
		
		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetGuid(System.Int32)"]/*'/>
		public Guid GetGuid(int i)
		{
			if (log != null) log.Debug("GetGuid");

			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			return command.Statement.Resultset.GetGuid(i);
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetInt16(System.Int32)"]/*'/>
		public Int16 GetInt16(int i)
		{
			if (log != null) log.Debug("GetInt16");

			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			// return command.Statement.Resultset.GetInt16(i);
			return Convert.ToInt16(GetValue(i));
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetInt32(System.Int32)"]/*'/>
		public Int32 GetInt32(int i)
		{
			if (log != null) log.Debug("GetInt32");

			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			// return command.Statement.Resultset.GetInt32(i);
			return Convert.ToInt32(GetValue(i));
		}
		
		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetInt64(System.Int32)"]/*'/>
		public Int64 GetInt64(int i)
		{
			if (log != null) log.Debug("GetInt64");

			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			// return command.Statement.Resultset.GetInt64(i);
			return Convert.ToInt64(GetValue(i));
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetFloat(System.Int32)"]/*'/>
		public float GetFloat(int i)
		{
			if (log != null) log.Debug("GetFloat");

			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			// return command.Statement.Resultset.GetFloat(i);
			return Convert.ToSingle(GetValue(i));
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetDouble(System.Int32)"]/*'/>
		public double GetDouble(int i)
		{
			if (log != null) log.Debug("GetDouble");

			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}
			
			// return command.Statement.Resultset.GetDouble(i);
			return Convert.ToDouble(GetValue(i));
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetString(System.Int32)"]/*'/>
		public String GetString(int i)
		{
			if (log != null) log.Debug("GetString");

			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			// return command.Statement.Resultset.GetString(i);
			return Convert.ToString(GetValue(i));
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetDecimal(System.Int32)"]/*'/>
		public Decimal GetDecimal(int i)
		{
			if (log != null) log.Debug("GetDecimal");

			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			// return command.Statement.Resultset.GetDecimal(i);
			return Convert.ToDecimal(GetValue(i));
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetDateTime(System.Int32)"]/*'/>
		public DateTime GetDateTime(int i)
		{
			if (log != null) log.Debug("GetDateTime");

			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			// return command.Statement.Resultset.GetDateTime(i);
			return Convert.ToDateTime(GetValue(i));
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetData(System.Int32)"]/*'/>		
		public IDataReader GetData(int i)
		{
			if (log != null) log.Debug("GetData");

			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			return command.Statement.Resultset.GetData(i);
		}		

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:IsDBNull(System.Int32)"]/*'/>
		public bool IsDBNull(int i)
		{	
			if (log != null) log.Debug("IsDbNull");

			if (position == STARTPOS)
			{
				throw new InvalidOperationException("There are no data to read.");
			}

			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}
			
			XSQLVAR sqlvar = command.Statement.Statement.OutSqlda.sqlvar[i];

			return sqlvar.sqlind == -1 ? true : false;
		}

		#endregion

		#region IENUMERABLE_METHODS

		/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="M:IEnumerable.GetEnumerator"]/*'/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new DbEnumerator(this);
		}

		#endregion

		#region SPECIFIC_METHODS

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetProviderType(System.Int32)"]/*'/>
		private int getProviderType(int i)
		{
			return command.Statement.Resultset.GetProviderType(i);
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetSize(System.Int32)"]/*'/>
		private int getSize(int i)
		{
			return command.Statement.Resultset.GetSize(i);
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetScale(System.Int32)"]/*'/>
		private int getScale(int i)
		{
			return command.Statement.Resultset.GetScale(i);
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetBaseTableName(System.Int32)"]/*'/>
		private String getBaseTableName(int i)
		{
			return command.Statement.Resultset.GetBaseTableName(i);
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetBaseColumnName(System.Int32)"]/*'/>
		private String getBaseColumnName(int i)
		{
			return command.Statement.Resultset.GetBaseColumnName(i);
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:IsNumeric(System.Int32)"]/*'/>
		private bool isNumeric(int i)
		{	
			if (log != null) log.Debug("IsDbNull");

			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}
			
			XSQLDA sqlda = command.Statement.Statement.OutSqlda;

			return FbField.IsNumeric(sqlda.sqlvar[i].sqltype);
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:IsLong(System.Int32)"]/*'/>
		private bool isLong(int i)
		{	
			if (log != null) log.Debug("IsDbNull");
			
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}
			
			XSQLDA sqlda = command.Statement.Statement.OutSqlda;

			return FbField.IsLong(sqlda.sqlvar[i].sqltype);			
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:AllowDBNull(System.Int32)"]/*'/>
		private bool allowDBNull(int i)
		{	
			if (log != null) log.Debug("IsDbNull");

			return command.Statement.Resultset.AllowDBNull(i);
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:IsAliased(System.Int32)"]/*'/>
		private bool isAliased(int i)
		{	
			if (log != null) log.Debug("IsAliased");

			return command.Statement.Resultset.IsAliased(i);
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:IsExpression(System.Int32)"]/*'/>
		private bool isExpression(int i)
		{	
			if (log != null) log.Debug("IsExpression");

			return command.Statement.Resultset.IsExpression(i);
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:GetSchemaTableStructure"]/*'/>
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

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="M:updateRecordsAffected"]/*'/>
		private void updateRecordsAffected()
		{
			if (command.RecordsAffected != -1)
			{
				recordsAffected = recordsAffected == -1 ? 0 : recordsAffected;
				recordsAffected += command.RecordsAffected;
			}
		}

		#endregion
	}
}
