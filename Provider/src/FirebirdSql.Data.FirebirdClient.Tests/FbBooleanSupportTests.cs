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
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[FbTestFixture(FbServerType.Default, false)]
	[FbTestFixture(FbServerType.Default, true)]
	[FbTestFixture(FbServerType.Embedded, default)]
	public class FbBooleanSupportTests : FbTestsBase
	{
		private bool _shouldTearDown;

		public FbBooleanSupportTests(FbServerType serverType, bool compression)
			: base(serverType, compression)
		{
			_shouldTearDown = false;
		}

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();

			if (!EnsureVersion(new Version("3.0.0.0")))
				return;

			_shouldTearDown = true;
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "CREATE TABLE withboolean (id INTEGER, bool BOOLEAN)";
				cmd.ExecuteNonQuery();
			}
			var data = new Dictionary<int, string>()
			{
				{ 0, "FALSE" },
				{ 1, "TRUE" },
				{ 2, "UNKNOWN" },
			};
			foreach (var item in data)
			{
				using (var cmd = Connection.CreateCommand())
				{
					cmd.CommandText = $"INSERT INTO withboolean (id, bool) VALUES ({item.Key}, {item.Value})";
					cmd.ExecuteNonQuery();
				}
			}
		}

		[TearDown]
		public override void TearDown()
		{
			if (_shouldTearDown)
			{
				using (var cmd = Connection.CreateCommand())
				{
					cmd.CommandText = "DROP TABLE withboolean";
					cmd.ExecuteNonQuery();
				}
			}
			base.TearDown();
		}

		[Test]
		public void SimpleSelectTest()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "SELECT id, bool FROM withboolean";
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						switch (reader.GetInt32(0))
						{
							case 0:
								Assert.IsFalse(reader.GetBoolean(1), "Column with value FALSE should have value false.");
								Assert.IsFalse(reader.IsDBNull(1), "Column with value FALSE should not be null.");
								break;
							case 1:
								Assert.IsTrue(reader.GetBoolean(1), "Column with value TRUE should have value true.");
								Assert.IsFalse(reader.IsDBNull(1), "Column with value TRUE should not be null.");
								break;
							case 2:
								Assert.IsTrue(reader.IsDBNull(1), "Column with value UNKNOWN should be null.");
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
		public void SimpleSelectSchemaTableTest()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "SELECT id, bool FROM withboolean";
				using (var reader = cmd.ExecuteReader())
				{
					var schema = reader.GetSchemaTable();
					Assert.AreEqual("BOOL", schema.Rows[1].ItemArray[0]);
					Assert.AreEqual(typeof(bool), schema.Rows[1].ItemArray[5]);
				}
			}
		}

		[TestCase(false, 0)]
		[TestCase(true, 1)]
		[TestCase(null, 2)]
		public void SimpleSelectWithBoolConditionTest(bool? value, int id)
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = $"SELECT id FROM withboolean WHERE bool IS NOT DISTINCT FROM @bool";
				cmd.Parameters.Add(new FbParameter("bool", value));
				Assert.AreEqual(id, cmd.ExecuteScalar());
			}
		}

		[TestCase(3, false)]
		[TestCase(4, true)]
		[TestCase(5, null)]
		public void ParametrizedInsertTest(int id, bool? value)
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "INSERT INTO withboolean (id, bool) VALUES (@id, @bool)";
				cmd.Parameters.Add("id", id);
				cmd.Parameters.Add("bool", value);
				Assert.DoesNotThrow(() => cmd.ExecuteNonQuery());
			}
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = $"SELECT bool FROM withboolean WHERE id = @id";
				cmd.Parameters.Add("id", id);
				Assert.AreEqual(value ?? (object)DBNull.Value, cmd.ExecuteScalar());
			}
		}
	}
}
