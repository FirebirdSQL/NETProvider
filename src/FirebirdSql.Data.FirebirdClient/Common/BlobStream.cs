using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FirebirdSql.Data.Common;

public sealed class BlobStream : Stream
{
	private readonly BlobBase _blobHandle;
	private int _position;

	private byte[] _currentSegment;
	private int _segmentPosition;

	private int Available => _currentSegment?.Length - _segmentPosition ?? 0;

	internal BlobStream(BlobBase blob)
	{
		_blobHandle = blob;
		_position = 0;
	}

	public override long Position
	{
		get => _position;
		set => Seek(value, SeekOrigin.Begin);
	}

	public override long Length
	{
		get
		{
			if (!_blobHandle.IsOpen)
				_blobHandle.Open();

			return _blobHandle.GetLength();
		}
	}

	public override void Flush()
	{
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		ValidateBufferSize(buffer, offset, count);

		if (!_blobHandle.IsOpen)
			_blobHandle.Open();

		var copied = 0;
		var remainingBufferSize = buffer.Length - offset;
		do
		{
			if (remainingBufferSize == 0)
				break;

			if (Available > 0)
			{
				var toCopy = Math.Min(Available, remainingBufferSize);
				Array.Copy(_currentSegment, _segmentPosition, buffer, offset + copied, toCopy);
				copied += toCopy;
				_segmentPosition += toCopy;
				remainingBufferSize -= toCopy;
				_position += toCopy;
			}

			if (_blobHandle.EOF)
				break;

			if (Available == 0)
			{
				_currentSegment = _blobHandle.GetSegment();
				_segmentPosition = 0;
			}
		} while (copied < count);

		return copied;
	}
	public override async Task<int> ReadAsync(byte[] buffer, int offset, int count,
		CancellationToken cancellationToken)
	{
		ValidateBufferSize(buffer, offset, count);

		if (!_blobHandle.IsOpen)
			await _blobHandle.OpenAsync(cancellationToken).ConfigureAwait(false);

		var copied = 0;
		var remainingBufferSize = buffer.Length - offset;
		do
		{
			if (remainingBufferSize == 0)
				break;

			if (Available > 0)
			{
				var toCopy = Math.Min(Available, remainingBufferSize);
				Array.Copy(_currentSegment, _segmentPosition, buffer, offset + copied, toCopy);
				copied += toCopy;
				_segmentPosition += toCopy;
				remainingBufferSize -= toCopy;
				_position += toCopy;
			}

			if (_blobHandle.EOF)
				break;

			if (Available == 0)
			{
				_currentSegment = await _blobHandle.GetSegmentAsync(cancellationToken).ConfigureAwait(false);
				_segmentPosition = 0;
			}
		} while (copied < count);

		return copied;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		if (!_blobHandle.IsOpen)
			_blobHandle.Open();

		var seekMode = origin switch
		{
			SeekOrigin.Begin => IscCodes.isc_blb_seek_from_head,
			SeekOrigin.Current => IscCodes.isc_blb_seek_relative,
			SeekOrigin.End => IscCodes.isc_blb_seek_from_tail,
			_ => throw new ArgumentOutOfRangeException(nameof(origin))
		};

		_blobHandle.Seek((int)offset, seekMode);
		return _position = _blobHandle.Position;
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		try
		{
			if (!_blobHandle.IsOpen)
				_blobHandle.Create();

			var chunk = count >= _blobHandle.SegmentSize ? _blobHandle.SegmentSize : count;
			var tmpBuffer = new byte[chunk];

			while (count > 0)
			{
				if (chunk > count)
				{
					chunk = count;
					tmpBuffer = new byte[chunk];
				}

				Array.Copy(buffer, offset, tmpBuffer, 0, chunk);
				_blobHandle.PutSegment(tmpBuffer);

				offset += chunk;
				count -= chunk;
				_position += chunk;
			}
		}
		catch
		{
			_blobHandle.Cancel();
			throw;
		}
	}
	public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		try
		{
			if (!_blobHandle.IsOpen)
				await _blobHandle.CreateAsync(cancellationToken).ConfigureAwait(false);

			var chunk = count >= _blobHandle.SegmentSize ? _blobHandle.SegmentSize : count;
			var tmpBuffer = new byte[chunk];

			while (count > 0)
			{
				if (chunk > count)
				{
					chunk = count;
					tmpBuffer = new byte[chunk];
				}

				Array.Copy(buffer, offset, tmpBuffer, 0, chunk);
				await _blobHandle.PutSegmentAsync(tmpBuffer, cancellationToken).ConfigureAwait(false);

				offset += chunk;
				count -= chunk;
				_position += chunk;
			}
		}
		catch
		{
			await _blobHandle.CancelAsync(cancellationToken).ConfigureAwait(false);
			throw;
		}
	}

	public override bool CanRead => true;
	public override bool CanSeek => true;
	public override bool CanWrite => true;

	protected override void Dispose(bool disposing)
	{
		_blobHandle.Close();
	}

	public override ValueTask DisposeAsync()
	{
		return _blobHandle.CloseAsync();
	}

	private static void ValidateBufferSize(byte[] buffer, int offset, int count)
	{
		if (buffer is null)
			throw new ArgumentNullException(nameof(buffer));

		if (buffer.Length < offset + count)
			throw new InvalidOperationException();
	}
}