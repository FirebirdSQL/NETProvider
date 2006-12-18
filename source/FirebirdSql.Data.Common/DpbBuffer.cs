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
using System.Text;
using System.Net;

namespace FirebirdSql.Data.Common
{
#if (SINGLE_DLL)
	internal
#else
	public
#endif
	sealed class DpbBuffer : BufferBase
	{			
		#region Constructors

		public DpbBuffer() : base()
		{
		}
		
		public DpbBuffer(bool isLittleEndian) : base(isLittleEndian)
		{
		}

		#endregion			
		
		#region Methods

		public void Append(int type, byte content)
		{						
			this.Write((byte)type);
			this.Write((byte)1);
			this.Write(content);
		}

		public void Append(int type, short content)
		{						
			this.Write((byte)type);
			this.Write((byte)2);
			if (!this.IsLittleEndian)
			{
				this.Write((short)IPAddress.NetworkToHostOrder(content));
			}
			else
			{
				this.Write((short)content);
			}
		}

		public void Append(int type, int content)
		{
			this.Write((byte)type);
			this.Write((byte)4);
			if (!this.IsLittleEndian)
			{
				this.Write((int)IPAddress.NetworkToHostOrder(content));
			}
			else
			{
				this.Write(content);
			}
		}

		public void Append(int type, string content)
		{
			this.Append(type, Encoding.Default.GetBytes(content));
		}

		public void Append(int type, byte[] content)
		{						
			this.Write((byte)type);
			this.Write((byte)content.Length);
			this.Write(content);
		}
				
		#endregion
	}
}
