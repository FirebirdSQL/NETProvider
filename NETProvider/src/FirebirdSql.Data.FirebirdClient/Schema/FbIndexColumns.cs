/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *	All Rights Reserved.
 * 
 *  Constributors:
 *      Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Data;
using System.Globalization;
using System.Text;

namespace FirebirdSql.Data.Schema
{
	internal class FbIndexColumns : FbSchema
	{
		#region Protected Methods

		protected override StringBuilder GetCommandText(string[] restrictions)
		{
			StringBuilder sql = new StringBuilder();
			StringBuilder where = new StringBuilder();

			sql.Append(
				@"SELECT
					null AS CONSTRAINT_CATALOG,
					null AS CONSTRAINT_SCHEMA,
					idx.rdb$index_name AS CONSTRAINT_NAME,
					null AS TABLE_CATALOG,
					null AS TABLE_SCHEMA,
					idx.rdb$relation_name AS TABLE_NAME,
					seg.rdb$field_name AS COLUMN_NAME,
					seg.rdb$field_position AS ORDINAL_POSITION,
					idx.rdb$index_name AS INDEX_NAME
				FROM rdb$indices idx
					LEFT JOIN rdb$index_segments seg ON idx.rdb$index_name = seg.rdb$index_name");

			if (restrictions != null)
			{
				int index = 0;

				/* TABLE_CATALOG */
				if (restrictions.Length >= 1 && restrictions[0] != null)
				{
				}

				/* TABLE_SCHEMA	*/
				if (restrictions.Length >= 2 && restrictions[1] != null)
				{
				}

				/* TABLE_NAME */
				if (restrictions.Length >= 3 && restrictions[2] != null)
				{
					where.AppendFormat(CultureInfo.CurrentCulture, "idx.rdb$relation_name = @p{0}", index++);
				}

				/* INDEX_NAME */
				if (restrictions.Length >= 4 && restrictions[3] != null)
				{
					if (where.Length > 0)
					{
						where.Append(" AND ");
					}

					where.AppendFormat(CultureInfo.CurrentCulture, "idx.rdb$index_name = @p{0}", index++);
				}

				/* COLUMN_NAME */
				if (restrictions.Length >= 5 && restrictions[4] != null)
				{
					if (where.Length > 0)
					{
						where.Append(" AND ");
					}

					where.AppendFormat(CultureInfo.CurrentCulture, "seg.rdb$field_name = @p{0}", index++);
				}
			}

			if (where.Length > 0)
			{
				sql.AppendFormat(CultureInfo.CurrentCulture, " WHERE {0} ", where.ToString());
			}

			sql.Append(" ORDER BY idx.rdb$relation_name, idx.rdb$index_name, seg.rdb$field_position");

			return sql;
		}

		#endregion
	}
}
