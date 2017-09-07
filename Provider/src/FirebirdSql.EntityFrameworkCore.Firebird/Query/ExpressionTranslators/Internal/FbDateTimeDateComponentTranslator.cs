using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal
{
	public class FbDateTimeDateComponentTranslator : IMemberTranslator
	{
		public virtual Expression Translate(MemberExpression memberExpression)
		{
			if (memberExpression.Expression != null && (memberExpression.Expression.Type == typeof(DateTime) || memberExpression.Expression.Type == typeof(DateTimeOffset)) && memberExpression.Member.Name == nameof(DateTime.Date))
			{
				return new SqlFunctionExpression("CAST", memberExpression.Type, new[]
				{
					new SqlFragmentExpression("DATE"),
					memberExpression.Expression,
				});
			}
			return null;
		}
	}
}
