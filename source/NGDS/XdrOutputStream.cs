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
using System.Collections;
using System.IO;
using System.Text;
using System.Net;

using FirebirdSql.Data.INGDS;

namespace FirebirdSql.Data.NGDS
{
	/// <include file='xmldoc/xdroutputstream.xml' path='doc/member[@name="T:XdrOutputStream"]/*'/>
	internal class XdrOutputStream : BinaryWriter
	{	
		#region FIELDS

		private byte[] fill	= new byte[32767];
		private byte[] pad 	= {0,0,0,0};

		#endregion 

		#region CONSTRUCTORS

		public XdrOutputStream(Stream output) : base(output)
		{			
			for (int i = 0; i < fill.Length; i++)
			{
				fill[i] = 32;
			}
		}

		#endregion

		#region METHODS
		 
		public void WriteOpaque(byte[] buffer, int len)
		{
			if (buffer != null && len > 0) 
			{
				Write(buffer, 0, buffer.Length);
				Write(fill, 0, len - buffer.Length);
				Write(pad, 0, ((4 - len) & 3));
			}
		}

		public void WriteBuffer(byte[] buffer, int len)
		{
			WriteInt(len);
			if (buffer != null && len > 0) 
			{
				Write(buffer, 0, len);
				Write(pad, 0, ((4 - len) & 3));
			}
		}

		public void WriteBlobBuffer(byte[] buffer)
		{
			int len = buffer.Length ; // 2 for short for buffer length
			
			if (len > short.MaxValue) 
			{
				throw(new IOException()); //Need a value???
			}
			WriteInt(len + 2);
			WriteInt(len + 2); //bizarre but true! three copies of the length
			Write((byte)((len >> 0) & 0xff));
			Write((byte)((len >> 8) & 0xff));
			Write(buffer, 0, len);
						
			Write(pad, 0, ((4 - len + 2) & 3));
		}

		public void WriteString(string s)
		{							
			WriteString(s, Encoding.Default);
		}
	    
		public void WriteString(string s, Encoding encoding)
		{	        				        
			byte[] buffer;
	        
			buffer = encoding.GetBytes(s);

			WriteBuffer(buffer, encoding.GetByteCount(s));
		}
	    
		public void WriteSet(int type, ArrayList s)
		{
			if (s == null) 
			{
				WriteInt(1);
				Write((byte)type); //e.g. gds.isc_tpb_version3
			}
			else 
			{
				WriteInt(s.Count + 1);
				Write((byte)type);
				IEnumerator i = s.GetEnumerator();
				while (i.MoveNext()) 
				{
					int n = (int)i.Current;
					Write((byte)n);
				}
				Write(pad, 0, ((4 - (s.Count + 1)) & 3));				
			}
		}

		internal void WriteTyped(int type, IXdrable item)
		{
			int size;
			if (item == null) 
			{
				WriteInt(1);
				Write((byte)type); //e.g. gds.isc_tpb_version3
				size = 1;
			}
			else 
			{
				size = item.Length + 1;
				WriteInt(size);
				Write((byte)type);
				item.Write(this);
			}
			Write(pad, 0, ((4 - size) & 3));
		}

		public void WriteShort(short val)
		{			
			int netVal = IPAddress.NetworkToHostOrder((int)val);

			Write(netVal);
		}

		public void WriteInt(int val)
		{
			int netVal = IPAddress.NetworkToHostOrder(val);

			Write(netVal);
		}

		public void WriteLong(long val)
		{
			long netVal = IPAddress.NetworkToHostOrder(val);

			Write(netVal);
		}

		public void WriteFloat(float val)
		{
			FloatLayout floatValue = new FloatLayout();			

			floatValue.f = val;
			floatValue.i0 = IPAddress.NetworkToHostOrder(floatValue.i0);

			Write(floatValue.f);
		}

		public void WriteDouble(double val)
		{
			DoubleLayout doubleValue = new DoubleLayout();
			int temp;			

			doubleValue.d = val;
			doubleValue.i0 = IPAddress.NetworkToHostOrder(doubleValue.i0);
			doubleValue.i4 = IPAddress.NetworkToHostOrder(doubleValue.i4);

			temp = doubleValue.i0;
			doubleValue.i0 = doubleValue.i4;
			doubleValue.i4 = temp;

			Write(doubleValue.d);
		}

		#endregion
	}
}
