/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using System.Linq;
using System.Linq.Expressions;
using FirebirdSql.EntityFrameworkCore.Firebird.Infrastructure.Internal;
using FirebirdSql.EntityFrameworkCore.Firebird.Query.Expressions.Internal;
using FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.Sql.Internal
{
	public class FbQuerySqlGenerator : DefaultQuerySqlGenerator, IFbExpressionVisitor
	{
		readonly IFbOptions _fbOptions;

		public FbQuerySqlGenerator(QuerySqlGeneratorDependencies dependencies, SelectExpression selectExpression, IFbOptions fbOptions)
			: base(dependencies, selectExpression)
		{
			_fbOptions = fbOptions;
		}

		protected override string TypedTrueLiteral => FbBoolTypeMapping.TrueLiteral;
		protected override string TypedFalseLiteral => FbBoolTypeMapping.FalseLiteral;

		protected override Expression VisitBinary(BinaryExpression binaryExpression)
		{
			if (binaryExpression.NodeType == ExpressionType.Modulo)
			{
				Sql.Append("MOD(");
				Visit(binaryExpression.Left);
				Sql.Append(", ");
				Visit(binaryExpression.Right);
				Sql.Append(")");
				return binaryExpression;
			}
			else if (binaryExpression.NodeType == ExpressionType.And && binaryExpression.Type != typeof(bool))
			{
				Sql.Append("BIN_AND(");
				Visit(binaryExpression.Left);
				Sql.Append(", ");
				Visit(binaryExpression.Right);
				Sql.Append(")");
				return binaryExpression;
			}
			else if (binaryExpression.NodeType == ExpressionType.Or && binaryExpression.Type != typeof(bool))
			{
				Sql.Append("BIN_OR(");
				Visit(binaryExpression.Left);
				Sql.Append(", ");
				Visit(binaryExpression.Right);
				Sql.Append(")");
				return binaryExpression;
			}
			else if (binaryExpression.NodeType == ExpressionType.ExclusiveOr)
			{
				Sql.Append("BIN_XOR(");
				Visit(binaryExpression.Left);
				Sql.Append(", ");
				Visit(binaryExpression.Right);
				Sql.Append(")");
				return binaryExpression;
			}
			else if (binaryExpression.NodeType == ExpressionType.LeftShift)
			{
				Sql.Append("BIN_SHL(");
				Visit(binaryExpression.Left);
				Sql.Append(", ");
				Visit(binaryExpression.Right);
				Sql.Append(")");
				return binaryExpression;
			}
			else if (binaryExpression.NodeType == ExpressionType.RightShift)
			{
				Sql.Append("BIN_SHR(");
				Visit(binaryExpression.Left);
				Sql.Append(", ");
				Visit(binaryExpression.Right);
				Sql.Append(")");
				return binaryExpression;
			}
			else
			{
				return base.VisitBinary(binaryExpression);
			}
		}

		protected override Expression VisitParameter(ParameterExpression parameterExpression)
		{
			if (_fbOptions.ExplicitParameterTypes)
			{
				Sql.Append("CAST(");
			}
			base.VisitParameter(parameterExpression);
			if (_fbOptions.ExplicitParameterTypes)
			{
				Sql.Append(" AS ");
				if (parameterExpression.Type == typeof(string))
				{
					if (ParameterValues.TryGetValue(parameterExpression.Name, out var parameterValue))
					{
						Sql.Append(((IFbTypeMappingSource)Dependencies.TypeMappingSource).StringParameterQueryType((string)parameterValue));
						IsCacheable = false;
					}
					else
					{
						Sql.Append(((IFbTypeMappingSource)Dependencies.TypeMappingSource).StringParameterQueryType());
					}
				}
				else
				{
					Sql.Append(Dependencies.TypeMappingSource.GetMapping(parameterExpression.Type).StoreType);
				}
				Sql.Append(")");
			}
			return parameterExpression;
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
				Sql.Append(long.MaxValue);
				Sql.Append(")");
			}
		}

		protected override string GenerateOperator(Expression expression)
		{
			if (expression.NodeType == ExpressionType.Add && expression.Type == typeof(string))
			{
				return " || ";
			}
			else if (expression.NodeType == ExpressionType.And && expression.Type == typeof(bool))
			{
				return " AND ";
			}
			else if (expression.NodeType == ExpressionType.Or && expression.Type == typeof(bool))
			{
				return " OR ";
			}
			return base.GenerateOperator(expression);
		}

		protected override Expression VisitConstant(ConstantExpression constantExpression)
		{
			var svalue = constantExpression.Value as string;
			if (_fbOptions.ExplicitStringLiteralTypes && constantExpression.Type == typeof(string))
			{
				Sql.Append("CAST(");
			}
			base.VisitConstant(constantExpression);
			if (_fbOptions.ExplicitStringLiteralTypes && constantExpression.Type == typeof(string))
			{
				Sql.Append(" AS ");
				Sql.Append(((IFbTypeMappingSource)Dependencies.TypeMappingSource).StringLiteralQueryType(svalue));
				Sql.Append(")");
			}
			return constantExpression;
		}

		public virtual Expression VisitSubstring(FbSubstringExpression substringExpression)
		{
			Sql.Append("SUBSTRING(");
			Visit(substringExpression.ValueExpression);
			Sql.Append(" FROM ");
			Visit(substringExpression.FromExpression);
			if (substringExpression.ForExpression != null)
			{
				Sql.Append(" FOR ");
				Visit(substringExpression.ForExpression);
			}
			Sql.Append(")");
			return substringExpression;
		}

		public virtual Expression VisitExtract(FbExtractExpression extractExpression)
		{
			Sql.Append("EXTRACT(");
			Sql.Append(extractExpression.Part);
			Sql.Append(" FROM ");
			Visit(extractExpression.ValueExpression);
			Sql.Append(")");
			return extractExpression;
		}

		public virtual Expression VisitDateMember(FbDateTimeDateMemberExpression dateTimeDateMemberExpression)
		{
			Sql.Append("CAST(");
			Visit(dateTimeDateMemberExpression.ValueExpression);
			Sql.Append(" AS DATE)");
			return dateTimeDateMemberExpression;
		}

		protected override void GeneratePseudoFromClause()
		{
			Sql.Append(" FROM RDB$DATABASE");
		}
	}
}
