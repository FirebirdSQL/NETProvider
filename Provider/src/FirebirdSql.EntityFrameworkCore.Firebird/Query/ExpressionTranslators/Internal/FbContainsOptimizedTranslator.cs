using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal
{
	public class FbContainsOptimizedTranslator : IMethodCallTranslator
	{
		static readonly MethodInfo MethodInfo = typeof(string).GetRuntimeMethod(nameof(string.Contains), new[] { typeof(string) });

		public virtual Expression Translate(MethodCallExpression methodCallExpression)
		{
			if (!methodCallExpression.Method.Equals(MethodInfo))
				return null;

			return Expression.GreaterThan(
				new SqlFunctionExpression("POSITION", typeof(int), new[]
				{
					methodCallExpression.Arguments[0],
					methodCallExpression.Object,
				}),
				Expression.Constant(0));
		}
	}
}
