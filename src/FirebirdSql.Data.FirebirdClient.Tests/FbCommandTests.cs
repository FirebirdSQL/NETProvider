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

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests;

[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Default))]
[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Embedded))]
public class FbCommandTests : FbTestsBase
{
	const string FiniteInfiniteLoopCommand =
@"execute block as
declare variable start_time timestamp;
begin
  start_time = cast('now' as timestamp);
  while (datediff(second from start_time to cast('now' as timestamp)) <= 10) do
  begin
  end
end";

	public FbCommandTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
		: base(serverType, compression, wireCrypt)
	{ }

	[Test]
	public async Task ExecuteNonQueryTest()
	{
		await using (var transaction = await Connection.BeginTransactionAsync())
		{
			await using (var command = Connection.CreateCommand())
			{
				command.Transaction = transaction;
				command.CommandText = "insert into TEST (INT_FIELD) values (?)";
				command.Parameters.Add("@INT_FIELD", 100);
				var affectedRows = await command.ExecuteNonQueryAsync();
				Assert.AreEqual(affectedRows, 1);
				await transaction.RollbackAsync();
			}
		}
	}

	[Test]
	public async Task ExecuteReaderTest()
	{
		await using (var command = Connection.CreateCommand())
		{
			command.CommandText = "select * from TEST";
			await using (var reader = await command.ExecuteReaderAsync())
			{ }
		}
	}

	[Test]
	public async Task ExecuteMultipleReaderTest()
	{
		await using (FbCommand
			command1 = Connection.CreateCommand(),
			command2 = Connection.CreateCommand())
		{
			command1.CommandText = "select * from test where int_field = 1";
			command2.CommandText = "select * from test where int_field = 2";

			await using (var r1 = await command1.ExecuteReaderAsync())
			{
				await using (var r2 = await command2.ExecuteReaderAsync())
				{ }

				// Try to call ExecuteReader in	command1
				// it should throw an exception
				Assert.ThrowsAsync<InvalidOperationException>(() => command1.ExecuteReaderAsync());
			}
		}
	}

	[Test]
	public async Task ExecuteReaderWithBehaviorTest()
	{
		await using (var command = new FbCommand("select * from TEST", Connection))
		{
			await using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
			{ }
		}
	}

	[Test]
	public async Task ExecuteScalarTest()
	{
		await using (var command = Connection.CreateCommand())
		{
			command.CommandText = "select CHAR_FIELD from TEST where INT_FIELD = ?";
			command.Parameters.Add("@INT_FIELD", 2);
			var charFieldValue = (await command.ExecuteScalarAsync()).ToString();
			Assert.AreEqual("IRow 2", charFieldValue.TrimEnd(' '));
		}
	}

	[Test]
	public async Task ExecuteScalarWithStoredProcedureTest()
	{
		await using (var command = Connection.CreateCommand())
		{
			command.CommandText = "SimpleSP";
			command.CommandType = CommandType.StoredProcedure;
			var result = (int)await command.ExecuteScalarAsync();
			Assert.AreEqual(1000, result);
		}
	}

	[Test]
	public async Task NamedParametersTest()
	{
		await using (var command = Connection.CreateCommand())
		{
			command.CommandText = "select CHAR_FIELD from TEST where INT_FIELD = @int_field or CHAR_FIELD = @char_field";
			command.Parameters.Add("@int_field", 2);
			command.Parameters.Add("@char_field", "TWO");
			await using (var reader = await command.ExecuteReaderAsync())
			{
				var count = 0;

				while (await reader.ReadAsync())
				{
					count++;
				}
				Assert.AreEqual(1, count, "Invalid number of records fetched.");
			}
		}
	}

	[Test]
	public async Task NamedParametersAndLiterals()
	{
		await using (var command = new FbCommand("update test set char_field = 'carlos@firebird.org', bigint_field = @bigint, varchar_field = 'carlos@ado.net' where int_field = @integer", Connection))
		{
			command.Parameters.Add("@bigint", FbDbType.BigInt).Value = 200;
			command.Parameters.Add("@integer", FbDbType.Integer).Value = 1;
			var recordsAffected = await command.ExecuteNonQueryAsync();
			Assert.AreEqual(recordsAffected, 1, "Invalid number of records affected.");
		}
	}

