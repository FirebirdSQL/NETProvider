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

using FirebirdSql.Data.Firebird.Gds;

namespace FirebirdSql.Data.Firebird.DbSchema
{
	internal class FbColumnsSchema : FbAbstractDbSchema
	{
		#region Constructors

		public FbColumnsSchema() : base("Columns")
		{
		}

		#endregion

		#region Add Methods

		public override void AddTables()
		{
			AddTable("rdb$relation_fields rfr");
		}

		public override void AddRestrictionColumns()
		{
			AddRestrictionColumn("rfr.rdb$relation_name", "TABLE_NAME", null);
			AddRestrictionColumn("rfr.rdb$field_name"	, "COLUMN_NAME", null);
		}

		public override void AddDataColumns()
		{
			AddDataColumn("fld.rdb$field_type"		, "COLUMN_DATA_TYPE");
			AddDataColumn("fld.rdb$field_sub_type"	, "COLUMN_SUB_TYPE");
			AddDataColumn("fld.rdb$field_length"	, "COLUMN_SIZE");
			AddDataColumn("fld.rdb$field_precision"	, "NUMERIC_PRECISION");
			AddDataColumn("fld.rdb$field_scale"		, "NUMERIC_SCALE");
			AddDataColumn("rfr.rdb$field_position"	, "COLUMN_ORDINAL");
			AddDataColumn("fld.rdb$default_source"	, "COLUMN_DEFAULT");
			AddDataColumn("fld.rdb$computed_source" , "COMPUTED_SOURCE");
			AddDataColumn("rfr.rdb$null_flag"		, "IS_NULLABLE");
			AddDataColumn("0"						, "IS_READONLY");
			AddDataColumn(
				"(select count(*)\n" +
				"from rdb$relation_constraints rel, rdb$indices idx, rdb$index_segments seg\n"	+
				"where rel.rdb$constraint_type = 'PRIMARY KEY'\n"	+
				"and rel.rdb$index_name = idx.rdb$index_name\n"	+
				"and idx.rdb$index_name = seg.rdb$index_name\n"	+
				"and rel.rdb$relation_name = rfr.rdb$relation_name\n"	+
				"and seg.rdb$field_name = rfr.rdb$field_name)", "PRIMARY_KEY");
			AddDataColumn(
				"(select count(*)\n"	+
				"from rdb$relation_constraints rel, rdb$indices idx, rdb$index_segments seg\n" +
				"where rel.rdb$constraint_type = 'UNIQUE'\n"	+
				"and rel.rdb$index_name = idx.rdb$index_name\n"	+
				"and idx.rdb$index_name = seg.rdb$index_name\n"	+
				"and rel.rdb$relation_name = rfr.rdb$relation_name\n" +
				"and seg.rdb$field_name = rfr.rdb$field_name)", "UNIQUE_KEY");
			AddDataColumn("cs.rdb$character_set_name"		, "CHARACTER_SET_NAME");
			AddDataColumn("coll.rdb$collation_name"			, "COLLATION_NAME");
			AddDataColumn("rfr.rdb$description"				, "DESCRIPTION");
		}

		public override void AddJoins()
		{
			AddJoin("left join", "rdb$fields fld"		, "rfr.rdb$field_source = fld.rdb$field_name");
			AddJoin("left join", "rdb$character_sets cs", "cs.rdb$character_set_id = fld.rdb$character_set_id");
			AddJoin("left join", "rdb$collations coll"	, "coll.rdb$collation_id = fld.rdb$collation_id AND coll.rdb$character_set_id = fld.rdb$character_set_id");
		}

		public override void AddOrderByColumns()
		{
			AddOrderBy("rfr.rdb$relation_name");
			AddOrderBy("rfr.rdb$field_position");
		}

		public override void AddWhereFilters()
		{
		}

		#endregion

		#region Parse Methods

		public override object[] ParseRestrictions(object[] restrictions)
		{
			object[] parsed = restrictions;

			return parsed;
		}

		#endregion

		#region Protected Methods

		protected override DataTable ProcessResult(DataTable schema)
		{
			DataColumn providerType = new DataColumn("PROVIDER_TYPE");
			providerType.Caption	= "Provider Type";
			providerType.DataType	= typeof(FbDbType);

			schema.Columns.Add(providerType);

			schema.BeginLoadData();

			foreach (DataRow row in schema.Rows)
			{
				int blrType = Convert.ToInt32(row["COLUMN_DATA_TYPE"]);
				
				int subType	= 0;
				if (row["COLUMN_SUB_TYPE"] != System.DBNull.Value)
				{
					subType	= Convert.ToInt32(row["COLUMN_SUB_TYPE"]);
				}
				
				int scale = 0 ;
				if (row["NUMERIC_SCALE"] != System.DBNull.Value)
				{
					scale = Convert.ToInt32(row["NUMERIC_SCALE"]);
				}

				if (row["IS_NULLABLE"] == DBNull.Value)
				{
					row["IS_NULLABLE"] = true;
				}
				else
				{
					row["IS_NULLABLE"] = false;
				}

				row["PROVIDER_TYPE"] = GdsField.GetFbTypeFromBlr(blrType, subType, scale);
			}

			schema.EndLoadData();
			schema.AcceptChanges();
            
			return schema;
		}

		#endregion
	}
}
