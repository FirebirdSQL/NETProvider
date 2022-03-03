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
using FirebirdSql.Data.Client.Native.Handles;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Native;

internal sealed class FesBlob : BlobBase
{
	#region Fields

	private FesDatabase _database;
	private IntPtr[] _statusVector;
	private BlobHandle _blobHandle;

	#endregion

	#region Properties

	public override DatabaseBase Database
	{
		get { return _database; }
	}

	public override int Handle
	{
		get { return _blobHandle.DangerousGetHandle().AsInt(); }
	}

	#endregion

	#region Constructors

	public FesBlob(FesDatabase database, FesTransaction transaction)
		: this(database, transaction, 0)
	{
	}

	public FesBlob(FesDatabase database, FesTransaction transaction, long blobId)
		: base(database)
	{
		_database = database;
		_transaction = transaction;
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

		var dbHandle = _database.HandlePtr;
		var trHandle = ((FesTransaction)_transaction).HandlePtr;

		_database.FbClient.isc_create_blob2(
			_statusVector,
			ref dbHandle,
			ref trHandle,
			ref _blobHandle,
			ref _blobId,
			0,
			new byte[0]);

		_database.ProcessStatusVector(_statusVector);

		RblAddValue(IscCodes.RBL_create);
	}
	protected override ValueTask CreateAsync(CancellationToken cancellationToken = default)
	{
		ClearStatusVector();

		var dbHandle = _database.HandlePtr;
		var trHandle = ((FesTransaction)_transaction).HandlePtr;

		_database.FbClient.isc_create_blob2(
			_statusVector,
			ref dbHandle,
			ref trHandle,
			ref _blobHandle,
			ref _blobId,
			0,
			new byte[0]);

		_database.ProcessStatusVector(_statusVector);

		RblAddValue(IscCodes.RBL_create);

		return ValueTask2.CompletedTask;
	}

	protected override void Open()
	{
		ClearStatusVector();

		var dbHandle = _database.HandlePtr;
		var trHandle = ((FesTransaction)_transaction).HandlePtr;

		_database.FbClient.isc_open_blob2(
			_statusVector,
			ref dbHandle,
			ref trHandle,
			ref _blobHandle,
			ref _blobId,
			0,
			new byte[0]);

		_database.ProcessStatusVector(_statusVector);
	}
	protected override ValueTask OpenAsync(CancellationToken cancellationToken = default)
	{
		ClearStatusVector();

		var dbHandle = _database.HandlePtr;
		var trHandle = ((FesTransaction)_transaction).HandlePtr;

		_database.FbClient.isc_open_blob2(
			_statusVector,
			ref dbHandle,
			ref trHandle,
			ref _blobHandle,
			ref _blobId,
			0,
			new byte[0]);

		_database.ProcessStatusVector(_statusVector);

		return ValueTask2.CompletedTask;
	}

	protected override void GetSegment(Stream stream)
	{
		var requested = (short)SegmentSize;
		short segmentLength = 0;

		ClearStatusVector();

		var tmp = new byte[requested];

		var status = _database.FbClient.isc_get_segment(
			_statusVector,
			ref _blobHandle,
			ref segmentLength,
			requested,
			tmp);


		RblRemoveValue(IscCodes.RBL_segment);

		if (_statusVector[1] == new IntPtr(IscCodes.isc_segstr_eof))
		{
			RblAddValue(IscCodes.RBL_eof_pending);
			return;
		}
		else
		{
			if (status == IntPtr.Zero || _statusVector[1] == new IntPtr(IscCodes.isc_segment))
			{
				RblAddValue(IscCodes.RBL_segment);
			}
			else
			{
				_database.ProcessStatusVector(_statusVector);
			}
		}

		stream.Write(tmp, 0, segmentLength);
	}
	protected override ValueTask GetSegmentAsync(Stream stream, CancellationToken cancellationToken = default)
	{
		var requested = (short)SegmentSize;
		short segmentLength = 0;

		ClearStatusVector();

		var tmp = new byte[requested];

		var status = _database.FbClient.isc_get_segment(
			_statusVector,
			ref _blobHandle,
			ref segmentLength,
			requested,
			tmp);


		RblRemoveValue(IscCodes.RBL_segment);

		if (_statusVector[1] == new IntPtr(IscCodes.isc_segstr_eof))
		{
			RblAddValue(IscCodes.RBL_eof_pending);
			return ValueTask2.CompletedTask;
		}
		else
		{
			if (status == IntPtr.Zero || _statusVector[1] == new IntPtr(IscCodes.isc_segment))
			{
				RblAddValue(IscCodes.RBL_segment);
			}
			else
			{
				_database.ProcessStatusVector(_statusVector);
			}
		}

		stream.Write(tmp, 0, segmentLength);

		return ValueTask2.CompletedTask;
	}

	protected override void PutSegment(byte[] buffer)
	{
		ClearStatusVector();

		_database.FbClient.isc_put_segment(
			_statusVector,
			ref _blobHandle,
			(short)buffer.Length,
			buffer);

		_database.ProcessStatusVector(_statusVector);
	}
	protected override ValueTask PutSegmentAsync(byte[] buffer, CancellationToken cancellationToken = default)
	{
		ClearStatusVector();

		_database.FbClient.isc_put_segment(
			_statusVector,
			ref _blobHandle,
			(short)buffer.Length,
			buffer);

		_database.ProcessStatusVector(_statusVector);

		return ValueTask2.CompletedTask;
	}

	protected override void Seek(int position)
	{
		throw new NotSupportedException();
	}
	protected override ValueTask SeekAsync(int position, CancellationToken cancellationToken = default)
	{
		throw new NotSupportedException();
	}

	protected override void Close()
	{
		ClearStatusVector();

		_database.FbClient.isc_close_blob(_statusVector, ref _blobHandle);

		_database.ProcessStatusVector(_statusVector);
	}
	protected override ValueTask CloseAsync(CancellationToken cancellationToken = default)
	{
		ClearStatusVector();

		_database.FbClient.isc_close_blob(_statusVector, ref _blobHandle);

		_database.ProcessStatusVector(_statusVector);

		return ValueTask2.CompletedTask;
	}

	protected override void Cancel()
	{
		ClearStatusVector();

		_database.FbClient.isc_cancel_blob(_statusVector, ref _blobHandle);

		_database.ProcessStatusVector(_statusVector);
	}
	protected override ValueTask CancelAsync(CancellationToken cancellationToken = default)
	{
		ClearStatusVector();

		_database.FbClient.isc_cancel_blob(_statusVector, ref _blobHandle);

		_database.ProcessStatusVector(_statusVector);

		return ValueTask2.CompletedTask;
	}

	#endregion

	#region Private Methods

	private void ClearStatusVector()
	{
		Array.Clear(_statusVector, 0, _statusVector.Length);
	}

	#endregion
}
