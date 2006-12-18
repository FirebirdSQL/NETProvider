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
using System.Collections;

namespace FirebirdSql.Data.Firebird.Gds
{
	internal class GdsInetWriter : BinaryWriter
	{	
		#region FIELDS

		private byte[]		fill;
		private byte[]		pad;
		private Encoding	encoding;

		#endregion 

		#region CONSTRUCTORS

		public GdsInetWriter(Stream output, Encoding encoding) : base(output)
		{
			fill			= new byte[32767];
			pad				= new byte[]{0,0,0,0};
			this.encoding	= encoding;

			for (int i = 0; i < fill.Length; i++)
			{
				fill[i] = 32;
			}
		}		

		#endregion

		#region WRITE_METHODS
		 
		public void WriteOpaque(byte[] buffer, int len)
		{
			if (buffer != null && len > 0) 
			{
				Write(buffer, 0, buffer.Length);
				Write(fill, 0, len - buffer.Length);
				Write(pad, 0, ((4 - len) & 3));
			}
		}

		public void WriteBuffer(byte[] buffer)
		{
			this.WriteBuffer(buffer, buffer.Length);
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
			WriteBuffer(encoding.GetBytes(s), encoding.GetByteCount(s));
		}

		public void WriteTyped(int type, byte[] buffer)
		{
			int size;

			if (buffer == null) 
			{
				WriteInt(1);
				Write((byte)type);
				size = 1;
			}
			else 
			{
				size = buffer.Length + 1;
				WriteInt(size);
				Write((byte)type);
				Write(buffer);
			}
			Write(pad, 0, ((4 - size) & 3));
		}

		public void WriteShort(short val)
		{
			Write((int)IPAddress.NetworkToHostOrder((int)val));
		}

		public void WriteInt(int val)
		{
			Write((int)IPAddress.NetworkToHostOrder(val));
		}

