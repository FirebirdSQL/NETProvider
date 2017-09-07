using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Migrations.Internal
{
	public class FbHistoryRepository : HistoryRepository
	{
		protected FbHistoryRepository(HistoryRepositoryDependencies dependencies)
			: base(dependencies)
		{ }

		protected override string ExistsSql
		{
			get
			{
				var escapedTableName = SqlGenerationHelper.EscapeLiteral(TableName);
				return $@"
SELECT COUNT(*)
FROM rdb$relations r
WHERE
	COALESCE(r.rdb$system_flag, 0) = 0
	AND
	rdb$view_blr IS NULL
	AND
	rdb$relation_name = '{escapedTableName}'";
			}
		}

		protected override bool InterpretExistsResult(object value) => value != DBNull.Value;

		public override string GetCreateIfNotExistsScript() => GetCreateScript();

		public override string GetBeginIfExistsScript(string migrationId)
		{
			throw new NotSupportedException("Generating idempotent scripts is currently not supported.");
		}

		public override string GetBeginIfNotExistsScript(string migrationId)
		{
			throw new NotSupportedException("Generating idempotent scripts is currently not supported.");
		}

		public override string GetEndIfScript()
		{
			throw new NotSupportedException("Generating idempotent scripts is currently not supported.");
		}
	}
}
