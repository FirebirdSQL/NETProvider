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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Default))]
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Embedded))]
	public class FbConnectionTests : FbTestsBase
	{
		public FbConnectionTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
			: base(serverType, compression, wireCrypt)
		{ }

		[Test]
		public Task BeginTransactionILUnspecifiedTest() => BeginTransactionILTestsHelper(IsolationLevel.Unspecified);

		[Test]
		public Task BeginTransactionILReadCommittedTest() => BeginTransactionILTestsHelper(IsolationLevel.ReadCommitted);

		[Test]
		public Task BeginTransactionILReadUncommittedTest() => BeginTransactionILTestsHelper(IsolationLevel.ReadUncommitted);

		[Test]
		public Task BeginTransactionILRepeatableReadTest() => BeginTransactionILTestsHelper(IsolationLevel.RepeatableRead);

		[Test]
		public Task BeginTransactionILSerializableTest() => BeginTransactionILTestsHelper(IsolationLevel.Serializable);

		[Test]
		public async Task BeginTransactionNoWaitTimeoutTest()
		{
			await using (var conn = new FbConnection(BuildConnectionString(ServerType, Compression, WireCrypt)))
			{
				await conn.OpenAsync();
				await using (var tx = await conn.BeginTransactionAsync(new FbTransactionOptions() { WaitTimeout = null }))
				{
					Assert.NotNull(tx);
					await tx.RollbackAsync();
				}
			}
		}

		[Test]
		public async Task BeginTransactionWithWaitTimeoutTest()
		{
			await using (var conn = new FbConnection(BuildConnectionString(ServerType, Compression, WireCrypt)))
			{
				await conn.OpenAsync();
				await using (var tx = await conn.BeginTransactionAsync(new FbTransactionOptions() { WaitTimeout = TimeSpan.FromSeconds(10) }))
				{
					Assert.NotNull(tx);
					await tx.RollbackAsync();
				}
			}
		}

		[Test]
		public async Task BeginTransactionWithWaitTimeoutInvalidValue1Test()
		{
			await using (var conn = new FbConnection(BuildConnectionString(ServerType, Compression, WireCrypt)))
			{
				await conn.OpenAsync();
				Assert.ThrowsAsync<ArgumentException>(() => conn.BeginTransactionAsync(new FbTransactionOptions() { WaitTimeout = TimeSpan.FromDays(9999) }));
			}
		}

		[Test]
		public async Task BeginTransactionWithWaitTimeoutInvalidValue2Test()
		{
			await using (var conn = new FbConnection(BuildConnectionString(ServerType, Compression, WireCrypt)))
			{
				await conn.OpenAsync();
				Assert.ThrowsAsync<ArgumentException>(() => conn.BeginTransactionAsync(new FbTransactionOptions() { WaitTimeout = TimeSpan.FromMilliseconds(1) }));
			}
		}

		[Test]
		public async Task CreateCommandTest()
		{
			await using (var command = Connection.CreateCommand())
			{
				Assert.AreEqual(command.Connection, Connection);
			}
		}

		[Test]
		public async Task ConnectionPoolingOnTest()
		{
			FbConnection.ClearAllPools();
			var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
			csb.Pooling = true;
			csb.ConnectionLifeTime = 5;
			var cs = csb.ToString();

			var active = await GetActiveConnections();

			await using (FbConnection
				myConnection1 = new FbConnection(cs),
				myConnection2 = new FbConnection(cs))
			{
				await myConnection1.OpenAsync();
				await myConnection2.OpenAsync();

				Assert.AreEqual(active + 2, await GetActiveConnections());
			}

			Assert.AreEqual(active + 2, await GetActiveConnections());
		}

		[Test]
		public async Task ConnectionPoolingOffTest()
		{
			FbConnection.ClearAllPools();
			var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
			csb.Pooling = false;
			csb.ConnectionLifeTime = 5;
			var cs = csb.ToString();

			var active = await GetActiveConnections();

			await using (FbConnection
				myConnection1 = new FbConnection(cs),
				myConnection2 = new FbConnection(cs))
			{
				await myConnection1.OpenAsync();
				await myConnection2.OpenAsync();

				Assert.AreEqual(active + 2, await GetActiveConnections());
			}

			Assert.AreEqual(active, await GetActiveConnections());
		}

		[Test]
		public async Task ConnectionPoolingLifetimeTest()
		{
			FbConnection.ClearAllPools();
			var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
			csb.Pooling = true;
			csb.ConnectionLifeTime = 5;
			var cs = csb.ToString();

			var active = await GetActiveConnections();

			await using (FbConnection
				myConnection1 = new FbConnection(cs),
				myConnection2 = new FbConnection(cs))
			{
				await myConnection1.OpenAsync();
				await myConnection2.OpenAsync();

				Assert.AreEqual(active + 2, await GetActiveConnections());
			}

			Thread.Sleep(TimeSpan.FromSeconds(csb.ConnectionLifeTime * 2));
			Assert.AreEqual(active, await GetActiveConnections());
		}

		[Test]
		public async Task ConnectionPoolingMaxPoolSizeTest()
		{
			FbConnection.ClearAllPools();
			var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
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
						Assert.ThrowsAsync<InvalidOperationException>(() => connection.OpenAsync());
					}
					else
					{
						Assert.DoesNotThrowAsync(() => connection.OpenAsync());
					}
				}
			}
			finally
			{
				foreach (var c in connections)
					await c.DisposeAsync();
			}
		}

		[Test]
		public async Task ConnectionPoolingMinPoolSizeTest()
		{
			var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
			csb.Pooling = true;
			csb.ConnectionLifeTime = 5;
			csb.MinPoolSize = 3;
			var cs = csb.ToString();

			var active = await GetActiveConnections();

			var connections = new List<FbConnection>();
			try
			{
				for (var i = 0; i < csb.MinPoolSize * 2; i++)
				{
					var connection = new FbConnection(cs);
					connections.Add(connection);
					Assert.DoesNotThrowAsync(() => connection.OpenAsync());
				}
			}
			finally
			{
				foreach (var c in connections)
					await c.DisposeAsync();
			}

			Thread.Sleep(TimeSpan.FromSeconds(csb.ConnectionLifeTime * 2));
			Assert.AreEqual(active + csb.MinPoolSize, await GetActiveConnections());
		}

		[Test]
		public async Task ConnectionPoolingFailedNewConnectionIsNotBlockingPool()
		{
			const int Size = 2;

			var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
			csb.Pooling = true;
			csb.ConnectionLifeTime = 100;
			csb.MaxPoolSize = Size;
			csb.Database = "invalid";
			var cs = csb.ToString();

			var retries = 0;
			while (true)
			{
				await using (var connection = new FbConnection(cs))
				{
					try
					{
						await connection.OpenAsync();
					}
					catch (FbException)
					{
						if (retries++ >= Size)
						{
							Assert.Pass();
							return;
						}
						else
						{
							continue;
						}
					}
					Assert.Fail();
				}
			}
		}

		[Test]
		public void NoDatabaseTriggersWrongConnectionStringTest()
		{
			var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
			csb.Pooling = true;
			csb.NoDatabaseTriggers = true;
			Assert.Throws<ArgumentException>(() => new FbConnection(csb.ToString()));
		}

		[Test]
		public async Task DatabaseTriggersTest()
		{
			var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
			csb.Pooling = false;

			int rows;

			csb.NoDatabaseTriggers = false;
			await using (var conn = new FbConnection(csb.ToString()))
			{
				await conn.OpenAsync();
				rows = await GetLogRowsCount(conn);
			}

			csb.NoDatabaseTriggers = true;
			await using (var conn = new FbConnection(csb.ToString()))
			{
				await conn.OpenAsync();
				Assert.AreEqual(rows, await GetLogRowsCount(conn));
			}

			csb.NoDatabaseTriggers = false;
			await using (var conn = new FbConnection(csb.ToString()))
			{
				await conn.OpenAsync();
				Assert.AreEqual(rows + 1, await GetLogRowsCount(conn));
			}
		}

		[Test]
		public async Task UserIDCorrectlyPassedToServer()
		{
			await using (var conn = new FbConnection(BuildConnectionString(ServerType, Compression, WireCrypt)))
			{
				await conn.OpenAsync();
				await using (var command = conn.CreateCommand())
				{
					command.CommandText = "select CURRENT_USER from RDB$DATABASE";
					var loggedUser = (string)await command.ExecuteScalarAsync();
					Assert.AreEqual(FbTestsSetup.UserID, loggedUser);
				}
			}

		}

		[Test]
		public async Task UseTrustedAuth()
		{
			if (!EnsureWireCrypt(FbWireCrypt.Disabled))
				return;
			if (!EnsureServerType(FbServerType.Default))
				return;

			var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
			csb.UserID = string.Empty;
			csb.Password = string.Empty;
			await using (var conn = new FbConnection(csb.ToString()))
			{
				Assert.DoesNotThrowAsync(conn.OpenAsync);
			}
		}

		[Test]
		public async Task CreateDropDatabaseUsingTrustedAuth()
		{
			if (!EnsureWireCrypt(FbWireCrypt.Disabled))
				return;
			if (!EnsureServerType(FbServerType.Default))
				return;

			if (ServerVersion >= new Version(3, 0, 0, 0))
			{
				await using (var cmd = Connection.CreateCommand())
				{
					cmd.CommandText = "create or alter global mapping admin_trusted_auth using plugin win_sspi from any user to role rdb$admin";
					await cmd.ExecuteNonQueryAsync();
				}
			}
			try
			{
				var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
				csb.UserID = string.Empty;
				csb.Password = string.Empty;
				csb.Database = $"{Guid.NewGuid()}.fdb";
				var cs = csb.ToString();
				Assert.DoesNotThrowAsync(() => FbConnection.CreateDatabaseAsync(cs, overwrite: true));
				Assert.DoesNotThrowAsync(() => FbConnection.DropDatabaseAsync(cs));
			}
			finally
			{
				if (ServerVersion >= new Version(3, 0, 0, 0))
				{
					await using (var cmd = Connection.CreateCommand())
					{
						cmd.CommandText = "drop global mapping admin_trusted_auth";
						await cmd.ExecuteNonQueryAsync();
					}
				}
			}
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task UseCompression(bool compression)
		{
			if (!EnsureServerVersion(new Version(3, 0, 0, 0)))
				return;
			if (!EnsureServerType(FbServerType.Default))
				return;

			var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
			csb.Compression = compression;
			await using (var conn = new FbConnection(csb.ToString()))
			{
				await conn.OpenAsync();
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
		public async Task UseWireCrypt(FbWireCrypt wireCrypt)
		{
			if (!EnsureServerVersion(new Version(3, 0, 0, 0)))
				return;
			if (!EnsureServerType(FbServerType.Default))
				return;

			var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
			csb.WireCrypt = wireCrypt;
			await using (var conn = new FbConnection(csb.ToString()))
			{
				await conn.OpenAsync();
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
			var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
			csb.Database = "enc.fdb";
			async Task Test()
			{
				await using (var conn = new FbConnection(csb.ToString()))
				{
					await conn.OpenAsync();
				}
			}
			Assert.ThrowsAsync<FbException>(Test);
			csb.CryptKey = Encoding.ASCII.GetBytes("1234567890123456");
			Assert.DoesNotThrowAsync(Test);
		}

		[Test, Explicit]
		public async Task DoNotGoBackToPoolAfterBroken()
		{
			var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
			csb.Pooling = true;
			await using (var conn = new FbConnection(csb.ToString()))
			{
				await conn.OpenAsync();
			}
			await using (var conn = new FbConnection(csb.ToString()))
			{
				await conn.OpenAsync();
				try
				{
					await using (var cmd = conn.CreateCommand())
					{
						cmd.CommandText = "select * from mon$statements union all select * from mon$statements";
						await using (var reader = await cmd.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{ }
						}
					}
				}
				catch (FbException)
				{ }
			}
		}

		[Test]
		public async Task CaseSensitiveLogin()
		{
			if (!EnsureServerVersion(new Version(3, 0, 0, 0)))
				return;

			var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
			await using (var conn = new FbConnection(csb.ToString()))
			{
				await conn.OpenAsync();
				await using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = "create or alter user \"CaseSensitive\" password 'password' using plugin Srp";
					await cmd.ExecuteNonQueryAsync();
				}

				csb.Pooling = false;
				csb.UserID = "\"CaseSensitive\"";
				csb.Password = "password";
				try
				{
					await using (var conn2 = new FbConnection(csb.ToString()))
					{
						Assert.DoesNotThrowAsync(() => conn2.OpenAsync());
					}
				}
				finally
				{
					await using (var cmd = conn.CreateCommand())
					{
						cmd.CommandText = "drop user \"CaseSensitive\" using plugin Srp";
						await cmd.ExecuteNonQueryAsync();
					}
				}
			}
		}

		[Test]
		public async Task InvalidCredentialsGiveProperError()
		{
			if (!EnsureServerType(FbServerType.Default))
				return;

			var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
			csb.Password = string.Empty;
			await using (var conn = new FbConnection(csb.ToString()))
			{
				try
				{
					await conn.OpenAsync();
					Assert.Fail();
				}
				catch (FbException ex) when (ex.ErrorCode == 335544472)
				{
					Assert.Pass();
				}
			}
		}

		[Test]
		public async Task InfoMessageTest()
		{
			var messageReceived = false;
			await using (var conn = new FbConnection(BuildConnectionString(ServerType, Compression, WireCrypt)))
			{
				conn.InfoMessage += (object sender, FbInfoMessageEventArgs e) =>
				{
					messageReceived = e.Message.Contains("jiri", StringComparison.OrdinalIgnoreCase);
				};
				await conn.OpenAsync();
				await using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = "revoke select on table test from jiri";
					await cmd.ExecuteNonQueryAsync();
				}
			}
			Assert.IsTrue(messageReceived);
		}

		[Test]
		public async Task ConnectionTimeoutUsingTimeout()
		{
			if (!EnsureServerType(FbServerType.Default))
				return;

			var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
			csb.ConnectionTimeout = 1;
			csb.DataSource = "10.0.0.0"; // intentionally wrong address
			await using (var conn = new FbConnection(csb.ToString()))
			{
				try
				{
					await conn.OpenAsync();
					Assert.Fail();
				}
				catch (TimeoutException)
				{
					Assert.Pass();
				}
			}
		}

		[Test]
		public async Task ConnectionTimeoutUsingCancellationToken()
		{
			if (!EnsureServerType(FbServerType.Default))
				return;

			var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
			csb.DataSource = "10.0.0.0"; // intentionally wrong address
			await using (var conn = new FbConnection(csb.ToString()))
			{
				try
				{
					using (var cts = new CancellationTokenSource(100))
					{
						await conn.OpenAsync(cts.Token);
					}
					Assert.Fail();
				}
				catch (OperationCanceledException)
				{
					Assert.Pass();
				}
			}
		}

		[Test]
		public async Task SrpWithLeadingZeros()
		{
			if (!EnsureServerVersion(new Version(3, 0, 0, 0)))
				return;

			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "create user DAVIDS password 'test' using plugin Srp";
				await cmd.ExecuteNonQueryAsync();
			}
			try
			{
				var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
				csb.UserID = "DAVIDS";
				csb.Password = "test";
				await using (var conn = new FbConnection(csb.ToString()))
				{
					Assert.DoesNotThrowAsync(() => conn.OpenAsync());
				}
			}
			finally
			{
				await using (var cmd = Connection.CreateCommand())
				{
					cmd.CommandText = "drop user DAVIDS using plugin Srp";
					await cmd.ExecuteNonQueryAsync();
				}
			}
		}

		[Test]
		public async Task ApplicationNameCorrectlyPassedToServer()
		{
			var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
			csb.ApplicationName = "aPP NaME";
			await using (var conn = new FbConnection(csb.ToString()))
			{
				await conn.OpenAsync();
				await using (var command = conn.CreateCommand())
				{
					command.CommandText = "select MON$REMOTE_PROCESS from MON$ATTACHMENTS";
					var applicationName = (string)await command.ExecuteScalarAsync();
					Assert.AreEqual(csb.ApplicationName, applicationName);
				}
			}

		}

		private async Task BeginTransactionILTestsHelper(IsolationLevel level)
		{
			await using (var conn = new FbConnection(BuildConnectionString(ServerType, Compression, WireCrypt)))
			{
				await conn.OpenAsync();
				await using (var tx = await conn.BeginTransactionAsync(level))
				{
					Assert.NotNull(tx);
					await tx.RollbackAsync();
				}
			}
		}

		private static async Task<int> GetLogRowsCount(FbConnection conn)
		{
			await using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = "select count(*) from log where text = 'on connect'";
				return Convert.ToInt32(await cmd.ExecuteScalarAsync());
			}
		}
	}
}
