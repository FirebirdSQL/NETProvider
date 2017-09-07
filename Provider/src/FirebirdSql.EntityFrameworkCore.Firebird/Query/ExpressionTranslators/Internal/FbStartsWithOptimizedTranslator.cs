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
			var patternConstantExpression = patternExpression as ConstantExpression;

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

			return patternConstantExpression != null
				? (string)patternConstantExpression.Value == string.Empty
					? (Expression)Expression.Constant(true)
					: startsWithExpression
				: Expression.OrElse(
					startsWithExpression,
					Expression.Equal(patternExpression, Expression.Constant(string.Empty)));
		}
	}
}
