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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using FirebirdSql.Data.FirebirdClient;
using NUnit.Framework;

namespace FirebirdSql.Data.UnitTests
{
	[FbTestFixture(FbServerType.Default, false)]
	[FbTestFixture(FbServerType.Default, true)]
	[FbTestFixture(FbServerType.Embedded, default(bool))]
	public class FbConnectionTests : TestsBase
	{
		#region Constructors

		public FbConnectionTests(FbServerType serverType, bool compression)
			: base(serverType, compression)
		{ }

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
			using (FbConnection conn = new FbConnection(BuildConnectionString(FbServerType, Compression)))
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
			using (FbConnection conn = new FbConnection(BuildConnectionString(FbServerType, Compression)))
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
			using (FbConnection conn = new FbConnection(BuildConnectionString(FbServerType, Compression)))
			{
				conn.Open();
				Assert.Throws<ArgumentException>(() => conn.BeginTransaction(new FbTransactionOptions() { WaitTimeout = TimeSpan.FromDays(9999) }));
			}
		}

		[Test]
		public void BeginTransactionWithWaitTimeoutInvalidValue2Test()
		{
			using (FbConnection conn = new FbConnection(BuildConnectionString(FbServerType, Compression)))
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
		public void ConnectionPoolingOnTest()
		{
			FbConnection.ClearAllPools();
			FbConnectionStringBuilder csb = BuildConnectionStringBuilder(FbServerType, Compression);
			csb.Pooling = true;
			csb.ConnectionLifeTime = 5;
			string cs = csb.ToString();

			int active = GetActiveConnections();

			using (FbConnection
				myConnection1 = new FbConnection(cs),
				myConnection2 = new FbConnection(cs))
			{
				myConnection1.Open();
				myConnection2.Open();

				Assert.AreEqual(active + 2, GetActiveConnections());
			}

			Assert.AreEqual(active + 2, GetActiveConnections());
		}

		[Test]
		public void ConnectionPoolingOffTest()
		{
			FbConnection.ClearAllPools();
			FbConnectionStringBuilder csb = BuildConnectionStringBuilder(FbServerType, Compression);
			csb.Pooling = false;
			csb.ConnectionLifeTime = 5;
			string cs = csb.ToString();

			int active = GetActiveConnections();

			using (FbConnection
				myConnection1 = new FbConnection(cs),
				myConnection2 = new FbConnection(cs))
			{
				myConnection1.Open();
				myConnection2.Open();

				Assert.AreEqual(active + 2, GetActiveConnections());
			}

			Assert.AreEqual(active, GetActiveConnections());
		}

		[Test]
		public void ConnectionPoolingLifeTimeTest()
		{
			FbConnection.ClearAllPools();
			FbConnectionStringBuilder csb = BuildConnectionStringBuilder(FbServerType, Compression);
			csb.Pooling = true;
			csb.ConnectionLifeTime = 5;
			string cs = csb.ToString();

			int active = GetActiveConnections();

			using (FbConnection
				myConnection1 = new FbConnection(cs),
				myConnection2 = new FbConnection(cs))
			{
				myConnection1.Open();
				myConnection2.Open();

				Assert.AreEqual(active + 2, GetActiveConnections());
			}

			Thread.Sleep(TimeSpan.FromSeconds(csb.ConnectionLifeTime * 2));
			Assert.AreEqual(active, GetActiveConnections());
		}

		[Test]
		public void ConnectionPoolingMaxPoolSizeTest()
		{
			FbConnection.ClearAllPools();
			FbConnectionStringBuilder csb = BuildConnectionStringBuilder(FbServerType, Compression);
			csb.Pooling = true;
			csb.ConnectionLifeTime = 120;
			csb.MaxPoolSize = 10;
			string cs = csb.ToString();

			var connections = new List<FbConnection>();
			try
			{
				for (int i = 0; i <= csb.MaxPoolSize; i++)
				{
					var connection = new FbConnection(cs);
					connections.Add(connection);
					if (i == csb.MaxPoolSize)
					{
						Assert.Throws<InvalidOperationException>(() => connection.Open());
					}
					else
					{
						Assert.DoesNotThrow(() => connection.Open());
					}
				}
			}
			finally
			{
				connections.ForEach(x => x.Dispose());
			}
		}

		[Test]
		public void ConnectionPoolingMinPoolSizeTest()
		{
			FbConnectionStringBuilder csb = BuildConnectionStringBuilder(FbServerType, Compression);
			csb.Pooling = true;
			csb.ConnectionLifeTime = 5;
			csb.MinPoolSize = 3;
			string cs = csb.ToString();

			int active = GetActiveConnections();

			var connections = new List<FbConnection>();
			try
			{
				for (int i = 0; i < csb.MinPoolSize * 2; i++)
				{
					var connection = new FbConnection(cs);
					connections.Add(connection);
					Assert.DoesNotThrow(() => connection.Open());
				}
			}
			finally
			{
				connections.ForEach(x => x.Dispose());
			}

			Thread.Sleep(TimeSpan.FromSeconds(csb.ConnectionLifeTime * 2));
			Assert.AreEqual(active + csb.MinPoolSize, GetActiveConnections());
		}

		[Test]
		public void NoDatabaseTriggersWrongConnectionStringTest()
		{
			FbConnectionStringBuilder csb = BuildConnectionStringBuilder(FbServerType, Compression);
			csb.Pooling = true;
			csb.NoDatabaseTriggers = true;
			Assert.Throws<ArgumentException>(() => new FbConnection(csb.ToString()));
		}

		[Test]
		public void DatabaseTriggersTest()
		{
			FbConnectionStringBuilder csb = BuildConnectionStringBuilder(FbServerType, Compression);
			csb.Pooling = false;

			int rows;

			csb.NoDatabaseTriggers = false;
			using (var conn = new FbConnection(csb.ToString()))
			{
				conn.Open();
				rows = LogRowsCount(conn);
				TestContext.WriteLine(rows);
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

		[Test]
		public void UserIDCorrectlyPassedToServer()
		{
			using (var conn = new FbConnection(BuildConnectionString(FbServerType, Compression)))
			{
				conn.Open();
				using (var command = conn.CreateCommand())
				{
					command.CommandText = "select CURRENT_USER from RDB$DATABASE";
					var loggedUser = (string)command.ExecuteScalar();
					Assert.AreEqual(TestsSetup.UserID, loggedUser);
				}
			}

		}

		[Test]
		public void UseTrustedAuth()
		{
			if (!EnsureServerType(FbServerType.Default))
				return;

			var csb = BuildConnectionStringBuilder(FbServerType, Compression);
			csb.UserID = string.Empty;
			csb.Password = string.Empty;
			using (var conn = new FbConnection(csb.ToString()))
			{
				Assert.DoesNotThrow(conn.Open);
			}
		}

		[TestCase(false)]
		[TestCase(true)]
		public void UseCompression(bool compression)
		{
			if (!EnsureVersion(new Version("3.0.0.0")))
				return;
			if (!EnsureServerType(FbServerType.Default))
				return;

			var csb = BuildConnectionStringBuilder(FbServerType, Compression);
			csb.Compression = compression;
			using (var conn = new FbConnection(csb.ToString()))
			{
				conn.Open();
				TestContext.WriteLine(conn.ServerVersion);
				const string Pattern = ":[^:]*Z[^:]*$";
				if (compression)
				{
					StringAssert.IsMatch(Pattern, conn.ServerVersion);
				}
				else
				{
					StringAssert.DoesNotMatch(Pattern, conn.ServerVersion);
				}
			}
		}

		[Test, Ignore("Needs plugin")]
		public void PassCryptKey()
		{
			var csb = BuildConnectionStringBuilder(FbServerType, Compression);
			csb.CryptKey = Encoding.ASCII.GetBytes("1234567890123456");
			using (var conn = new FbConnection(csb.ToString()))
			{
				conn.Open();
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

		private void BeginTransactionILTestsHelper(IsolationLevel level)
		{
			using (FbConnection conn = new FbConnection(BuildConnectionString(FbServerType, Compression)))
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
				return Convert.ToInt32(cmd.ExecuteScalar());
			}
		}

		#endregion
	}
}
