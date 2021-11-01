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

using System.Linq.Expressions;
using FirebirdSql.EntityFrameworkCore.Firebird.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.Internal
{
	public class FbSqlExpressionFactory : SqlExpressionFactory
	{
		readonly IRelationalTypeMappingSource _typeMappingSource;

		public FbSqlExpressionFactory(SqlExpressionFactoryDependencies dependencies)
			: base(dependencies)
		{
			_typeMappingSource = dependencies.TypeMappingSource;
		}

		public FbSubstringExpression Substring(SqlExpression valueExpression, SqlExpression fromExpression, SqlExpression forExpression)
			=> (FbSubstringExpression)ApplyDefaultTypeMapping(new FbSubstringExpression(valueExpression, fromExpression, forExpression, null));

		public FbExtractExpression Extract(string part, SqlExpression valueExpression)
			=> (FbExtractExpression)ApplyDefaultTypeMapping(new FbExtractExpression(part, valueExpression, null));

		public FbDateTimeDateMemberExpression DateTimeDateMember(SqlExpression valueExpression)
			=> (FbDateTimeDateMemberExpression)ApplyDefaultTypeMapping(new FbDateTimeDateMemberExpression(valueExpression, null));

		public FbTrimExpression Trim(string where, SqlExpression whatExpression, SqlExpression valueExpression)
			=> (FbTrimExpression)ApplyDefaultTypeMapping(new FbTrimExpression(where, whatExpression, valueExpression, null));

		public override SqlExpression ApplyTypeMapping(SqlExpression sqlExpression, RelationalTypeMapping typeMapping)
			=> sqlExpression == null || sqlExpression.TypeMapping != null
				? sqlExpression
				: sqlExpression switch
				{
					FbSubstringExpression e => ApplyTypeMappingOnSubstring(e),
					FbExtractExpression e => ApplyTypeMappingOnExtract(e),
					FbDateTimeDateMemberExpression e => ApplyTypeMappingOnDateTimeDateMember(e),
					FbTrimExpression e => ApplyTypeMappingOnTrim(e),
					_ => base.ApplyTypeMapping(sqlExpression, typeMapping)
				};

		SqlExpression ApplyTypeMappingOnSubstring(FbSubstringExpression expression)
		{
			return new FbSubstringExpression(
				ApplyDefaultTypeMapping(expression.ValueExpression),
				ApplyDefaultTypeMapping(expression.FromExpression),
				ApplyDefaultTypeMapping(expression.ForExpression),
				expression.TypeMapping ?? _typeMappingSource.FindMapping(expression.Type));
		}

		SqlExpression ApplyTypeMappingOnExtract(FbExtractExpression expression)
		{
			return new FbExtractExpression(
				expression.Part,
				ApplyDefaultTypeMapping(expression.ValueExpression),
				expression.TypeMapping ?? _typeMappingSource.FindMapping(expression.Type));
		}

		SqlExpression ApplyTypeMappingOnDateTimeDateMember(FbDateTimeDateMemberExpression expression)
		{
			return new FbDateTimeDateMemberExpression(
				ApplyDefaultTypeMapping(expression.ValueExpression),
				expression.TypeMapping ?? _typeMappingSource.FindMapping(expression.Type));
		}

		SqlExpression ApplyTypeMappingOnTrim(FbTrimExpression expression)
		{
			return new FbTrimExpression(
				expression.Where,
				ApplyDefaultTypeMapping(expression.WhatExpression),
				ApplyDefaultTypeMapping(expression.ValueExpression),
				expression.TypeMapping ?? _typeMappingSource.FindMapping(expression.Type));
		}
	}
}
