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
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version10;

internal sealed class GdsBlob : BlobBase
{
	const int DataSegment = 0;
	const int SeekMode = 0;

	#region Fields

	private readonly GdsDatabase _database;
	private int _blobHandle;

	#endregion

	#region Properties

	public override DatabaseBase Database
	{
		get { return _database; }
	}

	public override int Handle
	{
		get { return _blobHandle; }
	}

	#endregion

	#region Constructors

	public GdsBlob(GdsDatabase database, GdsTransaction transaction)
		: this(database, transaction, 0)
	{ }

	public GdsBlob(GdsDatabase database, GdsTransaction transaction, long blobId)
		: base(database)
	{
		_database = database;
		_transaction = transaction;
		_position = 0;
		_blobHandle = 0;
		_blobId = blobId;
	}

	#endregion

	#region Protected Methods

	protected override void Create()
	{
		try
		{
			CreateOrOpen(IscCodes.op_create_blob, null);
			RblAddValue(IscCodes.RBL_create);
		}
		catch (IscException)
		{
			throw;
		}
	}
	protected override async ValueTask CreateAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			await CreateOrOpenAsync(IscCodes.op_create_blob, null, cancellationToken).ConfigureAwait(false);
			RblAddValue(IscCodes.RBL_create);
		}
		catch (IscException)
		{
			throw;
		}
	}

	protected override void Open()
	{
		try
		{
			CreateOrOpen(IscCodes.op_open_blob, null);
		}
		catch (IscException)
		{
			throw;
		}
	}
	protected override async ValueTask OpenAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			await CreateOrOpenAsync(IscCodes.op_open_blob, null, cancellationToken).ConfigureAwait(false);
		}
		catch (IscException)
		{
			throw;
		}
	}

	protected override void GetSegment(Stream stream)
	{
		var requested = SegmentSize;

		try
		{
			_database.Xdr.Write(IscCodes.op_get_segment);
			_database.Xdr.Write(_blobHandle);
			_database.Xdr.Write(requested < short.MaxValue - 12 ? requested : short.MaxValue - 12);
			_database.Xdr.Write(DataSegment);
			_database.Xdr.Flush();

			var response = (GenericResponse)_database.ReadResponse();

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
			throw IscException.ForIOException(ex);
		}
	}
	protected override async ValueTask GetSegmentAsync(Stream stream, CancellationToken cancellationToken = default)
	{
		var requested = SegmentSize;

		try
		{
			await _database.Xdr.WriteAsync(IscCodes.op_get_segment, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(_blobHandle, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(requested < short.MaxValue - 12 ? requested : short.MaxValue - 12, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(DataSegment, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			var response = (GenericResponse)await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false);

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
			throw IscException.ForIOException(ex);
		}
	}

	protected override void PutSegment(byte[] buffer)
	{
		try
		{
			_database.Xdr.Write(IscCodes.op_batch_segments);
			_database.Xdr.Write(_blobHandle);
			_database.Xdr.WriteBlobBuffer(buffer);
			_database.Xdr.Flush();

			_database.ReadResponse();
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	protected override async ValueTask PutSegmentAsync(byte[] buffer, CancellationToken cancellationToken = default)
	{
		try
		{
			await _database.Xdr.WriteAsync(IscCodes.op_batch_segments, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(_blobHandle, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteBlobBufferAsync(buffer, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false);
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	protected override void Seek(int position)
	{
		try
		{
			_database.Xdr.Write(IscCodes.op_seek_blob);
			_database.Xdr.Write(_blobHandle);
			_database.Xdr.Write(SeekMode);
			_database.Xdr.Write(position);
			_database.Xdr.Flush();

			var response = (GenericResponse)_database.ReadResponse();

			_position = response.ObjectHandle;
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	protected override async ValueTask SeekAsync(int position, CancellationToken cancellationToken = default)
	{
		try
		{
			await _database.Xdr.WriteAsync(IscCodes.op_seek_blob, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(_blobHandle, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(SeekMode, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(position, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			var response = (GenericResponse)await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false);

			_position = response.ObjectHandle;
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	protected override void Close()
	{
		_database.ReleaseObject(IscCodes.op_close_blob, _blobHandle);
	}
	protected override ValueTask CloseAsync(CancellationToken cancellationToken = default)
	{
		return _database.ReleaseObjectAsync(IscCodes.op_close_blob, _blobHandle, cancellationToken);
	}

	protected override void Cancel()
	{
		_database.ReleaseObject(IscCodes.op_cancel_blob, _blobHandle);
	}
	protected override ValueTask CancelAsync(CancellationToken cancellationToken = default)
	{
		return _database.ReleaseObjectAsync(IscCodes.op_cancel_blob, _blobHandle, cancellationToken);
	}

	#endregion

	#region Private API Methods

	private void CreateOrOpen(int op, BlobParameterBuffer bpb)
	{
		try
		{
			_database.Xdr.Write(op);
			if (bpb != null)
			{
				_database.Xdr.WriteTyped(IscCodes.isc_bpb_version1, bpb.ToArray());
			}
			_database.Xdr.Write(_transaction.Handle);
			_database.Xdr.Write(_blobId);
			_database.Xdr.Flush();

			var response = (GenericResponse)_database.ReadResponse();

			_blobId = response.BlobId;
			_blobHandle = response.ObjectHandle;
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	private async ValueTask CreateOrOpenAsync(int op, BlobParameterBuffer bpb, CancellationToken cancellationToken = default)
	{
		try
		{
			await _database.Xdr.WriteAsync(op, cancellationToken).ConfigureAwait(false);
			if (bpb != null)
			{
				await _database.Xdr.WriteTypedAsync(IscCodes.isc_bpb_version1, bpb.ToArray(), cancellationToken).ConfigureAwait(false);
			}
			await _database.Xdr.WriteAsync(_transaction.Handle, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(_blobId, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			var response = (GenericResponse)await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false);

			_blobId = response.BlobId;
			_blobHandle = response.ObjectHandle;
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	#endregion
}
