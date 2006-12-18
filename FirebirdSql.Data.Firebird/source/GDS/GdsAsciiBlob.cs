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
using System.IO;
using System.Data;
using System.Text;
using System.Collections;

namespace FirebirdSql.Data.Firebird.Gds
{
	internal class GdsAsciiBlob : GdsBlob
	{
		#region CONSTRUCTORS
	
		public GdsAsciiBlob(GdsDbAttachment db, GdsTransaction transaction, long handle):
				base(db, transaction, handle)
		{
		}

		public GdsAsciiBlob(GdsDbAttachment db, GdsTransaction transaction):
			base(db, transaction)
		{
		}

		#endregion

		#region METHODS

		public string Read()
		{
			MemoryStream ms			= new MemoryStream();
			Encoding	 encoding	= this.DB.Parameters.Charset.Encoding;
						
			this.Open();
			
			while (!EOF)
			{
				byte[] segment = this.GetSegment();
				ms.Write(segment, 0, segment.Length);
			}

			return encoding.GetString(ms.ToArray(), 0, (int)ms.Length);
		}

		public void Write(string data)
		{
			Create();

			byte[]	buffer	  = DB.Parameters.Charset.Encoding.GetBytes(data);
			byte[]	tmpBuffer = null;

			int	length	= buffer.Length;
			int	offset	= 0;
			int	chunk	= length >= DB.Parameters.PacketSize ? 
									DB.Parameters.PacketSize : length;

			tmpBuffer = new byte[chunk];				
			while (length > 0)
			{					
				if (chunk > length) 
				{
					chunk	  = (int)length;
					tmpBuffer = new byte[chunk];
				}					
				System.Array.Copy(buffer, offset, tmpBuffer, 0, chunk);					
				PutSegment(tmpBuffer);
				
				offset += chunk;					
				length -= chunk;
			}
		}

		#endregion
	}
}
