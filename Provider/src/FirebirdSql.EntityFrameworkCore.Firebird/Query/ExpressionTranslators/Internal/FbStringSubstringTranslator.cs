using System.Linq.Expressions;
using System.Reflection;
using FirebirdSql.EntityFrameworkCore.Firebird.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal
{
	public class FbStringSubstringTranslator : IMethodCallTranslator
	{
		static readonly MethodInfo SubstringMethod = typeof(string).GetRuntimeMethod(nameof(string.Substring), new[] { typeof(int), typeof(int) });

		public virtual Expression Translate(MethodCallExpression methodCallExpression)
		{
			if (!methodCallExpression.Equals(SubstringMethod))
				return null;

			var from = methodCallExpression.Arguments[0].NodeType == ExpressionType.Constant
				? (Expression)Expression.Constant((int)((ConstantExpression)methodCallExpression.Arguments[0]).Value + 1)
				: Expression.Add(methodCallExpression.Arguments[0], Expression.Constant(1));

			return new FbSubstringExpression(
				methodCallExpression.Object,
				from,
				methodCallExpression.Arguments[1]);
		}
	}
}
