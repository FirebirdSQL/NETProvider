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

//$Authors = Jiri Cincura (jiri@cincura.net), Jean Ressouche, Rafael Almeida (ralms@ralms.net)

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal
{
	public class FbMathTranslator : IMethodCallTranslator
	{
		static readonly Dictionary<MethodInfo, string> SupportedMethodTranslations = new Dictionary<MethodInfo, string>
		{
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(decimal) }), "ABS" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(double) }), "ABS" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(float) }), "ABS" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(int) }), "ABS" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(long) }), "ABS" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(sbyte) }), "ABS" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(short) }), "ABS" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Ceiling), new[] { typeof(decimal) }), "CEILING" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Ceiling), new[] { typeof(double) }), "CEILING" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Floor), new[] { typeof(decimal) }), "FLOOR" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Floor), new[] { typeof(double) }), "FLOOR" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Pow), new[] { typeof(double), typeof(double) }), "POWER" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Exp), new[] { typeof(double) }), "EXP" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Log10), new[] { typeof(double) }), "LOG10" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Log), new[] { typeof(double) }), "LOG" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Log), new[] { typeof(double), typeof(double) }), "LOG" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Sqrt), new[] { typeof(double) }), "SQRT" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Acos), new[] { typeof(double) }), "ACOS" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Asin), new[] { typeof(double) }), "ASIN" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Atan), new[] { typeof(double) }), "ATAN" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Atan2), new[] { typeof(double), typeof(double) }), "ATAN2" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Cos), new[] { typeof(double) }), "COS" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Sin), new[] { typeof(double) }), "SIN" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Tan), new[] { typeof(double) }), "TAN" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(decimal) }), "SIGN" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(double) }), "SIGN" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(float) }), "SIGN" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(int) }), "SIGN" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(long) }), "SIGN" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(sbyte) }), "SIGN" },
			{ typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(short) }), "SIGN" }
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

		public virtual Expression Translate(MethodCallExpression methodCallExpression)
		{
			var method = methodCallExpression.Method;
			if (SupportedMethodTranslations.TryGetValue(method, out var sqlFunctionName))
			{
				return new SqlFunctionExpression(
					sqlFunctionName,
					methodCallExpression.Type,
					methodCallExpression.Arguments);
			}

			if (TruncateMethodInfos.Contains(method))
			{
				var firstArgument = methodCallExpression.Arguments[0];

				if (firstArgument.NodeType == ExpressionType.Convert)
				{
					firstArgument = new ExplicitCastExpression(firstArgument, firstArgument.Type);
				}

				return new SqlFunctionExpression(
					"TRUNC",
					methodCallExpression.Type,
					new[] { firstArgument, Expression.Constant(0) });
			}

			if (RoundMethodInfos.Contains(method))
			{
				var firstArgument = methodCallExpression.Arguments[0];

				if (firstArgument.NodeType == ExpressionType.Convert)
				{
					firstArgument = new ExplicitCastExpression(firstArgument, firstArgument.Type);
				}

				return new SqlFunctionExpression(
					"ROUND",
					methodCallExpression.Type,
					methodCallExpression.Arguments.Count == 1
						? new[] { firstArgument, Expression.Constant(0) }
						: new[] { firstArgument, methodCallExpression.Arguments[1] });
			}

			return null;
		}
	}
}
