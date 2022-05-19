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
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using FirebirdSql.EntityFrameworkCore.Firebird.Infrastructure.Internal;
using FirebirdSql.EntityFrameworkCore.Firebird.Query.Expressions.Internal;
using FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.Internal;

public class FbQuerySqlGenerator : QuerySqlGenerator
{
	readonly IFbOptions _fbOptions;

	public FbQuerySqlGenerator(QuerySqlGeneratorDependencies dependencies, IFbOptions fbOptions)
		: base(dependencies)
	{
		_fbOptions = fbOptions;
	}

	protected override Expression VisitSqlBinary(SqlBinaryExpression sqlBinaryExpression)
	{
		if (sqlBinaryExpression.OperatorType == ExpressionType.Modulo)
		{
			Sql.Append("MOD(");
			Visit(sqlBinaryExpression.Left);
			Sql.Append(", ");
			Visit(sqlBinaryExpression.Right);
			Sql.Append(")");
			return sqlBinaryExpression;
		}
		else if (sqlBinaryExpression.OperatorType == ExpressionType.And && sqlBinaryExpression.TypeMapping.ClrType != typeof(bool))
		{
			Sql.Append("BIN_AND(");
			Visit(sqlBinaryExpression.Left);
			Sql.Append(", ");
			Visit(sqlBinaryExpression.Right);
			Sql.Append(")");
			return sqlBinaryExpression;
		}
		else if (sqlBinaryExpression.OperatorType == ExpressionType.Or && sqlBinaryExpression.TypeMapping.ClrType != typeof(bool))
		{
			Sql.Append("BIN_OR(");
			Visit(sqlBinaryExpression.Left);
			Sql.Append(", ");
			Visit(sqlBinaryExpression.Right);
			Sql.Append(")");
			return sqlBinaryExpression;
		}
		else if (sqlBinaryExpression.OperatorType == ExpressionType.ExclusiveOr)
		{
			Sql.Append("BIN_XOR(");
			Visit(sqlBinaryExpression.Left);
			Sql.Append(", ");
			Visit(sqlBinaryExpression.Right);
			Sql.Append(")");
			return sqlBinaryExpression;
		}
		else if (sqlBinaryExpression.OperatorType == ExpressionType.LeftShift)
		{
			Sql.Append("BIN_SHL(");
			Visit(sqlBinaryExpression.Left);
			Sql.Append(", ");
			Visit(sqlBinaryExpression.Right);
			Sql.Append(")");
			return sqlBinaryExpression;
		}
		else if (sqlBinaryExpression.OperatorType == ExpressionType.RightShift)
		{
			Sql.Append("BIN_SHR(");
			Visit(sqlBinaryExpression.Left);
			Sql.Append(", ");
			Visit(sqlBinaryExpression.Right);
			Sql.Append(")");
			return sqlBinaryExpression;
		}
		else
		{
			return base.VisitSqlBinary(sqlBinaryExpression);
		}
	}

	protected override Expression VisitSqlParameter(SqlParameterExpression sqlParameterExpression)
	{
		var shouldExplicitParameterTypes = _fbOptions.ExplicitParameterTypes;
		if (shouldExplicitParameterTypes)
		{
			Sql.Append("CAST(");
		}
		base.VisitSqlParameter(sqlParameterExpression);
		if (shouldExplicitParameterTypes)
		{
			Sql.Append(" AS ");
			if (sqlParameterExpression.Type == typeof(string))
			{
				var isUnicode = FbTypeMappingSource.IsUnicode(sqlParameterExpression.TypeMapping);
				Sql.Append(((IFbSqlGenerationHelper)Dependencies.SqlGenerationHelper).StringParameterQueryType(isUnicode));
			}
			else
			{
				Sql.Append(sqlParameterExpression.TypeMapping.StoreType);
			}
			Sql.Append(")");
		}
		return sqlParameterExpression;
	}

	protected override Expression VisitSqlConstant(SqlConstantExpression sqlConstantExpression)
	{
		var shouldExplicitStringLiteralTypes = _fbOptions.ExplicitStringLiteralTypes && sqlConstantExpression.Type == typeof(string);
		if (shouldExplicitStringLiteralTypes)
		{
			Sql.Append("CAST(");
		}
		base.VisitSqlConstant(sqlConstantExpression);
		if (shouldExplicitStringLiteralTypes)
		{
			Sql.Append(" AS ");
			Sql.Append(((IFbSqlGenerationHelper)Dependencies.SqlGenerationHelper).StringLiteralQueryType(sqlConstantExpression.Value as string));
			Sql.Append(")");
		}
		return sqlConstantExpression;
	}

	protected override void GenerateTop(SelectExpression selectExpression)
	{
		// handled by GenerateLimitOffset
	}

