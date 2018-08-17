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
using System.Data;
using System.Text;
using System.IO;

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

		public string ReadString()
		{
			var buffer = Read();
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
						var segment = GetSegment();
						ms.Write(segment, 0, segment.Length);
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

		public void Write(string data)
		{
			Write(_charset.GetBytes(data));
		}

		public void Write(byte[] buffer)
		{
			Write(buffer, 0, buffer.Length);
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

		protected abstract void Create();
		protected abstract void Open();
		protected abstract byte[] GetSegment();
		protected abstract void PutSegment(byte[] buffer);
		protected abstract void Seek(int position);
		protected abstract void GetBlobInfo();
		protected abstract void Close();
		protected abstract void Cancel();

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
