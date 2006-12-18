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
using System.Text;

using FirebirdSql.Data.INGDS;
using FirebirdSql.Data.NGDS;

namespace FirebirdSql.Data.Firebird
{
	internal class FbDatabaseInfo
	{
		#region FIELDS

		private FbIscConnection connection;

		// Database info
		private static byte[] databaseInfo  = new byte[]
		{
			GdsCodes.isc_info_firebird_version,
			GdsCodes.isc_info_end
		};

		// Statement info size
		private static int INFO_SIZE = 1024;

		private string			iscVersion;
		private string			firebirdVersion;
		private int				serverClass;
		private int				odsMajorVersion;
		private int				odsMinorVersion;
		private int				pageSize;
		private int				sqlDialect;
		private string			charset;
		private int				numBuffers;
		private bool			isReadOnly;		
		private int				currentMemory;
		private int				maxMemory;

		#endregion

		#region PROPERTIES

		public string FirebirdVersion
		{
			get { return firebirdVersion; }
		}

		#endregion

		#region CONSTRUCTORS

		public FbDatabaseInfo(FbIscConnection connection)
		{
			this.connection	= connection;
			GetInfo();
		}

		#endregion

		#region METHODS

		public void GetInfo()
		{
			byte[] buffer = new byte[INFO_SIZE];

			connection.GDS.isc_database_info(
				connection.db,
				databaseInfo.Length,
				databaseInfo,
				INFO_SIZE,
				buffer);

			int pos = 0;
			int length;
			int type;
			while ((type = buffer[pos++]) != GdsCodes.isc_info_end)
			{
				length = connection.GDS.isc_vax_integer(buffer, pos, 2);
				pos += 2;
				switch (type) 
				{					
					case GdsCodes.isc_info_isc_version:
						iscVersion = Encoding.Default.GetString(buffer, pos, length);
						pos += length;
						break;

					case GdsCodes.isc_info_firebird_version:
						firebirdVersion = Encoding.Default.GetString(buffer, pos, length);
						pos += length;
						break;

					case GdsCodes.isc_info_db_class:
						serverClass = connection.GDS.isc_vax_integer(buffer, pos, length);
						pos += length;
						break;

					case GdsCodes.isc_info_ods_version:
						odsMajorVersion = connection.GDS.isc_vax_integer(buffer, pos, length);
						pos += length;
						break;

					case GdsCodes.isc_info_ods_minor_version:
						odsMinorVersion = connection.GDS.isc_vax_integer(buffer, pos, length);
						pos += length;
						break;

					case GdsCodes.isc_info_page_size:
						pageSize = connection.GDS.isc_vax_integer(buffer, pos, length);
						pos += length;
						break;

					case GdsCodes.isc_info_db_sql_dialect:
						sqlDialect = connection.GDS.isc_vax_integer(buffer, pos, length);
						pos += length;
						break;

					case GdsCodes.frb_info_att_charset:
						charset = Encoding.Default.GetString(buffer, pos, length);
						pos += length;
						break;

					case GdsCodes.isc_info_num_buffers:
						numBuffers = connection.GDS.isc_vax_integer(buffer, pos, length);
						pos += length;
						break;

					case GdsCodes.isc_info_db_read_only:
						isReadOnly = buffer[pos] == 1 ? true : false;
						pos += length;
						break;
					
					case GdsCodes.isc_info_current_memory:
						currentMemory = connection.GDS.isc_vax_integer(buffer, pos, length);
						pos += length;
						break;

					case GdsCodes.isc_info_max_memory:
						maxMemory = connection.GDS.isc_vax_integer(buffer, pos, length);
						pos += length;
						break;

					default:
						pos += length;
						break;
				}
			}
		}

		#endregion
	}
}
