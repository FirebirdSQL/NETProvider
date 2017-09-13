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
using FirebirdSql.EntityFrameworkCore.Firebird.Query.Sql.Internal;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.Expressions.Internal
{
	public class FbSubstringExpression : Expression
	{
		public virtual Expression ValueExpression { get; }
		public virtual Expression FromExpression { get; }
		public virtual Expression ForExpression { get; }

		public FbSubstringExpression(Expression valueExpression, Expression fromExpression, Expression forExpression)
		{
			ValueExpression = valueExpression;
			FromExpression = fromExpression;
			ForExpression = forExpression;
		}

		public override ExpressionType NodeType => ExpressionType.Extension;
		public override bool CanReduce => false;
		public override Type Type => typeof(string);

		protected override Expression Accept(ExpressionVisitor visitor)
		{
			if (visitor is IFbExpressionVisitor specificVisitor)
			{
				return specificVisitor.VisitSubstring(this);
			}
			else
			{
				return base.Accept(visitor);
			}
		}

		protected override Expression VisitChildren(ExpressionVisitor visitor)
		{
			var newValueExpression = visitor.Visit(ValueExpression);
			var newFromExpression = visitor.Visit(FromExpression);
			var newForExpression = visitor.Visit(ForExpression);

			return newValueExpression != ValueExpression || newFromExpression != FromExpression || newForExpression != ForExpression
				? new FbSubstringExpression(newValueExpression, newFromExpression, newForExpression)
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
			return obj.GetType() == GetType() && Equals((FbSubstringExpression)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = ValueExpression.GetHashCode();
				hashCode = (hashCode * 397) ^ FromExpression.GetHashCode();
				hashCode = (hashCode * 397) ^ ForExpression.GetHashCode();
				return hashCode;
			}
		}
	}
}
