using System;
using System.Text;
using Microsoft.EntityFrameworkCore.Update;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Update.Internal
{
	public class FbUpdateSqlGenerator : UpdateSqlGenerator, IFbUpdateSqlGenerator
	{
		public FbUpdateSqlGenerator(UpdateSqlGeneratorDependencies dependencies)
			: base(dependencies)
		{ }

		protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, ColumnModification columnModification)
		{
#warning Finish
			throw new NotImplementedException();
		}

		protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
		{
#warning Finish
			throw new NotImplementedException();
		}
	}
}
