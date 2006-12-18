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
	internal class FbProcedureParametersSchema : FbAbstractDbSchema
	{
		#region Constructors

		public FbProcedureParametersSchema() : base("ProcedureParameters")
		{
		}

		#endregion

		#region Add Methods

		public override void AddTables()
		{
			AddTable("rdb$procedure_parameters pp");
		}

		public override void AddRestrictionColumns()
		{
			AddRestrictionColumn("pp.rdb$procedure_name", "PROCEDURE_NAME", null);
			AddRestrictionColumn("pp.rdb$parameter_name", "PARAMETER_NAME", null);
		}

		public override void AddDataColumns()
		{
			AddDataColumn("pp.rdb$parameter_type"	, "PARAMETER_TYPE");			
			AddDataColumn("pp.rdb$parameter_number"	, "ORDINAL_POSITION");
			AddDataColumn("fld.rdb$field_type"		, "DATA_TYPE");
			AddDataColumn("fld.rdb$field_sub_type"	, "SUB_DATA_TYPE");
			AddDataColumn("fld.rdb$field_length"	, "COLUMN_SIZE");
			AddDataColumn("fld.rdb$field_precision"	, "NUMERIC_PRECISION");
			AddDataColumn("fld.rdb$field_scale"		, "NUMERIC_SCALE");			
			AddDataColumn("cs.rdb$character_set_name", "CHARACTER_SET_NAME");
			AddDataColumn("coll.rdb$collation_name"	, "COLLATION_NAME");
			AddDataColumn("pp.rdb$description"		, "DESCRIPTION");
		}

		public override void AddJoins()
		{
			AddJoin("left join", "rdb$fields fld", "pp.rdb$field_source = fld.rdb$field_name");
			AddJoin("left join", "rdb$character_sets cs", "cs.rdb$character_set_id = fld.rdb$character_set_id");
			AddJoin("left join", "rdb$collations coll"	, "coll.rdb$collation_id = fld.rdb$collation_id AND coll.rdb$character_set_id = fld.rdb$character_set_id");
		}

		public override void AddOrderByColumns()
		{
			AddOrderBy("pp.rdb$procedure_name");
			AddOrderBy("pp.rdb$parameter_type");
			AddOrderBy("pp.rdb$parameter_number");
		}

		public override void AddWhereFilters()
		{
		}

		#endregion

		#region Protected Methods

		public override object[] ParseRestrictions(object[] restrictions)
		{
			object[] parsed = restrictions;

			return parsed;
		}

		protected override DataTable ProcessResult(DataTable schema)
		{
			DataColumn providerType = new DataColumn("PROVIDER_TYPE");
			providerType.Caption	= "Provider Type";
			providerType.DataType	= typeof(FbDbType);

			schema.Columns.Add(providerType);

			schema.BeginLoadData();

			foreach (DataRow row in schema.Rows)
			{
				int blrType = Convert.ToInt32(row["DATA_TYPE"]);
				
				int subType	= 0;
				if (row["SUB_DATA_TYPE"] != System.DBNull.Value)
				{
					subType	= Convert.ToInt32(row["SUB_DATA_TYPE"]);
				}
				
				int scale = 0 ;
				if (row["NUMERIC_SCALE"] != System.DBNull.Value)
				{
					scale = Convert.ToInt32(row["NUMERIC_SCALE"]);
				}

				row["PROVIDER_TYPE"] = GdsField.GetFbTypeFromBlr(blrType, subType, scale);
			}

			schema.EndLoadData();
            
			return schema;
		}

		#endregion
	}
}