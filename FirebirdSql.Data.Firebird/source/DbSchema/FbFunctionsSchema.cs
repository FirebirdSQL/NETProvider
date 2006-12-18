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
	internal class FbFunctionsSchema : FbAbstractDbSchema
	{
		#region CONSTRUCTORS

		public FbFunctionsSchema() : base("Functions")
		{
		}

		#endregion

		#region ADD_METHODS

		public override void AddTables()
		{
			AddTable("rdb$functions");
		}

		public override void AddRestrictionColumns()
		{
			AddRestrictionColumn("rdb$function_name", "FUNCTION_NAME"	, null);
			AddRestrictionColumn("rdb$system_flag"	, "SYSTEM_FUNCTION"	, null);
		}

		public override void AddDataColumns()
		{
			AddDataColumn("rdb$function_type"	, "FUNCTION_TYPE");
			AddDataColumn("rdb$query_name"		, "QUERY_NAME");
			AddDataColumn("rdb$module_name"		, "FUNCTION_MODULE_NAME");
			AddDataColumn("rdb$entrypoint"		, "FUNCTION_ENTRY_POINT");
			AddDataColumn("rdb$return_argument"	, "RETURN_ARGUMENT");
			AddDataColumn("rdb$description"		, "DESCRIPTION");
		}

		public override void AddJoins()
		{
		}

		public override void AddOrderByColumns()
		{
			AddOrderBy("rdb$function_name");
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