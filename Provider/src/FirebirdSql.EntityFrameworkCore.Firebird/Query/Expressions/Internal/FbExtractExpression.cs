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
using System.Linq.Expressions;
using FirebirdSql.EntityFrameworkCore.Firebird.Query.Sql.Internal;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.Expressions.Internal
{
	public class FbExtractExpression : Expression
	{
		public virtual string Part { get; }
		public virtual Expression ValueExpression { get; }

		public FbExtractExpression(string part, Expression valueExpression)
		{
			Part = part;
			ValueExpression = valueExpression;
		}

		public override ExpressionType NodeType => ExpressionType.Extension;
		public override bool CanReduce => false;
		public override Type Type => typeof(int);

		protected override Expression Accept(ExpressionVisitor visitor)
		{
			if (visitor is IFbExpressionVisitor specificVisitor)
			{
				return specificVisitor.VisitExtract(this);
			}
			else
			{
				return base.Accept(visitor);
			}
		}

		protected override Expression VisitChildren(ExpressionVisitor visitor)
		{
			var newValueExpression = visitor.Visit(ValueExpression);

			return newValueExpression != ValueExpression
				? new FbExtractExpression(Part, newValueExpression)
				: this;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			return obj.GetType() == GetType() && Equals((FbExtractExpression)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Part.GetHashCode();
				hashCode = (hashCode * 397) ^ ValueExpression.GetHashCode();
				return hashCode;
			}
		}
	}
}
