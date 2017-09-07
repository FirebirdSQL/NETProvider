using System.Linq.Expressions;
using FirebirdSql.EntityFrameworkCore.Firebird.Query.Sql.Internal;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.Expressions.Internal
{
	public class FbSubstringExpression : Expression
	{
		public FbSubstringExpression(Expression valueExpression, Expression fromExpression, Expression forExpression)
		{
			ValueExpression = valueExpression;
			FromExpression = fromExpression;
			ForExpression = forExpression;
		}

		public virtual Expression ValueExpression { get; }
		public virtual Expression FromExpression { get; }
		public virtual Expression ForExpression { get; }

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
			var newSubjectExpression = visitor.Visit(ValueExpression);
			var newFromExpression = visitor.Visit(FromExpression);
			var newForExpression = visitor.Visit(ForExpression);

			return newFromExpression != FromExpression || newForExpression != ForExpression || newSubjectExpression != ValueExpression
				? new FbSubstringExpression(newSubjectExpression, newFromExpression, newForExpression)
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
