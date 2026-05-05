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

//$Authors = Ebubekir Cagri Sen (ebubekircagrisen@gmail.com)

using System;
using System.Threading.Tasks;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests;

[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Default))]
[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Embedded))]
public class DomainTypeMappingTests : FbTestsBase
{
	public DomainTypeMappingTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
		: base(serverType, compression, wireCrypt, insertTestData: false)
	{ }

	private FbConnectionStringBuilder BuildBuilderWithDomainMappings(string booleanDomains, string guidDomains)
	{
		var builder = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
		if (booleanDomains != null)
			builder.BooleanDomains = booleanDomains;
		if (guidDomains != null)
			builder.GuidDomains = guidDomains;
		return builder;
	}

	private async Task ResetTableAsync()
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "DELETE FROM USER_TYPE_TEST";
			await cmd.ExecuteNonQueryAsync();
		}
	}

	[OneTimeTearDown]
	public async Task TearDownAsync()
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "DROP TABLE USER_TYPE_TEST";
			await cmd.ExecuteNonQueryAsync();
		}
	}


	private async Task SeedRowAsync(int id, short isActive, short? optFlag, Guid rowGuid, Guid? optGuid)
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "INSERT INTO USER_TYPE_TEST (ID, IS_ACTIVE, OPT_FLAG, ROW_GUID, OPT_GUID) VALUES (@id, @act, @opt, @g, @og)";
			cmd.Parameters.Add("@id", id);
			cmd.Parameters.Add("@act", isActive);
			cmd.Parameters.Add("@opt", (object)optFlag ?? DBNull.Value);
			cmd.Parameters.Add("@g", rowGuid);
			cmd.Parameters.Add("@og", optGuid.HasValue ? (object)optGuid.Value : DBNull.Value);
			await cmd.ExecuteNonQueryAsync();
		}
	}

	[Test]
	public async Task Read_BooleanMapping_ReportsBoolType()
	{
		await ResetTableAsync();
		await SeedRowAsync(1, 1, 0, Guid.NewGuid(), null);

		var cs = BuildBuilderWithDomainMappings("D_BOOL%", null).ToString();
		await using (var conn = new FbConnection(cs))
		{
			await conn.OpenAsync();
			await using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = "SELECT IS_ACTIVE, OPT_FLAG FROM USER_TYPE_TEST WHERE ID = 1";
				await using (var reader = await cmd.ExecuteReaderAsync())
				{
					Assert.IsTrue(await reader.ReadAsync());
					Assert.AreEqual(typeof(bool), reader.GetFieldType(0));
					Assert.AreEqual(typeof(bool), reader.GetFieldType(1));
					Assert.AreEqual(true, reader.GetBoolean(0));
					Assert.AreEqual(false, reader.GetBoolean(1));
				}
			}
		}
	}

	[Test]
	public async Task Read_NoMapping_ReportsShortType()
	{
		await ResetTableAsync();
		await SeedRowAsync(2, 1, null, Guid.NewGuid(), null);

		var cs = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt).ToString();
		await using (var conn = new FbConnection(cs))
		{
			await conn.OpenAsync();
			await using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = "SELECT IS_ACTIVE FROM USER_TYPE_TEST WHERE ID = 2";
				await using (var reader = await cmd.ExecuteReaderAsync())
				{
					Assert.IsTrue(await reader.ReadAsync());
					Assert.AreEqual(typeof(short), reader.GetFieldType(0));
				}
			}
		}
	}

	[Test]
	public async Task Write_BoolToSmallintRoundtrip()
	{
		await ResetTableAsync();

		var cs = BuildBuilderWithDomainMappings("D_BOOL%", null).ToString();
		await using (var conn = new FbConnection(cs))
		{
			await conn.OpenAsync();
			await using (var insert = conn.CreateCommand())
			{
				insert.CommandText = "INSERT INTO USER_TYPE_TEST (ID, IS_ACTIVE, OPT_FLAG, ROW_GUID) VALUES (@id, @act, @opt, @g)";
				insert.Parameters.Add("@id", 10);
				insert.Parameters.Add("@act", true);
				insert.Parameters.Add("@opt", false);
				insert.Parameters.Add("@g", Guid.NewGuid());
				var affected = await insert.ExecuteNonQueryAsync();
				Assert.AreEqual(1, affected);
			}
			await using (var read = conn.CreateCommand())
			{
				read.CommandText = "SELECT IS_ACTIVE, OPT_FLAG FROM USER_TYPE_TEST WHERE ID = 10";
				await using (var reader = await read.ExecuteReaderAsync())
				{
					Assert.IsTrue(await reader.ReadAsync());
					Assert.AreEqual(true, reader.GetBoolean(0));
					Assert.AreEqual(false, reader.GetBoolean(1));
				}
			}
		}
	}

	[Test]
	public async Task Write_RawNumeric_AlsoStillWorks()
	{
		await ResetTableAsync();

		var cs = BuildBuilderWithDomainMappings("D_BOOL%", null).ToString();
		await using (var conn = new FbConnection(cs))
		{
			await conn.OpenAsync();
			await using (var insert = conn.CreateCommand())
			{
				insert.CommandText = "INSERT INTO USER_TYPE_TEST (ID, IS_ACTIVE, ROW_GUID) VALUES (@id, @act, @g)";
				insert.Parameters.Add("@id", 11);
				insert.Parameters.Add("@act", (short)1);
				insert.Parameters.Add("@g", Guid.NewGuid());
				await insert.ExecuteNonQueryAsync();
			}
			await using (var read = conn.CreateCommand())
			{
				read.CommandText = "SELECT IS_ACTIVE FROM USER_TYPE_TEST WHERE ID = 11";
				await using (var reader = await read.ExecuteReaderAsync())
				{
					Assert.IsTrue(await reader.ReadAsync());
					Assert.AreEqual(true, reader.GetBoolean(0));
				}
			}
		}
	}

	[Test]
	public async Task Read_GuidMapping_ReportsGuidType()
	{
		await ResetTableAsync();
		var g = Guid.NewGuid();
		await SeedRowAsync(20, 1, null, g, null);

		var cs = BuildBuilderWithDomainMappings(null, "D_GUID%").ToString();
		await using (var conn = new FbConnection(cs))
		{
			await conn.OpenAsync();
			await using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = "SELECT ROW_GUID FROM USER_TYPE_TEST WHERE ID = 20";
				await using (var reader = await cmd.ExecuteReaderAsync())
				{
					Assert.IsTrue(await reader.ReadAsync());
					Assert.AreEqual(typeof(Guid), reader.GetFieldType(0));
					Assert.AreEqual(g, reader.GetGuid(0));
				}
			}
		}
	}

	[Test]
	public async Task PatternList_BothFamiliesMatched()
	{
		await ResetTableAsync();
		await SeedRowAsync(40, 1, 0, Guid.NewGuid(), null);

		var cs = BuildBuilderWithDomainMappings("D_BOOL%,BOOL\\_%", null).ToString();
		await using (var conn = new FbConnection(cs))
		{
			await conn.OpenAsync();
			await using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = "SELECT IS_ACTIVE, OPT_FLAG FROM USER_TYPE_TEST WHERE ID = 40";
				await using (var reader = await cmd.ExecuteReaderAsync())
				{
					Assert.IsTrue(await reader.ReadAsync());
					Assert.AreEqual(typeof(bool), reader.GetFieldType(0));
					Assert.AreEqual(typeof(bool), reader.GetFieldType(1));
				}
			}
		}
	}

	[Test]
	public async Task NonMatchingPattern_NoOverride()
	{
		await ResetTableAsync();
		await SeedRowAsync(50, 1, null, Guid.NewGuid(), null);

		var cs = BuildBuilderWithDomainMappings("ZZZ\\_NEVER\\_%", null).ToString();
		await using (var conn = new FbConnection(cs))
		{
			await conn.OpenAsync();
			await using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = "SELECT IS_ACTIVE FROM USER_TYPE_TEST WHERE ID = 50";
				await using (var reader = await cmd.ExecuteReaderAsync())
				{
					Assert.IsTrue(await reader.ReadAsync());
					Assert.AreEqual(typeof(short), reader.GetFieldType(0));
				}
			}
		}
	}

	[Test]
	public async Task NullValue_BoolMapping_ReturnsDbNull()
	{
		await ResetTableAsync();
		await SeedRowAsync(60, 1, null, Guid.NewGuid(), null);

		var cs = BuildBuilderWithDomainMappings("D_BOOL%", null).ToString();
		await using (var conn = new FbConnection(cs))
		{
			await conn.OpenAsync();
			await using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = "SELECT OPT_FLAG FROM USER_TYPE_TEST WHERE ID = 60";
				await using (var reader = await cmd.ExecuteReaderAsync())
				{
					Assert.IsTrue(await reader.ReadAsync());
					Assert.IsTrue(reader.IsDBNull(0));
					Assert.AreEqual(typeof(bool), reader.GetFieldType(0));
				}
			}
		}
	}

	[Test]
	public async Task DataAdapter_BoolColumnRoundtrip()
	{
		await ResetTableAsync();
		// Seed a row so FbCommandBuilder can infer the PK from the filled DataTable.
		await SeedRowAsync(70, 1, null, Guid.NewGuid(), null);

		var cs = BuildBuilderWithDomainMappings("D_BOOL%", "D_GUID%").ToString();
		await using (var conn = new FbConnection(cs))
		{
			await conn.OpenAsync();
			using (var adapter = new FbDataAdapter(new FbCommand("SELECT ID, IS_ACTIVE, ROW_GUID FROM USER_TYPE_TEST WHERE ID = 70", conn)))
			using (var builder = new FbCommandBuilder(adapter))
			{
				var table = new System.Data.DataTable();
				adapter.Fill(table);

				// Verify the seeded row came back with domain-mapped bool type.
				Assert.AreEqual(1, table.Rows.Count);
				Assert.AreEqual(typeof(bool), table.Columns["IS_ACTIVE"].DataType);
				Assert.AreEqual(typeof(Guid), table.Columns["ROW_GUID"].DataType);

				// Update the row.
				table.Rows[0]["IS_ACTIVE"] = false;
				adapter.Update(table);

				table.Clear();
				adapter.Fill(table);
				Assert.AreEqual(1, table.Rows.Count);
				Assert.AreEqual(false, table.Rows[0]["IS_ACTIVE"]);
			}
		}
	}

	[Test]
	public async Task Resolver_CacheHit_OnlyFetchesOncePerSchema()
	{
		await ResetTableAsync();
		await SeedRowAsync(80, 1, null, Guid.NewGuid(), null);

		var cs = BuildBuilderWithDomainMappings("D_BOOL%", "D_GUID%").ToString();
		await using (var conn = new FbConnection(cs))
		{
			await conn.OpenAsync();
			var resolver = conn.InnerConnection.DomainResolver;
			Assert.AreEqual(0, resolver.FetchCount);

			for (var i = 0; i < 5; i++)
			{
				await using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = "SELECT IS_ACTIVE, ROW_GUID FROM USER_TYPE_TEST WHERE ID = 80";
					await using (var reader = await cmd.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync()) { }
					}
				}
			}

			// Two columns from one table -> at most one fetch round-trip.
			Assert.AreEqual(1, resolver.FetchCount);
		}
	}

	[Test]
	public async Task Resolver_RepeatedPrepare_NoStackOverflow()
	{
		await ResetTableAsync();
		await SeedRowAsync(90, 1, null, Guid.NewGuid(), null);

		var cs = BuildBuilderWithDomainMappings("D_BOOL%", "D_GUID%").ToString();
		await using (var conn = new FbConnection(cs))
		{
			await conn.OpenAsync();
			for (var i = 0; i < 100; i++)
			{
				await using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = $"SELECT IS_ACTIVE FROM USER_TYPE_TEST WHERE ID = {90 + (i % 5)}";
					await using (var reader = await cmd.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync()) { }
					}
				}
			}
			Assert.Pass();
		}
	}

	[Test]
	public async Task GetSchemaTable_BoolMapping_ReportsCorrectDataType()
	{
		await ResetTableAsync();
		await SeedRowAsync(100, 1, 0, Guid.NewGuid(), null);

		var cs = BuildBuilderWithDomainMappings("D_BOOL%", "D_GUID%").ToString();
		await using (var conn = new FbConnection(cs))
		{
			await conn.OpenAsync();
			await using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = "SELECT IS_ACTIVE, OPT_FLAG, ROW_GUID FROM USER_TYPE_TEST WHERE ID = 100";
				await using (var reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.KeyInfo))
				{
					var schema = reader.GetSchemaTable();
					Assert.IsNotNull(schema);

					// IS_ACTIVE: D_BOOL domain -> should report Boolean
					var isActiveType = (Type)schema.Rows[0]["DataType"];
					Assert.AreEqual(typeof(bool), isActiveType, "IS_ACTIVE should be reported as Boolean via domain mapping");

					// OPT_FLAG: D_BOOL_NULLABLE domain -> should also report Boolean
					var optFlagType = (Type)schema.Rows[1]["DataType"];
					Assert.AreEqual(typeof(bool), optFlagType, "OPT_FLAG should be reported as Boolean via domain mapping");

					// ROW_GUID: D_GUID domain -> should report Guid
					var rowGuidType = (Type)schema.Rows[2]["DataType"];
					Assert.AreEqual(typeof(Guid), rowGuidType, "ROW_GUID should be reported as Guid via domain mapping");
				}
			}
		}
	}
}
