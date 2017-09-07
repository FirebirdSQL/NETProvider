using System;
using System.Linq.Expressions;
using FirebirdSql.EntityFrameworkCore.Firebird.Query.Expressions.Internal;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.Sql.Internal
{
	public class FbQuerySqlGenerator : DefaultQuerySqlGenerator, IFbExpressionVisitor
	{
		public FbQuerySqlGenerator(QuerySqlGeneratorDependencies dependencies, SelectExpression selectExpression)
			: base(dependencies, selectExpression)
		{ }

		protected override void GenerateTop(SelectExpression selectExpression)
		{
			if (selectExpression.Limit != null)
			{
				Sql.Append("FIRST ");
				Visit(selectExpression.Limit);
				Sql.Append(" ");
			}

			if (selectExpression.Offset != null)
			{
				Sql.Append("SKIP ");
				Visit(selectExpression.Offset);
				Sql.Append(" ");
			}
		}

		protected override Expression VisitBinary(BinaryExpression binaryExpression)
		{
			if (binaryExpression.NodeType == ExpressionType.Add && binaryExpression.Left.Type == typeof(string) && binaryExpression.Right.Type == typeof(string))
			{
				Sql.Append("(");
				Visit(binaryExpression.Left);
				Sql.Append(" || ");
				var exp = Visit(binaryExpression.Right);
				Sql.Append(")");
				return exp;
			}
			else
			{
				return base.VisitBinary(binaryExpression);
			}
		}

		public virtual Expression VisitSubstring(FbSubstringExpression substringExpression)
		{
			Sql.Append("SUBSTRING(");
			Visit(substringExpression.ValueExpression);
			Sql.Append(" FROM ");
			Visit(substringExpression.FromExpression);
			Sql.Append(" FOR ");
			Visit(substringExpression.ForExpression);
			Sql.Append(")");
			return substringExpression;
		}
	}
}
