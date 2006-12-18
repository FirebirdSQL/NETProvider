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

namespace FirebirdSql.Data.NGDS
{
	/// <include file='xmldoc/isc_tr_handle_impl.xml' path='doc/member[@name="T:isc_tr_handle_impl"]/*'/>
	internal class isc_tr_handle_impl : isc_tr_handle
	{
		#region FIELDS

		private int					rtr_id;
		private isc_db_handle_impl	rtr_rdb;
		private ArrayList			blobs = new ArrayList();		

		private TxnState			state = TxnState.NOTRANSACTION;

		#endregion

		#region PROPERTIES

		/// <include file='xmldoc/isc_tr_handle_impl.xml' path='doc/member[@name="P:DbHandle"]/*'/>
		public isc_db_handle DbHandle
		{
			get { return rtr_rdb; }
			set
			{
				this.rtr_rdb = (isc_db_handle_impl)value;
				rtr_rdb.AddTransaction(this);
			}
		}

		/// <include file='xmldoc/isc_tr_handle_impl.xml' path='doc/member[@name="P:State"]/*'/>
		public TxnState State
		{
			get { return state; }
			set { state = value; }
		}
	
		/// <include file='xmldoc/isc_tr_handle_impl.xml' path='doc/member[@name="P:TransactionId"]/*'/>
		public int TransactionId
		{
			get { return rtr_id; }
			set { rtr_id = value; }
		}

		#endregion

		#region METHODS

		/// <include file='xmldoc/isc_tr_handle_impl.xml' path='doc/member[@name="M:UnsetDbHandle"]/*'/>
		public void UnsetDbHandle()
		{
			rtr_rdb.RemoveTransaction(this);
			rtr_rdb = null;
		}

		/// <include file='xmldoc/isc_tr_handle_impl.xml' path='doc/member[@name="M:AddBlob(FirebirdSql.Data.NGDS.isc_blob_handle_impl)"]/*'/>
		public void AddBlob(isc_blob_handle_impl blob) 
		{
			blobs.Add(blob);
		}

		/// <include file='xmldoc/isc_tr_handle_impl.xml' path='doc/member[@name="M:RemoveBlob(FirebirdSql.Data.NGDS.isc_blob_handle_impl)"]/*'/>
		public void RemoveBlob(isc_blob_handle_impl blob) 
		{
			blobs.Remove(blob);
		}

		#endregion
	}
}
