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
	internal class FbForeignKeysSchema : FbAbstractDbSchema
	{
		#region Constructors

		public FbForeignKeysSchema() : base("ForeignKeys")
		{
		}

		#endregion

		#region Add Methods

		public override void AddTables()
		{
			this.AddTable("rdb$relation_constraints rc");
		}

		public override void AddRestrictionColumns()
		{
			this.AddRestrictionColumn(
				"rc.rdb$relation_name", 
				"PK_TABLE_NAME", 
				null);
			
			this.AddRestrictionColumn(
				"pidx.rdb$relation_name", 
				"FK_TABLE_NAME", 
				null);
		}

		public override void AddDataColumns()
		{
			this.AddDataColumn("fseg.rdb$field_name"	, "PK_COLUMN_NAME");
			this.AddDataColumn("rc.rdb$constraint_name"	, "FK_NAME");
			this.AddDataColumn("pseg.rdb$field_name"	, "FK_COLUMN_NAME");
			this.AddDataColumn("fseg.rdb$field_position", "ORDINAL");
			this.AddDataColumn("ref.rdb$match_option"	, "MATCH_OPTION");
			this.AddDataColumn("ref.rdb$update_rule"	, "UPDATE_RULE");
			this.AddDataColumn("ref.rdb$delete_rule"	, "DELETE_RULE");
			this.AddDataColumn("rc.rdb$deferrable"		, "IS_DEFERRABLE");			
			this.AddDataColumn(
				"rc.rdb$initially_deferred"	, 
				"INITIALLY_DEFERRED");
		}

		public override void AddJoins()
		{
			this.AddJoin(
				"inner join", 
				"rdb$indices fidx", 
				"(rc.rdb$index_name = fidx.rdb$index_name AND rc.rdb$constraint_type = 'FOREIGN KEY')");

			this.AddJoin(
				"inner join", 
				"rdb$ref_constraints ref", 
				"rc.rdb$constraint_name = ref.rdb$constraint_name");

			this.AddJoin(
				"inner join", 
				"rdb$index_segments fseg", 
				"fidx.rdb$index_name = fseg.rdb$index_name");
			
			this.AddJoin(
				"inner join", 
				"rdb$indices pidx", 
				"fidx.rdb$foreign_key = pidx.rdb$index_name");

			this.AddJoin(
				"inner join", 
				"rdb$index_segments pseg", 
				"(fidx.rdb$foreign_key = pseg.rdb$index_name AND pseg.rdb$field_position=fseg.rdb$field_position)");
		}

		public override void AddOrderByColumns()
		{
			this.AddOrderBy("rc.rdb$constraint_name");
			this.AddOrderBy("fseg.rdb$field_position");
		}

		public override void AddWhereFilters()
		{
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