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
using System.Reflection;
using FirebirdSql.EntityFrameworkCore.Firebird.Query.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal;

public class FbObjectToStringTranslator : IMethodCallTranslator
{
	static readonly HashSet<Type> SupportedTypes = new HashSet<Type>
		{
			typeof(int),
			typeof(long),
			typeof(DateTime),
			typeof(byte),
			typeof(byte[]),
			typeof(double),
			typeof(char),
			typeof(short),
			typeof(float),
			typeof(decimal),
			typeof(TimeSpan),
			typeof(uint),
			typeof(ushort),
			typeof(ulong),
			typeof(sbyte),
			typeof(DateOnly),
			typeof(TimeOnly),
		};

	readonly FbSqlExpressionFactory _fbSqlExpressionFactory;

	public FbObjectToStringTranslator(FbSqlExpressionFactory fbSqlExpressionFactory)
	{
		_fbSqlExpressionFactory = fbSqlExpressionFactory;
	}

	public SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
	{
		if (instance == null || method.Name != nameof(ToString) || arguments.Count != 0)
		{
			return null;
		}

		if (instance.TypeMapping?.ClrType == typeof(string))
		{
			return instance;
		}

		if (SupportedTypes.Contains(instance.Type))
		{
			return _fbSqlExpressionFactory.Convert(instance, typeof(string));
		}
		else if (instance.Type == typeof(Guid))
		{
			return _fbSqlExpressionFactory.Function("UUID_TO_CHAR", new[] { instance }, true, new[] { true }, typeof(string));
		}
		else if (instance.Type == typeof(bool))
		{
			if (instance is not ColumnExpression { IsNullable: false })
			{
				return _fbSqlExpressionFactory.Case(
					instance,
					new[]
					{
						new CaseWhenClause(
							_fbSqlExpressionFactory.Constant(false),
							_fbSqlExpressionFactory.Constant(false.ToString())),
						new CaseWhenClause(
							_fbSqlExpressionFactory.Constant(true),
							_fbSqlExpressionFactory.Constant(true.ToString()))
					},
					_fbSqlExpressionFactory.Constant(string.Empty));
			}
			else
			{
				return _fbSqlExpressionFactory.Case(
					new[]
					{
						new CaseWhenClause(
							instance,
							_fbSqlExpressionFactory.Constant(true.ToString()))
					},
					_fbSqlExpressionFactory.Constant(false.ToString()));
			}
		}

		return null;
	}
}
