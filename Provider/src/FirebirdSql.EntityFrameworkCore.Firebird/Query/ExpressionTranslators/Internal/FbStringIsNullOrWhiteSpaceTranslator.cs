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

//$Authors = Jiri Cincura (jiri@cincura.net), Jean Ressouche, Rafael Almeida (ralms@ralms.net)

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal
{
	public class FbStringIsNullOrWhiteSpaceTranslator : IMethodCallTranslator
	{
		static readonly MethodInfo IsNullOrWhiteSpaceMethod = typeof(string).GetRuntimeMethod(nameof(string.IsNullOrWhiteSpace), new[] { typeof(string) });

		public Expression Translate(MethodCallExpression methodCallExpression)
		{
			if (!methodCallExpression.Method.Equals(IsNullOrWhiteSpaceMethod))
				return null;

			var argument = methodCallExpression.Arguments[0];
			return Expression.MakeBinary(
				ExpressionType.OrElse,
				new IsNullExpression(argument),
				Expression.Equal(
					new SqlFunctionExpression(
						"TRIM",
						typeof(string),
						new[] { argument }),
					Expression.Constant(string.Empty, typeof(string))));
		}
	}
}
