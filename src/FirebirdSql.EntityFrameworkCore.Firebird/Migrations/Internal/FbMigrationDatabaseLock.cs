﻿/*
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

//$Authors = Jiri Cincura (jiri@cincura.net)

using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Migrations.Internal;

public class FbMigrationDatabaseLock(
	IRelationalCommand releaseLockCommand,
	RelationalCommandParameterObject relationalCommandParameters,
	IHistoryRepository historyRepository,
	CancellationToken cancellationToken = default)
	: IMigrationsDatabaseLock
{
	public virtual IHistoryRepository HistoryRepository => historyRepository;

	public void Dispose()
		=> releaseLockCommand.ExecuteScalar(relationalCommandParameters);

	public async ValueTask DisposeAsync()
		=> await releaseLockCommand.ExecuteScalarAsync(relationalCommandParameters, cancellationToken).ConfigureAwait(false);
}
