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
	class FbEndsWithOptimizedTranslator : IMethodCallTranslator
	{
		static readonly MethodInfo MethodInfo = typeof(string).GetRuntimeMethod(nameof(string.EndsWith), new[] { typeof(string) });

		readonly FbSqlExpressionFactory _fbSqlExpressionFactory;

		public FbEndsWithOptimizedTranslator(FbSqlExpressionFactory fbSqlExpressionFactory)
		{
			_fbSqlExpressionFactory = fbSqlExpressionFactory;
		}

		public SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
		{
			if (!method.Equals(MethodInfo))
				return null;

			var patternExpression = arguments[0];
			var endsWithExpression = _fbSqlExpressionFactory.Equal(
				_fbSqlExpressionFactory.Function(
						"RIGHT",
						new[] {
							instance,
							_fbSqlExpressionFactory.Function(
								"CHARACTER_LENGTH",
								new[] { patternExpression },
								typeof(int)) },
						instance.Type),
				patternExpression);
			return patternExpression is SqlConstantExpression sqlConstantExpression
				? (string)sqlConstantExpression.Value == string.Empty
					? (SqlExpression)_fbSqlExpressionFactory.Constant(true)
					: endsWithExpression
				: _fbSqlExpressionFactory.OrElse(
					endsWithExpression,
					_fbSqlExpressionFactory.Equal(
						patternExpression,
						_fbSqlExpressionFactory.Constant(string.Empty)));
		}
	}
}
