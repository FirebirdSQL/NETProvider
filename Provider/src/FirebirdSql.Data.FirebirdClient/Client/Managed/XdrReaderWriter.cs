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

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;
using FirebirdSql.Data.Types;

namespace FirebirdSql.Data.Client.Managed
{
	sealed class XdrReaderWriter : IXdrReader, IXdrWriter
	{
		readonly Stream _stream;
		readonly Charset _charset;

		byte[] _smallBuffer;

		public XdrReaderWriter(Stream stream, Charset charset)
		{
			_stream = stream;
			_charset = charset;

			_smallBuffer = new byte[8];
		}

		public XdrReaderWriter(Stream stream)
			: this(stream, Charset.DefaultCharset)
		{ }

		#region Read

		public async Task<byte[]> ReadBytes(byte[] buffer, int count, AsyncWrappingCommonArgs async)
		{
			if (count > 0)
			{
				var toRead = count;
				var currentlyRead = -1;
				while (toRead > 0 && currentlyRead != 0)
				{
					toRead -= (currentlyRead = await async.AsyncSyncCall(_stream.ReadAsync, _stream.Read, buffer, count - toRead, toRead).ConfigureAwait(false));
				}
				if (currentlyRead == 0)
				{
					if (_stream is ITracksIOFailure tracksIOFailure)
					{
						tracksIOFailure.IOFailed = true;
					}
					throw new IOException();
				}
			}
			return buffer;
		}

		public async Task<byte[]> ReadOpaque(int length, AsyncWrappingCommonArgs async)
		{
			var buffer = new byte[length];
			await ReadBytes(buffer, length, async).ConfigureAwait(false);
			await ReadPad((4 - length) & 3, async).ConfigureAwait(false);
			return buffer;
		}

		public async Task<byte[]> ReadBuffer(AsyncWrappingCommonArgs async)
		{
			return await ReadOpaque((ushort)await ReadInt32(async).ConfigureAwait(false), async).ConfigureAwait(false);
		}

		public Task<string> ReadString(AsyncWrappingCommonArgs async) => ReadString(_charset, async);
		public Task<string> ReadString(int length, AsyncWrappingCommonArgs async) => ReadString(_charset, length, async);
		public async Task<string> ReadString(Charset charset, AsyncWrappingCommonArgs async) => await ReadString(charset, await ReadInt32(async).ConfigureAwait(false), async).ConfigureAwait(false);
		public async Task<string> ReadString(Charset charset, int length, AsyncWrappingCommonArgs async)
		{
			var buffer = await ReadOpaque(length, async).ConfigureAwait(false);
			return charset.GetString(buffer, 0, buffer.Length);
		}

		public async Task<short> ReadInt16(AsyncWrappingCommonArgs async)
		{
			return Convert.ToInt16(await ReadInt32(async).ConfigureAwait(false));
		}

		public async Task<int> ReadInt32(AsyncWrappingCommonArgs async)
		{
			await ReadBytes(_smallBuffer, 4, async).ConfigureAwait(false);
			return TypeDecoder.DecodeInt32(_smallBuffer);
		}

		public async Task<long> ReadInt64(AsyncWrappingCommonArgs async)
		{
			await ReadBytes(_smallBuffer, 8, async).ConfigureAwait(false);
			return TypeDecoder.DecodeInt64(_smallBuffer);
		}

		public async Task<Guid> ReadGuid(AsyncWrappingCommonArgs async)
		{
			return TypeDecoder.DecodeGuid(await ReadOpaque(16, async).ConfigureAwait(false));
		}

		public async Task<float> ReadSingle(AsyncWrappingCommonArgs async)
		{
			return BitConverter.ToSingle(BitConverter.GetBytes(await ReadInt32(async).ConfigureAwait(false)), 0);
		}

		public async Task<double> ReadDouble(AsyncWrappingCommonArgs async)
		{
			return BitConverter.ToDouble(BitConverter.GetBytes(await ReadInt64(async).ConfigureAwait(false)), 0);
		}

