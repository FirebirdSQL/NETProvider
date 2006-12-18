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
	internal class FbViewsSchema : FbAbstractDbSchema
	{
		#region Constructors

		public FbViewsSchema() : base("Views")
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
		}

		public override void AddDataColumns()
		{
			this.AddDataColumn("rel.rdb$view_source", "DEFINITION");
			this.AddDataColumn("rel.rdb$description", "DESCRIPTION");
		}

		public override void AddJoins()
		{
		}

		public override void AddOrderByColumns()
		{
			this.AddOrderBy("rel.rdb$relation_name");
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