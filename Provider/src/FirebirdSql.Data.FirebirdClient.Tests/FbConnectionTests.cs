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
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[TestFixtureSource(typeof(FbDefaultServerTypeTestFixtureSource))]
	[TestFixtureSource(typeof(FbEmbeddedServerTypeTestFixtureSource))]
	public class FbConnectionTests : FbTestsBase
	{
		#region Constructors

		public FbConnectionTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
			: base(serverType, compression, wireCrypt)
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
			using (var conn = new FbConnection(BuildConnectionString(FbServerType, Compression, WireCrypt)))
			{
				conn.Open();
				var tx = conn.BeginTransaction(new FbTransactionOptions() { WaitTimeout = null });
				Assert.NotNull(tx);
				tx.Rollback();
			}
		}

		[Test]
		public void BeginTransactionWithWaitTimeoutTest()
		{
			using (var conn = new FbConnection(BuildConnectionString(FbServerType, Compression, WireCrypt)))
			{
				conn.Open();
				var tx = conn.BeginTransaction(new FbTransactionOptions() { WaitTimeout = TimeSpan.FromSeconds(10) });
				Assert.NotNull(tx);
				tx.Rollback();
			}
		}

		[Test]
		public void BeginTransactionWithWaitTimeoutInvalidValue1Test()
		{
			using (var conn = new FbConnection(BuildConnectionString(FbServerType, Compression, WireCrypt)))
			{
				conn.Open();
				Assert.Throws<ArgumentException>(() => conn.BeginTransaction(new FbTransactionOptions() { WaitTimeout = TimeSpan.FromDays(9999) }));
			}
		}

		[Test]
		public void BeginTransactionWithWaitTimeoutInvalidValue2Test()
		{
			using (var conn = new FbConnection(BuildConnectionString(FbServerType, Compression, WireCrypt)))
			{
				conn.Open();
				Assert.Throws<ArgumentException>(() => conn.BeginTransaction(new FbTransactionOptions() { WaitTimeout = TimeSpan.FromMilliseconds(1) }));
			}
		}

		[Test]
		public void CreateCommandTest()
		{
			var command = Connection.CreateCommand();

			Assert.AreEqual(command.Connection, Connection);
		}

		[Test]
		public void ConnectionPoolingOnTest()
		{
			FbConnection.ClearAllPools();
			var csb = BuildConnectionStringBuilder(FbServerType, Compression, WireCrypt);
			csb.Pooling = true;
			csb.ConnectionLifeTime = 5;
			var cs = csb.ToString();

			var active = GetActiveConnections();

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
			var csb = BuildConnectionStringBuilder(FbServerType, Compression, WireCrypt);
			csb.Pooling = false;
			csb.ConnectionLifeTime = 5;
			var cs = csb.ToString();

			var active = GetActiveConnections();

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
		public void ConnectionPoolingLifetimeTest()
		{
			FbConnection.ClearAllPools();
			var csb = BuildConnectionStringBuilder(FbServerType, Compression, WireCrypt);
			csb.Pooling = true;
			csb.ConnectionLifeTime = 5;
			var cs = csb.ToString();

			var active = GetActiveConnections();

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
			var csb = BuildConnectionStringBuilder(FbServerType, Compression, WireCrypt);
			csb.Pooling = true;
			csb.ConnectionLifeTime = 120;
			csb.MaxPoolSize = 10;
			var cs = csb.ToString();

			var connections = new List<FbConnection>();
			try
			{
				for (var i = 0; i <= csb.MaxPoolSize; i++)
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
			var csb = BuildConnectionStringBuilder(FbServerType, Compression, WireCrypt);
			csb.Pooling = true;
			csb.ConnectionLifeTime = 5;
			csb.MinPoolSize = 3;
			var cs = csb.ToString();

			var active = GetActiveConnections();

			var connections = new List<FbConnection>();
			try
			{
				for (var i = 0; i < csb.MinPoolSize * 2; i++)
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
			var csb = BuildConnectionStringBuilder(FbServerType, Compression, WireCrypt);
			csb.Pooling = true;
			csb.NoDatabaseTriggers = true;
			Assert.Throws<ArgumentException>(() => new FbConnection(csb.ToString()));
		}

		[Test]
		public void DatabaseTriggersTest()
		{
			var csb = BuildConnectionStringBuilder(FbServerType, Compression, WireCrypt);
			csb.Pooling = false;

			int rows;

			csb.NoDatabaseTriggers = false;
			using (var conn = new FbConnection(csb.ToString()))
			{
				conn.Open();
				rows = GetLogRowsCount(conn);
			}

			csb.NoDatabaseTriggers = true;
			using (var conn = new FbConnection(csb.ToString()))
			{
				conn.Open();
				Assert.AreEqual(rows, GetLogRowsCount(conn));
			}

			csb.NoDatabaseTriggers = false;
			using (var conn = new FbConnection(csb.ToString()))
			{
				conn.Open();
				Assert.AreEqual(rows + 1, GetLogRowsCount(conn));
			}
		}

		[Test]
		public void UserIDCorrectlyPassedToServer()
		{
			using (var conn = new FbConnection(BuildConnectionString(FbServerType, Compression, WireCrypt)))
			{
				conn.Open();
				using (var command = conn.CreateCommand())
				{
					command.CommandText = "select CURRENT_USER from RDB$DATABASE";
					var loggedUser = (string)command.ExecuteScalar();
					Assert.AreEqual(FbTestsSetup.UserID, loggedUser);
				}
			}

		}

		[Test]
		public void UseTrustedAuth()
		{
			if (WireCrypt == FbWireCrypt.Required)
				return;
			if (!EnsureServerType(FbServerType.Default))
				return;

			var csb = BuildConnectionStringBuilder(FbServerType, Compression, WireCrypt);
			csb.UserID = string.Empty;
			csb.Password = string.Empty;
			using (var conn = new FbConnection(csb.ToString()))
			{
				Assert.DoesNotThrow(conn.Open);
			}
		}

		[Test]
		public void CreateDropDatabaseUsingTrustedAuth()
		{
			if (WireCrypt == FbWireCrypt.Required)
				return;
			if (!EnsureServerType(FbServerType.Default))
				return;

			if (GetServerVersion() >= new Version(3, 0, 0, 0))
			{
				using (var cmd = Connection.CreateCommand())
				{
					cmd.CommandText = "create or alter global mapping admin_trusted_auth using plugin win_sspi from any user to role rdb$admin";
					cmd.ExecuteNonQuery();
				}
			}
			try
			{
				var csb = BuildConnectionStringBuilder(FbServerType, Compression, WireCrypt);
				csb.UserID = string.Empty;
				csb.Password = string.Empty;
				csb.Database = $"{Guid.NewGuid().ToString()}.fdb";
				var cs = csb.ToString();
				Assert.DoesNotThrow(() => FbConnection.CreateDatabase(cs, overwrite: true));
				Assert.DoesNotThrow(() => FbConnection.DropDatabase(cs));
			}
			finally
			{
				if (GetServerVersion() >= new Version(3, 0, 0, 0))
				{
					using (var cmd = Connection.CreateCommand())
					{
						cmd.CommandText = "drop global mapping admin_trusted_auth";
						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		[TestCase(false)]
		[TestCase(true)]
		public void UseCompression(bool compression)
		{
			if (!EnsureVersion(new Version(3, 0, 0, 0)))
				return;
			if (!EnsureServerType(FbServerType.Default))
				return;

			var csb = BuildConnectionStringBuilder(FbServerType, Compression, WireCrypt);
			csb.Compression = compression;
			using (var conn = new FbConnection(csb.ToString()))
			{
				conn.Open();
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

		[TestCase(FbWireCrypt.Disabled)]
		[TestCase(FbWireCrypt.Enabled)]
		[TestCase(FbWireCrypt.Required)]
		public void UseWireCrypt(FbWireCrypt wireCrypt)
		{
			if (!EnsureVersion(new Version(3, 0, 0, 0)))
				return;
			if (!EnsureServerType(FbServerType.Default))
				return;

			var csb = BuildConnectionStringBuilder(FbServerType, Compression, WireCrypt);
			csb.WireCrypt = wireCrypt;
			using (var conn = new FbConnection(csb.ToString()))
			{
				conn.Open();
				const string Pattern = ":[^:]*C[^:]*$";
				if (wireCrypt == FbWireCrypt.Enabled || wireCrypt == FbWireCrypt.Required)
				{
					StringAssert.IsMatch(Pattern, conn.ServerVersion);
				}
				else
				{
					StringAssert.DoesNotMatch(Pattern, conn.ServerVersion);
				}
			}
		}

		[Test, Explicit]
		public void PassCryptKey()
		{
			var csb = BuildConnectionStringBuilder(FbServerType, Compression, WireCrypt);
			csb.Database = "enc.fdb";
			void Test()
			{
				using (var conn = new FbConnection(csb.ToString()))
				{
					conn.Open();
				}
			}
			Assert.Throws<FbException>(Test);
			csb.CryptKey = Encoding.ASCII.GetBytes("1234567890123456");
			Assert.DoesNotThrow(Test);
		}

		[Test, Explicit]
		public void DoNotGoBackToPoolAfterBroken()
		{
			var csb = BuildConnectionStringBuilder(FbServerType, Compression, WireCrypt);
			csb.Pooling = true;
			using (var conn = new FbConnection(csb.ToString()))
			{
				conn.Open();
			}
			using (var conn = new FbConnection(csb.ToString()))
			{
				conn.Open();
				try
				{
					using (var cmd = conn.CreateCommand())
					{
						cmd.CommandText = "select * from mon$statements union all select * from mon$statements";
						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{ }
						}
					}
				}
				catch (FbException)
				{ }
			}
		}

		[Test]
		public void CaseSensitiveLogin()
		{
			if (!EnsureVersion(new Version(3, 0, 0, 0)))
				return;

			var connectionString = BuildConnectionString(FbServerType, Compression, WireCrypt);
			using (var conn = new FbConnection(connectionString))
			{
				conn.Open();
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = "create or alter user \"CaseSensitive\" password 'password' using plugin Srp";
					cmd.ExecuteNonQuery();
				}

				var csBuilder = new FbConnectionStringBuilder(connectionString)
				{
					Pooling = false,
					UserID = "\"CaseSensitive\"",
					Password = "password"
				};
				try
				{
					using (var conn2 = new FbConnection(csBuilder.ToString()))
					{
						conn2.Open();
					}
				}
				finally
				{
					using (var cmd = conn.CreateCommand())
					{
						cmd.CommandText = "drop user \"CaseSensitive\" using plugin Srp";
						cmd.ExecuteNonQuery();
					}
				}
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
			using (var conn = new FbConnection(BuildConnectionString(FbServerType, Compression, WireCrypt)))
			{
				conn.Open();
				var tx = conn.BeginTransaction(level);
				Assert.NotNull(tx);
				tx.Rollback();
			}
		}

		private int GetLogRowsCount(FbConnection conn)
		{
			using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = "select count(*) from log where text = 'on connect'";
				return Convert.ToInt32(cmd.ExecuteScalar());
			}
		}

		#endregion
	}
}