	[Test]
	public async Task NamedParametersReuseTest()
	{
		await using (var command = new FbCommand("select * from test where int_field >= @lang and int_field <= @lang", Connection))
		{
			command.Parameters.Add("@lang", FbDbType.Integer).Value = 10;
			await using (var reader = await command.ExecuteReaderAsync())
			{
				var count = 0;
				var intValue = 0;
				while (await reader.ReadAsync())
				{
					if (count == 0)
					{
						intValue = reader.GetInt32(0);
					}
					count++;
				}
				Assert.AreEqual(1, count, "Invalid number of records fetched.");
				Assert.AreEqual(10, intValue, "Invalid record fetched.");
			}
		}
	}

	[Test]
	public async Task NamedParametersPublicAccessor()
	{
		await using (var command = new FbCommand("select * from test where int_field >= @x1 and int_field <= @x2", Connection))
		{
			Assert.IsNotNull(command.NamedParameters, "Unexpected null reference.");
			Assert.IsTrue(command.NamedParameters.Count == 0, "Expected count 0 of named parameters before command prepare.");

			await command.PrepareAsync();

			Assert.IsTrue(command.NamedParameters.Count == 2, "Expected count 2 of named parameters after command prepare.");
			Assert.AreEqual(command.NamedParameters[0], "@x1");
			Assert.AreEqual(command.NamedParameters[1], "@x2");
		}
	}

	[Test]
	public async Task ExecuteStoredProcTest()
	{
		await using (var command = new FbCommand("EXECUTE PROCEDURE GETVARCHARFIELD(?)", Connection))
		{
			command.CommandType = CommandType.StoredProcedure;
			command.Parameters.Add("@ID", FbDbType.VarChar).Direction = ParameterDirection.Input;
			command.Parameters.Add("@VARCHAR_FIELD", FbDbType.VarChar).Direction = ParameterDirection.Output;
			command.Parameters[0].Value = 1;
			await command.ExecuteNonQueryAsync();
			Assert.AreEqual("IRow Number 1", command.Parameters[1].Value);
		}
	}

	[Test]
	public async Task RecordAffectedTest()
	{
		await using (var command = new FbCommand("insert into test (int_field) values (100000)", Connection))
		{
			await using (var reader = await command.ExecuteReaderAsync())
			{
				Assert.AreEqual(1, reader.RecordsAffected);
				while (await reader.ReadAsync())
				{ }
				Assert.AreEqual(1, reader.RecordsAffected);
			}
		}
	}

	[Test]
	public async Task ExecuteNonQueryWithOutputParameters()
	{
		await using (var command = new FbCommand("EXECUTE PROCEDURE GETASCIIBLOB(?)", Connection))
		{
			command.CommandType = CommandType.StoredProcedure;
			command.Parameters.Add("@ID", FbDbType.VarChar).Direction = ParameterDirection.Input;
			command.Parameters.Add("@CLOB_FIELD", FbDbType.Text).Direction = ParameterDirection.Output;
			command.Parameters[0].Value = 1;
			await command.ExecuteNonQueryAsync();
			Assert.AreEqual("IRow Number 1", command.Parameters[1].Value, "Output parameter value is not valid");
		}
	}

	[Test]
	public async Task InvalidParameterFormat()
	{
		await using (var transaction = await Connection.BeginTransactionAsync())
		{
			try
			{
				await using (var command = new FbCommand("update test set timestamp_field = @timestamp where int_field = @integer", Connection, transaction))
				{
					command.Parameters.Add("@timestamp", FbDbType.TimeStamp).Value = 1;
					command.Parameters.Add("@integer", FbDbType.Integer).Value = 1;
					await command.ExecuteNonQueryAsync();
				}
				await transaction.CommitAsync();
			}
			catch
			{
				await transaction.RollbackAsync();
			}
		}
	}

