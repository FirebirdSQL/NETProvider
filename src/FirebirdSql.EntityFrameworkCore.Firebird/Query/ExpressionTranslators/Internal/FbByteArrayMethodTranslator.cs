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

using System.Collections.Generic;
using System.Reflection;
using FirebirdSql.EntityFrameworkCore.Firebird.Query.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal;

public class FbByteArrayMethodTranslator : IMethodCallTranslator
{
	readonly FbSqlExpressionFactory _fbSqlExpressionFactory;

	public FbByteArrayMethodTranslator(FbSqlExpressionFactory fbSqlExpressionFactory)
	{
		_fbSqlExpressionFactory = fbSqlExpressionFactory;
	}

	public SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
	{
		if (method.IsGenericMethod
			&& method.GetGenericMethodDefinition().Equals(EnumerableMethods.Contains)
			&& arguments[0].Type == typeof(byte[]))
		{
			var value = arguments[1] is SqlConstantExpression constantValue
				? _fbSqlExpressionFactory.Function("ASCII_CHAR", new[] { _fbSqlExpressionFactory.Constant((byte)constantValue.Value) }, false, new[] { false }, typeof(string))
				: _fbSqlExpressionFactory.Function("ASCII_CHAR", new[] { _fbSqlExpressionFactory.Convert(_fbSqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]), typeof(byte)) }, true, new[] { true }, typeof(string));

			return _fbSqlExpressionFactory.GreaterThan(
				_fbSqlExpressionFactory.Function(
					"POSITION",
					new[] { value, _fbSqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]) },
					true,
					new[] { true, true },
					typeof(int)),
				_fbSqlExpressionFactory.Constant(0));
		}
		return null;
	}
}
