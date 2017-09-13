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
using FirebirdSql.EntityFrameworkCore.Firebird.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal
{
	public class FbStringSubstringTranslator : IMethodCallTranslator
	{
		static readonly MethodInfo SubstringOnlyStartMethod = typeof(string).GetRuntimeMethod(nameof(string.Substring), new[] { typeof(int) });
		static readonly MethodInfo SubstringStartAndLengthMethod = typeof(string).GetRuntimeMethod(nameof(string.Substring), new[] { typeof(int), typeof(int) });

		public virtual Expression Translate(MethodCallExpression methodCallExpression)
		{
			if (!(methodCallExpression.Method.Equals(SubstringOnlyStartMethod) || methodCallExpression.Method.Equals(SubstringStartAndLengthMethod)))
				return null;

			var fromExpression = methodCallExpression.Arguments[0].NodeType == ExpressionType.Constant
				? (Expression)Expression.Constant((int)((ConstantExpression)methodCallExpression.Arguments[0]).Value + 1)
				: Expression.Add(methodCallExpression.Arguments[0], Expression.Constant(1));

			var forExpression = methodCallExpression.Arguments.Count == 2
				? methodCallExpression.Arguments[1]
				: null;

			return new FbSubstringExpression(
				methodCallExpression.Object,
				fromExpression,
				forExpression);
		}
	}
}
