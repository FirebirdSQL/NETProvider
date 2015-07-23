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
 *  Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *  Copyright (c) 2014-2015 Jiri Cincura (jiri@cincura.net)
 *  All Rights Reserved.
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Reflection;
using FirebirdSql.Data.FirebirdClient;
using NUnit.Framework;

namespace FirebirdSql.Data.UnitTests
{
	[TestFixture]
	public class FbConnectionTests : TestsBase
	{
		#region Constructors

		public FbConnectionTests()
			: base(false)
		{
		}

		#endregion

		#region Unit Tests

		[Test]
		public void BeginTransactionILUnspecifiedTest()
		{
			BeginTransactionILTestsHelper(IsolationLevel.Unspecified);
		}

		[Test]
		public void BeginTransactionILReadCommittedTest()
		{
			BeginTransactionILTestsHelper(IsolationLevel.ReadCommitted);
		}

		[Test]
		public void BeginTransactionILReadUncommittedTest()
		{
			BeginTransactionILTestsHelper(IsolationLevel.ReadUncommitted);
		}

		[Test]
		public void BeginTransactionILRepeatableReadTest()
		{
			BeginTransactionILTestsHelper(IsolationLevel.RepeatableRead);
		}

		[Test]
		public void BeginTransactionILSerializableTest()
		{
			BeginTransactionILTestsHelper(IsolationLevel.Serializable);
		}

		[Test]
		public void BeginTransactionNoWaitTimeoutTest()
		{
			using (FbConnection conn = new FbConnection(BuildConnectionString()))
			{
				conn.Open();
				FbTransaction tx = conn.BeginTransaction(new FbTransactionOptions() { WaitTimeout = null });
				Assert.NotNull(tx);
				tx.Rollback();
			}
		}

		[Test]
		public void BeginTransactionWithWaitTimeoutTest()
		{
			using (FbConnection conn = new FbConnection(BuildConnectionString()))
			{
				conn.Open();
				FbTransaction tx = conn.BeginTransaction(new FbTransactionOptions() { WaitTimeout = TimeSpan.FromSeconds(10) });
				Assert.NotNull(tx);
				tx.Rollback();
			}
		}

		[Test]
		public void BeginTransactionWithWaitTimeoutInvalidValue1Test()
		{
			using (FbConnection conn = new FbConnection(BuildConnectionString()))
			{
				conn.Open();
				Assert.Throws<ArgumentException>(() => conn.BeginTransaction(new FbTransactionOptions() { WaitTimeout = TimeSpan.FromDays(9999) }));
			}
		}

		[Test]
		public void BeginTransactionWithWaitTimeoutInvalidValue2Test()
		{
			using (FbConnection conn = new FbConnection(BuildConnectionString()))
			{
				conn.Open();
				Assert.Throws<ArgumentException>(() => conn.BeginTransaction(new FbTransactionOptions() { WaitTimeout = TimeSpan.FromMilliseconds(1) }));
			}
		}

		[Test]
		public void CreateCommandTest()
		{
			FbCommand command = Connection.CreateCommand();

			Assert.AreEqual(command.Connection, Connection);
		}

		[Test]
		public void FbConnectionStringBuilderTest()
		{
			FbConnectionStringBuilder cs = new FbConnectionStringBuilder();

			cs.DataSource = ConfigurationManager.AppSettings["DataSource"];
			cs.Database = ConfigurationManager.AppSettings["Database"];
			cs.Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
			cs.UserID = ConfigurationManager.AppSettings["User"];
			cs.Password = ConfigurationManager.AppSettings["Password"];
			cs.ServerType = (FbServerType)Convert.ToInt32(ConfigurationManager.AppSettings["ServerType"]);
			cs.Charset = ConfigurationManager.AppSettings["Charset"];
			cs.Pooling = Convert.ToBoolean(ConfigurationManager.AppSettings["Pooling"]);

			using (FbConnection c = new FbConnection(cs.ToString()))
			{
				c.Open();
			}
		}

		[Test]
		public void ConnectionPoolingTest()
		{
			FbConnectionStringBuilder csb = BuildConnectionStringBuilder();
			csb.Pooling = true;
			csb.ConnectionLifeTime = 5;
			string cs = csb.ToString();

			FbConnection myConnection1 = new FbConnection(cs);
			FbConnection myConnection2 = new FbConnection(cs);

			int active = ActiveConnections();

			Console.WriteLine("Open two connections.");
			myConnection1.Open();
			myConnection2.Open();

			Console.WriteLine("Return both of the connections to the pool.");
			myConnection1.Close();
			myConnection2.Close();

			Assert.AreEqual(active + 2, ActiveConnections());
		}

