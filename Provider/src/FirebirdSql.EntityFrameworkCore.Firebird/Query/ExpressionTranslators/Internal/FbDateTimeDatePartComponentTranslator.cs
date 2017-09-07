using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
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
			if (MemberDatePartMapping.TryGetValue(memberExpression.Member, out var part))
			{
				return new SqlFunctionExpression("EXTRACT", memberExpression.Type, new[]
				{
#warning Expression wrong
					new SqlFragmentExpression($"{part} FROM {memberExpression.Expression}")
				});
			}
			return null;
		}
	}
}
