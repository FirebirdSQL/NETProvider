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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System.Security.Cryptography;
using System.Threading.Tasks;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Default))]
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Embedded))]
	public class FbBlobTests : FbTestsBase
	{
		public FbBlobTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
			: base(serverType, compression, wireCrypt)
		{ }

		[Test]
		public async Task BinaryBlobTest()
		{
			var id_value = GetId();
			var insert_values = new byte[100000 * 4];
			using (var rng = new RNGCryptoServiceProvider())
			{
				rng.GetBytes(insert_values);
			}

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var insert = new FbCommand("INSERT INTO TEST (int_field, blob_field) values(@int_field, @blob_field)", Connection, transaction))
				{
					insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
					insert.Parameters.Add("@blob_field", FbDbType.Binary).Value = insert_values;
					await insert.ExecuteNonQueryAsync();
				}
				await transaction.CommitAsync();
			}

			await using (var select = new FbCommand($"SELECT blob_field FROM TEST WHERE int_field = {id_value}", Connection))
			{
				var select_values = (byte[])await select.ExecuteScalarAsync();
				CollectionAssert.AreEqual(insert_values, select_values);
			}
		}

		[Test]
		public async Task ReaderGetBytes()
		{
			var id_value = GetId();
			var insert_values = new byte[100000 * 4];
			using (var rng = new RNGCryptoServiceProvider())
			{
				rng.GetBytes(insert_values);
			}

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var insert = new FbCommand("INSERT INTO TEST (int_field, blob_field) values(@int_field, @blob_field)", Connection, transaction))
				{
					insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
					insert.Parameters.Add("@blob_field", FbDbType.Binary).Value = insert_values;
					await insert.ExecuteNonQueryAsync();
				}
				await transaction.CommitAsync();
			}

			await using (var select = new FbCommand($"SELECT blob_field FROM TEST WHERE int_field = {id_value}", Connection))
			{
				var select_values = new byte[100000 * 4];
				using (var reader = await select.ExecuteReaderAsync())
				{
					var index = 0;
					var segmentSize = 1000;
					while (await reader.ReadAsync())
					{
						while (index < 400000)
						{
							reader.GetBytes(0, index, select_values, index, segmentSize);

							index += segmentSize;
						}
					}
				}
				CollectionAssert.AreEqual(insert_values, select_values);
			}
		}
	}
}
