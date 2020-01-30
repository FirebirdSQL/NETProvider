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
	public class FbDateTimeDatePartComponentTranslator : IMemberTranslator
	{
		const string YearDayPart = "YEARDAY";
		static readonly Dictionary<MemberInfo, string> MemberDatePartMapping = new Dictionary<MemberInfo, string>
		{
			{  typeof(DateTime).GetProperty(nameof(DateTime.Year)), "YEAR" },
			{  typeof(DateTime).GetProperty(nameof(DateTime.Month)), "MONTH" },
			{  typeof(DateTime).GetProperty(nameof(DateTime.Day)), "DAY" },
			{  typeof(DateTime).GetProperty(nameof(DateTime.Hour)), "HOUR" },
			{  typeof(DateTime).GetProperty(nameof(DateTime.Minute)), "MINUTE" },
			{  typeof(DateTime).GetProperty(nameof(DateTime.Second)), "SECOND" },
			{  typeof(DateTime).GetProperty(nameof(DateTime.Millisecond)), "MILLISECOND" },
			{  typeof(DateTime).GetProperty(nameof(DateTime.DayOfYear)), YearDayPart },
			{  typeof(DateTime).GetProperty(nameof(DateTime.DayOfWeek)), "WEEKDAY" },
		};

		readonly FbSqlExpressionFactory _fbSqlExpressionFactory;

		public FbDateTimeDatePartComponentTranslator(FbSqlExpressionFactory fbSqlExpressionFactory)
		{
			_fbSqlExpressionFactory = fbSqlExpressionFactory;
		}

		public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
		{
			if (!MemberDatePartMapping.TryGetValue(member, out var part))
				return null;

			var result = (SqlExpression)_fbSqlExpressionFactory.Extract(part, instance);
			if (part == YearDayPart)
			{
				result = _fbSqlExpressionFactory.Add(result, _fbSqlExpressionFactory.Constant(1));
			}
			return result;
		}
	}
}
