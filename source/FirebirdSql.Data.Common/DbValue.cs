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
using System.Globalization;

namespace FirebirdSql.Data.Common
{
#if (SINGLE_DLL)
	internal
#else
	public
#endif
	sealed class DbValue
	{
		#region Fields

		private StatementBase	statement;
		private DbField			field;
		private object			value;

		#endregion

		#region Properties

		public DbField Field
		{
			get { return this.field; }
		}

		public object Value
		{
			get { return this.getValue(); }
			set { this.value = value; }
		}

		#endregion
		
		#region Constructor

		public DbValue(DbField field, object value)
		{
			this.field = field;
			this.value = value == null ? System.DBNull.Value : value;
		}

		public DbValue(StatementBase statement, DbField field)
		{
			this.statement	= statement;
			this.field		= field;
			this.value		= field.Value;
		}

		public DbValue(StatementBase statement, DbField field, object value)
		{
			this.statement	= statement;
			this.field		= field;
			this.value		= value == null ? System.DBNull.Value : value;
		}

		#endregion

		#region Methods

		public bool IsDBNull()
		{
			if (this.value == null || this.value == System.DBNull.Value)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public string GetString()
		{
			if (this.Field.DbDataType == DbDataType.Text)
			{
				// This is for ascii blobs
				this.value = this.getClobData((long)this.value);
			}
			return this.value.ToString();
		}

		public char GetChar()
		{
			return Convert.ToChar(this.value);
		}

		public byte GetByte()
		{
			return Convert.ToByte(this.value, CultureInfo.InvariantCulture.NumberFormat);
		}

		public short GetInt16()
		{
			return Convert.ToInt16(this.value, CultureInfo.InvariantCulture.NumberFormat);
		}

		public int GetInt32()
		{
			return Convert.ToInt32(this.value, CultureInfo.InvariantCulture.NumberFormat);
		}

		public long GetInt64()
		{
			return Convert.ToInt64(this.value, CultureInfo.InvariantCulture.NumberFormat);
		}

		public decimal GetDecimal()
		{
			return Convert.ToDecimal(this.value, CultureInfo.InvariantCulture.NumberFormat);
		}

		public float GetFloat()
		{
			return Convert.ToSingle(this.value, CultureInfo.InvariantCulture.NumberFormat);
		}

		public double GetDouble()
		{
			return Convert.ToDouble(this.value, CultureInfo.InvariantCulture.NumberFormat);
		}

		public DateTime GetDateTime()
		{
			return Convert.ToDateTime(this.value, CultureInfo.CurrentUICulture.DateTimeFormat);
		}

		public Array GetArray()
		{
			if (this.value is long)
			{
				this.value = this.getArrayData((long)this.value);
			}
			return (Array)this.value;
		}

		public byte[] GetBinary()
		{
			if (this.value is long)
			{
				this.value = this.getBlobData((long)this.value);
			}
			return (byte[])this.value;
		}

		public decimal DecodeDecimal()
		{
			int scale	= this.field.NumericScale;
			int type	= this.field.SqlType;

			return TypeDecoder.DecodeDecimal(this.value, scale, type);	
		}

		public int EncodeDate()
		{
			return TypeEncoder.EncodeDate(this.GetDateTime());
		}

		public int EncodeTime()
		{
			return TypeEncoder.EncodeTime(this.GetDateTime());
		}

		#endregion

		#region Private Methods

		private object getValue()
		{
			if (this.IsDBNull())
			{
				return System.DBNull.Value;
			}

			switch (this.field.DbDataType)
			{
					/*
				case DbDataType.Char:
				case DbDataType.VarChar:
					return this.GetString();

				case DbDataType.SmallInt:
					return this.GetInt16();

				case DbDataType.Integer:
					return this.GetInt32();

				case DbDataType.BigInt:
					return this.GetInt64();

				case DbDataType.Decimal:
				case DbDataType.Numeric:
					return this.GetDecimal();

				case DbDataType.Float:
					return this.GetFloat();

				case DbDataType.Double:
					return this.GetDouble();
			
				case DbDataType.Date:
				case DbDataType.Time:
				case DbDataType.TimeStamp:
					return this.GetDateTime();
					*/

				case DbDataType.Text:
					if (this.statement == null)
					{
						return this.GetInt64();
					}
					else
					{
						return this.GetString();
					}

				case DbDataType.Binary:
					if (this.statement == null)
					{
						return this.GetInt64();
					}
					else
					{
						return this.GetBinary();
					}
							
				case DbDataType.Array:
					if (this.statement == null)
					{
						return this.GetInt64();
					}
					else
					{
						return this.GetArray();
					}
														
				default:
					return this.value;
			}
		}

		private string getClobData(long blobId)
		{
			BlobBase clob = this.statement.CreateBlob(blobId);
			
			return clob.ReadString();
		}

		private byte[] getBlobData(long blobId)
		{
			BlobBase blob = this.statement.CreateBlob(blobId);
			
			return blob.Read();
		}

		private Array getArrayData(long handle)
		{
			ArrayBase gdsArray = this.statement.CreateArray(
				handle,
				this.Field.Relation,
				this.Field.Name);
			
			return gdsArray.Read();
		}

		#endregion
	}
}