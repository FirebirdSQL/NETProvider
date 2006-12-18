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
using System.Net;
using System.Text;
using System.Collections;

namespace FirebirdSql.Data.Firebird.Gds
{
	internal class GdsArray : GdsSlice
	{
		#region CONSTRUCTORS

		public GdsArray(
			GdsDbAttachment db,
			GdsTransaction transaction,
			long handle, 
			string tableName, 
			string fieldName) : base(db, transaction, handle, tableName, fieldName)
		{
			// Lookup array information
			this.LookupBounds();
		}

		public GdsArray(
			GdsDbAttachment db,
			GdsTransaction transaction,
			string tableName, 
			string fieldName) : this(db, transaction, 0, tableName, fieldName)
		{
			// Lookup array information
			this.LookupDesc();
		}

		#endregion

		#region METHODS

		public System.Array Read()
		{
			byte[] slice = GetSlice(this.getSliceLength(true));
			
			return decodeSlice(slice);
		}

		public void Write(System.Array source_array)
		{
			this.SetDesc(source_array);
			this.PutSlice(source_array, this.getSliceLength(false));
		}

		#endregion

		#region PRIVATE_METHODS
				
		private int getSliceLength(bool read)
		{
			Encoding	encoding		= this.DB.Parameters.Charset.Encoding;
			int 		length 			= 0;
			int			elements		= 0;

			for (int i = 0; i < this.Description.Dimensions; i++)
			{
				GdsArrayBound bound = this.Description.Bounds[i];
				
				elements += bound.LowerBound * bound.UpperBound;
			}

			switch (this.Description.DataType)
			{
				case GdsCodes.blr_text:
				case GdsCodes.blr_text2:
				case GdsCodes.blr_cstring:
				case GdsCodes.blr_cstring2:
				case GdsCodes.blr_varying:
				case GdsCodes.blr_varying2:
					// Char & VarChar
					length =  elements * encoding.GetMaxByteCount(this.Description.Length);
					length += elements * ((4 - encoding.GetMaxByteCount(this.Description.Length)) & 3);
					break;

				case GdsCodes.blr_short:
					length = elements * this.Description.Length;
					if (read)
					{
						length *= 2;
					}
					break;
				
				default:
					length = elements * this.Description.Length;
					break;
			}						
			
			return length;
		}
						
		private System.Array decodeSlice(byte[] slice)
		{
			System.Array 	sliceData		= null;			
			int				slicePosition	= 0;
			FbDbType		type			= FbDbType.Array;
			Type			systemType		= null;
			int 			temp 			= 0;
			Encoding		encoding		= DB.Parameters.Charset.Encoding;
			int[]			lengths 		= new int[this.Description.Dimensions];
			int[]			lowerBounds		= new int[this.Description.Dimensions];			

			// Get upper and lower bounds of each dimension
			for (int i = 0; i < this.Description.Dimensions; i++)
			{
				lowerBounds[i] 	= this.Description.Bounds[i].LowerBound;
				lengths[i] 		= this.Description.Bounds[i].UpperBound;
			}
			
			sliceData = getArrayFromDesc(ref type, ref systemType, lengths, lowerBounds);

			System.Array tempData = System.Array.CreateInstance(systemType, sliceData.Length);

			for (int i = 0; i < tempData.Length; i++)
			{
				if (slicePosition >= slice.Length)
				{
					break;
				}
				int item_length = this.Description.Length;
				
				switch(type)
				{							
					case FbDbType.Text:
					{
						// Char
						string strValue = encoding.GetString(slice, slicePosition, item_length);
												
						tempData.SetValue(strValue, i);
					}
					break;
					
					case FbDbType.VarChar:
					{
						// VarChar						
						item_length 	= BitConverter.ToInt32(slice, slicePosition);
						item_length 	= IPAddress.HostToNetworkOrder(item_length);
						slicePosition 	+= 4;
						
						string strValue = encoding.GetString(slice, slicePosition, item_length);
												
						tempData.SetValue(strValue, i);
					}
					break;
					
					case FbDbType.SmallInt:
					{
						// Smallint
						int sintValue = BitConverter.ToInt32(slice, slicePosition);
						sintValue = IPAddress.HostToNetworkOrder(sintValue);
	
						slicePosition += 2;
	
						if (this.Description.Scale < 0)		
						{
							decimal dvalue = GdsDecodeType.DecodeDecimal(
														sintValue, 
														this.Description.Scale,
														GdsCodes.SQL_SHORT);
							tempData.SetValue(dvalue, i);
						}
						else
						{							
							tempData.SetValue(Convert.ToInt16(sintValue), i);
						}
					}
					break;
	
					case FbDbType.Integer:
					{
						// Integer
						int intValue = BitConverter.ToInt32(slice, slicePosition);
						intValue = IPAddress.HostToNetworkOrder(intValue);
					
						if (this.Description.Scale < 0)		
						{								
							decimal dvalue = GdsDecodeType.DecodeDecimal(
														intValue, 
														this.Description.Scale,
														GdsCodes.SQL_LONG);
							tempData.SetValue(dvalue, i);
						}
						else
						{							
							tempData.SetValue(intValue, i);
						}
					}
					break;
	
					case FbDbType.BigInt:
					{
						// BigInt/Long
						long bintValue = BitConverter.ToInt64(slice, slicePosition);
						bintValue = IPAddress.HostToNetworkOrder(bintValue);
					
						if (this.Description.Scale < 0)		
						{
							decimal dvalue = GdsDecodeType.DecodeDecimal(
														bintValue, 
														this.Description.Scale,
														GdsCodes.SQL_INT64);
							tempData.SetValue(dvalue, i);
						}
						else
						{							
							tempData.SetValue(bintValue, i);
						}
					}
					break;
					
					case FbDbType.Double:
					{
						// Double						
						DoubleLayout dlayout = new DoubleLayout();
			
						dlayout.d	= BitConverter.ToDouble(slice, slicePosition);
						dlayout.i0	= IPAddress.HostToNetworkOrder(dlayout.i0);
						dlayout.i4	= IPAddress.HostToNetworkOrder(dlayout.i4);
						
						temp 		= dlayout.i0;
						dlayout.i0 	= dlayout.i4;
						dlayout.i4 	= temp;
			
						tempData.SetValue(dlayout.d, i);
					}
					break;
	
					case FbDbType.Float:
					{
						// Float
						FloatLayout flayout = new FloatLayout();
						flayout.i0 = IPAddress.HostToNetworkOrder(BitConverter.ToInt32(slice, slicePosition));
					
						tempData.SetValue(flayout.f, i);
					}
					break;
										
					case FbDbType.TimeStamp:
					{
						// TimeStamp
						int idate	= BitConverter.ToInt32(slice, slicePosition);
						idate		= IPAddress.HostToNetworkOrder(idate);
						
						int itime	= BitConverter.ToInt32(slice, slicePosition + 4);
						itime		= IPAddress.HostToNetworkOrder(itime);
					
						DateTime date = GdsDecodeType.DecodeDate(idate);
						DateTime time = GdsDecodeType.DecodeTime(itime);

						DateTime timestamp = new System.DateTime(
										date.Year, date.Month, date.Day,
										time.Hour,time.Minute, time.Second, time.Millisecond);
					
						
						tempData.SetValue(timestamp, i);
					}
					break;
					
					case FbDbType.Time:
					{
						// Time
						int itime 	= BitConverter.ToInt32(slice, slicePosition);
						itime		= IPAddress.HostToNetworkOrder(itime);
					
						DateTime time = GdsDecodeType.DecodeTime(itime);					
						
						tempData.SetValue(time, i);
					}
					break;
					
					case FbDbType.Date:
					{
						// Date
						int idate 	= BitConverter.ToInt32(slice, slicePosition);
						idate		= IPAddress.HostToNetworkOrder(idate);
					
						DateTime date = GdsDecodeType.DecodeDate(idate);
						
						tempData.SetValue(date, i);
					}
					break;
				}
				
				slicePosition += item_length;
			}
			
			if (systemType.IsPrimitive)
			{
				// For primitive types we can use System.Buffer to copy generated data to destination array
				System.Buffer.BlockCopy(tempData, 0, sliceData, 0, System.Buffer.ByteLength(tempData));
			}
			else
			{
				sliceData = tempData;	
			}
			
			return sliceData;
		}		
						
