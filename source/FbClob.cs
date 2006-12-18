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
	/// <include file='xmldoc/fbclob.xml' path='doc/member[@name="T:FbClob"]/*'/>
	internal sealed class FbClob : Blob
	{
		#region CONSTRUCTORS
	
		/// <include file='xmldoc/fbclob.xml' path='doc/member[@name="M:#ctor(FirebirdSql.Data.Firebird.FbConnection,FirebirdSql.Data.Firebird.FbTransaction,System.Int64)"]/*'/>
		public FbClob(FbConnection connection, FbTransaction transaction, long clobId):
				base(connection, transaction, clobId)
		{
		}

		/// <include file='xmldoc/fbclob.xml' path='doc/member[@name="M:#ctor(FirebirdSql.Data.Firebird.FbConnection,FirebirdSql.Data.Firebird.FbTransaction)"]/*'/>
		public FbClob(FbConnection connection, FbTransaction transaction):
			base(connection, transaction)
		{
		}

		#endregion

		#region METHODS

		/// <include file='xmldoc/fbclob.xml' path='doc/member[@name="M:Read()"]/*'/>
		public string Read()
		{			
			StringBuilder data				 = new StringBuilder();
			isc_blob_handle_impl clob_handle = null;

			try
			{
				clob_handle = Open();
			
				while (!clob_handle.IsEof())
				{
					byte[] clobData = GetSegment(clob_handle);					

					data.Append(encoding.GetString(clobData));
				}
			}
			catch(GDSException ex)
			{
				throw new FbException(ex.Message, ex);
			}
			finally
			{
				if (clob_handle != null)
				{
					Close(clob_handle);
				}
			}

			return data.ToString();
		}

		/// <include file='xmldoc/fbclob.xml' path='doc/member[@name="M:Write(System.String)"]/*'/>
		public long Write(string data)
		{
			long returnValue;
			isc_blob_handle_impl clob_handle = null;

			try
			{
				clob_handle = Create();

				byte[]	buffer	  = encoding.GetBytes(data);
				byte[]	tmpBuffer = null;

				int	length	= buffer.Length;
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
					System.Array.Copy(buffer, offset, tmpBuffer, 0, chunk);					
					PutSegment(clob_handle, tmpBuffer);
					
					offset += chunk;					
					length -= chunk;
				}

				returnValue = clob_handle.blob_id;
			}
			catch(GDSException ex)
			{
				throw new FbException(ex.Message, ex);
			}
			finally
			{
				if (clob_handle != null)
				{
					Close(clob_handle);
				}
			}

			return returnValue;
		}

		#endregion
	}
}
