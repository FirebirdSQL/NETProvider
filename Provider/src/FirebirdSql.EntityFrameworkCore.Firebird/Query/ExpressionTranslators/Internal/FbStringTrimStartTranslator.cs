using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal
{
	public class FbStringTrimStartTranslator : IMethodCallTranslator
	{
		public Expression Translate(MethodCallExpression methodCallExpression)
		{
#warning Finish
			throw new NotImplementedException();
		}
	}
}