	[Test]
	public async Task UnicodeTest()
	{
		try
		{
			await using (var create = new FbCommand("CREATE TABLE VARCHARTEST (VARCHAR_FIELD  VARCHAR(10))", Connection))
			{
				await create.ExecuteNonQueryAsync();
			}
			var statements = new[]
			{
					"INSERT INTO VARCHARTEST (VARCHAR_FIELD) VALUES ('1')",
					"INSERT INTO VARCHARTEST (VARCHAR_FIELD) VALUES ('11')",
					"INSERT INTO VARCHARTEST (VARCHAR_FIELD) VALUES ('111')",
					"INSERT INTO VARCHARTEST (VARCHAR_FIELD) VALUES ('1111')"
				};
			foreach (string statement in statements)
			{
				await using (var insert = new FbCommand(statement, Connection))
				{
					await insert.ExecuteNonQueryAsync();
				}
			}
			await using (var cmd = new FbCommand("select * from varchartest", Connection))
			{
				await using (var r = await cmd.ExecuteReaderAsync())
				{
					while (await r.ReadAsync())
					{
						var dummy = r[0];
					}
				}
			}
		}
		finally
		{
			await using (var drop = new FbCommand("DROP TABLE VARCHARTEST", Connection))
			{
				await drop.ExecuteNonQueryAsync();
			}
		}
	}

