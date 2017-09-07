using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Sql;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.Sql.Internal
{
	public class FbQuerySqlGeneratorFactory : QuerySqlGeneratorFactoryBase
	{
		public FbQuerySqlGeneratorFactory(QuerySqlGeneratorDependencies dependencies)
			: base(dependencies)
		{ }

		public override IQuerySqlGenerator CreateDefault(SelectExpression selectExpression)
			=> new FbQuerySqlGenerator(Dependencies, selectExpression);
	}
}
