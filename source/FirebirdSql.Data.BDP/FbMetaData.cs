/*
 *  Firebird BDP - Borland Data provider Firebird
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
 *  Copyright (c) 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Collections;
using System.Data;
using System.Text;

using Borland.Data.Common;
using Borland.Data.Schema;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Bdp
{
	public class FbMetaData : ISQLMetaData
	{
		#region Fields

		private FbConnection	connection;
		private Hashtable		metadataProps;

		#endregion

		#region Constructors

		public FbMetaData(FbConnection connection)
		{
			this.connection	= connection;

			this.metadataProps = new Hashtable();

			this.metadataProps.Add(MetaDataProps.MaxColumnsInTable, 32767);
			this.metadataProps.Add(MetaDataProps.MaxObjectNameLength, 31);
			this.metadataProps.Add(MetaDataProps.MaxRowSize, 65535);
			this.metadataProps.Add(MetaDataProps.ObjectQuoteChar, "\"");
			this.metadataProps.Add(MetaDataProps.ObjectQuoteSuffix, "\"");
			this.metadataProps.Add(MetaDataProps.ProcSupportsCursor, true);
			this.metadataProps.Add(MetaDataProps.ProcSupportsCursors, false);
			this.metadataProps.Add(MetaDataProps.QuoteObjects, false);
		}

		#endregion

		#region ISQLMetaData Methods

#if (DIAMONDBACK)

		public ISQLSchemaCreate GetSchemaCreate()
		{
			return new FbSQLSchemaCreate(this.connection);
		}

#endif		

		public void GetProperty(MetaDataProps property, out object value)
		{
			switch (property)
			{
				case MetaDataProps.QualifiedName:
					value = this.metadataProps[MetaDataProps.ObjectName];
					break;

				case MetaDataProps.QuotedObjectName:
					string quote = this.metadataProps[MetaDataProps.ObjectQuoteChar].ToString();
					value = String.Format("{0}{1}{0}", quote, this.metadataProps[MetaDataProps.ObjectName]);
					break;
		
				default:
					if (this.metadataProps.ContainsKey(property))
					{
						value = this.metadataProps[property];
					}
					else
					{
						value = null;
					}
					break;
			}
		}

		public void SetProperty(MetaDataProps property, object value)
		{
			if (this.metadataProps.ContainsKey(property))
			{
				this.metadataProps[property] = value;
			}
			else
			{
				this.metadataProps.Add(property, value);
			}
		}

		public DataTable GetSchemaTable(IDataReader reader, IDbCommand command)
        {
#if (DIAMONDBACK)
            DataTable schema = BdpMetaDataHelper.GetSchemaTable();
#else
            DataTable schema = BdpMetaDataHelper.CreateSchemaTable();
#endif

			/* Prepare the command that is requested for obtain
			 * schema information
			 */
			FbCommand c = (FbCommand)this.connection.GetSQLCommand();
			c.Prepare(command.CommandText, (short)command.Parameters.Count);

			Descriptor fields = c.GetFieldsDescriptor();

			/* Prepare the command used to obtain schema information like
			 * Primary and Unique Key information ...
			 */
			ISQLCursor	cursor			= null;
			short		numCols			= 0;
			string		currentTable	= "";
			FbCommand	sc				= (FbCommand)this.connection.GetSQLCommand();

			sc.Prepare(this.GetSchemaCommandText(), 2);

            // Fill schema information
			for (int i = 0; i < fields.Count; i++)
			{
				bool isKeyColumn	= false;
				bool isUnique		= false;
				bool isReadOnly		= true;

				// Get schema information
				sc.SetParameter(0, 0, ParameterDirection.Input, BdpType.String, BdpType.stFixed, 0, 0, 31, fields[i].Relation, true);
				sc.SetParameter(1, 0, ParameterDirection.Input, BdpType.String, BdpType.stFixed, 0, 0, 31, fields[i].Name, true);

				sc.Execute(out cursor, ref numCols);

				if (cursor.Next() == 0)
				{
					if (((FbCursor)cursor).GetValue(0) != System.DBNull.Value || 
						((FbCursor)cursor).GetValue(1) != System.DBNull.Value)
					{
						isReadOnly = true;
					}
					else
					{
						isReadOnly = false;
					}
					if (Convert.ToInt32(((FbCursor)cursor).GetValue(2)) > 0)
					{
						isKeyColumn = true;
					}
					if (Convert.ToInt32(((FbCursor)cursor).GetValue(3)) > 0)
					{
						isUnique = true;
					}
				}
				
				// Add column schema information
				DataRow row = schema.NewRow();

				row["ColumnName"]		= fields[i].Alias.Length > 0 ? fields[i].Alias : fields[i].Name;
				row["ColumnOrdinal"]	= i;
				row["ColumnSize"]		= fields[i].GetSize();
				if (fields[i].IsNumeric())
				{
					row["NumericPrecision"]	= fields[i].GetSize();
					row["NumericScale"]		= fields[i].NumericScale*(-1);
				}
				row["DataType"]			= fields[i].GetSystemType();
				row["ProviderType"]		= BdpTypeHelper.GetBdpType(fields[i].DbDataType);
				row["IsLong"]			= fields[i].IsLong();
				row["AllowDBNull"]		= fields[i].AllowDBNull();
				row["NumericPrecision"]	= 0;
				row["IsAutoIncrement"]	= false;
				row["IsRowVersion"]		= false;
				row["IsReadOnly"]		= isReadOnly;
				row["IsUnique"]			= isUnique;
				row["IsKeyColumn"]		= isKeyColumn;
				row["BaseSchemaName"]	= DBNull.Value;
				row["BaseCatalogName"]	= DBNull.Value;
				row["BaseTableName"]	= fields[i].Relation;
				row["BaseColumnName"]	= fields[i].Name;
				row["ProviderSubType"]	= BdpTypeHelper.GetBdpSubType(fields[i].DbDataType);;

				if (currentTable != String.Empty && 
					currentTable != fields[i].Relation)
				{
					return BdpMetaDataHelper.GetSchemaTable();
				}
				else
				{
					schema.Rows.Add(row);
					currentTable = fields[i].Relation;
				}		

				sc.Close();
			}

			// Free resources
			cursor.Release();
			c.Release();
			sc.Release();			

			return schema;
		}

		public DataTable GetObjectList(ObjectType type)
		{
			throw new NotSupportedException();
		}

		public DataTable GetTables(string tableName, TableType tableType)
		{
			StringBuilder	sql			= new StringBuilder();
			StringBuilder	where		= new StringBuilder();
			FbCommand		select		= new FbCommand(this.connection);
			ISQLCursor		cursor		= null;
#if (DIAMONDBACK)
            DataTable		schema		= BdpMetaDataHelper.GetTables();
#else
			DataTable		schema		= BdpMetaDataHelper.CreateTableTable();
#endif
			short			resultCols	= 0;

			sql.Append(
				@"SELECT " +
				"rdb$relation_name AS TableName "	+
				"FROM " +
				"rdb$relations");
			
			if (tableName != null && tableName.Length > 0)
			{
				where.AppendFormat("rdb$relation_name = '{0}'", tableName);
			}

			if (where.Length > 0)
			{
				where.Append(" AND ");
			}

			switch (tableType)
			{
				case TableType.SystemTable:
					where.Append("rdb$view_source IS null AND rdb$system_flag = 1");
					break;

				case TableType.View:
					where.Append("rdb$view_source IS NOT null AND rdb$system_flag = 0");
					break;

				case TableType.Table:
				default:
					where.Append("rdb$view_source IS NULL AND rdb$system_flag = 0");
					break;
			}

			if (where.Length > 0)
			{
				sql.AppendFormat(" WHERE {0} ", where.ToString());
			}

			sql.Append(" ORDER BY rdb$system_flag, rdb$owner_name, rdb$relation_name");

			// Prepare and execute the command
			select.Prepare(sql.ToString(), 0);
			select.Execute(out cursor, ref resultCols);

			int recno = 0;

			while (cursor.Next() != -1)
			{
				DataRow row = schema.NewRow();

				row["Recno"]		= recno++;
				row["CatalogName"]	= null;
				row["SchemaName"]	= null;
				row["TableName"]	= ((FbCursor)cursor).GetValue(0).ToString().Trim();
				row["TableType"]	= tableType;

				schema.Rows.Add(row);
			}

			cursor.Release();
			select.Release();

			return schema;
		}

		public DataTable GetColumns(
			string tableName, 
			string columnName, 
			ColumnType columnType)
		{
			StringBuilder	sql			= new StringBuilder();
			StringBuilder	where		= new StringBuilder();
			FbCommand		select		= new FbCommand(this.connection);
			ISQLCursor		cursor		= null;
#if (DIAMONDBACK)
            DataTable		schema		= BdpMetaDataHelper.GetColumns();
#else
			DataTable		schema		= BdpMetaDataHelper.CreateColumnTable();
#endif
			short			resultCols	= 0;

			sql.Append(
				@"SELECT " +
				"rfr.rdb$relation_name AS TableName, " +
				"rfr.rdb$field_name AS ColumName, " +
				"rfr.rdb$field_position AS ColumnPosition, " +
				"fld.rdb$field_type AS ColumnDataType, " +
				"fld.rdb$field_sub_type AS ColumnSubType, " +
				"fld.rdb$field_length AS ColumnSize, " +
				"fld.rdb$field_precision AS ColumnPrecision, " +
				"fld.rdb$field_scale AS ColumnScale, " +
				"rfr.rdb$null_flag AS ColumnNullable " +
				"FROM " +
				"rdb$relation_fields rfr " +
				"left join rdb$fields fld ON rfr.rdb$field_source = fld.rdb$field_name ");

			if (tableName != null && tableName.Length > 0)
			{
				where.AppendFormat("rfr.rdb$relation_name = '{0}'", tableName);
			}

			if (columnName != null && columnName.Length > 0)
			{
				if (where.Length > 0)
				{
					where.Append(" AND ");
				}

				where.AppendFormat("rfr.rdb$field_name = '{0}'", columnName);
			}

			if (where.Length > 0)
			{
				sql.AppendFormat(" WHERE {0} ", where.ToString());
			}

			sql.Append(" ORDER BY rfr.rdb$relation_name, rfr.rdb$field_position");

			// Prepare and execute the command
			select.Prepare(sql.ToString(), 0);
			select.Execute(out cursor, ref resultCols);

			int recno = 0;

			while (cursor.Next() != -1)
			{
				DataRow row = schema.NewRow();

				row["Recno"]			= recno++;
				row["CatalogName"]		= null;
				row["SchemaName"]		= null;
				row["TableName"]		= ((FbCursor)cursor).GetValue(0).ToString().Trim();
				row["ColumnName"]		= ((FbCursor)cursor).GetValue(1).ToString().Trim();
				row["ColumnPosition"]	= ((FbCursor)cursor).GetValue(2);
				row["ColumnType"]		= columnType;
				row["ColumnDataType"]	= ((FbCursor)cursor).GetValue(3);
				row["ColumnTypeName"]	= String.Empty;
				row["ColumnSubtype"]	= ((FbCursor)cursor).GetValue(4);
				row["ColumnLength"]		= ((FbCursor)cursor).GetValue(5);
				row["ColumnPrecision"]	= ((FbCursor)cursor).GetValue(6);
				row["ColumnScale"]		= ((FbCursor)cursor).GetValue(7);

				if (((FbCursor)cursor).GetValue(8) == DBNull.Value)
				{
					row["ColumnNullable"] = true;
				}
				else
				{
					row["ColumnNullable"] = false;
				}

				schema.Rows.Add(row);
			}

			cursor.Release();
			select.Release();

			return schema;
		}

		public DataTable GetIndices(string tableName, IndexType indexType)
		{
			switch (indexType)
			{
				case IndexType.PrimaryKey:
					return this.GetPrimaryKeyIndexes(tableName);

				case IndexType.Unique:
					return this.GetUniqueIndexes(tableName);

				default:
					return this.GetNonUniqueIndexes(tableName);
			}
		}

		public DataTable GetProcedures(string spName, ProcedureType procType)
		{
			StringBuilder	sql			= new StringBuilder();
			StringBuilder	where		= new StringBuilder();
			FbCommand		select		= new FbCommand(this.connection);
			ISQLCursor		cursor		= null;
#if (DIAMONDBACK)
            DataTable		schema		= BdpMetaDataHelper.GetProcedures();
#else
			DataTable		schema		= BdpMetaDataHelper.CreateProcedureTable();
#endif
			short			resultCols	= 0;

			sql.Append(
				@"SELECT " +
				"rdb$procedure_name AS ProcName, " +
				"rdb$procedure_inputs AS Inputs, " +
				"rdb$procedure_outputs AS Outputs " +
				"FROM " +
				"rdb$procedures");

			if (spName != null && spName.Length > 0)
			{
				where.AppendFormat("rdb$procedure_name = '{0}'", spName);
			}

			if (where.Length > 0)
			{
				sql.AppendFormat(" WHERE {0} ", where.ToString());
			}

			sql.Append(" ORDER BY rdb$procedure_name");

			// Prepare and execute the command
			select.Prepare(sql.ToString(), 0);
			select.Execute(out cursor, ref resultCols);

			int recno = 0;

			while (cursor.Next() != -1)
			{
				DataRow row = schema.NewRow();

				row["Recno"]		= recno++;
				row["CatalogName"]	= null;
				row["SchemaName"]	= null;
				row["ProcName"]		= ((FbCursor)cursor).GetValue(0).ToString().Trim();
				row["ProcType"]		= procType;
				row["InParams"]		= ((FbCursor)cursor).GetValue(1);
				row["OutParams"]	= ((FbCursor)cursor).GetValue(2);
				
				schema.Rows.Add(row);
			}

			cursor.Release();
			select.Release();

			return schema;
		}

		public DataTable GetProcedureParams(string spName, string paramName)
		{
			StringBuilder	sql			= new StringBuilder();
			StringBuilder	where		= new StringBuilder();
			FbCommand		select		= new FbCommand(this.connection);
			ISQLCursor		cursor		= null;
			short			resultCols	= 0;
#if (DIAMONDBACK)
            DataTable schema = BdpMetaDataHelper.GetProcedureParams();
#else
			DataTable schema = BdpMetaDataHelper.CreateProcedureParamsTable();
#endif

			sql.Append(
				@"SELECT " +
				"pp.rdb$procedure_name AS ProcName, " +
				"pp.rdb$parameter_name AS ParamName, " +
				"pp.rdb$parameter_type AS ParamType, " +
				"pp.rdb$parameter_number AS ParamPosition, " +
				"fld.rdb$field_type AS ParamDataType, " +
				"fld.rdb$field_sub_type AS ParamSubType, " +
				"fld.rdb$field_length AS ParamSize, " +
				"fld.rdb$field_precision AS ParamPrecision, " +
				"fld.rdb$field_scale AS ParamScale " +
				"FROM " +
				"rdb$procedure_parameters pp " +
				"left join rdb$fields fld ON pp.rdb$field_source = fld.rdb$field_name " +
				"left join rdb$character_sets cs ON cs.rdb$character_set_id = fld.rdb$character_set_id " +
				"left join rdb$collations coll ON (coll.rdb$collation_id = fld.rdb$collation_id AND coll.rdb$character_set_id = fld.rdb$character_set_id) ");

			if (spName != null && spName.Length > 0)
			{
				where.AppendFormat("pp.rdb$procedure_name = '{0}'", spName);
			}

			if (paramName != null && paramName.Length > 0)
			{
				if (where.Length > 0)
				{
					where.Append(" AND ");
				}

				where.AppendFormat("pp.rdb$parameter_name = '{0}'", paramName);
			}

			if (where.Length > 0)
			{
				sql.AppendFormat(" WHERE {0} ", where.ToString());
			}

			// Prepare and execute the command
			select.Prepare(sql.ToString(), 0);
			select.Execute(out cursor, ref resultCols);

			int recno = 0;

			while (cursor.Next() != -1)
			{
				DataRow row = schema.NewRow();

				int blrType		= 0;
				int subType		= 0;
				int length		= 0;
				int precision	= 0;
				int scale		= 0;
				int direction	= 0;
				
				if (((FbCursor)cursor).GetValue(4) != null && 
					((FbCursor)cursor).GetValue(4) != DBNull.Value)
				{
					blrType = Convert.ToInt32(((FbCursor)cursor).GetValue(4));
				}
				if (((FbCursor)cursor).GetValue(5) != null && 
					((FbCursor)cursor).GetValue(5) != DBNull.Value)
				{
					subType = Convert.ToInt32(((FbCursor)cursor).GetValue(5));
				}
				if (((FbCursor)cursor).GetValue(6) != null && 
					((FbCursor)cursor).GetValue(6) != DBNull.Value)
				{
					length = Convert.ToInt32(((FbCursor)cursor).GetValue(6));
				}
				if (((FbCursor)cursor).GetValue(7) != null && 
					((FbCursor)cursor).GetValue(7) != DBNull.Value)
				{
					precision = Convert.ToInt32(((FbCursor)cursor).GetValue(7));
				}
				if (((FbCursor)cursor).GetValue(8) != null && 
					((FbCursor)cursor).GetValue(8) != DBNull.Value)
				{
					scale = Convert.ToInt32(((FbCursor)cursor).GetValue(8));
				}
				if (((FbCursor)cursor).GetValue(2) != null && 
					((FbCursor)cursor).GetValue(2) != DBNull.Value)
				{
					direction = Convert.ToInt32(((FbCursor)cursor).GetValue(2));
				}
			
				DbDataType paramDbType = TypeHelper.GetDbDataType(blrType, subType, scale);				

				row["Recno"]			= recno++;
				row["CatalogName"]		= null;
				row["SchemaName"]		= null;
				row["ProcName"]			= ((FbCursor)cursor).GetValue(0).ToString().Trim();
				row["ParamName"]		= ((FbCursor)cursor).GetValue(1).ToString().Trim();
				if (direction == 0)
				{
					row["ParamType"] = ParameterDirection.Input;
				}
				else if (direction == 1)
				{
					row["ParamType"] = ParameterDirection.Output;
				}
				row["ParamPosition"]	= Convert.ToInt32(((FbCursor)cursor).GetValue(3));
				row["ParamDataType"]	= BdpTypeHelper.GetBdpType(paramDbType);
				row["ParamSubType"]		= BdpTypeHelper.GetBdpSubType(paramDbType);
				row["ParamTypeName"]	= TypeHelper.GetDataTypeName(paramDbType);
				row["ParamLength"]		= length;
				row["ParamPrecision"]	= precision;
				row["ParamScale"]		= Convert.ToInt16(scale);
				row["ParamNullable"]	= true;
			
				schema.Rows.Add(row);
			}

			cursor.Release();
			select.Release();

			return schema;
		}

		#endregion

		#region Private Methods

		private DataTable GetNonUniqueIndexes(string tableName)
		{
			StringBuilder	sql			= new StringBuilder();
			StringBuilder	where		= new StringBuilder();
			FbCommand		select		= new FbCommand(this.connection);
			ISQLCursor		cursor		= null;
#if (DIAMONDBACK)
			DataTable		schema		= BdpMetaDataHelper.GetIndices();
#else
            DataTable		schema		= BdpMetaDataHelper.GetIndexTable();
#endif
			short			resultCols	= 0;

			sql.Append(
				@"SELECT " +
				"idx.rdb$relation_name AS TableName, " +
				"idx.rdb$index_name AS IndexName, " +
				"seg.rdb$field_name AS ColumnName, " +
				"seg.rdb$field_position AS FieldPosition, " +
				"idx.rdb$index_type AS IndexType, " +
				"idx.rdb$index_inactive AS InactiveIndex, " +
				"idx.rdb$unique_flag AS IsUnique, " +
				"idx.rdb$description AS Description " +
				"FROM " +
				"rdb$indices idx " +
				"left join rdb$index_segments seg ON idx.rdb$index_name = seg.rdb$index_name ");

			if (tableName != null && tableName.Length > 0)
			{
				where.AppendFormat("rdb$relation_name = '{0}' AND idx.rdb$unique_flag IS NULL", tableName);
			}

			if (where.Length > 0)
			{
				sql.AppendFormat(" WHERE {0} ", where.ToString());
			}

			sql.Append(" ORDER BY idx.rdb$relation_name, idx.rdb$index_name, seg.rdb$field_position");

			/*
			RecNo
			CatalogName
			SchemaName
			TableName
			IndexName
			ColumnName
			ColumnPosition
			PKeyName
			IndexType
			SortOrder
			Filter
			*/		

			// Prepare and execute the command
			select.Prepare(sql.ToString(), 0);
			select.Execute(out cursor, ref resultCols);

			int recno = 0;

			while (cursor.Next() != -1)
			{
				DataRow row = schema.NewRow();

				row["Recno"]			= recno++;
				row["CatalogName"]		= null;
				row["SchemaName"]		= null;
				row["TableName"]		= ((FbCursor)cursor).GetValue(0).ToString().Trim();
				row["IndexName"]		= ((FbCursor)cursor).GetValue(1).ToString().Trim();
				row["ColumnName"]		= ((FbCursor)cursor).GetValue(2).ToString().Trim();
				row["ColumnPosition"]	= ((FbCursor)cursor).GetValue(3);
				row["PKeyName"]			= String.Empty;
				row["IndexType"]		= IndexType.NonUnique;
				row["SortOrder"]		= String.Empty;
				row["Filter"]			= String.Empty;

				schema.Rows.Add(row);
			}

			cursor.Release();
			select.Release();

			return schema;
		}

		private DataTable GetPrimaryKeyIndexes(string tableName)
        {
#if (DIAMONDBACK)
            return BdpMetaDataHelper.GetIndices();
#else
            return BdpMetaDataHelper.CreateIndexTable();
#endif
		}

		private DataTable GetUniqueIndexes(string tableName)
		{
#if (DIAMONDBACK)
            return BdpMetaDataHelper.GetIndices();
#else
            return BdpMetaDataHelper.CreateIndexTable();
#endif
		}

		#endregion

		#region Private Methods

		private string GetSchemaCommandText()
		{
			System.Text.StringBuilder sql = new System.Text.StringBuilder();

			sql.AppendFormat(
				"SELECT " + 
				"fld.rdb$computed_blr		as computed_blr,\n"		+
				"fld.rdb$computed_source	as computed_source,\n"	+
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

		#endregion
	}
}
