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
	internal class FbForeignKeys : FbSchema
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
					co.rdb$constraint_name AS CONSTRAINT_NAME,
					null AS TABLE_CATALOG,
	                null AS TABLE_SCHEMA,
					co.rdb$relation_name AS TABLE_NAME,
					null as REFERENCED_TABLE_CATALOG,
					null as REFERENCED_TABLE_SCHEMA,
					refidx.rdb$relation_name as REFERENCED_TABLE_NAME,
					co.rdb$deferrable AS IS_DEFERRABLE,
					co.rdb$initially_deferred AS INITIALLY_DEFERRED,
					ref.rdb$match_option AS MATCH_OPTION,
					ref.rdb$update_rule AS UPDATE_RULE,
					ref.rdb$delete_rule AS DELETE_RULE,
					co.rdb$index_name as INDEX_NAME
				FROM rdb$relation_constraints co
	                INNER JOIN rdb$ref_constraints ref ON co.rdb$constraint_name = ref.rdb$constraint_name
					INNER JOIN rdb$indices tempidx ON co.rdb$index_name = tempidx.rdb$index_name
					INNER JOIN rdb$indices refidx ON refidx.rdb$index_name = tempidx.rdb$foreign_key");

			where.Append("co.rdb$constraint_type = 'FOREIGN KEY'");

			if (restrictions != null)
			{
				int index = 0;

				/* CONSTRAINT_CATALOG	*/
				if (restrictions.Length >= 1 && restrictions[0] != null)
				{
				}

				/* CONSTRAINT_SCHEMA */
				if (restrictions.Length >= 2 && restrictions[1] != null)
				{
				}

				/* TABLE_NAME */
				if (restrictions.Length >= 3 && restrictions[2] != null)
				{
					where.AppendFormat(CultureInfo.CurrentCulture, " AND co.rdb$relation_name = @p{0}", index++);
				}

				/* CONSTRAINT_NAME */
				if (restrictions.Length >= 4 && restrictions[3] != null)
				{
					where.AppendFormat(CultureInfo.CurrentCulture, " AND rel.rdb$constraint_name = @p{0}", index++);
				}
			}

			if (where.Length > 0)
			{
				sql.AppendFormat(CultureInfo.CurrentCulture, " WHERE {0} ", where.ToString());
			}

			sql.Append(" ORDER BY co.rdb$relation_name, co.rdb$constraint_name");

			return sql;
		}

		#endregion
	}
}
