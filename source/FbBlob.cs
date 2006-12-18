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
using System.Collections;

using FirebirdSql.Data.INGDS;
using FirebirdSql.Data.NGDS;

namespace FirebirdSql.Data.Firebird
{
	/// <include file='xmldoc/fbblob.xml' path='doc/member[@name="T:FbBlob"]/*'/>
	internal sealed class FbBlob : Blob
	{
		#region CONSTRUCTORS
	
		/// <include file='xmldoc/fbblob.xml' path='doc/member[@name="M:#ctor(FirebirdSql.Data.Firebird.FbConnection,FirebirdSql.Data.Firebird.FbTransaction,System.Int64)"]/*'/>
		public FbBlob(FbConnection connection, FbTransaction transaction, long blobId):
				base(connection, transaction, blobId)
		{
		}

		/// <include file='xmldoc/fbblob.xml' path='doc/member[@name="M:#ctor(FirebirdSql.Data.Firebird.FbConnection,FirebirdSql.Data.Firebird.FbTransaction)"]/*'/>
		public FbBlob(FbConnection connection, FbTransaction transaction):
			base(connection,transaction)
		{
		}

		#endregion

		#region METHODS

		/// <include file='xmldoc/fbblob.xml' path='doc/member[@name="M:Read()"]/*'/>
		public byte[] Read()
		{
			ArrayList				data		= new ArrayList();
			isc_blob_handle_impl	blob_handle = null;
			int						bytesRead	= 0;
						
			try
			{
				blob_handle = Open();
			
				while (!blob_handle.IsEof())
				{
					byte[] tempData = GetSegment(blob_handle);					

					data.Add(tempData);

					bytesRead += tempData.Length;
				}				
			}
			catch(GDSException ex)
			{
				throw new FbException(ex.Message, ex);
			}
			finally
			{
				if (blob_handle != null)
				{
					Close(blob_handle);
				}
			}

			byte[]	blobData		= new byte[bytesRead];
			int		lastPosition	= 0;

			for (int i = 0; i < data.Count; i++)
			{
				System.Array.Copy((byte[])data[i], 0, blobData, lastPosition, ((byte[])data[i]).Length);

				lastPosition += ((byte[])data[i]).Length;
			}

			return blobData;
		}

		/// <include file='xmldoc/fbblob.xml' path='doc/member[@name="M:Write(System.Array)"]/*'/>
		public long Write(byte[] data)
		{
			long returnValue;
			isc_blob_handle_impl blob_handle = null;

			try
			{
				blob_handle = Create();
				
				byte[]	tmpBuffer = null;

				int	length	= data.Length;
				int	offset	= 0;
				int	chunk	= length >= connection.IscConnection.PacketSize ? 
											connection.IscConnection.PacketSize : length;

				tmpBuffer = new byte[chunk];
				
				while (length > 0)				
				{					
					if (chunk > length) 
					{
						chunk	  = (int)length;
						tmpBuffer = new byte[chunk];
					}					
					System.Array.Copy(data, offset, tmpBuffer, 0, chunk);					
					PutSegment(blob_handle, tmpBuffer);
					
					offset += chunk;					
					length -= chunk;
				}

				returnValue = blob_handle.blob_id;
			}
			catch(GDSException ex)
			{
				throw new FbException(ex.Message, ex);
			}
			finally
			{
				if (blob_handle != null)
				{
					Close(blob_handle);
				}
			}

			return returnValue;
		}

		#endregion
	}
}
