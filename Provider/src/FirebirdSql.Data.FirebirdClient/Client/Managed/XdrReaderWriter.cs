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

		public async ValueTask<byte[]> ReadBytesAsync(byte[] buffer, int count, AsyncWrappingCommonArgs async)
		{
			if (count > 0)
			{
				var toRead = count;
				var currentlyRead = -1;
				while (toRead > 0 && currentlyRead != 0)
				{
					toRead -= (currentlyRead = await _dataProvider.ReadAsync(buffer, count - toRead, toRead, async).ConfigureAwait(false));
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

		public async ValueTask<byte[]> ReadOpaqueAsync(int length, AsyncWrappingCommonArgs async)
		{
			var buffer = new byte[length];
			await ReadBytesAsync(buffer, length, async).ConfigureAwait(false);
			await ReadPadAsync((4 - length) & 3, async).ConfigureAwait(false);
			return buffer;
		}

		public async ValueTask<byte[]> ReadBufferAsync(AsyncWrappingCommonArgs async)
		{
			return await ReadOpaqueAsync((ushort)await ReadInt32Async(async).ConfigureAwait(false), async).ConfigureAwait(false);
		}

		public ValueTask<string> ReadStringAsync(AsyncWrappingCommonArgs async) => ReadStringAsync(_charset, async);
		public ValueTask<string> ReadStringAsync(int length, AsyncWrappingCommonArgs async) => ReadStringAsync(_charset, length, async);
		public async ValueTask<string> ReadStringAsync(Charset charset, AsyncWrappingCommonArgs async) => await ReadStringAsync(charset, await ReadInt32Async(async).ConfigureAwait(false), async).ConfigureAwait(false);
		public async ValueTask<string> ReadStringAsync(Charset charset, int length, AsyncWrappingCommonArgs async)
		{
			var buffer = await ReadOpaqueAsync(length, async).ConfigureAwait(false);
			return charset.GetString(buffer, 0, buffer.Length);
		}

		public async ValueTask<short> ReadInt16Async(AsyncWrappingCommonArgs async)
		{
			return Convert.ToInt16(await ReadInt32Async(async).ConfigureAwait(false));
		}

		public async ValueTask<int> ReadInt32Async(AsyncWrappingCommonArgs async)
		{
			await ReadBytesAsync(_smallBuffer, 4, async).ConfigureAwait(false);
			return TypeDecoder.DecodeInt32(_smallBuffer);
		}

		public async ValueTask<long> ReadInt64Async(AsyncWrappingCommonArgs async)
		{
			await ReadBytesAsync(_smallBuffer, 8, async).ConfigureAwait(false);
			return TypeDecoder.DecodeInt64(_smallBuffer);
		}

		public async ValueTask<Guid> ReadGuidAsync(AsyncWrappingCommonArgs async)
		{
			return TypeDecoder.DecodeGuid(await ReadOpaqueAsync(16, async).ConfigureAwait(false));
		}

		public async ValueTask<float> ReadSingleAsync(AsyncWrappingCommonArgs async)
		{
			return BitConverter.ToSingle(BitConverter.GetBytes(await ReadInt32Async(async).ConfigureAwait(false)), 0);
		}

		public async ValueTask<double> ReadDoubleAsync(AsyncWrappingCommonArgs async)
		{
			return BitConverter.ToDouble(BitConverter.GetBytes(await ReadInt64Async(async).ConfigureAwait(false)), 0);
		}

		public async ValueTask<DateTime> ReadDateTimeAsync(AsyncWrappingCommonArgs async)
		{
			var date = await ReadDateAsync(async).ConfigureAwait(false);
			var time = await ReadTimeAsync(async).ConfigureAwait(false);
			return date.Add(time);
		}

		public async ValueTask<DateTime> ReadDateAsync(AsyncWrappingCommonArgs async)
		{
			return TypeDecoder.DecodeDate(await ReadInt32Async(async).ConfigureAwait(false));
		}

		public async ValueTask<TimeSpan> ReadTimeAsync(AsyncWrappingCommonArgs async)
		{
			return TypeDecoder.DecodeTime(await ReadInt32Async(async).ConfigureAwait(false));
		}

		public async ValueTask<decimal> ReadDecimalAsync(int type, int scale, AsyncWrappingCommonArgs async)
		{
			switch (type & ~1)
			{
				case IscCodes.SQL_SHORT:
					return TypeDecoder.DecodeDecimal(await ReadInt16Async(async).ConfigureAwait(false), scale, type);
				case IscCodes.SQL_LONG:
					return TypeDecoder.DecodeDecimal(await ReadInt32Async(async).ConfigureAwait(false), scale, type);
				case IscCodes.SQL_QUAD:
				case IscCodes.SQL_INT64:
					return TypeDecoder.DecodeDecimal(await ReadInt64Async(async).ConfigureAwait(false), scale, type);
				case IscCodes.SQL_DOUBLE:
				case IscCodes.SQL_D_FLOAT:
					return TypeDecoder.DecodeDecimal(await ReadDoubleAsync(async).ConfigureAwait(false), scale, type);
				case IscCodes.SQL_INT128:
					return TypeDecoder.DecodeDecimal(await ReadInt128Async(async).ConfigureAwait(false), scale, type);
				default:
					throw new ArgumentOutOfRangeException(nameof(type), $"{nameof(type)}={type}");
			}
		}

		public async ValueTask<bool> ReadBooleanAsync(AsyncWrappingCommonArgs async)
		{
			return TypeDecoder.DecodeBoolean(await ReadOpaqueAsync(1, async).ConfigureAwait(false));
		}

		public async ValueTask<FbZonedDateTime> ReadZonedDateTimeAsync(bool isExtended, AsyncWrappingCommonArgs async)
		{
			var dt = await ReadDateTimeAsync(async).ConfigureAwait(false);
			dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
			return TypeHelper.CreateZonedDateTime(dt, (ushort)await ReadInt16Async(async).ConfigureAwait(false), isExtended ? await ReadInt16Async(async).ConfigureAwait(false) : (short?)null);
		}

		public async ValueTask<FbZonedTime> ReadZonedTimeAsync(bool isExtended, AsyncWrappingCommonArgs async)
		{
			return TypeHelper.CreateZonedTime(await ReadTimeAsync(async).ConfigureAwait(false), (ushort)await ReadInt16Async(async).ConfigureAwait(false), isExtended ? await ReadInt16Async(async).ConfigureAwait(false) : (short?)null);
		}

		public async ValueTask<FbDecFloat> ReadDec16Async(AsyncWrappingCommonArgs async)
		{
			return TypeDecoder.DecodeDec16(await ReadOpaqueAsync(8, async).ConfigureAwait(false));
		}

		public async ValueTask<FbDecFloat> ReadDec34Async(AsyncWrappingCommonArgs async)
		{
			return TypeDecoder.DecodeDec34(await ReadOpaqueAsync(16, async).ConfigureAwait(false));
		}

		public async ValueTask<BigInteger> ReadInt128Async(AsyncWrappingCommonArgs async)
		{
			return TypeDecoder.DecodeInt128(await ReadOpaqueAsync(16, async).ConfigureAwait(false));
		}

		public async ValueTask<IscException> ReadStatusVectorAsync(AsyncWrappingCommonArgs async)
		{
			IscException exception = null;
			var eof = false;

			while (!eof)
			{
				var arg = await ReadInt32Async(async).ConfigureAwait(false);

				switch (arg)
				{
					case IscCodes.isc_arg_gds:
					default:
						var er = await ReadInt32Async(async).ConfigureAwait(false);
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
						exception.Errors.Add(new IscError(arg, await ReadStringAsync(async).ConfigureAwait(false)));
						break;

					case IscCodes.isc_arg_number:
						exception.Errors.Add(new IscError(arg, await ReadInt32Async(async).ConfigureAwait(false)));
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
		public async ValueTask<int> ReadOperationAsync(AsyncWrappingCommonArgs async)
		{
			int operation;
			do
			{
				operation = await ReadInt32Async(async).ConfigureAwait(false);
			} while (operation == IscCodes.op_dummy);
			return operation;
		}

		#endregion

		#region Write

		public ValueTask FlushAsync(AsyncWrappingCommonArgs async) => _dataProvider.FlushAsync(async);

		public ValueTask WriteBytesAsync(byte[] buffer, int count, AsyncWrappingCommonArgs async) => _dataProvider.WriteAsync(buffer, 0, count, async);

		public ValueTask WriteOpaqueAsync(byte[] buffer, AsyncWrappingCommonArgs async) => WriteOpaqueAsync(buffer, buffer.Length, async);
		public async ValueTask WriteOpaqueAsync(byte[] buffer, int length, AsyncWrappingCommonArgs async)
		{
			if (buffer != null && length > 0)
			{
				await _dataProvider.WriteAsync(buffer, 0, buffer.Length, async).ConfigureAwait(false);
				await WriteFillAsync(length - buffer.Length, async).ConfigureAwait(false);
				await WritePadAsync((4 - length) & 3, async).ConfigureAwait(false);
			}
		}

		public ValueTask WriteBufferAsync(byte[] buffer, AsyncWrappingCommonArgs async) => WriteBufferAsync(buffer, buffer?.Length ?? 0, async);
		public async ValueTask WriteBufferAsync(byte[] buffer, int length, AsyncWrappingCommonArgs async)
		{
			await WriteAsync(length, async).ConfigureAwait(false);
			if (buffer != null && length > 0)
			{
				await _dataProvider.WriteAsync(buffer, 0, length, async).ConfigureAwait(false);
				await WritePadAsync((4 - length) & 3, async).ConfigureAwait(false);
			}
		}

		public async ValueTask WriteBlobBufferAsync(byte[] buffer, AsyncWrappingCommonArgs async)
		{
			var length = buffer.Length; // 2 for short for buffer length
			if (length > short.MaxValue)
				throw new IOException("Blob buffer too big.");
			await WriteAsync(length + 2, async).ConfigureAwait(false);
			await WriteAsync(length + 2, async).ConfigureAwait(false);  //bizarre but true! three copies of the length
			await _dataProvider.WriteAsync(new[] { (byte)((length >> 0) & 0xff), (byte)((length >> 8) & 0xff) }, 0, 2, async).ConfigureAwait(false);
			await _dataProvider.WriteAsync(buffer, 0, length, async).ConfigureAwait(false);
			await WritePadAsync((4 - length + 2) & 3, async).ConfigureAwait(false);
		}

		public async ValueTask WriteTypedAsync(int type, byte[] buffer, AsyncWrappingCommonArgs async)
		{
			int length;
			if (buffer == null)
			{
				await WriteAsync(1, async).ConfigureAwait(false);
				await _dataProvider.WriteAsync(new[] { (byte)type }, 0, 1, async).ConfigureAwait(false);
				length = 1;
			}
			else
			{
				length = buffer.Length + 1;
				await WriteAsync(length, async).ConfigureAwait(false);
				await _dataProvider.WriteAsync(new[] { (byte)type }, 0, 1, async).ConfigureAwait(false);
				await _dataProvider.WriteAsync(buffer, 0, buffer.Length, async).ConfigureAwait(false);
			}
			await WritePadAsync((4 - length) & 3, async).ConfigureAwait(false);
		}

		public ValueTask WriteAsync(string value, AsyncWrappingCommonArgs async)
		{
			var buffer = _charset.GetBytes(value);
			return WriteBufferAsync(buffer, buffer.Length, async);
		}

		public ValueTask WriteAsync(short value, AsyncWrappingCommonArgs async)
		{
			return WriteAsync((int)value, async);
		}

		public ValueTask WriteAsync(int value, AsyncWrappingCommonArgs async)
		{
			return _dataProvider.WriteAsync(TypeEncoder.EncodeInt32(value), 0, 4, async);
		}

		public ValueTask WriteAsync(long value, AsyncWrappingCommonArgs async)
		{
			return _dataProvider.WriteAsync(TypeEncoder.EncodeInt64(value), 0, 8, async);
		}

		public ValueTask WriteAsync(float value, AsyncWrappingCommonArgs async)
		{
			var buffer = BitConverter.GetBytes(value);
			return WriteAsync(BitConverter.ToInt32(buffer, 0), async);
		}

		public ValueTask WriteAsync(double value, AsyncWrappingCommonArgs async)
		{
			var buffer = BitConverter.GetBytes(value);
			return WriteAsync(BitConverter.ToInt64(buffer, 0), async);
		}

		public ValueTask WriteAsync(decimal value, int type, int scale, AsyncWrappingCommonArgs async)
		{
			var numeric = TypeEncoder.EncodeDecimal(value, scale, type);
			switch (type & ~1)
			{
				case IscCodes.SQL_SHORT:
					return WriteAsync((short)numeric, async);
				case IscCodes.SQL_LONG:
					return WriteAsync((int)numeric, async);
				case IscCodes.SQL_QUAD:
				case IscCodes.SQL_INT64:
					return WriteAsync((long)numeric, async);
				case IscCodes.SQL_DOUBLE:
				case IscCodes.SQL_D_FLOAT:
					return WriteAsync((double)numeric, async);
				case IscCodes.SQL_INT128:
					return WriteAsync((BigInteger)numeric, async);
				default:
					throw new ArgumentOutOfRangeException(nameof(type), $"{nameof(type)}={type}");
			}
		}

		public ValueTask WriteAsync(bool value, AsyncWrappingCommonArgs async)
		{
			return WriteOpaqueAsync(TypeEncoder.EncodeBoolean(value), async);
		}

		public async ValueTask WriteAsync(DateTime value, AsyncWrappingCommonArgs async)
		{
			await WriteDateAsync(value, async).ConfigureAwait(false);
			await WriteTimeAsync(TypeHelper.DateTimeToTimeSpan(value), async).ConfigureAwait(false);
		}

		public ValueTask WriteAsync(Guid value, AsyncWrappingCommonArgs async)
		{
			return WriteOpaqueAsync(TypeEncoder.EncodeGuid(value), async);
		}

		public ValueTask WriteAsync(FbDecFloat value, int size, AsyncWrappingCommonArgs async)
		{
			return WriteOpaqueAsync(size switch
			{
				16 => TypeEncoder.EncodeDec16(value),
				34 => TypeEncoder.EncodeDec34(value),
				_ => throw new ArgumentOutOfRangeException(),
			}, async);
		}

		public ValueTask WriteAsync(BigInteger value, AsyncWrappingCommonArgs async)
		{
			return WriteOpaqueAsync(TypeEncoder.EncodeInt128(value), async);
		}

		public ValueTask WriteDateAsync(DateTime value, AsyncWrappingCommonArgs async)
		{
			return WriteAsync(TypeEncoder.EncodeDate(Convert.ToDateTime(value)), async);
		}

		public ValueTask WriteTimeAsync(TimeSpan value, AsyncWrappingCommonArgs async)
		{
			return WriteAsync(TypeEncoder.EncodeTime(value), async);
		}

		#endregion

		#region Pad + Fill

		readonly static byte[] PadArray = new byte[] { 0, 0, 0, 0 };
		ValueTask WritePadAsync(int length, AsyncWrappingCommonArgs async)
		{
			return _dataProvider.WriteAsync(PadArray, 0, length, async);
		}

		async ValueTask ReadPadAsync(int length, AsyncWrappingCommonArgs async)
		{
			Debug.Assert(length < _smallBuffer.Length);
			await ReadBytesAsync(_smallBuffer, length, async).ConfigureAwait(false);
		}

		readonly static byte[] FillArray = Enumerable.Repeat((byte)32, 32767).ToArray();
		ValueTask WriteFillAsync(int length, AsyncWrappingCommonArgs async)
		{
			return _dataProvider.WriteAsync(FillArray, 0, length, async);
		}

		#endregion
	}
}
