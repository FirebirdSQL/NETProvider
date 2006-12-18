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
using System.Data;
using System.Text;
using System.Collections;

using FirebirdSql.Data.INGDS;
using FirebirdSql.Data.NGDS;

namespace FirebirdSql.Data.Firebird
{
	/// <include file='xmldoc/blob.xml' path='doc/member[@name="T:Blob"]/*'/>
	internal class Blob
	{
		#region FIELDS

		protected FbConnection  connection;
		protected FbTransaction transaction;
		protected long			blob_id;
		protected Encoding		encoding;

		#endregion

		#region PROPERTIES

		/// <include file='xmldoc/blob.xml' path='doc/member[@name="P:BlobId"]/*'/>
		public long BlobId
		{
			get { return blob_id; }
			set { blob_id = value; }
		}

		#endregion

		#region CONSTRUCTORS
	
		private Blob()
		{
			connection	= null;
			transaction	= null;
		}

		/// <include file='xmldoc/blob.xml' path='doc/member[@name="M:#ctor(FirebirdSql.Data.Firebird.FbConnection,FirebirdSql.Data.Firebird.FbTransaction,System.Int64)"]/*'/>
		public Blob(FbConnection connection, FbTransaction transaction, long blob_id) : this()
		{
			this.connection		= connection;
			this.transaction	= transaction;
			this.BlobId			= blob_id;
			this.encoding		= connection.Encoding;
		}

		/// <include file='xmldoc/blob.xml' path='doc/member[@name="M:#ctor(FirebirdSql.Data.Firebird.FbConnection,FirebirdSql.Data.Firebird.FbTransaction)"]/*'/>
		public Blob(FbConnection connection, FbTransaction transaction) : this()
		{
			this.connection		= connection;
			this.transaction	= transaction;			
			this.encoding		= connection.Encoding;
		}

		#endregion

		#region METHODS

		/// <include file='xmldoc/blob.xml' path='doc/member[@name="M:Create()"]/*'/>
		protected virtual isc_blob_handle_impl Create()
		{
			 isc_blob_handle_impl blob_handle = null;

			try
			{
				blob_handle = 
					(isc_blob_handle_impl)connection.IscConnection.GDS.get_new_isc_blob_handle();
				
				connection.IscConnection.GDS.isc_create_blob2(connection.IscConnection.db, transaction.IscTransaction, blob_handle, null);
			}
			catch(GDSException ex)
			{
				throw ex;
			}

			return blob_handle;
		}

		/// <include file='xmldoc/blob.xml' path='doc/member[@name="M:Open"]/*'/>
		protected virtual isc_blob_handle_impl Open()
		{
			isc_blob_handle_impl blob_handle = null;

			try
			{
				blob_handle = 
					(isc_blob_handle_impl)connection.IscConnection.GDS.get_new_isc_blob_handle ();

				blob_handle.BlobId = BlobId;

				connection.IscConnection.GDS.isc_open_blob2(connection.IscConnection.db, transaction.IscTransaction, blob_handle, null);
			}
			catch(GDSException ex)
			{
				throw ex;
			}

			return blob_handle;
		}

		/// <include file='xmldoc/blob.xml' path='doc/member[@name="M:GetSegment(FirebirdSql.Data.INGDS.isc_blob_handle)"]/*'/>
		protected virtual byte[] GetSegment(isc_blob_handle_impl blob_handle)
		{
			try
			{
				return connection.IscConnection.GDS.isc_get_segment(blob_handle, 
													connection.IscConnection.PacketSize);
			}
			catch(GDSException ex)
			{
				throw ex;
			}
		}

		/// <include file='xmldoc/blob.xml' path='doc/member[@name="M:PutSegment(FirebirdSql.Data.INGDS.isc_blob_handle,byte[])"]/*'/>
		protected virtual void PutSegment(isc_blob_handle_impl blob_handle, byte[] data)
		{
			try
			{
				connection.IscConnection.GDS.isc_put_segment(blob_handle, data);
			}
			catch(GDSException ex)
			{
				throw ex;
			}
		}

		/// <include file='xmldoc/blob.xml' path='doc/member[@name="M:Close(FirebirdSql.Data.INGDS.isc_blob_handle)"]/*'/>
		protected virtual void Close(isc_blob_handle_impl blob_handle)
		{
			try
			{
				connection.IscConnection.GDS.isc_close_blob(blob_handle);
			}
			catch(GDSException ex)
			{
				throw ex;
			}
		}		

		#endregion
	}
}
