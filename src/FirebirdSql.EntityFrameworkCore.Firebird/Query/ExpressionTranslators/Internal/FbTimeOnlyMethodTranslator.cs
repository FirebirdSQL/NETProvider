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

public class FbTimeOnlyMethodTranslator : IMethodCallTranslator
{
	static readonly MethodInfo FromDateTime = typeof(TimeOnly).GetRuntimeMethod(nameof(TimeOnly.FromDateTime), [typeof(DateTime)]);
	static readonly MethodInfo FromTimeSpan = typeof(TimeOnly).GetRuntimeMethod(nameof(TimeOnly.FromTimeSpan), [typeof(TimeSpan)]);

	readonly ISqlExpressionFactory _sqlExpressionFactory;

	public FbTimeOnlyMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
	{
		_sqlExpressionFactory = sqlExpressionFactory;
	}

	public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
	{
		if (method.DeclaringType != typeof(TimeOnly))
		{
			return null;
		}

		if ((method == FromDateTime || method == FromTimeSpan) && instance is null && arguments.Count == 1)
		{
			return _sqlExpressionFactory.Convert(arguments[0], typeof(TimeOnly));
		}

		return null;
	}
}
