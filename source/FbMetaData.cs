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
using System.Text;

namespace FirebirdSql.Data.Firebird
{
	/// <include file='xmldoc/fbmetadata.xml' path='doc/member[@name="T:FbMetaData"]/*'/>
	internal class FbMetaData
	{		
		/// <include file='xmldoc/fbmetadata.xml' path='doc/member[@name="M:GetSchemaColumns(FirebirdSql.Data.Firebird.FbTransaction,System.String,System.String,System.Boolean,System.Boolean)"]/*'/>
		public static FbStatement GetSchemaColumns(FbTransaction transaction, 
			string	tableCat,
			string	tableSchema,
			bool	addTableParam,
			bool	addColumnParam)
		{
			StringBuilder sql = new StringBuilder();

			sql.AppendFormat(
				"select NULL as table_cat,NULL as table_schem,\n"	+
					"rfr.rdb$relation_name     as table_name,\n"	+
					"rfr.rdb$field_name        as colum_name,\n"	+
					"rfr.rdb$field_position    as column_ordinal,\n"+
					"fld.rdb$field_length      as column_size,\n"	+
					"fld.rdb$field_precision   as numeric_precision,\n"	+
					"fld.rdb$field_scale       as numeric_scale,\n"	+
					"fld.rdb$field_type        as data_type,\n"		+
					"fld.rdb$field_sub_type    as data_sub_type,\n"	+
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
				
			if (addTableParam)
			{
				sql.Append("\n and rfr.rdb$relation_name = ?");
			}
	
			if (addColumnParam)
			{
				sql.Append("\n and rfr.rdb$field_name = ?");
			}

			sql.Append("\n order by rfr.rdb$relation_name, rfr.rdb$field_position");
						
			try
			{				
				FbStatement statement = new FbStatement();

				statement.CommandText = sql.ToString();
				statement.Connection  = transaction.Connection;
				statement.Transaction = transaction;

				if (addTableParam)
				{
					statement.Parameters.Add("@TABLE_NAME", FbType.VarChar);
				}
				
				if (addColumnParam)
				{
					statement.Parameters.Add("@COLUMN_NAME", FbType.VarChar);
				}

				return statement;
			}
			catch (FbException ex)
			{
				throw ex;
			}
		}
	}
}
