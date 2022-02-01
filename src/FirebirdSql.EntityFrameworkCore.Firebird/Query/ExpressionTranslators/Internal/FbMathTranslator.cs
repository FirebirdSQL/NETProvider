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
using System.Reflection;
using FirebirdSql.EntityFrameworkCore.Firebird.Query.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal;

public class FbMathTranslator : IMethodCallTranslator
{
	static readonly Dictionary<MethodInfo, string> SupportedMethodTranslations = new Dictionary<MethodInfo, string>
		{
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(decimal) }), "ABS" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(double) }), "ABS" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(short) }), "ABS" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(int) }), "ABS" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(long) }), "ABS" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(sbyte) }), "ABS" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(float) }), "ABS" },

			{ typeof(Math).GetRuntimeMethod(nameof(Math.Ceiling), new[] { typeof(double) }), "CEILING" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Ceiling), new[] { typeof(decimal) }), "CEILING" },

			{ typeof(Math).GetRuntimeMethod(nameof(Math.Floor), new[] { typeof(double) }), "FLOOR" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Floor), new[] { typeof(decimal) }), "FLOOR" },

			{ typeof(Math).GetRuntimeMethod(nameof(Math.Pow), new[] { typeof(double), typeof(double) }), "POWER" },

			{ typeof(Math).GetRuntimeMethod(nameof(Math.Exp), new[] { typeof(double) }), "EXP" },

			{ typeof(Math).GetRuntimeMethod(nameof(Math.Log10), new[] { typeof(double) }), "LOG10" },

			{ typeof(Math).GetRuntimeMethod(nameof(Math.Log), new[] { typeof(double) }), "LN" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Log), new[] { typeof(double), typeof(double) }), "LOG" },

			{ typeof(Math).GetRuntimeMethod(nameof(Math.Sqrt), new[] { typeof(double) }), "SQRT" },

			{ typeof(Math).GetRuntimeMethod(nameof(Math.Acos), new[] { typeof(double) }), "ACOS" },

			{ typeof(Math).GetRuntimeMethod(nameof(Math.Asin), new[] { typeof(double) }), "ASIN" },

			{ typeof(Math).GetRuntimeMethod(nameof(Math.Atan), new[] { typeof(double) }), "ATAN" },

			{ typeof(Math).GetRuntimeMethod(nameof(Math.Atan2), new[] { typeof(double), typeof(double) }), "ATAN2" },

			{ typeof(Math).GetRuntimeMethod(nameof(Math.Cos), new[] { typeof(double) }), "COS" },

			{ typeof(Math).GetRuntimeMethod(nameof(Math.Sin), new[] { typeof(double) }), "SIN" },

			{ typeof(Math).GetRuntimeMethod(nameof(Math.Tan), new[] { typeof(double) }), "TAN" },

			{ typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(float) }), "SIGN" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(long) }), "SIGN" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(int) }), "SIGN" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(sbyte) }), "SIGN" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(double) }), "SIGN" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(decimal) }), "SIGN" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(short) }), "SIGN" },

			{ typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(float), typeof(float) }), "MAXVALUE" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(sbyte), typeof(sbyte) }), "MAXVALUE" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(ulong), typeof(ulong) }), "MAXVALUE" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(uint), typeof(uint) }), "MAXVALUE" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(long), typeof(long) }), "MAXVALUE" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(ushort), typeof(ushort) }), "MAXVALUE" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(short), typeof(short) }), "MAXVALUE" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(double), typeof(double) }), "MAXVALUE" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(decimal), typeof(decimal) }), "MAXVALUE" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(byte), typeof(byte) }), "MAXVALUE" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(int), typeof(int) }), "MAXVALUE" },

			{ typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(float), typeof(float) }), "MINVALUE" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(sbyte), typeof(sbyte) }), "MINVALUE" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(ulong), typeof(ulong) }), "MINVALUE" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(uint), typeof(uint) }), "MINVALUE" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(long), typeof(long) }), "MINVALUE" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(ushort), typeof(ushort) }), "MINVALUE" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(short), typeof(short) }), "MINVALUE" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(double), typeof(double) }), "MINVALUE" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(decimal), typeof(decimal) }), "MINVALUE" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(byte), typeof(byte) }), "MINVALUE" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(int), typeof(int) }), "MINVALUE" },
		};

	static readonly HashSet<MethodInfo> TruncateMethodInfos = new HashSet<MethodInfo>
		{
			typeof(Math).GetRuntimeMethod(nameof(Math.Truncate), new[] { typeof(decimal) }),
			typeof(Math).GetRuntimeMethod(nameof(Math.Truncate), new[] { typeof(double) })
		};

	static readonly HashSet<MethodInfo> RoundMethodInfos = new HashSet<MethodInfo>
		{
			typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(decimal) }),
			typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(double) }),
			typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(decimal), typeof(int) }),
			typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(double), typeof(int) })
		};

	readonly FbSqlExpressionFactory _fbSqlExpressionFactory;

	public FbMathTranslator(FbSqlExpressionFactory fbSqlExpressionFactory)
	{
		_fbSqlExpressionFactory = fbSqlExpressionFactory;
	}

	public SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
	{
		if (SupportedMethodTranslations.TryGetValue(method, out var sqlFunctionName))
		{
			return _fbSqlExpressionFactory.Function(sqlFunctionName, arguments, true, arguments.Select(_ => true), method.ReturnType);
		}
		if (TruncateMethodInfos.Contains(method))
		{
			return _fbSqlExpressionFactory.ApplyDefaultTypeMapping(_fbSqlExpressionFactory.Function(
				"TRUNC",
				new[] { _fbSqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]), _fbSqlExpressionFactory.Constant(0) },
				true,
				new[] { true, false },
				method.ReturnType));
		}
		if (RoundMethodInfos.Contains(method))
		{
			var roundArguments = arguments.Count == 1
				? new[] { _fbSqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]), _fbSqlExpressionFactory.Constant(0) }
				: new[] { _fbSqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]), _fbSqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]) };
			var nullability = arguments.Count == 1
				? new[] { true, false }
				: new[] { true, true };
			return _fbSqlExpressionFactory.ApplyDefaultTypeMapping(_fbSqlExpressionFactory.Function(
				"ROUND",
				roundArguments,
				true,
				nullability,
				method.ReturnType));
		}
		return null;
	}
}
