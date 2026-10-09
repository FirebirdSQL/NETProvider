/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.Internal;

public class FbSqlTranslatingExpressionVisitor : RelationalSqlTranslatingExpressionVisitor
{
	public FbSqlTranslatingExpressionVisitor(RelationalSqlTranslatingExpressionVisitorDependencies dependencies, QueryCompilationContext queryCompilationContext, QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
		: base(dependencies, queryCompilationContext, queryableMethodTranslatingExpressionVisitor)
	{ }

	protected override Expression VisitUnary(UnaryExpression unaryExpression)
	{
		if (unaryExpression.NodeType == ExpressionType.ArrayLength && unaryExpression.Operand.Type == typeof(byte[]))
		{
			if (!(base.Visit(unaryExpression.Operand) is SqlExpression sqlExpression))
			{
				return null;
			}
			return Dependencies.SqlExpressionFactory.Function("OCTET_LENGTH", new[] { sqlExpression }, true, new[] { true }, typeof(int));
		}
		return base.VisitUnary(unaryExpression);
	}

	public override SqlExpression GenerateGreatest(IReadOnlyList<SqlExpression> expressions, Type resultType)
	{
		var resultTypeMapping = ExpressionExtensions.InferTypeMapping(expressions);
		return Dependencies.SqlExpressionFactory.Function(
		   "MAXVALUE",
		   expressions,
		   nullable: true,
		   Enumerable.Repeat(true, expressions.Count),
		   resultType,
		   resultTypeMapping);
	}

	public override SqlExpression GenerateLeast(IReadOnlyList<SqlExpression> expressions, Type resultType)
	{
		var resultTypeMapping = ExpressionExtensions.InferTypeMapping(expressions);
		return Dependencies.SqlExpressionFactory.Function(
		   "MINVALUE",
		   expressions,
		   nullable: true,
		   Enumerable.Repeat(true, expressions.Count),
		   resultType,
		   resultTypeMapping);
	}
}
