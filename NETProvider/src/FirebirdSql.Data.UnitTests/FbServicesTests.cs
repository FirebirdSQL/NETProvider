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
 *  All Rights Reserved.
 *
 *  Contributors:
 *   Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Configuration;
using System.IO;
using System.Data;
using System.Text;
using System.Linq;

using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Services;
using NUnit.Framework;
using System.Text.RegularExpressions;
using System.Threading;

namespace FirebirdSql.Data.UnitTests
{
	[TestFixture(FbServerType.Default)]
	public class FbServicesTests : TestsBase
	{
		#region Constructors

		public FbServicesTests(FbServerType serverType)
			: base(serverType, false)
		{
		}

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

		#region Event Handlers

		void ServiceOutput(object sender, ServiceOutputEventArgs e)
		{
			Console.WriteLine(e.Message);
		}

		#endregion

		#region Unit Tests

		[Test]
		public void BackupRestoreTest()
		{
			BackupRestoreTest_BackupPart();
			BackupRestoreTest_RestorePart();
		}
		void BackupRestoreTest_BackupPart()
		{
			FbBackup backupSvc = new FbBackup();

			backupSvc.ConnectionString = BuildServicesConnectionString(FbServerType);
			backupSvc.Options = FbBackupFlags.IgnoreLimbo;
			backupSvc.BackupFiles.Add(new FbBackupFile(TestsSetup.BackupRestoreFile, 2048));
			backupSvc.Verbose = true;

			backupSvc.ServiceOutput += new EventHandler<ServiceOutputEventArgs>(ServiceOutput);

			backupSvc.Execute();
		}
		void BackupRestoreTest_RestorePart()
		{
			FbRestore restoreSvc = new FbRestore();

			restoreSvc.ConnectionString = BuildServicesConnectionString(FbServerType);
			restoreSvc.Options = FbRestoreFlags.Create | FbRestoreFlags.Replace;
			restoreSvc.PageSize = 4096;
			restoreSvc.Verbose = true;
			restoreSvc.BackupFiles.Add(new FbBackupFile(TestsSetup.BackupRestoreFile, 2048));

			restoreSvc.ServiceOutput += new EventHandler<ServiceOutputEventArgs>(ServiceOutput);

			restoreSvc.Execute();
		}

		[Test]
		public void StreamingBackupRestoreTest()
		{
			if (!EnsureVersion(new Version("2.5.0.0")))
				return;

			using (var ms = new MemoryStream())
			{
				StreamingBackupRestoreTest_BackupPart(ms);
				ms.Position = 0;
				StreamingBackupRestoreTest_RestorePart(ms);
			}
		}
		public void StreamingBackupRestoreTest_BackupPart(MemoryStream buffer)
		{
			FbStreamingBackup backupSvc = new FbStreamingBackup();

			backupSvc.ConnectionString = BuildServicesConnectionString(FbServerType);
			backupSvc.Options = FbBackupFlags.IgnoreLimbo;
			backupSvc.OutputStream = buffer;

			backupSvc.ServiceOutput += new EventHandler<ServiceOutputEventArgs>(ServiceOutput);

			backupSvc.Execute();
		}
		public void StreamingBackupRestoreTest_RestorePart(MemoryStream buffer)
		{
			FbStreamingRestore restoreSvc = new FbStreamingRestore();

			restoreSvc.ConnectionString = BuildServicesConnectionString(FbServerType);
			restoreSvc.Options = FbRestoreFlags.Create | FbRestoreFlags.Replace;
			restoreSvc.PageSize = 4096;
			restoreSvc.Verbose = true;
			restoreSvc.InputStream = buffer;

			restoreSvc.ServiceOutput += ServiceOutput;

			restoreSvc.Execute();
		}

		[Test]
		public void ValidationTest()
		{
			FbValidation validationSvc = new FbValidation();

			validationSvc.ConnectionString = BuildServicesConnectionString(FbServerType);
			validationSvc.Options = FbValidationFlags.ValidateDatabase;

			validationSvc.ServiceOutput += ServiceOutput;

			validationSvc.Execute();
		}

		[Test]
		public void SweepTest()
		{
			FbValidation validationSvc = new FbValidation();

			validationSvc.ConnectionString = BuildServicesConnectionString(FbServerType);
			validationSvc.Options = FbValidationFlags.SweepDatabase;

			validationSvc.ServiceOutput += ServiceOutput;

			validationSvc.Execute();
		}

		[Test]
		public void SetPropertiesTest()
		{
			FbConfiguration configurationSvc = new FbConfiguration();

			configurationSvc.ConnectionString = BuildServicesConnectionString(FbServerType);

			configurationSvc.SetSweepInterval(1000);
			configurationSvc.SetReserveSpace(true);
			configurationSvc.SetForcedWrites(true);
		}

		[Test]
		[Category("Local")]
		public void ShutdownOnlineTest()
		{
			FbConfiguration configurationSvc = new FbConfiguration();

			configurationSvc.ConnectionString = BuildServicesConnectionString(FbServerType);

			configurationSvc.DatabaseShutdown(FbShutdownMode.Forced, 10);
			configurationSvc.DatabaseOnline();
		}

