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
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Services;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests;

[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Default))]
public class FbServicesTests : FbTestsBase
{
	public FbServicesTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
		: base(serverType, compression, wireCrypt)
	{ }

	[SetUp]
	public override async Task SetUp()
	{
		await base.SetUp();

		if (Connection != null && Connection.State == ConnectionState.Open)
		{
			await Connection.CloseAsync();
		}
	}

	[Test]
	public async Task BackupRestoreTest()
	{
		var backupName = $"{Guid.NewGuid()}.bak";
		var connectionString = BuildServicesConnectionString(ServerType, Compression, WireCrypt, true);
		await BackupPartHelper(backupName, connectionString);
		await RestorePartHelper(backupName, connectionString);
		// test the database was actually restored fine
		await Connection.OpenAsync();
		await Connection.CloseAsync();
	}

	[Test]
	public async Task BackupRestoreZipTest()
	{
		if (!EnsureServerVersionAtLeast(new Version(4, 0, 0, 0)))
			return;

		var backupName = $"{Guid.NewGuid()}.bak";
		var csb = BuildServicesConnectionStringBuilder(ServerType, Compression, WireCrypt, true);
		var connectionString = csb.ToString();
		await BackupPartHelper(backupName, connectionString, x =>
		{
			x.Options |= FbBackupFlags.Zip;
		});
		await RestorePartHelper(backupName, connectionString);
		// test the database was actually restored fine
		await Connection.OpenAsync();
		await Connection.CloseAsync();
	}

	[Test]
	public async Task BackupRestoreVerbIntTest()
	{
		if (!EnsureServerVersionAtLeast(new Version(3, 0, 0, 0)))
			return;

		var backupName = $"{Guid.NewGuid()}.bak";
		var csb = BuildServicesConnectionStringBuilder(ServerType, Compression, WireCrypt, true);
		var connectionString = csb.ToString();
		await BackupPartHelper(backupName, connectionString, x =>
		{
			x.Verbose = false;
			x.VerboseInterval = 1_000_000;
		});
		await RestorePartHelper(backupName, connectionString, x =>
		{
			x.Verbose = false;
			x.VerboseInterval = 1_000_000;
		});
		// test the database was actually restored fine
		await Connection.OpenAsync();
		await Connection.CloseAsync();
	}

	[Test]
	public async Task BackupRestoreParallelTest()
	{
		if (!EnsureServerVersionAtLeast(new Version(5, 0, 0, 0)))
			return;

		var backupName = $"{Guid.NewGuid()}.bak";
		var csb = BuildServicesConnectionStringBuilder(ServerType, Compression, WireCrypt, true);
		csb.ParallelWorkers = 6;
		var connectionString = csb.ToString();
		await BackupPartHelper(backupName, connectionString);
		await RestorePartHelper(backupName, connectionString);
		// test the database was actually restored fine
		await Connection.OpenAsync();
		await Connection.CloseAsync();
	}

