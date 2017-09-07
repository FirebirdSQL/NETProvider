using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal
{
	public class FbStringLengthTranslator : IMemberTranslator
	{
		public virtual Expression Translate(MemberExpression memberExpression)
		{
			if (memberExpression.Expression != null && memberExpression.Expression.Type == typeof(string) && memberExpression.Member.Name == nameof(string.Length))
			{
				return new ExplicitCastExpression(
					new SqlFunctionExpression("CHARACTER_LENGTH", memberExpression.Type, new[] { memberExpression.Expression }),
					typeof(int));
			}
			return null;
		}
	}
}
