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
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed;

sealed class FirebirdNetworkHandlingWrapper : IDataProvider, ITracksIOFailure, IDisposable
{
	public const string CompressionName = "zlib";
	public const string EncryptionName = "Arc4";

	const int PreferredBufferSize = 32 * 1024;
	const int DirectReadWriteThreshold = 8 * 1024;

	readonly IDataProvider _dataProvider;

	readonly ByteRingBuffer _outputBuffer;
	readonly ByteRingBuffer _inputBuffer;
	readonly byte[] _readBuffer;

	byte[] _compressionBuffer;
	Ionic.Zlib.ZlibCodec _compressor;
	Ionic.Zlib.ZlibCodec _decompressor;

	Org.BouncyCastle.Crypto.Engines.RC4Engine _decryptor;
	Org.BouncyCastle.Crypto.Engines.RC4Engine _encryptor;

	bool _disposed;

	public FirebirdNetworkHandlingWrapper(IDataProvider dataProvider)
	{
		_dataProvider = dataProvider;

		_outputBuffer = new ByteRingBuffer(PreferredBufferSize);
		_inputBuffer = new ByteRingBuffer(PreferredBufferSize);
		_readBuffer = new byte[PreferredBufferSize];
		_disposed = false;
	}

	public bool IOFailed { get; set; }

	public int Read(byte[] buffer, int offset, int count)
	{
		EnsureNotDisposed();
		if (count <= 0)
			return 0;

		if (_inputBuffer.Count == 0 && _decompressor == null && count >= DirectReadWriteThreshold)
		{
			int read;
			try
			{
				read = _dataProvider.Read(buffer, offset, count);
			}
			catch (IOException)
			{
				IOFailed = true;
				throw;
			}

			if (read > 0 && _decryptor != null)
			{
				_decryptor.ProcessBytes(buffer, offset, read, buffer, offset);
			}
			return read;
		}

		if (_inputBuffer.Count < count)
		{
			FillInputBuffer();
		}

		return _inputBuffer.CopyTo(buffer.AsSpan(offset, count));
	}

	public int Read(Span<byte> buffer, int offset, int count)
	{
		EnsureNotDisposed();
		if (count <= 0)
			return 0;

		// Cannot decrypt into arbitrary spans (BouncyCastle API is byte[] based),
		// so bypass only when no transforms are active.
		if (_inputBuffer.Count == 0 && _decompressor == null && _decryptor == null && count >= DirectReadWriteThreshold)
		{
			try
			{
				return _dataProvider.Read(buffer, offset, count);
			}
			catch (IOException) {
				IOFailed = true;
				throw;
			}
		}

		if (_inputBuffer.Count < count)
		{
			FillInputBuffer();
		}

		return _inputBuffer.CopyTo(buffer.Slice(offset, count));
	}

	public async ValueTask<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
	{
		EnsureNotDisposed();
		if (count <= 0)
			return 0;

		if (_inputBuffer.Count == 0 && _decompressor == null && count >= DirectReadWriteThreshold)
		{
			int read;
			try
			{
				read = await _dataProvider.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
			}
			catch (IOException)
			{
				IOFailed = true;
				throw;
			}

			if (read > 0 && _decryptor != null)
			{
				_decryptor.ProcessBytes(buffer, offset, read, buffer, offset);
			}
			return read;
		}

		if (_inputBuffer.Count < count)
		{
			await FillInputBufferAsync(cancellationToken).ConfigureAwait(false);
		}

		return _inputBuffer.CopyTo(buffer.AsSpan(offset, count));
	}

	public async ValueTask<int> ReadAsync(Memory<byte> buffer, int offset, int count, CancellationToken cancellationToken = default)
	{
		EnsureNotDisposed();
		if (count <= 0)
			return 0;

		var destination = buffer.Slice(offset, count);
		if (_inputBuffer.Count == 0 && _decompressor == null && count >= DirectReadWriteThreshold
			&& MemoryMarshal.TryGetArray(destination, out ArraySegment<byte> segment))
		{
			int read;
			try
			{
				read = await _dataProvider.ReadAsync(segment.Array, segment.Offset, segment.Count, cancellationToken).ConfigureAwait(false);
			}
			catch (IOException)
			{
				IOFailed = true;
				throw;
			}

			if (read > 0 && _decryptor != null)
			{
				_decryptor.ProcessBytes(segment.Array, segment.Offset, read, segment.Array, segment.Offset);
			}
			return read;
		}

		if (_inputBuffer.Count < count)
		{
			await FillInputBufferAsync(cancellationToken).ConfigureAwait(false);
		}

		return _inputBuffer.CopyTo(destination.Span);
	}

