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

namespace FirebirdSql.Data.Common
{
#if (SINGLE_DLL)
	internal
#else
	public
#endif
	sealed class SpbBuffer : BufferBase
	{
		#region Constructors
		
		public SpbBuffer() : base()
		{
		}
		
		public SpbBuffer(bool isLittleEndian) : base(isLittleEndian)
		{
		}

		#endregion
		
		#region Methods

		public void Append(int type, byte content)
		{						
			this.Write((byte)type);
			this.Write(content);
		}

		public void Append(int type, int content)
		{						
			this.Write((byte)type);
			this.Write(content);			
		}

		public void Append(int type, string content)
		{			
			byte[] contents = System.Text.Encoding.Default.GetBytes(content);
			
			this.Write((byte)type);
			this.Write((short)contents.Length);
			this.Write(contents);
		}

		public void Append(byte type, string content)
		{			
			byte[] contents = System.Text.Encoding.Default.GetBytes(content);
			
			this.Write(type);
			this.Write((byte)contents.Length);
			this.Write(contents);
		}

		public void Append(int type, byte[] content)
		{						
			this.Write((byte)type);
			this.Write((short)content.Length);
			this.Write(content);
		}

		public void Append(byte type, byte[] content)
		{						
			this.Write(type);
			this.Write((byte)content.Length);
			this.Write(content);
		}
				
		#endregion
	}
}
