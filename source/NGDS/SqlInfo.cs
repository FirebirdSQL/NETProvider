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
 * 
 *  This file was originally ported from JayBird <http://firebird.sourceforge.net/>
 */

using System;
using FirebirdSql.Data.INGDS;

namespace FirebirdSql.Data.NGDS
{	
	/// <include file='xmldoc/sqlinfo.xml' path='doc/member[@name="T:SqlInfo"]/*'/>
	internal class SqlInfo 
	{
		#region FIELDS

		private int statementType;
		private int insertCount;
		private int updateCount;
		private int deleteCount;
		private int selectCount;

		#endregion

		#region PROPERTIES

		/// <include file='xmldoc/sqlinfo.xml' path='doc/member[@name="P:SqlInfo"]/*'/>	
		public int StatementType
		{
			get { return statementType; }
		}

		/// <include file='xmldoc/sqlinfo.xml' path='doc/member[@name="P:SqlInfo"]/*'/>	
		public int InsertCount
		{
			get { return insertCount; }
		}

		/// <include file='xmldoc/sqlinfo.xml' path='doc/member[@name="P:SqlInfo"]/*'/>	
		public int UpdateCount
		{
			get { return updateCount; }
		}

		/// <include file='xmldoc/sqlinfo.xml' path='doc/member[@name="P:SqlInfo"]/*'/>	
		public int DeleteCount
		{
			get { return deleteCount; }
		}

		/// <include file='xmldoc/sqlinfo.xml' path='doc/member[@name="P:SqlInfo"]/*'/>	
		public int SelectCount
		{
			get { return selectCount; }
		}

		#endregion

		#region CONSTRUCTORS

		/// <include file='xmldoc/sqlinfo.xml' path='doc/member[@name="M:#ctor(System.Array,FirebirdSql.Data.INGDS.IGDS)"]/*'/>			
		public SqlInfo(byte[] buffer, IGDS gds) 
		{
			int pos = 0;
			int length;
			int type;
			while ((type = buffer[pos++]) != GdsCodes.isc_info_end) 
			{
				length = gds.isc_vax_integer(buffer, pos, 2);
				pos += 2;
				switch (type) 
				{
					case GdsCodes.isc_info_sql_records:
						int l;
						int t;
						while ((t = buffer[pos++]) != GdsCodes.isc_info_end) 
						{
							l = gds.isc_vax_integer(buffer, pos, 2);
							pos += 2;
							switch (t) 
							{
								case GdsCodes.isc_info_req_insert_count:
									insertCount = gds.isc_vax_integer(buffer, pos, l);
									break;
								
								case GdsCodes.isc_info_req_update_count:
									updateCount = gds.isc_vax_integer(buffer, pos, l);
									break;
								
								case GdsCodes.isc_info_req_delete_count:
									deleteCount = gds.isc_vax_integer(buffer, pos, l);
									break;
								
								case GdsCodes.isc_info_req_select_count:
									selectCount = gds.isc_vax_integer(buffer, pos, l);
									break;
								
								default:
									break;
							}
							pos += l;
						}
						break;
					
					case GdsCodes.isc_info_sql_stmt_type:
						statementType = gds.isc_vax_integer(buffer, pos, length);
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