		[Test]
		public void ConnectionPoolingTimeOutTest()
		{
			FbConnectionStringBuilder csb = BuildConnectionStringBuilder();
			csb.Pooling = true;
			csb.ConnectionLifeTime = 5;
			string cs = csb.ToString();

			int active = ActiveConnections();

			using (FbConnection
				myConnection1 = new FbConnection(cs),
				myConnection2 = new FbConnection(cs))
			{
				myConnection1.Open();
				myConnection2.Open();

				Assert.AreEqual(active + 2, ActiveConnections());

				myConnection1.Close();
				myConnection2.Close();
			}

			System.Threading.Thread.Sleep(csb.ConnectionLifeTime * 2 * 1000);

			Assert.AreEqual(active, ActiveConnections());
		}

		[Test]
		public void ConnectionPoolingMaxPoolSizeTest()
		{
			FbConnectionStringBuilder csb = BuildConnectionStringBuilder();
			csb.Pooling = true;
			csb.ConnectionLifeTime = 120;
			csb.MaxPoolSize = 10;
			string cs = csb.ToString();

			var connections = new List<FbConnection>();
			var thrown = false;
			try
			{
				for (int i = 0; i <= csb.MaxPoolSize; i++)
				{
					var connection = new FbConnection(cs);
					if (i == csb.MaxPoolSize)
					{
						try
						{
							connection.Open();
						}
						catch (InvalidOperationException)
						{
							thrown = true;
						}
					}
					else
					{
						Assert.DoesNotThrow(() => connection.Open());
					}
					connections.Add(connection);
				}
			}
			finally
			{
				connections.ForEach(x => x.Dispose());
			}

			Assert.IsTrue(thrown);
		}

		[Test]
		public void ConnectionPoolingMinPoolSizeTest()
		{
			FbConnectionStringBuilder csb = BuildConnectionStringBuilder();
			csb.Pooling = true;
			csb.ConnectionLifeTime = 5;
			csb.MinPoolSize = 3;
			string cs = csb.ToString();

			int active = ActiveConnections();

			var connections = new List<FbConnection>();
			try
			{
				for (int i = 0; i < csb.MinPoolSize * 2; i++)
				{
					var connection = new FbConnection(cs);
					Assert.DoesNotThrow(() => connection.Open());
					connections.Add(connection);
				}
			}
			finally
			{
				connections.ForEach(x => x.Dispose());
			}

			System.Threading.Thread.Sleep(csb.ConnectionLifeTime * 2 * 1000);

			Assert.AreEqual(active + csb.MinPoolSize, ActiveConnections());
		}

		[Test]
		public void NoDatabaseTriggersWrongConnectionStringTest()
		{
			FbConnectionStringBuilder csb = BuildConnectionStringBuilder();
			csb.Pooling = true;
			csb.NoDatabaseTriggers = true;
			Assert.Throws<ArgumentException>(() => new FbConnection(csb.ToString()));
		}

		[Test]
		public void DatabaseTriggersTest()
		{
			FbConnectionStringBuilder csb = BuildConnectionStringBuilder();
			csb.Pooling = false;

			int rows;

			csb.NoDatabaseTriggers = false;
			using (var conn = new FbConnection(csb.ToString()))
			{
				conn.Open();
				rows = LogRowsCount(conn);
				Console.WriteLine(rows);
			}

			csb.NoDatabaseTriggers = true;
			using (var conn = new FbConnection(csb.ToString()))
			{
				conn.Open();
				Assert.AreEqual(rows, LogRowsCount(conn));
			}

			csb.NoDatabaseTriggers = false;
			using (var conn = new FbConnection(csb.ToString()))
			{
				conn.Open();
				Assert.AreEqual(rows + 1, LogRowsCount(conn));
			}
		}

		#endregion

		#region Methods

		public FbTransaction BeginTransaction(IsolationLevel level)
		{
			switch (level)
			{
				case IsolationLevel.Unspecified:
					return Connection.BeginTransaction();

				default:
					return Connection.BeginTransaction(level);
			}
		}

		public static int ActiveConnections()
		{
			using (FbConnection conn = new FbConnection(BuildConnectionString()))
			{
				conn.Open();
				using (FbCommand cmd = conn.CreateCommand())
				{
					cmd.CommandText = "select count(*) from mon$attachments where mon$attachment_id <> current_connection";
					return (int)cmd.ExecuteScalar();
				}
			}
		}

		private void BeginTransactionILTestsHelper(IsolationLevel level)
		{
			using (FbConnection conn = new FbConnection(BuildConnectionString()))
			{
				conn.Open();
				FbTransaction tx = conn.BeginTransaction(level);
				Assert.NotNull(tx);
				tx.Rollback();
			}
		}

		private int LogRowsCount(FbConnection conn)
		{
			using (FbCommand cmd = conn.CreateCommand())
			{
				cmd.CommandText = "select count(*) from log where text = 'on connect'";
				return (int)cmd.ExecuteScalar();
			}
		}

		#endregion

		#region Event Handlers

		public void OnStateChange(object sender, StateChangeEventArgs e)
		{
			Console.WriteLine("OnStateChange");
			Console.WriteLine("  event args: (" +
				   "originalState=" + e.OriginalState +
				   " currentState=" + e.CurrentState + ")");
		}

		#endregion
	}
}
