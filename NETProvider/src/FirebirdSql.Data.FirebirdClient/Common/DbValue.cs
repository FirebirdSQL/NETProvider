/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *  All Rights Reserved.
 *   
 *  Contributors:
 *    Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Globalization;

namespace FirebirdSql.Data.Common
{
	internal sealed class DbValue
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
			get { return this.GetValue(); }
			set { this.value = value; }
		}

		#endregion

		#region Constructors

		public DbValue(DbField field, object value)
		{
			this.field = field;
			this.value = value ?? DBNull.Value;
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
			this.value		= value ?? DBNull.Value;
		}

		#endregion

		#region Methods

		public bool IsDBNull()
		{
			return TypeHelper.IsDBNull(this.value);
		}

		public string GetString()
		{
			if (this.Field.DbDataType == DbDataType.Text && this.value is long)
			{
				this.value = this.GetClobData((long)this.value);
			}
			if (this.value is byte[])
			{
				return this.Field.Charset.GetString((byte[])this.value);
			}

			return this.value.ToString();
		}

		public char GetChar()
		{
			return Convert.ToChar(this.value, CultureInfo.CurrentCulture);
		}

		public bool GetBoolean()
		{
			return Convert.ToBoolean(this.value, CultureInfo.InvariantCulture);
		}

		public byte GetByte()
		{
			return Convert.ToByte(this.value, CultureInfo.InvariantCulture);
		}

		public short GetInt16()
		{
			return Convert.ToInt16(this.value, CultureInfo.InvariantCulture);
		}

		public int GetInt32()
		{
			return Convert.ToInt32(this.value, CultureInfo.InvariantCulture);
		}

		public long GetInt64()
		{
			return Convert.ToInt64(this.value, CultureInfo.InvariantCulture);
		}

		public decimal GetDecimal()
		{
			return Convert.ToDecimal(this.value, CultureInfo.InvariantCulture);
		}

		public float GetFloat()
		{
			return Convert.ToSingle(this.value, CultureInfo.InvariantCulture);
		}

		public Guid GetGuid()
		{
			if (this.Value is Guid)
			{
				return (Guid)this.Value;
			}
			else if (this.Value is byte[])
			{
				return new Guid((byte[])this.value);
			}

			throw new InvalidOperationException("Incorrect Guid value");
		}

		public double GetDouble()
		{
			return Convert.ToDouble(this.value, CultureInfo.InvariantCulture);
		}

		public DateTime GetDateTime()
		{
			if (this.value is TimeSpan)
				return new DateTime(0 * 10000L + 621355968000000000 + ((TimeSpan)this.value).Ticks);
			else if (this.value is DateTimeOffset)
				return Convert.ToDateTime(((DateTimeOffset)this.value).DateTime, CultureInfo.CurrentCulture.DateTimeFormat);
			else
				return Convert.ToDateTime(this.value, CultureInfo.CurrentCulture.DateTimeFormat);
		}

		public Array GetArray()
		{
			if (this.value is long)
			{
				this.value = this.GetArrayData((long)this.value);
			}

			return (Array)this.value;
		}

		public byte[] GetBinary()
		{
			if (this.value is long)
			{
				this.value = this.GetBlobData((long)this.value);
			}

			return (byte[])this.value;
		}

		public int GetDate()
		{
			return TypeEncoder.EncodeDate(this.GetDateTime());
		}

		public int GetTime()
		{
			if (this.value is TimeSpan)
				return TypeEncoder.EncodeTime((TimeSpan)this.value);
			else
				return TypeEncoder.EncodeTime(TypeHelper.DateTimeToTimeSpan(this.GetDateTime()));
		}

		public byte[] GetBytes()
		{
			if (this.IsDBNull())
			{
				int length = field.Length;

				if (this.Field.SqlType == IscCodes.SQL_VARYING)
				{
					// Add two bytes more for store	value length
					length += 2;
				}

				return new byte[length];
			}


			switch (this.Field.DbDataType)
			{
				case DbDataType.Char:
					if (this.Field.Charset.IsOctetsCharset)
					{
						return (byte[])this.value;
					}
					else
					{
						string svalue = this.GetString();

						if ((this.Field.Length % this.Field.Charset.BytesPerCharacter) == 0 &&
							svalue.Length > this.Field.CharCount)
						{
							throw new IscException(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
						}

						byte[] buffer = new byte[this.Field.Length];
						for (int i = 0; i < buffer.Length; i++)
						{
							buffer[i] = 32;
						}

						byte[] bytes = this.Field.Charset.GetBytes(svalue);

						Buffer.BlockCopy(bytes, 0, buffer, 0, bytes.Length);

						return buffer;
					}

				case DbDataType.VarChar:
					if (this.Field.Charset.IsOctetsCharset)
					{
						return (byte[])this.value;
					}
					else
					{
						string svalue = this.GetString();

						if ((this.Field.Length % this.Field.Charset.BytesPerCharacter) == 0 &&
							svalue.Length > this.Field.CharCount)
						{
							throw new IscException(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
						}

						byte[] sbuffer = this.Field.Charset.GetBytes(svalue);
						byte[] buffer = new byte[this.Field.Length + 2];

						// Copy	length
						Buffer.BlockCopy(BitConverter.GetBytes((short)sbuffer.Length), 0, buffer, 0, 2);

						// Copy	string value
						Buffer.BlockCopy(sbuffer, 0, buffer, 2, sbuffer.Length);

						return buffer;
					}

				case DbDataType.Numeric:
				case DbDataType.Decimal:
					return this.GetNumericBytes();

				case DbDataType.SmallInt:
					return BitConverter.GetBytes(this.GetInt16());

				case DbDataType.Integer:
					return BitConverter.GetBytes(this.GetInt32());

				case DbDataType.Array:
				case DbDataType.Binary:
				case DbDataType.Text:
				case DbDataType.BigInt:
					return BitConverter.GetBytes(this.GetInt64());

				case DbDataType.Float:
					return BitConverter.GetBytes(this.GetFloat());

				case DbDataType.Double:
					return BitConverter.GetBytes(this.GetDouble());

				case DbDataType.Date:
					return BitConverter.GetBytes(TypeEncoder.EncodeDate(this.GetDateTime()));

				case DbDataType.Time:
					return BitConverter.GetBytes(this.GetTime());

				case DbDataType.TimeStamp:
					var dt = this.GetDateTime();
					byte[] date = BitConverter.GetBytes(TypeEncoder.EncodeDate(dt));
					byte[] time = BitConverter.GetBytes(TypeEncoder.EncodeTime(TypeHelper.DateTimeToTimeSpan(dt)));

					byte[] result = new byte[8];

					Buffer.BlockCopy(date, 0, result, 0, date.Length);
					Buffer.BlockCopy(time, 0, result, 4, time.Length);

					return result;

				case DbDataType.Guid:
					return this.GetGuid().ToByteArray();

				default:
					throw new NotSupportedException("Unknown data type");
			}
		}

		private byte[] GetNumericBytes()
		{
			decimal value = this.GetDecimal();
			object numeric = TypeEncoder.EncodeDecimal(value, this.Field.NumericScale, this.Field.DataType);

			switch (field.SqlType)
			{
				case IscCodes.SQL_SHORT:
					return BitConverter.GetBytes((short)numeric);

				case IscCodes.SQL_LONG:
					return BitConverter.GetBytes((int)numeric);

				case IscCodes.SQL_INT64:
				case IscCodes.SQL_QUAD:
					return BitConverter.GetBytes((long)numeric);

				case IscCodes.SQL_DOUBLE:
					return BitConverter.GetBytes(this.GetDouble());

				default:
					return null;
			}
		}

		#endregion

		#region Private Methods

		private object GetValue()
		{
			if (this.IsDBNull())
			{
				return System.DBNull.Value;
			}

			switch (this.field.DbDataType)
			{
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

		private string GetClobData(long blobId)
		{
			BlobBase clob = this.statement.CreateBlob(blobId);

			return clob.ReadString();
		}

		private byte[] GetBlobData(long blobId)
		{
			BlobBase blob = this.statement.CreateBlob(blobId);

			return blob.Read();
		}

		private Array GetArrayData(long handle)
		{
			if (this.field.ArrayHandle == null)
			{
				this.field.ArrayHandle = this.statement.CreateArray(handle, this.Field.Relation, this.Field.Name);
			}

			ArrayBase gdsArray = this.statement.CreateArray(this.field.ArrayHandle.Descriptor);
			
			gdsArray.Handle			= handle;
			gdsArray.DB				= this.statement.Database;
			gdsArray.Transaction	= this.statement.Transaction;

			return gdsArray.Read();
		}

		#endregion
	}
}
