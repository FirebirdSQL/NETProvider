using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal
{
	public class FbStringTrimTranslator : IMethodCallTranslator
	{
		static readonly MethodInfo TrimWithoutArgsMethod = typeof(string).GetRuntimeMethod(nameof(string.Trim), new Type[] { });
		static readonly MethodInfo TrimWithCharArrayArgMethod = typeof(string).GetRuntimeMethod(nameof(string.Trim), new[] { typeof(char[]) });

		public virtual Expression Translate(MethodCallExpression methodCallExpression)
		{
			if (methodCallExpression.Equals(TrimWithoutArgsMethod) || methodCallExpression.Equals(TrimWithCharArrayArgMethod)
				// no arguments
				&& ((methodCallExpression.Arguments[0] as ConstantExpression)?.Value as Array)?.Length == 0)
			{
				var sqlArguments = new[] { methodCallExpression.Object };
				return new SqlFunctionExpression(
					"TRIM",
					methodCallExpression.Type,
					sqlArguments);
			}
			return null;
		}
	}
}
