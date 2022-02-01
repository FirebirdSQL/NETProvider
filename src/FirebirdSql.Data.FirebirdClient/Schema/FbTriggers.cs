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

internal class FbTriggers : FbSchema
{
	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		var sql = new StringBuilder();
		var where = new StringBuilder();

		sql.Append(
			@"SELECT
					null AS TABLE_CATALOG,
					null AS TABLE_SCHEMA,
					rdb$relation_name AS TABLE_NAME,
					rdb$trigger_name AS TRIGGER_NAME,
					rdb$system_flag AS IS_SYSTEM_TRIGGER,
					rdb$trigger_type AS TRIGGER_TYPE,
					rdb$trigger_inactive AS IS_INACTIVE,
					rdb$trigger_sequence AS SEQUENCE,
					rdb$trigger_source AS SOURCE,
					rdb$description AS DESCRIPTION
				FROM rdb$triggers");

		if (restrictions != null)
		{
			var index = 0;

			/* TABLE_CATALOG */
			if (restrictions.Length >= 1 && restrictions[0] != null)
			{
			}

			/* TABLE_SCHEMA */
			if (restrictions.Length >= 2 && restrictions[1] != null)
			{
			}

			/* TABLE_NAME */
			if (restrictions.Length >= 3 && restrictions[2] != null)
			{
				where.AppendFormat("rdb$relation_name = @p{0}", index++);
			}

			/* TRIGGER_NAME */
			if (restrictions.Length >= 4 && restrictions[3] != null)
			{
				if (where.Length > 0)
				{
					where.Append(" AND ");
				}

				where.AppendFormat("rdb$trigger_name = @p{0}", index++);
			}
		}

		if (where.Length > 0)
		{
			sql.AppendFormat(" WHERE {0} ", where.ToString());
		}

		sql.Append(" ORDER BY TABLE_NAME, TRIGGER_NAME");

		return sql;
	}

	protected override void ProcessResult(DataTable schema)
	{
		schema.BeginLoadData();

		foreach (DataRow row in schema.Rows)
		{
			if (row["IS_SYSTEM_TRIGGER"] == DBNull.Value ||
				Convert.ToInt32(row["IS_SYSTEM_TRIGGER"], CultureInfo.InvariantCulture) == 0)
			{
				row["IS_SYSTEM_TRIGGER"] = false;
			}
			else
			{
				row["IS_SYSTEM_TRIGGER"] = true;
			}
		}

		schema.EndLoadData();
		schema.AcceptChanges();
	}

	#endregion
}
