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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal;

public class SqlServerDateOnlyMethodTranslator : IMethodCallTranslator
{
	readonly Dictionary<MethodInfo, string> _methodInfoDatePartMapping = new()
	{
		{ typeof(DateOnly).GetRuntimeMethod(nameof(DateOnly.AddYears), [typeof(int)]), "year" },
		{ typeof(DateOnly).GetRuntimeMethod(nameof(DateOnly.AddMonths), [typeof(int)]), "month" },
		{ typeof(DateOnly).GetRuntimeMethod(nameof(DateOnly.AddDays), [typeof(int)]), "day" }
	};

	readonly ISqlExpressionFactory _sqlExpressionFactory;

	public SqlServerDateOnlyMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
	{
		_sqlExpressionFactory = sqlExpressionFactory;
	}

	public SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
	{
		if (_methodInfoDatePartMapping.TryGetValue(method, out var datePart) && instance != null)
		{
			instance = _sqlExpressionFactory.ApplyDefaultTypeMapping(instance);

			return _sqlExpressionFactory.Function(
				"DATEADD",
				new[] { _sqlExpressionFactory.Fragment(datePart), _sqlExpressionFactory.Convert(arguments[0], typeof(int)), instance },
				nullable: true,
				argumentsPropagateNullability: new[] { false, true, true },
				instance.Type,
				instance.TypeMapping);
		}

		if (method.DeclaringType == typeof(DateOnly) && method.Name == nameof(DateOnly.FromDateTime) && arguments.Count == 1)
		{
			return _sqlExpressionFactory.Convert(arguments[0], typeof(DateOnly));
		}

		return null;
	}
}
