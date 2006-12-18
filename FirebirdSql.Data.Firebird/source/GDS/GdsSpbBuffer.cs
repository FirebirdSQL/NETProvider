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

namespace FirebirdSql.Data.Firebird.Gds
{
	internal class GdsSpbBuffer : GdsBuffer
	{
		#region CONSTRUCTORS
		
		public GdsSpbBuffer() : base()
		{
		}
		
		#endregion
		
		#region METHODS
				
		public void Append(int type)
		{
			Write((byte)type);
		}

		public void Append(int type, byte[] content)
		{						
			Write((byte)type);
			Write((short)content.Length);
			Write(content);
		}

		public void Append(byte type, byte[] content)
		{						
			Write(type);
			Write((byte)content.Length);
			Write(content);
		}

		public void Append(int type, byte content)
		{						
			Write((byte)type);
			Write(content);
		}

		public void Append(int type, int content)
		{						
			Write((byte)type);
			Write(content);			
		}

		public void Append(int type, string content)
		{			
			byte[] contents = System.Text.Encoding.Default.GetBytes(content);
			
			Write((byte)type);
			Write((short)contents.Length);
			Write(contents);
		}

		public void Append(byte type, string content)
		{			
			byte[] contents = System.Text.Encoding.Default.GetBytes(content);
			
			Write(type);
			Write((byte)contents.Length);
			Write(contents);
		}
				
		#endregion
	}
}