	[Test]
	public async Task SimplifiedChineseTest()
	{
		const string Value = "中文";
		try
		{
			await using (var cmd = new FbCommand("CREATE TABLE TABLE1 (FIELD1 varchar(20))", Connection))
			{
				await cmd.ExecuteNonQueryAsync();
			}
			await using (var cmd = new FbCommand("INSERT INTO TABLE1 VALUES (@value)", Connection))
			{
				cmd.Parameters.Add("@value", FbDbType.VarChar).Value = Value;
				await cmd.ExecuteNonQueryAsync();
			}
			await using (var cmd = new FbCommand($"INSERT INTO TABLE1 VALUES ('{Value}')", Connection))
			{
				await cmd.ExecuteNonQueryAsync();
			}
			await using (var cmd = new FbCommand("SELECT * FROM TABLE1", Connection))
			{
				await using (var reader = await cmd.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						Assert.AreEqual(Value, reader[0]);
					}
				}
			}
		}
		finally
		{
			await using (var cmd = new FbCommand("DROP TABLE TABLE1", Connection))
			{
				await cmd.ExecuteNonQueryAsync();
			}
		}
	}

	[Test]
	public async Task InsertDateTest()
	{
		await using (var command = new FbCommand("insert into TEST (int_field, date_field) values (1002, @date)", Connection))
		{
			command.Parameters.Add("@date", FbDbType.Date).Value = DateTime.Now.ToString();
			var ra = await command.ExecuteNonQueryAsync();
			Assert.AreEqual(ra, 1);
		}
	}

	[Test]
	public async Task InsertNullTest()
	{
		await using (var command = new FbCommand("insert into TEST (int_field) values (@value)", Connection))
		{
			command.Parameters.Add("@value", FbDbType.Integer).Value = null;
			try
			{
				await command.ExecuteNonQueryAsync();
				Assert.Fail("The command was executed without throwing an exception.");
			}
			catch
			{ }
		}
	}

	[Test]
	public async Task InsertDateTimeTest()
	{
		var value = DateTime.Now;

		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "insert into test (int_field, timestamp_field) values (1002, @dt)";
			cmd.Parameters.Add("@dt", FbDbType.TimeStamp).Value = value;
			var ra = await cmd.ExecuteNonQueryAsync();
			Assert.AreEqual(ra, 1);
		}

		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select timestamp_field from test where int_field = 1002";
			var result = (DateTime)await cmd.ExecuteScalarAsync();
			Assert.AreEqual(value.ToString(), result.ToString());
		}
	}

	[Test]
	public async Task InsertTimeStampTest()
	{
		var value = DateTime.Now.ToString();

		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "insert into test (int_field, timestamp_field) values (1002, @ts)";
			cmd.Parameters.Add("@ts", FbDbType.TimeStamp).Value = value;
			var ra = await cmd.ExecuteNonQueryAsync();
			Assert.AreEqual(ra, 1);
		}

		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select timestamp_field from test where int_field = 1002";
			var result = (DateTime)await cmd.ExecuteScalarAsync();
			Assert.AreEqual(value, result.ToString());
		}
	}

	[Test]
	public async Task InsertTimeTest()
	{
		var t = new TimeSpan(0, 5, 6, 7, 231);

		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "insert into test (int_field, time_field) values (2245, @t)";
			cmd.Parameters.Add("@t", FbDbType.Time).Value = t;
			var ra = await cmd.ExecuteNonQueryAsync();
			Assert.AreEqual(ra, 1);
		}

		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select time_field from test where int_field = 2245";
			var result = (TimeSpan)await cmd.ExecuteScalarAsync();
			Assert.AreEqual(t.Hours, result.Hours, "hours are not same");
			Assert.AreEqual(t.Minutes, result.Minutes, "minutes are not same");
			Assert.AreEqual(t.Seconds, result.Seconds, "seconds are not same");
			Assert.AreEqual(t.Milliseconds, result.Milliseconds, "milliseconds are not same");
		}
	}

	[Test]
	public async Task InsertTimeOldTest()
	{
		var t = DateTime.Today;
		t = t.AddHours(5);
		t = t.AddMinutes(6);
		t = t.AddSeconds(7);
		t = t.AddMilliseconds(231);

		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "insert into test (int_field, time_field) values (2245, @t)";
			cmd.Parameters.Add("@t", FbDbType.Time).Value = t;
			var ra = await cmd.ExecuteNonQueryAsync();
			Assert.AreEqual(ra, 1);
		}

		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select time_field from test where int_field = 2245";
			var result = (TimeSpan)await cmd.ExecuteScalarAsync();
			Assert.AreEqual(t.Hour, result.Hours, "hours are not same");
			Assert.AreEqual(t.Minute, result.Minutes, "minutes are not same");
			Assert.AreEqual(t.Second, result.Seconds, "seconds are not same");
			Assert.AreEqual(t.Millisecond, result.Milliseconds, "milliseconds are not same");
		}
	}

	[Test]
	public async Task ParameterDescribeTest()
	{
		await using (var command = new FbCommand("insert into TEST (int_field) values (@value)", Connection))
		{
			await command.PrepareAsync();
			command.Parameters.Add("@value", FbDbType.Integer).Value = 100000;
			await command.ExecuteNonQueryAsync();
		}
	}

	[Test]
	public async Task ReadOnlyTransactionTest()
	{
		await using (var command = Connection.CreateCommand())
		{
			await using (var transaction = await Connection.BeginTransactionAsync(new FbTransactionOptions() { TransactionBehavior = FbTransactionBehavior.Read, WaitTimeout = null }))
			{
				command.Transaction = transaction;
				command.CommandType = CommandType.Text;
				command.CommandText = "CREATE TABLE X_TABLE_1(FIELD VARCHAR(50));";
				Assert.ThrowsAsync<FbException>(() => command.ExecuteNonQueryAsync());
				await transaction.CommitAsync();
			}
		}
	}

	[Test]
	public async Task ReturningClauseParameterTest()
	{
		if (!EnsureServerVersionAtMost(new Version(5, 0, 0, 0)))
			return;

		const int ColumnValue = 1234;
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = string.Format("update TEST set int_field = '{0}' where int_field = 1 returning int_field", ColumnValue);
			cmd.Parameters.Add(new FbParameter() { Direction = ParameterDirection.Output });
			await cmd.ExecuteNonQueryAsync();
			var returningValue = cmd.Parameters[0].Value;
			Assert.AreEqual(ColumnValue, returningValue);
		}
	}

	[Test]
	public async Task ReturningClauseScalarTest()
	{
		const int ColumnValue = 1234;
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = string.Format("update TEST set int_field = '{0}' where int_field = 1 returning int_field", ColumnValue);
			var returningValue = (int)await cmd.ExecuteScalarAsync();
			Assert.AreEqual(ColumnValue, returningValue);
		}
	}

	[Test]
	public async Task ReturningClauseReaderTest()
	{
		const int ColumnValue = 1234;
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = string.Format("update TEST set int_field = '{0}' where int_field = 1 returning int_field", ColumnValue);
			await using (var reader = await cmd.ExecuteReaderAsync())
			{
				Assert.IsTrue(await reader.ReadAsync());
				var returningValue = (int)reader[0];
				Assert.AreEqual(ColumnValue, returningValue);
			}
		}
	}

	[Test]
	public async Task ReadingVarcharOctetsTest()
	{
		await using (var cmd = Connection.CreateCommand())
		{
			const string data = "1234";
			byte[] read = null;

			cmd.CommandText = string.Format("select cast('{0}' as varchar(10) character set octets) from rdb$database", data);
			await using (var reader = await cmd.ExecuteReaderAsync())
			{
				if (await reader.ReadAsync())
				{
					read = (byte[])reader[0];
				}
			}

			var expected = Encoding.ASCII.GetBytes(data);
			Assert.AreEqual(expected, read);
		}
	}

	[Test]
	public async Task ReadingCharOctetsTest()
	{
		await using (var cmd = Connection.CreateCommand())
		{
			const string data = "1234";
			byte[] read = null;

			cmd.CommandText = string.Format("select cast('{0}' as char(10) character set octets) from rdb$database", data);
			await using (var reader = await cmd.ExecuteReaderAsync())
			{
				if (await reader.ReadAsync())
				{
					read = (byte[])reader[0];
				}
			}

			var expected = new byte[10];
			Encoding.ASCII.GetBytes(data).CopyTo(expected, 0);
			Assert.AreEqual(expected, read);
		}
	}

	[Test]
	public async Task GetCommandPlanTest()
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select * from test";
			await cmd.PrepareAsync();
			var plan = default(string);
			Assert.DoesNotThrowAsync(async () => { plan = await cmd.GetCommandPlanAsync(); });
			Assert.IsNotEmpty(plan);
		}
	}

	[Test]
	public async Task GetCommandExplainedPlanTest()
	{
		if (!EnsureServerVersionAtLeast(new Version(3, 0, 0, 0)))
			return;

		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select * from test";
			await cmd.PrepareAsync();
			var plan = default(string);
			Assert.DoesNotThrowAsync(async () => { plan = await cmd.GetCommandExplainedPlanAsync(); });
			Assert.IsNotEmpty(plan);
		}
	}

	[Test]
	public async Task GetCommandPlanNoPlanTest()
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "recreate table NoPlan (id int)";
			await cmd.PrepareAsync();
			var plan = default(string);
			Assert.DoesNotThrowAsync(async () => { plan = await cmd.GetCommandPlanAsync(); });
			Assert.IsEmpty(plan);
		}
	}

	[Test]
	public async Task GetCommandExplainedPlanNoPlanTest()
	{
		if (!EnsureServerVersionAtLeast(new Version(3, 0, 0, 0)))
			return;

		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "recreate table NoPaln (id int)";
			await cmd.PrepareAsync();
			var plan = default(string);
			Assert.DoesNotThrowAsync(async () => { plan = await cmd.GetCommandExplainedPlanAsync(); });
			Assert.IsEmpty(plan);
		}
	}

	[Test]
	public async Task ReadsTimeWithProperPrecisionTest()
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select cast('00:00:01.4321' as time) from rdb$database";
			var result = (TimeSpan)await cmd.ExecuteScalarAsync();
			Assert.AreEqual(TimeSpan.FromTicks(14321000), result);
		}
	}

	[Test]
	public async Task PassesTimeSpanWithProperPrecisionTest()
	{
		var ts = TimeSpan.FromTicks(14321000);
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select cast(@value as time) from rdb$database";
			cmd.Parameters.Add("value", ts);
			var result = (TimeSpan)await cmd.ExecuteScalarAsync();
			Assert.AreEqual(ts, result);
		}
	}

	[Test]
	public async Task ReadsDateTimeWithProperPrecisionTest()
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select cast('1.2.2015 05:06:01.4321' as timestamp) from rdb$database";
			var result = (DateTime)await cmd.ExecuteScalarAsync();
			Assert.AreEqual(new DateTime(635583639614321000), result);
		}
	}

	[Test]
	public async Task PassesDateTimeWithProperPrecisionTest()
	{
		var dt = new DateTime(635583639614321000);
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select cast(@value as timestamp) from rdb$database";
			cmd.Parameters.Add("value", dt);
			var result = (DateTime)await cmd.ExecuteScalarAsync();
			Assert.AreEqual(dt, result);
		}
	}

	[Test]
	public async Task HighLowSurrogatePassingTest()
	{
		await using (var cmd = Connection.CreateCommand())
		{
			const string Value = "😊!";
			cmd.CommandText = "select cast(@value1 as varchar(2) character set utf8), cast(@value2 as char(2) character set utf8) from rdb$database";
			cmd.Parameters.Add("value1", Value);
			cmd.Parameters.Add("value2", Value);
			await using (var reader = await cmd.ExecuteReaderAsync())
			{
				await reader.ReadAsync();
				Assert.AreEqual(Value, reader[0]);
				Assert.AreEqual(Value, reader[1]);
			}
		}
	}

	[Test]
	public async Task HighLowSurrogateReadingTest()
	{
		await using (var cmd = Connection.CreateCommand())
		{
			const string Value = "😊!";
			cmd.CommandText = "select cast(x'F09F988A21' as varchar(2) character set utf8), cast(x'F09F988A21' as char(2) character set utf8) from rdb$database";
			await using (var reader = await cmd.ExecuteReaderAsync())
			{
				await reader.ReadAsync();
				Assert.AreEqual(Value, reader[0]);
				Assert.AreEqual(Value, reader[1]);
			}
		}
	}

	[Test]
	public async Task ExecuteNonQueryReturnsMinusOneOnNonInsertUpdateDeleteTest()
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select 1 from rdb$database";
			var ra = await cmd.ExecuteNonQueryAsync();
			Assert.AreEqual(-1, ra);
		}
	}

	[Test]
	public async Task CommandCancellationDirectTest()
	{
		if (!EnsureServerVersionAtLeast(new Version(2, 5, 0, 0)))
			return;

		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = FiniteInfiniteLoopCommand;
			async Task Execute()
			{
				await Task.Yield();
				await cmd.ExecuteNonQueryAsync();
			}
			var executeTask = Execute();
			Thread.Sleep(2000);
			cmd.Cancel();
			Thread.Sleep(2000);
			Assert.ThrowsAsync<OperationCanceledException>(async () => await executeTask);
		}
	}

	[Test]
	public async Task CommandCancellationCancellationTokenTest()
	{
		if (!EnsureServerVersionAtLeast(new Version(2, 5, 0, 0)))
			return;

		using (var cts = new CancellationTokenSource())
		{
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = FiniteInfiniteLoopCommand;
				async Task Execute(CancellationToken cancellationToken)
				{
					await Task.Yield();
					await cmd.ExecuteNonQueryAsync(cancellationToken);
				}
				var executeTask = Execute(cts.Token);
				Thread.Sleep(2000);
				cts.Cancel();
				Thread.Sleep(2000);
				Assert.ThrowsAsync<OperationCanceledException>(async () => await executeTask);
			}
		}
	}

	[Test]
	public async Task CommandUsableAfterCancellationTest()
	{
		if (!EnsureServerVersionAtLeast(new Version(2, 5, 0, 0)))
			return;

		using (var cts = new CancellationTokenSource())
		{
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = FiniteInfiniteLoopCommand;
				async Task Execute(CancellationToken cancellationToken)
				{
					await Task.Yield();
					await cmd.ExecuteNonQueryAsync(cancellationToken);
				}
				var executeTask = Execute(cts.Token);
				Thread.Sleep(2000);
				cts.Cancel();
				Thread.Sleep(2000);
				try
				{
					await executeTask;
				}
				catch (OperationCanceledException)
				{ }
				cmd.CommandText = "select 1 from rdb$database union all select 6 from rdb$database";
				await using (var reader = await cmd.ExecuteReaderAsync())
				{
					var result = new List<int>();
					while (await reader.ReadAsync())
					{
						result.Add(reader.GetInt32(0));
					}
					CollectionAssert.AreEqual(new[] { 1, 6 }, result);
				}
			}
		}
	}

	[Test]
	public async Task ExecuteNonQueryOnAlreadyCancelledToken()
	{
		if (!EnsureServerVersionAtLeast(new Version(2, 5, 0, 0)))
			return;

		using (var cts = new CancellationTokenSource())
		{
			cts.Cancel();
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select 1 from rdb$database";
				Assert.ThrowsAsync<OperationCanceledException>(() => cmd.ExecuteNonQueryAsync(cts.Token));
			}
		}
	}

	[Test]
	public async Task PassDateOnly()
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select cast(@x as date) from rdb$database";
			cmd.Parameters.Add("x", new DateOnly(2021, 11, 29));
			var value = (DateTime)await cmd.ExecuteScalarAsync();
			Assert.AreEqual(2021, value.Year);
			Assert.AreEqual(11, value.Month);
			Assert.AreEqual(29, value.Day);
		}
	}

	[Test]
	public async Task PassTimeOnly()
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select cast(@x as time) from rdb$database";
			cmd.Parameters.Add("x", new TimeOnly(501940213000));
			var value = (TimeSpan)await cmd.ExecuteScalarAsync();
			Assert.AreEqual(501940213000, value.Ticks);
		}
	}

	[Test]
	public async Task CommandTimeoutTest()
	{
		if (!EnsureServerVersionAtLeast(new Version(4, 0, 0, 0)))
			return;

		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = FiniteInfiniteLoopCommand;
			cmd.CommandTimeout = 2;
			Assert.ThrowsAsync<OperationCanceledException>(async () => await cmd.ExecuteNonQueryAsync());
		}
	}

	[Test]
	public async Task DefaultTimeoutValueTest()
	{
		var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
		await using (var conn = new FbConnection(csb.ToString()))
		{
			await using (var cmd = conn.CreateCommand())
			{
				Assert.AreEqual(0, cmd.CommandTimeout);
			}
		}
	}

	[Test]
	public async Task TimeoutConnectionStringTest()
	{
		var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
		csb.CommandTimeout = 20;
		await using (var conn = new FbConnection(csb.ToString()))
		{
			await using (var cmd = conn.CreateCommand())
			{
				Assert.AreEqual(20, cmd.CommandTimeout);
			}
		}
	}

	[Test]
	public async Task TimeoutNegativeConnectionStringTest()
	{
		var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
		csb.CommandTimeout = -1;
		await using (var conn = new FbConnection(csb.ToString()))
		{
			await using (var cmd = conn.CreateCommand())
			{
				Assert.AreEqual(0, cmd.CommandTimeout);
			}
		}
	}

	[Test]
	public async Task Timeout0ConnectionStringTest()
	{
		var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
		csb.CommandTimeout = 0;
		await using (var conn = new FbConnection(csb.ToString()))
		{
			await using (var cmd = conn.CreateCommand())
			{
				Assert.AreEqual(0, cmd.CommandTimeout);
			}
		}
	}

	[Test]
	public async Task SetTimeoutTest()
	{
		var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
		await using (var conn = new FbConnection(csb.ToString()))
		{
			await using (var cmd = conn.CreateCommand())
			{
				cmd.CommandTimeout = 6;
				Assert.AreEqual(6, cmd.CommandTimeout);
			}
		}
	}

	[Test]
	public async Task SetTimeout0Test()
	{
		var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
		await using (var conn = new FbConnection(csb.ToString()))
		{
			await using (var cmd = conn.CreateCommand())
			{
				cmd.CommandTimeout = 0;
				Assert.AreEqual(0, cmd.CommandTimeout);
			}
		}
	}

	[Test]
	public async Task TimeoutConnectionStringOverrideTest()
	{
		var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
		csb.CommandTimeout = 20;
		await using (var conn = new FbConnection(csb.ToString()))
		{
			await using (var cmd = conn.CreateCommand())
			{
				cmd.CommandTimeout = 2;
				Assert.AreEqual(2, cmd.CommandTimeout);
			}
		}
	}

	[Test]
	public async Task TimeoutConnectionStringOverride0Test()
	{
		var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
		csb.CommandTimeout = 20;
		await using (var conn = new FbConnection(csb.ToString()))
		{
			await using (var cmd = conn.CreateCommand())
			{
				cmd.CommandTimeout = 0;
				Assert.AreEqual(0, cmd.CommandTimeout);
			}
		}
	}
}
