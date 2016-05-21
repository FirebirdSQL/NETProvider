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

		private StatementBase _statement;
		private DbField _field;
		private object _value;

		#endregion

		#region Properties

		public DbField Field
		{
			get { return _field; }
		}

		public object Value
		{
			get { return GetValue(); }
			set { _value = value; }
		}

		#endregion

		#region Constructors

		public DbValue(DbField field, object value)
		{
			_field = field;
			_value = value ?? DBNull.Value;
		}

		public DbValue(StatementBase statement, DbField field)
		{
			_statement = statement;
			_field = field;
			_value = field.Value;
		}

		public DbValue(StatementBase statement, DbField field, object value)
		{
			_statement = statement;
			_field = field;
			_value = value ?? DBNull.Value;
		}

		#endregion

		#region Methods

		public bool IsDBNull()
		{
			return TypeHelper.IsDBNull(_value);
		}

		public string GetString()
		{
			if (Field.DbDataType == DbDataType.Text && _value is long)
			{
				_value = GetClobData((long)_value);
			}
			if (_value is byte[])
			{
				return Field.Charset.GetString((byte[])_value);
			}

			return _value.ToString();
		}

		public char GetChar()
		{
			return Convert.ToChar(_value, CultureInfo.CurrentCulture);
		}

		public bool GetBoolean()
		{
			return Convert.ToBoolean(_value, CultureInfo.InvariantCulture);
		}

		public byte GetByte()
		{
			return Convert.ToByte(_value, CultureInfo.InvariantCulture);
		}

		public short GetInt16()
		{
			return Convert.ToInt16(_value, CultureInfo.InvariantCulture);
		}

		public int GetInt32()
		{
			return Convert.ToInt32(_value, CultureInfo.InvariantCulture);
		}

		public long GetInt64()
		{
			return Convert.ToInt64(_value, CultureInfo.InvariantCulture);
		}

		public decimal GetDecimal()
		{
			return Convert.ToDecimal(_value, CultureInfo.InvariantCulture);
		}

		public float GetFloat()
		{
			return Convert.ToSingle(_value, CultureInfo.InvariantCulture);
		}

		public Guid GetGuid()
		{
			if (Value is Guid)
			{
				return (Guid)Value;
			}
			else if (Value is byte[])
			{
				return new Guid((byte[])_value);
			}

			throw new InvalidOperationException("Incorrect Guid value");
		}

		public double GetDouble()
		{
			return Convert.ToDouble(_value, CultureInfo.InvariantCulture);
		}

		public DateTime GetDateTime()
		{
			if (_value is TimeSpan)
				return new DateTime(0 * 10000L + 621355968000000000 + ((TimeSpan)_value).Ticks);
			else if (_value is DateTimeOffset)
				return Convert.ToDateTime(((DateTimeOffset)_value).DateTime, CultureInfo.CurrentCulture.DateTimeFormat);
			else
				return Convert.ToDateTime(_value, CultureInfo.CurrentCulture.DateTimeFormat);
		}

		public Array GetArray()
		{
			if (_value is long)
			{
				_value = GetArrayData((long)_value);
			}

			return (Array)_value;
		}

		public byte[] GetBinary()
		{
			if (_value is long)
			{
				_value = GetBlobData((long)_value);
			}

			return (byte[])_value;
		}

		public int GetDate()
		{
			return TypeEncoder.EncodeDate(GetDateTime());
		}

		public int GetTime()
		{
			if (_value is TimeSpan)
				return TypeEncoder.EncodeTime((TimeSpan)_value);
			else
				return TypeEncoder.EncodeTime(TypeHelper.DateTimeToTimeSpan(GetDateTime()));
		}

		public byte[] GetBytes()
		{
			if (IsDBNull())
			{
				int length = _field.Length;

				if (Field.SqlType == IscCodes.SQL_VARYING)
				{
					// Add two bytes more for store	value length
					length += 2;
				}

				return new byte[length];
			}


			switch (Field.DbDataType)
			{
				case DbDataType.Char:
					{
						var buffer = new byte[Field.Length];
						byte[] bytes;

						if (Field.Charset.IsOctetsCharset)
						{
							bytes = GetBinary();
						}
						else
						{
							var svalue = GetString();

							if ((Field.Length % Field.Charset.BytesPerCharacter) == 0 &&
								svalue.Length > Field.CharCount)
							{
								throw IscException.ForErrorCodes(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
							}

							bytes = Field.Charset.GetBytes(svalue);
						}

						for (var i = 0; i < buffer.Length; i++)
						{
							buffer[i] = (byte)' ';
						}
						Buffer.BlockCopy(bytes, 0, buffer, 0, bytes.Length);
						return buffer;
					}

				case DbDataType.VarChar:
					{
						var buffer = new byte[Field.Length + 2];
						byte[] bytes;

						if (Field.Charset.IsOctetsCharset)
						{
							bytes = GetBinary();
						}
						else
						{
							var svalue = GetString();

							if ((Field.Length % Field.Charset.BytesPerCharacter) == 0 &&
								svalue.Length > Field.CharCount)
							{
								throw IscException.ForErrorCodes(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
							}

							bytes = Field.Charset.GetBytes(svalue);
						}

						Buffer.BlockCopy(BitConverter.GetBytes((short)bytes.Length), 0, buffer, 0, 2);
						Buffer.BlockCopy(bytes, 0, buffer, 2, bytes.Length);
						return buffer;
					}

				case DbDataType.Numeric:
				case DbDataType.Decimal:
					return GetNumericBytes();

				case DbDataType.SmallInt:
					return BitConverter.GetBytes(GetInt16());

				case DbDataType.Integer:
					return BitConverter.GetBytes(GetInt32());

				case DbDataType.Array:
				case DbDataType.Binary:
				case DbDataType.Text:
				case DbDataType.BigInt:
					return BitConverter.GetBytes(GetInt64());

				case DbDataType.Float:
					return BitConverter.GetBytes(GetFloat());

				case DbDataType.Double:
					return BitConverter.GetBytes(GetDouble());

				case DbDataType.Date:
					return BitConverter.GetBytes(TypeEncoder.EncodeDate(GetDateTime()));

				case DbDataType.Time:
					return BitConverter.GetBytes(GetTime());

				case DbDataType.TimeStamp:
					var dt = GetDateTime();
					var date = BitConverter.GetBytes(TypeEncoder.EncodeDate(dt));
					var time = BitConverter.GetBytes(TypeEncoder.EncodeTime(TypeHelper.DateTimeToTimeSpan(dt)));

					var result = new byte[8];

					Buffer.BlockCopy(date, 0, result, 0, date.Length);
					Buffer.BlockCopy(time, 0, result, 4, time.Length);

					return result;

				case DbDataType.Guid:
					return GetGuid().ToByteArray();

				case DbDataType.Boolean:
					return BitConverter.GetBytes(GetBoolean());

				default:
					throw TypeHelper.InvalidDataType((int)Field.DbDataType);
			}
		}

		private byte[] GetNumericBytes()
		{
			decimal value = GetDecimal();
			object numeric = TypeEncoder.EncodeDecimal(value, Field.NumericScale, Field.DataType);

			switch (_field.SqlType)
			{
				case IscCodes.SQL_SHORT:
					return BitConverter.GetBytes((short)numeric);

				case IscCodes.SQL_LONG:
					return BitConverter.GetBytes((int)numeric);

				case IscCodes.SQL_INT64:
				case IscCodes.SQL_QUAD:
					return BitConverter.GetBytes((long)numeric);

				case IscCodes.SQL_DOUBLE:
					return BitConverter.GetBytes(GetDouble());

				default:
					return null;
			}
		}

		#endregion

		#region Private Methods

		private object GetValue()
		{
			if (IsDBNull())
			{
				return DBNull.Value;
			}

			switch (_field.DbDataType)
			{
				case DbDataType.Text:
					if (_statement == null)
					{
						return GetInt64();
					}
					else
					{
						return GetString();
					}

				case DbDataType.Binary:
					if (_statement == null)
					{
						return GetInt64();
					}
					else
					{
						return GetBinary();
					}

				case DbDataType.Array:
					if (_statement == null)
					{
						return GetInt64();
					}
					else
					{
						return GetArray();
					}

				default:
					return _value;
			}
		}

		private string GetClobData(long blobId)
		{
			BlobBase clob = _statement.CreateBlob(blobId);

			return clob.ReadString();
		}

		private byte[] GetBlobData(long blobId)
		{
			BlobBase blob = _statement.CreateBlob(blobId);

			return blob.Read();
		}

		private Array GetArrayData(long handle)
		{
			if (_field.ArrayHandle == null)
			{
				_field.ArrayHandle = _statement.CreateArray(handle, Field.Relation, Field.Name);
			}

			ArrayBase gdsArray = _statement.CreateArray(_field.ArrayHandle.Descriptor);

			gdsArray.Handle = handle;
			gdsArray.DB = _statement.Database;
			gdsArray.Transaction = _statement.Transaction;

			return gdsArray.Read();
		}

		#endregion
	}
}