		private System.Array getArrayFromDesc(ref FbDbType type, ref Type systemType, int[] lengths, int[] lowerBounds)
		{
			switch (this.Description.DataType)
			{
				case GdsCodes.blr_text:
				case GdsCodes.blr_text2:
				case GdsCodes.blr_cstring:
				case GdsCodes.blr_cstring2:
					// Char
					type 		= FbDbType.Text;
					systemType 	= typeof(System.String);
					break;

				case GdsCodes.blr_varying:
				case GdsCodes.blr_varying2:
					// VarChar
					type = FbDbType.VarChar;
					systemType 	= typeof(System.String);
					break;

				case GdsCodes.blr_short:
					// Short/Smallint
					type = FbDbType.SmallInt;
					if (this.Description.Scale < 0)
					{
						systemType 	= typeof(System.Decimal);
					}
					else
					{
						systemType 	= typeof(System.Int16);
					}
					break;

				case GdsCodes.blr_long:
					// Integer
					type = FbDbType.Integer;
					if (this.Description.Scale < 0)
					{
						systemType 	= typeof(System.Decimal);
					}
					else
					{
						systemType 	= typeof(System.Int32);
					}
					break;
				
				case GdsCodes.blr_float:
					// Float
					type = FbDbType.Float;
					systemType 	= typeof(System.Single);
					break;
									
				case GdsCodes.blr_double:
				case GdsCodes.blr_d_float:
					// Double
					type = FbDbType.Double;
					systemType 	= typeof(System.Double);
					break;
												
				case GdsCodes.blr_quad:
				case GdsCodes.blr_int64:
					// Long/Quad
					type = FbDbType.BigInt;
					if (this.Description.Scale < 0)
					{
						systemType 	= typeof(System.Decimal);
					}
					else
					{
						systemType 	= typeof(System.Int64);
					}
					break;
				
				case GdsCodes.blr_timestamp:
					// Timestamp
					type = FbDbType.TimeStamp;
					systemType 	= typeof(System.DateTime);
					break;

				case GdsCodes.blr_sql_time:			
					// Time
					type = FbDbType.Time;
					systemType 	= typeof(System.DateTime);
					break;

				case GdsCodes.blr_sql_date:				
					// Date
					type = FbDbType.Date;
					systemType 	= typeof(System.DateTime);
					break;
				
				default:
					throw new NotSupportedException("Unknown data type");
			}
			
			return System.Array.CreateInstance(systemType, lengths, lowerBounds);
		}

		#endregion
	}	
}
