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
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[FbTestFixture(FbServerType.Default, false)]
	[FbTestFixture(FbServerType.Default, true)]
	[FbTestFixture(FbServerType.Embedded, default)]
	public class FbCommandTests : FbTestsBase
	{
		#region Constructors

		public FbCommandTests(FbServerType serverType, bool compression)
			: base(serverType, compression)
		{ }

		#endregion

		#region Unit Tests

		[Test]
		public void ExecuteNonQueryTest()
		{
			Transaction = Connection.BeginTransaction();

			var command = Connection.CreateCommand();

			command.Transaction = Transaction;
			command.CommandText = "insert into TEST	(INT_FIELD)	values (?) ";

			command.Parameters.Add("@INT_FIELD", 100);

			var affectedRows = command.ExecuteNonQuery();

			Assert.AreEqual(affectedRows, 1);

			Transaction.Rollback();

			command.Dispose();
		}

		[Test]
		public void ExecuteReaderTest()
		{
			var command = Connection.CreateCommand();

			command.CommandText = "select *	from TEST";

			var reader = command.ExecuteReader();
			reader.Close();

			command.Dispose();
		}

		[Test]
		public void ExecuteMultipleReaderTest()
		{
			var command1 = Connection.CreateCommand();
			var command2 = Connection.CreateCommand();

			command1.CommandText = "select * from test where int_field = 1";
			command2.CommandText = "select * from test where int_field = 2";

			var r1 = command1.ExecuteReader();
			var r2 = command2.ExecuteReader();

			r2.Close();

			try
			{
				// Try to call ExecuteReader in	command1
				// it should throw an exception
				r2 = command1.ExecuteReader();

				throw new InvalidProgramException();
			}
			catch
			{
				r1.Close();
			}
		}

		[Test]
		public void ExecuteReaderWithBehaviorTest()
		{
			var command = new FbCommand("select *	from TEST", Connection);

			var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
			reader.Close();

			command.Dispose();
		}

		[Test]
		public void ExecuteScalarTest()
		{
			var command = Connection.CreateCommand();

			command.CommandText = "select CHAR_FIELD from TEST where INT_FIELD = ?";
			command.Parameters.Add("@INT_FIELD", 2);

			var charFieldValue = command.ExecuteScalar().ToString();

			Assert.AreEqual("IRow 2", charFieldValue.TrimEnd(' '));

			command.Dispose();
		}

		[Test]
		public void ExecuteScalarWithStoredProcedureTest()
		{
			var command = Connection.CreateCommand();

			command.CommandText = "SimpleSP";
			command.CommandType = CommandType.StoredProcedure;

			var result = (int)command.ExecuteScalar();

			Assert.AreEqual(1000, result);

			command.Dispose();
		}

		[Test]
		public void PrepareTest()
		{
			var command = new FbCommand("insert into PrepareTest(test_field) values(@test_field);", Connection);

			command.Parameters.Add("@test_field", FbDbType.VarChar).Value = DBNull.Value;
			command.Prepare();

			for (var i = 0; i < 5; i++)
			{
				if (i < 1)
				{
					command.Parameters[0].Value = DBNull.Value;
				}
				else
				{
					command.Parameters[0].Value = i.ToString();
				}
				command.ExecuteNonQuery();
			}

			command.Dispose();

			var select = new FbCommand("select * from	PrepareTest", Connection);
			var reader = select.ExecuteReader();
			var count = 0;
			while (reader.Read())
			{
				if (count == 0)
				{
					Assert.AreEqual(DBNull.Value, reader[0], "Invalid value.");
				}
				else
				{
					Assert.AreEqual(count, reader.GetInt32(0), "Invalid	value.");
				}

				count++;
			}
			reader.Close();
			select.Dispose();
		}

		[Test]
		public void NamedParametersTest()
		{
			var command = Connection.CreateCommand();

			command.CommandText = "select CHAR_FIELD from TEST where INT_FIELD = @int_field	or CHAR_FIELD =	@char_field";

			command.Parameters.Add("@int_field", 2);
			command.Parameters.Add("@char_field", "TWO");

			var reader = command.ExecuteReader();

			var count = 0;

			while (reader.Read())
			{
				count++;
			}

			Assert.AreEqual(1, count, "Invalid number of records fetched.");

			reader.Close();
			command.Dispose();
		}

		[Test]
		public void NamedParametersAndLiterals()
		{
			var sql = "update test set char_field = 'carlos@firebird.org', bigint_field = @bigint, varchar_field	= 'carlos@ado.net' where int_field = @integer";

			var command = new FbCommand(sql, Connection);
			command.Parameters.Add("@bigint", FbDbType.BigInt).Value = 200;
			command.Parameters.Add("@integer", FbDbType.Integer).Value = 1;

			var recordsAffected = command.ExecuteNonQuery();

			command.Dispose();

			Assert.AreEqual(recordsAffected, 1, "Invalid number	of records affected.");
		}

		[Test]
		public void NamedParametersReuseTest()
		{
			var sql = "select * from	test where int_field >=	@lang and int_field	<= @lang";

			var command = new FbCommand(sql, Connection);
			command.Parameters.Add("@lang", FbDbType.Integer).Value = 10;

			var reader = command.ExecuteReader();

			var count = 0;
			var intValue = 0;

			while (reader.Read())
			{
				if (count == 0)
				{
					intValue = reader.GetInt32(0);
				}
				count++;
			}

			Assert.AreEqual(1, count, "Invalid number of records fetched.");
			Assert.AreEqual(10, intValue, "Invalid record fetched.");

			reader.Close();
			command.Dispose();
		}

		[Test]
		public void ExecuteStoredProcTest()
		{
			var command = new FbCommand("EXECUTE PROCEDURE GETVARCHARFIELD(?)", Connection);

			command.CommandType = CommandType.StoredProcedure;

			command.Parameters.Add("@ID", FbDbType.VarChar).Direction = ParameterDirection.Input;
			command.Parameters.Add("@VARCHAR_FIELD", FbDbType.VarChar).Direction = ParameterDirection.Output;

			command.Parameters[0].Value = 1;

			command.ExecuteNonQuery();

			Assert.AreEqual("IRow Number 1", command.Parameters[1].Value);
		}

		[Test]
		public void RecordAffectedTest()
		{
			var sql = "insert into test (int_field) values (100000)";

			var command = new FbCommand(sql, Connection);

			var reader = command.ExecuteReader();

			Assert.AreEqual(1, reader.RecordsAffected);

			while (reader.Read())
			{
			}

			reader.Close();

			Assert.AreEqual(1, reader.RecordsAffected);
		}

		[Test]
		public void ExecuteNonQueryWithOutputParameters()
		{
			var command = new FbCommand("EXECUTE PROCEDURE GETASCIIBLOB(?)", Connection);

			command.CommandType = CommandType.StoredProcedure;

			command.Parameters.Add("@ID", FbDbType.VarChar).Direction = ParameterDirection.Input;
			command.Parameters.Add("@CLOB_FIELD", FbDbType.Text).Direction = ParameterDirection.Output;

			command.Parameters[0].Value = 1;

			command.ExecuteNonQuery();

			Assert.AreEqual("IRow Number 1", command.Parameters[1].Value, "Output parameter value is not valid");

			command.Dispose();
		}

		[Test]
		public void InvalidParameterFormat()
		{
			var sql = "update test set timestamp_field =	@timestamp where int_field = @integer";

			var transaction = Connection.BeginTransaction();
			try
			{
				var command = new FbCommand(sql, Connection, transaction);
				command.Parameters.Add("@timestamp", FbDbType.TimeStamp).Value = 1;
				command.Parameters.Add("@integer", FbDbType.Integer).Value = 1;

				command.ExecuteNonQuery();

				command.Dispose();

				transaction.Commit();
			}
			catch
			{
				transaction.Rollback();
			}
		}

		[Test]
		public void UnicodeTest()
		{
			try
			{
				using (var create = new FbCommand("CREATE TABLE VARCHARTEST (VARCHAR_FIELD  VARCHAR(10))", Connection))
				{
					create.ExecuteNonQuery();
				}

				var l = new List<string>();

				l.Add("INSERT INTO VARCHARTEST (VARCHAR_FIELD) VALUES ('1')");
				l.Add("INSERT INTO VARCHARTEST (VARCHAR_FIELD) VALUES ('11')");
				l.Add("INSERT INTO VARCHARTEST (VARCHAR_FIELD) VALUES ('111')");
				l.Add("INSERT INTO VARCHARTEST (VARCHAR_FIELD) VALUES ('1111')");

				foreach (string statement in l)
				{
					using (var insert = new FbCommand(statement, Connection))
					{
						insert.ExecuteNonQuery();
					}
				}

				var sql = "select * from	varchartest";

				using (var cmd = new FbCommand(sql, Connection))
				{
					using (var r = cmd.ExecuteReader())
					{
						while (r.Read())
						{
							var dummy = r[0];
						}
					}
				}
			}
			finally
			{
				using (var drop = new FbCommand("DROP TABLE VARCHARTEST", Connection))
				{
					drop.ExecuteNonQuery();
				}
			}
		}

		[Test]
		public void SimplifiedChineseTest()
		{
			const string Value = "中文";
			try
			{
				using (var cmd = new FbCommand("CREATE TABLE TABLE1 (FIELD1 varchar(20))", Connection))
				{
					cmd.ExecuteNonQuery();
				}

				using (var cmd = new FbCommand("INSERT INTO TABLE1 VALUES (@value)", Connection))
				{
					cmd.Parameters.Add("@value", FbDbType.VarChar).Value = Value;
					cmd.ExecuteNonQuery();
				}
				using (var cmd = new FbCommand($"INSERT INTO TABLE1 VALUES ('{Value}')", Connection))
				{
					cmd.ExecuteNonQuery();
				}

				using (var cmd = new FbCommand("SELECT * FROM TABLE1", Connection))
				{
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							Assert.AreEqual(Value, reader[0]);
						}
					}
				}
			}
			finally
			{
				using (var cmd = new FbCommand("DROP TABLE TABLE1", Connection))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

		[Test]
		public void InsertDateTest()
		{
			var sql = "insert into TEST (int_field, date_field) values (1002, @date)";

			var command = new FbCommand(sql, Connection);

			command.Parameters.Add("@date", FbDbType.Date).Value = DateTime.Now.ToString();

			var ra = command.ExecuteNonQuery();

			Assert.AreEqual(ra, 1);
		}

		[Test]
		public void InsertNullTest()
		{
			var sql = "insert into TEST (int_field) values (@value)";

			var command = new FbCommand(sql, Connection);
			command.Parameters.Add("@value", FbDbType.Integer).Value = null;

			try
			{
				command.ExecuteNonQuery();

				throw new Exception("The command was executed without throw	an exception");
			}
			catch
			{ }
		}

		[Test]
		public void InsertDateTimeTest()
		{
			var value = DateTime.Now;

			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "insert into test (int_field, timestamp_field) values (1002, @dt)";
				cmd.Parameters.Add("@dt", FbDbType.TimeStamp).Value = value;

				var ra = cmd.ExecuteNonQuery();

				Assert.AreEqual(ra, 1);
			}

			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select timestamp_field from test where int_field = 1002";
				var result = (DateTime)cmd.ExecuteScalar();

				Assert.AreEqual(value.ToString(), result.ToString());
			}
		}

		[Test]
		public void InsertTimeStampTest()
		{
			var value = DateTime.Now.ToString();

			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "insert into test (int_field, timestamp_field) values (1002, @ts)";
				cmd.Parameters.Add("@ts", FbDbType.TimeStamp).Value = value;

				var ra = cmd.ExecuteNonQuery();

				Assert.AreEqual(ra, 1);
			}

			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select timestamp_field from test where int_field = 1002";
				var result = (DateTime)cmd.ExecuteScalar();

				Assert.AreEqual(value, result.ToString());
			}
		}

		[Test]
		public void InsertTimeTest()
		{
			var t = new TimeSpan(0, 5, 6, 7, 231);

			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "insert into test (int_field, time_field) values (2245, @t)";
				cmd.Parameters.Add("@t", FbDbType.Time).Value = t;

				var ra = cmd.ExecuteNonQuery();

				Assert.AreEqual(ra, 1);
			}

			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select time_field from test where int_field = 2245";
				var result = (TimeSpan)cmd.ExecuteScalar();

				Assert.AreEqual(t.Hours, result.Hours, "hours are not same");
				Assert.AreEqual(t.Minutes, result.Minutes, "minutes are not same");
				Assert.AreEqual(t.Seconds, result.Seconds, "seconds are not same");
				Assert.AreEqual(t.Milliseconds, result.Milliseconds, "milliseconds are not same");
			}
		}

		[Test]
		public void InsertTimeOldTest()
		{
			var t = DateTime.Today;
			t = t.AddHours(5);
			t = t.AddMinutes(6);
			t = t.AddSeconds(7);
			t = t.AddMilliseconds(231);

			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "insert into test (int_field, time_field) values (2245, @t)";
				cmd.Parameters.Add("@t", FbDbType.Time).Value = t;

				var ra = cmd.ExecuteNonQuery();

				Assert.AreEqual(ra, 1);
			}

			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select time_field from test where int_field = 2245";
				var result = (TimeSpan)cmd.ExecuteScalar();

				Assert.AreEqual(t.Hour, result.Hours, "hours are not same");
				Assert.AreEqual(t.Minute, result.Minutes, "minutes are not same");
				Assert.AreEqual(t.Second, result.Seconds, "seconds are not same");
				Assert.AreEqual(t.Millisecond, result.Milliseconds, "milliseconds are not same");
			}
		}

		[Test]
		public void ParameterDescribeTest()
		{
			var sql = "insert into TEST (int_field) values (@value)";

			var command = new FbCommand(sql, Connection);
			command.Prepare();
			command.Parameters.Add("@value", FbDbType.Integer).Value = 100000;

			command.ExecuteNonQuery();

			command.Dispose();
		}

		[Test]
		public void ReadOnlyTransactionTest()
		{
			using (IDbCommand command = Connection.CreateCommand())
			{
				using (IDbTransaction transaction = Connection.BeginTransaction(new FbTransactionOptions() { TransactionBehavior = FbTransactionBehavior.Read, WaitTimeout = null }))
				{
					try
					{
						command.Transaction = transaction;
						command.CommandType = System.Data.CommandType.Text;
						command.CommandText = "CREATE TABLE	X_TABLE_1(FIELD	VARCHAR(50));";
						command.ExecuteNonQuery();
						transaction.Commit();
					}
					catch (FbException)
					{
					}
				}
			}
		}

		[Test]
		public void DisposeTest()
		{
			var tables = Connection.GetSchema("Tables", new string[] { null, null, null, null });

			var selectSql = "SELECT * FROM TEST";

			var c1 = new FbCommand(selectSql, Connection);
			IDataReader r = c1.ExecuteReader();

			while (r.Read())
			{
			}
		}

		[Test]
		public void ReturningClauseParameterTest()
		{
			const int ColumnValue = 1234;
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = string.Format("update TEST set int_field = '{0}' where int_field = 1 returning int_field", ColumnValue);
				cmd.Parameters.Add(new FbParameter() { Direction = ParameterDirection.Output });
				cmd.ExecuteNonQuery();
				var returningValue = cmd.Parameters[0].Value;
				Assert.AreEqual(ColumnValue, returningValue);
			}
		}

		[Test]
		public void ReturningClauseScalarTest()
		{
			const int ColumnValue = 1234;
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = string.Format("update TEST set int_field = '{0}' where int_field = 1 returning int_field", ColumnValue);
				cmd.Parameters.Add(new FbParameter() { Direction = ParameterDirection.Output });
				var returningValue = (int)cmd.ExecuteScalar();
				Assert.AreEqual(ColumnValue, returningValue);
			}
		}

		[Test]
		public void ReturningClauseReaderTest()
		{
			const int ColumnValue = 1234;
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = string.Format("update TEST set int_field = '{0}' where int_field = 1 returning int_field", ColumnValue);
				cmd.Parameters.Add(new FbParameter() { Direction = ParameterDirection.Output });
				using (var reader = cmd.ExecuteReader())
				{
					Assert.IsTrue(reader.Read());
					var returningValue = (int)reader[0];
					Assert.AreEqual(ColumnValue, returningValue);
				}
			}
		}

		[Test]
		public void ReadingVarcharOctetsTest()
		{
			using (var cmd = Connection.CreateCommand())
			{
				const string data = "1234";
				byte[] read = null;

				cmd.CommandText = string.Format("select cast('{0}' as varchar(10) character set octets) from rdb$database", data);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						read = (byte[])reader[0];
					}
				}

				var expected = Encoding.ASCII.GetBytes(data);
				Assert.AreEqual(expected, read);
			}
		}

		[Test]
		public void ReadingCharOctetsTest()
		{
			using (var cmd = Connection.CreateCommand())
			{
				const string data = "1234";
				byte[] read = null;

				cmd.CommandText = string.Format("select cast('{0}' as char(10) character set octets) from rdb$database", data);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
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
		public void CommandCancellationTest()
		{
			if (!EnsureVersion(new Version("2.5.0.0")))
				return;

			var cancelled = false;

			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText =
@"execute block as
declare variable start_time timestamp;
begin
  start_time = cast('now' as timestamp);
  while (datediff(second from start_time to cast('now' as timestamp)) <= 10) do
  begin
  end
end";
				ThreadPool.QueueUserWorkItem(_ =>
				{
					try
					{
						cmd.ExecuteNonQuery();
					}
					catch (FbException ex)
					{
						cancelled = "HY008" == ex.SQLSTATE;
					}
				});
				Thread.Sleep(2000);
				cmd.Cancel();
				Thread.Sleep(2000);
				Assert.IsTrue(cancelled);
			}
		}

		[Test]
		public void NoCommandPlanTest()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "recreate table NoCommandPlanTest (id int)";
				cmd.Prepare();
				var plan = default(string);
				Assert.DoesNotThrow(() => { plan = cmd.CommandPlan; });
				Assert.IsEmpty(plan);
			}
		}

		[Test]
		public void ReadsTimeWithProperPrecision()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast('00:00:01.4321' as time) from rdb$database";
				var result = (TimeSpan)cmd.ExecuteScalar();
				Assert.AreEqual(TimeSpan.FromTicks(14321000), result);
			}
		}

		[Test]
		public void PassesTimeSpanWithProperPrecision()
		{
			var ts = TimeSpan.FromTicks(14321000);
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast(@value as time) from rdb$database";
				cmd.Parameters.Add("value", ts);
				var result = (TimeSpan)cmd.ExecuteScalar();
				Assert.AreEqual(ts, result);
			}
		}

		[Test]
		public void ReadsDateTimeWithProperPrecision()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast('1.2.2015 05:06:01.4321' as timestamp) from rdb$database";
				var result = (DateTime)cmd.ExecuteScalar();
				Assert.AreEqual(new DateTime(635583639614321000), result);
			}
		}

		[Test]
		public void PassesDateTimeWithProperPrecision()
		{
			var dt = new DateTime(635583639614321000);
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast(@value as timestamp) from rdb$database";
				cmd.Parameters.Add("value", dt);
				var result = (DateTime)cmd.ExecuteScalar();
				Assert.AreEqual(dt, result);
			}
		}

		#endregion
	}
}
