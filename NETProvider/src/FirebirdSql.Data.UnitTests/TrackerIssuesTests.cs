/*
 *  Firebird ADO.NET Data provider for .NET and Mono
 *
 *     The contents of this file are subject to the Initial
 *     Developer's Public License Version 1.0 (the "License");
 *     you may not use this file except in compliance with the
 *     License. You may obtain a copy of the License at
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *     express or implied.  See the License for the specific
 *     language governing rights and limitations under the License.
 *
 *  Copyright (c) 2006 Carlos Guzman Alvarez
 *  Copyright (c) 2014-2015 Jiri Cincura (jiri@cincura.net)
 *  All Rights Reserved.
 */

using System;
using System.Data;
using System.Configuration;
using System.Collections.Specialized;
using System.Text;

using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Isql;
using NUnit.Framework;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

namespace FirebirdSql.Data.UnitTests
{
	[TestFixture]
	public class TrackerIssuesTests : TestsBase
	{
		#region Constructors

		public TrackerIssuesTests()
		{
		}

		#endregion

		#region Unit Tests

		[Test]
		public void DNET60()
		{
			using (FbCommand command = Connection.CreateCommand())
			{
				command.CommandText = "select ' ' AS EmptyColumn from rdb$database";

				using (FbDataReader r = command.ExecuteReader())
				{
					while (r.Read())
					{
					}
				}
			}
		}

		[Test]
		public void DNET183()
		{
			const string value = "foo  ";

			using (FbCommand cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast(@foo as varchar(5)) from rdb$database";
				cmd.Parameters.Add(new FbParameter() { ParameterName = "@foo", FbDbType = FbDbType.VarChar, Size = 5, Value = value });
				using (FbDataReader reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						Assert.AreEqual(value, (string)reader[0]);
					}
				}
			}
		}

		[Test]
		public void DNET217()
		{
			StringBuilder cols = new StringBuilder();
			string separator = string.Empty;
			for (int i = 0; i < 1235; i++)
			{
				if (i % 2 == 0)
					cols.AppendFormat("{0}'r' as col{1}", separator, i);
				else
					cols.AppendFormat("{0}24 as col{1}", separator, i);

				separator = ",";
			}
			using (FbCommand cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select " + cols.ToString() + " from rdb$database where 'x' = @x or 'x' = @x and current_timestamp = @y and current_timestamp = @y and current_timestamp = @y and current_timestamp = @y and current_timestamp = @y and current_timestamp = @y and current_timestamp = @y and current_timestamp = @y and current_timestamp = @y and current_timestamp = @y and current_timestamp = @y";
				cmd.Parameters.Add(new FbParameter() { ParameterName = "@x", Value = "z" });
				cmd.Parameters.Add(new FbParameter() { ParameterName = "@y", Value = DateTime.Now });
				using (FbDataReader reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{ }
				}
			}
		}

		[Test]
		public void DNET260()
		{
			using (FbCommand cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
RECREATE PROCEDURE TEST_SP (
  P01 SMALLINT,
  P02 INTEGER,
  P03 INTEGER,
  P04 FLOAT,
  P05 INTEGER,
  P06 INTEGER,
  P07 DATE,
  P08 DATE )
RETURNS (
  R01 FLOAT,
  R02 FLOAT,
  R03 FLOAT,
  R04 FLOAT,
  R05 FLOAT,
  R06 FLOAT,
  R07 FLOAT,
  R08 FLOAT,
  R09 FLOAT,
  R10 FLOAT,
  R11 FLOAT,
  R12 FLOAT,
  R13 FLOAT,
  R14 FLOAT,
  R15 FLOAT,
  R16 FLOAT,
  R17 FLOAT,
  R18 FLOAT,
  R19 FLOAT,
  R20 FLOAT,
  R21 FLOAT,
  R22 FLOAT,
  R23 FLOAT,
  R24 FLOAT,
  R25 FLOAT,
  R26 FLOAT,
  R27 FLOAT,
  R28 FLOAT,
  R29 FLOAT,
  R30 FLOAT,
  R31 FLOAT,
  R32 FLOAT,
  R33 FLOAT,
  R34 FLOAT,
  R35 FLOAT,
  R36 FLOAT,
  R37 FLOAT,
  R38 FLOAT,
  R39 FLOAT,
  R40 FLOAT,
  R41 FLOAT,
  R42 FLOAT,
  R43 FLOAT,
  R44 FLOAT,
  R45 FLOAT,
  R46 FLOAT,
  R47 FLOAT,
  R48 FLOAT,
  R49 FLOAT,
  R50 FLOAT,
  R51 FLOAT,
  R52 FLOAT,
  R53 FLOAT,
  R54 FLOAT,
  R55 FLOAT,
  R56 FLOAT,
  R57 FLOAT,
  R58 FLOAT,
  R59 FLOAT,
  R60 FLOAT,
  R61 FLOAT,
  R62 FLOAT,
  R63 FLOAT,
  R64 FLOAT,
  R65 FLOAT,
  R66 FLOAT,
  R67 FLOAT,
  R68 FLOAT,
  R69 FLOAT,
  R70 FLOAT,
  R71 FLOAT,
  R72 FLOAT,
  R73 FLOAT,
  R74 FLOAT,
  R75 FLOAT,
  R76 FLOAT,
  R77 FLOAT,
  R78 FLOAT,
  R79 FLOAT,
  R80 FLOAT,
  R81 FLOAT,
  R82 FLOAT,
  R83 FLOAT,
  R84 FLOAT,
  R85 FLOAT,
  R86 FLOAT,
  R87 FLOAT,
  R88 FLOAT,
  R89 FLOAT,
  R90 FLOAT,
  R91 FLOAT,
  R92 FLOAT,
  R93 FLOAT,
  R94 FLOAT,
  R95 FLOAT )
AS
BEGIN
  SUSPEND;
END
";
				cmd.ExecuteNonQuery();
			}

			using (FbCommand cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "TEST_SP";
				cmd.CommandType = CommandType.StoredProcedure;

				cmd.Parameters.Add(new FbParameter() { Value = 1 });
				cmd.Parameters.Add(new FbParameter() { Value = 1 });
				cmd.Parameters.Add(new FbParameter() { Value = 1 });
				cmd.Parameters.Add(new FbParameter() { Value = 1 });
				cmd.Parameters.Add(new FbParameter() { Value = 1 });
				cmd.Parameters.Add(new FbParameter() { Value = 1 });
				cmd.Parameters.Add(new FbParameter() { Value = DateTime.Today });
				cmd.Parameters.Add(new FbParameter() { Value = DateTime.Today });

				cmd.ExecuteNonQuery();
			}
		}

