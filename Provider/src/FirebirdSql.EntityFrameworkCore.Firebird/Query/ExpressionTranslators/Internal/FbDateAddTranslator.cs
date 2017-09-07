using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

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
			{  typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddYears), new[] { typeof(int) }), "YEAR" },
			{  typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddMonths), new[] { typeof(int) }), "MONTH" },
			{  typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddDays), new[] { typeof(double) }), "DAY" },
			{  typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddHours), new[] { typeof(double) }), "HOUR" },
			{  typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddMinutes), new[] { typeof(double) }), "MINUTE" },
			{  typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddSeconds), new[] { typeof(double) }), "SECOND" },
			{  typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddMilliseconds), new[] { typeof(double) }), "MILLISECOND" },
		};

		public virtual Expression Translate(MethodCallExpression methodCallExpression)
		{
			if (!MethodInfoDatePartMapping.TryGetValue(methodCallExpression.Method, out var part))
				return null;

			return new SqlFunctionExpression("DATEADD", methodCallExpression.Type, new[]
			{
				new SqlFragmentExpression(part),
				methodCallExpression.Arguments.First(),
				methodCallExpression.Object,
			});
		}
	}
}
