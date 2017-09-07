using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal
{
	class FbEndsWithOptimizedTranslator : IMethodCallTranslator
	{
		static readonly MethodInfo MethodInfo = typeof(string).GetRuntimeMethod(nameof(string.EndsWith), new[] { typeof(string) });

		public virtual Expression Translate(MethodCallExpression methodCallExpression)
		{
			if (methodCallExpression.Method.Equals(MethodInfo))
			{
				var patternExpression = methodCallExpression.Arguments[0];
				var patternConstantExpression = patternExpression as ConstantExpression;

				var endsWithExpression = new NullCompensatedExpression(
					Expression.Equal(
						new SqlFunctionExpression(
							"RIGHT",
							methodCallExpression.Object.Type,
							new[]
							{
								methodCallExpression.Object,
								new SqlFunctionExpression("CHARACTER_LENGTH", typeof(int), new[] { patternExpression })
							}),
						patternExpression));

				return patternConstantExpression != null
					? (string)patternConstantExpression.Value == string.Empty
						? (Expression)Expression.Constant(true)
						: endsWithExpression
					: Expression.OrElse(
						endsWithExpression,
						Expression.Equal(patternExpression, Expression.Constant(string.Empty)));
			}
			return null;
		}
	}
}
