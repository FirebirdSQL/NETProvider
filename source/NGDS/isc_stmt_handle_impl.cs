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
using System.Collections;
using FirebirdSql.Data.INGDS;

using FirebirdSql.Data.Firebird;

namespace FirebirdSql.Data.NGDS
{	
	/// <include file='xmldoc/isc_stmt_handle_impl.xml' path='doc/member[@name="T:isc_stmt_handle_impl"]/*'/>
	internal class isc_stmt_handle_impl : isc_stmt_handle
	{
		#region FIELDS

		internal int				rsr_id;
		internal isc_db_handle_impl rsr_rdb;
		internal XSQLDA				in_sqlda  = null;
		internal XSQLDA				out_sqlda = null;
		internal ArrayList			rows = new ArrayList();
		internal bool				allRowsFetched	  = false;
		internal bool				isSingletonResult = false;

		#endregion

		#region PROPERTIES

		/// <include file='xmldoc/isc_stmt_handle_impl.xml' path='doc/member[@name="P:DbHandle"]/*'/>
		public isc_db_handle DbHandle
		{
			get { return rsr_rdb; }
			set
			{
				this.rsr_rdb = (isc_db_handle_impl)value;
			}
		}

		/// <include file='xmldoc/isc_stmt_handle_impl.xml' path='doc/member[@name="T:InSqlda"]/*'/>
		public XSQLDA InSqlda
		{
			get { return in_sqlda; }
		}

		/// <include file='xmldoc/isc_stmt_handle_impl.xml' path='doc/member[@name="P:OutSqlda"]/*'/>
		public XSQLDA OutSqlda
		{
			get { return out_sqlda; }
		}

		#endregion

		#region METHODS

		/// <include file='xmldoc/isc_stmt_handle_impl.xml' path='doc/member[@name="M:ClearRows"]/*'/>
		public void ClearRows() 
		{
			rows.Clear();
			allRowsFetched = false;
		}

		#endregion
	}
}
