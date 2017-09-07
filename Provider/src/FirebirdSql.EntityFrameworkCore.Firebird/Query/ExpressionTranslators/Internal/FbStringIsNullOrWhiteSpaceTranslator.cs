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
