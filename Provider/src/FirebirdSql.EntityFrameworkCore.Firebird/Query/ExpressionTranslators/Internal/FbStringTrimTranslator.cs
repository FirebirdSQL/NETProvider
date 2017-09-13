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
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal
{
	public class FbStringTrimTranslator : IMethodCallTranslator
	{
		static readonly MethodInfo TrimWithoutArgsMethod = typeof(string).GetRuntimeMethod(nameof(string.Trim), new Type[] { });
		static readonly MethodInfo TrimWithCharArrayArgMethod = typeof(string).GetRuntimeMethod(nameof(string.Trim), new[] { typeof(char[]) });

		public virtual Expression Translate(MethodCallExpression methodCallExpression)
		{
			if (methodCallExpression.Method.Equals(TrimWithoutArgsMethod) || methodCallExpression.Method.Equals(TrimWithCharArrayArgMethod)
				// no arguments
				&& ((methodCallExpression.Arguments[0] as ConstantExpression)?.Value as Array)?.Length == 0)
			{
				var sqlArguments = new[] { methodCallExpression.Object };
				return new SqlFunctionExpression(
					"TRIM",
					methodCallExpression.Type,
					sqlArguments);
			}
			return null;
		}
	}
}
