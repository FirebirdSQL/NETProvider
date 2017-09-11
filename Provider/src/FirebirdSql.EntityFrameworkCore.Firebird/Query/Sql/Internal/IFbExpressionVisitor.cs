using System.Linq.Expressions;
using FirebirdSql.EntityFrameworkCore.Firebird.Query.Expressions.Internal;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.Sql.Internal
{
	public interface IFbExpressionVisitor
	{
		Expression VisitSubstring(FbSubstringExpression substringExpression);
		Expression VisitExtract(FbExtractExpression extractExpression);
	}
}
