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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FirebirdSql.Data.Common;

internal abstract class BlobBase
{
	private int _rblFlags;
	private Charset _charset;
	private int _segmentSize;

	protected long _blobId;
	protected int _position;
	protected TransactionBase _transaction;

	public abstract int Handle { get; }
	public long Id => _blobId;
	public bool EOF => (_rblFlags & IscCodes.RBL_eof_pending) != 0;

	protected int SegmentSize => _segmentSize;

	public abstract DatabaseBase Database { get; }

	protected BlobBase(DatabaseBase db)
	{
		_segmentSize = db.PacketSize;
		_charset = db.Charset;
	}

	public string ReadString()
	{
		var buffer = Read();
		return _charset.GetString(buffer, 0, buffer.Length);
	}
	public async ValueTask<string> ReadStringAsync(CancellationToken cancellationToken = default)
	{
		var buffer = await ReadAsync(cancellationToken).ConfigureAwait(false);
		return _charset.GetString(buffer, 0, buffer.Length);
	}

	public byte[] Read()
	{
		using (var ms = new MemoryStream())
		{
			try
			{
				Open();

				while (!EOF)
				{
					GetSegment(ms);
				}

				Close();
			}
			catch
			{
				// Cancel the blob and rethrow the exception
				Cancel();

				throw;
			}

			return ms.ToArray();
		}
	}
	public async ValueTask<byte[]> ReadAsync(CancellationToken cancellationToken = default)
	{
		using (var ms = new MemoryStream())
		{
			try
			{
				await OpenAsync(cancellationToken).ConfigureAwait(false);

				while (!EOF)
				{
					await GetSegmentAsync(ms, cancellationToken).ConfigureAwait(false);
				}

				await CloseAsync(cancellationToken).ConfigureAwait(false);
			}
			catch
			{
				// Cancel the blob and rethrow the exception
				await CancelAsync(cancellationToken).ConfigureAwait(false);

				throw;
			}

			return ms.ToArray();
		}
	}

	public void Write(string data)
	{
		Write(_charset.GetBytes(data));
	}
	public ValueTask WriteAsync(string data, CancellationToken cancellationToken = default)
	{
		return WriteAsync(_charset.GetBytes(data), cancellationToken);
	}

	public void Write(byte[] buffer)
	{
		Write(buffer, 0, buffer.Length);
	}
	public ValueTask WriteAsync(byte[] buffer, CancellationToken cancellationToken = default)
	{
		return WriteAsync(buffer, 0, buffer.Length, cancellationToken);
	}

	public void Write(byte[] buffer, int index, int count)
	{
		try
		{
			Create();

			var length = count;
			var offset = index;
			var chunk = length >= _segmentSize ? _segmentSize : length;

			var tmpBuffer = new byte[chunk];

			while (length > 0)
			{
				if (chunk > length)
				{
					chunk = length;
					tmpBuffer = new byte[chunk];
				}

				Array.Copy(buffer, offset, tmpBuffer, 0, chunk);
				PutSegment(tmpBuffer);

				offset += chunk;
				length -= chunk;
			}

			Close();
		}
		catch
		{
			// Cancel the blob and rethrow the exception
			Cancel();

			throw;
		}
	}
	public async ValueTask WriteAsync(byte[] buffer, int index, int count, CancellationToken cancellationToken = default)
	{
		try
		{
			await CreateAsync(cancellationToken).ConfigureAwait(false);

			var length = count;
			var offset = index;
			var chunk = length >= _segmentSize ? _segmentSize : length;

			var tmpBuffer = new byte[chunk];

			while (length > 0)
			{
				if (chunk > length)
				{
					chunk = length;
					tmpBuffer = new byte[chunk];
				}

				Array.Copy(buffer, offset, tmpBuffer, 0, chunk);
				await PutSegmentAsync(tmpBuffer, cancellationToken).ConfigureAwait(false);

				offset += chunk;
				length -= chunk;
			}

			await CloseAsync(cancellationToken).ConfigureAwait(false);
		}
		catch
		{
			// Cancel the blob and rethrow the exception
			await CancelAsync(cancellationToken).ConfigureAwait(false);

			throw;
		}
	}

	protected abstract void Create();
	protected abstract ValueTask CreateAsync(CancellationToken cancellationToken = default);

	protected abstract void Open();
	protected abstract ValueTask OpenAsync(CancellationToken cancellationToken = default);

	protected abstract void GetSegment(Stream stream);
	protected abstract ValueTask GetSegmentAsync(Stream stream, CancellationToken cancellationToken = default);

	protected abstract void PutSegment(byte[] buffer);
	protected abstract ValueTask PutSegmentAsync(byte[] buffer, CancellationToken cancellationToken = default);

	protected abstract void Seek(int position);
	protected abstract ValueTask SeekAsync(int position, CancellationToken cancellationToken = default);

	protected abstract void Close();
	protected abstract ValueTask CloseAsync(CancellationToken cancellationToken = default);

	protected abstract void Cancel();
	protected abstract ValueTask CancelAsync(CancellationToken cancellationToken = default);

	protected void RblAddValue(int rblValue)
	{
		_rblFlags |= rblValue;
	}

	protected void RblRemoveValue(int rblValue)
	{
		_rblFlags &= ~rblValue;
	}
}
