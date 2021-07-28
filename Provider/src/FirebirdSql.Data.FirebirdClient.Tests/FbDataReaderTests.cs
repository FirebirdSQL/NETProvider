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
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Default))]
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Embedded))]
	public class FbDataReaderTests : FbTestsBase
	{
		public FbDataReaderTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
			: base(serverType, compression, wireCrypt)
		{ }

		[Test]
		public async Task ReadTest()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select * from TEST", Connection, transaction))
				{
					await using (var reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							for (var i = 0; i < reader.FieldCount; i++)
							{
								reader.GetValue(i);
							}
						}
					}
				}
				await transaction.RollbackAsync();
			}
		}

		[Test]
		public async Task ReadClobTest()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select * from TEST", Connection, transaction))
				{
					await using (var reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							reader.GetValue("clob_field");
						}
					}
				}
				await transaction.RollbackAsync();
			}
		}

		[Test]
		public async Task BigIntGetStringTest()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select * from TEST", Connection, transaction))
				{
					await using (var reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							reader.GetString("bigint_field");
						}
					}
				}
				await transaction.RollbackAsync();
			}
		}

		[Test]
		public async Task GetValuesTest()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select * from TEST", Connection, transaction))
				{
					await using (var reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							var values = new object[reader.FieldCount];
							reader.GetValues(values);
						}
					}
				}
				await transaction.RollbackAsync();
			}
		}

		[Test]
		public async Task IndexerByIndexTest()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select * from TEST", Connection, transaction))
				{
					await using (var reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							for (var i = 0; i < reader.FieldCount; i++)
							{
								var dummy = reader[i];
							}
						}
					}
				}
				await transaction.RollbackAsync();
			}
		}

		[Test]
		public async Task IndexerByNameTest()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select * from TEST", Connection, transaction))
				{
					await using (var reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							for (var i = 0; i < reader.FieldCount; i++)
							{
								var dummy = reader[reader.GetName(i)];
							}
						}
					}
				}
				await transaction.RollbackAsync();
			}
		}

		[Test]
		public async Task GetSchemaTableTest()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select * from TEST", Connection, transaction))
				{
					await using (var reader = await command.ExecuteReaderAsync(CommandBehavior.SchemaOnly))
					{
						using (var schema = await reader.GetSchemaTableAsync())
						{
							var currRows = schema.Select(null, null, DataViewRowState.CurrentRows);
						}
					}
				}
				await transaction.RollbackAsync();
			}
		}

		[Test]
		public async Task GetSchemaTableWithExpressionFieldTest()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select TEST.*, 0 AS VALOR from TEST", Connection, transaction))
				{
					await using (var reader = await command.ExecuteReaderAsync(CommandBehavior.SchemaOnly))
					{
						using (var schema = await reader.GetSchemaTableAsync())
						{
							var currRows = schema.Select(null, null, DataViewRowState.CurrentRows);
						}
					}
				}
				await transaction.RollbackAsync();
			}
		}

		[Test]
		public async Task RecordAffectedTest()
		{
			await using (var command = new FbCommand("insert into test (int_field) values (100000)", Connection))
			{
				await using (var reader = await command.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{ }
					Assert.AreEqual(1, reader.RecordsAffected);
				}
			}
		}

		[Test]
		public async Task GetBytesLengthTest()
		{
			await using (var command = new FbCommand("select blob_field from TEST where int_field = @int_field", Connection))
			{
				command.Parameters.Add("@int_field", FbDbType.Integer).Value = 2;
				await using (var reader = await command.ExecuteReaderAsync())
				{
					await reader.ReadAsync();
					var length = reader.GetBytes(0, 0, null, 0, 0);
					Assert.AreEqual(13, length, "Incorrect blob length");
				}
			}
		}

		[Test]
		public async Task GetCharsLengthTest()
		{
			await using (var command = new FbCommand("select clob_field from TEST where int_field = @int_field", Connection))
			{
				command.Parameters.Add("@int_field", FbDbType.Integer).Value = 50;
				await using (var reader = await command.ExecuteReaderAsync())
				{
					await reader.ReadAsync();
					var length = reader.GetChars(0, 0, null, 0, 0);
					Assert.AreEqual(14, length, "Incorrect clob length");
				}
			}
		}

		[Test]
		public async Task ValidateDecimalSchema()
		{
			await using (var test = new FbCommand("select decimal_field from test", Connection))
			{
				await using (var r = await test.ExecuteReaderAsync(CommandBehavior.SchemaOnly))
				{
					using (var schema = await r.GetSchemaTableAsync())
					{
						Assert.AreEqual(schema.Rows[0]["ColumnSize"], 8, "Invalid length");
						Assert.AreEqual(schema.Rows[0]["NumericPrecision"], 15, "Invalid precision");
						Assert.AreEqual(schema.Rows[0]["NumericScale"], 2, "Invalid scale");
					}
				}
			}
		}

		[Test]
		public async Task DisposeTest()
		{
			await using (var command = new FbCommand("DATAREADERTEST", Connection))
			{
				command.CommandType = CommandType.StoredProcedure;
				FbCommandBuilder.DeriveParameters(command);
				await using (var reader = await command.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{ }
				}
			}
		}

		[Test]
		public async Task GetOrdinalTest()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select first 1 0 as fOo, 0 as \"BaR\", 0 as BAR from TEST", Connection, transaction))
				{
					await using (var reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							var foo = reader.GetOrdinal("foo");
							var FOO = reader.GetOrdinal("FOO");
							var fOo = reader.GetOrdinal("fOo");
							Assert.AreEqual(0, foo);
							Assert.AreEqual(0, FOO);
							Assert.AreEqual(0, fOo);

							var bar = reader.GetOrdinal("bar");
							var BaR = reader.GetOrdinal("BaR");
							Assert.AreEqual(1, bar);
							Assert.AreEqual(1, BaR);

							var BAR = reader.GetOrdinal("BAR");
							Assert.AreEqual(2, BAR);
						}
					}
				}
				await transaction.RollbackAsync();
			}
		}

		[Test]
		public async Task ReadBinaryTest()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				var bytes = new byte[1024];
				var random = new Random();
				for (var i = 0; i < bytes.Length; i++)
				{
					bytes[i] = (byte)random.Next(byte.MinValue, byte.MaxValue);
				}
				var binaryString = $"x'{BitConverter.ToString(bytes).Replace("-", string.Empty)}'";

				await using (var command = new FbCommand($"select {binaryString} from TEST", Connection, transaction))
				{
					await using (var reader = await command.ExecuteReaderAsync())
					{
						if (await reader.ReadAsync())
						{
							var value = (byte[])reader[0];
							Assert.AreEqual(bytes, value);
						}
					}
				}
				await transaction.RollbackAsync();
			}
		}

		[Test]
		public async Task ReadGuidRoundTripTest()
		{
			await using (var cmd = Connection.CreateCommand())
			{
				var guid = Guid.NewGuid();
				var commandText = $"select char_to_uuid('{guid}') from rdb$database";
				cmd.CommandText = commandText;
				await using (var reader = await cmd.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						Assert.AreEqual(guid, reader.GetGuid(0));
					}
					else
					{
						Assert.Fail();
					}
				}
			}
		}

		[Test]
		public async Task ReadGuidRoundTrip2Test()
		{
			await using (var cmd = Connection.CreateCommand())
			{
				var commandText = @"
execute block
returns (a varchar(16) character set octets, b varchar(36) character set ascii)
as
declare guid varchar(16) character set octets;
begin
	guid = gen_uuid();
	for select :guid, uuid_to_char(:guid) from rdb$database into a, b do
	begin
		suspend;
	end
end";
				cmd.CommandText = commandText;
				await using (var reader = await cmd.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						StringAssert.AreEqualIgnoringCase(reader.GetString(1), reader.GetGuid(0).ToString());
					}
					else
					{
						Assert.Fail();
					}
				}
			}
		}

		[Test]
		public async Task ReadGuidRoundTrip3Test()
		{
			await using (var cmd = Connection.CreateCommand())
			{
				var guid = Guid.NewGuid();
				var commandText = $"select cast(@guid as varchar(16) character set octets) from rdb$database";
				cmd.CommandText = commandText;
				cmd.Parameters.Add("guid", guid);
				await using (var reader = await cmd.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						Assert.AreEqual(guid, reader.GetGuid(0));
					}
					else
					{
						Assert.Fail();
					}
				}
			}
		}

		[Test]
		public async Task DNET60_EmptyFieldReadingError()
		{
			await using (var command = Connection.CreateCommand())
			{
				command.CommandText = "select '' AS EmptyColumn from rdb$database";
				await using (var r = await command.ExecuteReaderAsync())
				{
					while (await r.ReadAsync())
					{ }
				}
			}
		}

		[Test]
		public async Task DNET183_VarcharSpacesShouldNotBeTrimmed()
		{
			const string value = "foo  ";

			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast(@foo as varchar(5)) from rdb$database";
				cmd.Parameters.Add(new FbParameter() { ParameterName = "@foo", FbDbType = FbDbType.VarChar, Size = 5, Value = value });
				await using (var reader = await cmd.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						Assert.AreEqual(value, (string)reader[0]);
					}
				}
			}
		}

		[Test]
		public async Task DNET749_CommandBehaviorCloseConnectionStackOverflow()
		{
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select * from rdb$database";
				await using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
				{
					while (await reader.ReadAsync())
					{ }
				}
			}
		}

		[Test]
		public async Task ReadCancellation()
		{
			if (!EnsureServerVersion(new Version(2, 5, 0, 0)))
				return;

			using (var cts = new CancellationTokenSource())
			{
				await using (var cmd = Connection.CreateCommand())
				{
					cmd.FetchSize = 1;
					cmd.CommandText =
@"execute block
returns (i int)
as
declare variable start_time timestamp;
begin
	i = 0;
	while (i < 100) do
	begin
		i = i + 1;
		suspend;

		start_time = cast('now' as timestamp);
		while (datediff(second from start_time to cast('now' as timestamp)) <= 2) do
		begin
		end
	end
end";
					await using (var reader = await cmd.ExecuteReaderAsync())
					{
						await reader.ReadAsync(cts.Token);
						cts.CancelAfter(100);
						Assert.ThrowsAsync<OperationCanceledException>(async () =>
						{
							while (await reader.ReadAsync(cts.Token))
							{ }
						});
					}
				}
			}
		}

		[Test]
		public async Task ReadOnAlreadyCancelledToken()
		{
			if (!EnsureServerVersion(new Version(2, 5, 0, 0)))
				return;

			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();
				await using (var cmd = Connection.CreateCommand())
				{
					cmd.CommandText ="select 1 from rdb$database";
					Assert.ThrowsAsync<OperationCanceledException>(() => cmd.ExecuteReaderAsync(cts.Token));
				}
			}
		}
	}
}
