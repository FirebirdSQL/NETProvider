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

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.Text;

namespace FirebirdSql.Data.Common;

internal static class NamedParametersParser
{
	public static (string sql, IReadOnlyList<string> parameters) Parse(string sql)
	{
		var sqlBuilder = new StringBuilder(sql.Length);
		var paramBuilder = new StringBuilder();

		if (sql.IndexOf('@') == -1)
		{
			return (sql, Array.Empty<string>());
		}

		var namedParameters = new List<string>();
		var inSingleQuotes = false;
		var inDoubleQuotes = false;
		var inParam = false;
		var inSingleLineComment = false;
		var inMultiLineComment = false;

		for (var i = 0; i < sql.Length; i++)
		{
			var sym = sql[i];

			if (inParam)
			{
				if (char.IsLetterOrDigit(sym) || sym == '_' || sym == '$')
				{
					paramBuilder.Append(sym);
				}
				else
				{
					namedParameters.Add(paramBuilder.ToString());
					paramBuilder.Length = 0;
					sqlBuilder.Append('?');
					sqlBuilder.Append(sym);
					inParam = false;
				}
			}
			else
			{
				var needsAdvance = false;
				if ((sym == '\'') && (!(inDoubleQuotes || inSingleLineComment || inMultiLineComment)))
				{
					inSingleQuotes = !inSingleQuotes;
				}
				else if ((sym == '\"') && (!(inSingleQuotes || inSingleLineComment || inMultiLineComment)))
				{
					inDoubleQuotes = !inDoubleQuotes;
				}
				else if ((sym == '-') && (i < sql.Length - 1) && (sql[i + 1] == '-') && (!(inSingleQuotes || inDoubleQuotes || inSingleLineComment || inMultiLineComment)))
				{
					inSingleLineComment = true;
					needsAdvance = true;
				}
				else if ((sym == '\n') && inSingleLineComment)
				{
					inSingleLineComment = false;
				}
				else if ((sym == '/') && (i < sql.Length - 1) && (sql[i + 1] == '*') && (!(inSingleQuotes || inDoubleQuotes || inSingleLineComment || inMultiLineComment)))
				{
					inMultiLineComment = true;
					needsAdvance = true;
				}
				else if ((sym == '*') && (i < sql.Length - 1) && (sql[i + 1] == '/') && (inMultiLineComment))
				{
					inMultiLineComment = false;
					needsAdvance = true;
				}
				else if ((!(inSingleQuotes || inDoubleQuotes || inSingleLineComment || inMultiLineComment)) && (sym == '@'))
				{
					inParam = true;
					paramBuilder.Append(sym);
					continue;
				}

				if (needsAdvance)
				{
					sqlBuilder.Append(sym);
					sym = sql[++i];
				}

				sqlBuilder.Append(sym);
			}
		}

		if (inParam)
		{
			namedParameters.Add(paramBuilder.ToString());
			sqlBuilder.Append('?');
		}

		return (sqlBuilder.ToString(), namedParameters);
	}
}
