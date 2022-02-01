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

using FirebirdSql.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Schema;

internal class FbDomains : FbSchema
{
	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		var sql = new StringBuilder();
		var where = new StringBuilder();

		sql.Append(
			@"SELECT
					null AS DOMAIN_CATALOG,
					null AS DOMAIN_SCHEMA,
					fld.rdb$field_name AS DOMAIN_NAME,
					null AS DOMAIN_DATA_TYPE,
					rdb$field_sub_type AS DOMAIN_SUB_TYPE,
					CAST(fld.rdb$field_length AS integer) AS DOMAIN_SIZE,
					CAST(fld.rdb$field_precision AS integer) AS NUMERIC_PRECISION,
					CAST(fld.rdb$field_scale  AS integer) AS NUMERIC_SCALE,
					CAST(fld.rdb$character_length AS integer) AS CHARACTER_MAX_LENGTH,
					CAST(fld.rdb$field_length AS integer) AS CHARACTER_OCTET_LENGTH,
					fld.rdb$null_flag AS COLUMN_NULLABLE,
					fld.rdb$dimensions AS COLUMN_ARRAY,
					fld.rdb$description AS DESCRIPTION,
					fld.rdb$field_type AS FIELD_TYPE,
					null AS CHARACTER_SET_CATALOG,
					null AS CHARACTER_SET_SCHEMA,
					cs.rdb$character_set_name AS CHARACTER_SET_NAME,
					null AS COLLATION_CATALOG,
					null AS COLLATION_SCHEMA,
					coll.rdb$collation_name AS COLLATION_NAME
				FROM rdb$fields fld
					LEFT JOIN rdb$character_sets cs ON cs.rdb$character_set_id = fld.rdb$character_set_id
					LEFT JOIN rdb$collations coll ON (coll.rdb$collation_id = fld.rdb$collation_id AND coll.rdb$character_set_id = fld.rdb$character_set_id)");

		where.Append("rdb$field_name NOT STARTING WITH 'RDB$'");

		if (restrictions != null)
		{
			var index = 0;

			/* DOMAIN_CATALOG */
			if (restrictions.Length >= 1 && restrictions[0] != null)
			{
			}

			/* DOMAIN_SCHEMA */
			if (restrictions.Length >= 2 && restrictions[1] != null)
			{
			}

			/* DOMAIN_NAME */
			if (restrictions.Length >= 3 && restrictions[2] != null)
			{
				where.AppendFormat(" AND rdb$field_name = @p{0}", index++);
			}
		}

		if (where.Length > 0)
		{
			sql.AppendFormat(" WHERE {0} ", where.ToString());
		}

		sql.Append(" ORDER BY DOMAIN_NAME");

		return sql;
	}

	protected override void ProcessResult(DataTable schema)
	{
		schema.BeginLoadData();
		schema.Columns.Add("IS_NULLABLE", typeof(bool));
		schema.Columns.Add("IS_ARRAY", typeof(bool));

		foreach (DataRow row in schema.Rows)
		{
			var blrType = Convert.ToInt32(row["FIELD_TYPE"], CultureInfo.InvariantCulture);

			var subType = 0;
			if (row["DOMAIN_SUB_TYPE"] != DBNull.Value)
			{
				subType = Convert.ToInt32(row["DOMAIN_SUB_TYPE"], CultureInfo.InvariantCulture);
			}

			var scale = 0;
			if (row["NUMERIC_SCALE"] != DBNull.Value)
			{
				scale = Convert.ToInt32(row["NUMERIC_SCALE"], CultureInfo.InvariantCulture);
			}

			row["IS_NULLABLE"] = (row["COLUMN_NULLABLE"] == DBNull.Value);
			row["IS_ARRAY"] = (row["COLUMN_ARRAY"] != DBNull.Value);

			var dbType = (FbDbType)TypeHelper.GetDbDataTypeFromBlrType(blrType, subType, scale);
			row["DOMAIN_DATA_TYPE"] = TypeHelper.GetDataTypeName((DbDataType)dbType).ToLowerInvariant();

			if (dbType == FbDbType.Char || dbType == FbDbType.VarChar)
			{
				row["DOMAIN_SIZE"] = row["CHARACTER_MAX_LENGTH"];
			}
			else
			{
				row["CHARACTER_OCTET_LENGTH"] = 0;
			}

			if (dbType == FbDbType.Binary || dbType == FbDbType.Text)
			{
				row["DOMAIN_SIZE"] = Int32.MaxValue;
			}

			if (row["NUMERIC_PRECISION"] == DBNull.Value)
			{
				row["NUMERIC_PRECISION"] = 0;
			}

			if ((dbType == FbDbType.Decimal || dbType == FbDbType.Numeric) &&
				(row["NUMERIC_PRECISION"] == DBNull.Value || Convert.ToInt32(row["NUMERIC_PRECISION"]) == 0))
			{
				row["NUMERIC_PRECISION"] = row["DOMAIN_SIZE"];
			}

			row["NUMERIC_SCALE"] = (-1) * scale;
		}

		schema.EndLoadData();
		schema.AcceptChanges();

		// Remove not more needed columns
		schema.Columns.Remove("COLUMN_NULLABLE");
		schema.Columns.Remove("COLUMN_ARRAY");
		schema.Columns.Remove("FIELD_TYPE");
		schema.Columns.Remove("CHARACTER_MAX_LENGTH");
	}

	#endregion
}
