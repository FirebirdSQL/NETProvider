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
using System.Collections.Specialized;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Firebird.Services
{
	#region Structs

	/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/overview/*'/>
	public struct FbServerConfig
	{
		#region Fields

		private int lockMemSize;
		private int lockSemCount;
		private int lockSignal;
		private int eventMemorySize;
		private int prioritySwitchDelay;
		private int minMemory;
		private int maxMemory;
		private int lockGrantOrder;
		private int anyLockMemory;
		private int anyLockSemaphore;
		private int anyLockSignal;
		private int anyEventMemory;
		private int lockHashSlots;
		private int deadlockTimeout;
		private int lockRequireSpins;
		private int connectionTimeout;
		private int dummyPacketInterval;
		private int ipcMapSize;
		private int defaultDbCachePages;

		#endregion

		#region Properties

		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="LockMemSize"]/*'/>
		public int LockMemSize
		{
			get { return this.lockMemSize; }
			set { this.lockMemSize = value; }
		}
		
		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="LockSemCount"]/*'/>
		public int LockSemCount
		{
			get { return this.lockSemCount; }
			set { this.lockSemCount = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="LockSignal"]/*'/>
		public int LockSignal
		{
			get { return this.lockSignal; }
			set { this.lockSignal = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="EventMemorySize"]/*'/>
		public int EventMemorySize
		{
			get { return this.eventMemorySize; }
			set { this.eventMemorySize = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="PrioritySwitchDelay"]/*'/>
		public int PrioritySwitchDelay
		{
			get { return this.prioritySwitchDelay; }
			set { this.prioritySwitchDelay = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="MinMemory"]/*'/>
		public int MinMemory
		{
			get { return this.minMemory; }
			set { this.minMemory = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="MaxMemory"]/*'/>
		public int MaxMemory
		{
			get { return this.maxMemory; }
			set { this.maxMemory = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="LockGrantOrder"]/*'/>
		public int LockGrantOrder
		{
			get { return this.lockGrantOrder; }
			set { this.lockGrantOrder = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="AnyLockMemory"]/*'/>
		public int AnyLockMemory
		{
			get { return this.anyLockMemory; }
			set { this.anyLockMemory = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="AnyLockSemaphore"]/*'/>
		public int AnyLockSemaphore
		{
			get { return this.anyLockSemaphore; }
			set { this.anyLockSemaphore = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="AnyLockSignal"]/*'/>
		public int AnyLockSignal
		{
			get { return this.anyLockSignal; }
			set { this.anyLockSignal = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="AnyEventMemory"]/*'/>
		public int AnyEventMemory
		{
			get { return this.anyEventMemory; }
			set { this.anyEventMemory = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="LockHashSlots"]/*'/>
		public int LockHashSlots
		{
			get { return this.lockHashSlots; }
			set { this.lockHashSlots = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="DeadlockTimeout"]/*'/>
		public int DeadlockTimeout
		{
			get { return this.deadlockTimeout; }
			set { this.deadlockTimeout = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="LockRequireSpins"]/*'/>
		public int LockRequireSpins
		{
			get { return this.lockRequireSpins; }
			set { this.lockRequireSpins = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="ConnectionTimeout"]/*'/>
		public int ConnectionTimeout
		{
			get { return this.connectionTimeout; }
			set { this.connectionTimeout = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="DummyPacketInterval"]/*'/>
		public int DummyPacketInterval
		{
			get { return this.dummyPacketInterval; }
			set { this.dummyPacketInterval = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="IpcMapSize"]/*'/>
		public int IpcMapSize
		{
			get { return this.ipcMapSize; }
			set { this.ipcMapSize = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbServerConfig"]/field[@name="DefaultDbCachePages"]/*'/>
		public int DefaultDbCachePages
		{
			get { return this.defaultDbCachePages; }
			set { this.defaultDbCachePages = value; }
		}


		#endregion
	}

	/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbDatabasesInfo"]/overview/*'/>
	public struct FbDatabasesInfo
	{
		#region Fields

		private int 				connectionCount;
		private StringCollection	databases;

		#endregion

		#region Properties

		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbDatabasesInfo"]/field[@name="ConnectionCount"]/*'/>
		public int ConnectionCount
		{
			get { return this.connectionCount; }
			set { this.connectionCount = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/struct[@name="FbDatabasesInfo"]/field[@name="Databases"]/*'/>
		public StringCollection	Databases
		{
			get 
			{ 
				if (this.databases == null)
				{
					this.databases = new StringCollection();
				}
				return this.databases; 
			}
		}

		#endregion
	}

	#endregion

	#region Enumerations

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
		#region Fields
		
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

		#region Properties

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
		
		#region Constructors

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
		#region Fields

		private string				serviceName;
		private ISvcAttachment		svc;
		private FbServiceParameters	parameters;
		private FbServiceState		state;
		private	int					queryBufferSize;
		private SpbBuffer			attachSpb;
		private SpbBuffer			querySpb;

		#endregion

		#region Protected Fields

		internal SpbBuffer StartSpb;		
				
		#endregion
		
		#region Properties
		
		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbService"]/property[@name="State"]/*'/>
		public FbServiceState State
		{
			get { return state; }
		}

		/// <summary>
		/// Connection parameters.
		/// </summary>
		public FbServiceParameters Parameters
		{
			get { return this.parameters; }
			set { this.parameters = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbService"]/property[@name="QueryBufferSize"]/*'/>
		public int QueryBufferSize
		{
			get { return this.queryBufferSize; }
			set { this.queryBufferSize = value; }
		}
		
		#endregion

		#region Constructors

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbService"]/constructor[@name="ctor"]/*'/>
		protected FbService()
		{
			this.serviceName 		= "service_mgr";
			this.state				= FbServiceState.Closed;
			this.parameters			= new FbServiceParameters();
			this.queryBufferSize	= 1024;
		}

		#endregion

		#region Methods

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbService"]/method[@name="Close"]/*'/>
		public void Close()
		{
			if (this.state != FbServiceState.Open)
			{
                return;
            }

			try
			{
				this.svc.Detach();
				this.svc	= null;
				
				this.state = FbServiceState.Closed;
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbService"]/method[@name="GetNextLine"]/*'/>
		public string GetNextLine()
		{
			byte[]	buffer;
			byte[]	items = new byte[]{ IscCodes.isc_info_svc_line };
			
			this.querySpb = new SpbBuffer();
			
			buffer = this.QueryService(items);
			
			ArrayList info = this.ParseQueryInfo(buffer);
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

		#region Protected Methods

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbService"]/method[@name="Open"]/*'/>
		protected void Open()
		{
			if (this.state != FbServiceState.Closed)
			{
				throw new InvalidOperationException("Service already Open.");
			}
			if (this.Parameters.UserName == null || 
				this.Parameters.UserName.Length == 0)
			{
				throw new InvalidOperationException("No user name was specified.");
			}
			if (this.Parameters.UserPassword == null || 
				this.Parameters.UserPassword.Length == 0)
			{
				throw new InvalidOperationException("No user password was specified.");
			}
			
			try
			{
				// New instance of SpbBuffer class
				this.attachSpb = new SpbBuffer();

				// Create an AttachmentParams instance
				AttachmentParams attparams = new AttachmentParams();

				attparams.DataSource	= parameters.DataSource;
				attparams.Database		= parameters.Database;
				attparams.UserName		= parameters.UserName;
				attparams.UserPassword	= parameters.UserPassword;
				attparams.Role			= parameters.Role;
				attparams.PacketSize	= parameters.PacketSize;

				// New instance for Service handler
				FactoryBase factory = ClientFactory.GetInstance(parameters.ServerType);
				this.svc = factory.CreateSvcConnection(attparams);
				
				// SPB configuration				
				this.attachSpb.Append(IscCodes.isc_spb_version);
				this.attachSpb.Append(IscCodes.isc_spb_current_version);
				this.attachSpb.Append(
					(byte)IscCodes.isc_spb_user_name, 
					this.Parameters.UserName);
				this.attachSpb.Append(
					(byte)IscCodes.isc_spb_password, 
					this.Parameters.UserPassword);
				if (this.Parameters.Role != null && this.Parameters.Role.Length > 0)
				{
					this.attachSpb.Append(
						(byte)IscCodes.isc_spb_sql_role_name, 
						this.Parameters.Role);
				}
				this.attachSpb.Append(
					(byte)IscCodes.isc_spb_dummy_packet_interval, 
					new byte[] {120, 10, 0, 0});

				// Set service name
				string service = String.Empty;
				
				switch (this.parameters.ServerType)
				{
					case 0:
						service = this.Parameters.DataSource + ":" + serviceName;
						break;

					default:
						service = serviceName;
						break;
				}
					
				// Initialize Services API
				this.svc.Attach(service, this.attachSpb);
				
				this.state = FbServiceState.Open;
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbService"]/method[@name="startTask"]/*'/>
		protected void StartTask()
		{					
			if (this.state == FbServiceState.Closed)
			{
				// Attach to Service Manager
				this.Open();
			}
			
			try
			{												
				// Start service operation
				this.svc.Start(this.StartSpb);				
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);				
			}
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbService"]/method[@name="queryService"]/*'/>
		protected byte[] QueryService(byte[] items)
		{
			if (this.state == FbServiceState.Closed)
			{
				// Attach to Service Manager
				this.Open();
			}

			if (this.querySpb == null)
			{
				this.querySpb = new SpbBuffer();
			}
			
			// Response buffer
			byte[] buffer = new byte[queryBufferSize];
							
			this.svc.Query(
				this.querySpb	,
				items.Length	,
				items			,
				buffer.Length	,
				buffer);
			
			return buffer;
		}

		/// <include file='Doc/en_EN/FbService.xml' path='doc/class[@name="FbService"]/method[@name="parseQueryInfo"]/*'/>
		protected ArrayList ParseQueryInfo(byte[] buffer)
		{
			int pos		= 0;
			int length	= 0;
			int type	= 0;
			
			ArrayList items = new ArrayList();			
			
			while ((type = buffer[pos++]) != IscCodes.isc_info_end)
			{
				length 	= svc.VaxInteger(buffer, pos, 2);
				pos 	+= 2;
				
				if (length != 0)
				{
					switch (type)
					{
						case IscCodes.isc_info_svc_version:
						case IscCodes.isc_info_svc_get_license_mask:
						case IscCodes.isc_info_svc_capabilities:
						case IscCodes.isc_info_svc_get_licensed_users:
							items.Add(svc.VaxInteger(buffer, pos, 4));
							pos += length;
							break;
						
						case IscCodes.isc_info_svc_server_version:
						case IscCodes.isc_info_svc_implementation:
						case IscCodes.isc_info_svc_get_env:
						case IscCodes.isc_info_svc_get_env_lock:
						case IscCodes.isc_info_svc_get_env_msg:
						case IscCodes.isc_info_svc_user_dbpath:
						case IscCodes.isc_info_svc_line:
						case IscCodes.isc_info_svc_to_eof:
							items.Add(Encoding.Default.GetString(buffer, pos, length));
							pos += length;
							break;
						
						case IscCodes.isc_info_svc_svr_db_info:
							items.Add(parseDatabasesInfo(buffer, ref pos));
							break;

						case IscCodes.isc_info_svc_get_users:
							items.Add(parseUserData(buffer, ref pos));
							break;
		
						case IscCodes.isc_info_svc_get_config:
							items.Add(parseServerConfig(buffer, ref pos));							
							break;					
					}
				}
			}
			
			return items;
		}

		#endregion

		#region Private Methods

		private FbServerConfig parseServerConfig(byte[] buffer, ref int pos)
		{
			FbServerConfig config = new FbServerConfig();

			pos = 1;		
			while(buffer[pos] != IscCodes.isc_info_flag_end)
			{			
				pos++;
				
				int key	= buffer[pos - 1];
				int keyValue = this.svc.VaxInteger(buffer, pos, 4);
						
				pos += 4;	
						
				switch (key)
				{
					case IscCodes.ISCCFG_LOCKMEM_KEY:
						config.LockMemSize = keyValue;
						break;
					
					case IscCodes.ISCCFG_LOCKSEM_KEY:
						config.LockSemCount = keyValue;
						break;
					
					case IscCodes.ISCCFG_LOCKSIG_KEY:
						config.LockSignal = keyValue;
						break;
					
					case IscCodes.ISCCFG_EVNTMEM_KEY:
						config.EventMemorySize = keyValue;
						break;
					
					case IscCodes.ISCCFG_PRIORITY_KEY:
						config.PrioritySwitchDelay = keyValue;
						break;
					
					case IscCodes.ISCCFG_MEMMIN_KEY:
						config.MinMemory = keyValue;
						break;
					
					case IscCodes.ISCCFG_MEMMAX_KEY:
						config.MaxMemory = keyValue;
						break;
					
					case IscCodes.ISCCFG_LOCKORDER_KEY:
						config.LockGrantOrder = keyValue;
						break;
					
					case IscCodes.ISCCFG_ANYLOCKMEM_KEY:
						config.AnyLockMemory = keyValue;
						break;
					
					case IscCodes.ISCCFG_ANYLOCKSEM_KEY:
						config.AnyLockSemaphore = keyValue;
						break;

					case IscCodes.ISCCFG_ANYLOCKSIG_KEY:
						config.AnyLockSignal = keyValue;
						break;
					
					case IscCodes.ISCCFG_ANYEVNTMEM_KEY:
						config.AnyEventMemory = keyValue;
						break;
					
					case IscCodes.ISCCFG_LOCKHASH_KEY:
						config.LockHashSlots = keyValue;
						break;
					
					case IscCodes.ISCCFG_DEADLOCK_KEY:
						config.DeadlockTimeout = keyValue;
						break;
					
					case IscCodes.ISCCFG_LOCKSPIN_KEY:
						config.LockRequireSpins = keyValue;
						break;
					
					case IscCodes.ISCCFG_CONN_TIMEOUT_KEY:
						config.ConnectionTimeout = keyValue;
						break;
					
					case IscCodes.ISCCFG_DUMMY_INTRVL_KEY:
						config.DummyPacketInterval = keyValue;
						break;
					
					case IscCodes.ISCCFG_IPCMAP_KEY:
						config.IpcMapSize = keyValue;
						break;
					
					case IscCodes.ISCCFG_DBCACHE_KEY:
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
			int 			type	= 0;
			int				length	= 0;
			
			pos = 1;
			while ((type = buffer[pos++]) != IscCodes.isc_info_end)
			{							
				switch (type)
				{
					case IscCodes.isc_spb_num_att:
						dbInfo.ConnectionCount = this.svc.VaxInteger(buffer, pos, 4);										
						pos += 4;
						break;

					case IscCodes.isc_spb_num_db:
						pos += 4;
						break;
					
					case IscCodes.isc_spb_dbname:
						length = svc.VaxInteger(buffer, pos, 2);
						pos += 2;										
						dbInfo.Databases.Add(Encoding.Default.GetString(buffer, pos, length));
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
																
			while ((type = buffer[pos++]) != IscCodes.isc_info_end)
			{								
				switch (type)
				{																		
					case IscCodes.isc_spb_sec_username:
					{
						currentUser = new FbUserData();

						users.Add(currentUser);

						length = svc.VaxInteger(buffer, pos, 2);
						pos += 2;										
						currentUser.UserName = Encoding.Default.GetString(buffer, pos, length);
						pos += length;
					}
					break;
					
					case IscCodes.isc_spb_sec_firstname:
						length = svc.VaxInteger(buffer, pos, 2);
						pos += 2;										
						currentUser.FirstName = Encoding.Default.GetString(buffer, pos, length);
						pos += length;
						break;

					case IscCodes.isc_spb_sec_middlename:
						length = svc.VaxInteger(buffer, pos, 2);
						pos += 2;										
						currentUser.MiddleName = Encoding.Default.GetString(buffer, pos, length);
						pos += length;
						break;

					case IscCodes.isc_spb_sec_lastname:
						length = svc.VaxInteger(buffer, pos, 2);
						pos += 2;										
						currentUser.LastName = Encoding.Default.GetString(buffer, pos, length);
						pos += length;
						break;

					case IscCodes.isc_spb_sec_userid:
						currentUser.UserID = this.svc.VaxInteger(buffer, pos, 4);
						pos += 4;
						break;
					
					case IscCodes.isc_spb_sec_groupid:
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
