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

using System;
using System.Collections.Generic;
using System.Reflection;
using FirebirdSql.EntityFrameworkCore.Firebird.Query.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal
{
	public class FbDateAddTranslator : IMethodCallTranslator
	{
		static readonly Dictionary<MethodInfo, string> MethodInfoDatePartMapping = new Dictionary<MethodInfo, string>
		{
			{  typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddYears), new[] { typeof(int) }), "YEAR" },
			{  typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMonths), new[] { typeof(int) }), "MONTH" },
			{  typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddDays), new[] { typeof(double) }), "DAY" },
			{  typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddHours), new[] { typeof(double) }), "HOUR" },
			{  typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMinutes), new[] { typeof(double) }), "MINUTE" },
			{  typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddSeconds), new[] { typeof(double) }), "SECOND" },
			{  typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMilliseconds), new[] { typeof(double) }), "MILLISECOND" },
		};

		readonly FbSqlExpressionFactory _fbSqlExpressionFactory;

		public FbDateAddTranslator(FbSqlExpressionFactory fbSqlExpressionFactory)
		{
			_fbSqlExpressionFactory = fbSqlExpressionFactory;
		}

		public SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
		{
			if (!MethodInfoDatePartMapping.TryGetValue(method, out var part))
				return null;

			return _fbSqlExpressionFactory.Function(
				"DATEADD",
				new[] { _fbSqlExpressionFactory.Fragment(part), arguments[0], instance },
				instance.Type);
		}
	}
}
