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
using System.Text;
using System.Net;
using System.IO;

namespace FirebirdSql.Data.Firebird.Gds
{
	internal class GdsInetReader : BinaryReader
	{
		#region FIELDS

		private byte[]		pad;
		private Encoding	encoding;
		private string		defaultCharset;

		#endregion

		#region CONTRUCTORS

		public GdsInetReader(Stream input, Encoding encoding, string defaultCharset) : base(input)
		{
			this.pad			= new byte[4];
			this.encoding		= encoding;
			this.defaultCharset = defaultCharset;
		}

		#endregion

		#region METHODS
		
		public object ReadValue(GdsField field)
		{
			object fieldValue = null;
			switch (field.SqlType & ~1)
			{
				case GdsCodes.SQL_TEXT:
				{
					byte[] bytes	= ReadOpaque(field.SqlLen);
					string s		= String.Empty;
					if (defaultCharset == "NONE")
					{
						s = field.Charset.Encoding.GetString(bytes);
					}
					else
					{
						s = encoding.GetString(bytes);
					}
					/*
					if (s.Length > field.CharCount)
					{
						s = s.Substring(0, field.CharCount);
					}
					*/
					fieldValue = s;
				}
				break;
					
				case GdsCodes.SQL_VARYING:
				{
					byte[] bytes	= ReadOpaque(ReadInt());
					string s		= String.Empty;
					if (defaultCharset == "NONE")
					{
						s = field.Charset.Encoding.GetString(bytes);
					}
					else
					{
						s = encoding.GetString(bytes);
					}
					/*
					if (s.Length > field.CharCount)
					{
						s = s.Substring(0, field.CharCount);
					}
					*/
					fieldValue = s.TrimEnd();
				}
				break;
					
				case GdsCodes.SQL_SHORT:
					fieldValue = (short)ReadInt();
					if (field.SqlScale < 0)
					{
						fieldValue = GdsDecodeType.DecodeDecimal(
							fieldValue,
							field.SqlScale,
							field.SqlType);
					}
					break;
					
				case GdsCodes.SQL_LONG:
					fieldValue = ReadInt();
					if (field.SqlScale < 0)
					{
						fieldValue = GdsDecodeType.DecodeDecimal(
							fieldValue,
							field.SqlScale,
							field.SqlType);
					}
					break;
					
				case GdsCodes.SQL_QUAD:
				case GdsCodes.SQL_INT64:
					fieldValue = ReadLong();
					if (field.SqlScale < 0)
					{
						fieldValue = GdsDecodeType.DecodeDecimal(
							fieldValue,
							field.SqlScale,
							field.SqlType);
					}
					break;

				case GdsCodes.SQL_FLOAT:
					fieldValue = ReadSingle();
					break;
					
				case GdsCodes.SQL_DOUBLE:	
					fieldValue = ReadDouble();
					break;
		
				case GdsCodes.SQL_TIMESTAMP:
					DateTime date = GdsDecodeType.DecodeDate(ReadInt());
					DateTime time = GdsDecodeType.DecodeTime(ReadInt());

					fieldValue = new System.DateTime(
						date.Year, date.Month, date.Day,
						time.Hour,time.Minute, time.Second, time.Millisecond);
					break;
										
				case GdsCodes.SQL_TYPE_TIME:
					fieldValue = GdsDecodeType.DecodeTime(ReadInt());
					break;
					
				case GdsCodes.SQL_TYPE_DATE:
					fieldValue = GdsDecodeType.DecodeDate(ReadInt());
					break;
					
				case GdsCodes.SQL_BLOB:
				case GdsCodes.SQL_ARRAY:
					fieldValue = ReadLong();
					break;
			}

			int sqlInd = ReadInt();

			if (sqlInd == 0) 
			{
				return fieldValue;
			}
			else if (sqlInd == -1) 
			{
				return null;
			}
			else 
			{
				throw new GdsException("invalid sqlind value: " + sqlInd);
			}
		}

		public byte[] ReadOpaque(int len)
		{
			byte[] buffer = new byte[len];

			int readed = 0;
			while (readed < len)
			{
				readed += Read(buffer, readed, len-readed);
			}
			Read(pad, 0, ((4 - len) & 3));

			return buffer;
		}

		public byte[] ReadBuffer()
		{
			int 	len 	= this.ReadInt();
			byte[] 	buffer 	= new byte[len];
			
			int readed = 0;
			while (readed < len)
			{				
				readed += Read(buffer, readed, len - readed);
			}
			Read(pad, 0, ((4 - len) & 3));
			
			return buffer;
		}

		public byte[] ReadSlice(GdsArrayDesc desc)
		{			
			int realLength 		= ReadInt();
			int totalElements	= (realLength + ((4 - desc.Length) & 3)) / desc.Length;
			int	currentElement	= 0;
			int readed 			= 0;
			
			// Skip Length bytes
			int skip = ReadInt();
			
			if (desc.DataType == GdsCodes.blr_short)
			{
				realLength = realLength * desc.Length;
			}

			byte[] slice = new byte[realLength];
			
			while (readed < realLength)
			{
				switch (desc.DataType)
				{
					case GdsCodes.blr_text:
					case GdsCodes.blr_text2:
					case GdsCodes.blr_cstring:
					case GdsCodes.blr_cstring2:
					{												
						// Skip fill bytes
						if (((4 - desc.Length) & 3) != 0)
						{
							readed += Read(slice, readed, desc.Length);
							Read(pad, 0, ((4 - desc.Length) & 3));
						}
						else
						{
							readed += Read(slice, readed, realLength);	
						}
					}
					break;

					case GdsCodes.blr_varying:
					case GdsCodes.blr_varying2:
					{						
						byte[] tmp = ReadBytes(4);												
						tmp.CopyTo(slice, readed);
						readed += 4;
						
						int itemLength = BitConverter.ToInt32(tmp, 0);						
						itemLength = IPAddress.HostToNetworkOrder(itemLength);
						
						if (itemLength != 0)
						{
							readed += Read(slice, readed, itemLength);
							
							// Skip fill bytes
							if (((4 - itemLength) & 3) != 0)
							{
								Read(pad, 0, ((4 - itemLength) & 3));
							}
						}
						currentElement++;
						
						if (currentElement == totalElements)
						{
							readed = realLength;
						}
					}
					break;
									
					default:
						readed += Read(slice, readed, realLength - readed);
						break;
				}
			}
						
			return slice;
		}

		public override string ReadString()
		{
			int len = this.ReadInt();
			byte[] buffer = new byte[len];
						
			Read(buffer, 0, len);
			Read(pad, 0, ((4 - len) & 3));
			
			return encoding.GetString(buffer);
		}

		public short ReadShort()
		{
			return IPAddress.HostToNetworkOrder(base.ReadInt16());
		}

		public int ReadInt()
		{
			return IPAddress.HostToNetworkOrder(base.ReadInt32());
		}

		public long ReadLong()
		{
			return IPAddress.HostToNetworkOrder(base.ReadInt64());
		}

		public override float ReadSingle()
		{			
			FloatLayout floatValue = new FloatLayout();
			floatValue.i0 = IPAddress.HostToNetworkOrder(base.ReadInt32());

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
