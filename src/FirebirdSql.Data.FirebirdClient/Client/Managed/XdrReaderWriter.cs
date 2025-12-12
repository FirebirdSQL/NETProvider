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
using System.Buffers;
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
	byte[] _boolbuffer;
	const int StackallocThreshold = 1024;

	public XdrReaderWriter(IDataProvider dataProvider, Charset charset)
	{
		_dataProvider = dataProvider;
		_charset = charset;

		_smallBuffer = new byte[8];
		_boolbuffer = new byte[1];
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

	public void ReadBytes(Span<byte> dst, int count)
	{
		if (count > 0)
		{
			var toRead = count;
			var currentlyRead = -1;
			while (toRead > 0 && currentlyRead != 0)
			{
				toRead -= (currentlyRead = _dataProvider.Read(dst, count - toRead, toRead));
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

    public async ValueTask ReadBytesAsync(Memory<byte> buffer, int count, CancellationToken cancellationToken = default)
    {
        if (count <= 0)
            return;
        var toRead = count;
        var offset = 0;
        while (toRead > 0)
        {
            var chunk = await _dataProvider.ReadAsync(buffer.Slice(offset, toRead), 0, toRead, cancellationToken).ConfigureAwait(false);
            if (chunk == 0)
            {
                if (_dataProvider is ITracksIOFailure tracksIOFailure)
                {
                    tracksIOFailure.IOFailed = true;
                }
                throw new IOException($"Missing {toRead} bytes to fill total {count}.");
            }
            offset += chunk;
            toRead -= chunk;
        }
    }

	public byte[] ReadOpaque(int length)
	{
		var buffer = length > 0 ? new byte[length] : Array.Empty<byte>();
		ReadBytes(buffer, length);
		ReadPad((4 - length) & 3);
		return buffer;
	}

	public void ReadOpaque(Span<byte> dst, int length)
	{
		ReadBytes(dst, length);
		ReadPad((4 - length) & 3);
	}

	public async ValueTask<byte[]> ReadOpaqueAsync(int length, CancellationToken cancellationToken = default)
	{
		var buffer = length > 0 ? new byte[length] : Array.Empty<byte>();
		await ReadBytesAsync(buffer, length, cancellationToken).ConfigureAwait(false);
		await ReadPadAsync((4 - length) & 3, cancellationToken).ConfigureAwait(false);
		return buffer;
	}

	public async ValueTask ReadOpaqueAsync(Memory<byte> buffer, int length, CancellationToken cancellationToken = default)
	{
		if (length <= 0)
			return;
		await ReadBytesAsync(buffer, length, cancellationToken).ConfigureAwait(false);
		await ReadPadAsync((4 - length) & 3, cancellationToken).ConfigureAwait(false);
	}

	public byte[] ReadBuffer()
	{
		return ReadOpaque((ushort)ReadInt32());
	}

	public void ReadBuffer(Span<byte> dst)
	{
		ReadOpaque(dst, (ushort)ReadInt32());
	}

	public async ValueTask<byte[]> ReadBufferAsync(CancellationToken cancellationToken = default)
	{
		return await ReadOpaqueAsync((ushort)await ReadInt32Async(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask ReadBufferAsync(Memory<byte> dst, CancellationToken cancellationToken = default)
	{
		var length = (ushort)await ReadInt32Async(cancellationToken).ConfigureAwait(false);
		if (dst.Length < length)
			throw new IOException($"Destination too small. Need {length}, have {dst.Length}.");
		await ReadOpaqueAsync(dst, length, cancellationToken).ConfigureAwait(false);
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
		if (length <= 0)
			return string.Empty;
		if (length <= StackallocThreshold)
		{
			Span<byte> buffer = stackalloc byte[length];
			ReadOpaque(buffer, length);
			return charset.GetString(buffer);
		}
		else
		{
			var rented = ArrayPool<byte>.Shared.Rent(length);
			try
			{
				ReadBytes(rented, length);
				ReadPad((4 - length) & 3);
				return charset.GetString(rented.AsSpan(0, length));
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(rented);
			}
		}
	}
	public async ValueTask<string> ReadStringAsync(Charset charset, int length, CancellationToken cancellationToken = default)
	{
		if (length <= 0)
			return string.Empty;
		var rented = ArrayPool<byte>.Shared.Rent(length);
		try
		{
			await ReadBytesAsync(rented, length, cancellationToken).ConfigureAwait(false);
			await ReadPadAsync((4 - length) & 3, cancellationToken).ConfigureAwait(false);
			return charset.GetString(rented.AsSpan(0, length));
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(rented);
		}
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
			Span<byte> buf = stackalloc byte[16];
			ReadOpaque(buf, 16);
			var rented = ArrayPool<byte>.Shared.Rent(16);
			try
			{
				buf.CopyTo(rented);
				return TypeDecoder.DecodeGuid(rented);
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(rented);
			}
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
			var rented = ArrayPool<byte>.Shared.Rent(16);
			try
			{
				await ReadBytesAsync(rented, 16, cancellationToken).ConfigureAwait(false);
				return TypeDecoder.DecodeGuid(rented);
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(rented);
			}
		}
	}

	public float Int2Single(int sqlType)
	{
		Span<byte> bytes = stackalloc byte[4];
		if (!BitConverter.TryWriteBytes(bytes, sqlType))
		{
			throw new InvalidOperationException("Failed to write Single bytes.");
		}
		return BitConverter.ToSingle(bytes);
	}

	public float ReadSingle()
	{
		return Int2Single(ReadInt32());
	}
	public async ValueTask<float> ReadSingleAsync(CancellationToken cancellationToken = default)
	{
		return Int2Single(await ReadInt32Async(cancellationToken).ConfigureAwait(false));
	}

	public double Long2Double(long sqlType)
	{
		Span<byte> bytes = stackalloc byte[8];
		if (!BitConverter.TryWriteBytes(bytes, sqlType))
		{
			throw new InvalidOperationException("Failed to write Double bytes.");
		}
		return BitConverter.ToDouble(bytes);
	}

	public double ReadDouble()
	{
		return Long2Double(ReadInt64());
	}
	public async ValueTask<double> ReadDoubleAsync(CancellationToken cancellationToken = default)
	{
		return Long2Double(await ReadInt64Async(cancellationToken).ConfigureAwait(false));
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
		Span<byte> bytes = stackalloc byte[4];
		ReadBytes(bytes, 4);
		return TypeDecoder.DecodeBoolean(bytes);
	}
	public async ValueTask<bool> ReadBooleanAsync(CancellationToken cancellationToken = default)
	{
		await ReadBytesAsync(_boolbuffer, 4, cancellationToken).ConfigureAwait(false);
		return TypeDecoder.DecodeBoolean(_boolbuffer);
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
		ReadBytes(_smallBuffer, 8);
		return TypeDecoder.DecodeDec16(_smallBuffer);
	}
	public async ValueTask<FbDecFloat> ReadDec16Async(CancellationToken cancellationToken = default)
	{
		await ReadBytesAsync(_smallBuffer, 8, cancellationToken).ConfigureAwait(false);
		return TypeDecoder.DecodeDec16(_smallBuffer);
	}

	public FbDecFloat ReadDec34()
	{
		var rented = ArrayPool<byte>.Shared.Rent(16);
		try
		{
			ReadBytes(rented, 16);
			return TypeDecoder.DecodeDec34(rented);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(rented);
		}
	}
	public async ValueTask<FbDecFloat> ReadDec34Async(CancellationToken cancellationToken = default)
	{
		var rented = ArrayPool<byte>.Shared.Rent(16);
		try
		{
			await ReadBytesAsync(rented, 16, cancellationToken).ConfigureAwait(false);
			return TypeDecoder.DecodeDec34(rented);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(rented);
		}
	}

	public BigInteger ReadInt128()
	{
		var rented = ArrayPool<byte>.Shared.Rent(16);
		try
		{
			ReadBytes(rented, 16);
			return TypeDecoder.DecodeInt128(rented);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(rented);
		}
	}
	public async ValueTask<BigInteger> ReadInt128Async(CancellationToken cancellationToken = default)
	{
		var rented = ArrayPool<byte>.Shared.Rent(16);
		try
		{
			await ReadBytesAsync(rented, 16, cancellationToken).ConfigureAwait(false);
			return TypeDecoder.DecodeInt128(rented);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(rented);
		}
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

	public void WriteBytes(ReadOnlySpan<byte> buffer)
	{
		_dataProvider.Write(buffer);
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

    public void WriteOpaque(ReadOnlySpan<byte> buffer, int length)
    {
        if (length > 0)
        {
            if (!buffer.IsEmpty)
            {
                _dataProvider.Write(buffer);
            }
            WriteFill(length - buffer.Length);
            WritePad((4 - length) & 3);
        }
    }

	public void WriteOpaque(ReadOnlySpan<byte> buffer)
	{
		var length = buffer.Length;
		if (length > 0) {
			_dataProvider.Write(buffer);
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

    public async ValueTask WriteOpaqueAsync(ReadOnlyMemory<byte> buffer, int length, CancellationToken cancellationToken = default)
    {
        if (length > 0)
        {
            if (buffer.Length > 0)
            {
                await _dataProvider.WriteAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
            }
            await WriteFillAsync(length - buffer.Length, cancellationToken).ConfigureAwait(false);
            await WritePadAsync((4 - length) & 3, cancellationToken).ConfigureAwait(false);
        }
    }

    public async ValueTask WriteOpaqueAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var length = buffer.Length;
        if (length > 0)
        {
            await _dataProvider.WriteAsync(buffer, 0, length, cancellationToken).ConfigureAwait(false);
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

    public async ValueTask WriteBufferAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var length = buffer.Length;
        await WriteAsync(length, cancellationToken).ConfigureAwait(false);
        if (length > 0)
        {
            await _dataProvider.WriteAsync(buffer, 0, length, cancellationToken).ConfigureAwait(false);
            await WritePadAsync((4 - length) & 3, cancellationToken).ConfigureAwait(false);
        }
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

	public void WriteBuffer(ReadOnlySpan<byte> buffer)
	{
		var length = buffer.Length;
		Write(length);
		if (length > 0)
		{
			_dataProvider.Write(buffer);
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
		Span<byte> lengthBytes = stackalloc byte[2];
		lengthBytes[0] = (byte)((length >> 0) & 0xff);
		lengthBytes[1] = (byte)((length >> 8) & 0xff);
		_dataProvider.Write(lengthBytes);
		_dataProvider.Write(buffer, 0, length);
		WritePad((4 - length + 2) & 3);
	}

	public void WriteBlobBuffer(ReadOnlySpan<byte> buffer)
	{
		var length = buffer.Length; // 2 for short for buffer length
		if (length > short.MaxValue)
			throw new IOException("Blob buffer too big.");
		Write(length + 2);
		Write(length + 2);  //bizarre but true! three copies of the length
		Span<byte> lengthBytes = [(byte)((length >> 0) & 0xff), (byte)((length >> 8) & 0xff)];
		_dataProvider.Write(lengthBytes);
		_dataProvider.Write(buffer);
		WritePad((4 - length + 2) & 3);
	}
	public async ValueTask WriteBlobBufferAsync(byte[] buffer, CancellationToken cancellationToken = default)
	{
		var length = buffer.Length; // 2 for short for buffer length
		if (length > short.MaxValue)
			throw new IOException("Blob buffer too big.");
		await WriteAsync(length + 2, cancellationToken).ConfigureAwait(false);
		await WriteAsync(length + 2, cancellationToken).ConfigureAwait(false);  //bizarre but true! three copies of the length
		var rented = ArrayPool<byte>.Shared.Rent(2);
		try
		{
			rented[0] = (byte)((length >> 0) & 0xff);
			rented[1] = (byte)((length >> 8) & 0xff);
			await _dataProvider.WriteAsync(rented, 0, 2, cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(rented);
		}
		await _dataProvider.WriteAsync(buffer, 0, length, cancellationToken).ConfigureAwait(false);
		await WritePadAsync((4 - length + 2) & 3, cancellationToken).ConfigureAwait(false);
	}

    public async ValueTask WriteBlobBufferAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var length = buffer.Length; // 2 for short for buffer length
        if (length > short.MaxValue)
            throw new IOException("Blob buffer too big.");
        await WriteAsync(length + 2, cancellationToken).ConfigureAwait(false);
        await WriteAsync(length + 2, cancellationToken).ConfigureAwait(false);  // three copies of the length
        Span<byte> lengthBytes = stackalloc byte[2];
        lengthBytes[0] = (byte)((length >> 0) & 0xff);
        lengthBytes[1] = (byte)((length >> 8) & 0xff);
        _dataProvider.Write(lengthBytes);
        await _dataProvider.WriteAsync(buffer, 0, length, cancellationToken).ConfigureAwait(false);
        await WritePadAsync((4 - length + 2) & 3, cancellationToken).ConfigureAwait(false);
    }

	public void WriteTyped(int type, byte[] buffer)
	{
		Span<byte> typeByte = stackalloc byte[1];
		int length;
		if (buffer == null)
		{
			Write(1);
			typeByte[0] = (byte)type;
			_dataProvider.Write(typeByte);
			length = 1;
		}
		else
		{
			length = buffer.Length + 1;
			Write(length);
			typeByte[0] = (byte)type;
			_dataProvider.Write(typeByte);
			_dataProvider.Write(buffer, 0, buffer.Length);
		}
		WritePad((4 - length) & 3);
	}

	public void WriteTyped(int type, ReadOnlySpan<byte> buffer)
	{
		int length;
		Span<byte> typeByte = stackalloc byte[1];
		if (buffer == null)
		{
			Write(1);
			typeByte[0] = (byte)type;
			_dataProvider.Write(typeByte);
			length = 1;
		}
		else
		{
			length = buffer.Length + 1;
			Write(length);
			typeByte[0] = (byte)type;
			_dataProvider.Write(typeByte);
			_dataProvider.Write(buffer);
		}
		WritePad((4 - length) & 3);
	}
	public async ValueTask WriteTypedAsync(int type, byte[] buffer, CancellationToken cancellationToken = default)
	{
		int length;
		if (buffer == null)
		{
			await WriteAsync(1, cancellationToken).ConfigureAwait(false);
			var rented = ArrayPool<byte>.Shared.Rent(1);
			try
			{
				rented[0] = (byte)type;
				await _dataProvider.WriteAsync(rented, 0, 1, cancellationToken).ConfigureAwait(false);
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(rented);
			}
			length = 1;
		}
		else
		{
			length = buffer.Length + 1;
			await WriteAsync(length, cancellationToken).ConfigureAwait(false);
			var rented = ArrayPool<byte>.Shared.Rent(1);
			try
			{
				rented[0] = (byte)type;
				await _dataProvider.WriteAsync(rented, 0, 1, cancellationToken).ConfigureAwait(false);
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(rented);
			}
			await _dataProvider.WriteAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
		}
		await WritePadAsync((4 - length) & 3, cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask WriteTypedAsync(int type, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
	{
		int length;
		if (buffer.Length == 0)
		{
			await WriteAsync(1, cancellationToken).ConfigureAwait(false);
			var rented = ArrayPool<byte>.Shared.Rent(1);
			try
			{
				rented[0] = (byte)type;
				await _dataProvider.WriteAsync(rented, 0, 1, cancellationToken).ConfigureAwait(false);
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(rented);
			}
			length = 1;
		}
		else
		{
			length = buffer.Length + 1;
			await WriteAsync(length, cancellationToken).ConfigureAwait(false);
			var rented = ArrayPool<byte>.Shared.Rent(1);
			try
			{
				rented[0] = (byte)type;
				await _dataProvider.WriteAsync(rented, 0, 1, cancellationToken).ConfigureAwait(false);
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(rented);
			}
        await _dataProvider.WriteAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
		}
		await WritePadAsync((4 - length) & 3, cancellationToken).ConfigureAwait(false);
	}

	public void Write(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			WriteBuffer(ReadOnlySpan<byte>.Empty);
			return;
		}
		var encoding = _charset.Encoding;
		var maxBytes = encoding.GetMaxByteCount(value.Length);
		if (maxBytes <= StackallocThreshold)
		{
			Span<byte> span = stackalloc byte[maxBytes];
			var written = encoding.GetBytes(value.AsSpan(), span);
			WriteBuffer(span[..written]);
		}
		else
		{
			var rented = ArrayPool<byte>.Shared.Rent(maxBytes);
			try
			{
				var written = encoding.GetBytes(value.AsSpan(), rented.AsSpan());
				WriteBuffer(rented.AsSpan(0, written));
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(rented);
			}
		}
	}
	public ValueTask WriteAsync(string value, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrEmpty(value))
		{
			return WriteBufferAsync(Array.Empty<byte>(), 0, cancellationToken);
		}
		var encoding = _charset.Encoding;
		var byteCount = encoding.GetByteCount(value);
		var rented = ArrayPool<byte>.Shared.Rent(byteCount);
		var written = encoding.GetBytes(value, 0, value.Length, rented, 0);
		var task = WriteBufferAsync(rented, written, cancellationToken);
		return ReturnAfter(task, rented);
	}

	static async ValueTask ReturnAfter(ValueTask writeTask, byte[] rented)
	{
		try { await writeTask.ConfigureAwait(false); }
		finally { ArrayPool<byte>.Shared.Return(rented); }
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
		Span<byte> bytes = stackalloc byte[4];
		TypeEncoder.EncodeInt32(value, bytes);
		_dataProvider.Write(bytes);
	}
	public ValueTask WriteAsync(int value, CancellationToken cancellationToken = default)
	{
		var rented = ArrayPool<byte>.Shared.Rent(4);
		Span<byte> span = rented;
		TypeEncoder.EncodeInt32(value, span);
		var task = _dataProvider.WriteAsync(rented, 0, 4, cancellationToken);
		return ReturnAfter(task, rented);
	}

	public void Write(long value)
	{
		Span<byte> bytes = stackalloc byte[8];
		TypeEncoder.EncodeInt64(value, bytes);
		_dataProvider.Write(bytes);
	}
	public ValueTask WriteAsync(long value, CancellationToken cancellationToken = default)
	{
		var rented = ArrayPool<byte>.Shared.Rent(8);
		Span<byte> span = rented;
		TypeEncoder.EncodeInt64(value, span);
		var task = _dataProvider.WriteAsync(rented, 0, 8, cancellationToken);
		return ReturnAfter(task, rented);
	}

	public void Write(float value)
	{
		Span<byte> buffer = stackalloc byte[4];
		if (!BitConverter.TryWriteBytes(buffer, value))
		{
			throw new InvalidOperationException("Failed to write Single bytes.");
		}
		Write(BitConverter.ToInt32(buffer));
	}
	public ValueTask WriteAsync(float value, CancellationToken cancellationToken = default)
	{
		var rented = ArrayPool<byte>.Shared.Rent(4);
		if (!BitConverter.TryWriteBytes(rented, value))
		{
			ArrayPool<byte>.Shared.Return(rented);
			throw new InvalidOperationException("Failed to write Single bytes.");
		}
		var intVal = BitConverter.ToInt32(rented, 0);
		ArrayPool<byte>.Shared.Return(rented);
		return WriteAsync(intVal, cancellationToken);
	}

	public void Write(double value)
	{
		Span<byte> buffer = stackalloc byte[8];
		if (!BitConverter.TryWriteBytes(buffer, value))
		{
			throw new InvalidOperationException("Failed to write Double bytes.");
		}
		Write(BitConverter.ToInt64(buffer));
	}
	public ValueTask WriteAsync(double value, CancellationToken cancellationToken = default)
	{
		var rented = ArrayPool<byte>.Shared.Rent(8);
		if (!BitConverter.TryWriteBytes(rented, value))
		{
			ArrayPool<byte>.Shared.Return(rented);
			throw new InvalidOperationException("Failed to write Double bytes.");
		}
		var longVal = BitConverter.ToInt64(rented, 0);
		ArrayPool<byte>.Shared.Return(rented);
		return WriteAsync(longVal, cancellationToken);
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
		Span<byte> buffer = stackalloc byte[1];
		TypeEncoder.EncodeBoolean(value, buffer);
		WriteOpaque(buffer);
	}
	public ValueTask WriteAsync(bool value, CancellationToken cancellationToken = default)
	{
		var rented = ArrayPool<byte>.Shared.Rent(1);
		TypeEncoder.EncodeBoolean(value, rented.AsSpan());
		var task = WriteOpaqueAsync(rented, 1, cancellationToken);
		return ReturnAfter(task, rented);
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
		Span<byte> bytes = stackalloc byte[16];
		TypeEncoder.EncodeGuid(value, bytes);
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
		var rented = ArrayPool<byte>.Shared.Rent(16);
		Span<byte> span = rented;
		TypeEncoder.EncodeGuid(value, span);
		if (sqlType == IscCodes.SQL_VARYING)
		{
			var task = WriteBufferAsync(rented, 16, cancellationToken);
			return ReturnAfter(task, rented);
		}
		else
		{
			var task = WriteOpaqueAsync(rented, 16, cancellationToken);
			return ReturnAfter(task, rented);
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
		WriteOpaque(TypeEncoder.EncodeInt128(value));
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
