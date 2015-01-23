/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *	All Rights Reserved.
 *   
 * Contributors:
 *   Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.IO;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Native
{
	internal sealed class FesBlob : BlobBase
	{
		#region Fields

		private FesDatabase db;
		private IntPtr[] statusVector;

		#endregion

		#region Properties

		public override IDatabase Database
		{
			get { return this.db; }
		}

		#endregion

		#region Constructors

		public FesBlob(IDatabase db, ITransaction transaction)
			: this(db, transaction, 0)
		{
		}

		public FesBlob(IDatabase db, ITransaction transaction, long blobId)
			: base(db)
		{
			if (!(db is FesDatabase))
			{
				throw new ArgumentException("Specified argument is not of FesDatabase type.");
			}
			if (!(transaction is FesTransaction))
			{
				throw new ArgumentException("Specified argument is not of FesTransaction type.");
			}

			this.db = (FesDatabase)db;
			this.transaction = (FesTransaction)transaction;
			this.position = 0;
			this.blobHandle = 0;
			this.blobId = blobId;
			this.statusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];
		}

		#endregion

		#region Protected Methods

		protected override void Create()
		{
			lock (this.db)
			{
				// Clear the status vector
				this.ClearStatusVector();

				int dbHandle = this.db.Handle;
				int trHandle = this.transaction.Handle;

				db.FbClient.isc_create_blob2(
					this.statusVector,
					ref	dbHandle,
					ref	trHandle,
					ref	this.blobHandle,
					ref	this.blobId,
					0,
					new byte[0]);

				FesConnection.ParseStatusVector(this.statusVector, this.db.Charset);

				this.RblAddValue(IscCodes.RBL_create);
			}
		}

		protected override void Open()
		{
			lock (this.db)
			{
				// Clear the status vector
				this.ClearStatusVector();

				int dbHandle = this.db.Handle;
				int trHandle = this.transaction.Handle;

				db.FbClient.isc_open_blob2(
					this.statusVector,
					ref	dbHandle,
					ref	trHandle,
					ref	this.blobHandle,
					ref	this.blobId,
					0,
					new byte[0]);

				FesConnection.ParseStatusVector(this.statusVector, this.db.Charset);
			}
		}

		protected override byte[] GetSegment()
		{
			short requested = (short)this.SegmentSize;
			short segmentLength = 0;

			lock (this.db)
			{
				// Clear the status vector
				this.ClearStatusVector();

				using (MemoryStream segment = new MemoryStream())
				{
					byte[] tmp = new byte[requested];

					IntPtr status = db.FbClient.isc_get_segment(
						this.statusVector,
						ref	this.blobHandle,
						ref	segmentLength,
						requested,
						tmp);

					if (segmentLength > 0)
					{
						segment.Write(tmp, 0, segmentLength > requested ? requested : segmentLength);
					}

					this.RblRemoveValue(IscCodes.RBL_segment);

					if (this.statusVector[1] == new IntPtr(IscCodes.isc_segstr_eof))
					{
						segment.SetLength(0);
						this.RblAddValue(IscCodes.RBL_eof_pending);
					}
					else
					{
						if (status == IntPtr.Zero || this.statusVector[1] == new IntPtr(IscCodes.isc_segment))
						{
							this.RblAddValue(IscCodes.RBL_segment);
						}
						else
						{
							this.db.ParseStatusVector(this.statusVector);
						}
					}

					return segment.ToArray();
				}
			}
		}

		protected override void PutSegment(byte[] buffer)
		{
			lock (this.db)
			{
				// Clear the status vector
				this.ClearStatusVector();

				db.FbClient.isc_put_segment(
					this.statusVector,
					ref	this.blobHandle,
					(short)buffer.Length,
					buffer);

				FesConnection.ParseStatusVector(this.statusVector, this.db.Charset);
			}
		}

		protected override void Seek(int position)
		{
			throw new NotSupportedException();
		}

		protected override void GetBlobInfo()
		{
			throw new NotSupportedException();
		}

		protected override void Close()
		{
			lock (this.db)
			{
				// Clear the status vector
				this.ClearStatusVector();

				db.FbClient.isc_close_blob(this.statusVector, ref this.blobHandle);

				FesConnection.ParseStatusVector(this.statusVector, this.db.Charset);
			}
		}

		protected override void Cancel()
		{
			lock (this.db)
			{
				// Clear the status vector
				this.ClearStatusVector();

				db.FbClient.isc_cancel_blob(this.statusVector, ref this.blobHandle);

				FesConnection.ParseStatusVector(this.statusVector, this.db.Charset);
			}
		}

		#endregion

		#region Private Methods

		private void ClearStatusVector()
		{
			Array.Clear(this.statusVector, 0, this.statusVector.Length);
		}

		#endregion
	}
}
