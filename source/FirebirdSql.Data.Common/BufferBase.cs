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
	abstract class BufferBase : BinaryWriter
	{	
		#region Fields

		private bool isLittleEndian;

		#endregion

		#region Properties
		
		public short Length
		{
			get { return (short)this.ToArray().Length; }
		}

		public bool IsLittleEndian
		{
			get { return this.isLittleEndian; }
		}
		
		#endregion
		
		#region Constructors
		
		protected BufferBase() : base(new MemoryStream())
		{
		}

		protected BufferBase(bool isLittleEndian) : this()
		{
			this.isLittleEndian = isLittleEndian;
		}
		
		#endregion			
		
		#region Methods
				
		public virtual void Append(int type)
		{
			this.Write((byte)type);
		}

		public byte[] ToArray()
		{
			return ((MemoryStream)(BaseStream)).ToArray();
		}
				
		#endregion
	}
}
