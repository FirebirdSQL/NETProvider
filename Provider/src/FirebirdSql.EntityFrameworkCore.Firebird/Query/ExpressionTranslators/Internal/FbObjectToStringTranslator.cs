using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal
{
	public class FbObjectToStringTranslator : IMethodCallTranslator
	{
		static readonly HashSet<Type> SupportedTypes = new HashSet<Type>
		{
			typeof(int),
			typeof(long),
			typeof(DateTime),
			typeof(Guid),
			typeof(bool),
			typeof(byte),
			typeof(byte[]),
			typeof(double),
			typeof(DateTimeOffset),
			typeof(char),
			typeof(short),
			typeof(float),
			typeof(decimal),
			typeof(TimeSpan),
			typeof(uint),
			typeof(ushort),
			typeof(ulong),
			typeof(sbyte),
		};

		public virtual Expression Translate(MethodCallExpression methodCallExpression)
		{
			if (methodCallExpression.Method.Name == nameof(ToString) && methodCallExpression.Arguments.Count == 0 && methodCallExpression.Object != null && SupportedTypes.Contains(methodCallExpression.Object.Type.UnwrapNullableType().UnwrapEnumType()))
			{
				return new ExplicitCastExpression(methodCallExpression.Object, typeof(string));
			}
			return null;
		}
	}
}
