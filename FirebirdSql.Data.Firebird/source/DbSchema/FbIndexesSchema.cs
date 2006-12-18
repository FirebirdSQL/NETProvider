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
	internal class FbIndexesSchema : FbAbstractDbSchema
	{
		#region CONSTRUCTORS

		public FbIndexesSchema() : base("Indexes")
		{
		}

		#endregion

		#region ADD_METHODS

		public override void AddTables()
		{
			AddTable("rdb$indices");
		}

		public override void AddRestrictionColumns()
		{
			AddRestrictionColumn("rdb$relation_name", "TABLE_NAME", null);
			AddRestrictionColumn("rdb$index_name"	, "INDEX_NAME", null);
		}

		public override void AddDataColumns()
		{
			AddDataColumn("rdb$index_type"		, "INDEX_TYPE");
			AddDataColumn("rdb$index_inactive"	, "INDEX_INACTIVE");
			AddDataColumn("rdb$system_flag"		, "IS_SYSTEM_INDEX");
			AddDataColumn("rdb$statistics"		, "INDEX_STATISTICS");
			AddDataColumn("rdb$unique_flag"		, "IS_UNIQUE");
			AddDataColumn("rdb$description"		, "DESCRIPTION");
		}

		public override void AddJoins()
		{			
		}

		public override void AddOrderByColumns()
		{
			AddOrderBy("rdb$relation_name");
			AddOrderBy("rdb$index_name");
		}

		public override void AddWhereFilters()
		{
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