		public void WriteLong(long val)
		{
			Write((long)IPAddress.NetworkToHostOrder(val));
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

		public void WriteSlice(GdsArrayDesc desc, Array sourceArray, int length)
		{
			IEnumerator i 		= sourceArray.GetEnumerator();
			int			written = 0;
						
			WriteInt(length);

			while(i.MoveNext())
			{
				switch (desc.DataType)
				{
					case GdsCodes.blr_text:
					case GdsCodes.blr_text2:
					case GdsCodes.blr_cstring:
					case GdsCodes.blr_cstring2:
					{
						// Char
						string value = i.Current != null ? (string)i.Current : String.Empty;
						WriteOpaque(
							encoding.GetBytes(value),
							desc.Length);
						
						written += desc.Length + ((4 - desc.Length) & 3);
					}
					break;

					case GdsCodes.blr_varying:
					case GdsCodes.blr_varying2:
					{	
						// VarChar
						string value = i.Current != null ? (string)i.Current : String.Empty;
						int len = encoding.GetByteCount(value);
						WriteInt(len);
						WriteOpaque(
							encoding.GetBytes(value),
							len);
						
						written += encoding.GetMaxByteCount(desc.Length) +
							((4 - encoding.GetMaxByteCount(desc.Length)) & 3);
					}
					break;
	
					case GdsCodes.blr_short:
					{
						// Short/Smallint
						if (desc.Scale < 0)
						{				
							short svalue = (short)GdsEncodeType.EncodeDecimal(
								Convert.ToDecimal(i.Current),
								desc.Scale,
								GdsCodes.SQL_SHORT);

							WriteShort(svalue);
						}
						else
						{
							WriteShort(Convert.ToInt16(i.Current));
						}
						
						written += desc.Length;
					}
					break;
	
					case GdsCodes.blr_long:
					{
						// Integer
						if (desc.Scale < 0)
						{
							int ivalue = (int)GdsEncodeType.EncodeDecimal(
								Convert.ToDecimal(i.Current),
								desc.Scale,
								GdsCodes.SQL_LONG);

							WriteInt(ivalue);
						}
						else
						{
							WriteInt(Convert.ToInt32(i.Current));
						}
						
						written += desc.Length;
					}
					break;
					
					case GdsCodes.blr_float:
					{
						// Float
						WriteFloat(Convert.ToSingle(i.Current));
						
						written += desc.Length;
					}
					break;
										
					case GdsCodes.blr_double:
					case GdsCodes.blr_d_float:
					{
						// Double
						WriteDouble(Convert.ToDouble(i.Current));
						
						written += desc.Length;
					}
					break;
													
					case GdsCodes.blr_quad:
					case GdsCodes.blr_int64:
					{
						// Long/Quad
						if (desc.Scale < 0)
						{
							long lvalue = (long)GdsEncodeType.EncodeDecimal(
								Convert.ToDecimal(i.Current),
								desc.Scale,
								GdsCodes.SQL_INT64);
							WriteLong(lvalue);
						}
						else
						{
							WriteLong(Convert.ToInt64(i.Current));
						}
						
						written += desc.Length;
					}
					break;
					
					case GdsCodes.blr_timestamp:
					{
						// TimeStamp
						WriteInt(
							GdsEncodeType.EncodeDate(
							Convert.ToDateTime(i.Current)));	// Date
						WriteInt(
							GdsEncodeType.EncodeTime(
							Convert.ToDateTime(i.Current)));	// Time
						
						written += desc.Length;
					}
					break;
					
					case GdsCodes.blr_sql_time:			
					{
						// Time
						WriteInt(
							GdsEncodeType.EncodeTime(
							Convert.ToDateTime(i.Current)));
						
						written += desc.Length;
					}
					break;

					case GdsCodes.blr_sql_date:
					{
						// Date
						WriteInt(
							GdsEncodeType.EncodeDate(
							Convert.ToDateTime(i.Current)));
						
						written += desc.Length;
					}
					break;
					
					default:
						throw new NotSupportedException("Unknown data type");
				}
			}

			// Check partial update
			if (written != length)
			{
				int fillBytes = 0;

				fillBytes = length - written;
				
				switch (desc.DataType)
				{
					case GdsCodes.blr_short:
						fillBytes *= 2;
						break;
					
					case GdsCodes.blr_varying:
					case GdsCodes.blr_varying2:
						int elements = (fillBytes + ((4 - desc.Length) & 3)) / desc.Length;
						fillBytes = elements * 4;
						break;					
				}
				
				byte[] dif = new byte[fillBytes];
				
				// For char datatypes we need to send spaces instead of 0
				switch (desc.DataType)
				{
					case GdsCodes.blr_text:
					case GdsCodes.blr_text2:
					case GdsCodes.blr_cstring:
					case GdsCodes.blr_cstring2:
						System.Array.Copy(fill, dif, fillBytes);
						break;
				}
				
				Write(dif, 0, dif.Length);
			}
		}
		
		public void WriteParameter(GdsField param)
		{
			this.fixNull(param);
			
			try 
			{
				object sqldata = param.SqlData;
				switch (param.SqlType & ~1) 
				{
					case GdsCodes.SQL_TEXT:
					{
						byte[] bytes = encoding.GetBytes(sqldata.ToString());
						this.WriteOpaque(bytes, param.SqlLen);
					}
					break;

					case GdsCodes.SQL_VARYING:
					{
						byte[] bytes = encoding.GetBytes(sqldata.ToString().TrimEnd());
						this.WriteBuffer(bytes, bytes.Length);
						// this.WriteInt(bytes.Length);
						// this.WriteOpaque(bytes, bytes.Length);
					}
					break;
									
					case GdsCodes.SQL_SHORT:
					{
						if (param.SqlScale < 0)
						{
							this.WriteShort(
								Convert.ToInt16(
									GdsEncodeType.EncodeDecimal(
									Convert.ToDecimal(sqldata),
									param.SqlScale,
									param.SqlType)));
						}
						else
						{
							this.WriteShort(Convert.ToInt16(sqldata));
						}
					}
					break;

					case GdsCodes.SQL_LONG:						
					{
						if (param.SqlScale < 0)
						{
							this.WriteInt(
								Convert.ToInt32(
									GdsEncodeType.EncodeDecimal(
									Convert.ToDecimal(sqldata),
									param.SqlScale,
									param.SqlType)));
						}
						else
						{
							this.WriteInt(Convert.ToInt32(sqldata));
						}
					}
					break;

					case GdsCodes.SQL_INT64:
					case GdsCodes.SQL_QUAD:
					{
						if (param.SqlScale < 0)
						{
							this.WriteLong(
								Convert.ToInt64(
									GdsEncodeType.EncodeDecimal(
									Convert.ToDecimal(sqldata),
									param.SqlScale,
									param.SqlType)));
						}
						else
						{
							this.WriteLong(Convert.ToInt64(sqldata));
						}
					}
					break;
				
					case GdsCodes.SQL_FLOAT:
						WriteFloat(Convert.ToSingle(sqldata));
						break;
				
					case GdsCodes.SQL_DOUBLE:
					case GdsCodes.SQL_D_FLOAT:
						WriteDouble(Convert.ToDouble(sqldata));
						break;

					case GdsCodes.SQL_TYPE_DATE:
						WriteInt(GdsEncodeType.EncodeDate(
							Convert.ToDateTime(sqldata)));						
						break;

					case GdsCodes.SQL_TYPE_TIME:
						WriteInt(GdsEncodeType.EncodeTime(
							Convert.ToDateTime(sqldata)));
						break;

					case GdsCodes.SQL_TIMESTAMP:
						WriteInt(GdsEncodeType.EncodeDate(
							Convert.ToDateTime(sqldata)));
						WriteInt(GdsEncodeType.EncodeTime(
							Convert.ToDateTime(sqldata)));
						break;
				
					case GdsCodes.SQL_BLOB:
					case GdsCodes.SQL_ARRAY:
						WriteLong(Convert.ToInt64(sqldata));
						break;
												
					default:
						throw new GdsException("Unknown sql data type: " + param.SqlType);
				}

				WriteInt(param.SqlInd);
			} 
			catch (IOException) 
			{
				throw new GdsException(GdsCodes.isc_net_write_err);
			}
		}

		#endregion

		#region PRIVATE_METHODS

		private void fixNull(GdsField field)
		{
			if ((field.SqlInd == -1) && ((field.SqlData == null)))
			{
				switch (field.SqlType & ~1) 
				{
					case GdsCodes.SQL_TEXT:
					case GdsCodes.SQL_VARYING:
						field.SqlData = String.Empty;
						break;
					
					case GdsCodes.SQL_SHORT:
						field.SqlData = (short)0;
						break;
					
					case GdsCodes.SQL_LONG:
						field.SqlData = (int)0;
						break;
					
					case GdsCodes.SQL_FLOAT:
						field.SqlData = (float)0;
						break;
					
					case GdsCodes.SQL_DOUBLE:
						field.SqlData = (double)0;
						break;
					
					case GdsCodes.SQL_BLOB:
					case GdsCodes.SQL_ARRAY:
					case GdsCodes.SQL_INT64:
					case GdsCodes.SQL_QUAD:					
						field.SqlData = (long)0;
						break;

					case GdsCodes.SQL_TYPE_DATE:
					case GdsCodes.SQL_TYPE_TIME:
					case GdsCodes.SQL_TIMESTAMP:
						field.SqlData = new System.DateTime(0 * 10000L + 621355968000000000);
						break;

					default:
						throw new GdsException("Unknown sql data type: " + field.SqlType);
				}
			}
		}

		#endregion
	}
}