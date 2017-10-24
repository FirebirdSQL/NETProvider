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

		public virtual void CreateIdentityForColumn(string columnName, string tableName, MigrationCommandListBuilder builder)
		{
			var identitySequenceName = CreateIdentitySequenceName(columnName, tableName);

			builder.AppendLine("EXECUTE BLOCK");
			builder.AppendLine("AS");
			builder.AppendLine("BEGIN");
			builder.IncrementIndent();
			builder.Append("if (not exists(select 1 from rdb$generators where rdb$generator_name = '");
			builder.Append(identitySequenceName);
			builder.Append("')) then");
			builder.AppendLine();
			builder.AppendLine("begin");
			builder.IncrementIndent();
			builder.Append("execute statement 'create sequence ");
			builder.Append(identitySequenceName);
			builder.Append("'");
			builder.Append(_sqlHelper.StatementTerminator);
			builder.AppendLine();
			builder.DecrementIndent();
			builder.AppendLine("end");
			builder.DecrementIndent();
			builder.Append("END");
			builder.EndCommand();

			builder.Append("CREATE TRIGGER ");
			builder.Append(_sqlHelper.DelimitIdentifier(CreateTriggerName(columnName, tableName)));
			builder.Append(" ACTIVE BEFORE INSERT ON ");
			builder.Append(_sqlHelper.DelimitIdentifier(tableName));
			builder.AppendLine();
			builder.AppendLine("AS");
			builder.AppendLine("BEGIN");
			builder.IncrementIndent();
			builder.Append("if (new.");
			builder.Append(_sqlHelper.DelimitIdentifier(columnName));
			builder.Append(" is null) then");
			builder.AppendLine();
			builder.AppendLine("begin");
			builder.IncrementIndent();
			builder.Append("new.");
			builder.Append(_sqlHelper.DelimitIdentifier(columnName));
			builder.Append(" = next value for ");
			builder.Append(identitySequenceName);
			builder.Append(_sqlHelper.StatementTerminator);
			builder.AppendLine();
			builder.DecrementIndent();
			builder.AppendLine("end");
			builder.DecrementIndent();
			builder.Append("END");
			builder.EndCommand();
		}

		public virtual void DropIdentityForColumn(string columnName, string tableName, MigrationCommandListBuilder builder)
		{
			var triggerName = CreateTriggerName(columnName, tableName);

			builder.AppendLine("EXECUTE BLOCK");
			builder.AppendLine("AS");
			builder.AppendLine("BEGIN");
			builder.IncrementIndent();
			builder.Append("if (exists(select 1 from rdb$triggers where rdb$trigger_name = '");
			builder.Append(triggerName);
			builder.Append("')) then");
			builder.AppendLine();
			builder.AppendLine("begin");
			builder.IncrementIndent();
			builder.Append("execute statement 'drop trigger ");
			builder.Append(_sqlHelper.DelimitIdentifier(triggerName));
			builder.Append("'");
			builder.Append(_sqlHelper.StatementTerminator);
			builder.AppendLine();
			builder.DecrementIndent();
			builder.AppendLine("end");
			builder.DecrementIndent();
			builder.Append("END");
			builder.EndCommand();
		}

		protected virtual string CreateTriggerName(string columnName, string tableName)
		{
			return string.Format("ID_{0}_{1}", tableName, columnName);
		}

		protected virtual string CreateIdentitySequenceName(string columnName, string tableName)
		{
			return "GEN_IDENTITY";
		}
	}
}
