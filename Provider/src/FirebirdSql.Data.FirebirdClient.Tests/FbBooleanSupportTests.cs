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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Default))]
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Embedded))]
	public class FbBooleanSupportTests : FbTestsBase
	{
		private bool _shouldTearDown;

		public FbBooleanSupportTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
			: base(serverType, compression, wireCrypt)
		{
			_shouldTearDown = false;
		}

		[SetUp]
		public override async Task SetUp()
		{
			await base.SetUp();

			if (!EnsureServerVersion(new Version(3, 0, 0, 0)))
				return;

			_shouldTearDown = true;
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "CREATE TABLE withboolean (id INTEGER, bool BOOLEAN)";
				await cmd.ExecuteNonQueryAsync();
			}
			var data = new Dictionary<int, string>()
			{
				{ 0, "FALSE" },
				{ 1, "TRUE" },
				{ 2, "UNKNOWN" },
			};
			foreach (var item in data)
			{
				await using (var cmd = Connection.CreateCommand())
				{
					cmd.CommandText = $"INSERT INTO withboolean (id, bool) VALUES ({item.Key}, {item.Value})";
					await cmd.ExecuteNonQueryAsync();
				}
			}
		}

		[TearDown]
		public override async Task TearDown()
		{
			if (_shouldTearDown)
			{
				await using (var cmd = Connection.CreateCommand())
				{
					cmd.CommandText = "DROP TABLE withboolean";
					await cmd.ExecuteNonQueryAsync();
				}
			}
			await base.TearDown();
		}

		[Test]
		public async Task SimpleSelectTest()
		{
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "SELECT id, bool FROM withboolean";
				await using (var reader = await cmd.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						switch (reader.GetInt32(0))
						{
							case 0:
								Assert.IsFalse(reader.GetBoolean(1), "Column with value FALSE should have value false.");
								Assert.IsFalse(await reader.IsDBNullAsync(1), "Column with value FALSE should not be null.");
								break;
							case 1:
								Assert.IsTrue(reader.GetBoolean(1), "Column with value TRUE should have value true.");
								Assert.IsFalse(await reader.IsDBNullAsync(1), "Column with value TRUE should not be null.");
								break;
							case 2:
								Assert.IsTrue(await reader.IsDBNullAsync(1), "Column with value UNKNOWN should be null.");
								break;
							default:
								Assert.Fail("Unexpected row in result set.");
								break;
						}
					}
				}
			}
		}

		[Test]
		public async Task SimpleSelectSchemaTableTest()
		{
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "SELECT id, bool FROM withboolean";
				await using (var reader = await cmd.ExecuteReaderAsync())
				{
					var schema = await reader.GetSchemaTableAsync();
					Assert.AreEqual(typeof(bool), schema.Rows[1].ItemArray[5]);
				}
			}
		}

		[TestCase(false, 0)]
		[TestCase(true, 1)]
		[TestCase(null, 2)]
		public async Task SimpleSelectWithBoolConditionTest(bool? value, int id)
		{
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = $"SELECT id FROM withboolean WHERE bool IS NOT DISTINCT FROM @bool";
				cmd.Parameters.Add(new FbParameter("bool", value));
				Assert.AreEqual(id, await cmd.ExecuteScalarAsync());
			}
		}

		[TestCase(3, false)]
		[TestCase(4, true)]
		[TestCase(5, null)]
		public async Task ParametrizedInsertTest(int id, bool? value)
		{
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "INSERT INTO withboolean (id, bool) VALUES (@id, @bool)";
				cmd.Parameters.Add("id", id);
				cmd.Parameters.Add("bool", value);
				Assert.DoesNotThrowAsync(() => cmd.ExecuteNonQueryAsync());
			}
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = $"SELECT bool FROM withboolean WHERE id = @id";
				cmd.Parameters.Add("id", id);
				Assert.AreEqual(value ?? (object)DBNull.Value, await cmd.ExecuteScalarAsync());
			}
		}
	}
}
