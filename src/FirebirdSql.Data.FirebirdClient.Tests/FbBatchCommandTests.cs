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

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Default))]
	public class FbBatchCommandTests : FbTestsBase
	{
		bool _shouldTearDown;

		public FbBatchCommandTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
			: base(serverType, compression, wireCrypt, false)
		{
			_shouldTearDown = false;
		}

		[SetUp]
		public override async Task SetUp()
		{
			await base.SetUp();

			if (!EnsureServerVersion(new Version(4, 0, 0, 0)))
				return;

			_shouldTearDown = true;
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "create table batch (i int check (i < 1000), i2 int128, ts timestamp)";
				await cmd.ExecuteNonQueryAsync();
			}
		}

		[TearDown]
		public override async Task TearDown()
		{
			if (_shouldTearDown)
			{
				await using (var cmd = Connection.CreateCommand())
				{
					cmd.CommandText = "drop table batch";
					await cmd.ExecuteNonQueryAsync();
				}
			}
			await base.TearDown();
		}

		[Test]
		public async Task DataProperlyInDatabase()
		{
			await Empty();

			var @is = new[] { -1, 6 };
			var bs = new[] { new BigInteger(long.MaxValue) * 2, new BigInteger(long.MaxValue) * 3 };
			var ts = new[] { new DateTime(2022, 01, 17, 1, 0, 0), new DateTime(2022, 01, 17, 2, 0, 0) };

			await using (var cmd = Connection.CreateBatchCommand())
			{
				cmd.CommandText = "insert into batch values (@i, @i2, @ts)";
				var batch1 = cmd.AddBatchParameters();
				batch1.Add("i", @is[0]);
				batch1.Add("i2", bs[0]);
				batch1.Add("ts", ts[0]);
				var batch2 = cmd.AddBatchParameters();
				batch2.Add("i", @is[1]);
				batch2.Add("i2", bs[1]);
				batch2.Add("ts", ts[1]);
				var result = await cmd.ExecuteNonQueryAsync();
			}
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select i, i2, ts from batch order by i";
				using (var reader = await cmd.ExecuteReaderAsync())
				{
					var index = 0;
					while (await reader.ReadAsync())
					{
						Assert.AreEqual(@is[index], reader[0]);
						Assert.AreEqual(bs[index], reader[1]);
						Assert.AreEqual(ts[index], reader[2]);
						index += 1;
					}
				}
			}
		}

		[Test]
		public async Task SuccessWithRecordsAffected()
		{
			await Empty();

			await using (var cmd = Connection.CreateBatchCommand())
			{
				cmd.CommandText = "insert into batch (i) values (@i)";
				var batch1 = cmd.AddBatchParameters();
				batch1.Add("i", 1);
				var batch2 = cmd.AddBatchParameters();
				batch2.Add("i", 2);
				var result = await cmd.ExecuteNonQueryAsync();

				Assert.AreEqual(2, result.Count);
				Assert.IsTrue(result.AllSuccess);
				Assert.IsTrue(result[0].IsSuccess);
				Assert.IsNull(result[0].Exception);
				Assert.AreEqual(1, result[0].RecordsAffected);
				Assert.IsTrue(result[1].IsSuccess);
				Assert.IsNull(result[1].Exception);
				Assert.AreEqual(1, result[1].RecordsAffected);
			}
		}

		[Test]
		public async Task SuccessWithoutRecordsAffected()
		{
			await Empty();

			var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
			csb.ReturnRecordsAffected = false;

			await using (var conn = new FbConnection(csb.ToString()))
			{
				await conn.OpenAsync();
				await using (var cmd = conn.CreateBatchCommand())
				{
					cmd.CommandText = "insert into batch (i) values (@i)";
					var batch1 = cmd.AddBatchParameters();
					batch1.Add("i", 1);
					var batch2 = cmd.AddBatchParameters();
					batch2.Add("i", 2);
					var result = await cmd.ExecuteNonQueryAsync();

					Assert.AreEqual(2, result.Count);
					Assert.IsTrue(result.AllSuccess);
					Assert.IsTrue(result[0].IsSuccess);
					Assert.IsNull(result[0].Exception);
					Assert.AreEqual(-1, result[0].RecordsAffected);
					Assert.IsTrue(result[1].IsSuccess);
					Assert.IsNull(result[1].Exception);
					Assert.AreEqual(-1, result[1].RecordsAffected);
				}
			}
		}

		[Test]
		public async Task ErrorWithoutMultiError()
		{
			await Empty();

			await using (var cmd = Connection.CreateBatchCommand())
			{
				cmd.MultiError = false;

				cmd.CommandText = "insert into batch (i) values (@i)";
				var batch1 = cmd.AddBatchParameters();
				batch1.Add("i", 1);
				var batch2 = cmd.AddBatchParameters();
				batch2.Add("i", 1200);
				var batch3 = cmd.AddBatchParameters();
				batch3.Add("i", 1300);
				var result = await cmd.ExecuteNonQueryAsync();

				Assert.AreEqual(2, result.Count);
				Assert.IsFalse(result.AllSuccess);
				Assert.IsTrue(result[0].IsSuccess);
				Assert.IsNull(result[0].Exception);
				Assert.AreEqual(1, result[0].RecordsAffected);
				Assert.IsFalse(result[1].IsSuccess);
				Assert.IsInstanceOf<FbException>(result[1].Exception);
				Assert.AreEqual(-1, result[1].RecordsAffected);
			}
		}

		[Test]
		public async Task ErrorWithMultiError()
		{
			await Empty();

			await using (var cmd = Connection.CreateBatchCommand())
			{
				cmd.MultiError = true;

				cmd.CommandText = "insert into batch (i) values (@i)";
				var batch1 = cmd.AddBatchParameters();
				batch1.Add("i", 1);
				var batch2 = cmd.AddBatchParameters();
				batch2.Add("i", 1200);
				var batch3 = cmd.AddBatchParameters();
				batch3.Add("i", 1300);
				var result = await cmd.ExecuteNonQueryAsync();

				Assert.AreEqual(3, result.Count);
				Assert.IsFalse(result.AllSuccess);
				Assert.IsTrue(result[0].IsSuccess);
				Assert.IsNull(result[0].Exception);
				Assert.AreEqual(1, result[0].RecordsAffected);
				Assert.IsFalse(result[1].IsSuccess);
				Assert.IsInstanceOf<FbException>(result[1].Exception);
				Assert.AreEqual(-1, result[1].RecordsAffected);
				Assert.IsFalse(result[2].IsSuccess);
				Assert.IsInstanceOf<FbException>(result[2].Exception);
				Assert.AreEqual(-1, result[2].RecordsAffected);
			}
		}

		[Test]
		[Ignore("Server bug (#7099).")]
		public async Task ErrorWithMultiErrorWithOverflow()
		{
			await Empty();

			await using (var cmd = Connection.CreateBatchCommand())
			{
				cmd.MultiError = true;

				cmd.CommandText = "insert into batch (i) values (@i)";
				var batch1 = cmd.AddBatchParameters();
				batch1.Add("i", 1);
				for (var i = 0; i < 100; i++)
				{
					var b = cmd.AddBatchParameters();
					b.Add("i", 1200);
				}
				var result = await cmd.ExecuteNonQueryAsync();

				Assert.AreEqual(3, result.Count);
				Assert.IsFalse(result.AllSuccess);
				Assert.IsTrue(result[0].IsSuccess);
				Assert.IsNull(result[0].Exception);
				Assert.AreEqual(1, result[0].RecordsAffected);
				Assert.IsFalse(result[1].IsSuccess);
				Assert.IsInstanceOf<FbException>(result[1].Exception);
				Assert.AreEqual(-1, result[1].RecordsAffected);
				Assert.IsFalse(result[2].IsSuccess);
				Assert.IsInstanceOf<FbException>(result[2].Exception);
				Assert.AreEqual(-1, result[2].RecordsAffected);
			}
		}

		async Task Empty()
		{
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "delete from batch";
				await cmd.ExecuteNonQueryAsync();
			}
		}
	}
}