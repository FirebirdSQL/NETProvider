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
 *	Copyright (c) 2014 Jiri Cincura (jiri@cincura.net)
 *	All Rights Reserved.
 */

using System;
using System.Collections;
using System.Text.RegularExpressions;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Services
{
	public sealed class FbServerProperties : FbService
	{
		#region Constructors

		public FbServerProperties(string connectionString = null)
			: base(connectionString)
		{ }

		#endregion

		#region Methods

		public int GetVersion()
		{
			return this.GetInt32(IscCodes.isc_info_svc_version);
		}

		public string GetServerVersion()
		{
			return this.GetString(IscCodes.isc_info_svc_server_version);
		}

		public string GetImplementation()
		{
			return this.GetString(IscCodes.isc_info_svc_implementation);
		}

		public string GetRootDirectory()
		{
			return this.GetString(IscCodes.isc_info_svc_get_env);
		}

		public string GetLockManager()
		{
			return this.GetString(IscCodes.isc_info_svc_get_env_lock);
		}

		public string GetMessageFile()
		{
			return this.GetString(IscCodes.isc_info_svc_get_env_msg);
		}

		public FbDatabasesInfo GetDatabasesInfo()
		{
			ArrayList info = this.GetInfo(IscCodes.isc_info_svc_svr_db_info);
			return info.Count != 0 ? (FbDatabasesInfo)info[0] : new FbDatabasesInfo();
		}

		public FbServerConfig GetServerConfig()
		{
			ArrayList info = this.GetInfo(IscCodes.isc_info_svc_get_config);
			return info.Count != 0 ? (FbServerConfig)info[0] : new FbServerConfig();
		}

		#endregion

		#region Private Methods

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
			return this.Query(items);
		}

		#endregion

		#region Static Methods

		public static Version ParseServerVersion(string version)
		{
			var m = Regex.Match(version, @"\w{2}-\w(\d+\.\d+\.\d+\.\d+) .*");
			if (!m.Success)
				return null;
			return new Version(m.Groups[1].Value);
		}

		#endregion
	}
}
