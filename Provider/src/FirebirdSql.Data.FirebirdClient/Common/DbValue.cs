/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Globalization;
using System.Numerics;
using System.Threading.Tasks;
using FirebirdSql.Data.Types;

namespace FirebirdSql.Data.Common
{
	internal sealed class DbValue
	{
		private StatementBase _statement;
		private DbField _field;
		private object _value;

		public DbField Field
		{
			get { return _field; }
		}

		public DbValue(DbField field, object value)
		{
			_field = field;
			_value = value ?? DBNull.Value;
		}

		public DbValue(StatementBase statement, DbField field, object value)
		{
			_statement = statement;
			_field = field;
			_value = value ?? DBNull.Value;
		}

		public ValueTask<bool> IsDBNull(AsyncWrappingCommonArgs async)
		{
			return ValueTask2.FromResult(TypeHelper.IsDBNull(_value));
		}

		public async ValueTask<object> GetValue(AsyncWrappingCommonArgs async)
		{
			if (await IsDBNull(async).ConfigureAwait(false))
			{
				return DBNull.Value;
			}

			switch (_field.DbDataType)
			{
				case DbDataType.Text:
					if (_statement == null)
					{
						return await GetInt64(async).ConfigureAwait(false);
					}
					else
					{
						return await GetString(async).ConfigureAwait(false);
					}

				case DbDataType.Binary:
					if (_statement == null)
					{
						return await GetInt64(async).ConfigureAwait(false);
					}
					else
					{
						return await GetBinary(async).ConfigureAwait(false);
					}

				case DbDataType.Array:
					if (_statement == null)
					{
						return await GetInt64(async).ConfigureAwait(false);
					}
					else
					{
						return await GetArray(async).ConfigureAwait(false);
					}

				default:
					return _value;
			}
		}

		public void SetValue(object value)
		{
			_value = value;
		}

		public async ValueTask<string> GetString(AsyncWrappingCommonArgs async)
		{
			if (Field.DbDataType == DbDataType.Text && _value is long l)
			{
				_value = await GetClobData(l, async).ConfigureAwait(false);
			}
			if (_value is byte[] bytes)
			{
				return Field.Charset.GetString(bytes);
			}

			return _value.ToString();
		}

		public ValueTask<char> GetChar(AsyncWrappingCommonArgs async)
		{
			return ValueTask2.FromResult(Convert.ToChar(_value, CultureInfo.CurrentCulture));
		}

		public ValueTask<bool> GetBoolean(AsyncWrappingCommonArgs async)
		{
			return ValueTask2.FromResult(Convert.ToBoolean(_value, CultureInfo.InvariantCulture));
		}

		public ValueTask<byte> GetByte(AsyncWrappingCommonArgs async)
		{
			return ValueTask2.FromResult(Convert.ToByte(_value, CultureInfo.InvariantCulture));
		}

		public ValueTask<short> GetInt16(AsyncWrappingCommonArgs async)
		{
			return ValueTask2.FromResult(Convert.ToInt16(_value, CultureInfo.InvariantCulture));
		}

		public ValueTask<int> GetInt32(AsyncWrappingCommonArgs async)
		{
			return ValueTask2.FromResult(Convert.ToInt32(_value, CultureInfo.InvariantCulture));
		}

		public ValueTask<long> GetInt64(AsyncWrappingCommonArgs async)
		{
			return ValueTask2.FromResult(Convert.ToInt64(_value, CultureInfo.InvariantCulture));
		}

		public ValueTask<decimal> GetDecimal(AsyncWrappingCommonArgs async)
		{
			return ValueTask2.FromResult(Convert.ToDecimal(_value, CultureInfo.InvariantCulture));
		}

		public ValueTask<float> GetFloat(AsyncWrappingCommonArgs async)
		{
			return ValueTask2.FromResult(Convert.ToSingle(_value, CultureInfo.InvariantCulture));
		}

