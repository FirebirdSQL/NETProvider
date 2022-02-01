/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Globalization;
using System.Data;
using System.Text;

namespace FirebirdSql.Data.Schema;

internal class FbCollations : FbSchema
{
	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		var sql = new StringBuilder();
		var where = new StringBuilder();

		sql.Append(
			@"SELECT
					null AS COLLATION_CATALOG,
					null AS COLLATION_SCHEMA,
					coll.rdb$collation_name AS COLLATION_NAME,
					cs.rdb$character_set_name AS CHARACTER_SET_NAME,
					coll.rdb$description AS DESCRIPTION
				FROM rdb$collations coll
					LEFT JOIN rdb$character_sets cs ON coll.rdb$character_set_id = cs.rdb$character_set_id");

		if (restrictions != null)
		{
			var index = 0;

			/* COLLATION_CATALOG */
			if (restrictions.Length >= 1 && restrictions[0] != null)
			{
			}

			/* COLLATION_SCHEMA */
			if (restrictions.Length >= 2 && restrictions[1] != null)
			{
			}

			/* COLLATION_NAME */
			if (restrictions.Length >= 3 && restrictions[2] != null)
			{
				where.AppendFormat("coll.rdb$collation_name = @p{0}", index++);
			}
		}

		if (where.Length > 0)
		{
			sql.AppendFormat(" WHERE {0} ", where.ToString());
		}

		sql.Append(" ORDER BY CHARACTER_SET_NAME, COLLATION_NAME");

		return sql;
	}

	#endregion
}
