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
	internal class FbPrimaryKeysSchema : FbAbstractDbSchema
	{
		#region Constructors

		public FbPrimaryKeysSchema() : base("PrimaryKeys")
		{
		}

		#endregion

		#region Add Methods

		public override void AddTables()
		{
			this.AddTable("rdb$relation_constraints rel");
		}

		public override void AddRestrictionColumns()
		{
			this.AddRestrictionColumn(
				"rel.rdb$relation_name", 
				"TABLE_NAME", 
				null);

			this.AddRestrictionColumn(
				"rel.rdb$constraint_name", 
				"PK_NAME", 
				null);
		}

		public override void AddDataColumns()
		{
			this.AddRestrictionColumn(
				"seg.rdb$field_name", 
				"COLUMN_NAME",
				null);
			
			this.AddRestrictionColumn(
				"seg.rdb$field_position", 
				"ORDINAL",
				null);
		}

		public override void AddJoins()
		{
			this.AddJoin(
				"left join", 
				"rdb$indices idx", 
				"rel.rdb$index_name = idx.rdb$index_name");
			
			this.AddJoin(
				"left join", 
				"rdb$index_segments seg", 
				"idx.rdb$index_name = seg.rdb$index_name");
		}

		public override void AddOrderByColumns()
		{
			this.AddOrderBy("rel.rdb$relation_name");
			this.AddOrderBy("rel.rdb$constraint_name");
			this.AddOrderBy("seg.rdb$field_position");
		}

		public override void AddWhereFilters()
		{
			this.AddWhereFilter("rel.rdb$constraint_type = 'PRIMARY KEY'");
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