		public ValueTask<Guid> GetGuid(AsyncWrappingCommonArgs async)
		{
			return ValueTask2.FromResult(_value switch
			{
				Guid guid => guid,
				byte[] bytes => TypeDecoder.DecodeGuid(bytes),
				_ => throw new InvalidOperationException($"Incorrect {nameof(Guid)} value."),
			});
		}

		public ValueTask<double> GetDouble(AsyncWrappingCommonArgs async)
		{
			return ValueTask2.FromResult(Convert.ToDouble(_value, CultureInfo.InvariantCulture));
		}

		public ValueTask<DateTime> GetDateTime(AsyncWrappingCommonArgs async)
		{
			return ValueTask2.FromResult(_value switch
			{
				TimeSpan ts => new DateTime(0 * 10000L + 621355968000000000 + ts.Ticks),
				DateTimeOffset dto => dto.DateTime,
				FbZonedDateTime zdt => zdt.DateTime,
				FbZonedTime zt => new DateTime(0 * 10000L + 621355968000000000 + zt.Time.Ticks),
				_ => Convert.ToDateTime(_value, CultureInfo.CurrentCulture.DateTimeFormat),
			});
		}

		public async ValueTask<Array> GetArray(AsyncWrappingCommonArgs async)
		{
			if (_value is long l)
			{
				_value = await GetArrayData(l, async).ConfigureAwait(false);
			}

			return (Array)_value;
		}

		public async ValueTask<byte[]> GetBinary(AsyncWrappingCommonArgs async)
		{
			if (_value is long l)
			{
				_value = await GetBlobData(l, async).ConfigureAwait(false);
			}
			if (_value is Guid guid)
			{
				return TypeEncoder.EncodeGuid(guid);
			}

			return (byte[])_value;
		}

		public async ValueTask<int> GetDate(AsyncWrappingCommonArgs async)
		{
			return TypeEncoder.EncodeDate(await GetDateTime(async).ConfigureAwait(false));
		}

		public async ValueTask<int> GetTime(AsyncWrappingCommonArgs async)
		{
			return _value switch
			{
				TimeSpan ts => TypeEncoder.EncodeTime(ts),
				FbZonedTime zt => TypeEncoder.EncodeTime(zt.Time),
				_ => TypeEncoder.EncodeTime(TypeHelper.DateTimeToTimeSpan(await GetDateTime(async).ConfigureAwait(false))),
			};
		}

		public ValueTask<ushort> GetTimeZoneId(AsyncWrappingCommonArgs async)
		{
			{
				if (_value is FbZonedDateTime zdt && TimeZoneMapping.TryGetByName(zdt.TimeZone, out var id))
				{
					return ValueTask2.FromResult(id);
				}
			}
			{
				if (_value is FbZonedTime zt && TimeZoneMapping.TryGetByName(zt.TimeZone, out var id))
				{
					return ValueTask2.FromResult(id);
				}
			}
			throw new InvalidOperationException($"Incorrect time zone value.");
		}

		public ValueTask<FbDecFloat> GetDec16(AsyncWrappingCommonArgs async)
		{
			return ValueTask2.FromResult((FbDecFloat)_value);
		}

		public ValueTask<FbDecFloat> GetDec34(AsyncWrappingCommonArgs async)
		{
			return ValueTask2.FromResult((FbDecFloat)_value);
		}

		public ValueTask<BigInteger> GetInt128(AsyncWrappingCommonArgs async)
		{
			return ValueTask2.FromResult((BigInteger)_value);
		}

