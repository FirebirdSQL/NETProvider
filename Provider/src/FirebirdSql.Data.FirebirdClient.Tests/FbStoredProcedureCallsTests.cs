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
using System.Data;
using System.Threading.Tasks;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Default))]
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Embedded))]
	public class FbStoredProcedureCallsTests : FbTestsBase
	{
		public FbStoredProcedureCallsTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
			: base(serverType, compression, wireCrypt)
		{ }

		[Test]
		public async Task FirebirdLikeTest00()
		{
			await using (var command = new FbCommand("EXECUTE PROCEDURE GETVARCHARFIELD(?)", Connection))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.Add("@ID", FbDbType.VarChar).Direction = ParameterDirection.Input;
				command.Parameters.Add("@VARCHAR_FIELD", FbDbType.VarChar).Direction = ParameterDirection.Output;
				command.Parameters[0].Value = 1;
				await command.ExecuteNonQueryAsync();
				var value = command.Parameters[1].Value;
				Assert.AreEqual("IRow Number 1", value);
			}
		}

		[Test]
		public async Task FirebirdLikeTest01()
		{
			await using (var command = new FbCommand("SELECT * FROM GETVARCHARFIELD(?)", Connection))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.Add("@ID", FbDbType.VarChar).Direction = ParameterDirection.Input;
				command.Parameters[0].Value = 1;
				await using (var reader = command.ExecuteReader())
				{
					await reader.ReadAsync();
					var value = reader[0];
					Assert.AreEqual("IRow Number 1", value);
				}
			}

		}

		[Test]
		public async Task SqlServerLikeTest00()
		{
			await using (var command = new FbCommand("GETVARCHARFIELD", Connection))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.Add("@ID", FbDbType.VarChar).Direction = ParameterDirection.Input;
				command.Parameters.Add("@VARCHAR_FIELD", FbDbType.VarChar).Direction = ParameterDirection.Output;
				command.Parameters[0].Value = 1;
				await command.ExecuteNonQueryAsync();
				var value = command.Parameters[1].Value;
				Assert.AreEqual("IRow Number 1", value);
			}
		}

		[Test]
		public async Task SqlServerLikeTest01()
		{
			await using (var command = new FbCommand("GETRECORDCOUNT", Connection))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.Add("@RECORDCOUNT", FbDbType.Integer).Direction = ParameterDirection.Output;
				await command.ExecuteNonQueryAsync();
				var value = command.Parameters[0].Value;
				Assert.Greater(Convert.ToInt32(value), 0);
			}
		}

		[Test]
		public async Task SqlServerLikeTest02()
		{
			await using (var command = new FbCommand("GETVARCHARFIELD", Connection))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.Add("@ID", FbDbType.VarChar).Value = 1;
				await using (var r = await command.ExecuteReaderAsync())
				{
					var count = 0;
					while (await r.ReadAsync())
					{
						count++;
					}
					Assert.AreEqual(1, count);
				}
			}
		}
	}
}
