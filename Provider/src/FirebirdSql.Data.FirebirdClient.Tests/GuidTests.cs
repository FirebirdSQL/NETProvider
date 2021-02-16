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
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Default))]
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Embedded))]
	public class GuidTests : FbTestsBase
	{
		public GuidTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
			: base(serverType, compression, wireCrypt)
		{ }

		[Test]
		public async Task InsertGuidTest()
		{
			var newGuid = Guid.Empty;
			var guidValue = Guid.NewGuid();

			await using (var insert = new FbCommand("INSERT INTO GUID_TEST (GUID_FIELD) VALUES (@GuidValue)", Connection))
			{
				insert.Parameters.Add("@GuidValue", FbDbType.Guid).Value = guidValue;
				await insert.ExecuteNonQueryAsync();
			}

			await using (var select = new FbCommand("SELECT * FROM GUID_TEST", Connection))
			{
				await using (var r = await select.ExecuteReaderAsync())
				{
					if (await r.ReadAsync())
					{
						newGuid = r.GetGuid(1);
					}
				}
			}

			Assert.AreEqual(guidValue, newGuid);
		}

		[Test]
		public async Task InsertNullGuidTest()
		{
			var id = GetId();

			await using (var insert = new FbCommand("INSERT INTO GUID_TEST (INT_FIELD, GUID_FIELD) VALUES (@IntField, @GuidValue)", Connection))
			{
				insert.Parameters.Add("@IntField", FbDbType.Integer).Value = id;
				insert.Parameters.Add("@GuidValue", FbDbType.Guid).Value = DBNull.Value;
				await insert.ExecuteNonQueryAsync();
			}

			await using (var select = new FbCommand("SELECT * FROM GUID_TEST WHERE INT_FIELD = @IntField", Connection))
			{
				select.Parameters.Add("@IntField", FbDbType.Integer).Value = id;
				await using (var r = await select.ExecuteReaderAsync())
				{
					if (await r.ReadAsync())
					{
						if (!await r.IsDBNullAsync(1))
						{
							Assert.Fail();
						}
					}
				}
			}
		}
	}
}
