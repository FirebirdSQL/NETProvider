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