	public void Write(ReadOnlySpan<byte> buffer)
	{
		EnsureNotDisposed();
		if (buffer.IsEmpty)
			return;

		if (_compressor == null && _encryptor == null && _outputBuffer.Count == 0 && buffer.Length >= DirectReadWriteThreshold)
		{
			try
			{
				_dataProvider.Write(buffer);
			}
			catch (IOException)
			{
				IOFailed = true;
				throw;
			}
			return;
		}

		_outputBuffer.Write(buffer);
	}

	public void Write(byte[] buffer, int offset, int count)
	{
		EnsureNotDisposed();
		if (buffer == null || count <= 0)
			return;

		if (_compressor == null && _encryptor == null && _outputBuffer.Count == 0 && count >= DirectReadWriteThreshold)
		{
			try
			{
				_dataProvider.Write(buffer, offset, count);
			}
			catch (IOException)
			{
				IOFailed = true;
				throw;
			}
			return;
		}

		_outputBuffer.Write(buffer.AsSpan(offset, count));
	}
	public ValueTask WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
	{
		EnsureNotDisposed();
		if (buffer == null || count <= 0)
			return ValueTask.CompletedTask;

		if (_compressor == null && _encryptor == null && _outputBuffer.Count == 0 && count >= DirectReadWriteThreshold)
		{
			return WriteDirectAsync(buffer, offset, count, cancellationToken);
		}

		_outputBuffer.Write(buffer.AsSpan(offset, count));
		return ValueTask.CompletedTask;

		async ValueTask WriteDirectAsync(byte[] directBuffer, int directOffset, int directCount, CancellationToken directCancellationToken)
		{
			try
			{
				await _dataProvider.WriteAsync(directBuffer, directOffset, directCount, directCancellationToken).ConfigureAwait(false);
			}
			catch (IOException)
			{
				IOFailed = true;
				throw;
			}
		}
	}

