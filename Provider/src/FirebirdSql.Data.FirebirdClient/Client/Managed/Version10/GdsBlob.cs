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
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version10
{
	internal sealed class GdsBlob : BlobBase
	{
		const int DataSegment = 0;
		const int SeekMode = 0;

		#region Fields

		private GdsDatabase _database;
		private int _blobHandle;

		#endregion

		#region Properties

		public override IDatabase Database
		{
			get { return _database; }
		}

		public override int Handle
		{
			get { return _blobHandle; }
		}

		#endregion

		#region Constructors

		public GdsBlob(IDatabase db, TransactionBase transaction)
			: this(db, transaction, 0)
		{ }

		public GdsBlob(IDatabase db, TransactionBase transaction, long blobId)
			: base(db)
		{
			if (!(db is GdsDatabase))
			{
				throw new ArgumentException($"Specified argument is not of {nameof(GdsDatabase)} type.");
			}
			if (!(transaction is GdsTransaction))
			{
				throw new ArgumentException($"Specified argument is not of {nameof(GdsTransaction)} type.");
			}

			_database = (GdsDatabase)db;
			_transaction = transaction;
			_position = 0;
			_blobHandle = 0;
			_blobId = blobId;
		}

		#endregion

		#region Protected Methods

		protected override async Task Create(AsyncWrappingCommonArgs async)
		{
			try
			{
				await CreateOrOpen(IscCodes.op_create_blob, null, async).ConfigureAwait(false);
				RblAddValue(IscCodes.RBL_create);
			}
			catch (IscException)
			{
				throw;
			}
		}

		protected override async Task Open(AsyncWrappingCommonArgs async)
		{
			try
			{
				await CreateOrOpen(IscCodes.op_open_blob, null, async).ConfigureAwait(false);
			}
			catch (IscException)
			{
				throw;
			}
		}

		protected override async Task GetSegment(Stream stream, AsyncWrappingCommonArgs async)
		{
			var requested = SegmentSize;

			try
			{
				await _database.Xdr.Write(IscCodes.op_get_segment, async).ConfigureAwait(false);
				await _database.Xdr.Write(_blobHandle, async).ConfigureAwait(false);
				await _database.Xdr.Write(requested < short.MaxValue - 12 ? requested : short.MaxValue - 12, async).ConfigureAwait(false);
				await _database.Xdr.Write(DataSegment, async).ConfigureAwait(false);
				await _database.Xdr.Flush(async).ConfigureAwait(false);

				var response = (GenericResponse)await _database.ReadResponse(async).ConfigureAwait(false);

				RblRemoveValue(IscCodes.RBL_segment);
				if (response.ObjectHandle == 1)
				{
					RblAddValue(IscCodes.RBL_segment);
				}
				else if (response.ObjectHandle == 2)
				{
					RblAddValue(IscCodes.RBL_eof_pending);
				}

				var buffer = response.Data;

				if (buffer.Length == 0)
				{
					// previous	segment	was	last, this has no data
					return;
				}

				var len = 0;
				var srcpos = 0;

				while (srcpos < buffer.Length)
				{
					len = (int)IscHelper.VaxInteger(buffer, srcpos, 2);
					srcpos += 2;

					stream.Write(buffer, srcpos, len);
					srcpos += len;
				}
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		protected override async Task PutSegment(byte[] buffer, AsyncWrappingCommonArgs async)
		{
			try
			{
				await _database.Xdr.Write(IscCodes.op_batch_segments, async).ConfigureAwait(false);
				await _database.Xdr.Write(_blobHandle, async).ConfigureAwait(false);
				await _database.Xdr.WriteBlobBuffer(buffer, async).ConfigureAwait(false);
				await _database.Xdr.Flush(async).ConfigureAwait(false);

				await _database.ReadResponse(async).ConfigureAwait(false);
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		protected override async Task Seek(int position, AsyncWrappingCommonArgs async)
		{
			try
			{
				await _database.Xdr.Write(IscCodes.op_seek_blob, async).ConfigureAwait(false);
				await _database.Xdr.Write(_blobHandle, async).ConfigureAwait(false);
				await _database.Xdr.Write(SeekMode, async).ConfigureAwait(false);
				await _database.Xdr.Write(position, async).ConfigureAwait(false);
				await _database.Xdr.Flush(async).ConfigureAwait(false);

				var response = (GenericResponse)await _database.ReadResponse(async).ConfigureAwait(false);

				_position = response.ObjectHandle;
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		protected override Task Close(AsyncWrappingCommonArgs async)
		{
			return _database.ReleaseObject(IscCodes.op_close_blob, _blobHandle, async);
		}

		protected override Task Cancel(AsyncWrappingCommonArgs async)
		{
			return _database.ReleaseObject(IscCodes.op_cancel_blob, _blobHandle, async);
		}

		#endregion

		#region Private API Methods

		private async Task CreateOrOpen(int op, BlobParameterBuffer bpb, AsyncWrappingCommonArgs async)
		{
			try
			{
				await _database.Xdr.Write(op, async).ConfigureAwait(false);
				if (bpb != null)
				{
					await _database.Xdr.WriteTyped(IscCodes.isc_bpb_version1, bpb.ToArray(), async).ConfigureAwait(false);
				}
				await _database.Xdr.Write(_transaction.Handle, async).ConfigureAwait(false);
				await _database.Xdr.Write(_blobId, async).ConfigureAwait(false);
				await _database.Xdr.Flush(async).ConfigureAwait(false);

				var response = (GenericResponse)await _database.ReadResponse(async).ConfigureAwait(false);

				_blobId = response.BlobId;
				_blobHandle = response.ObjectHandle;
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		#endregion
	}
}
