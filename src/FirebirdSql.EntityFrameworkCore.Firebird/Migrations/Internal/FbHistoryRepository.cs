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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Migrations.Internal;

public class FbHistoryRepository(HistoryRepositoryDependencies dependencies) : HistoryRepository(dependencies)
{
	static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(1);

	protected override string ExistsSql
	{
		get
		{
			var escapedTableName = Dependencies.TypeMappingSource.GetMapping(typeof(string)).GenerateSqlLiteral(TableName);
			return $"""
				SELECT COUNT(*) FROM rdb$relations WHERE COALESCE(rdb$system_flag, 0) = 0 AND rdb$view_blr IS NULL AND rdb$relation_name = {escapedTableName}
				""";
		}
	}

	protected override bool InterpretExistsResult(object value)
		=> Convert.ToInt64(value) != 0;

	public override string GetCreateIfNotExistsScript()
	{
		var script = GetCreateScript();
		return BuildIfTableNotExistsBlock(script).CommandText;
	}

	protected virtual string LockTableName { get; } = "__EFMigrationsLock";

	public override LockReleaseBehavior LockReleaseBehavior
		=> LockReleaseBehavior.Explicit;

	public override IMigrationsDatabaseLock AcquireDatabaseLock()
	{
		Dependencies.MigrationsLogger.AcquiringMigrationLock();

		CreateLockTableCommand().ExecuteNonQuery(CreateRelationalCommandParameters());

		var retryDelay = RetryDelay;
		while (true)
		{
			var dbLock = CreateMigrationDatabaseLock();
			var insertCount = CreateInsertLockCommand(DateTime.UtcNow)
				.ExecuteScalar(CreateRelationalCommandParameters());
			if ((int)insertCount == 1)
			{
				return dbLock;
			}

			Thread.Sleep(retryDelay);
			if (retryDelay < TimeSpan.FromMinutes(1))
			{
				retryDelay = retryDelay.Add(retryDelay);
			}
		}
	}

	public override async Task<IMigrationsDatabaseLock> AcquireDatabaseLockAsync(CancellationToken cancellationToken = default)
	{
		Dependencies.MigrationsLogger.AcquiringMigrationLock();

		await CreateLockTableCommand().ExecuteNonQueryAsync(CreateRelationalCommandParameters(), cancellationToken).ConfigureAwait(false);

		var retryDelay = RetryDelay;
		while (true)
		{
			var dbLock = CreateMigrationDatabaseLock();
			var insertCount = await CreateInsertLockCommand(DateTime.UtcNow)
				.ExecuteScalarAsync(CreateRelationalCommandParameters(), cancellationToken)
				.ConfigureAwait(false);
			if ((int)insertCount == 1)
			{
				return dbLock;
			}

			await Task.Delay(RetryDelay, cancellationToken).ConfigureAwait(true);
			if (retryDelay < TimeSpan.FromMinutes(1))
			{
				retryDelay = retryDelay.Add(retryDelay);
			}
		}
	}

	public override string GetBeginIfExistsScript(string migrationId)
		=> throw new NotSupportedException("Generating idempotent scripts is currently not supported.");

	public override string GetBeginIfNotExistsScript(string migrationId)
		=> throw new NotSupportedException("Generating idempotent scripts is currently not supported.");

	public override string GetEndIfScript()
		=> throw new NotSupportedException("Generating idempotent scripts is currently not supported.");

	IRelationalCommand CreateLockTableCommand()
	{
		return BuildIfTableNotExistsBlock($"""
			CREATE TABLE "{LockTableName}" (
				"Id" INT NOT NULL CONSTRAINT "PK_{LockTableName}" PRIMARY KEY,
				"Timestamp" TIMESTAMP NOT NULL
			)
			""");
	}

	IRelationalCommand CreateInsertLockCommand(DateTime timestamp)
	{
		var timestampLiteral = Dependencies.TypeMappingSource.GetMapping(typeof(DateTime)).GenerateSqlLiteral(timestamp);
		return Dependencies.RawSqlCommandBuilder.Build($"""
			EXECUTE BLOCK
			RETURNS (ROWS_AFFECTED INT)
			AS
			BEGIN
				ROWS_AFFECTED = 1;
				BEGIN
					INSERT INTO "{LockTableName}" ("Id", "Timestamp") VALUES (1, {timestampLiteral});
					WHEN SQLSTATE '23000' DO
					BEGIN
						ROWS_AFFECTED = 0;
					END
				END
				SUSPEND;
			END
			""");
	}

	IRelationalCommand CreateDeleteLockCommand()
	{
		return Dependencies.RawSqlCommandBuilder.Build($"""
			DELETE FROM "{LockTableName}" WHERE "Id" = 1
			""");
	}

	FbMigrationDatabaseLock CreateMigrationDatabaseLock()
		=> new(CreateDeleteLockCommand(), CreateRelationalCommandParameters(), this);

	RelationalCommandParameterObject CreateRelationalCommandParameters()
		=> new(
			Dependencies.Connection,
			null,
			null,
			Dependencies.CurrentContext.Context,
			Dependencies.CommandLogger, CommandSource.Migrations);

	IRelationalCommand BuildIfTableNotExistsBlock(string statement)
	{
		var statementLiteral = Dependencies.TypeMappingSource.GetMapping(typeof(string)).GenerateSqlLiteral(statement);
		return Dependencies.RawSqlCommandBuilder.Build($"""
			EXECUTE BLOCK
			AS
			BEGIN
				BEGIN
					EXECUTE STATEMENT
						{statementLiteral};
					WHEN SQLSTATE '42S01' DO
					BEGIN
					END
				END
			END
			""");
	}
}
