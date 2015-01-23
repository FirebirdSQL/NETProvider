/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *  All Rights Reserved.
 * 
 *  Contributors:
 *      Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Data;
using System.Globalization;
using System.Text;

namespace FirebirdSql.Data.Schema
{
	internal class FbViews : FbSchema
	{
		#region Protected Methods

		protected override StringBuilder GetCommandText(string[] restrictions)
		{
			StringBuilder sql = new StringBuilder();
			StringBuilder where = new StringBuilder();

			sql.Append(
				@"SELECT
					null AS VIEW_CATALOG,
					null AS VIEW_SCHEMA,
					rel.rdb$relation_name AS VIEW_NAME,
					rel.rdb$system_flag AS IS_SYSTEM_VIEW,
					rel.rdb$view_source AS DEFINITION,
					rel.rdb$description AS DESCRIPTION
				FROM rdb$relations rel");

			where.Append("rel.rdb$view_source IS NOT NULL");

			if (restrictions != null)
			{
				int index = 0;

				/* VIEW_CATALOG */
				if (restrictions.Length >= 1 && restrictions[0] != null)
				{
				}

				/* VIEW_SCHEMA */
				if (restrictions.Length >= 2 && restrictions[1] != null)
				{
				}

				/* VIEW_NAME */
				if (restrictions.Length >= 3 && restrictions[2] != null)
				{
					where.AppendFormat(CultureInfo.CurrentUICulture, " AND rel.rdb$relation_name = @p{0}", index++);
				}
			}

			if (where.Length > 0)
			{
				sql.AppendFormat(CultureInfo.CurrentUICulture, " WHERE {0} ", where.ToString());
			}

			sql.Append(" ORDER BY rel.rdb$relation_name");

			return sql;
		}

		protected override DataTable ProcessResult(DataTable schema)
		{
			schema.BeginLoadData();

			foreach (DataRow row in schema.Rows)
			{
				if (row["IS_SYSTEM_VIEW"] == DBNull.Value ||
					Convert.ToInt32(row["IS_SYSTEM_VIEW"], CultureInfo.InvariantCulture) == 0)
				{
					row["IS_SYSTEM_VIEW"] = false;
				}
				else
				{
					row["IS_SYSTEM_VIEW"] = true;
				}
			}

			schema.EndLoadData();
			schema.AcceptChanges();

			return schema;
		}

		#endregion
	}
}
