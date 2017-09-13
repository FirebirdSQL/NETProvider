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
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal
{
	public class FbObjectToStringTranslator : IMethodCallTranslator
	{
		static readonly HashSet<Type> SupportedTypes = new HashSet<Type>
		{
			typeof(int),
			typeof(long),
			typeof(DateTime),
			typeof(Guid),
			typeof(bool),
			typeof(byte),
			typeof(byte[]),
			typeof(double),
			typeof(DateTimeOffset),
			typeof(char),
			typeof(short),
			typeof(float),
			typeof(decimal),
			typeof(TimeSpan),
			typeof(uint),
			typeof(ushort),
			typeof(ulong),
			typeof(sbyte),
		};

		public virtual Expression Translate(MethodCallExpression methodCallExpression)
		{
			if (methodCallExpression.Method.Name == nameof(ToString) && methodCallExpression.Arguments.Count == 0 && methodCallExpression.Object != null && SupportedTypes.Contains(methodCallExpression.Object.Type.UnwrapNullableType().UnwrapEnumType()))
			{
				return new ExplicitCastExpression(methodCallExpression.Object, typeof(string));
			}
			return null;
		}
	}
}