		public async ValueTask<byte[]> GetBytes(AsyncWrappingCommonArgs async)
		{
			if (await IsDBNull(async).ConfigureAwait(false))
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
							bytes = await GetBinary(async).ConfigureAwait(false);
						}
						else if (Field.Charset.IsNoneCharset)
						{
							var bvalue = Field.Charset.GetBytes(await GetString(async).ConfigureAwait(false));
							if (bvalue.Length > Field.Length)
							{
								throw IscException.ForErrorCodes(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
							}
							bytes = bvalue;
						}
						else
						{
							var svalue = await GetString(async).ConfigureAwait(false);
							if ((Field.Length % Field.Charset.BytesPerCharacter) == 0 && svalue.Length > Field.CharCount)
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
							bytes = await GetBinary(async).ConfigureAwait(false);
						}
						else if (Field.Charset.IsNoneCharset)
						{
							var bvalue = Field.Charset.GetBytes(await GetString(async).ConfigureAwait(false));
							if (bvalue.Length > Field.Length)
							{
								throw IscException.ForErrorCodes(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
							}
							bytes = bvalue;
						}
						else
						{
							var svalue = await GetString(async).ConfigureAwait(false);
							if ((Field.Length % Field.Charset.BytesPerCharacter) == 0 && svalue.Length > Field.CharCount)
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
					return await GetNumericBytes(async).ConfigureAwait(false);

				case DbDataType.SmallInt:
					return BitConverter.GetBytes(await GetInt16(async).ConfigureAwait(false));

				case DbDataType.Integer:
					return BitConverter.GetBytes(await GetInt32(async).ConfigureAwait(false));

				case DbDataType.Array:
				case DbDataType.Binary:
				case DbDataType.Text:
				case DbDataType.BigInt:
					return BitConverter.GetBytes(await GetInt64(async).ConfigureAwait(false));

				case DbDataType.Float:
					return BitConverter.GetBytes(await GetFloat(async).ConfigureAwait(false));

				case DbDataType.Double:
					return BitConverter.GetBytes(await GetDouble(async).ConfigureAwait(false));

				case DbDataType.Date:
					return BitConverter.GetBytes(await GetDate(async).ConfigureAwait(false));

				case DbDataType.Time:
					return BitConverter.GetBytes(await GetTime(async).ConfigureAwait(false));

				case DbDataType.TimeStamp:
					{
						var dt = await GetDateTime(async).ConfigureAwait(false);
						var date = BitConverter.GetBytes(TypeEncoder.EncodeDate(dt));
						var time = BitConverter.GetBytes(TypeEncoder.EncodeTime(TypeHelper.DateTimeToTimeSpan(dt)));

						var result = new byte[8];
						Buffer.BlockCopy(date, 0, result, 0, date.Length);
						Buffer.BlockCopy(time, 0, result, 4, time.Length);
						return result;
					}

				case DbDataType.Guid:
					return TypeEncoder.EncodeGuid(await GetGuid(async).ConfigureAwait(false));

				case DbDataType.Boolean:
					return BitConverter.GetBytes(await GetBoolean(async).ConfigureAwait(false));

				case DbDataType.TimeStampTZ:
					{
						var dt = await GetDateTime(async).ConfigureAwait(false);
						var date = BitConverter.GetBytes(TypeEncoder.EncodeDate(dt));
						var time = BitConverter.GetBytes(TypeEncoder.EncodeTime(TypeHelper.DateTimeToTimeSpan(dt)));
						var tzId = BitConverter.GetBytes(await GetTimeZoneId(async).ConfigureAwait(false));

						var result = new byte[10];
						Buffer.BlockCopy(date, 0, result, 0, date.Length);
						Buffer.BlockCopy(time, 0, result, 4, time.Length);
						Buffer.BlockCopy(tzId, 0, result, 8, tzId.Length);
						return result;
					}

				case DbDataType.TimeStampTZEx:
					{
						var dt = await GetDateTime(async).ConfigureAwait(false);
						var date = BitConverter.GetBytes(TypeEncoder.EncodeDate(dt));
						var time = BitConverter.GetBytes(TypeEncoder.EncodeTime(TypeHelper.DateTimeToTimeSpan(dt)));
						var tzId = BitConverter.GetBytes(await GetTimeZoneId(async).ConfigureAwait(false));
						var offset = new byte[] { 0, 0 };

						var result = new byte[12];
						Buffer.BlockCopy(date, 0, result, 0, date.Length);
						Buffer.BlockCopy(time, 0, result, 4, time.Length);
						Buffer.BlockCopy(tzId, 0, result, 8, tzId.Length);
						Buffer.BlockCopy(offset, 0, result, 10, offset.Length);
						return result;
					}

				case DbDataType.TimeTZ:
					{
						var time = BitConverter.GetBytes(await GetTime(async).ConfigureAwait(false));
						var tzId = BitConverter.GetBytes(await GetTimeZoneId(async).ConfigureAwait(false));

						var result = new byte[6];
						Buffer.BlockCopy(time, 0, result, 0, time.Length);
						Buffer.BlockCopy(tzId, 0, result, 4, tzId.Length);
						return result;
					}

				case DbDataType.TimeTZEx:
					{
						var time = BitConverter.GetBytes(await GetTime(async).ConfigureAwait(false));
						var tzId = BitConverter.GetBytes(await GetTimeZoneId(async).ConfigureAwait(false));
						var offset = new byte[] { 0, 0 };

						var result = new byte[8];
						Buffer.BlockCopy(time, 0, result, 0, time.Length);
						Buffer.BlockCopy(tzId, 0, result, 4, tzId.Length);
						Buffer.BlockCopy(offset, 0, result, 6, offset.Length);
						return result;
					}

				case DbDataType.Dec16:
					return DecimalCodec.DecFloat16.EncodeDecimal(await GetDec16(async).ConfigureAwait(false));

				case DbDataType.Dec34:
					return DecimalCodec.DecFloat34.EncodeDecimal(await GetDec34(async).ConfigureAwait(false));

				case DbDataType.Int128:
					return Int128Helper.GetBytes(await GetInt128(async).ConfigureAwait(false));

				default:
					throw TypeHelper.InvalidDataType((int)Field.DbDataType);
			}
		}

		private async ValueTask<byte[]> GetNumericBytes(AsyncWrappingCommonArgs async)
		{
			var value = await GetDecimal(async).ConfigureAwait(false);
			var numeric = TypeEncoder.EncodeDecimal(value, Field.NumericScale, Field.DataType);

			switch (_field.SqlType)
			{
				case IscCodes.SQL_SHORT:
					return BitConverter.GetBytes((short)numeric);

				case IscCodes.SQL_LONG:
					return BitConverter.GetBytes((int)numeric);

				case IscCodes.SQL_QUAD:
				case IscCodes.SQL_INT64:
					return BitConverter.GetBytes((long)numeric);

				case IscCodes.SQL_DOUBLE:
				case IscCodes.SQL_D_FLOAT:
					return BitConverter.GetBytes((double)numeric);

				case IscCodes.SQL_INT128:
					return Int128Helper.GetBytes((BigInteger)numeric);

				default:
					return null;
			}
		}

		private Task<string> GetClobData(long blobId, AsyncWrappingCommonArgs async)
		{
			var clob = _statement.CreateBlob(blobId);

			return clob.ReadString(async);
		}

		private Task<byte[]> GetBlobData(long blobId, AsyncWrappingCommonArgs async)
		{
			var blob = _statement.CreateBlob(blobId);

			return blob.Read(async);
		}

		private async Task<Array> GetArrayData(long handle, AsyncWrappingCommonArgs async)
		{
			if (_field.ArrayHandle == null)
			{
				_field.ArrayHandle = await _statement.CreateArray(handle, Field.Relation, Field.Name, async).ConfigureAwait(false);
			}

			var gdsArray = await _statement.CreateArray(_field.ArrayHandle.Descriptor, async).ConfigureAwait(false);

			gdsArray.Handle = handle;
			gdsArray.Database = _statement.Database;
			gdsArray.Transaction = _statement.Transaction;

			return await gdsArray.Read(async).ConfigureAwait(false);
		}
	}
}
