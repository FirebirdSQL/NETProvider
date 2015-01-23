/*
 *	Firebird ADO.NET Data provider for .NET	and	Mono 
 * 
 *	   The contents	of this	file are subject to	the	Initial	
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this	file except	in compliance with the 
 *	   License.	You	may	obtain a copy of the License at	
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software	distributed	under the License is distributed on	
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied.	See	the	License	for	the	specific 
 *	   language	governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *	All	Rights Reserved.
 *   
 *  Contributors:
 *    Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Collections;
using System.Data;
using System.Text;

using FirebirdSql.Data.FirebirdClient;
using NUnit.Framework;

namespace FirebirdSql.Data.UnitTests
{
	[TestFixture]
	public class FbCommandTests : TestsBase
	{
		#region Constructors

		public FbCommandTests()
			: base(false)
		{
		}

		#endregion

		#region Unit Tests

		[Test]
		public void ExecuteNonQueryTest()
		{
			Transaction = Connection.BeginTransaction();

			FbCommand command = Connection.CreateCommand();

			command.Transaction = Transaction;
			command.CommandText = "insert into TEST	(INT_FIELD)	values (?) ";

			command.Parameters.Add("@INT_FIELD", 100);

			int affectedRows = command.ExecuteNonQuery();

			Assert.AreEqual(affectedRows, 1);

			Transaction.Rollback();

			command.Dispose();
		}

		[Test]
		public void ExecuteReaderTest()
		{
			FbCommand command = Connection.CreateCommand();

			command.CommandText = "select *	from TEST";

			FbDataReader reader = command.ExecuteReader();
			reader.Close();

			command.Dispose();
		}

		[Test]
		public void ExecuteMultipleReaderTest()
		{
			FbCommand command1 = Connection.CreateCommand();
			FbCommand command2 = Connection.CreateCommand();

			command1.CommandText = "select * from test where int_field = 1";
			command2.CommandText = "select * from test where int_field = 2";

			FbDataReader r1 = command1.ExecuteReader();
			FbDataReader r2 = command2.ExecuteReader();

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
			FbCommand command = new FbCommand("select *	from TEST", Connection);

			FbDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
			reader.Close();

			command.Dispose();
		}

		[Test]
		public void ExecuteScalarTest()
		{
			FbCommand command = Connection.CreateCommand();

			command.CommandText = "select CHAR_FIELD from TEST where INT_FIELD = ?";
			command.Parameters.Add("@INT_FIELD", 2);

			string charFieldValue = command.ExecuteScalar().ToString();

			Console.WriteLine("Scalar value: {0}", charFieldValue);

			command.Dispose();
		}

		[Test]
		public void ExecuteScalarWithStoredProcedureTest()
		{
			FbCommand command = Connection.CreateCommand();

			command.CommandText = "SimpleSP";
			command.CommandType = CommandType.StoredProcedure;

			int result = (int)command.ExecuteScalar();

			Assert.AreEqual(1000, result);

			command.Dispose();
		}

		[Test]
		public void PrepareTest()
		{
			// Insert data using a prepared	statement
			FbCommand command = new FbCommand("insert into PrepareTest(test_field) values(@test_field);", Connection);

			command.Parameters.Add("@test_field", FbDbType.VarChar).Value = DBNull.Value;
			command.Prepare();

			for (int i = 0; i < 5; i++)
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

			// Check that data is correct
			FbCommand select = new FbCommand("select * from	PrepareTest", Connection);
			FbDataReader reader = select.ExecuteReader();
			int count = 0;
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
			FbCommand command = Connection.CreateCommand();

			command.CommandText = "select CHAR_FIELD from TEST where INT_FIELD = @int_field	or CHAR_FIELD =	@char_field";

			command.Parameters.Add("@int_field", 2);
			command.Parameters.Add("@char_field", "TWO");

			FbDataReader reader = command.ExecuteReader();

			int count = 0;

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
			string sql = "update test set char_field = 'carlos@firebird.org', bigint_field = @bigint, varchar_field	= 'carlos@ado.net' where int_field = @integer";

			FbCommand command = new FbCommand(sql, this.Connection);
			command.Parameters.Add("@bigint", FbDbType.BigInt).Value = 200;
			command.Parameters.Add("@integer", FbDbType.Integer).Value = 1;

			int recordsAffected = command.ExecuteNonQuery();

			command.Dispose();

			Assert.AreEqual(recordsAffected, 1, "Invalid number	of records affected.");
		}

		[Test]
		public void NamedParametersReuseTest()
		{
			string sql = "select * from	test where int_field >=	@lang and int_field	<= @lang";

			FbCommand command = new FbCommand(sql, this.Connection);
			command.Parameters.Add("@lang", FbDbType.Integer).Value = 10;

			FbDataReader reader = command.ExecuteReader();

			int count = 0;
			int intValue = 0;

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
			FbCommand command = new FbCommand("EXECUTE PROCEDURE GETVARCHARFIELD(?)", Connection);

			command.CommandType = CommandType.StoredProcedure;

			command.Parameters.Add("@ID", FbDbType.VarChar).Direction = ParameterDirection.Input;
			command.Parameters.Add("@VARCHAR_FIELD", FbDbType.VarChar).Direction = ParameterDirection.Output;

			command.Parameters[0].Value = 1;

			// This	will fill output parameters	values
			command.ExecuteNonQuery();

			Console.WriteLine("Output Parameters");
			Console.WriteLine(command.Parameters[1].Value);
		}

		[Test]
		public void RecordAffectedTest()
		{
			string sql = "insert into test (int_field) values (100000)";

			FbCommand command = new FbCommand(sql, this.Connection);

			FbDataReader reader = command.ExecuteReader();

			Assert.AreEqual(1, reader.RecordsAffected, "RecordsAffected value is incorrect");

			bool nextResult = true;

			while (nextResult)
			{
				while (reader.Read())
				{
				}

				nextResult = reader.NextResult();
			}

			reader.Close();

			Assert.AreEqual(1, reader.RecordsAffected, "RecordsAffected value is incorrect");
		}

		[Test]
		public void ExecuteNonQueryWithOutputParameters()
		{
			FbCommand command = new FbCommand("EXECUTE PROCEDURE GETASCIIBLOB(?)", Connection);

			command.CommandType = CommandType.StoredProcedure;

			command.Parameters.Add("@ID", FbDbType.VarChar).Direction = ParameterDirection.Input;
			command.Parameters.Add("@CLOB_FIELD", FbDbType.Text).Direction = ParameterDirection.Output;

			command.Parameters[0].Value = 1;

			// This	will fill output parameters	values
			command.ExecuteNonQuery();

			// Check that the output parameter has a correct value
			Assert.AreEqual("IRow Number 1", command.Parameters[1].Value, "Output parameter value is not valid");

			// Dispose command - this will do a	transaction	commit
			command.Dispose();
		}

		[Test]
		public void InvalidParameterFormat()
		{
			string sql = "update test set timestamp_field =	@timestamp where int_field = @integer";

			FbTransaction transaction = this.Connection.BeginTransaction();
			try
			{
				FbCommand command = new FbCommand(sql, this.Connection, transaction);
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
			string createTable = "CREATE TABLE VARCHARTEST (VARCHAR_FIELD  VARCHAR(10));";

			FbCommand ct = new FbCommand(createTable, this.Connection);
			ct.ExecuteNonQuery();
			ct.Dispose();

			ArrayList l = new ArrayList();

			l.Add("INSERT INTO VARCHARTEST (VARCHAR_FIELD) VALUES ('1');");
			l.Add("INSERT INTO VARCHARTEST (VARCHAR_FIELD) VALUES ('11');");
			l.Add("INSERT INTO VARCHARTEST (VARCHAR_FIELD) VALUES ('111');");
			l.Add("INSERT INTO VARCHARTEST (VARCHAR_FIELD) VALUES ('1111');");

			foreach (string statement in l)
			{
				FbCommand insert = new FbCommand(statement, this.Connection);
				insert.ExecuteNonQuery();
				insert.Dispose();
			}

			string sql = "select * from	varchartest";

			FbCommand cmd = new FbCommand(sql, this.Connection);
			FbDataReader r = cmd.ExecuteReader();

			while (r.Read())
			{
				Console.WriteLine("{0} :: {1}", r[0], r[0].ToString().Length);
			}

			r.Close();
		}

		[Test]
		public void SimplifiedChineseTest()
		{
			string createTable = "CREATE TABLE TABLE1 (FIELD1 varchar(20))";
			FbCommand create = new FbCommand(createTable, this.Connection);
			create.ExecuteNonQuery();
			create.Dispose();

			// insert using	parametrized SQL
			string sql = "INSERT INTO Table1 VALUES	(@value)";
			FbCommand command = new FbCommand(sql, this.Connection);
			command.Parameters.Add("@value", FbDbType.VarChar).Value = "中文";
			command.ExecuteNonQuery();
			command.Dispose();

			sql = "SELECT *	FROM TABLE1";
			FbCommand select = new FbCommand(sql, this.Connection);
			string result = select.ExecuteScalar().ToString();
			select.Dispose();

			Assert.AreEqual("中文", result, "Incorrect results in	parametrized insert");

			sql = "DELETE FROM TABLE1";
			FbCommand delete = new FbCommand(sql, this.Connection);
			delete.ExecuteNonQuery();
			delete.Dispose();

			// insert using	plain SQL
			sql = "INSERT INTO Table1 VALUES ('中文')";
			FbCommand plainCommand = new FbCommand(sql, this.Connection);
			plainCommand.ExecuteNonQuery();
			plainCommand.Dispose();

			sql = "SELECT *	FROM TABLE1";
			select = new FbCommand(sql, this.Connection);
			result = select.ExecuteScalar().ToString();
			select.Dispose();

			Assert.AreEqual("中文", result, "Incorrect results in	plain insert");
		}

		[Test]
		public void InsertDateTest()
		{
			string sql = "insert into TEST (int_field, date_field) values (1002, @date)";

			FbCommand command = new FbCommand(sql, this.Connection);

			command.Parameters.Add("@date", FbDbType.Date).Value = DateTime.Now.ToString();

			int ra = command.ExecuteNonQuery();

			Assert.AreEqual(ra, 1);
		}

		[Test]
		public void InsertNullTest()
		{
			string sql = "insert into TEST (int_field) values (@value)";

			FbCommand command = new FbCommand(sql, this.Connection);
			command.Parameters.Add("@value", FbDbType.Integer).Value = null;

			try
			{
				command.ExecuteNonQuery();

				throw new Exception("The command was executed without throw	an exception");
			}
			catch
			{
			}
		}

		[Test]
		public void InsertDateTimeTest()
		{
			DateTime value = DateTime.Now;

			using (FbCommand cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "insert into test (int_field, timestamp_field) values (1002, @dt)";
				cmd.Parameters.Add("@dt", FbDbType.TimeStamp).Value = value;

				int ra = cmd.ExecuteNonQuery();

				Assert.AreEqual(ra, 1);
			}

			using (FbCommand cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select timestamp_field from test where int_field = 1002";
				DateTime result = (DateTime)cmd.ExecuteScalar();

				Assert.AreEqual(value.ToString(), result.ToString());
			}
		}

		[Test]
		public void InsertTimeStampTest()
		{
			string value = DateTime.Now.ToString();

			using (FbCommand cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "insert into test (int_field, timestamp_field) values (1002, @ts)";
				cmd.Parameters.Add("@ts", FbDbType.TimeStamp).Value = value;

				int ra = cmd.ExecuteNonQuery();

				Assert.AreEqual(ra, 1);
			}

			using (FbCommand cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select timestamp_field from test where int_field = 1002";
				DateTime result = (DateTime)cmd.ExecuteScalar();

				Assert.AreEqual(value, result.ToString());
			}
		}

		[Test]
		public void InsertTimeTest()
		{
			TimeSpan t = new TimeSpan(0, 5, 6, 7, 231);

			using (FbCommand cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "insert into test (int_field, time_field) values (2245, @t)";
				cmd.Parameters.Add("@t", FbDbType.Time).Value = t;

				int ra = cmd.ExecuteNonQuery();

				Assert.AreEqual(ra, 1);
			}

			using (FbCommand cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select time_field from test where int_field = 2245";
				TimeSpan result = (TimeSpan)cmd.ExecuteScalar();

				Assert.AreEqual(t.Hours, result.Hours, "hours are not same");
				Assert.AreEqual(t.Minutes, result.Minutes, "minutes are not same");
				Assert.AreEqual(t.Seconds, result.Seconds, "seconds are not same");
				Assert.AreEqual(t.Milliseconds, result.Milliseconds, "milliseconds are not same");
			}
		}

		[Test]
		public void InsertTimeOldTest()
		{
			DateTime t = DateTime.Today;
			t = t.AddHours(5);
			t = t.AddMinutes(6);
			t = t.AddSeconds(7);
			t = t.AddMilliseconds(231);

			using (FbCommand cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "insert into test (int_field, time_field) values (2245, @t)";
				cmd.Parameters.Add("@t", FbDbType.Time).Value = t;

				int ra = cmd.ExecuteNonQuery();

				Assert.AreEqual(ra, 1);
			}

			using (FbCommand cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select time_field from test where int_field = 2245";
				TimeSpan result = (TimeSpan)cmd.ExecuteScalar();

				Assert.AreEqual(t.Hour, result.Hours, "hours are not same");
				Assert.AreEqual(t.Minute, result.Minutes, "minutes are not same");
				Assert.AreEqual(t.Second, result.Seconds, "seconds are not same");
				Assert.AreEqual(t.Millisecond, result.Milliseconds, "milliseconds are not same");
			}
		}

		[Test]
		public void ParameterDescribeTest()
		{
			string sql = "insert into TEST (int_field) values (@value)";

			FbCommand command = new FbCommand(sql, this.Connection);
			command.Prepare();
			command.Parameters.Add("@value", FbDbType.Integer).Value = 100000;

			command.ExecuteNonQuery();

			command.Dispose();
		}

		[Test]
		public void ReadOnlyTransactionTest()
		{
			using (IDbCommand command = this.Connection.CreateCommand())
			{
				using (IDbTransaction transaction = this.Connection.BeginTransaction(new FbTransactionOptions() { TransactionBehavior = FbTransactionBehavior.Read, WaitTimeout = null }))
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
			DataTable tables = this.Connection.GetSchema("Tables", new string[] { null, null, null, null });

			string selectSql = "SELECT * FROM TEST";

			FbCommand c1 = new FbCommand(selectSql, this.Connection);
			IDataReader r = c1.ExecuteReader();

			while (r.Read())
			{
			}
		}

		[Test]
		public void ReturningClauseTest()
		{
			using (FbCommand cmd = Connection.CreateCommand())
			{
				const string columnValue = "foobar";

				cmd.CommandText = string.Format("update rdb$database set rdb$description = '{0}' returning rdb$description", columnValue);
				cmd.Parameters.Add(new FbParameter() { Direction = ParameterDirection.Output });
				cmd.ExecuteNonQuery();
				Assert.AreEqual(columnValue, cmd.Parameters[0].Value);
			}
		}

		[Test]
		public void ReadingVarcharOctetsTest()
		{
			using (FbCommand cmd = Connection.CreateCommand())
			{
				const string data = "1234";
				byte[] read = null;

				cmd.CommandText = string.Format("select cast('{0}' as varchar(10) character set octets) from rdb$database", data);
				using (FbDataReader reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						read = (byte[])reader[0];
					}
				}

				byte[] expected = Encoding.ASCII.GetBytes(data);
				Assert.AreEqual(expected, read);
			}
		}

		[Test]
		public void ReadingCharOctetsTest()
		{
			using (FbCommand cmd = Connection.CreateCommand())
			{
				const string data = "1234";
				byte[] read = null;

				cmd.CommandText = string.Format("select cast('{0}' as char(10) character set octets) from rdb$database", data);
				using (FbDataReader reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						read = (byte[])reader[0];
					}
				}

				byte[] expected = new byte[10];
				Encoding.ASCII.GetBytes(data).CopyTo(expected, 0);
				Assert.AreEqual(expected, read);
			}
		}

		[Test]
		public void CommandCancellationTest()
		{
			if (GetServerVersion() < new Version("2.5.0.0"))
			{
				Assert.Inconclusive("Not supported on this version.");
				return;
			}

			bool cancelled = false;

			using (FbCommand cmd = Connection.CreateCommand())
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
				cmd.BeginExecuteNonQuery(o =>
				{
					try
					{
						cmd.EndExecuteNonQuery(o as IAsyncResult);
					}
					catch (FbException ex)
					{
						cancelled = "HY008" == ex.SQLSTATE;
					}
				}, null);
				System.Threading.Thread.Sleep(2000);
				cmd.Cancel();
				System.Threading.Thread.Sleep(2000);
				Assert.IsTrue(cancelled);
			}
		}

		[Test]
		public void NoCommandPlanTest()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "recreate table NoCommandPlanTest (id int)";
				cmd.ExecuteNonQuery();
				var plan = default(string);
				Assert.DoesNotThrow(() => { plan=cmd.CommandPlan; });
				Assert.IsEmpty(plan);
			}
		}

		#endregion
	}
}
