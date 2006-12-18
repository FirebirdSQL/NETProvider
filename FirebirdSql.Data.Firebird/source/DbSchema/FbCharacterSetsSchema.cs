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
	internal class FbCharacterSetsSchema : FbAbstractDbSchema
	{
		#region CONSTRUCTORS

		public FbCharacterSetsSchema() : base("CharacterSets")
		{
		}

		#endregion

		#region ADD_METHODS

		public override void AddTables()
		{
			AddTable("rdb$character_sets");
		}

		public override void AddRestrictionColumns()
		{
			AddRestrictionColumn("rdb$character_set_name", "CHARACTER_SET_NAME", null);			
			AddRestrictionColumn("rdb$character_set_id"	 , "CHARACTER_SET_ID", null);
		}

		public override void AddDataColumns()
		{
			AddDataColumn("rdb$default_collate_name", "DEFAULT_COLLATION");
			AddDataColumn("rdb$bytes_per_character"	, "BYTES_PER_CHARACTER");
			AddDataColumn("rdb$description"			, "DESCRIPTION");
		}

		public override void AddJoins()
		{
		}

		public override void AddOrderByColumns()
		{
			AddOrderBy("rdb$character_set_name");
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