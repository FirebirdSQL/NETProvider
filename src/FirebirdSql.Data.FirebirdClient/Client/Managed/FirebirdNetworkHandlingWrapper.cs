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
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed;

sealed class FirebirdNetworkHandlingWrapper : IDataProvider, ITracksIOFailure
{
	public const string CompressionName = "zlib";
	public const string EncryptionName = "Arc4";

	const int PreferredBufferSize = 32 * 1024;

	readonly IDataProvider _dataProvider;

	readonly Queue<byte> _outputBuffer;
	readonly Queue<byte> _inputBuffer;
	readonly byte[] _readBuffer;

	byte[] _compressionBuffer;
	Ionic.Zlib.ZlibCodec _compressor;
	Ionic.Zlib.ZlibCodec _decompressor;

	Org.BouncyCastle.Crypto.Engines.RC4Engine _decryptor;
	Org.BouncyCastle.Crypto.Engines.RC4Engine _encryptor;

	public FirebirdNetworkHandlingWrapper(IDataProvider dataProvider)
	{
		_dataProvider = dataProvider;

		_outputBuffer = new Queue<byte>(PreferredBufferSize);
		_inputBuffer = new Queue<byte>(PreferredBufferSize);
		_readBuffer = new byte[PreferredBufferSize];
	}

	public bool IOFailed { get; set; }

	public int Read(byte[] buffer, int offset, int count)
	{
		if (_inputBuffer.Count < count)
		{
			var readBuffer = _readBuffer;
			int read;
			try
			{
				read = _dataProvider.Read(readBuffer, 0, readBuffer.Length);
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
	public async ValueTask<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
	{
		if (_inputBuffer.Count < count)
		{
			var readBuffer = _readBuffer;
			int read;
			try
			{
				read = await _dataProvider.ReadAsync(readBuffer, 0, readBuffer.Length, cancellationToken).ConfigureAwait(false);
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

	public void Write(byte[] buffer, int offset, int count)
	{
		for (var i = offset; i < count; i++)
			_outputBuffer.Enqueue(buffer[offset + i]);
	}
	public ValueTask WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
	{
		for (var i = offset; i < count; i++)
			_outputBuffer.Enqueue(buffer[offset + i]);
		return ValueTask2.CompletedTask;
	}

	public void Flush()
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
			_dataProvider.Write(buffer, 0, count);
			_dataProvider.Flush();
		}
		catch (IOException)
		{
			IOFailed = true;
			throw;
		}
	}
	public async ValueTask FlushAsync(CancellationToken cancellationToken = default)
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
			await _dataProvider.WriteAsync(buffer, 0, count, cancellationToken).ConfigureAwait(false);
			await _dataProvider.FlushAsync(cancellationToken).ConfigureAwait(false);
		}
		catch (IOException)
		{
			IOFailed = true;
			throw;
		}
	}

	public void StartCompression()
	{
		_compressionBuffer = new byte[PreferredBufferSize];
		_compressor = new Ionic.Zlib.ZlibCodec(Ionic.Zlib.CompressionMode.Compress);
		_decompressor = new Ionic.Zlib.ZlibCodec(Ionic.Zlib.CompressionMode.Decompress);
	}

	public void StartEncryption(byte[] key)
	{
		_encryptor = CreateCipher(key);
		_decryptor = CreateCipher(key);
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

	static Org.BouncyCastle.Crypto.Engines.RC4Engine CreateCipher(byte[] key)
	{
		var cipher = new Org.BouncyCastle.Crypto.Engines.RC4Engine();
		cipher.Init(default, new Org.BouncyCastle.Crypto.Parameters.KeyParameter(key));
		return cipher;
	}
}
