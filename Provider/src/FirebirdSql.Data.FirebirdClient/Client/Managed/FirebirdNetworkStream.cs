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
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace FirebirdSql.Data.Client.Managed
{
	class FirebirdNetworkStream : Stream, ITracksIOFailure
	{
		public const string CompressionName = "zlib";
		public const string EncryptionName = "Arc4";

		const int PreferredBufferSize = 32 * 1024;

		readonly NetworkStream _networkStream;

		readonly Queue<byte> _outputBuffer;
		readonly Queue<byte> _inputBuffer;
		readonly byte[] _readBuffer;

		byte[] _compressionBuffer;
		Ionic.Zlib.ZlibCodec _compressor;
		Ionic.Zlib.ZlibCodec _decompressor;

		Org.BouncyCastle.Crypto.Engines.RC4Engine _decryptor;
		Org.BouncyCastle.Crypto.Engines.RC4Engine _encryptor;

		public FirebirdNetworkStream(NetworkStream networkStream)
		{
			_networkStream = networkStream;

			_outputBuffer = new Queue<byte>(PreferredBufferSize);
			_inputBuffer = new Queue<byte>(PreferredBufferSize);
			_readBuffer = new byte[PreferredBufferSize];
		}

		public bool IOFailed { get; set; }

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (_inputBuffer.Count < count)
			{
				var readBuffer = _readBuffer;
				int read;
				try
				{
					read = _networkStream.Read(readBuffer, 0, readBuffer.Length);
				}
				catch (IOException)
				{
					IOFailed = true;
					throw;
				}
				if (read != 0)
				{
					if (_decryptor != null)
					{
						_decryptor.ProcessBytes(readBuffer, 0, read, readBuffer, 0);
					}
					if (_decompressor != null)
					{
						read = HandleDecompression(readBuffer, read);
						readBuffer = _compressionBuffer;
					}
					WriteToInputBuffer(readBuffer, read);
				}
			}
			var dataLength = ReadFromInputBuffer(buffer, offset, count);
			return dataLength;
		}
		public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			if (_inputBuffer.Count < count)
			{
				var readBuffer = _readBuffer;
				int read;
				try
				{
					read = await _networkStream.ReadAsync(readBuffer, 0, readBuffer.Length, cancellationToken).ConfigureAwait(false);
				}
				catch (IOException)
				{
					IOFailed = true;
					throw;
				}
				if (read != 0)
				{
					if (_decryptor != null)
					{
						_decryptor.ProcessBytes(readBuffer, 0, read, readBuffer, 0);
					}
					if (_decompressor != null)
					{
						read = HandleDecompression(readBuffer, read);
						readBuffer = _compressionBuffer;
					}
					WriteToInputBuffer(readBuffer, read);
				}
			}
			var dataLength = ReadFromInputBuffer(buffer, offset, count);
			return dataLength;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			for (var i = 0; i < count; i++)
				WriteByte(buffer[i]);
		}

		public override void WriteByte(byte value)
		{
			_outputBuffer.Enqueue(value);
		}

		public override void Flush()
		{
			var buffer = _outputBuffer.ToArray();
			_outputBuffer.Clear();
			var count = buffer.Length;
			if (_compressor != null)
			{
				count = HandleCompression(buffer, count);
				buffer = _compressionBuffer;
			}
			if (_encryptor != null)
			{
				_encryptor.ProcessBytes(buffer, 0, count, buffer, 0);
			}
			try
			{
				_networkStream.Write(buffer, 0, count);
				_networkStream.Flush();
			}
			catch (IOException)
			{
				IOFailed = true;
				throw;
			}
		}

		public void StartCompression(Ionic.Zlib.ZlibCodec compressor, Ionic.Zlib.ZlibCodec decompressor)
		{
			_compressionBuffer = new byte[PreferredBufferSize];
			_compressor = compressor;
			_decompressor = decompressor;
		}

		public void StartEncryption(Org.BouncyCastle.Crypto.Engines.RC4Engine encryptor, Org.BouncyCastle.Crypto.Engines.RC4Engine decryptor)
		{
			_encryptor = encryptor;
			_decryptor = decryptor;
		}

		protected override void Dispose(bool disposing)
		{
			_networkStream.Dispose();
			base.Dispose(disposing);
		}

		int ReadFromInputBuffer(byte[] buffer, int offset, int count)
		{
			var read = Math.Min(count, _inputBuffer.Count);
			for (var i = 0; i < read; i++)
			{
				buffer[offset + i] = _inputBuffer.Dequeue();
			}
			return read;
		}

		void WriteToInputBuffer(byte[] data, int count)
		{
			for (var i = 0; i < count; i++)
			{
				_inputBuffer.Enqueue(data[i]);
			}
		}

		int HandleDecompression(byte[] buffer, int count)
		{
			_decompressor.InputBuffer = buffer;
			_decompressor.NextOut = 0;
			_decompressor.NextIn = 0;
			_decompressor.AvailableBytesIn = count;
			while (true)
			{
				_decompressor.OutputBuffer = _compressionBuffer;
				_decompressor.AvailableBytesOut = _compressionBuffer.Length - _decompressor.NextOut;
				var rc = _decompressor.Inflate(Ionic.Zlib.FlushType.None);
				if (rc != Ionic.Zlib.ZlibConstants.Z_OK)
					throw new IOException($"Error '{rc}' while decompressing the data.");
				if (_decompressor.AvailableBytesIn > 0 || _decompressor.AvailableBytesOut == 0)
				{
					ResizeBuffer(ref _compressionBuffer);
					continue;
				}
				break;
			}
			return _decompressor.NextOut;
		}

		int HandleCompression(byte[] buffer, int count)
		{
			_compressor.InputBuffer = buffer;
			_compressor.NextOut = 0;
			_compressor.NextIn = 0;
			_compressor.AvailableBytesIn = count;
			while (true)
			{
				_compressor.OutputBuffer = _compressionBuffer;
				_compressor.AvailableBytesOut = _compressionBuffer.Length - _compressor.NextOut;
				var rc = _compressor.Deflate(Ionic.Zlib.FlushType.None);
				if (rc != Ionic.Zlib.ZlibConstants.Z_OK)
					throw new IOException($"Error '{rc}' while compressing the data.");
				if (_compressor.AvailableBytesIn > 0 || _compressor.AvailableBytesOut == 0)
				{
					ResizeBuffer(ref _compressionBuffer);
					continue;
				}
				break;
			}
			while (true)
			{
				_compressor.OutputBuffer = _compressionBuffer;
				_compressor.AvailableBytesOut = _compressionBuffer.Length - _compressor.NextOut;
				var rc = _compressor.Deflate(Ionic.Zlib.FlushType.Sync);
				if (rc != Ionic.Zlib.ZlibConstants.Z_OK)
					throw new IOException($"Error '{rc}' while compressing the data.");
				if (_compressor.AvailableBytesIn > 0 || _compressor.AvailableBytesOut == 0)
				{
					ResizeBuffer(ref _compressionBuffer);
					continue;
				}
				break;
			}
			return _compressor.NextOut;
		}

		static void ResizeBuffer(ref byte[] buffer)
		{
			Array.Resize(ref buffer, buffer.Length * 2);
		}

		public override bool CanRead => throw new NotSupportedException();
		public override bool CanSeek => throw new NotSupportedException();
		public override bool CanWrite => throw new NotSupportedException();
		public override long Length => throw new NotSupportedException();
		public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

		public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
		public override void SetLength(long value) => throw new NotSupportedException();
	}
}