		[Test]
		public void DNET273()
		{
			using (FbCommand cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "insert into test (INT_FIELD, CLOB_FIELD) values (@INT_FIELD, @CLOB_FIELD)";
				cmd.Parameters.Add("@INT_FIELD", FbDbType.Integer).Value = 100;
				cmd.Parameters.Add("@CLOB_FIELD", FbDbType.Binary).Value = new byte[] { 0x00, 0x001 };
				cmd.ExecuteNonQuery();
			}
		}

		[Test]
		public void DNET274()
		{
			for (int i = 1000; i < 21000; i++)
			{
				new FbCommand() { CommandText = string.Format("insert into test (INT_FIELD) values ({0})", i), Connection = Connection }.ExecuteNonQuery();
			}
		}

		[Test]
		public void DNET595()
		{
			const int NumberOfThreads = 15;

			var threads = new List<Thread>();

			FbConnectionStringBuilder csb = BuildConnectionStringBuilder();
			csb.Pooling = true;
			csb.ConnectionLifeTime = 5;
			string cs = csb.ToString();

			for (int i = 0; i < NumberOfThreads; i++)
			{
				var t = new Thread(o =>
				{
					for (int j = 0; j < 50; j++)
					{
						GetSomething(cs);
					}
				});
				t.IsBackground = true;
				t.Start();
				threads.Add(t);
			}
			foreach (var thread in threads)
			{
				thread.Join();
			}
			Assert.AreEqual(NumberOfThreads + 1, FbConnectionTests.ActiveConnections());

			var sw = new Stopwatch();
			sw.Start();
			while (sw.Elapsed.TotalSeconds < 60)
			{
				GetSomething(cs);
			}
			Assert.LessOrEqual(FbConnectionTests.ActiveConnections(), 2 + 1);
		}

		[Test]
		public void DNET()
		{
			const string ConnectionString = @"data source=localhost;port number=20455;initial catalog=;user id=SYSDBA;password=masterkey;pooling=False;packet size=16384;character set=UTF8";
			var cs = new FbConnectionString(ConnectionString);
			Assert.AreEqual("localhost", cs.DataSource);
			Assert.AreEqual(20455, cs.Port);
			Assert.AreEqual(string.Empty, cs.Database);
			Assert.AreEqual("SYSDBA", cs.UserID);
			Assert.AreEqual("masterkey", cs.Password);
			Assert.AreEqual(false, cs.Pooling);
			Assert.AreEqual(16384, cs.PacketSize);
			Assert.AreEqual("UTF8", cs.Charset);
		}

		#endregion

		#region Methods

		private static void GetSomething(string connectionString)
		{
			using (FbConnection conn = new FbConnection(connectionString))
			{
				conn.Open();
				using (FbCommand command = new FbCommand("select current_timestamp from mon$database", conn))
				{
					FbConnectionStringBuilder csb = BuildConnectionStringBuilder();
					csb.Pooling = true;
					csb.ConnectionLifeTime = 5;
					string cs = csb.ToString(); command.ExecuteScalar();
				}
			}
		}

		#endregion
	}
}
