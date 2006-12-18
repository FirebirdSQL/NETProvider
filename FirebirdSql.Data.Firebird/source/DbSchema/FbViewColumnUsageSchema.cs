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

namespace FirebirdSql.Data.Firebird.DbSchema
{
	internal class FbViewColumnUsage : FbAbstractDbSchema
	{
		#region Constructors

		public FbViewColumnUsage() : base("ViewColumnUsage")
		{
		}

		#endregion

		#region Add Methods

		public override void AddTables()
		{
			this.AddTable("rdb$relations rel");
		}

		public override void AddRestrictionColumns()
		{
			this.AddRestrictionColumn("rel.rdb$relation_name", "VIEW_NAME", null);
			this.AddRestrictionColumn("rfr.rdb$field_name", "COLUMN_NAME", null);
		}

		public override void AddDataColumns()
		{
			this.AddDataColumn("fld.rdb$field_type"		, "COLUMN_DATA_TYPE");
			this.AddDataColumn("fld.rdb$field_sub_type"	, "COLUMN_SUB_TYPE");
			this.AddDataColumn("fld.rdb$field_length"	, "COLUMN_SIZE");
			this.AddDataColumn("fld.rdb$field_precision", "NUMERIC_PRECISION");
			this.AddDataColumn("fld.rdb$field_scale"	, "NUMERIC_SCALE");
			this.AddDataColumn("rfr.rdb$field_position"	, "COLUMN_ORDINAL");
			this.AddDataColumn("fld.rdb$default_source"	, "COLUMN_DEFAULT");
			this.AddDataColumn("fld.rdb$null_flag"		, "IS_NULLABLE");
			this.AddDataColumn("0"						, "IS_READONLY");
			this.AddDataColumn("rfr.rdb$description"	, "DESCRIPTION");
		}

		public override void AddJoins()
		{
			this.AddJoin("left join", "rdb$relation_fields rfr"	, "rel.rdb$relation_name = rfr.rdb$relation_name");
			this.AddJoin("left join", "rdb$fields fld"			, "rfr.rdb$field_source = fld.rdb$field_name");
		}

		public override void AddOrderByColumns()
		{
			this.AddOrderBy("rel.rdb$relation_name");
			this.AddOrderBy("rfr.rdb$field_position");
		}

		public override void AddWhereFilters()
		{
			this.AddWhereFilter("rel.rdb$view_source IS NOT NULL");
		}

		#endregion

		#region Parse Methods

		public override object[] ParseRestrictions(object[] restrictions)
		{
			object[] parsed = restrictions;

			return parsed;
		}

		#endregion
	}
}