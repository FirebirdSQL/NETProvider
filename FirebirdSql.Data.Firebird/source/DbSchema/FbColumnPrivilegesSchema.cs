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
using System.Text.RegularExpressions;

namespace FirebirdSql.Data.Firebird.DbSchema
{
	internal class FbColumnPrivilegesSchema : FbAbstractDbSchema
	{
		#region CONSTRUCTORS

		public FbColumnPrivilegesSchema() : base("Column_Privileges")
		{
		}

		#endregion

		#region ADD_METHODS

		public override void AddTables()
		{
			AddTable("rdb$user_privileges");
		}

		public override void AddRestrictionColumns()
		{
			AddRestrictionColumn("rdb$relation_name", "TABLE_NAME"	, null);
			AddRestrictionColumn("rdb$field_name"	, "COLUMN_NAME"	, null);
			AddRestrictionColumn("rdb$user"			, "GRANTEE"		, null);
			AddRestrictionColumn("rdb$grantor"		, "GRANTOR"		, null);
		}

		public override void AddDataColumns()
		{
			AddDataColumn("rdb$privilege"	, "PRIVILEGE");
			AddDataColumn("rdb$grant_option", "WITH_GRANT");
		}

		public override void AddJoins()
		{
		}

		public override void AddOrderByColumns()
		{
			AddOrderBy("rdb$relation_name");
		}

		public override void AddWhereFilters()
		{
			AddWhereFilter("rdb$object_type = 0");
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