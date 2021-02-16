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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Threading.Tasks;
using System.Transactions;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Default))]
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Embedded))]
	public class TransactionScopeTests : FbTestsBase
	{
		public TransactionScopeTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
			: base(serverType, compression, wireCrypt)
		{ }

		[Test]
		public async Task SimpleSelectTest()
		{
			var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);

			csb.Enlist = true;

			using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
			{
				await using (var c = new FbConnection(csb.ToString()))
				{
					c.Open();

					await using (var command = new FbCommand("select * from TEST where (0=1)", c))
					{
						await using (var r = await command.ExecuteReaderAsync())
						{
							while (await r.ReadAsync())
							{
							}
						}
					}
				}

				scope.Complete();
			}
		}

		[Test]
		public async Task InsertTest()
		{
			var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);

			csb.Enlist = true;

			using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
			{
				await using (var c = new FbConnection(csb.ToString()))
				{
					await c.OpenAsync();

					var sql = "insert into TEST (int_field, date_field) values (1002, @date)";

					await using (var command = new FbCommand(sql, c))
					{
						command.Parameters.Add("@date", FbDbType.Date).Value = DateTime.Now.ToString();

						var ra = await command.ExecuteNonQueryAsync();

						Assert.AreEqual(ra, 1);
					}
				}

				scope.Complete();
			}
		}
	}
}

