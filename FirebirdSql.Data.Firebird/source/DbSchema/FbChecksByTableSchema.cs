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
	internal class FbChecksByTableSchema : FbAbstractDbSchema
	{
		#region CONSTRUCTORS

		public FbChecksByTableSchema() : base("CheckConstraintsByTable")
		{
		}

		#endregion

		#region ADD_METHODS

		public override void AddTables()
		{
			AddTable("rdb$relation_constraints chktb");
		}

		public override void AddRestrictionColumns()
		{
			AddRestrictionColumn("chktb.rdb$relation_name"	, "TABLE_NAME", null);
			AddRestrictionColumn("chktb.rdb$constraint_name", "CONSTRAINT_NAME", null);
		}

		public override void AddDataColumns()
		{
			AddDataColumn("trig.rdb$trigger_source"	, "CHECK_CLAUSULE");
			AddDataColumn("trig.rdb$description"	, "DESCRIPTION");
		}

		public override void AddJoins()
		{
			AddJoin("left join", "rdb$check_constraints chk", "chktb.rdb$constraint_name = chk.rdb$constraint_name");
			AddJoin("left join", "rdb$triggers trig"		, "chk.rdb$trigger_name = trig.rdb$trigger_name");
		}

		public override void AddOrderByColumns()
		{
			AddOrderBy("chktb.rdb$relation_name");
			AddOrderBy("chktb.rdb$constraint_name");
		}

		public override void AddWhereFilters()
		{
			AddWhereFilter("chktb.rdb$constraint_type = 'CHECK'");
		}

		#endregion

		#region PARSE_METHODS

		public override object[] ParseRestrictions(object[] restrictions)
		{
			object[] parsed = restrictions;

			if (parsed != null)
			{
				if (parsed.Length == 7 && parsed[6] != null)
				{
					switch (parsed[6].ToString().ToUpper())
					{
						case "UNIQUE":
							parsed[3] = "u";
							break;

						case "PRIMARY KEY":
							parsed[3] = "p";
							break;

						case "FOREIGN KEY":
							parsed[3] = "f";
							break;

						case "CHECK":
							parsed[3] = "c";
							break;
					}
				}
			}

			return parsed;
		}

		#endregion
	}
}