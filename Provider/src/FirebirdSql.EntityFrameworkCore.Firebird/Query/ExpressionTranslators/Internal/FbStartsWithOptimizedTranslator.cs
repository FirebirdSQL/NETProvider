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
	public class FbStartsWithOptimizedTranslator : IMethodCallTranslator
	{
		static readonly MethodInfo StartsWithMethod = typeof(string).GetRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) });
		static readonly MethodInfo ConcatMethod = typeof(string).GetRuntimeMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) });

		public virtual Expression Translate(MethodCallExpression methodCallExpression)
		{
			if (!methodCallExpression.Method.Equals(StartsWithMethod))
				return null;

			var patternExpression = methodCallExpression.Arguments[0];

			var startsWithExpression = Expression.AndAlso(
				   new LikeExpression(
					   methodCallExpression.Object,
					   Expression.Add(methodCallExpression.Arguments[0], Expression.Constant("%", typeof(string)), ConcatMethod)),
				   new NullCompensatedExpression(
						Expression.Equal(
							new SqlFunctionExpression(
								"LEFT",
								methodCallExpression.Object.Type,
								new[]
								{
									methodCallExpression.Object,
									new SqlFunctionExpression("CHARACTER_LENGTH", typeof(int), new[] { patternExpression })
								}),
							patternExpression)));

			return patternExpression is ConstantExpression patternConstantExpression
				? (string)patternConstantExpression.Value == string.Empty
					? (Expression)Expression.Constant(true)
					: startsWithExpression
				: Expression.OrElse(
					startsWithExpression,
					Expression.Equal(patternExpression, Expression.Constant(string.Empty)));
		}
	}
}