		[Test]
		[Category("Local")]
		public void ShutdownOnline2Test()
		{
			if (!EnsureVersion(new Version("2.5.0.0")))
				return;

			FbConfiguration configurationSvc = new FbConfiguration();

			configurationSvc.ConnectionString = BuildServicesConnectionString(FbServerType);

			configurationSvc.DatabaseShutdown2(FbShutdownOnlineMode.Full, FbShutdownType.ForceShutdown, 10);
			configurationSvc.DatabaseOnline2(FbShutdownOnlineMode.Normal);
		}

		[Test]
		public void StatisticsTest()
		{
			FbStatistical statisticalSvc = new FbStatistical();

			statisticalSvc.ConnectionString = BuildServicesConnectionString(FbServerType);
			statisticalSvc.Options = FbStatisticalFlags.SystemTablesRelations;

			statisticalSvc.ServiceOutput += ServiceOutput;

			statisticalSvc.Execute();
		}

		[Test]
		public void FbLogTest()
		{
			FbLog logSvc = new FbLog();

			logSvc.ConnectionString = BuildServicesConnectionString(FbServerType, false);

			logSvc.ServiceOutput += ServiceOutput;

			logSvc.Execute();
		}

		[Test]
		public void AddUserTest()
		{
			FbSecurity securitySvc = new FbSecurity();

			securitySvc.ConnectionString = BuildServicesConnectionString(FbServerType, false);

			FbUserData user = new FbUserData();

			user.UserName = "new_user";
			user.UserPassword = "1";

			securitySvc.AddUser(user);
		}

		[Test]
		public void DeleteUser()
		{
			FbSecurity securitySvc = new FbSecurity();

			securitySvc.ConnectionString = BuildServicesConnectionString(FbServerType, false);

			FbUserData user = new FbUserData();

			user.UserName = "new_user";

			securitySvc.DeleteUser(user);
		}

		[Test]
		public void DisplayUser()
		{
			FbSecurity securitySvc = new FbSecurity();

			securitySvc.ConnectionString = BuildServicesConnectionString(FbServerType, false);

			FbUserData user = securitySvc.DisplayUser("SYSDBA");

			Console.WriteLine("User name {0}", user.UserName);
		}

		[Test]
		public void DisplayUsers()
		{
			FbSecurity securitySvc = new FbSecurity();

			securitySvc.ConnectionString = BuildServicesConnectionString(FbServerType, false);

			FbUserData[] users = securitySvc.DisplayUsers();

			Console.WriteLine("User List");

			for (int i = 0; i < users.Length; i++)
			{
				Console.WriteLine("User {0} name {1}", i, users[i].UserName);
			}
		}

		[Test]
		public void ServerPropertiesTest()
		{
			FbServerProperties serverProp = new FbServerProperties();

			serverProp.ConnectionString = BuildServicesConnectionString(FbServerType, false);

			FbServerConfig serverConfig = serverProp.GetServerConfig();
			FbDatabasesInfo databasesInfo = serverProp.GetDatabasesInfo();

			Console.WriteLine(serverProp.GetMessageFile());
			Console.WriteLine(serverProp.GetLockManager());
			Console.WriteLine(serverProp.GetRootDirectory());
			Console.WriteLine(serverProp.GetImplementation());
			Console.WriteLine(serverProp.GetServerVersion());
			Console.WriteLine(serverProp.GetVersion());
		}

		[Test]
		public void NBackupBackupRestoreTest()
		{
			if (!EnsureVersion(new Version("2.5.0.0")))
				return;

			NBackupBackupRestoreTest_BackupPart();
			NBackupBackupRestoreTest_RestorePart();
		}
		void NBackupBackupRestoreTest_BackupPart()
		{
			Action<int> doLevel = l =>
			{
				var nbak = new FbNBackup();

				nbak.ConnectionString = BuildServicesConnectionString(FbServerType);
				nbak.Level = l;
				nbak.BackupFile = TestsSetup.BackupRestoreFile + l.ToString();
				nbak.DirectIO = true;
				nbak.Options = FbNBackupFlags.NoDatabaseTriggers;

				nbak.ServiceOutput += ServiceOutput;

				nbak.Execute();
			};
			doLevel(0);
			doLevel(1);
		}
		void NBackupBackupRestoreTest_RestorePart()
		{
			FbConnection.DropDatabase(BuildConnectionString(FbServerType));

			var nrest = new FbNRestore();

			nrest.ConnectionString = BuildServicesConnectionString(FbServerType);
			nrest.BackupFiles = Enumerable.Range(0, 2).Select(l => TestsSetup.BackupRestoreFile + l.ToString());
			nrest.DirectIO = true;

			nrest.ServiceOutput += ServiceOutput;

			nrest.Execute();
		}

		[Test]
		public void TraceTest()
		{
			var trace = new FbTrace();
			trace.ConnectionString = BuildServicesConnectionString(FbServerType, false);
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
				new FbTrace(BuildServicesConnectionString(FbServerType, false)).Stop(sessionId);
			});
			trace.Start("test");
		}

		#endregion
	}
}
