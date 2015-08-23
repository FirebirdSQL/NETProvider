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

		private FesDatabase _db;
		private IntPtr[] _statusVector;

		#endregion

		#region Properties

		public override IDatabase Database
		{
			get { return _db; }
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

			_db = (FesDatabase)db;
			_transaction = (FesTransaction)transaction;
			_position = 0;
			_blobHandle = 0;
			_blobId = blobId;
			_statusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];
		}

		#endregion

		#region Protected Methods

		protected override void Create()
		{
			lock (_db)
			{
				// Clear the status vector
				ClearStatusVector();

				int dbHandle = _db.Handle;
				int trHandle = _transaction.Handle;

				_db.FbClient.isc_create_blob2(
					_statusVector,
					ref	dbHandle,
					ref	trHandle,
					ref _blobHandle,
					ref _blobId,
					0,
					new byte[0]);

				FesConnection.ParseStatusVector(_statusVector, _db.Charset);

				RblAddValue(IscCodes.RBL_create);
			}
		}

		protected override void Open()
		{
			lock (_db)
			{
				// Clear the status vector
				ClearStatusVector();

				int dbHandle = _db.Handle;
				int trHandle = _transaction.Handle;

				_db.FbClient.isc_open_blob2(
					_statusVector,
					ref	dbHandle,
					ref	trHandle,
					ref _blobHandle,
					ref _blobId,
					0,
					new byte[0]);

				FesConnection.ParseStatusVector(_statusVector, _db.Charset);
			}
		}

		protected override byte[] GetSegment()
		{
			short requested = (short)SegmentSize;
			short segmentLength = 0;

			lock (_db)
			{
				// Clear the status vector
				ClearStatusVector();

				using (MemoryStream segment = new MemoryStream())
				{
					byte[] tmp = new byte[requested];

					IntPtr status = _db.FbClient.isc_get_segment(
						_statusVector,
						ref _blobHandle,
						ref	segmentLength,
						requested,
						tmp);

					if (segmentLength > 0)
					{
						segment.Write(tmp, 0, segmentLength > requested ? requested : segmentLength);
					}

					RblRemoveValue(IscCodes.RBL_segment);

					if (_statusVector[1] == new IntPtr(IscCodes.isc_segstr_eof))
					{
						segment.SetLength(0);
						RblAddValue(IscCodes.RBL_eof_pending);
					}
					else
					{
						if (status == IntPtr.Zero || _statusVector[1] == new IntPtr(IscCodes.isc_segment))
						{
							RblAddValue(IscCodes.RBL_segment);
						}
						else
						{
							_db.ParseStatusVector(_statusVector);
						}
					}

					return segment.ToArray();
				}
			}
		}

		protected override void PutSegment(byte[] buffer)
		{
			lock (_db)
			{
				// Clear the status vector
				ClearStatusVector();

				_db.FbClient.isc_put_segment(
					_statusVector,
					ref _blobHandle,
					(short)buffer.Length,
					buffer);

				FesConnection.ParseStatusVector(_statusVector, _db.Charset);
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
			lock (_db)
			{
				// Clear the status vector
				ClearStatusVector();

				_db.FbClient.isc_close_blob(_statusVector, ref _blobHandle);

				FesConnection.ParseStatusVector(_statusVector, _db.Charset);
			}
		}

		protected override void Cancel()
		{
			lock (_db)
			{
				// Clear the status vector
				ClearStatusVector();

				_db.FbClient.isc_cancel_blob(_statusVector, ref _blobHandle);

				FesConnection.ParseStatusVector(_statusVector, _db.Charset);
			}
		}

		#endregion

		#region Private Methods

		private void ClearStatusVector()
		{
			Array.Clear(_statusVector, 0, _statusVector.Length);
		}

		#endregion
	}
}
