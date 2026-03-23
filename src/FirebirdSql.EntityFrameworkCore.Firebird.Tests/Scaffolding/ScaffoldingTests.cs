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

	[TestCase("SMALLINT")]
	[TestCase("INTEGER")]
	[TestCase("FLOAT")]
	[TestCase("DATE")]
	[TestCase("TIME")]
	[TestCase("CHAR(12)")]
	[TestCase("BIGINT")]
	[TestCase("BOOLEAN")]
	[TestCase("DOUBLE PRECISION")]
	[TestCase("TIMESTAMP")]
	[TestCase("VARCHAR(24)")]
	[TestCase("BLOB SUB_TYPE TEXT")]
	[TestCase("BLOB SUB_TYPE BINARY")]
	[TestCase("DECIMAL(4,1)")]
	[TestCase("DECIMAL(9,1)")]
	[TestCase("DECIMAL(18,1)")]
	[TestCase("NUMERIC(4,1)")]
	[TestCase("NUMERIC(9,1)")]
	[TestCase("NUMERIC(18,1)")]
	public async Task ReadsCorrectFieldType(string dataType)
	{
		var tableName = $"TEST_READS_FIELD_TYPE_CORRECT";
		var columnName = "FIELD";

		using var commandTable = Connection.CreateCommand();
		commandTable.CommandText = $"recreate table {tableName} ({columnName} {dataType})";
		await commandTable.ExecuteNonQueryAsync();

		var modelFactory = GetModelFactory();
		var model = modelFactory.Create(Connection.ConnectionString, new DatabaseModelFactoryOptions(new string[] { tableName }));
		var table = model.Tables.Single(x => x.Name == tableName);
		var column = table.Columns.Single(x => x.Name == columnName);

		Assert.That(column.StoreType, Is.EqualTo(dataType));
	}

	[Test]
	public async Task ExpressionIndexDoesNotBreakScaffolding()
	{
		var tableName = "TEST_EXPR_IDX";

		using var commandTable = Connection.CreateCommand();
		commandTable.CommandText = $"recreate table {tableName} (ID INTEGER NOT NULL, DATA VARCHAR(100))";
		await commandTable.ExecuteNonQueryAsync();

		using var commandIndex = Connection.CreateCommand();
		commandIndex.CommandText = $"create index IDX_EXPR on {tableName} computed by (upper(DATA))";
		await commandIndex.ExecuteNonQueryAsync();

		var modelFactory = GetModelFactory();
		var model = modelFactory.Create(Connection.ConnectionString, new DatabaseModelFactoryOptions(new string[] { tableName }));
		var table = model.Tables.Single(x => x.Name == tableName);

		Assert.That(table.Indexes, Has.None.Matches<Microsoft.EntityFrameworkCore.Scaffolding.Metadata.DatabaseIndex>(x => x.Name == "IDX_EXPR"));
	}

	[Test]
	public async Task RegularIndexScaffoldedAlongsideExpressionIndex()
	{
		var tableName = "TEST_MIX_IDX";

		using var commandTable = Connection.CreateCommand();
		commandTable.CommandText = $"recreate table {tableName} (ID INTEGER NOT NULL, DATA VARCHAR(100))";
		await commandTable.ExecuteNonQueryAsync();

		using var commandRegularIndex = Connection.CreateCommand();
		commandRegularIndex.CommandText = $"create index IDX_REGULAR on {tableName} (DATA)";
		await commandRegularIndex.ExecuteNonQueryAsync();

		using var commandExprIndex = Connection.CreateCommand();
		commandExprIndex.CommandText = $"create index IDX_EXPR_MIX on {tableName} computed by (upper(DATA))";
		await commandExprIndex.ExecuteNonQueryAsync();

		var modelFactory = GetModelFactory();
		var model = modelFactory.Create(Connection.ConnectionString, new DatabaseModelFactoryOptions(new string[] { tableName }));
		var table = model.Tables.Single(x => x.Name == tableName);

		Assert.Multiple(() =>
		{
			Assert.That(table.Indexes, Has.Some.Matches<Microsoft.EntityFrameworkCore.Scaffolding.Metadata.DatabaseIndex>(x => x.Name == "IDX_REGULAR"));
			Assert.That(table.Indexes, Has.None.Matches<Microsoft.EntityFrameworkCore.Scaffolding.Metadata.DatabaseIndex>(x => x.Name == "IDX_EXPR_MIX"));
		});
	}

	static IDatabaseModelFactory GetModelFactory()
	{
		return new FbDatabaseModelFactory();
	}
}
