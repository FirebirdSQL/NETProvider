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
using System.IO;
using System.Collections;

namespace FirebirdSql.Data.Firebird.Gds
{
	internal abstract class GdsBlob
	{
		#region FIELDS

		private GdsDbAttachment	db;
		private GdsTransaction		transaction;
		private long				handle;
		private int					rblId;
		private	int					rblFlags;
		private int					position;

		#endregion

		#region PROPERTIES

		public GdsDbAttachment DB
		{
			get { return db; }
		}

		public long Handle
		{
			get { return handle; }
		}

		public int RblId
		{
			get { return rblId; }
		}

		public bool EOF
		{
			get { return (rblFlags & GdsCodes.RBL_eof_pending) != 0; }
		}

		public int Position
		{
			get { return position; }
		}

		#endregion

		#region CONSTRUCTORS
	
		protected GdsBlob(GdsDbAttachment db, GdsTransaction transaction, long handle)
		{
			this.db				= db;
			this.transaction	= transaction;
			this.handle			= handle;
		}

		protected GdsBlob(GdsDbAttachment db, GdsTransaction transaction)
		{
			this.db				= db;
			this.transaction	= transaction;			
		}

		#endregion

		#region METHODS

		public void Close()
		{	
			db.ReleaseObject(GdsCodes.op_close_blob, rblId);
		}		

		#endregion

		#region PROTECTED_METHODS

		protected void Create()
		{
			try
			{
				createOrOpen(GdsCodes.op_create_blob, null);
				rblAddValue(GdsCodes.RBL_create);
			}
			catch(GdsException ex)
			{
				throw ex;
			}
		}

		protected void Open()
		{
			try
			{
				createOrOpen(GdsCodes.op_open_blob, null);
			}
			catch(GdsException ex)
			{
				throw ex;
			}
		}

		protected byte[] GetSegment()
		{
			int requested = db.Parameters.PacketSize;

			lock (db) 
			{
				try 
				{
					db.Send.WriteInt(GdsCodes.op_get_segment);
					db.Send.WriteInt(rblId);
					db.Send.WriteInt((requested + 2 < short.MaxValue) ? requested+2 : short.MaxValue);
					db.Send.WriteInt(0);
					db.Send.Flush();

					GdsResponse r = db.ReceiveResponse();

					rblRemoveValue(GdsCodes.RBL_segment);
					if (r.ObjectHandle == 1) 
					{
						rblAddValue(GdsCodes.RBL_segment);						
					}
					else if (r.ObjectHandle == 2) 
					{
						rblAddValue(GdsCodes.RBL_eof_pending);
					}
					byte[] buffer = r.Data;
					if (buffer.Length == 0) 
					{
						// previous segment was last, this has no data
						return buffer;
					}
					int len		= 0;
					int srcpos	= 0;
					int destpos = 0;
					while (srcpos < buffer.Length) 
					{
						len = db.VaxInteger(buffer, srcpos, 2);
						srcpos	+= 2;
						System.Array.Copy(buffer, srcpos, buffer, destpos, len);
						srcpos	+= len;
						destpos += len;
					}
					byte[] result = new byte[destpos];
					System.Array.Copy(buffer, 0, result, 0, destpos);

					return result;
				}
				catch (IOException) 
				{
					throw new GdsException(GdsCodes.isc_net_read_err);
				}
			}
		}

		protected void PutSegment(byte[] data)
		{
			lock (db) 
			{
				try 
				{
					db.Send.WriteInt(GdsCodes.op_batch_segments);
					db.Send.WriteInt(rblId);
					db.Send.WriteBlobBuffer(data);
					db.Send.Flush();

					GdsResponse r = db.ReceiveResponse();
				}
				catch (IOException) 
				{
					throw new GdsException(GdsCodes.isc_net_read_err);
				}
			}		
		}

		protected void Seek(int position)
		{
			lock (db)
			{
				try 
				{
					db.Send.WriteInt(GdsCodes.op_seek_blob);
					db.Send.WriteInt(rblId);
					db.Send.WriteInt(0);
					db.Send.WriteInt(position);
					db.Send.Flush();

					GdsResponse r = db.ReceiveResponse();
	
					this.position = r.ObjectHandle;
				} 
				catch (IOException) 
				{
					throw new GdsException(GdsCodes.isc_network_error);
				} 
			}
		}

		protected void GetBlobInfo()
		{
		}

		#endregion

		#region PRIVATE_API_METHODS

		private void rblAddValue(int rblValue)
		{
			this.rblFlags |= rblValue;
		}

		public void rblRemoveValue(int rblValue)
		{
			this.rblFlags &= ~rblValue;
		}

		private void createOrOpen(int op, GdsDpbBuffer bpb)
		{
			lock (db)
			{
				try 
				{
					db.Send.WriteInt(op);
					if (bpb != null) 
					{
						db.Send.WriteTyped(GdsCodes.isc_bpb_version1, bpb.ToArray());
					}
					db.Send.WriteInt(transaction.Handle);
					db.Send.WriteLong(handle);
					db.Send.Flush();

					GdsResponse r = db.ReceiveResponse();

					rblId	= r.ObjectHandle;
					handle	= r.BlobHandle;
				}
				catch (IOException) 
				{
					throw new GdsException(GdsCodes.isc_net_read_err);
				}
			}
		}

		#endregion
	}
}