	[TestCase(true)]
	[TestCase(false)]
	public async Task StreamingBackupRestoreTest(bool verbose)
	{
		if (!EnsureServerVersionAtLeast(new Version(2, 5, 0, 0)))
			return;

		Task BackupPart(MemoryStream buffer)
		{
			var backupSvc = new FbStreamingBackup();
			backupSvc.ConnectionString = BuildServicesConnectionString(ServerType, Compression, WireCrypt, true);
			backupSvc.Options = FbBackupFlags.IgnoreLimbo;
			backupSvc.OutputStream = buffer;
			backupSvc.ServiceOutput += ServiceOutput;
			return backupSvc.ExecuteAsync();
		}
		Task RestorePart(MemoryStream buffer, bool verbose)
		{
			var restoreSvc = new FbStreamingRestore();
			restoreSvc.ConnectionString = BuildServicesConnectionString(ServerType, Compression, WireCrypt, true);
			restoreSvc.Options = FbRestoreFlags.Create | FbRestoreFlags.Replace;
			restoreSvc.PageSize = FbTestsSetup.PageSize;
			restoreSvc.Verbose = verbose;
			restoreSvc.InputStream = buffer;
			restoreSvc.ServiceOutput += ServiceOutput;
			return restoreSvc.ExecuteAsync();
		}

		await Connection.OpenAsync();
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "create table dummy_data (foo varchar(1000) primary key)";
			await cmd.ExecuteNonQueryAsync();
		}
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = @"execute block
as
declare cnt int;
begin
	cnt = 199999;
	while (cnt > 0) do
	begin
		insert into dummy_data values (uuid_to_char(gen_uuid()));
		cnt = cnt - 1;
	end
end";
			await cmd.ExecuteNonQueryAsync();
		}
		await Connection.CloseAsync();

		using (var ms = new MemoryStream())
		{
			await BackupPart(ms);
			ms.Position = 0;
			await RestorePart(ms, verbose);
			// test the database was actually restored fine
			await Connection.OpenAsync();
			await Connection.CloseAsync();
		}

		await Connection.OpenAsync();
		using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "drop table dummy_data";
			await cmd.ExecuteNonQueryAsync();
		}
		await Connection.CloseAsync();
	}

	[Test]
	public async Task ValidationTest()
	{
		var validationSvc = new FbValidation();
		validationSvc.ConnectionString = BuildServicesConnectionString(ServerType, Compression, WireCrypt, true);
		validationSvc.Options = FbValidationFlags.ValidateDatabase;
		validationSvc.ServiceOutput += ServiceOutput;
		await validationSvc.ExecuteAsync();
	}

	[Test]
	public async Task SweepTest()
	{
		var validationSvc = new FbValidation();
		validationSvc.ConnectionString = BuildServicesConnectionString(ServerType, Compression, WireCrypt, true);
		validationSvc.Options = FbValidationFlags.SweepDatabase;
		validationSvc.ServiceOutput += ServiceOutput;
		await validationSvc.ExecuteAsync();
	}

	[Test]
	public async Task SweepParallelTest()
	{
		if (!EnsureServerVersionAtLeast(new Version(5, 0, 0, 0)))
			return;

		var validationSvc = new FbValidation();
		var csb = BuildServicesConnectionStringBuilder(ServerType, Compression, WireCrypt, true);
		csb.ParallelWorkers = 6;
		validationSvc.ConnectionString = csb.ToString();
		validationSvc.Options = FbValidationFlags.SweepDatabase;
		validationSvc.ServiceOutput += ServiceOutput;
		await validationSvc.ExecuteAsync();
	}

	[Test]
	public async Task SetPropertiesTest()
	{
		var configurationSvc = new FbConfiguration();
		configurationSvc.ConnectionString = BuildServicesConnectionString(ServerType, Compression, WireCrypt, true);
		await configurationSvc.SetSweepIntervalAsync(1000);
		await configurationSvc.SetReserveSpaceAsync(true);
		await configurationSvc.SetForcedWritesAsync(true);
	}

	[Test]
	public async Task ShutdownOnlineTest()
	{
		var configurationSvc = new FbConfiguration();
		configurationSvc.ConnectionString = BuildServicesConnectionString(ServerType, Compression, WireCrypt, true);
		await configurationSvc.DatabaseShutdownAsync(FbShutdownMode.Forced, 10);
		await configurationSvc.DatabaseOnlineAsync();
	}

	[Test]
	public async Task ShutdownOnline2Test()
	{
		if (!EnsureServerVersionAtLeast(new Version(2, 5, 0, 0)))
			return;

		var configurationSvc = new FbConfiguration();
		configurationSvc.ConnectionString = BuildServicesConnectionString(ServerType, Compression, WireCrypt, true);
		await configurationSvc.DatabaseShutdown2Async(FbShutdownOnlineMode.Full, FbShutdownType.ForceShutdown, 10);
		await configurationSvc.DatabaseOnline2Async(FbShutdownOnlineMode.Normal);
	}

	[Test]
	public async Task StatisticsTest()
	{
		var statisticalSvc = new FbStatistical();
		statisticalSvc.ConnectionString = BuildServicesConnectionString(ServerType, Compression, WireCrypt, true);
		statisticalSvc.Options = FbStatisticalFlags.SystemTablesRelations;
		statisticalSvc.ServiceOutput += ServiceOutput;
		await statisticalSvc.ExecuteAsync();
	}

	[Test]
	public async Task StatisticsRecordVersionTest()
	{
		var sb = new StringBuilder();
		var statisticalSvc = new FbStatistical();
		statisticalSvc.ConnectionString = BuildServicesConnectionString(ServerType, Compression, WireCrypt, true);
		statisticalSvc.Options = FbStatisticalFlags.RecordVersionStatistics;
		statisticalSvc.ServiceOutput += (object sender, ServiceOutputEventArgs e) =>
		{
			sb.AppendLine(e.Message);
		};
		await statisticalSvc.ExecuteAsync();
		var statisticalOutput = sb.ToString();
		Assert.IsTrue(statisticalOutput.Contains("Average record length"), "Record statistics not found");
	}

	[Test]
	public async Task FbLogTest()
	{
		var logSvc = new FbLog();
		logSvc.ConnectionString = BuildServicesConnectionString(ServerType, Compression, WireCrypt, false);
		logSvc.ServiceOutput += ServiceOutput;
		await logSvc.ExecuteAsync();
	}

	[Test]
	public async Task AddDeleteUserTest()
	{
		var securitySvc = new FbSecurity();
		securitySvc.ConnectionString = BuildServicesConnectionString(ServerType, Compression, WireCrypt, false);
		{
			var user = new FbUserData();
			user.UserName = "new_user";
			user.UserPassword = "1";
			await securitySvc.AddUserAsync(user);
		}
		{
			var user = new FbUserData();
			user.UserName = "new_user";
			await securitySvc.DeleteUserAsync(user);
		}
	}

	[Test]
	public async Task DisplayUser()
	{
		var securitySvc = new FbSecurity();
		securitySvc.ConnectionString = BuildServicesConnectionString(ServerType, Compression, WireCrypt, false);
		var user = await securitySvc.DisplayUserAsync("SYSDBA");
	}

	[Test]
	public async Task DisplayUsers()
	{
		var securitySvc = new FbSecurity();
		securitySvc.ConnectionString = BuildServicesConnectionString(ServerType, Compression, WireCrypt, false);
		var users = await securitySvc.DisplayUsersAsync();
	}

	[Test]
	public void ServerPropertiesTest()
	{
		var serverProp = new FbServerProperties();
		serverProp.ConnectionString = BuildServicesConnectionString(ServerType, Compression, WireCrypt, false);
		foreach (var m in serverProp.GetType()
			.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
			.Where(x => !x.IsSpecialName)
			.Where(x => x.Name.EndsWith("Async")))
		{
			Assert.DoesNotThrowAsync(() => (Task)m.Invoke(serverProp, new object[] { CancellationToken.None }), m.Name);
		}
	}

	[Test]
	public async Task NBackupBackupRestoreTest()
	{
		if (!EnsureServerVersionAtLeast(new Version(2, 5, 0, 0)))
			return;

		const int Levels = 2;
		var backupName = $"{Guid.NewGuid()}.nbak";
		async Task BackupPart()
		{
			Task DoLevel(int level)
			{
				var nbak = new FbNBackup();
				nbak.ConnectionString = BuildServicesConnectionString(ServerType, Compression, WireCrypt, true);
				nbak.Level = level;
				nbak.BackupFile = backupName + level.ToString();
				nbak.DirectIO = true;
				nbak.Options = FbNBackupFlags.NoDatabaseTriggers;
				nbak.ServiceOutput += ServiceOutput;
				return nbak.ExecuteAsync();
			}
			for (var i = 0; i < Levels; i++)
			{
				await DoLevel(i);
			}
		}
		Task RestorePart()
		{
			FbConnection.DropDatabase(BuildConnectionString(ServerType, Compression, WireCrypt));
			var nrest = new FbNRestore();
			nrest.ConnectionString = BuildServicesConnectionString(ServerType, Compression, WireCrypt, true);
			nrest.BackupFiles = Enumerable.Range(0, Levels).Select(l => backupName + l.ToString());
			nrest.DirectIO = true;
			nrest.ServiceOutput += ServiceOutput;
			return nrest.ExecuteAsync();
		}
		await BackupPart();
		await RestorePart();
	}

	[Test]
	public async Task TraceTest()
	{
		var trace = new FbTrace();
		trace.ConnectionString = BuildServicesConnectionString(ServerType, Compression, WireCrypt, false);
		trace.DatabasesConfigurations.Add(new FbDatabaseTraceConfiguration()
		{
			Enabled = true,
			Events = FbDatabaseTraceEvents.Connections | FbDatabaseTraceEvents.Errors,
			TimeThreshold = TimeSpan.FromMilliseconds(1),
		});

		var sessionId = -1;
		trace.ServiceOutput += (sender, e) =>
		{
			if (sessionId == -1)
			{
				var match = Regex.Match(e.Message, @"Trace session ID (\d+) started");
				if (match.Success)
				{
					sessionId = int.Parse(match.Groups[1].Value);
				}
			}
			ServiceOutput(sender, e);
		};

		async Task Stop()
		{
			await Task.Delay(2000);
			await new FbTrace(connectionString: BuildServicesConnectionString(ServerType, Compression, WireCrypt, false))
				.StopAsync(sessionId);
		}
		var stopTask = Stop();
		await trace.StartAsync("test");
		await stopTask;

		Assert.AreNotEqual(-1, sessionId);
	}

	[Test]
	public async Task NoLingerTest()
	{
		if (!EnsureServerVersionAtLeast(new Version(3, 0, 0, 0)))
			return;

		var configurationSvc = new FbConfiguration();
		configurationSvc.ConnectionString = BuildServicesConnectionString(ServerType, Compression, WireCrypt, true);
		await configurationSvc.NoLingerAsync();
	}

	[Test, Explicit]
	public void StatisticsWithEncryptedTest()
	{
		var csb = BuildServicesConnectionStringBuilder(ServerType, Compression, WireCrypt, true);
		csb.Database = "enc.fdb";
		Task Test()
		{
			var statisticalSvc = new FbStatistical(csb.ToString());
			statisticalSvc.ServiceOutput += ServiceOutput;
			return statisticalSvc.ExecuteAsync();
		}
		Assert.ThrowsAsync<FbException>(Test);
		csb.CryptKey = Encoding.ASCII.GetBytes("1234567890123456");
		Assert.DoesNotThrowAsync(Test);
	}

	[Test]
	public async Task Validation2Test()
	{
		if (!EnsureServerVersionAtLeast(new Version(3, 0, 0, 0)))
			return;

		var validationSvc = new FbValidation2();
		validationSvc.ConnectionString = BuildServicesConnectionString(ServerType, Compression, WireCrypt, true);
		validationSvc.TablesInclude = "_*";
		validationSvc.TablesExclude = "X*";
		validationSvc.IndicesInclude = "_*";
		validationSvc.IndicesExclude = "X*";
		validationSvc.LockTimeout = 6;
		validationSvc.ServiceOutput += ServiceOutput;
		await validationSvc.ExecuteAsync();
	}

	[Test]
	public async Task NFixupTest()
	{
		if (!EnsureServerVersionAtLeast(new Version(4, 0, 0, 0)))
			return;

		var deltaFile = Path.GetTempFileName();
		Connection.Open();
		try
		{
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = $"alter database add difference file '{deltaFile}'";
				await cmd.ExecuteNonQueryAsync();
				cmd.CommandText = "alter database begin backup";
				await cmd.ExecuteNonQueryAsync();
			}
		}
		finally
		{
			Connection.Close();
		}
		File.Delete(deltaFile);

		Assert.ThrowsAsync<FbException>(() => Connection.OpenAsync());

		var fixup = new FbNFixup();
		fixup.ConnectionString = BuildServicesConnectionString(ServerType, Compression, WireCrypt, true);
		fixup.ServiceOutput += ServiceOutput;
		await fixup.ExecuteAsync();

		Assert.DoesNotThrowAsync(() => Connection.OpenAsync());
	}

	static Task BackupPartHelper(string backupName, string connectionString, Action<FbBackup> configure = null)
	{
		var backupSvc = new FbBackup();
		backupSvc.ConnectionString = connectionString;
		backupSvc.Options = FbBackupFlags.IgnoreLimbo;
		backupSvc.BackupFiles.Add(new FbBackupFile(backupName, 2048));
		backupSvc.Verbose = true;
		backupSvc.Statistics = FbBackupRestoreStatistics.TotalTime | FbBackupRestoreStatistics.TimeDelta;
		backupSvc.ServiceOutput += ServiceOutput;
		configure?.Invoke(backupSvc);
		return backupSvc.ExecuteAsync();
	}
	static Task RestorePartHelper(string backupName, string connectionString, Action<FbRestore> configure = null)
	{
		var restoreSvc = new FbRestore();
		restoreSvc.ConnectionString = connectionString;
		restoreSvc.Options = FbRestoreFlags.Create | FbRestoreFlags.Replace;
		restoreSvc.PageSize = FbTestsSetup.PageSize;
		restoreSvc.Verbose = true;
		restoreSvc.Statistics = FbBackupRestoreStatistics.TotalTime | FbBackupRestoreStatistics.TimeDelta;
		restoreSvc.BackupFiles.Add(new FbBackupFile(backupName, 2048));
		restoreSvc.ServiceOutput += ServiceOutput;
		configure?.Invoke(restoreSvc);
		return restoreSvc.ExecuteAsync();
	}

	static void ServiceOutput(object sender, ServiceOutputEventArgs e)
	{
		var dummy = e.Message;
	}
}
