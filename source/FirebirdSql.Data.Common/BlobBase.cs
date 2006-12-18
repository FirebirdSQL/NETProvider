/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.ibphoenix.com/main.nfs?a=ibphoenix&l=;PAGES;NAME='ibp_idpl'
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Data;
using System.Text;
using System.IO;
using System.Collections;

namespace FirebirdSql.Data.Common
{
#if (SINGLE_DLL)
	internal
#else
	public
#endif
	abstract class BlobBase
	{
		#region Fields

		private	int			rblFlags;
		private Encoding	encoding;
		private int			segmentSize;

		#endregion

		#region Protected Fields

		protected long			blobId;
		protected int			blobHandle;
		protected int			position;
		protected ITransaction	transaction;

		#endregion

		#region Properties

		public int Handle
		{
			get { return this.blobHandle; }
		}

		public long Id
		{
			get { return this.blobId; }
		}

		public bool EOF
		{
			get { return (this.rblFlags & IscCodes.RBL_eof_pending) != 0; }
		}

		#endregion

		#region Protected Properties

		protected Encoding Encoding
		{
			get { return this.encoding; }
			set { this.encoding = value; }
		}

		protected int SegmentSize
		{
			get { return this.segmentSize; }
			set { this.segmentSize = value; }
		}

		#endregion

		#region Abstract Properties

		public abstract IDbAttachment DB
		{
			get;
		}

		#endregion

		#region Constructors

		protected BlobBase(IDbAttachment db)
		{
			this.segmentSize	= db.Parameters.PacketSize;
			this.encoding		= db.Parameters.Charset.Encoding;
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

		#region Methods

		public string ReadString()
		{
			return this.encoding.GetString(this.Read());
		}

		public byte[] Read()
		{
			MemoryStream ms = new MemoryStream();
						
			try
			{
				this.Open();
			
				while (!EOF)
				{
					byte[] segment = this.GetSegment();
					ms.Write(segment, 0, segment.Length);
				}

				this.Close();
			}
			catch (Exception ex)
			{
				// Cancel the blob and throw the exception
				this.Cancel();
				throw ex;
			}

			return ms.ToArray();
		}

		public void Write(string data)
		{
			this.Write(this.encoding.GetBytes(data));
		}

		public void Write(byte[] buffer)
		{
			this.Write(buffer, 0, buffer.Length);
		}

		public void Write(byte[] buffer, int index, int count)
		{
			try
			{
				this.Create();
			
				byte[]	tmpBuffer = null;

				int	length	= count;
				int	offset	= index;
				int	chunk	= length >= this.segmentSize ? this.segmentSize : length;

				tmpBuffer = new byte[chunk];
			
				while (length > 0)
				{					
					if (chunk > length) 
					{
						chunk	  = (int)length;
						tmpBuffer = new byte[chunk];
					}					
					System.Array.Copy(buffer, offset, tmpBuffer, 0, chunk);					
					this.PutSegment(tmpBuffer);
				
					offset += chunk;
					length -= chunk;
				}

				this.Close();
			}
			catch (Exception ex)
			{
				// Cancel the blob and throw the exception
				this.Cancel();
				throw ex;
			}
		}

		#endregion

		#region Protected Methods

		protected void RblAddValue(int rblValue)
		{
			this.rblFlags |= rblValue;
		}

		protected void RblRemoveValue(int rblValue)
		{
			this.rblFlags &= ~rblValue;
		}

		#endregion
	}
}