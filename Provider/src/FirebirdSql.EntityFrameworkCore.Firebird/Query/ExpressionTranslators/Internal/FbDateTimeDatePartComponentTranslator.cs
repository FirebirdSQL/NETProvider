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
using FirebirdSql.EntityFrameworkCore.Firebird.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal
{
	public class FbDateTimeDatePartComponentTranslator : IMemberTranslator
	{
		static readonly Dictionary<MemberInfo, string> MemberDatePartMapping = new Dictionary<MemberInfo, string>
		{
			{  typeof(DateTime).GetProperty(nameof(DateTime.Year)), "YEAR" },
			{  typeof(DateTime).GetProperty(nameof(DateTime.Month)), "MONTH" },
			{  typeof(DateTime).GetProperty(nameof(DateTime.Day)), "DAY" },
			{  typeof(DateTime).GetProperty(nameof(DateTime.Hour)), "HOUR" },
			{  typeof(DateTime).GetProperty(nameof(DateTime.Minute)), "MINUTE" },
			{  typeof(DateTime).GetProperty(nameof(DateTime.Second)), "SECOND" },
			{  typeof(DateTime).GetProperty(nameof(DateTime.Millisecond)), "MILLISECOND" },
			{  typeof(DateTime).GetProperty(nameof(DateTime.DayOfYear)), "YEARDAY" },
			{  typeof(DateTime).GetProperty(nameof(DateTime.DayOfWeek)), "WEEKDAY" },
			{  typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.Year)), "YEAR" },
			{  typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.Month)), "MONTH" },
			{  typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.Day)), "DAY" },
			{  typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.Hour)), "HOUR" },
			{  typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.Minute)), "MINUTE" },
			{  typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.Second)), "SECOND" },
			{  typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.Millisecond)), "MILLISECOND" },
			{  typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.DayOfYear)), "YEARDAY" },
			{  typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.DayOfWeek)), "WEEKDAY" },
		};

		public virtual Expression Translate(MemberExpression memberExpression)
		{
			if (!MemberDatePartMapping.TryGetValue(memberExpression.Member, out var part))
				return null;

			return new FbExtractExpression(part, memberExpression.Expression);
		}
	}
}
