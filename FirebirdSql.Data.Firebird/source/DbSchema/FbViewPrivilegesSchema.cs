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
	internal class FbViewPrivilegesSchema : FbAbstractDbSchema
	{
		#region Constructors

		public FbViewPrivilegesSchema() : base("View_Privileges")
		{
		}

		#endregion

		#region Add Methods

		public override void AddTables()
		{
			this.AddTable("rdb$user_privileges priv");
		}

		public override void AddRestrictionColumns()
		{
			this.AddRestrictionColumn(
				"priv.rdb$relation_name", "VIEW_NAME", null);

			this.AddRestrictionColumn(
				"priv.rdb$user", "GRANTEE", null);
			
			this.AddRestrictionColumn(
				"priv.rdb$grantor", "GRANTOR", null);
		}
		
		public override void AddDataColumns()
		{
			this.AddDataColumn("priv.rdb$privilege"		, "PRIVILEGE");
			this.AddDataColumn("priv.rdb$grant_option"	, "WITH_GRANT");
		}

		public override void AddJoins()
		{
			this.AddJoin("left join", "rdb$relations rel", "priv.rdb$relation_name = rel.rdb$relation_name AND rel.rdb$view_source IS NOT NULL");
		}

		public override void AddOrderByColumns()
		{
			this.AddOrderBy("priv.rdb$relation_name");
			this.AddOrderBy("priv.rdb$user");
		}

		public override void AddWhereFilters()
		{
			this.AddWhereFilter("priv.rdb$object_type = 0");
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