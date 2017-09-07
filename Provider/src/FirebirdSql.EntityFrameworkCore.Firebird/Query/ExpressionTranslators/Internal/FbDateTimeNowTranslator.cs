using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal
{
	public class FbDateTimeNowTranslator : IMemberTranslator
	{
		public virtual Expression Translate(MemberExpression memberExpression)
		{
			if (memberExpression.Expression == null && memberExpression.Member.DeclaringType == typeof(DateTime) && memberExpression.Member.Name == nameof(DateTime.Now))
			{
				return new SqlFragmentExpression("CURRENT_TIMESTAMP");
			}
			return null;
		}
	}
}
