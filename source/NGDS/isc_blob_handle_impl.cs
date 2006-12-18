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
	/// <include file='xmldoc/isc_blob_handle_impl.xml' path='doc/member[@name="T:isc_blob_handle_impl"]/*'/>
	internal class isc_blob_handle_impl : isc_blob_handle 
	{
		#region FIELDS

		internal isc_db_handle_impl	 db;
		internal isc_tr_handle_impl	 tr;
		
		internal int	rbl_id;
		internal long	blob_id;		
		private	 int	rbl_flags;

		#endregion

		#region PROPERTIES

		/// <include file='xmldoc/isc_blob_handle_impl.xml' path='doc/member[@name="P:BlobId"]/*'/>
		public long BlobId
		{
			get { return blob_id; }
			set { blob_id = value; }			
		}

		#endregion

		#region METHODS

		/// <include file='xmldoc/isc_blob_handle_impl.xml' path='doc/member[@name="M:#ctor"]/*'/>
		public bool IsEof()
		{		
			return (rbl_flags & GdsCodes.RBL_eof_pending) != 0;
		}

		public void RBLAddValue(int rblValue)
		{
			this.rbl_flags |= rblValue;
		}

		public void RBLRemoveValue(int rblValue)
		{
			this.rbl_flags &= ~rblValue;
		}

		#endregion
	}
}
