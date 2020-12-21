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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.IO;
using System.Threading.Tasks;

namespace FirebirdSql.Data.Common
{
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

		public abstract IDatabase Database { get; }

		protected BlobBase(IDatabase db)
		{
			_segmentSize = db.PacketSize;
			_charset = db.Charset;
		}

		public async Task<string> ReadString(AsyncWrappingCommonArgs async)
		{
			var buffer = await Read(async).ConfigureAwait(false);
			return _charset.GetString(buffer, 0, buffer.Length);
		}

		public async Task<byte[]> Read(AsyncWrappingCommonArgs async)
		{
			using (var ms = new MemoryStream())
			{
				try
				{
					await Open(async).ConfigureAwait(false);

					while (!EOF)
					{
						await GetSegment(ms, async).ConfigureAwait(false);
					}

					await Close(async).ConfigureAwait(false);
				}
				catch
				{
					// Cancel the blob and rethrow the exception
					await Cancel(async).ConfigureAwait(false);

					throw;
				}

				return ms.ToArray();
			}
		}

		public Task Write(string data, AsyncWrappingCommonArgs async)
		{
			return Write(_charset.GetBytes(data), async);
		}

		public Task Write(byte[] buffer, AsyncWrappingCommonArgs async)
		{
			return Write(buffer, 0, buffer.Length, async);
		}

		public async Task Write(byte[] buffer, int index, int count, AsyncWrappingCommonArgs async)
		{
			try
			{
				await Create(async).ConfigureAwait(false);

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
					await PutSegment(tmpBuffer, async).ConfigureAwait(false);

					offset += chunk;
					length -= chunk;
				}

				await Close(async).ConfigureAwait(false);
			}
			catch
			{
				// Cancel the blob and rethrow the exception
				await Cancel(async).ConfigureAwait(false);

				throw;
			}
		}

		protected abstract Task Create(AsyncWrappingCommonArgs async);
		protected abstract Task Open(AsyncWrappingCommonArgs async);
		protected abstract Task GetSegment(Stream stream, AsyncWrappingCommonArgs async);
		protected abstract Task PutSegment(byte[] buffer, AsyncWrappingCommonArgs async);
		protected abstract Task Seek(int position, AsyncWrappingCommonArgs async);
		protected abstract Task Close(AsyncWrappingCommonArgs async);
		protected abstract Task Cancel(AsyncWrappingCommonArgs async);

		protected void RblAddValue(int rblValue)
		{
			_rblFlags |= rblValue;
		}

		protected void RblRemoveValue(int rblValue)
		{
			_rblFlags &= ~rblValue;
		}
	}
}
