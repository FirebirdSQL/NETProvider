using Microsoft.EntityFrameworkCore.Storage;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal
{
	public class FbSqlGenerationHelper : RelationalSqlGenerationHelper
	{
		public FbSqlGenerationHelper(RelationalSqlGenerationHelperDependencies dependencies)
			: base(dependencies)
		{ }
	}
}
