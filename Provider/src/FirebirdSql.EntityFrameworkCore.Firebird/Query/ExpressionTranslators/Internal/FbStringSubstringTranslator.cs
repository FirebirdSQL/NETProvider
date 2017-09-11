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
