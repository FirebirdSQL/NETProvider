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
using System.Data;
using System.Globalization;
using System.Text;

namespace FirebirdSql.Data.Schema;

internal class FbCharacterSets : FbSchema
{
	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		var sql = new StringBuilder();
		var where = new StringBuilder();

		sql.Append(
			@"SELECT
					null AS CHARACTER_SET_CATALOG,
					null AS CHARACTER_SET_SCHEMA,
					rdb$character_set_name AS CHARACTER_SET_NAME,
					rdb$character_set_id AS CHARACTER_SET_ID,
				    rdb$default_collate_name AS DEFAULT_COLLATION,
				    rdb$bytes_per_character AS BYTES_PER_CHARACTER,
				    rdb$description AS DESCRIPTION
				 FROM rdb$character_sets");

		if (restrictions != null)
		{
			var index = 0;

			/* CHARACTER_SET_CATALOG */
			if (restrictions.Length >= 1 && restrictions[0] != null)
			{
			}

			/* CHARACTER_SET_SCHEMA */
			if (restrictions.Length >= 2 && restrictions[1] != null)
			{
			}

			/* CHARACTER_SET_NAME */
			if (restrictions.Length >= 3 && restrictions[2] != null)
			{
				where.AppendFormat("rdb$character_set_name = @p{0}", index++);
			}
		}

		if (where.Length > 0)
		{
			sql.AppendFormat(" WHERE {0} ", where.ToString());
		}

		sql.Append(" ORDER BY CHARACTER_SET_NAME");

		return sql;
	}

	#endregion
}