	protected override void GenerateLimitOffset(SelectExpression selectExpression)
	{
		if (selectExpression.Limit != null && selectExpression.Offset != null)
		{
			Sql.AppendLine();
			Sql.Append("ROWS (");
			Visit(selectExpression.Offset);
			Sql.Append(" + 1) TO (");
			Visit(selectExpression.Offset);
			Sql.Append(" + ");
			Visit(selectExpression.Limit);
			Sql.Append(")");
		}
		else if (selectExpression.Limit != null && selectExpression.Offset == null)
		{
			Sql.AppendLine();
			Sql.Append("ROWS (");
			Visit(selectExpression.Limit);
			Sql.Append(")");
		}
		else if (selectExpression.Limit == null && selectExpression.Offset != null)
		{
			Sql.AppendLine();
			Sql.Append("ROWS (");
			Visit(selectExpression.Offset);
			Sql.Append(" + 1) TO (");
			Sql.Append(long.MaxValue.ToString(CultureInfo.InvariantCulture));
			Sql.Append(")");
		}
	}

	protected override string GetOperator(SqlBinaryExpression binaryExpression)
	{
		if (binaryExpression.OperatorType == ExpressionType.Add && binaryExpression.TypeMapping.ClrType == typeof(string))
		{
			return " || ";
		}
		else if (binaryExpression.OperatorType == ExpressionType.AndAlso || binaryExpression.OperatorType == ExpressionType.And)
		{
			return " AND ";
		}
		else if (binaryExpression.OperatorType == ExpressionType.OrElse || binaryExpression.OperatorType == ExpressionType.Or)
		{
			return " OR ";
		}
		return base.GetOperator(binaryExpression);
	}

	// https://github.com/aspnet/EntityFrameworkCore/issues/19031
	protected override void GenerateOrderings(SelectExpression selectExpression)
	{
		if (selectExpression.Orderings.Any())
		{
			var orderings = selectExpression.Orderings.ToList();

			if (selectExpression.Limit == null
				&& selectExpression.Offset == null)
			{
				orderings.RemoveAll(oe => oe.Expression is SqlConstantExpression || oe.Expression is SqlParameterExpression);
			}

			if (orderings.Count > 0)
			{
				Sql.AppendLine()
					.Append("ORDER BY ");

				GenerateList(orderings, e => Visit(e));
			}
		}
	}

	protected override void GeneratePseudoFromClause()
	{
		Sql.Append(" FROM RDB$DATABASE");
	}

	protected override Expression VisitOrdering(OrderingExpression orderingExpression)
	{
		if (orderingExpression.Expression is SqlConstantExpression
			|| orderingExpression.Expression is SqlParameterExpression
			|| (orderingExpression.Expression is SqlFragmentExpression sqlFragment && sqlFragment.Sql.Equals("(SELECT 1)", StringComparison.Ordinal)))
		{
			Sql.Append("(SELECT 1");
			GeneratePseudoFromClause();
			Sql.Append(")");
		}
		else
		{
			Visit(orderingExpression.Expression);
		}
		if (!orderingExpression.IsAscending)
		{
			Sql.Append(" DESC");
		}
		return orderingExpression;
	}

	protected override Expression VisitTableValuedFunction(TableValuedFunctionExpression tableValuedFunctionExpression)
	{
		Sql.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(tableValuedFunctionExpression.StoreFunction.Name));
		if (tableValuedFunctionExpression.Arguments.Any())
		{
			Sql.Append("(");
			GenerateList(tableValuedFunctionExpression.Arguments, e => Visit(e));
			Sql.Append(")");
		}
		Sql.Append(AliasSeparator);
		Sql.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(tableValuedFunctionExpression.Alias));
		return tableValuedFunctionExpression;
	}

	protected override Expression VisitExtension(Expression extensionExpression)
	{
		return extensionExpression switch
		{
			FbSpacedFunctionExpression spacedFunctionExpression => VisitSpacedFunction(spacedFunctionExpression),
			_ => base.VisitExtension(extensionExpression),
		};
	}

	public virtual Expression VisitSpacedFunction(FbSpacedFunctionExpression spacedFunctionExpression)
	{
		Sql.Append(spacedFunctionExpression.Name);
		Sql.Append("(");
		for (var i = 0; i < spacedFunctionExpression.Arguments.Count; i++)
		{
			Visit(spacedFunctionExpression.Arguments[i]);
			if (i < spacedFunctionExpression.Arguments.Count - 1)
			{
				Sql.Append(" ");
			}
		}
		Sql.Append(")");
		return spacedFunctionExpression;
	}

	void GenerateList<T>(IReadOnlyList<T> items, Action<T> generationAction, Action<IRelationalCommandBuilder> joinAction = null)
	{
		joinAction ??= (isb => isb.Append(", "));

		for (var i = 0; i < items.Count; i++)
		{
			if (i > 0)
			{
				joinAction(Sql);
			}

			generationAction(items[i]);
		}
	}
}
