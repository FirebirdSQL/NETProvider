/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Data;
using System.Text;
using System.IO;
using System.Collections;

namespace FirebirdSql.Data.Common
{
	internal abstract class BlobBase
	{
		#region Fields

		private int		_rblFlags;
		private Charset _charset;
		private int		_segmentSize;

		#endregion

		#region Properties

		public int Handle
		{
			get { return _blobHandle; }
		}

		public long Id
		{
			get { return _blobId; }
		}

		public bool EOF
		{
			get { return (_rblFlags & IscCodes.RBL_eof_pending) != 0; }
		}

		#endregion

		#region Protected Fields

		protected long          _blobId;
		protected int           _blobHandle;
		protected int           _position;
		protected ITransaction  _transaction;

		#endregion

		#region Protected Properties

		protected int SegmentSize
		{
			get { return _segmentSize; }
		}

		#endregion

		#region Abstract Properties

		public abstract IDatabase Database
		{
			get;
		}

		#endregion

		#region Constructors

		protected BlobBase(IDatabase db)
		{
			_segmentSize = db.PacketSize;
			_charset = db.Charset;
		}

		#endregion

		#region Methods

		public string ReadString()
		{
			byte[] buffer = Read();
			return _charset.GetString(buffer, 0, buffer.Length);
		}

		public byte[] Read()
		{
			MemoryStream ms = new MemoryStream();

			try
			{
				Open();

				while (!EOF)
				{
					byte[] segment = GetSegment();
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

				byte[] tmpBuffer = null;

				int length	= count;
				int offset	= index;
				int chunk	= length >= _segmentSize ? _segmentSize : length;

				tmpBuffer = new byte[chunk];

				while (length > 0)
				{
					if (chunk > length)
					{
						chunk = (int)length;
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

		#endregion

		#region Protected Abstract Methods

		protected abstract void Create();
		protected abstract void Open();
		protected abstract byte[] GetSegment();
		protected abstract void PutSegment(byte[] buffer);
		protected abstract void Seek(int position);
		protected abstract void GetBlobInfo();
		protected abstract void Close();
		protected abstract void Cancel();

		#endregion

		#region Protected Methods

		protected void RblAddValue(int rblValue)
		{
			_rblFlags |= rblValue;
		}

		protected void RblRemoveValue(int rblValue)
		{
			_rblFlags &= ~rblValue;
		}

		#endregion
	}
}