		public async Task<DateTime> ReadDateTime(AsyncWrappingCommonArgs async)
		{
			var date = await ReadDate(async).ConfigureAwait(false);
			var time = await ReadTime(async).ConfigureAwait(false);
			return date.Add(time);
		}

		public async Task<DateTime> ReadDate(AsyncWrappingCommonArgs async)
		{
			return TypeDecoder.DecodeDate(await ReadInt32(async).ConfigureAwait(false));
		}

		public async Task<TimeSpan> ReadTime(AsyncWrappingCommonArgs async)
		{
			return TypeDecoder.DecodeTime(await ReadInt32(async).ConfigureAwait(false));
		}

		public async Task<decimal> ReadDecimal(int type, int scale, AsyncWrappingCommonArgs async)
		{
			switch (type & ~1)
			{
				case IscCodes.SQL_SHORT:
					return TypeDecoder.DecodeDecimal(await ReadInt16(async).ConfigureAwait(false), scale, type);
				case IscCodes.SQL_LONG:
					return TypeDecoder.DecodeDecimal(await ReadInt32(async).ConfigureAwait(false), scale, type);
				case IscCodes.SQL_QUAD:
				case IscCodes.SQL_INT64:
					return TypeDecoder.DecodeDecimal(await ReadInt64(async).ConfigureAwait(false), scale, type);
				case IscCodes.SQL_DOUBLE:
				case IscCodes.SQL_D_FLOAT:
					return TypeDecoder.DecodeDecimal(await ReadDouble(async).ConfigureAwait(false), scale, type);
				case IscCodes.SQL_INT128:
					return TypeDecoder.DecodeDecimal(await ReadInt128(async).ConfigureAwait(false), scale, type);
				default:
					throw new ArgumentOutOfRangeException(nameof(type), $"{nameof(type)}={type}");
			}
		}

		public async Task<bool> ReadBoolean(AsyncWrappingCommonArgs async)
		{
			return TypeDecoder.DecodeBoolean(await ReadOpaque(1, async).ConfigureAwait(false));
		}

		public async Task<FbZonedDateTime> ReadZonedDateTime(bool isExtended, AsyncWrappingCommonArgs async)
		{
			var dt = await ReadDateTime(async).ConfigureAwait(false);
			dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
			return TypeHelper.CreateZonedDateTime(dt, (ushort)await ReadInt16(async).ConfigureAwait(false), isExtended ? await ReadInt16(async).ConfigureAwait(false) : (short?)null);
		}

		public async Task<FbZonedTime> ReadZonedTime(bool isExtended, AsyncWrappingCommonArgs async)
		{
			return TypeHelper.CreateZonedTime(await ReadTime(async).ConfigureAwait(false), (ushort)await ReadInt16(async).ConfigureAwait(false), isExtended ? await ReadInt16(async).ConfigureAwait(false) : (short?)null);
		}

		public async Task<FbDecFloat> ReadDec16(AsyncWrappingCommonArgs async)
		{
			return TypeDecoder.DecodeDec16(await ReadOpaque(8, async).ConfigureAwait(false));
		}

		public async Task<FbDecFloat> ReadDec34(AsyncWrappingCommonArgs async)
		{
			return TypeDecoder.DecodeDec34(await ReadOpaque(16, async).ConfigureAwait(false));
		}

		public async Task<BigInteger> ReadInt128(AsyncWrappingCommonArgs async)
		{
			return TypeDecoder.DecodeInt128(await ReadOpaque(16, async).ConfigureAwait(false));
		}

