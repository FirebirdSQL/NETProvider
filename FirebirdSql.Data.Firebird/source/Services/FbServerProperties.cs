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
using System.Collections;
using FirebirdSql.Data.Firebird.Gds;

namespace FirebirdSql.Data.Firebird.Services
{
	/// <include file='Doc/en_EN/FbServerProperties.xml' path='doc/class[@name="FbServerProperties"]/overview/*'/>
	public sealed class FbServerProperties : FbService
	{
		#region PROPERTIES

		/// <include file='Doc/en_EN/FbServerProperties.xml' path='doc/class[@name="FbServerProperties"]/property[@name="Version"]/*'/>
		public int Version
		{
			get
			{
				byte[] buffer	= queryService(new byte[] {GdsCodes.isc_info_svc_version});
				ArrayList info	= base.parseQueryInfo(buffer);
						
				return info.Count != 0 ? (int)info[0] : 0;
			}
		}

		/// <include file='Doc/en_EN/FbServerProperties.xml' path='doc/class[@name="FbServerProperties"]/property[@name="ServerVersion"]/*'/>
		public string ServerVersion
		{
			get
			{
				byte[] buffer = queryService(new byte[] {GdsCodes.isc_info_svc_server_version});
				ArrayList info = base.parseQueryInfo(buffer);

				return info.Count != 0 ? (string)info[0] : null;
			}
		}

		/// <include file='Doc/en_EN/FbServerProperties.xml' path='doc/class[@name="FbServerProperties"]/property[@name="Implementation"]/*'/>
		public string Implementation
		{
			get
			{
				byte[] buffer	= queryService(new byte[] {GdsCodes.isc_info_svc_implementation});
				ArrayList info	= base.parseQueryInfo(buffer);

				return info.Count != 0 ? (string)info[0] : null;
			}
		}

		/// <include file='Doc/en_EN/FbServerProperties.xml' path='doc/class[@name="FbServerProperties"]/property[@name="RootDirectory"]/*'/>
		public string RootDirectory
		{			
			get
			{
				byte[] buffer = queryService(new byte[] {GdsCodes.isc_info_svc_get_env});
				ArrayList info = base.parseQueryInfo(buffer);
			
				return info.Count != 0 ? (string)info[0] : null;
			}
		}

		/// <include file='Doc/en_EN/FbServerProperties.xml' path='doc/class[@name="FbServerProperties"]/property[@name="LockManager"]/*'/>
		public string LockManager
		{
			get
			{
				byte[] buffer = queryService(new byte[] {GdsCodes.isc_info_svc_get_env_lock});
				ArrayList info = base.parseQueryInfo(buffer);

				return info.Count != 0 ? (string)info[0] : null;
			}
		}

		/// <include file='Doc/en_EN/FbServerProperties.xml' path='doc/class[@name="FbServerProperties"]/property[@name="MessageFile"]/*'/>
		public string MessageFile
		{			
			get
			{
				byte[] buffer = queryService(new byte[] {GdsCodes.isc_info_svc_get_env_msg});
				ArrayList info = base.parseQueryInfo(buffer);
			
				return info.Count != 0 ? (string)info[0] : null;
			}
		}

		/// <include file='Doc/en_EN/FbServerProperties.xml' path='doc/class[@name="FbServerProperties"]/property[@name="DatabasesInfo"]/*'/>
		public FbDatabasesInfo DatabasesInfo
		{
			get
			{
				byte[] buffer = queryService(new byte[] {GdsCodes.isc_info_svc_svr_db_info});
				ArrayList info = base.parseQueryInfo(buffer);

				return info.Count != 0 ? (FbDatabasesInfo)info[0] : new FbDatabasesInfo();
			}
		}

		/// <include file='Doc/en_EN/FbServerProperties.xml' path='doc/class[@name="FbServerProperties"]/property[@name="ServerConfig"]/*'/>
		public FbServerConfig ServerConfig
		{
			get
			{
				byte[] buffer = queryService(new byte[] {GdsCodes.isc_info_svc_get_config});
				ArrayList info = base.parseQueryInfo(buffer);

				return info.Count != 0 ? (FbServerConfig)info[0] : new FbServerConfig();
			}
		}

		#endregion

		#region CONSTRUCTORS

		/// <include file='Doc/en_EN/FbServerProperties.xml' path='doc/class[@name="FbServerProperties"]/constructor[@name="FbServerProperties"]/*'/>
		public FbServerProperties() : base()
		{
		}
		
		#endregion
	}
}
