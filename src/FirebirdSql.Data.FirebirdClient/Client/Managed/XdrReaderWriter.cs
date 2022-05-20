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

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;
using FirebirdSql.Data.Types;

namespace FirebirdSql.Data.Client.Managed;

sealed class XdrReaderWriter : IXdrReader, IXdrWriter
{
	readonly IDataProvider _dataProvider;
	readonly Charset _charset;

	byte[] _smallBuffer;

	public XdrReaderWriter(IDataProvider dataProvider, Charset charset)
	{
		_dataProvider = dataProvider;
		_charset = charset;

		_smallBuffer = new byte[8];
	}

	public XdrReaderWriter(IDataProvider dataProvider)
		: this(dataProvider, Charset.DefaultCharset)
	{ }

	#region Read

	public byte[] ReadBytes(byte[] buffer, int count)
	{
		if (count > 0)
		{
			var toRead = count;
			var currentlyRead = -1;
			while (toRead > 0 && currentlyRead != 0)
			{
				toRead -= (currentlyRead = _dataProvider.Read(buffer, count - toRead, toRead));
			}
			if (currentlyRead == 0)
			{
				if (_dataProvider is ITracksIOFailure tracksIOFailure)
				{
					tracksIOFailure.IOFailed = true;
				}
				throw new IOException($"Missing {toRead} bytes to fill total {count}.");
			}
		}
		return buffer;
	}
	public async ValueTask<byte[]> ReadBytesAsync(byte[] buffer, int count, CancellationToken cancellationToken = default)
	{
		if (count > 0)
		{
			var toRead = count;
			var currentlyRead = -1;
			while (toRead > 0 && currentlyRead != 0)
			{
				toRead -= (currentlyRead = await _dataProvider.ReadAsync(buffer, count - toRead, toRead, cancellationToken).ConfigureAwait(false));
			}
			if (currentlyRead == 0)
			{
				if (_dataProvider is ITracksIOFailure tracksIOFailure)
				{
					tracksIOFailure.IOFailed = true;
				}
				throw new IOException($"Missing {toRead} bytes to fill total {count}.");
			}
		}
		return buffer;
	}

	public byte[] ReadOpaque(int length)
	{
		var buffer = new byte[length];
		ReadBytes(buffer, length);
		ReadPad((4 - length) & 3);
		return buffer;
	}
	public async ValueTask<byte[]> ReadOpaqueAsync(int length, CancellationToken cancellationToken = default)
	{
		var buffer = new byte[length];
		await ReadBytesAsync(buffer, length, cancellationToken).ConfigureAwait(false);
		await ReadPadAsync((4 - length) & 3, cancellationToken).ConfigureAwait(false);
		return buffer;
	}