		public async Task<IscException> ReadStatusVector(AsyncWrappingCommonArgs async)
		{
			IscException exception = null;
			var eof = false;

			while (!eof)
			{
				var arg = await ReadInt32(async).ConfigureAwait(false);

				switch (arg)
				{
					case IscCodes.isc_arg_gds:
					default:
						var er = await ReadInt32(async).ConfigureAwait(false);
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
						exception.Errors.Add(new IscError(arg, await ReadString(async).ConfigureAwait(false)));
						break;

					case IscCodes.isc_arg_number:
						exception.Errors.Add(new IscError(arg, await ReadInt32(async).ConfigureAwait(false)));
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
		public async Task<int> ReadOperation(AsyncWrappingCommonArgs async)
		{
			int operation;
			do
			{
				operation = await ReadInt32(async).ConfigureAwait(false);
			} while (operation == IscCodes.op_dummy);
			return operation;
		}

		#endregion

		#region Write

		public Task Flush(AsyncWrappingCommonArgs async)
		{
			return async.AsyncSyncCall(_stream.FlushAsync, _stream.Flush);
		}

		public Task WriteBytes(byte[] buffer, int count, AsyncWrappingCommonArgs async)
		{
			return async.AsyncSyncCall(_stream.WriteAsync, _stream.Write, buffer, 0, count);
		}

		public Task WriteOpaque(byte[] buffer, AsyncWrappingCommonArgs async) => WriteOpaque(buffer, buffer.Length, async);
		public async Task WriteOpaque(byte[] buffer, int length, AsyncWrappingCommonArgs async)
		{
			if (buffer != null && length > 0)
			{
				await async.AsyncSyncCall(_stream.WriteAsync, _stream.Write, buffer, 0, buffer.Length).ConfigureAwait(false);
				await WriteFill(length - buffer.Length, async).ConfigureAwait(false);
				await WritePad((4 - length) & 3, async).ConfigureAwait(false);
			}
		}

		public Task WriteBuffer(byte[] buffer, AsyncWrappingCommonArgs async) => WriteBuffer(buffer, buffer?.Length ?? 0, async);
		public async Task WriteBuffer(byte[] buffer, int length, AsyncWrappingCommonArgs async)
		{
			await Write(length, async).ConfigureAwait(false);
			if (buffer != null && length > 0)
			{
				await async.AsyncSyncCall(_stream.WriteAsync, _stream.Write, buffer, 0, length).ConfigureAwait(false);
				await WritePad((4 - length) & 3, async).ConfigureAwait(false);
			}
		}

		public async Task WriteBlobBuffer(byte[] buffer, AsyncWrappingCommonArgs async)
		{
			var length = buffer.Length; // 2 for short for buffer length
			if (length > short.MaxValue)
				throw new IOException("Blob buffer too big.");
			await Write(length + 2, async).ConfigureAwait(false);
			await Write(length + 2, async).ConfigureAwait(false);  //bizarre but true! three copies of the length
			await async.AsyncSyncCall(_stream.WriteAsync, _stream.Write, new[] { (byte)((length >> 0) & 0xff), (byte)((length >> 8) & 0xff) }, 0, 2).ConfigureAwait(false);
			await async.AsyncSyncCall(_stream.WriteAsync, _stream.Write, buffer, 0, length).ConfigureAwait(false);
			await WritePad((4 - length + 2) & 3, async).ConfigureAwait(false);
		}

		public async Task WriteTyped(int type, byte[] buffer, AsyncWrappingCommonArgs async)
		{
			int length;
			if (buffer == null)
			{
				await Write(1, async).ConfigureAwait(false);
				await async.AsyncSyncCall(_stream.WriteAsync, _stream.Write, new[] { (byte)type }, 0, 1).ConfigureAwait(false);
				length = 1;
			}
			else
			{
				length = buffer.Length + 1;
				await Write(length, async).ConfigureAwait(false);
				await async.AsyncSyncCall(_stream.WriteAsync, _stream.Write, new[] { (byte)type }, 0, 1).ConfigureAwait(false);
				await async.AsyncSyncCall(_stream.WriteAsync, _stream.Write, buffer, 0, buffer.Length).ConfigureAwait(false);
			}
			await WritePad((4 - length) & 3, async).ConfigureAwait(false);
		}

		public Task Write(string value, AsyncWrappingCommonArgs async)
		{
			var buffer = _charset.GetBytes(value);
			return WriteBuffer(buffer, buffer.Length, async);
		}

		public Task Write(short value, AsyncWrappingCommonArgs async)
		{
			return Write((int)value, async);
		}

		public Task Write(int value, AsyncWrappingCommonArgs async)
		{
			return async.AsyncSyncCall(_stream.WriteAsync, _stream.Write, TypeEncoder.EncodeInt32(value), 0, 4);
		}

		public Task Write(long value, AsyncWrappingCommonArgs async)
		{
			return async.AsyncSyncCall(_stream.WriteAsync, _stream.Write, TypeEncoder.EncodeInt64(value), 0, 8);
		}

		public Task Write(float value, AsyncWrappingCommonArgs async)
		{
			var buffer = BitConverter.GetBytes(value);
			return Write(BitConverter.ToInt32(buffer, 0), async);
		}

		public Task Write(double value, AsyncWrappingCommonArgs async)
		{
			var buffer = BitConverter.GetBytes(value);
			return Write(BitConverter.ToInt64(buffer, 0), async);
		}

		public Task Write(decimal value, int type, int scale, AsyncWrappingCommonArgs async)
		{
			var numeric = TypeEncoder.EncodeDecimal(value, scale, type);
			switch (type & ~1)
			{
				case IscCodes.SQL_SHORT:
					return Write((short)numeric, async);
				case IscCodes.SQL_LONG:
					return Write((int)numeric, async);
				case IscCodes.SQL_QUAD:
				case IscCodes.SQL_INT64:
					return Write((long)numeric, async);
				case IscCodes.SQL_DOUBLE:
				case IscCodes.SQL_D_FLOAT:
					return Write((double)numeric, async);
				case IscCodes.SQL_INT128:
					return Write((BigInteger)numeric, async);
				default:
					throw new ArgumentOutOfRangeException(nameof(type), $"{nameof(type)}={type}");
			}
		}

		public Task Write(bool value, AsyncWrappingCommonArgs async)
		{
			return WriteOpaque(TypeEncoder.EncodeBoolean(value), async);
		}

		public async Task Write(DateTime value, AsyncWrappingCommonArgs async)
		{
			await WriteDate(value, async).ConfigureAwait(false);
			await WriteTime(TypeHelper.DateTimeToTimeSpan(value), async).ConfigureAwait(false);
		}

		public Task Write(Guid value, AsyncWrappingCommonArgs async)
		{
			return WriteOpaque(TypeEncoder.EncodeGuid(value), async);
		}

		public Task Write(FbDecFloat value, int size, AsyncWrappingCommonArgs async)
		{
			return WriteOpaque(size switch
			{
				16 => TypeEncoder.EncodeDec16(value),
				34 => TypeEncoder.EncodeDec34(value),
				_ => throw new ArgumentOutOfRangeException(),
			}, async);
		}

		public Task Write(BigInteger value, AsyncWrappingCommonArgs async)
		{
			return WriteOpaque(TypeEncoder.EncodeInt128(value), async);
		}

		public Task WriteDate(DateTime value, AsyncWrappingCommonArgs async)
		{
			return Write(TypeEncoder.EncodeDate(Convert.ToDateTime(value)), async);
		}

		public Task WriteTime(TimeSpan value, AsyncWrappingCommonArgs async)
		{
			return Write(TypeEncoder.EncodeTime(value), async);
		}

		#endregion

		#region Pad + Fill

		readonly static byte[] PadArray = new byte[] { 0, 0, 0, 0 };
		Task WritePad(int length, AsyncWrappingCommonArgs async)
		{
			return async.AsyncSyncCall(_stream.WriteAsync, _stream.Write, PadArray, 0, length);
		}

		Task ReadPad(int length, AsyncWrappingCommonArgs async)
		{
			Debug.Assert(length < _smallBuffer.Length);
			return ReadBytes(_smallBuffer, length, async);
		}

		readonly static byte[] FillArray = Enumerable.Repeat((byte)32, 32767).ToArray();
		Task WriteFill(int length, AsyncWrappingCommonArgs async)
		{
			return async.AsyncSyncCall(_stream.WriteAsync, _stream.Write, FillArray, 0, length);
		}

		#endregion
	}
}
