/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.ibphoenix.com/main.nfs?a=ibphoenix&l=;PAGES;NAME='ibp_idpl'
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using NUnit.Framework;
using System;
using System.IO;
using System.Data;
using FirebirdSql.Data.Firebird;
using FirebirdSql.Data.Firebird.Services;
using System.Configuration;

namespace FirebirdSql.Data.Firebird.Tests
{
	[TestFixture]
	public class FbServicesTest : BaseTest 
	{	
		[Test]
		public void BackupTest()
		{
			FbBackup backupSvc = new FbBackup();
			
			backupSvc.UserName 		= ConfigurationSettings.AppSettings["User"];
			backupSvc.UserPassword 	= ConfigurationSettings.AppSettings["Password"];
			
			backupSvc.Database = ConfigurationSettings.AppSettings["Database"];
			backupSvc.BackupFiles.Add(new FbBackupFile(@"c:\testdb.gbk", 2048));
			backupSvc.Verbose = true;
			
			backupSvc.Options = FbBackupFlags.IgnoreLimbo;
			
			backupSvc.Start();

			string lineOutput;
			while((lineOutput = backupSvc.GetNextLine()) != null)
			{
				Console.WriteLine(lineOutput);
			}

			backupSvc.Close();
		}
		
		[Test]
		public void RestoreTest()
		{
			FbRestore restoreSvc = new FbRestore();
			
			restoreSvc.UserName 		= ConfigurationSettings.AppSettings["User"];
			restoreSvc.UserPassword 	= ConfigurationSettings.AppSettings["Password"];
			
			restoreSvc.Database = @"c:\testdb.gdb";
			restoreSvc.BackupFiles.Add(new FbBackupFile(@"c:\testdb.gbk", 2048));
			restoreSvc.Verbose	= true;
			restoreSvc.PageSize = 4096;
			restoreSvc.Options	= FbRestoreFlags.Create | FbRestoreFlags.Replace; 

			restoreSvc.Start();

			string lineOutput;
			while((lineOutput = restoreSvc.GetNextLine()) != null)
			{
				Console.WriteLine(lineOutput);
			}

			restoreSvc.Close();
		}

		[Test]
		public void ValidationTest()
		{
			FbValidation validationSvc = new FbValidation();
			
			validationSvc.UserName 		= ConfigurationSettings.AppSettings["User"];
			validationSvc.UserPassword 	= ConfigurationSettings.AppSettings["Password"];
			
			validationSvc.Database 	= ConfigurationSettings.AppSettings["Database"];
			validationSvc.Options	= FbValidationFlags.ValidateDatabase; 

			validationSvc.Start();

			string lineOutput;
			while((lineOutput = validationSvc.GetNextLine()) != null)
			{
				Console.WriteLine(lineOutput);
			}

			validationSvc.Close();
		}		
		
		[Test]
		public void SetPropertiesTest()
		{
			FbConfiguration configurationSvc = new FbConfiguration();
			
			configurationSvc.UserName 		= ConfigurationSettings.AppSettings["User"];
			configurationSvc.UserPassword 	= ConfigurationSettings.AppSettings["Password"];
			
			configurationSvc.Database 	= ConfigurationSettings.AppSettings["Database"];
						
			configurationSvc.SetSweepInterval(1000);
			configurationSvc.SetReserveSpace(true);
			configurationSvc.SetForcedWrites(true);
			configurationSvc.DatabaseShutdown(FbShutdownMode.Forced, 10);
			configurationSvc.DatabaseOnline();
		}		
		
		[Test]
		public void StatisticsTest()
		{
			FbStatistical statisticalSvc = new FbStatistical();
			
			statisticalSvc.UserName 	= ConfigurationSettings.AppSettings["User"];;
			statisticalSvc.UserPassword = ConfigurationSettings.AppSettings["Password"];;
			
			statisticalSvc.Database = ConfigurationSettings.AppSettings["Database"];;
			statisticalSvc.Options	= FbStatisticalFlags.SystemTablesRelations;
						
			statisticalSvc.Start();

			string lineOutput;
			while((lineOutput = statisticalSvc.GetNextLine()) != null)
			{
				Console.WriteLine(lineOutput);
			}

			statisticalSvc.Close();
		}
		
		[Test]
		public void FbLogTest()
		{
			FbLog logSvc = new FbLog();
			
			logSvc.UserName 		= ConfigurationSettings.AppSettings["User"];
			logSvc.UserPassword 	= ConfigurationSettings.AppSettings["Password"];
									
			logSvc.Start();

			string lineOutput;
			while((lineOutput = logSvc.GetNextLine()) != null)
			{
				Console.WriteLine(lineOutput);
			}

			logSvc.Close();
		}

		[Test]
		public void AddUserTest()
		{
			FbSecurity securitySvc = new FbSecurity();
			
			securitySvc.UserName 		= ConfigurationSettings.AppSettings["User"];
			securitySvc.UserPassword 	= ConfigurationSettings.AppSettings["Password"];
			
			FbUserData user = new FbUserData();
			
			user.UserName 		= "new_user";
			user.UserPassword 	= "1";
			
			securitySvc.AddUser(user);
		}
		
		[Test]
		public void DeleteUser()
		{
			FbSecurity securitySvc = new FbSecurity();
			
			securitySvc.UserName 		= ConfigurationSettings.AppSettings["User"];
			securitySvc.UserPassword 	= ConfigurationSettings.AppSettings["Password"];
			
			FbUserData user = new FbUserData();
			
			user.UserName = "new_user";
						
			securitySvc.DeleteUser(user);
		}

		[Test]
		public void DisplayUsers()
		{
			FbSecurity securitySvc = new FbSecurity();
			
			securitySvc.UserName 		= ConfigurationSettings.AppSettings["User"];
			securitySvc.UserPassword 	= ConfigurationSettings.AppSettings["Password"];
											
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
			
			serverProp.UserName 		= ConfigurationSettings.AppSettings["User"];
			serverProp.UserPassword 	= ConfigurationSettings.AppSettings["Password"];
						
			FbServerConfig 	serverConfig	= serverProp.ServerConfig;
			FbDatabasesInfo databasesInfo	= serverProp.DatabasesInfo;
			string			messageFile		= serverProp.MessageFile;
			string			lockManager		= serverProp.LockManager;
			string			rootDirectory	= serverProp.RootDirectory;
			string			implementation	= serverProp.Implementation;
			string 			serverVersion	= serverProp.ServerVersion;
			int				version			= serverProp.Version;						
		}
	}
}
