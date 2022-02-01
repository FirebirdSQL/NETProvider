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

internal class FbTables : FbSchema
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
					null AS TABLE_TYPE,
					rdb$system_flag AS IS_SYSTEM_TABLE,
					rdb$owner_name AS OWNER_NAME,
					rdb$description AS DESCRIPTION,
					rdb$view_source AS VIEW_SOURCE
				FROM rdb$relations");

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

			/* TABLE_TYPE */
			if (restrictions.Length >= 4 && restrictions[3] != null)
			{
				if (where.Length > 0)
				{
					where.Append(" AND ");
				}

				switch (restrictions[3].ToString())
				{
					case "VIEW":
						where.Append("rdb$view_source IS NOT NULL");
						break;

					case "SYSTEM TABLE":
						where.Append("rdb$view_source IS NULL and rdb$system_flag = 1");
						break;

					case "TABLE":
					default:
						where.Append("rdb$view_source IS NULL and rdb$system_flag = 0");
						break;
				}
			}
		}

		if (where.Length > 0)
		{
			sql.AppendFormat(" WHERE {0} ", where.ToString());
		}

		sql.Append(" ORDER BY IS_SYSTEM_TABLE, OWNER_NAME, TABLE_NAME");

		return sql;
	}

	protected override void ProcessResult(DataTable schema)
	{
		schema.BeginLoadData();

		foreach (DataRow row in schema.Rows)
		{
			row["TABLE_TYPE"] = "TABLE";
			if (row["IS_SYSTEM_TABLE"] == DBNull.Value ||
				Convert.ToInt32(row["IS_SYSTEM_TABLE"], CultureInfo.InvariantCulture) == 0)
			{
				row["IS_SYSTEM_TABLE"] = false;
			}
			else
			{
				row["IS_SYSTEM_TABLE"] = true;
				row["TABLE_TYPE"] = "SYSTEM_TABLE";
			}
			if (row["VIEW_SOURCE"] != null &&
				row["VIEW_SOURCE"].ToString().Length > 0)
			{
				row["TABLE_TYPE"] = "VIEW";
			}
		}

		schema.EndLoadData();
		schema.AcceptChanges();

		schema.Columns.Remove("VIEW_SOURCE");
	}

	#endregion
}
