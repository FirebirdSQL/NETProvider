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
using FirebirdSql.Data.Client.Native.Handle;

namespace FirebirdSql.Data.Client.Native
{
	internal sealed class FesBlob : BlobBase
	{
		#region Fields

		private FesDatabase _db;
		private IntPtr[] _statusVector;
		private BlobHandle _blobHandle;

		#endregion

		#region Properties

		public override IDatabase Database
		{
			get { return _db; }
		}

		public override int Handle
		{
			get { return _blobHandle.DangerousGetHandle().AsInt(); }
		}

		#endregion

		#region Constructors

		public FesBlob(IDatabase db, TransactionBase transaction)
			: this(db, transaction, 0)
		{
		}

		public FesBlob(IDatabase db, TransactionBase transaction, long blobId)
			: base(db)
		{
			if (!(db is FesDatabase))
			{
				throw new ArgumentException($"Specified argument is not of {nameof(FesDatabase)} type.");
			}
			if (!(transaction is FesTransaction))
			{
				throw new ArgumentException($"Specified argument is not of {nameof(FesTransaction)} type.");
			}

			_db = (FesDatabase)db;
			_transaction = (FesTransaction)transaction;
			_position = 0;
			_blobHandle = new BlobHandle();
			_blobId = blobId;
			_statusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];
		}

		#endregion

		#region Protected Methods

		protected override void Create()
		{
			ClearStatusVector();

			DatabaseHandle dbHandle = _db.HandlePtr;
			TransactionHandle trHandle = ((FesTransaction)_transaction).HandlePtr;

			_db.FbClient.isc_create_blob2(
				_statusVector,
				ref dbHandle,
				ref trHandle,
				ref _blobHandle,
				ref _blobId,
				0,
				new byte[0]);

			_db.ProcessStatusVector(_statusVector);

			RblAddValue(IscCodes.RBL_create);
		}

		protected override void Open()
		{
			ClearStatusVector();

			DatabaseHandle dbHandle = _db.HandlePtr;
			TransactionHandle trHandle = ((FesTransaction)_transaction).HandlePtr;

			_db.FbClient.isc_open_blob2(
				_statusVector,
				ref dbHandle,
				ref trHandle,
				ref _blobHandle,
				ref _blobId,
				0,
				new byte[0]);

			_db.ProcessStatusVector(_statusVector);
		}

		protected override byte[] GetSegment()
		{
			short requested = (short)SegmentSize;
			short segmentLength = 0;

			ClearStatusVector();

			using (MemoryStream segment = new MemoryStream())
			{
				byte[] tmp = new byte[requested];

				IntPtr status = _db.FbClient.isc_get_segment(
					_statusVector,
					ref _blobHandle,
					ref segmentLength,
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
						_db.ProcessStatusVector(_statusVector);
					}
				}

				return segment.ToArray();
			}
		}

		protected override void PutSegment(byte[] buffer)
		{
			ClearStatusVector();

			_db.FbClient.isc_put_segment(
				_statusVector,
				ref _blobHandle,
				(short)buffer.Length,
				buffer);

			_db.ProcessStatusVector(_statusVector);
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
			ClearStatusVector();

			_db.FbClient.isc_close_blob(_statusVector, ref _blobHandle);

			_db.ProcessStatusVector(_statusVector);
		}

		protected override void Cancel()
		{
			ClearStatusVector();

			_db.FbClient.isc_cancel_blob(_statusVector, ref _blobHandle);

			_db.ProcessStatusVector(_statusVector);
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
