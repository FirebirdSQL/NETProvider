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

public class FbTimeSpanPartComponentTranslator : IMemberTranslator
{
	const string SecondPart = "SECOND";
	const string MillisecondPart = "MILLISECOND";
	private static readonly Dictionary<MemberInfo, string> MemberMapping = new Dictionary<MemberInfo, string>
		{
			{  typeof(TimeSpan).GetProperty(nameof(TimeSpan.Hours)), "HOUR" },
			{  typeof(TimeSpan).GetProperty(nameof(TimeSpan.Minutes)), "MINUTE" },
			{  typeof(TimeSpan).GetProperty(nameof(TimeSpan.Seconds)), SecondPart },
			{  typeof(TimeSpan).GetProperty(nameof(TimeSpan.Milliseconds)), MillisecondPart },
		};

	readonly FbSqlExpressionFactory _fbSqlExpressionFactory;

	public FbTimeSpanPartComponentTranslator(FbSqlExpressionFactory fbSqlExpressionFactory)
	{
		_fbSqlExpressionFactory = fbSqlExpressionFactory;
	}

	public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
	{
		if (!MemberMapping.TryGetValue(member, out var part))
			return null;

		var result = (SqlExpression)_fbSqlExpressionFactory.SpacedFunction(
			"EXTRACT",
			new[] { _fbSqlExpressionFactory.Fragment(part), _fbSqlExpressionFactory.Fragment("FROM"), instance },
			true,
			new[] { false, false, true },
			typeof(int));
		if (part == SecondPart || part == MillisecondPart)
		{
			result = _fbSqlExpressionFactory.Function("TRUNC", new[] { result }, true, new[] { true }, typeof(int));
		}
		return result;
	}
}
