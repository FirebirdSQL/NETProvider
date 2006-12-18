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
	internal class FbDomainsSchema : FbAbstractDbSchema
	{
		#region CONSTRUCTORS

		public FbDomainsSchema() : base("Domains")
		{
		}

		#endregion

		#region ADD_METHODS

		public override void AddTables()
		{
			AddTable("rdb$fields");
		}

		public override void AddRestrictionColumns()
		{
			AddRestrictionColumn("rdb$field_name", "DOMAIN_NAME", null);
		}

		public override void AddDataColumns()
		{
			AddDataColumn("rdb$field_type"		, "DOMAIN_DATA_TYPE");
			AddDataColumn("rdb$field_sub_type"	, "DOMAIN_SUB_TYPE");
			AddDataColumn("rdb$field_length"	, "DOMAIN_SIZE");
			AddDataColumn("rdb$field_precision"	, "DOMAIN_PRECISION");
			AddDataColumn("rdb$field_scale"		, "DOMAIN_SCALE");
			AddDataColumn("rdb$null_flag"		, "IS_NULLABLE");
			AddDataColumn("rdb$dimensions"		, "ARRAY_DIMENSIONS");
			AddDataColumn("rdb$description"		, "DESCRIPTION");
		}

		public override void AddJoins()
		{
		}

		public override void AddOrderByColumns()
		{
			AddOrderBy("rdb$field_name");
		}

		public override void AddWhereFilters()
		{
			AddWhereFilter("not rdb$field_name starting with 'RDB$'");
		}

		#endregion

		#region PARSE_METHODS

		public override object[] ParseRestrictions(object[] restrictions)
		{
			object[] parsed = restrictions;

			return parsed;
		}

		#endregion
	}
}