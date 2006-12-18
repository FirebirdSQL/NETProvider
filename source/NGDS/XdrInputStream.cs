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
using System.IO;
using System.Text;
using System.Net;

namespace FirebirdSql.Data.NGDS
{
	/// <include file='xmldoc/xdrinputstream.xml' path='doc/member[@name="T:XdrInputStream"]/*'/>
	internal class XdrInputStream : BinaryReader
	{
		#region FIELDS

		private byte[] pad = new byte[4];

		#endregion

		#region CONSTRUCTORS

		public XdrInputStream(Stream input) : base(input)
		{			
		}

		#endregion 

		#region METHODS
			
		public byte[] ReadOpaque(int len)
		{
			byte[] buffer = new byte[len];

			int readed = 0;
			while (readed<len)
			{
				readed += Read(buffer, readed, len-readed);
			}
			Read(pad, 0, ((4 - len) & 3));

			return buffer;
		}

		public byte[] ReadBuffer()
		{
			int len = this.ReadInt();
			byte[] buffer = new byte[len];
			
			int readed = 0;
			while (readed<len)
			{				
				readed += Read(buffer, readed, len-readed);
			}
			Read(pad, 0, ((4 - len) & 3));
			
			return buffer;
		}

		public override string ReadString()
		{
			int len = this.ReadInt();
			byte[] buffer = new byte[len];
						
			Read(buffer, 0, len);
			Read(pad, 0, ((4 - len) & 3));
			
			return Encoding.Default.GetString(buffer);
		}

		public short ReadShort()
		{
			short hostVal = base.ReadInt16();

			return IPAddress.HostToNetworkOrder(hostVal);
		}

		public int ReadInt()
		{
			int hostVal = base.ReadInt32();

			return IPAddress.HostToNetworkOrder(hostVal);
		}

		public long ReadLong()
		{
			long hostVal = base.ReadInt64();

			return IPAddress.HostToNetworkOrder(hostVal);
		}

		public override float ReadSingle()
		{			
			FloatLayout floatValue = new FloatLayout();

			floatValue.f  = base.ReadSingle();;
			floatValue.i0 = IPAddress.HostToNetworkOrder(floatValue.i0);

			return floatValue.f;
		}
		
		public override double ReadDouble()
		{			
			DoubleLayout doubleValue = new DoubleLayout();
			int temp;			

			doubleValue.d = base.ReadDouble();
			doubleValue.i0 = IPAddress.HostToNetworkOrder(doubleValue.i0);
			doubleValue.i4 = IPAddress.HostToNetworkOrder(doubleValue.i4);

			temp = doubleValue.i0;
			doubleValue.i0 = doubleValue.i4;
			doubleValue.i4 = temp;

			return doubleValue.d;
		}		

		#endregion
	}
}
