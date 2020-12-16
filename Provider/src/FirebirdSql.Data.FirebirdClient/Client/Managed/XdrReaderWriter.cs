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

		public byte[] ReadBytes(byte[] buffer, int count)
		{
			if (count > 0)
			{
				var toRead = count;
				var currentlyRead = -1;
				while (toRead > 0 && currentlyRead != 0)
				{
					toRead -= (currentlyRead = _stream.Read(buffer, count - toRead, toRead));
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
		public async Task<byte[]> ReadBytesAsync(byte[] buffer, int count)
		{
			if (count > 0)
			{
				var toRead = count;
				var currentlyRead = -1;
				while (toRead > 0 && currentlyRead != 0)
				{
					toRead -= (currentlyRead = await _stream.ReadAsync(buffer, count - toRead, toRead).ConfigureAwait(false));
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

		public byte[] ReadOpaque(int length)
		{
			var buffer = new byte[length];
			ReadBytes(buffer, length);
			ReadPad((4 - length) & 3);
			return buffer;
		}

		public byte[] ReadBuffer()
		{
			return ReadOpaque((ushort)ReadInt32());
		}

		public string ReadString() => ReadString(_charset);
		public string ReadString(int length) => ReadString(_charset, length);
		public string ReadString(Charset charset) => ReadString(charset, ReadInt32());
		public string ReadString(Charset charset, int length)
		{
			var buffer = ReadOpaque(length);
			return charset.GetString(buffer, 0, buffer.Length);
		}

		public short ReadInt16()
		{
			return Convert.ToInt16(ReadInt32());
		}

		public int ReadInt32()
		{
			ReadBytes(_smallBuffer, 4);
			return TypeDecoder.DecodeInt32(_smallBuffer);
		}
		public async Task<int> ReadInt32Async()
		{
			await ReadBytesAsync(_smallBuffer, 4).ConfigureAwait(false);
			return TypeDecoder.DecodeInt32(_smallBuffer);
		}

		public long ReadInt64()
		{
			ReadBytes(_smallBuffer, 8);
			return TypeDecoder.DecodeInt64(_smallBuffer);
		}

		public Guid ReadGuid()
		{
			return TypeDecoder.DecodeGuid(ReadOpaque(16));
		}

		public float ReadSingle()
		{
			return BitConverter.ToSingle(BitConverter.GetBytes(ReadInt32()), 0);
		}

		public double ReadDouble()
		{
			return BitConverter.ToDouble(BitConverter.GetBytes(ReadInt64()), 0);
		}

		public DateTime ReadDateTime()
		{
			var date = ReadDate();
			var time = ReadTime();
			return date.Add(time);
		}

		public DateTime ReadDate()
		{
			return TypeDecoder.DecodeDate(ReadInt32());
		}

		public TimeSpan ReadTime()
		{
			return TypeDecoder.DecodeTime(ReadInt32());
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

		public bool ReadBoolean()
		{
			return TypeDecoder.DecodeBoolean(ReadOpaque(1));
		}

		public FbZonedDateTime ReadZonedDateTime(bool isExtended)
		{
			var dt = ReadDateTime();
			dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
			return TypeHelper.CreateZonedDateTime(dt, (ushort)ReadInt16(), isExtended ? ReadInt16() : (short?)null);
		}

		public FbZonedTime ReadZonedTime(bool isExtended)
		{
			return TypeHelper.CreateZonedTime(ReadTime(), (ushort)ReadInt16(), isExtended ? ReadInt16() : (short?)null);
		}

		public FbDecFloat ReadDec16()
		{
			return TypeDecoder.DecodeDec16(ReadOpaque(8));
		}

		public FbDecFloat ReadDec34()
		{
			return TypeDecoder.DecodeDec34(ReadOpaque(16));
		}

		public BigInteger ReadInt128()
		{
			return TypeDecoder.DecodeInt128(ReadOpaque(16));
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

		#endregion

		#region Write

		public void Flush()
		{
			_stream.Flush();
		}

		public void WriteBytes(byte[] buffer, int count)
		{
			_stream.Write(buffer, 0, count);
		}

		public void WriteOpaque(byte[] buffer) => WriteOpaque(buffer, buffer.Length);
		public void WriteOpaque(byte[] buffer, int length)
		{
			if (buffer != null && length > 0)
			{
				_stream.Write(buffer, 0, buffer.Length);
				WriteFill(length - buffer.Length);
				WritePad((4 - length) & 3);
			}
		}

		public void WriteBuffer(byte[] buffer) => WriteBuffer(buffer, buffer?.Length ?? 0);
		public void WriteBuffer(byte[] buffer, int length)
		{
			Write(length);
			if (buffer != null && length > 0)
			{
				_stream.Write(buffer, 0, length);
				WritePad((4 - length) & 3);
			}
		}

		public void WriteBlobBuffer(byte[] buffer)
		{
			var length = buffer.Length; // 2 for short for buffer length
			if (length > short.MaxValue)
				throw new IOException("Blob buffer too big.");
			Write(length + 2);
			Write(length + 2);  //bizarre but true! three copies of the length
			_stream.WriteByte((byte)((length >> 0) & 0xff));
			_stream.WriteByte((byte)((length >> 8) & 0xff));
			_stream.Write(buffer, 0, length);
			WritePad((4 - length + 2) & 3);
		}

		public void WriteTyped(int type, byte[] buffer)
		{
			int length;
			if (buffer == null)
			{
				Write(1);
				_stream.WriteByte((byte)type);
				length = 1;
			}
			else
			{
				length = buffer.Length + 1;
				Write(length);
				_stream.WriteByte((byte)type);
				_stream.Write(buffer, 0, buffer.Length);
			}
			WritePad((4 - length) & 3);
		}

		public void Write(string value)
		{
			var buffer = _charset.GetBytes(value);
			WriteBuffer(buffer, buffer.Length);
		}

		public void Write(short value)
		{
			Write((int)value);
		}

		public void Write(int value)
		{
			_stream.Write(TypeEncoder.EncodeInt32(value), 0, 4);
		}

		public void Write(long value)
		{
			_stream.Write(TypeEncoder.EncodeInt64(value), 0, 8);
		}

		public void Write(float value)
		{
			var buffer = BitConverter.GetBytes(value);
			Write(BitConverter.ToInt32(buffer, 0));
		}

		public void Write(double value)
		{
			var buffer = BitConverter.GetBytes(value);
			Write(BitConverter.ToInt64(buffer, 0));
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

		public void Write(bool value)
		{
			WriteOpaque(TypeEncoder.EncodeBoolean(value));
		}

		public void Write(DateTime value)
		{
			WriteDate(value);
			WriteTime(TypeHelper.DateTimeToTimeSpan(value));
		}

		public void Write(Guid value)
		{
			WriteOpaque(TypeEncoder.EncodeGuid(value));
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

		public void Write(BigInteger value)
		{
			WriteOpaque(TypeEncoder.EncodeInt128(value));
		}

		public void WriteDate(DateTime value)
		{
			Write(TypeEncoder.EncodeDate(Convert.ToDateTime(value)));
		}

		public void WriteTime(TimeSpan value)
		{
			Write(TypeEncoder.EncodeTime(value));
		}

		#endregion

		#region Operation

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
		public async Task<int> ReadOperationAsync()
		{
			int operation;
			do
			{
				operation = await ReadInt32Async().ConfigureAwait(false);
			} while (operation == IscCodes.op_dummy);
			return operation;
		}

		#endregion

		#region Pad + Fill

		readonly static byte[] PadArray = new byte[] { 0, 0, 0, 0 };
		void WritePad(int length)
		{
			_stream.Write(PadArray, 0, length);
		}


		void ReadPad(int length)
		{
			Debug.Assert(length < _smallBuffer.Length);
			ReadBytes(_smallBuffer, length);
		}

		readonly static byte[] FillArray = Enumerable.Repeat((byte)32, 32767).ToArray();
		void WriteFill(int length)
		{
			_stream.Write(FillArray, 0, length);
		}

		#endregion
	}
}
