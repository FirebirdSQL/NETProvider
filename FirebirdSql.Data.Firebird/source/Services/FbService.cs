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

using System;
using System.IO;
using System.Text;
using System.Net;
using System.Collections;
using FirebirdSql.Data.Firebird.Gds;

namespace FirebirdSql.Data.Firebird.Services
{
	#region STRUCTS

	/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/overview/*'/>
	public struct FbServerConfig
	{
		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="LockMemSize"]/*'/>
		public int LockMemSize;
		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="LockSemCount"]/*'/>
		public int LockSemCount;
		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="LockSignal"]/*'/>
		public int LockSignal;
		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="EventMemorySize"]/*'/>
		public int EventMemorySize;
		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="PrioritySwitchDelay"]/*'/>
		public int PrioritySwitchDelay;
		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="MinMemory"]/*'/>
		public int MinMemory;
		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="MaxMemory"]/*'/>
		public int MaxMemory;
		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="LockGrantOrder"]/*'/>
		public int LockGrantOrder;
		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="AnyLockMemory"]/*'/>
		public int AnyLockMemory;
		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="AnyLockSemaphore"]/*'/>
		public int AnyLockSemaphore;
		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="AnyLockSignal"]/*'/>
		public int AnyLockSignal;
		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="AnyEventMemory"]/*'/>
		public int AnyEventMemory;
		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="LockHashSlots"]/*'/>
		public int LockHashSlots;
		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="DeadlockTimeout"]/*'/>
		public int DeadlockTimeout;
		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="LockRequireSpins"]/*'/>
		public int LockRequireSpins;
		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="ConnectionTimeout"]/*'/>
		public int ConnectionTimeout;
		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="DummyPacketInterval"]/*'/>
		public int DummyPacketInterval;
		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="IpcMapSize"]/*'/>
		public int IpcMapSize;
		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="DefaultDbCachePages"]/*'/>
		public int DefaultDbCachePages;
	}

	/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbDatabasesInfo"]/overview/*'/>
	public struct FbDatabasesInfo
	{
		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbDatabasesInfo"]/field[@name="ConnectionCount"]/*'/>
		public int 		ConnectionCount;
		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbDatabasesInfo"]/field[@name="DatabaseCount"]/*'/>
		public int 		DatabaseCount;
		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbDatabasesInfo"]/field[@name="Databases"]/*'/>
		public string[]	Databases;
	}

	#endregion

	#region ENUMS

	/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServiceState"]/overview/*'/>
	public enum FbServiceState
	{
		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServiceState"]/field[@name="Closed"]/*'/>
		Closed	= 0,
		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServiceState"]/field[@name="Open"]/*'/>
		Open 	= 1
	}

	#endregion

	/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbUserData"]/overview/*'/>
	public sealed class FbUserData
	{
		#region FIELDS
		
		private string 	userName	= String.Empty;
		private string 	firstName	= String.Empty;
		private string 	lastName	= String.Empty;
		private string 	middleName	= String.Empty;
		private	string 	userPassword	= String.Empty;
		private int 	userID;
		private int 	groupID;
		private string 	groupName	= String.Empty;
		private string 	roleName	= String.Empty;

		#endregion

		#region PROPERTIES

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbUserData"]/property[@name="UserName"]/*'/>
		public string UserName
		{
			get { return userName; }
			set 
			{
				if (value == null)
				{
					throw new InvalidOperationException("The user name cannot be null.");
				}
				if (value.Length > 31)
				{
					throw new InvalidOperationException("The user name cannot have more tha 31 characters.");
				}
				
				userName = value; 
			}
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbUserData"]/property[@name="UserPassword"]/*'/>
		public string UserPassword
		{
			get { return userPassword; }
			set 
			{ 
				if (value == null)
				{
					throw new InvalidOperationException("The user password cannot be null.");
				}
				if (value.Length > 31)
				{
					throw new InvalidOperationException("The user password cannot have more tha 31 characters.");
				}

				userPassword = value; 
			}
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbUserData"]/property[@name="FirstName"]/*'/>		
		public string FirstName
		{
			get { return firstName; }
			set { firstName = value; }		
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbUserData"]/property[@name="LastName"]/*'/>
		public string LastName
		{
			get { return lastName; }
			set { lastName = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbUserData"]/property[@name="MiddleName"]/*'/>
		public string MiddleName
		{
			get { return middleName; }
			set { middleName = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbUserData"]/property[@name="UserID"]/*'/>
		public int UserID
		{
			get { return userID; }
			set { userID = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbUserData"]/property[@name="GroupID"]/*'/>
		public int GroupID
		{			
			get { return groupID; }
			set { groupID = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbUserData"]/property[@name="GroupName"]/*'/>
		public string GroupName
		{			
			get { return groupName; }
			set { groupName = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbUserData"]/property[@name="RoleName"]/*'/>
		public string RoleName
		{
			get { return roleName; }
			set { roleName = value; }
		}
		
		#endregion
		
		#region CONSTRUCTORS

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbUserData"]/constructor[@name="FbUserData"]/*'/>
		public FbUserData()
		{
			userName		= String.Empty;
			firstName		= String.Empty;
			lastName		= String.Empty;
			middleName		= String.Empty;
			userPassword	= String.Empty;
			userID			= 0;
			groupID			= 0;
			roleName		= String.Empty;
		}
		
		#endregion		
	}

	/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbService"]/overview/*'/>
	public abstract class FbService
	{
		#region FIELDS

		private GdsSvcAttachment	svc;
		
		private string			serverName;
		private string			serviceName;
		private string			userName;
		private	string			userPassword;
		private	string			roleName;
		private int				serverPort;
		private FbServiceState	state;
		private	int				queryBufferSize;
		
		internal GdsSpbBuffer	attachSpb;
		internal GdsSpbBuffer	startSpb;
		internal GdsSpbBuffer	querySpb;
				
		#endregion
		
		#region PROPERTIES
		
		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbService"]/property[@name="State"]/*'/>
		public FbServiceState State
		{
			get { return state; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbService"]/property[@name="ServerName"]/*'/>		
		public string ServerName
		{
			get { return serverName; }
			set { serverName = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbService"]/property[@name="UserName"]/*'/>
		public string UserName
		{
			get { return userName; }
			set { userName = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbService"]/property[@name="UserPassword"]/*'/>
		public string UserPassword
		{
			get { return userPassword; }
			set { userPassword = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbService"]/property[@name="RoleName"]/*'/>
		public string RoleName
		{
			get { return roleName; }
			set { roleName = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbService"]/property[@name="ServerPort"]/*'/>
		public int ServerPort
		{
			get { return serverPort; }
			set { serverPort = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbService"]/property[@name="QueryBufferSize"]/*'/>
		public int QueryBufferSize
		{
			get { return queryBufferSize; }
			set { queryBufferSize = value; }
		}
		
		#endregion

		#region CONSTRUCTORS

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbService"]/constructor[@name="ctor"]/*'/>
		protected FbService()
		{
			state			= FbServiceState.Closed;
			serverName		= "localhost";
			serviceName 	= "service_mgr";
			userName		= "SYSDBA";			
			userPassword	= "masterkey";
			serverPort		= 3050;
			roleName 		= String.Empty;
			queryBufferSize	= 1024;
		}

		#endregion

		#region METHODS

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbService"]/method[@name="Open"]/*'/>
		public void Open()
		{
			if (state != FbServiceState.Closed)
			{
				throw new InvalidOperationException("Service already Open.");
			}
			if (userName == null || userName.Length == 0)
			{
				throw new InvalidOperationException("No user name was specified.");
			}
			if (userPassword == null || userPassword.Length == 0)
			{
				throw new InvalidOperationException("No user password was specified.");
			}
			
			try
			{
				// New instance of GdsSpbBuffer class
				attachSpb = new GdsSpbBuffer();

				// Setup Attach Info
				GdsAttachParams	 parameters	= new GdsAttachParams();
				parameters.DataSource	= serverName;
				parameters.Port			= serverPort;

				// New instance for Service handler
				svc	= new GdsSvcAttachment(parameters);
				
				// SPB configuration				
				attachSpb.Append(GdsCodes.isc_spb_current_version);
				attachSpb.Append((byte)GdsCodes.isc_spb_user_name, userName);
				attachSpb.Append((byte)GdsCodes.isc_spb_password, userPassword);
				if (roleName != String.Empty)
				{
					attachSpb.Append((byte)GdsCodes.isc_spb_sql_role_name, roleName);
				}
				attachSpb.Append((byte)GdsCodes.isc_spb_dummy_packet_interval, 
								new byte[] {120, 10, 0, 0});

				// Set service name
				string service = serverName + ":" + serviceName;
	
				// Initialize Services API
				svc.Attach(service, attachSpb);
				
				state = FbServiceState.Open;
			}
			catch (GdsException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbService"]/method[@name="Close"]/*'/>
		public void Close()
		{
			if (state != FbServiceState.Open)
			{
				throw new InvalidOperationException("Service already Closed.");
			}

			try
			{
				svc.Detach();
				svc	= null;
				
				state = FbServiceState.Closed;
			}
			catch (GdsException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbService"]/method[@name="GetNextLine"]/*'/>
		public string GetNextLine()
		{
			byte[]	buffer;
			byte[]	items	= new byte[]{ GdsCodes.isc_info_svc_line };
			
			querySpb = new GdsSpbBuffer();
			
			buffer = queryService(items);
			
			ArrayList info = parseQueryInfo(buffer);
			if (info.Count != 0)
			{
				return info[0] as string;
			}
			else
			{
				return null;	
			}
		}

		#endregion

		#region PROTECTED_METHODS

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbService"]/method[@name="startTask"]/*'/>
		protected void startTask()
		{					
			if (state == FbServiceState.Closed)
			{
				// Attach to Service Manager
				Open();
			}
			
			try
			{												
				// Start service operation
				svc.Start(startSpb);				
			}
			catch (GdsException ex)
			{
				throw new FbException(ex.Message, ex);				
			}
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbService"]/method[@name="queryService"]/*'/>
		protected byte[] queryService(byte[] items)
		{
			if (state == FbServiceState.Closed)
			{
				// Attach to Service Manager
				Open();
			}

			if (querySpb == null)
			{
				querySpb = new GdsSpbBuffer();
			}
			
			// Response buffer
			byte[] buffer = new byte[queryBufferSize];
							
			svc.Query(
				querySpb		,
				items.Length	,
				items			,
				buffer.Length	,
				buffer);
			
			return buffer;
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbService"]/method[@name="parseQueryInfo"]/*'/>
		protected ArrayList parseQueryInfo(byte[] buffer)
		{
			int pos		= 0;
			int length	= 0;
			int type	= 0;
			
			ArrayList items = new ArrayList();			
			
			while ((type = buffer[pos++]) != GdsCodes.isc_info_end)
			{
				length 	= svc.VaxInteger(buffer, pos, 2);
				pos 	+= 2;
				
				if (length != 0)
				{
					switch (type)
					{
						case GdsCodes.isc_info_svc_version:
						case GdsCodes.isc_info_svc_get_license_mask:
						case GdsCodes.isc_info_svc_capabilities:
						case GdsCodes.isc_info_svc_get_licensed_users:
							items.Add(svc.VaxInteger(buffer, pos, 4));
							pos += length;
							break;
						
						case GdsCodes.isc_info_svc_server_version:
						case GdsCodes.isc_info_svc_implementation:
						case GdsCodes.isc_info_svc_get_env:
						case GdsCodes.isc_info_svc_get_env_lock:
						case GdsCodes.isc_info_svc_get_env_msg:
						case GdsCodes.isc_info_svc_user_dbpath:
						case GdsCodes.isc_info_svc_line:
						case GdsCodes.isc_info_svc_to_eof:
							items.Add(Encoding.Default.GetString(buffer, pos, length));
							pos += length;
							break;
						
						case GdsCodes.isc_info_svc_svr_db_info:
						{															
							items.Add(parseDatabasesInfo(buffer, ref pos));
						}
						break;

						case GdsCodes.isc_info_svc_get_users:
						{
							items.Add(parseUserData(buffer, ref pos));
						}
						break;
		
						case GdsCodes.isc_info_svc_get_config:
						{							
							items.Add(parseServerConfig(buffer, ref pos));							
						}
						break;					
					}
				}
			}
			
			return items;
		}

		#endregion

		#region PRIVATE_METHODS

		private FbServerConfig parseServerConfig(byte[] buffer, ref int pos)
		{
			FbServerConfig	config = new FbServerConfig();

			pos = 1;		
			while(buffer[pos] != GdsCodes.isc_info_flag_end)
			{			
				pos++;
				
				int key	= buffer[pos - 1];
				int keyValue = svc.VaxInteger(buffer, pos, 4);
						
				pos += 4;	
						
				switch (key)
				{
					case GdsCodes.ISCCFG_LOCKMEM_KEY:
						config.LockMemSize = keyValue;
						break;
					
					case GdsCodes.ISCCFG_LOCKSEM_KEY:
						config.LockSemCount = keyValue;
						break;
					
					case GdsCodes.ISCCFG_LOCKSIG_KEY:
						config.LockSignal = keyValue;
						break;
					
					case GdsCodes.ISCCFG_EVNTMEM_KEY:
						config.EventMemorySize = keyValue;
						break;
					
					case GdsCodes.ISCCFG_PRIORITY_KEY:
						config.PrioritySwitchDelay = keyValue;
						break;
					
					case GdsCodes.ISCCFG_MEMMIN_KEY:
						config.MinMemory = keyValue;
						break;
					
					case GdsCodes.ISCCFG_MEMMAX_KEY:
						config.MaxMemory = keyValue;
						break;
					
					case GdsCodes.ISCCFG_LOCKORDER_KEY:
						config.LockGrantOrder = keyValue;
						break;
					
					case GdsCodes.ISCCFG_ANYLOCKMEM_KEY:
						config.AnyLockMemory = keyValue;
						break;
					
					case GdsCodes.ISCCFG_ANYLOCKSEM_KEY:
						config.AnyLockSemaphore = keyValue;
						break;

					case GdsCodes.ISCCFG_ANYLOCKSIG_KEY:
						config.AnyLockSignal = keyValue;
						break;
					
					case GdsCodes.ISCCFG_ANYEVNTMEM_KEY:
						config.AnyEventMemory = keyValue;
						break;
					
					case GdsCodes.ISCCFG_LOCKHASH_KEY:
						config.LockHashSlots = keyValue;
						break;
					
					case GdsCodes.ISCCFG_DEADLOCK_KEY:
						config.DeadlockTimeout = keyValue;
						break;
					
					case GdsCodes.ISCCFG_LOCKSPIN_KEY:
						config.LockRequireSpins = keyValue;
						break;
					
					case GdsCodes.ISCCFG_CONN_TIMEOUT_KEY:
						config.ConnectionTimeout = keyValue;
						break;
					
					case GdsCodes.ISCCFG_DUMMY_INTRVL_KEY:
						config.DummyPacketInterval = keyValue;
						break;
					
					case GdsCodes.ISCCFG_IPCMAP_KEY:
						config.IpcMapSize = keyValue;
						break;
					
					case GdsCodes.ISCCFG_DBCACHE_KEY:
						config.DefaultDbCachePages = keyValue;
						break;
				}
			}
			
			pos++;
			
			return config;
		}

		private FbDatabasesInfo parseDatabasesInfo(byte[] buffer, ref int pos)
		{
			FbDatabasesInfo dbInfo 	= new FbDatabasesInfo();			
			int 			i		= 0;
			int 			type	= 0;
			int				length	= 0;
			
			pos = 1;
			while ((type = buffer[pos++]) != GdsCodes.isc_info_end)
			{							
				switch (type)
				{
					case GdsCodes.isc_spb_num_att:
						dbInfo.ConnectionCount = svc.VaxInteger(buffer, pos, 4);										
						pos += 4;
						break;
					
					case GdsCodes.isc_spb_num_db:
						dbInfo.DatabaseCount = svc.VaxInteger(buffer, pos, 4);
						dbInfo.Databases = new string[dbInfo.DatabaseCount];
						pos += 4;
						break;
					
					case GdsCodes.isc_spb_dbname:
						length = svc.VaxInteger(buffer, pos, 2);
						pos += 2;										
						dbInfo.Databases[i++] = Encoding.Default.GetString(buffer, pos, length);
						pos += length;
						break;
				}
			}
			pos--;			
			
			return dbInfo;
		}

		private FbUserData[] parseUserData(byte[] buffer, ref int pos)
		{
			ArrayList 	users 		= new ArrayList();
			FbUserData	currentUser = null;
			int			type		= 0;
			int			length		= 0;
																
			while ((type = buffer[pos++]) != GdsCodes.isc_info_end)
			{								
				switch (type)
				{																		
					case GdsCodes.isc_spb_sec_username:
					{
						currentUser = new FbUserData();

						users.Add(currentUser);

						length = svc.VaxInteger(buffer, pos, 2);
						pos += 2;										
						currentUser.UserName = Encoding.Default.GetString(buffer, pos, length);
						pos += length;
					}
						break;
					
					case GdsCodes.isc_spb_sec_firstname:
						length = svc.VaxInteger(buffer, pos, 2);
						pos += 2;										
						currentUser.FirstName = Encoding.Default.GetString(buffer, pos, length);
						pos += length;
						break;

					case GdsCodes.isc_spb_sec_middlename:
						length = svc.VaxInteger(buffer, pos, 2);
						pos += 2;										
						currentUser.MiddleName = Encoding.Default.GetString(buffer, pos, length);
						pos += length;
						break;

					case GdsCodes.isc_spb_sec_lastname:
						length = svc.VaxInteger(buffer, pos, 2);
						pos += 2;										
						currentUser.LastName = Encoding.Default.GetString(buffer, pos, length);
						pos += length;
						break;

					case GdsCodes.isc_spb_sec_userid:
						currentUser.UserID = this.svc.VaxInteger(buffer, pos, 4);
						pos += 4;
						break;
					
					case GdsCodes.isc_spb_sec_groupid:
						currentUser.GroupID = this.svc.VaxInteger(buffer, pos, 4);
						pos += 4;							
						break;					
				}
			}
			pos--;
			
			return (FbUserData[])users.ToArray(typeof(FbUserData));
		}
		
		#endregion
	}
}
