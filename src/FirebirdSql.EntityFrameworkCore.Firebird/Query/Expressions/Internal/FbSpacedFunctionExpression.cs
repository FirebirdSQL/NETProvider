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
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.Expressions.Internal;

public class FbSpacedFunctionExpression : SqlFunctionExpression, IEquatable<FbSpacedFunctionExpression>
{
	public FbSpacedFunctionExpression(string name, IEnumerable<SqlExpression> arguments, bool nullable, IEnumerable<bool> argumentsPropagateNullability, Type type, RelationalTypeMapping typeMapping)
		: base(name, arguments, nullable, argumentsPropagateNullability, type, typeMapping)
	{ }

	public override SqlFunctionExpression ApplyTypeMapping(RelationalTypeMapping typeMapping)
	{
		return new FbSpacedFunctionExpression(Name, Arguments, IsNullable, ArgumentsPropagateNullability, Type, typeMapping ?? TypeMapping);
	}

	protected override Expression VisitChildren(ExpressionVisitor visitor)
	{
		var changed = false;

		var arguments = new SqlExpression[Arguments.Count];
		for (var i = 0; i < arguments.Length; i++)
		{
			arguments[i] = (SqlExpression)visitor.Visit(Arguments[i]);
			changed |= arguments[i] != Arguments[i];
		}

		return changed
			? new FbSpacedFunctionExpression(Name, arguments, IsNullable, ArgumentsPropagateNullability, Type, TypeMapping)
			: this;
	}

	public override SqlFunctionExpression Update(SqlExpression instance, IReadOnlyList<SqlExpression> arguments)
	{
		if (instance != null)
			throw new ArgumentException("Instance must be null.", nameof(instance));

		return !arguments.SequenceEqual(Arguments)
			? new FbSpacedFunctionExpression(Name, arguments, IsNullable, ArgumentsPropagateNullability, Type, TypeMapping)
			: this;
	}

	public override bool Equals(object obj)
	{
		return obj != null
			&& (ReferenceEquals(this, obj)
				|| obj is FbSpacedFunctionExpression fbExtraFunctionExpression
				&& Equals(fbExtraFunctionExpression));
	}

	public virtual bool Equals(FbSpacedFunctionExpression other)
	{
		return base.Equals(other);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