	public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, int offset, int count, CancellationToken cancellationToken = default)
	{
		EnsureNotDisposed();
		if (count <= 0)
			return ValueTask.CompletedTask;

		if (_compressor == null && _encryptor == null && _outputBuffer.Count == 0 && count >= DirectReadWriteThreshold)
		{
			return WriteDirectAsync(buffer, offset, count, cancellationToken);
		}

		_outputBuffer.Write(buffer.Span.Slice(offset, count));
		return ValueTask.CompletedTask;

		async ValueTask WriteDirectAsync(ReadOnlyMemory<byte> directBuffer, int directOffset, int directCount, CancellationToken directCancellationToken)
		{
			try
			{
				await _dataProvider.WriteAsync(directBuffer, directOffset, directCount, directCancellationToken).ConfigureAwait(false);
			}
			catch (IOException)
			{
				IOFailed = true;
				throw;
			}
		}
	}

	public void Flush()
	{
		EnsureNotDisposed();
		try
		{
			if (_compressor != null)
			{
				FlushCompressed();
			}
			else if (_outputBuffer.Count > 0)
			{
				FlushPlain();
			}

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
		EnsureNotDisposed();
		try
		{
			if (_compressor != null)
			{
				await FlushCompressedAsync(cancellationToken).ConfigureAwait(false);
			}
			else if (_outputBuffer.Count > 0)
			{
				await FlushPlainAsync(cancellationToken).ConfigureAwait(false);
			}

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
		EnsureNotDisposed();
		_compressionBuffer = new byte[PreferredBufferSize];
		_compressor = new Ionic.Zlib.ZlibCodec(Ionic.Zlib.CompressionMode.Compress);
		_decompressor = new Ionic.Zlib.ZlibCodec(Ionic.Zlib.CompressionMode.Decompress);
	}

	public void StartEncryption(byte[] key)
	{
		EnsureNotDisposed();
		_encryptor = CreateCipher(key);
		_decryptor = CreateCipher(key);
	}

	void FillInputBuffer()
	{
		EnsureNotDisposed();
		try
		{
			if (_decompressor == null)
			{
				_inputBuffer.EnsureFree(1);
				_inputBuffer.GetWriteSegment(out var writeOffset, out var writeLength);
				var toRead = Math.Min(writeLength, _readBuffer.Length);
				var read = _dataProvider.Read(_inputBuffer.Buffer, writeOffset, toRead);
				if (read <= 0)
					return;

				if (_decryptor != null)
				{
					_decryptor.ProcessBytes(_inputBuffer.Buffer, writeOffset, read, _inputBuffer.Buffer, writeOffset);
				}
				_inputBuffer.AdvanceWrite(read);
			}
			else
			{
				var read = _dataProvider.Read(_readBuffer, 0, _readBuffer.Length);
				if (read <= 0)
					return;

				if (_decryptor != null)
				{
					_decryptor.ProcessBytes(_readBuffer, 0, read, _readBuffer, 0);
				}
				read = HandleDecompression(_readBuffer, read);
				_inputBuffer.Write(_compressionBuffer.AsSpan(0, read));
			}
		}
		catch (IOException)
		{
			IOFailed = true;
			throw;
		}
	}

	async ValueTask FillInputBufferAsync(CancellationToken cancellationToken)
	{
		EnsureNotDisposed();
		try
		{
			if (_decompressor == null)
			{
				_inputBuffer.EnsureFree(1);
				_inputBuffer.GetWriteSegment(out var writeOffset, out var writeLength);
				var toRead = Math.Min(writeLength, _readBuffer.Length);
				var read = await _dataProvider.ReadAsync(_inputBuffer.Buffer, writeOffset, toRead, cancellationToken).ConfigureAwait(false);
				if (read <= 0)
					return;

				if (_decryptor != null)
				{
					_decryptor.ProcessBytes(_inputBuffer.Buffer, writeOffset, read, _inputBuffer.Buffer, writeOffset);
				}
				_inputBuffer.AdvanceWrite(read);
			}
			else
			{
				var read = await _dataProvider.ReadAsync(_readBuffer, 0, _readBuffer.Length, cancellationToken).ConfigureAwait(false);
				if (read <= 0)
					return;

				if (_decryptor != null)
				{
					_decryptor.ProcessBytes(_readBuffer, 0, read, _readBuffer, 0);
				}
				read = HandleDecompression(_readBuffer, read);
				_inputBuffer.Write(_compressionBuffer.AsSpan(0, read));
			}
		}
		catch (IOException)
		{
			IOFailed = true;
			throw;
		}
	}

	int HandleDecompression(byte[] buffer, int count)
	{
		EnsureNotDisposed();
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

	static void ResizeBuffer(ref byte[] buffer)
	{
		Array.Resize(ref buffer, buffer.Length * 2);
	}

	void FlushPlain()
	{
		EnsureNotDisposed();
		_outputBuffer.GetReadSegments(out var off1, out var len1, out var off2, out var len2);

		try
		{
			if (_encryptor != null)
			{
				if (len1 > 0)
				{
					_encryptor.ProcessBytes(_outputBuffer.Buffer, off1, len1, _outputBuffer.Buffer, off1);
				}
				if (len2 > 0)
				{
					_encryptor.ProcessBytes(_outputBuffer.Buffer, off2, len2, _outputBuffer.Buffer, off2);
				}
			}

			if (len1 > 0)
			{
				_dataProvider.Write(_outputBuffer.Buffer, off1, len1);
			}
			if (len2 > 0)
			{
				_dataProvider.Write(_outputBuffer.Buffer, off2, len2);
			}
		}
		finally
		{
			_outputBuffer.Consume(len1 + len2);
		}
	}

	async ValueTask FlushPlainAsync(CancellationToken cancellationToken)
	{
		EnsureNotDisposed();
		_outputBuffer.GetReadSegments(out var off1, out var len1, out var off2, out var len2);

		try
		{
			if (_encryptor != null)
			{
				if (len1 > 0)
				{
					_encryptor.ProcessBytes(_outputBuffer.Buffer, off1, len1, _outputBuffer.Buffer, off1);
				}
				if (len2 > 0)
				{
					_encryptor.ProcessBytes(_outputBuffer.Buffer, off2, len2, _outputBuffer.Buffer, off2);
				}
			}

			if (len1 > 0)
			{
				await _dataProvider.WriteAsync(_outputBuffer.Buffer, off1, len1, cancellationToken).ConfigureAwait(false);
			}
			if (len2 > 0)
			{
				await _dataProvider.WriteAsync(_outputBuffer.Buffer, off2, len2, cancellationToken).ConfigureAwait(false);
			}
		}
		finally
		{
			_outputBuffer.Consume(len1 + len2);
		}
	}

	void FlushCompressed()
	{
		EnsureNotDisposed();
		_outputBuffer.GetReadSegments(out var off1, out var len1, out var off2, out var len2);
		try
		{
			if (len1 > 0)
			{
				DeflateAndWrite(_outputBuffer.Buffer, off1, len1, Ionic.Zlib.FlushType.None);
			}
			if (len2 > 0)
			{
				DeflateAndWrite(_outputBuffer.Buffer, off2, len2, Ionic.Zlib.FlushType.None);
			}
			DeflateAndWrite(Array.Empty<byte>(), 0, 0, Ionic.Zlib.FlushType.Sync);
		}
		finally
		{
			_outputBuffer.Consume(len1 + len2);
		}
	}

	async ValueTask FlushCompressedAsync(CancellationToken cancellationToken)
	{
		EnsureNotDisposed();
		_outputBuffer.GetReadSegments(out var off1, out var len1, out var off2, out var len2);
		try
		{
			if (len1 > 0)
			{
				await DeflateAndWriteAsync(_outputBuffer.Buffer, off1, len1, Ionic.Zlib.FlushType.None, cancellationToken).ConfigureAwait(false);
			}
			if (len2 > 0)
			{
				await DeflateAndWriteAsync(_outputBuffer.Buffer, off2, len2, Ionic.Zlib.FlushType.None, cancellationToken).ConfigureAwait(false);
			}
			await DeflateAndWriteAsync(Array.Empty<byte>(), 0, 0, Ionic.Zlib.FlushType.Sync, cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			_outputBuffer.Consume(len1 + len2);
		}
	}

	void DeflateAndWrite(byte[] input, int offset, int count, Ionic.Zlib.FlushType flushType)
	{
		EnsureNotDisposed();
		_compressor.InputBuffer = input;
		_compressor.NextIn = offset;
		_compressor.AvailableBytesIn = count;

		while (true)
		{
			_compressor.OutputBuffer = _compressionBuffer;
			_compressor.NextOut = 0;
			_compressor.AvailableBytesOut = _compressionBuffer.Length;
			var rc = _compressor.Deflate(flushType);
			if (rc != Ionic.Zlib.ZlibConstants.Z_OK)
				throw new IOException($"Error '{rc}' while compressing the data.");

			var produced = _compressor.NextOut;
			if (produced > 0)
			{
				if (_encryptor != null)
				{
					_encryptor.ProcessBytes(_compressionBuffer, 0, produced, _compressionBuffer, 0);
				}
				_dataProvider.Write(_compressionBuffer, 0, produced);
			}

			if (_compressor.AvailableBytesIn > 0 || _compressor.AvailableBytesOut == 0)
			{
				continue;
			}
			break;
		}
	}

	async ValueTask DeflateAndWriteAsync(byte[] input, int offset, int count, Ionic.Zlib.FlushType flushType, CancellationToken cancellationToken)
	{
		EnsureNotDisposed();
		_compressor.InputBuffer = input;
		_compressor.NextIn = offset;
		_compressor.AvailableBytesIn = count;

		while (true)
		{
			_compressor.OutputBuffer = _compressionBuffer;
			_compressor.NextOut = 0;
			_compressor.AvailableBytesOut = _compressionBuffer.Length;
			var rc = _compressor.Deflate(flushType);
			if (rc != Ionic.Zlib.ZlibConstants.Z_OK)
				throw new IOException($"Error '{rc}' while compressing the data.");

			var produced = _compressor.NextOut;
			if (produced > 0)
			{
				if (_encryptor != null)
				{
					_encryptor.ProcessBytes(_compressionBuffer, 0, produced, _compressionBuffer, 0);
				}
				await _dataProvider.WriteAsync(_compressionBuffer, 0, produced, cancellationToken).ConfigureAwait(false);
			}

			if (_compressor.AvailableBytesIn > 0 || _compressor.AvailableBytesOut == 0)
			{
				continue;
			}
			break;
		}
	}

	static Org.BouncyCastle.Crypto.Engines.RC4Engine CreateCipher(byte[] key)
	{
		var cipher = new Org.BouncyCastle.Crypto.Engines.RC4Engine();
		cipher.Init(default, new Org.BouncyCastle.Crypto.Parameters.KeyParameter(key));
		return cipher;
	}

	public void Dispose()
	{
		if (_disposed)
			return;
		_disposed = true;

		_inputBuffer.Dispose();
		_outputBuffer.Dispose();

		_compressionBuffer = null;
		_compressor = null;
		_decompressor = null;
		_decryptor = null;
		_encryptor = null;
	}

	void EnsureNotDisposed()
	{
		if (_disposed)
			throw new ObjectDisposedException(nameof(FirebirdNetworkHandlingWrapper));
	}

	sealed class ByteRingBuffer : IDisposable
	{
		byte[] _buffer;
		int _head;
		int _count;
		bool _disposed;

		public byte[] Buffer => _buffer;
		public int Count => _count;

		public ByteRingBuffer(int initialCapacity)
		{
			_buffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
			_head = 0;
			_count = 0;
			_disposed = false;
		}

		public void Dispose()
		{
			if (_disposed)
				return;
			_disposed = true;

			var buffer = _buffer;
			_buffer = Array.Empty<byte>();
			_head = 0;
			_count = 0;

			if (buffer.Length > 0)
			{
				ArrayPool<byte>.Shared.Return(buffer);
			}
		}

		void EnsureNotDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException(nameof(ByteRingBuffer));
		}

		public void EnsureFree(int bytes)
		{
			EnsureNotDisposed();
			if (bytes <= 0)
				return;

			var free = _buffer.Length - _count;
			if (free >= bytes)
				return;

			Grow(_count + bytes);
		}

		void Grow(int requiredCapacity)
		{
			EnsureNotDisposed();
			var newCapacity = _buffer.Length;
			while (newCapacity < requiredCapacity)
			{
				newCapacity *= 2;
			}

				var newBuffer = ArrayPool<byte>.Shared.Rent(newCapacity);

				GetReadSegments(out var off1, out var len1, out var off2, out var len2);
				if (len1 > 0)
				{
					System.Buffer.BlockCopy(_buffer, off1, newBuffer, 0, len1);
				}
				if (len2 > 0)
				{
					System.Buffer.BlockCopy(_buffer, off2, newBuffer, len1, len2);
				}

				ArrayPool<byte>.Shared.Return(_buffer);
				_buffer = newBuffer;
				_head = 0;
		}

		public void Write(ReadOnlySpan<byte> src)
		{
			EnsureNotDisposed();
			if (src.IsEmpty)
				return;

			EnsureFree(src.Length);

			var tail = (_head + _count) % _buffer.Length;
			var len1 = Math.Min(src.Length, _buffer.Length - tail);
			src.Slice(0, len1).CopyTo(_buffer.AsSpan(tail, len1));

			var len2 = src.Length - len1;
			if (len2 > 0)
			{
				src.Slice(len1, len2).CopyTo(_buffer.AsSpan(0, len2));
			}

			_count += src.Length;
		}

		public int CopyTo(Span<byte> dst)
		{
			EnsureNotDisposed();
			if (dst.IsEmpty || _count == 0)
				return 0;

			var toCopy = Math.Min(dst.Length, _count);
			var len1 = Math.Min(toCopy, _buffer.Length - _head);
			_buffer.AsSpan(_head, len1).CopyTo(dst);
			var len2 = toCopy - len1;
			if (len2 > 0)
			{
				_buffer.AsSpan(0, len2).CopyTo(dst.Slice(len1, len2));
			}
			Consume(toCopy);
			return toCopy;
		}

		public void Consume(int bytes)
		{
			EnsureNotDisposed();
			if (bytes <= 0)
				return;

			if (bytes > _count)
				throw new ArgumentOutOfRangeException(nameof(bytes));

			_head = (_head + bytes) % _buffer.Length;
			_count -= bytes;
			if (_count == 0)
			{
				_head = 0;
			}
		}

		public void GetReadSegments(out int offset1, out int length1, out int offset2, out int length2)
		{
			EnsureNotDisposed();
			if (_count == 0)
			{
				offset1 = offset2 = length1 = length2 = 0;
				return;
			}

			offset1 = _head;
			length1 = Math.Min(_count, _buffer.Length - _head);
			offset2 = 0;
			length2 = _count - length1;
		}

		public void GetWriteSegment(out int offset, out int length)
		{
			EnsureNotDisposed();
			if (_count == _buffer.Length)
			{
				offset = 0;
				length = 0;
				return;
			}

			var tail = (_head + _count) % _buffer.Length;
			offset = tail;
			length = tail >= _head ? _buffer.Length - tail : _head - tail;
		}

		public void AdvanceWrite(int bytes)
		{
			EnsureNotDisposed();
			if (bytes <= 0)
				return;

			if (bytes > _buffer.Length - _count)
				throw new ArgumentOutOfRangeException(nameof(bytes));

			_count += bytes;
		}
	}
}
