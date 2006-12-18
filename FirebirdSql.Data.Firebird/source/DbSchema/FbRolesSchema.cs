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
	internal class FbRolesSchema : FbAbstractDbSchema
	{
		#region CONSTRUCTORS

		public FbRolesSchema() : base("Roles")
		{
		}

		#endregion

		#region ADD_METHODS

		public override void AddTables()
		{
			AddTable("rdb$roles");
		}

		public override void AddRestrictionColumns()
		{
			AddRestrictionColumn("rdb$role_name", "ROLE_NAME", null);
		}

		public override void AddDataColumns()
		{
			AddDataColumn("rdb$owner_name"	, "OWNER_NAME");
		}

		public override void AddJoins()
		{
		}

		public override void AddOrderByColumns()
		{
			AddOrderBy("rdb$role_name");
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