	public byte[] ReadBuffer()
	{
		return ReadOpaque((ushort)ReadInt32());
	}
	public async ValueTask<byte[]> ReadBufferAsync(CancellationToken cancellationToken = default)
	{
		return await ReadOpaqueAsync((ushort)await ReadInt32Async(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
	}

	public string ReadString()
	{
		return ReadString(_charset);
	}
	public ValueTask<string> ReadStringAsync(CancellationToken cancellationToken = default)
	{
		return ReadStringAsync(_charset, cancellationToken);
	}

	public string ReadString(int length)
	{
		return ReadString(_charset, length);
	}
	public ValueTask<string> ReadStringAsync(int length, CancellationToken cancellationToken = default)
	{
		return ReadStringAsync(_charset, length, cancellationToken);
	}

	public string ReadString(Charset charset)
	{
		return ReadString(charset, ReadInt32());
	}
	public async ValueTask<string> ReadStringAsync(Charset charset, CancellationToken cancellationToken = default)
	{
		return await ReadStringAsync(charset, await ReadInt32Async(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
	}

	public string ReadString(Charset charset, int length)
	{
		var buffer = ReadOpaque(length);
		return charset.GetString(buffer, 0, buffer.Length);
	}
	public async ValueTask<string> ReadStringAsync(Charset charset, int length, CancellationToken cancellationToken = default)
	{
		var buffer = await ReadOpaqueAsync(length, cancellationToken).ConfigureAwait(false);
		return charset.GetString(buffer, 0, buffer.Length);
	}

	public short ReadInt16()
	{
		return Convert.ToInt16(ReadInt32());
	}
	public async ValueTask<short> ReadInt16Async(CancellationToken cancellationToken = default)
	{
		return Convert.ToInt16(await ReadInt32Async(cancellationToken).ConfigureAwait(false));
	}

	public int ReadInt32()
	{
		ReadBytes(_smallBuffer, 4);
		return TypeDecoder.DecodeInt32(_smallBuffer);
	}
	public async ValueTask<int> ReadInt32Async(CancellationToken cancellationToken = default)
	{
		await ReadBytesAsync(_smallBuffer, 4, cancellationToken).ConfigureAwait(false);
		return TypeDecoder.DecodeInt32(_smallBuffer);
	}

	public long ReadInt64()
	{
		ReadBytes(_smallBuffer, 8);
		return TypeDecoder.DecodeInt64(_smallBuffer);
	}
	public async ValueTask<long> ReadInt64Async(CancellationToken cancellationToken = default)
	{
		await ReadBytesAsync(_smallBuffer, 8, cancellationToken).ConfigureAwait(false);
		return TypeDecoder.DecodeInt64(_smallBuffer);
	}

	public Guid ReadGuid(int sqlType)
	{
		if (sqlType == IscCodes.SQL_VARYING)
		{
			return TypeDecoder.DecodeGuid(ReadBuffer());
		}
		else
		{
			return TypeDecoder.DecodeGuid(ReadOpaque(16));
		}
	}
	public async ValueTask<Guid> ReadGuidAsync(int sqlType, CancellationToken cancellationToken = default)
	{
		if (sqlType == IscCodes.SQL_VARYING)
		{
			return TypeDecoder.DecodeGuid(await ReadBufferAsync(cancellationToken).ConfigureAwait(false));
		}
		else
		{
			return TypeDecoder.DecodeGuid(await ReadOpaqueAsync(16, cancellationToken).ConfigureAwait(false));
		}
	}

	public float ReadSingle()
	{
		return BitConverter.ToSingle(BitConverter.GetBytes(ReadInt32()), 0);
	}
	public async ValueTask<float> ReadSingleAsync(CancellationToken cancellationToken = default)
	{
		return BitConverter.ToSingle(BitConverter.GetBytes(await ReadInt32Async(cancellationToken).ConfigureAwait(false)), 0);
	}

	public double ReadDouble()
	{
		return BitConverter.ToDouble(BitConverter.GetBytes(ReadInt64()), 0);
	}
	public async ValueTask<double> ReadDoubleAsync(CancellationToken cancellationToken = default)
	{
		return BitConverter.ToDouble(BitConverter.GetBytes(await ReadInt64Async(cancellationToken).ConfigureAwait(false)), 0);
	}

	public DateTime ReadDateTime()
	{
		var date = ReadDate();
		var time = ReadTime();
		return date.Add(time);
	}
	public async ValueTask<DateTime> ReadDateTimeAsync(CancellationToken cancellationToken = default)
	{
		var date = await ReadDateAsync(cancellationToken).ConfigureAwait(false);
		var time = await ReadTimeAsync(cancellationToken).ConfigureAwait(false);
		return date.Add(time);
	}

	public DateTime ReadDate()
	{
		return TypeDecoder.DecodeDate(ReadInt32());
	}
	public async ValueTask<DateTime> ReadDateAsync(CancellationToken cancellationToken = default)
	{
		return TypeDecoder.DecodeDate(await ReadInt32Async(cancellationToken).ConfigureAwait(false));
	}

	public TimeSpan ReadTime()
	{
		return TypeDecoder.DecodeTime(ReadInt32());
	}
	public async ValueTask<TimeSpan> ReadTimeAsync(CancellationToken cancellationToken = default)
	{
		return TypeDecoder.DecodeTime(await ReadInt32Async(cancellationToken).ConfigureAwait(false));
	}

	public decimal ReadDecimal(int type, int scale)
	{
		switch (type & ~1)
		{
			case IscCodes.SQL_SHORT:
				return TypeDecoder.DecodeDecimal(ReadInt16(), scale, type);
			case IscCodes.SQL_LONG:
				return TypeDecoder.DecodeDecimal(ReadInt32(), scale, type);
			case IscCodes.SQL_QUAD:
			case IscCodes.SQL_INT64:
				return TypeDecoder.DecodeDecimal(ReadInt64(), scale, type);
			case IscCodes.SQL_DOUBLE:
			case IscCodes.SQL_D_FLOAT:
				return TypeDecoder.DecodeDecimal(ReadDouble(), scale, type);
			case IscCodes.SQL_INT128:
				return TypeDecoder.DecodeDecimal(ReadInt128(), scale, type);
			default:
				throw new ArgumentOutOfRangeException(nameof(type), $"{nameof(type)}={type}");
		}
	}
	public async ValueTask<decimal> ReadDecimalAsync(int type, int scale, CancellationToken cancellationToken = default)
	{
		switch (type & ~1)
		{
			case IscCodes.SQL_SHORT:
				return TypeDecoder.DecodeDecimal(await ReadInt16Async(cancellationToken).ConfigureAwait(false), scale, type);
			case IscCodes.SQL_LONG:
				return TypeDecoder.DecodeDecimal(await ReadInt32Async(cancellationToken).ConfigureAwait(false), scale, type);
			case IscCodes.SQL_QUAD:
			case IscCodes.SQL_INT64:
				return TypeDecoder.DecodeDecimal(await ReadInt64Async(cancellationToken).ConfigureAwait(false), scale, type);
			case IscCodes.SQL_DOUBLE:
			case IscCodes.SQL_D_FLOAT:
				return TypeDecoder.DecodeDecimal(await ReadDoubleAsync(cancellationToken).ConfigureAwait(false), scale, type);
			case IscCodes.SQL_INT128:
				return TypeDecoder.DecodeDecimal(await ReadInt128Async(cancellationToken).ConfigureAwait(false), scale, type);
			default:
				throw new ArgumentOutOfRangeException(nameof(type), $"{nameof(type)}={type}");
		}
	}

	public bool ReadBoolean()
	{
		return TypeDecoder.DecodeBoolean(ReadOpaque(1));
	}
	public async ValueTask<bool> ReadBooleanAsync(CancellationToken cancellationToken = default)
	{
		return TypeDecoder.DecodeBoolean(await ReadOpaqueAsync(1, cancellationToken).ConfigureAwait(false));
	}

	public FbZonedDateTime ReadZonedDateTime(bool isExtended)
	{
		var dt = ReadDateTime();
		dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
		return TypeHelper.CreateZonedDateTime(dt, (ushort)ReadInt16(), isExtended ? ReadInt16() : (short?)null);
	}
	public async ValueTask<FbZonedDateTime> ReadZonedDateTimeAsync(bool isExtended, CancellationToken cancellationToken = default)
	{
		var dt = await ReadDateTimeAsync(cancellationToken).ConfigureAwait(false);
		dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
		return TypeHelper.CreateZonedDateTime(dt, (ushort)await ReadInt16Async(cancellationToken).ConfigureAwait(false), isExtended ? await ReadInt16Async(cancellationToken).ConfigureAwait(false) : (short?)null);
	}

	public FbZonedTime ReadZonedTime(bool isExtended)
	{
		return TypeHelper.CreateZonedTime(ReadTime(), (ushort)ReadInt16(), isExtended ? ReadInt16() : (short?)null);
	}
	public async ValueTask<FbZonedTime> ReadZonedTimeAsync(bool isExtended, CancellationToken cancellationToken = default)
	{
		return TypeHelper.CreateZonedTime(await ReadTimeAsync(cancellationToken).ConfigureAwait(false), (ushort)await ReadInt16Async(cancellationToken).ConfigureAwait(false), isExtended ? await ReadInt16Async(cancellationToken).ConfigureAwait(false) : (short?)null);
	}

	public FbDecFloat ReadDec16()
	{
		return TypeDecoder.DecodeDec16(ReadOpaque(8));
	}
	public async ValueTask<FbDecFloat> ReadDec16Async(CancellationToken cancellationToken = default)
	{
		return TypeDecoder.DecodeDec16(await ReadOpaqueAsync(8, cancellationToken).ConfigureAwait(false));
	}

	public FbDecFloat ReadDec34()
	{
		return TypeDecoder.DecodeDec34(ReadOpaque(16));
	}
	public async ValueTask<FbDecFloat> ReadDec34Async(CancellationToken cancellationToken = default)
	{
		return TypeDecoder.DecodeDec34(await ReadOpaqueAsync(16, cancellationToken).ConfigureAwait(false));
	}

	public BigInteger ReadInt128()
	{
		return TypeDecoder.DecodeInt128(ReadOpaque(16));
	}
	public async ValueTask<BigInteger> ReadInt128Async(CancellationToken cancellationToken = default)
	{
		return TypeDecoder.DecodeInt128(await ReadOpaqueAsync(16, cancellationToken).ConfigureAwait(false));
	}

	public IscException ReadStatusVector()
	{
		IscException exception = null;
		var eof = false;

		while (!eof)
		{
			var arg = ReadInt32();

			switch (arg)
			{
				case IscCodes.isc_arg_gds:
				default:
					var er = ReadInt32();
					if (er != 0)
					{
						if (exception == null)
						{
							exception = IscException.ForBuilding();
						}
						exception.Errors.Add(new IscError(arg, er));
					}
					break;

				case IscCodes.isc_arg_end:
					exception?.BuildExceptionData();
					eof = true;
					break;

				case IscCodes.isc_arg_interpreted:
				case IscCodes.isc_arg_string:
				case IscCodes.isc_arg_sql_state:
					exception.Errors.Add(new IscError(arg, ReadString()));
					break;

				case IscCodes.isc_arg_number:
					exception.Errors.Add(new IscError(arg, ReadInt32()));
					break;
			}
		}

		return exception;
	}
	public async ValueTask<IscException> ReadStatusVectorAsync(CancellationToken cancellationToken = default)
	{
		IscException exception = null;
		var eof = false;

		while (!eof)
		{
			var arg = await ReadInt32Async(cancellationToken).ConfigureAwait(false);

			switch (arg)
			{
				case IscCodes.isc_arg_gds:
				default:
					var er = await ReadInt32Async(cancellationToken).ConfigureAwait(false);
					if (er != 0)
					{
						if (exception == null)
						{
							exception = IscException.ForBuilding();
						}
						exception.Errors.Add(new IscError(arg, er));
					}
					break;

				case IscCodes.isc_arg_end:
					exception?.BuildExceptionData();
					eof = true;
					break;

				case IscCodes.isc_arg_interpreted:
				case IscCodes.isc_arg_string:
				case IscCodes.isc_arg_sql_state:
					exception.Errors.Add(new IscError(arg, await ReadStringAsync(cancellationToken).ConfigureAwait(false)));
					break;

				case IscCodes.isc_arg_number:
					exception.Errors.Add(new IscError(arg, await ReadInt32Async(cancellationToken).ConfigureAwait(false)));
					break;
			}
		}

		return exception;
	}

	/* loop	as long	as we are receiving	dummy packets, just
	 * throwing	them away--note	that if	we are a server	we won't
	 * be receiving	them, but it is	better to check	for	them at
	 * this	level rather than try to catch them	in all places where
	 * this	routine	is called
	 */
	public int ReadOperation()
	{
		int operation;
		do
		{
			operation = ReadInt32();
		} while (operation == IscCodes.op_dummy);
		return operation;
	}
	public async ValueTask<int> ReadOperationAsync(CancellationToken cancellationToken = default)
	{
		int operation;
		do
		{
			operation = await ReadInt32Async(cancellationToken).ConfigureAwait(false);
		} while (operation == IscCodes.op_dummy);
		return operation;
	}

	#endregion

	#region Write

	public void Flush()
	{
		_dataProvider.Flush();
	}
	public ValueTask FlushAsync(CancellationToken cancellationToken = default)
	{
		return _dataProvider.FlushAsync(cancellationToken);
	}

	public void WriteBytes(byte[] buffer, int count)
	{
		_dataProvider.Write(buffer, 0, count);
	}
	public ValueTask WriteBytesAsync(byte[] buffer, int count, CancellationToken cancellationToken = default)
	{
		return _dataProvider.WriteAsync(buffer, 0, count, cancellationToken);
	}

	public void WriteOpaque(byte[] buffer)
	{
		WriteOpaque(buffer, buffer.Length);
	}
	public ValueTask WriteOpaqueAsync(byte[] buffer, CancellationToken cancellationToken = default)
	{
		return WriteOpaqueAsync(buffer, buffer.Length, cancellationToken);
	}

	public void WriteOpaque(byte[] buffer, int length)
	{
		if (buffer != null && length > 0)
		{
			_dataProvider.Write(buffer, 0, buffer.Length);
			WriteFill(length - buffer.Length);
			WritePad((4 - length) & 3);
		}
	}
	public async ValueTask WriteOpaqueAsync(byte[] buffer, int length, CancellationToken cancellationToken = default)
	{
		if (buffer != null && length > 0)
		{
			await _dataProvider.WriteAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
			await WriteFillAsync(length - buffer.Length, cancellationToken).ConfigureAwait(false);
			await WritePadAsync((4 - length) & 3, cancellationToken).ConfigureAwait(false);
		}
	}

	public void WriteBuffer(byte[] buffer)
	{
		WriteBuffer(buffer, buffer?.Length ?? 0);
	}
	public ValueTask WriteBufferAsync(byte[] buffer, CancellationToken cancellationToken = default)
	{
		return WriteBufferAsync(buffer, buffer?.Length ?? 0, cancellationToken);
	}

	public void WriteBuffer(byte[] buffer, int length)
	{
		Write(length);
		if (buffer != null && length > 0)
		{
			_dataProvider.Write(buffer, 0, length);
			WritePad((4 - length) & 3);
		}
	}
	public async ValueTask WriteBufferAsync(byte[] buffer, int length, CancellationToken cancellationToken = default)
	{
		await WriteAsync(length, cancellationToken).ConfigureAwait(false);
		if (buffer != null && length > 0)
		{
			await _dataProvider.WriteAsync(buffer, 0, length, cancellationToken).ConfigureAwait(false);
			await WritePadAsync((4 - length) & 3, cancellationToken).ConfigureAwait(false);
		}
	}

	public void WriteBlobBuffer(byte[] buffer)
	{
		var length = buffer.Length; // 2 for short for buffer length
		if (length > short.MaxValue)
			throw new IOException("Blob buffer too big.");
		Write(length + 2);
		Write(length + 2);  //bizarre but true! three copies of the length
		_dataProvider.Write(new[] { (byte)((length >> 0) & 0xff), (byte)((length >> 8) & 0xff) }, 0, 2);
		_dataProvider.Write(buffer, 0, length);
		WritePad((4 - length + 2) & 3);
	}
	public async ValueTask WriteBlobBufferAsync(byte[] buffer, CancellationToken cancellationToken = default)
	{
		var length = buffer.Length; // 2 for short for buffer length
		if (length > short.MaxValue)
			throw new IOException("Blob buffer too big.");
		await WriteAsync(length + 2, cancellationToken).ConfigureAwait(false);
		await WriteAsync(length + 2, cancellationToken).ConfigureAwait(false);  //bizarre but true! three copies of the length
		await _dataProvider.WriteAsync(new[] { (byte)((length >> 0) & 0xff), (byte)((length >> 8) & 0xff) }, 0, 2, cancellationToken).ConfigureAwait(false);
		await _dataProvider.WriteAsync(buffer, 0, length, cancellationToken).ConfigureAwait(false);
		await WritePadAsync((4 - length + 2) & 3, cancellationToken).ConfigureAwait(false);
	}

	public void WriteTyped(int type, byte[] buffer)
	{
		int length;
		if (buffer == null)
		{
			Write(1);
			_dataProvider.Write(new[] { (byte)type }, 0, 1);
			length = 1;
		}
		else
		{
			length = buffer.Length + 1;
			Write(length);
			_dataProvider.Write(new[] { (byte)type }, 0, 1);
			_dataProvider.Write(buffer, 0, buffer.Length);
		}
		WritePad((4 - length) & 3);
	}
	public async ValueTask WriteTypedAsync(int type, byte[] buffer, CancellationToken cancellationToken = default)
	{
		int length;
		if (buffer == null)
		{
			await WriteAsync(1, cancellationToken).ConfigureAwait(false);
			await _dataProvider.WriteAsync(new[] { (byte)type }, 0, 1, cancellationToken).ConfigureAwait(false);
			length = 1;
		}
		else
		{
			length = buffer.Length + 1;
			await WriteAsync(length, cancellationToken).ConfigureAwait(false);
			await _dataProvider.WriteAsync(new[] { (byte)type }, 0, 1, cancellationToken).ConfigureAwait(false);
			await _dataProvider.WriteAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
		}
		await WritePadAsync((4 - length) & 3, cancellationToken).ConfigureAwait(false);
	}

	public void Write(string value)
	{
		var buffer = _charset.GetBytes(value);
		WriteBuffer(buffer, buffer.Length);
	}
	public ValueTask WriteAsync(string value, CancellationToken cancellationToken = default)
	{
		var buffer = _charset.GetBytes(value);
		return WriteBufferAsync(buffer, buffer.Length, cancellationToken);
	}

	public void Write(short value)
	{
		Write((int)value);
	}
	public ValueTask WriteAsync(short value, CancellationToken cancellationToken = default)
	{
		return WriteAsync((int)value, cancellationToken);
	}

	public void Write(int value)
	{
		_dataProvider.Write(TypeEncoder.EncodeInt32(value), 0, 4);
	}
	public ValueTask WriteAsync(int value, CancellationToken cancellationToken = default)
	{
		return _dataProvider.WriteAsync(TypeEncoder.EncodeInt32(value), 0, 4, cancellationToken);
	}

	public void Write(long value)
	{
		_dataProvider.Write(TypeEncoder.EncodeInt64(value), 0, 8);
	}
	public ValueTask WriteAsync(long value, CancellationToken cancellationToken = default)
	{
		return _dataProvider.WriteAsync(TypeEncoder.EncodeInt64(value), 0, 8, cancellationToken);
	}

	public void Write(float value)
	{
		var buffer = BitConverter.GetBytes(value);
		Write(BitConverter.ToInt32(buffer, 0));
	}
	public ValueTask WriteAsync(float value, CancellationToken cancellationToken = default)
	{
		var buffer = BitConverter.GetBytes(value);
		return WriteAsync(BitConverter.ToInt32(buffer, 0), cancellationToken);
	}

	public void Write(double value)
	{
		var buffer = BitConverter.GetBytes(value);
		Write(BitConverter.ToInt64(buffer, 0));
	}
	public ValueTask WriteAsync(double value, CancellationToken cancellationToken = default)
	{
		var buffer = BitConverter.GetBytes(value);
		return WriteAsync(BitConverter.ToInt64(buffer, 0), cancellationToken);
	}

	public void Write(decimal value, int type, int scale)
	{
		var numeric = TypeEncoder.EncodeDecimal(value, scale, type);
		switch (type & ~1)
		{
			case IscCodes.SQL_SHORT:
				Write((short)numeric);
				break;
			case IscCodes.SQL_LONG:
				Write((int)numeric);
				break;
			case IscCodes.SQL_QUAD:
			case IscCodes.SQL_INT64:
				Write((long)numeric);
				break;
			case IscCodes.SQL_DOUBLE:
			case IscCodes.SQL_D_FLOAT:
				Write((double)numeric);
				break;
			case IscCodes.SQL_INT128:
				Write((BigInteger)numeric);
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(type), $"{nameof(type)}={type}");
		}
	}
	public ValueTask WriteAsync(decimal value, int type, int scale, CancellationToken cancellationToken = default)
	{
		var numeric = TypeEncoder.EncodeDecimal(value, scale, type);
		switch (type & ~1)
		{
			case IscCodes.SQL_SHORT:
				return WriteAsync((short)numeric, cancellationToken);
			case IscCodes.SQL_LONG:
				return WriteAsync((int)numeric, cancellationToken);
			case IscCodes.SQL_QUAD:
			case IscCodes.SQL_INT64:
				return WriteAsync((long)numeric, cancellationToken);
			case IscCodes.SQL_DOUBLE:
			case IscCodes.SQL_D_FLOAT:
				return WriteAsync((double)numeric, cancellationToken);
			case IscCodes.SQL_INT128:
				return WriteAsync((BigInteger)numeric, cancellationToken);
			default:
				throw new ArgumentOutOfRangeException(nameof(type), $"{nameof(type)}={type}");
		}
	}

	public void Write(bool value)
	{
		WriteOpaque(TypeEncoder.EncodeBoolean(value));
	}
	public ValueTask WriteAsync(bool value, CancellationToken cancellationToken = default)
	{
		return WriteOpaqueAsync(TypeEncoder.EncodeBoolean(value), cancellationToken);
	}

	public void Write(DateTime value)
	{
		WriteDate(value);
		WriteTime(TypeHelper.DateTimeTimeToTimeSpan(value));
	}
	public async ValueTask WriteAsync(DateTime value, CancellationToken cancellationToken = default)
	{
		await WriteDateAsync(value, cancellationToken).ConfigureAwait(false);
		await WriteTimeAsync(TypeHelper.DateTimeTimeToTimeSpan(value), cancellationToken).ConfigureAwait(false);
	}

	public void Write(Guid value, int sqlType)
	{
		var bytes = TypeEncoder.EncodeGuid(value);
		if (sqlType == IscCodes.SQL_VARYING)
		{
			WriteBuffer(bytes);
		}
		else
		{
			WriteOpaque(bytes);
		}
	}
	public ValueTask WriteAsync(Guid value, int sqlType, CancellationToken cancellationToken = default)
	{
		var bytes = TypeEncoder.EncodeGuid(value);
		if (sqlType == IscCodes.SQL_VARYING)
		{
			return WriteBufferAsync(bytes, cancellationToken);
		}
		else
		{
			return WriteOpaqueAsync(bytes, cancellationToken);
		}
	}

	public void Write(FbDecFloat value, int size)
	{
		WriteOpaque(size switch
		{
			16 => TypeEncoder.EncodeDec16(value),
			34 => TypeEncoder.EncodeDec34(value),
			_ => throw new ArgumentOutOfRangeException(),
		});
	}
	public ValueTask WriteAsync(FbDecFloat value, int size, CancellationToken cancellationToken = default)
	{
		return WriteOpaqueAsync(size switch
		{
			16 => TypeEncoder.EncodeDec16(value),
			34 => TypeEncoder.EncodeDec34(value),
			_ => throw new ArgumentOutOfRangeException(),
		}, cancellationToken);
	}

	public void Write(BigInteger value)
	{
		WriteOpaqueAsync(TypeEncoder.EncodeInt128(value));
	}
	public ValueTask WriteAsync(BigInteger value, CancellationToken cancellationToken = default)
	{
		return WriteOpaqueAsync(TypeEncoder.EncodeInt128(value), cancellationToken);
	}

	public void WriteDate(DateTime value)
	{
		Write(TypeEncoder.EncodeDate(Convert.ToDateTime(value)));
	}
	public ValueTask WriteDateAsync(DateTime value, CancellationToken cancellationToken = default)
	{
		return WriteAsync(TypeEncoder.EncodeDate(Convert.ToDateTime(value)), cancellationToken);
	}

	public void WriteTime(TimeSpan value)
	{
		Write(TypeEncoder.EncodeTime(value));
	}
	public ValueTask WriteTimeAsync(TimeSpan value, CancellationToken cancellationToken = default)
	{
		return WriteAsync(TypeEncoder.EncodeTime(value), cancellationToken);
	}

	#endregion

	#region Pad + Fill

	static readonly byte[] PadArray = new byte[] { 0, 0, 0, 0 };
	void WritePad(int length)
	{
		_dataProvider.Write(PadArray, 0, length);
	}
	ValueTask WritePadAsync(int length, CancellationToken cancellationToken = default)
	{
		return _dataProvider.WriteAsync(PadArray, 0, length, cancellationToken);
	}

	void ReadPad(int length)
	{
		Debug.Assert(length < _smallBuffer.Length);
		ReadBytes(_smallBuffer, length);
	}
	async ValueTask ReadPadAsync(int length, CancellationToken cancellationToken = default)
	{
		Debug.Assert(length < _smallBuffer.Length);
		await ReadBytesAsync(_smallBuffer, length, cancellationToken).ConfigureAwait(false);
	}

	static readonly byte[] FillArray = Enumerable.Repeat((byte)32, 32767).ToArray();
	void WriteFill(int length)
	{
		_dataProvider.Write(FillArray, 0, length);
	}
	ValueTask WriteFillAsync(int length, CancellationToken cancellationToken = default)
	{
		return _dataProvider.WriteAsync(FillArray, 0, length, cancellationToken);
	}

	#endregion
}
