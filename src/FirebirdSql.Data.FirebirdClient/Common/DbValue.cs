/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
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
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Types;

namespace FirebirdSql.Data.Common;

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

	public bool IsDBNull()
	{
		return TypeHelper.IsDBNull(_value);
	}

	public object GetValue()
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
	public async ValueTask<object> GetValueAsync(CancellationToken cancellationToken = default)
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
					return await GetStringAsync(cancellationToken).ConfigureAwait(false);
				}

			case DbDataType.Binary:
				if (_statement == null)
				{
					return GetInt64();
				}
				else
				{
					return await GetBinaryAsync(cancellationToken).ConfigureAwait(false);
				}

			case DbDataType.Array:
				if (_statement == null)
				{
					return GetInt64();
				}
				else
				{
					return await GetArrayAsync(cancellationToken).ConfigureAwait(false);
				}

			default:
				return _value;
		}
	}

	public void SetValue(object value)
	{
		_value = value;
	}

	public string GetString()
	{
		if (Field.DbDataType == DbDataType.Text && _value is long l)
		{
			_value = GetClobData(l);
		}

		if (_value is byte[] bytes)
		{
			return Field.Charset.GetString(bytes);
		}
		return _value.ToString();
	}
	public async ValueTask<string> GetStringAsync(CancellationToken cancellationToken = default)
	{
		if (Field.DbDataType == DbDataType.Text && _value is long l)
		{
			_value = await GetClobDataAsync(l, cancellationToken).ConfigureAwait(false);
		}

		if (_value is byte[] bytes)
		{
			return Field.Charset.GetString(bytes);
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
		return _value switch
		{
			Guid guid => guid,
			byte[] bytes => TypeDecoder.DecodeGuid(bytes),
			_ => throw new InvalidOperationException($"Incorrect {nameof(Guid)} value."),
		};
	}

	public double GetDouble()
	{
		return Convert.ToDouble(_value, CultureInfo.InvariantCulture);
	}

	public DateTime GetDateTime()
	{
		return _value switch
		{
			DateTimeOffset dto => dto.DateTime,
			FbZonedDateTime zdt => zdt.DateTime,
			_ => Convert.ToDateTime(_value, CultureInfo.CurrentCulture.DateTimeFormat),
		};
	}

	public TimeSpan GetTimeSpan()
	{
		return (TimeSpan)_value;
	}

	public FbDecFloat GetDecFloat()
	{
		return (FbDecFloat)_value;
	}

	public BigInteger GetInt128()
	{
		return (BigInteger)_value;
	}

	public FbZonedDateTime GetZonedDateTime()
	{
		return (FbZonedDateTime)_value;
	}

	public FbZonedTime GetZonedTime()
	{
		return (FbZonedTime)_value;
	}

	public Array GetArray()
	{
		if (_value is long l)
		{
			_value = GetArrayData(l);
		}

		return (Array)_value;
	}
	public async ValueTask<Array> GetArrayAsync(CancellationToken cancellationToken = default)
	{
		if (_value is long l)
		{
			_value = await GetArrayDataAsync(l, cancellationToken).ConfigureAwait(false);
		}

		return (Array)_value;
	}

	public byte[] GetBinary()
	{
		if (_value is long l)
		{
			_value = GetBlobData(l);
		}
		if (_value is Guid guid)
		{
			return TypeEncoder.EncodeGuid(guid);
		}

		return (byte[])_value;
	}
	public async ValueTask<byte[]> GetBinaryAsync(CancellationToken cancellationToken = default)
	{
		if (_value is long l)
		{
			_value = await GetBlobDataAsync(l, cancellationToken).ConfigureAwait(false);
		}
		if (_value is Guid guid)
		{
			return TypeEncoder.EncodeGuid(guid);
		}

		return (byte[])_value;
	}

	public int GetDate()
	{
		return _value switch
		{
#if NET6_0_OR_GREATER
			DateOnly @do => TypeEncoder.EncodeDate(@do),
#endif
			_ => TypeEncoder.EncodeDate(GetDateTime()),
		};
	}

	public int GetTime()
	{
		return _value switch
		{
			TimeSpan ts => TypeEncoder.EncodeTime(ts),
			FbZonedTime zt => TypeEncoder.EncodeTime(zt.Time),
#if NET6_0_OR_GREATER
			TimeOnly to => TypeEncoder.EncodeTime(to),
#endif
			_ => TypeEncoder.EncodeTime(TypeHelper.DateTimeTimeToTimeSpan(GetDateTime())),
		};
	}

	public ushort GetTimeZoneId()
	{
		{
			if (_value is FbZonedDateTime zdt && TimeZoneMapping.TryGetByName(zdt.TimeZone, out var id))
			{
				return id;
			}
		}
		{
			if (_value is FbZonedTime zt && TimeZoneMapping.TryGetByName(zt.TimeZone, out var id))
			{
				return id;
			}
		}
		throw new InvalidOperationException($"Incorrect time zone value.");
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
					else if (Field.Charset.IsNoneCharset)
					{
						var bvalue = Field.Charset.GetBytes(GetString());
						if (bvalue.Length > Field.Length)
						{
							throw IscException.ForErrorCodes(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
						}
						bytes = bvalue;
					}
					else
					{
						var svalue = GetString();
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
						bytes = GetBinary();
					}
					else if (Field.Charset.IsNoneCharset)
					{
						var bvalue = Field.Charset.GetBytes(GetString());
						if (bvalue.Length > Field.Length)
						{
							throw IscException.ForErrorCodes(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
						}
						bytes = bvalue;
					}
					else
					{
						var svalue = GetString();
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
				return BitConverter.GetBytes(GetDate());

			case DbDataType.Time:
				return BitConverter.GetBytes(GetTime());

			case DbDataType.TimeStamp:
				{
					var dt = GetDateTime();
					var date = BitConverter.GetBytes(TypeEncoder.EncodeDate(dt));
					var time = BitConverter.GetBytes(TypeEncoder.EncodeTime(TypeHelper.DateTimeTimeToTimeSpan(dt)));

					var result = new byte[8];
					Buffer.BlockCopy(date, 0, result, 0, date.Length);
					Buffer.BlockCopy(time, 0, result, 4, time.Length);
					return result;
				}

			case DbDataType.Guid:
				{
					var bytes = TypeEncoder.EncodeGuid(GetGuid());
					byte[] buffer;
					if (Field.SqlType == IscCodes.SQL_VARYING)
					{
						buffer = new byte[bytes.Length + 2];
						Buffer.BlockCopy(BitConverter.GetBytes((short)bytes.Length), 0, buffer, 0, 2);
						Buffer.BlockCopy(bytes, 0, buffer, 2, bytes.Length);
					}
					else
					{
						buffer = new byte[bytes.Length];
						Buffer.BlockCopy(bytes, 0, buffer, 0, bytes.Length);
					}
					return buffer;
				}

			case DbDataType.Boolean:
				return BitConverter.GetBytes(GetBoolean());

			case DbDataType.TimeStampTZ:
				{
					var dt = GetDateTime();
					var date = BitConverter.GetBytes(TypeEncoder.EncodeDate(dt));
					var time = BitConverter.GetBytes(TypeEncoder.EncodeTime(TypeHelper.DateTimeTimeToTimeSpan(dt)));
					var tzId = BitConverter.GetBytes(GetTimeZoneId());

					var result = new byte[10];
					Buffer.BlockCopy(date, 0, result, 0, date.Length);
					Buffer.BlockCopy(time, 0, result, 4, time.Length);
					Buffer.BlockCopy(tzId, 0, result, 8, tzId.Length);
					return result;
				}

			case DbDataType.TimeStampTZEx:
				{
					var dt = GetDateTime();
					var date = BitConverter.GetBytes(TypeEncoder.EncodeDate(dt));
					var time = BitConverter.GetBytes(TypeEncoder.EncodeTime(TypeHelper.DateTimeTimeToTimeSpan(dt)));
					var tzId = BitConverter.GetBytes(GetTimeZoneId());
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
					var time = BitConverter.GetBytes(GetTime());
					var tzId = BitConverter.GetBytes(GetTimeZoneId());

					var result = new byte[6];
					Buffer.BlockCopy(time, 0, result, 0, time.Length);
					Buffer.BlockCopy(tzId, 0, result, 4, tzId.Length);
					return result;
				}

			case DbDataType.TimeTZEx:
				{
					var time = BitConverter.GetBytes(GetTime());
					var tzId = BitConverter.GetBytes(GetTimeZoneId());
					var offset = new byte[] { 0, 0 };

					var result = new byte[8];
					Buffer.BlockCopy(time, 0, result, 0, time.Length);
					Buffer.BlockCopy(tzId, 0, result, 4, tzId.Length);
					Buffer.BlockCopy(offset, 0, result, 6, offset.Length);
					return result;
				}

			case DbDataType.Dec16:
				return DecimalCodec.DecFloat16.EncodeDecimal(GetDecFloat());

			case DbDataType.Dec34:
				return DecimalCodec.DecFloat34.EncodeDecimal(GetDecFloat());

			case DbDataType.Int128:
				return Int128Helper.GetBytes(GetInt128());

			default:
				throw TypeHelper.InvalidDataType((int)Field.DbDataType);
		}
	}
	public async ValueTask<byte[]> GetBytesAsync(CancellationToken cancellationToken = default)
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
						bytes = await GetBinaryAsync(cancellationToken).ConfigureAwait(false);
					}
					else if (Field.Charset.IsNoneCharset)
					{
						var bvalue = Field.Charset.GetBytes(await GetStringAsync(cancellationToken).ConfigureAwait(false));
						if (bvalue.Length > Field.Length)
						{
							throw IscException.ForErrorCodes(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
						}
						bytes = bvalue;
					}
					else
					{
						var svalue = await GetStringAsync(cancellationToken).ConfigureAwait(false);
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
						bytes = await GetBinaryAsync(cancellationToken).ConfigureAwait(false);
					}
					else if (Field.Charset.IsNoneCharset)
					{
						var bvalue = Field.Charset.GetBytes(await GetStringAsync(cancellationToken).ConfigureAwait(false));
						if (bvalue.Length > Field.Length)
						{
							throw IscException.ForErrorCodes(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
						}
						bytes = bvalue;
					}
					else
					{
						var svalue = await GetStringAsync(cancellationToken).ConfigureAwait(false);
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
				return BitConverter.GetBytes(GetDate());

			case DbDataType.Time:
				return BitConverter.GetBytes(GetTime());

			case DbDataType.TimeStamp:
				{
					var dt = GetDateTime();
					var date = BitConverter.GetBytes(TypeEncoder.EncodeDate(dt));
					var time = BitConverter.GetBytes(TypeEncoder.EncodeTime(TypeHelper.DateTimeTimeToTimeSpan(dt)));

					var result = new byte[8];
					Buffer.BlockCopy(date, 0, result, 0, date.Length);
					Buffer.BlockCopy(time, 0, result, 4, time.Length);
					return result;
				}

			case DbDataType.Guid:
				{
					var bytes = TypeEncoder.EncodeGuid(GetGuid());
					byte[] buffer;
					if (Field.SqlType == IscCodes.SQL_VARYING)
					{
						buffer = new byte[bytes.Length + 2];
						Buffer.BlockCopy(BitConverter.GetBytes((short)bytes.Length), 0, buffer, 0, 2);
						Buffer.BlockCopy(bytes, 0, buffer, 2, bytes.Length);
					}
					else
					{
						buffer = new byte[bytes.Length];
						Buffer.BlockCopy(bytes, 0, buffer, 0, bytes.Length);
					}
					return buffer;
				}

			case DbDataType.Boolean:
				return BitConverter.GetBytes(GetBoolean());

			case DbDataType.TimeStampTZ:
				{
					var dt = GetDateTime();
					var date = BitConverter.GetBytes(TypeEncoder.EncodeDate(dt));
					var time = BitConverter.GetBytes(TypeEncoder.EncodeTime(TypeHelper.DateTimeTimeToTimeSpan(dt)));
					var tzId = BitConverter.GetBytes(GetTimeZoneId());

					var result = new byte[10];
					Buffer.BlockCopy(date, 0, result, 0, date.Length);
					Buffer.BlockCopy(time, 0, result, 4, time.Length);
					Buffer.BlockCopy(tzId, 0, result, 8, tzId.Length);
					return result;
				}

			case DbDataType.TimeStampTZEx:
				{
					var dt = GetDateTime();
					var date = BitConverter.GetBytes(TypeEncoder.EncodeDate(dt));
					var time = BitConverter.GetBytes(TypeEncoder.EncodeTime(TypeHelper.DateTimeTimeToTimeSpan(dt)));
					var tzId = BitConverter.GetBytes(GetTimeZoneId());
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
					var time = BitConverter.GetBytes(GetTime());
					var tzId = BitConverter.GetBytes(GetTimeZoneId());

					var result = new byte[6];
					Buffer.BlockCopy(time, 0, result, 0, time.Length);
					Buffer.BlockCopy(tzId, 0, result, 4, tzId.Length);
					return result;
				}

			case DbDataType.TimeTZEx:
				{
					var time = BitConverter.GetBytes(GetTime());
					var tzId = BitConverter.GetBytes(GetTimeZoneId());
					var offset = new byte[] { 0, 0 };

					var result = new byte[8];
					Buffer.BlockCopy(time, 0, result, 0, time.Length);
					Buffer.BlockCopy(tzId, 0, result, 4, tzId.Length);
					Buffer.BlockCopy(offset, 0, result, 6, offset.Length);
					return result;
				}

			case DbDataType.Dec16:
				return DecimalCodec.DecFloat16.EncodeDecimal(GetDecFloat());

			case DbDataType.Dec34:
				return DecimalCodec.DecFloat34.EncodeDecimal(GetDecFloat());

			case DbDataType.Int128:
				return Int128Helper.GetBytes(GetInt128());

			default:
				throw TypeHelper.InvalidDataType((int)Field.DbDataType);
		}
	}

	private byte[] GetNumericBytes()
	{
		var value = GetDecimal();
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

	private string GetClobData(long blobId)
	{
		var clob = _statement.CreateBlob(blobId);
		return clob.ReadString();
	}
	private ValueTask<string> GetClobDataAsync(long blobId, CancellationToken cancellationToken = default)
	{
		var clob = _statement.CreateBlob(blobId);
		return clob.ReadStringAsync(cancellationToken);
	}

	private byte[] GetBlobData(long blobId)
	{
		var blob = _statement.CreateBlob(blobId);
		return blob.Read();
	}
	private ValueTask<byte[]> GetBlobDataAsync(long blobId, CancellationToken cancellationToken = default)
	{
		var blob = _statement.CreateBlob(blobId);
		return blob.ReadAsync(cancellationToken);
	}

	private Array GetArrayData(long handle)
	{
		if (_field.ArrayHandle == null)
		{
			_field.ArrayHandle = _statement.CreateArray(handle, Field.Relation, Field.Name);
		}

		var gdsArray = _statement.CreateArray(_field.ArrayHandle.Descriptor);
		gdsArray.Handle = handle;
		gdsArray.Database = _statement.Database;
		gdsArray.Transaction = _statement.Transaction;
		return gdsArray.Read();
	}
	private async ValueTask<Array> GetArrayDataAsync(long handle, CancellationToken cancellationToken = default)
	{
		if (_field.ArrayHandle == null)
		{
			_field.ArrayHandle = await _statement.CreateArrayAsync(handle, Field.Relation, Field.Name, cancellationToken).ConfigureAwait(false);
		}

		var gdsArray = await _statement.CreateArrayAsync(_field.ArrayHandle.Descriptor, cancellationToken).ConfigureAwait(false);
		gdsArray.Handle = handle;
		gdsArray.Database = _statement.Database;
		gdsArray.Transaction = _statement.Transaction;
		return await gdsArray.ReadAsync(cancellationToken).ConfigureAwait(false);
	}
}
