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

public class FbStringTrimTranslator : IMethodCallTranslator
{
	static readonly MethodInfo TrimWithoutArgsMethod = typeof(string).GetRuntimeMethod(nameof(string.Trim), new Type[] { });
	static readonly MethodInfo TrimWithCharArgMethod = typeof(string).GetRuntimeMethod(nameof(string.Trim), new[] { typeof(char) });
	static readonly MethodInfo TrimEndWithoutArgsMethod = typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), new Type[] { });
	static readonly MethodInfo TrimEndWithCharArgMethod = typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), new[] { typeof(char) });
	static readonly MethodInfo TrimStartWithoutArgsMethod = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), new Type[] { });
	static readonly MethodInfo TrimStartWithCharArgMethod = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), new[] { typeof(char) });

	readonly FbSqlExpressionFactory _fbSqlExpressionFactory;

	public FbStringTrimTranslator(FbSqlExpressionFactory fbSqlExpressionFactory)
	{
		_fbSqlExpressionFactory = fbSqlExpressionFactory;
	}

	public SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
	{
		if (!TryGetTrimDefinition(instance, method, arguments, out var trimArguments, out var nullability))
		{
			return null;
		}
		return _fbSqlExpressionFactory.SpacedFunction(
			"TRIM",
			trimArguments,
			true,
			nullability,
			typeof(string));
	}

	bool TryGetTrimDefinition(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments, out IEnumerable<SqlExpression> trimArguments, out IEnumerable<bool> nullability)
	{
		if (method.Equals(TrimWithoutArgsMethod))
		{
			trimArguments = new[] { _fbSqlExpressionFactory.Fragment("BOTH"), _fbSqlExpressionFactory.Fragment("FROM"), instance };
			nullability = new[] { false, false, true };
			return true;
		}
		if (method.Equals(TrimWithCharArgMethod))
		{
			trimArguments = new[] { _fbSqlExpressionFactory.Fragment("BOTH"), _fbSqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]), _fbSqlExpressionFactory.Fragment("FROM"), instance };
			nullability = new[] { false, true, false, true };
			return true;
		}
		if (method.Equals(TrimEndWithoutArgsMethod))
		{
			trimArguments = new[] { _fbSqlExpressionFactory.Fragment("TRAILING"), _fbSqlExpressionFactory.Fragment("FROM"), instance };
			nullability = new[] { false, false, true };
			return true;
		}
		if (method.Equals(TrimEndWithCharArgMethod))
		{
			trimArguments = new[] { _fbSqlExpressionFactory.Fragment("TRAILING"), _fbSqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]), _fbSqlExpressionFactory.Fragment("FROM"), instance };
			nullability = new[] { false, true, false, true };
			return true;
		}
		if (method.Equals(TrimStartWithoutArgsMethod))
		{
			trimArguments = new[] { _fbSqlExpressionFactory.Fragment("LEADING"), _fbSqlExpressionFactory.Fragment("FROM"), instance };
			nullability = new[] { false, false, true };
			return true;
		}
		if (method.Equals(TrimStartWithCharArgMethod))
		{
			trimArguments = new[] { _fbSqlExpressionFactory.Fragment("LEADING"), _fbSqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]), _fbSqlExpressionFactory.Fragment("FROM"), instance };
			nullability = new[] { false, true, false, true };
			return true;
		}
		trimArguments = default;
		nullability = default;
		return false;
	}

}
