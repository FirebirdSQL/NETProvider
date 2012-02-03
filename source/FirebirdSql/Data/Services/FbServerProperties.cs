/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.Collections;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Services
{
	public sealed class FbServerProperties : FbService
	{
		#region · Properties ·

		public int Version
		{
			get { return this.GetInt32(IscCodes.isc_info_svc_version); }
		}

		public string ServerVersion
		{
			get { return this.GetString(IscCodes.isc_info_svc_server_version); }
		}

		public string Implementation
		{
			get { return this.GetString(IscCodes.isc_info_svc_implementation); }
		}

		public string RootDirectory
		{
			get { return this.GetString(IscCodes.isc_info_svc_get_env); }
		}

		public string LockManager
		{
			get { return this.GetString(IscCodes.isc_info_svc_get_env_lock); }
		}

		public string MessageFile
		{
			get { return this.GetString(IscCodes.isc_info_svc_get_env_msg); }
		}

		public FbDatabasesInfo DatabasesInfo
		{
			get
			{
				ArrayList info = this.GetInfo(IscCodes.isc_info_svc_svr_db_info);

				return info.Count != 0 ? (FbDatabasesInfo)info[0] : new FbDatabasesInfo();
			}
		}

		public FbServerConfig ServerConfig
		{
			get
			{
				ArrayList info = this.GetInfo(IscCodes.isc_info_svc_get_config);

				return info.Count != 0 ? (FbServerConfig)info[0] : new FbServerConfig();
			}
		}

		#endregion

		#region · Constructors ·

		public FbServerProperties()
			: base()
		{ }

		#endregion

		#region · Private Methods ·

		private string GetString(int item)
		{
			ArrayList info = this.GetInfo(item);

			return info.Count != 0 ? (string)info[0] : null;
		}

		private int GetInt32(int item)
		{
			ArrayList info = this.GetInfo(item);

			return info.Count != 0 ? (int)info[0] : 0;
		}

		private ArrayList GetInfo(int item)
		{
			return this.GetInfo(new byte[] { (byte)item });
		}

		private ArrayList GetInfo(byte[] items)
		{
			byte[] buffer = this.QueryService(items);

			return this.ParseQueryInfo(buffer);
		}

		#endregion
	}
}
