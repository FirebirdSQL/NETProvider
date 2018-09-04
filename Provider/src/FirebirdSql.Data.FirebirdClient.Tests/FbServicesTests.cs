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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using FirebirdSql.Data.Services;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[FbTestFixture(FbServerType.Default, false)]
	[FbTestFixture(FbServerType.Default, true)]
	public class FbServicesTests : FbTestsBase
	{
		#region Constructors

		public FbServicesTests(FbServerType serverType, bool compression)
			: base(serverType, compression)
		{ }

		#endregion

		#region Setup Method

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();

			if (Connection != null && Connection.State == ConnectionState.Open)
			{
				Connection.Close();
			}
		}

		#endregion

		#region Unit Tests

		[Test]
		public void BackupRestoreTest()
		{
			var backupName = $"{Guid.NewGuid().ToString()}.bak";
			void BackupPart()
			{
				var backupSvc = new FbBackup();

				backupSvc.ConnectionString = BuildServicesConnectionString(FbServerType, Compression, true);
				backupSvc.Options = FbBackupFlags.IgnoreLimbo;
				backupSvc.BackupFiles.Add(new FbBackupFile(backupName, 2048));
				backupSvc.Verbose = true;

				backupSvc.ServiceOutput += ServiceOutput;

				backupSvc.Execute();
			}
			void RestorePart()
			{
				var restoreSvc = new FbRestore();

				restoreSvc.ConnectionString = BuildServicesConnectionString(FbServerType, Compression, true);
				restoreSvc.Options = FbRestoreFlags.Create | FbRestoreFlags.Replace;
				restoreSvc.PageSize = FbTestsSetup.PageSize;
				restoreSvc.Verbose = true;
				restoreSvc.BackupFiles.Add(new FbBackupFile(backupName, 2048));

				restoreSvc.ServiceOutput += ServiceOutput;

				restoreSvc.Execute();
			}
			BackupPart();
			RestorePart();
			// test the database was actually restored fine
			Connection.Open();
			Connection.Close();
		}

		[TestCase(true)]
		[TestCase(false)]
		public void StreamingBackupRestoreTest(bool verbose)
		{
			if (!EnsureVersion(new Version("2.5.0.0")))
				return;

			Connection.Open();
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "create table dummy_data (foo varchar(1000) primary key)";
				cmd.ExecuteNonQuery();
			}
			using (var cmd = Connection.CreateCommand())
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
				cmd.ExecuteNonQuery();
			}
			Connection.Close();

			using (var ms = new MemoryStream())
			{
				StreamingBackupRestoreTest_BackupPart(ms);
				ms.Position = 0;
				StreamingBackupRestoreTest_RestorePart(ms, verbose);
				// test the database was actually restored fine
				Connection.Open();
				Connection.Close();
			}

			Connection.Open();
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "drop table dummy_data";
				cmd.ExecuteNonQuery();
			}
			Connection.Close();
		}
		private void StreamingBackupRestoreTest_BackupPart(MemoryStream buffer)
		{
			var backupSvc = new FbStreamingBackup();

			backupSvc.ConnectionString = BuildServicesConnectionString(FbServerType, Compression, true);
			backupSvc.Options = FbBackupFlags.IgnoreLimbo;
			backupSvc.OutputStream = buffer;

			backupSvc.ServiceOutput += ServiceOutput;

			backupSvc.Execute();
		}
		private void StreamingBackupRestoreTest_RestorePart(MemoryStream buffer, bool verbose)
		{
			var restoreSvc = new FbStreamingRestore();

			restoreSvc.ConnectionString = BuildServicesConnectionString(FbServerType, Compression, true);
			restoreSvc.Options = FbRestoreFlags.Create | FbRestoreFlags.Replace;
			restoreSvc.PageSize = FbTestsSetup.PageSize;
			restoreSvc.Verbose = verbose;
			restoreSvc.InputStream = buffer;

			restoreSvc.ServiceOutput += ServiceOutput;

			restoreSvc.Execute();
		}

		[Test]
		public void ValidationTest()
		{
			var validationSvc = new FbValidation();

			validationSvc.ConnectionString = BuildServicesConnectionString(FbServerType, Compression, true);
			validationSvc.Options = FbValidationFlags.ValidateDatabase;

			validationSvc.ServiceOutput += ServiceOutput;

			validationSvc.Execute();
		}

		[Test]
		public void SweepTest()
		{
			var validationSvc = new FbValidation();

			validationSvc.ConnectionString = BuildServicesConnectionString(FbServerType, Compression, true);
			validationSvc.Options = FbValidationFlags.SweepDatabase;

			validationSvc.ServiceOutput += ServiceOutput;

			validationSvc.Execute();
		}

		[Test]
		public void SetPropertiesTest()
		{
			var configurationSvc = new FbConfiguration();

			configurationSvc.ConnectionString = BuildServicesConnectionString(FbServerType, Compression, true);

			configurationSvc.SetSweepInterval(1000);
			configurationSvc.SetReserveSpace(true);
			configurationSvc.SetForcedWrites(true);
		}

		[Test]
		public void ShutdownOnlineTest()
		{
			var configurationSvc = new FbConfiguration();

			configurationSvc.ConnectionString = BuildServicesConnectionString(FbServerType, Compression, true);

			configurationSvc.DatabaseShutdown(FbShutdownMode.Forced, 10);
			configurationSvc.DatabaseOnline();
		}

		[Test]
		public void ShutdownOnline2Test()
		{
			if (!EnsureVersion(new Version("2.5.0.0")))
				return;

			var configurationSvc = new FbConfiguration();

			configurationSvc.ConnectionString = BuildServicesConnectionString(FbServerType, Compression, true);

			configurationSvc.DatabaseShutdown2(FbShutdownOnlineMode.Full, FbShutdownType.ForceShutdown, 10);
			configurationSvc.DatabaseOnline2(FbShutdownOnlineMode.Normal);
		}

		[Test]
		public void StatisticsTest()
		{
			var statisticalSvc = new FbStatistical();

			statisticalSvc.ConnectionString = BuildServicesConnectionString(FbServerType, Compression, true);
			statisticalSvc.Options = FbStatisticalFlags.SystemTablesRelations;

			statisticalSvc.ServiceOutput += ServiceOutput;

			statisticalSvc.Execute();
		}

		[Test]
		public void FbLogTest()
		{
			var logSvc = new FbLog();

			logSvc.ConnectionString = BuildServicesConnectionString(FbServerType, Compression, false);

			logSvc.ServiceOutput += ServiceOutput;

			logSvc.Execute();
		}

		[Test]
		public void AddDeleteUserTest()
		{
			var securitySvc = new FbSecurity();

			securitySvc.ConnectionString = BuildServicesConnectionString(FbServerType, Compression, false);

			{
				var user = new FbUserData();
				user.UserName = "new_user";
				user.UserPassword = "1";
				securitySvc.AddUser(user);
			}

			{
				var user = new FbUserData();
				user.UserName = "new_user";
				securitySvc.DeleteUser(user);
			}
		}

		[Test]
		public void DisplayUser()
		{
			var securitySvc = new FbSecurity();

			securitySvc.ConnectionString = BuildServicesConnectionString(FbServerType, Compression, false);

			var user = securitySvc.DisplayUser("SYSDBA");
		}

		[Test]
		public void DisplayUsers()
		{
			var securitySvc = new FbSecurity();

			securitySvc.ConnectionString = BuildServicesConnectionString(FbServerType, Compression, false);

			var users = securitySvc.DisplayUsers();
		}

		[Test]
		public void ServerPropertiesTest()
		{
			var serverProp = new FbServerProperties();

			serverProp.ConnectionString = BuildServicesConnectionString(FbServerType, Compression, false);

			foreach (var m in serverProp.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).Where(x => !x.IsSpecialName))
			{
				Assert.DoesNotThrow(() => m.Invoke(serverProp, null), m.Name);
			}
		}

		[Test]
		public void NBackupBackupRestoreTest()
		{
			if (!EnsureVersion(new Version("2.5.0.0")))
				return;

			const int Levels = 2;
			var backupName = $"{Guid.NewGuid().ToString()}.nbak";
			void BackupPart()
			{
				void DoLevel(int level)
				{
					var nbak = new FbNBackup();

					nbak.ConnectionString = BuildServicesConnectionString(FbServerType, Compression, true);
					nbak.Level = level;
					nbak.BackupFile = backupName + level.ToString();
					nbak.DirectIO = true;
					nbak.Options = FbNBackupFlags.NoDatabaseTriggers;

					nbak.ServiceOutput += ServiceOutput;

					nbak.Execute();
				}
				for (var i = 0; i < Levels; i++)
				{
					DoLevel(i);
				}
			}
			void RestorePart()
			{
				FbConnection.DropDatabase(BuildConnectionString(FbServerType, Compression));

				var nrest = new FbNRestore();

				nrest.ConnectionString = BuildServicesConnectionString(FbServerType, Compression, true);
				nrest.BackupFiles = Enumerable.Range(0, Levels).Select(l => backupName + l.ToString());
				nrest.DirectIO = true;

				nrest.ServiceOutput += ServiceOutput;

				nrest.Execute();
			}
			BackupPart();
			RestorePart();
		}

		[Test]
		public void TraceTest()
		{
			var trace = new FbTrace();
			trace.ConnectionString = BuildServicesConnectionString(FbServerType, Compression, false);
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

			ThreadPool.QueueUserWorkItem(_ =>
			{
				Thread.Sleep(2000);
				new FbTrace(connectionString: BuildServicesConnectionString(FbServerType, Compression, false)).Stop(sessionId);
			});
			trace.Start("test");

			Assert.AreNotEqual(-1, sessionId);
		}

		[Test]
		public void NoLingerTest()
		{
			if (!EnsureVersion(new Version("3.0.0.0")))
				return;

			var configurationSvc = new FbConfiguration();

			configurationSvc.ConnectionString = BuildServicesConnectionString(FbServerType, Compression, true);

			configurationSvc.NoLinger();
		}

		[Test, Explicit]
		public void StatisticsWithEncryptedTest()
		{
			var csb = BuildServicesConnectionStringBuilder(FbServerType, Compression, true);
			csb.Database = "enc.fdb";
			void Test()
			{
				var statisticalSvc = new FbStatistical(csb.ToString());
				statisticalSvc.ServiceOutput += ServiceOutput;
				statisticalSvc.Execute();
			}
			Assert.Throws<FbException>(Test);
			csb.CryptKey = Encoding.ASCII.GetBytes("1234567890123456");
			Assert.DoesNotThrow(Test);
		}

		#endregion

		#region Methods

		static void ServiceOutput(object sender, ServiceOutputEventArgs e)
		{
			var dummy = e.Message;
		}

		#endregion
	}
}
