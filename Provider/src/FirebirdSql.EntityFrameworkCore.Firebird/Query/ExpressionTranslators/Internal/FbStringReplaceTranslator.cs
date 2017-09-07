using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal
{
	public class FbStringReplaceTranslator : IMethodCallTranslator
	{
		static readonly MethodInfo ReplaceMethod = typeof(string).GetRuntimeMethod(nameof(string.Replace), new[] { typeof(string), typeof(string) });

		public virtual Expression Translate(MethodCallExpression methodCallExpression)
		{
			if (!methodCallExpression.Equals(ReplaceMethod))
				return null;

			return new SqlFunctionExpression(
				"REPLACE",
				methodCallExpression.Type,
				new[] { methodCallExpression.Object }.Concat(methodCallExpression.Arguments));
		}
	}
}
