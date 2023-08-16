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

//$Authors = Jiri Cincura (jiri@cincura.net)

using System.Linq;
using System.Threading.Tasks;
using FirebirdSql.EntityFrameworkCore.Firebird.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding;
using NUnit.Framework;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Tests.Scaffolding;
#pragma warning disable EF1001
public class ScaffoldingTests : EntityFrameworkCoreTestsBase
{
	[Test]
	public void JustCanRun()
	{
		var modelFactory = GetModelFactory();
		Assert.DoesNotThrow(() => modelFactory.Create(Connection, new DatabaseModelFactoryOptions()));
	}

	[Test]
	public async Task ReadsNullableCorrect()
	{
		var tableName = "TEST_READS_IS_NULL_FROM_DOMAIN";
		var columnNameNoDomainNull = "NO_DOMAIN_NULL";
		var columnNameNoDomainNotNull = "NO_DOMAIN_NOT_NULL";
		var columnNameDomainNull = "DOMAIN_NULL";
		var columnNameDomainNotNull = "DOMAIN_NOT_NUL";

		using var commandDomainNull = Connection.CreateCommand();
		commandDomainNull.CommandText = "create domain DOMAIN_NULL as INTEGER";
		await commandDomainNull.ExecuteNonQueryAsync();

		using var commandDomainNotNull = Connection.CreateCommand();
		commandDomainNotNull.CommandText = "create domain DOMAIN_NOT_NULL as INTEGER NOT NULL";
		await commandDomainNotNull.ExecuteNonQueryAsync();

		using var commandTable = Connection.CreateCommand();
		commandTable.CommandText = $"create table {tableName} ({columnNameNoDomainNull} INTEGER, {columnNameNoDomainNotNull} INTEGER NOT NULL, {columnNameDomainNull} DOMAIN_NULL, {columnNameDomainNotNull} DOMAIN_NOT_NULL)";
		await commandTable.ExecuteNonQueryAsync();

		var modelFactory = GetModelFactory();
		var model = modelFactory.Create(Connection.ConnectionString, new DatabaseModelFactoryOptions(new string[] { tableName }));
		var table = model.Tables.Single(x => x.Name == tableName);
		var columnNoDomainNull = table.Columns.Single(x => x.Name == columnNameNoDomainNull);
		var columnNoDomainNotNull = table.Columns.Single(x => x.Name == columnNameNoDomainNotNull);
		var columnDomainNull = table.Columns.Single(x => x.Name == columnNameDomainNull);
		var columnDomainNotNull = table.Columns.Single(x => x.Name == columnNameDomainNotNull);

		Assert.Multiple(() =>
		{
			Assert.That(columnNoDomainNull.IsNullable, Is.True);
			Assert.That(columnNoDomainNotNull.IsNullable, Is.False);
			Assert.That(columnDomainNull.IsNullable, Is.True);
			Assert.That(columnDomainNotNull.IsNullable, Is.False);
		});

	}

	static IDatabaseModelFactory GetModelFactory()
	{
		return new FbDatabaseModelFactory();
	}
}
