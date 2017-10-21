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

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Migrations
{
	public class FbMigrationSqlGeneratorBehavior : IFbMigrationSqlGeneratorBehavior
	{
		readonly ISqlGenerationHelper _sqlHelper;

		public FbMigrationSqlGeneratorBehavior(ISqlGenerationHelper sqlHelper)
		{
			_sqlHelper = sqlHelper;
		}

		public IEnumerable<MigrationCommandListBuilder> CreateIdentityForColumn(MigrationCommandListBuilder builder, string columnName, string tableName)
		{
			var mergeColumnTable = string.Format("{0}_{1}", columnName, tableName).ToUpper();
			var sequenceName = string.Format("GEN_{0}", mergeColumnTable);
			var triggerName = string.Format("ID_{0}", mergeColumnTable);

			builder.AppendLine("EXECUTE BLOCK");
			builder.AppendLine("AS");
			builder.AppendLine("BEGIN");
			builder.Append("if (not exists(select 1 from rdb$generators where rdb$generator_name = '");
			builder.Append(sequenceName);
			builder.Append("')) then");
			builder.AppendLine();
			builder.AppendLine("begin");
			builder.Indent();
			builder.Append("execute statement 'create sequence ");
			builder.Append(sequenceName);
			builder.Append("';");
			builder.DecrementIndent();
			builder.AppendLine();
			builder.AppendLine("end");
			builder.AppendLine("END");
			yield return builder;

			builder.Append("CREATE OR ALTER TRIGGER ");
			builder.Append(_sqlHelper.DelimitIdentifier(triggerName));
			builder.Append(" ACTIVE BEFORE INSERT ON ");
			builder.Append(_sqlHelper.DelimitIdentifier(tableName));
			builder.AppendLine();
			builder.AppendLine("AS");
			builder.AppendLine("BEGIN");
			builder.Append("if (new.");
			builder.Append(_sqlHelper.DelimitIdentifier(columnName));
			builder.Append(" is null) then");
			builder.AppendLine();
			builder.AppendLine("begin");
			builder.Indent();
			builder.Append("new.");
			builder.Append(_sqlHelper.DelimitIdentifier(columnName));
			builder.Append(" = next value for ");
			builder.Append(sequenceName);
			builder.Append(";");
			builder.DecrementIndent();
			builder.AppendLine();
			builder.AppendLine("end");
			builder.Append("END");
			yield return builder;
		}

		public IEnumerable<MigrationCommandListBuilder> DropIdentityForColumn(MigrationCommandListBuilder builder, string columnName, string tableName)
		{
			throw new System.NotImplementedException();
		}
	}
}
