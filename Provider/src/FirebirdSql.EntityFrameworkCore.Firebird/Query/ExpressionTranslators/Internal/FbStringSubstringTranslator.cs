/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using System.Collections.Generic;
using System.Reflection;
using FirebirdSql.EntityFrameworkCore.Firebird.Query.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal
{
	public class FbStringSubstringTranslator : IMethodCallTranslator
	{
		static readonly MethodInfo SubstringOnlyStartMethod = typeof(string).GetRuntimeMethod(nameof(string.Substring), new[] { typeof(int) });
		static readonly MethodInfo SubstringStartAndLengthMethod = typeof(string).GetRuntimeMethod(nameof(string.Substring), new[] { typeof(int), typeof(int) });

		readonly FbSqlExpressionFactory _fbSqlExpressionFactory;

		public FbStringSubstringTranslator(FbSqlExpressionFactory fbSqlExpressionFactory)
		{
			_fbSqlExpressionFactory = fbSqlExpressionFactory;
		}

		public SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
		{
			if (!(method.Equals(SubstringOnlyStartMethod) || method.Equals(SubstringStartAndLengthMethod)))
				return null;

			var fromExpression = _fbSqlExpressionFactory.Add(arguments[0], _fbSqlExpressionFactory.Constant(1));
			var forExpression = arguments.Count == 2 ? arguments[1] : null;
			return _fbSqlExpressionFactory.Substring(instance, fromExpression, forExpression);
		}
	}
}
