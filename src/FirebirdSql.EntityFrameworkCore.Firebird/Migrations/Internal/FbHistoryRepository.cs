/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
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
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Migrations.Internal;

public class FbHistoryRepository : HistoryRepository
{
	public FbHistoryRepository(HistoryRepositoryDependencies dependencies)
		: base(dependencies)
	{ }

	protected override string ExistsSql
	{
		get
		{
			var escapedTableName = Dependencies.TypeMappingSource.GetMapping(typeof(string)).GenerateSqlLiteral(TableName);
			return $@"SELECT COUNT(*) FROM rdb$relations WHERE COALESCE(rdb$system_flag, 0) = 0 AND rdb$view_blr IS NULL AND rdb$relation_name = {escapedTableName}";
		}
	}

	protected override bool InterpretExistsResult(object value) => Convert.ToInt64(value) != 0;

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
