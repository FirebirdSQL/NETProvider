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

	public override void Create()
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

		_isOpen = true;

		RblAddValue(IscCodes.RBL_create);
	}
	public override ValueTask CreateAsync(CancellationToken cancellationToken = default)
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

		_isOpen = true;

		RblAddValue(IscCodes.RBL_create);

		return ValueTask.CompletedTask;
	}

	public override void Open()
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

		_isOpen = true;
	}
	public override ValueTask OpenAsync(CancellationToken cancellationToken = default)
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

		_isOpen = true;

		return ValueTask.CompletedTask;
	}

	public override int GetLength()
	{
		ClearStatusVector();

		var buffer = new byte[20];

		_database.FbClient.isc_blob_info(
			_statusVector,
			ref _blobHandle,
			1,
			new byte[] { IscCodes.isc_info_blob_total_length },
			(short)buffer.Length,
			buffer);

		_database.ProcessStatusVector(_statusVector);

		var length = IscHelper.VaxInteger(buffer, 1, 2);
		var size = IscHelper.VaxInteger(buffer, 3, (int)length);

		return (int)size;
	}

	public override ValueTask<int> GetLengthAsync(CancellationToken cancellationToken = default)
	{
		ClearStatusVector();

		var buffer = new byte[20];

		_database.FbClient.isc_blob_info(
			_statusVector,
			ref _blobHandle,
			1,
			new byte[] { IscCodes.isc_info_blob_total_length },
			(short)buffer.Length,
			buffer);

		_database.ProcessStatusVector(_statusVector);

		var length = IscHelper.VaxInteger(buffer, 1, 2);
		var size = IscHelper.VaxInteger(buffer, 3, (int)length);

		return ValueTask.FromResult((int)size);
	}

	public override void GetSegment(Stream stream)
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
	public override ValueTask GetSegmentAsync(Stream stream, CancellationToken cancellationToken = default)
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
			return ValueTask.CompletedTask;
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

		return ValueTask.CompletedTask;
	}

	public override byte[] GetSegment()
	{
		var requested = (short)(SegmentSize - 2);
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
			return Array.Empty<byte>();
		}

		if (status == IntPtr.Zero || _statusVector[1] == new IntPtr(IscCodes.isc_segment))
		{
			RblAddValue(IscCodes.RBL_segment);
		}
		else
		{
			_database.ProcessStatusVector(_statusVector);
		}

		var actualSegment = tmp;
		if (actualSegment.Length != segmentLength)
		{
			tmp = new byte[segmentLength];
			Array.Copy(actualSegment, tmp, segmentLength);
			actualSegment = tmp;
		}

		return actualSegment;
	}
	public override ValueTask<byte[]> GetSegmentAsync(CancellationToken cancellationToken = default)
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
			return ValueTask.FromResult(Array.Empty<byte>());
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

		var actualSegment = tmp;
		if (actualSegment.Length != segmentLength)
		{
			tmp = new byte[segmentLength];
			Array.Copy(actualSegment, tmp, segmentLength);
			actualSegment = tmp;
		}

		return ValueTask.FromResult(actualSegment);
	}

	public override void PutSegment(byte[] buffer)
	{
		ClearStatusVector();

		_database.FbClient.isc_put_segment(
			_statusVector,
			ref _blobHandle,
			(short)buffer.Length,
			buffer);

		_database.ProcessStatusVector(_statusVector);
	}
	public override ValueTask PutSegmentAsync(byte[] buffer, CancellationToken cancellationToken = default)
	{
		ClearStatusVector();

		_database.FbClient.isc_put_segment(
			_statusVector,
			ref _blobHandle,
			(short)buffer.Length,
			buffer);

		_database.ProcessStatusVector(_statusVector);

		return ValueTask.CompletedTask;
	}

	public override void Seek(int position, int seekOperation)
	{
		ClearStatusVector();

		var resultingPosition = 0;
		_database.FbClient.isc_seek_blob(
			_statusVector,
			ref _blobHandle,
			(short)seekOperation,
			position,
			ref resultingPosition);

		_database.ProcessStatusVector(_statusVector);
	}
	public override ValueTask SeekAsync(int position, int seekOperation, CancellationToken cancellationToken = default)
	{
		ClearStatusVector();

		var resultingPosition = 0;
		_database.FbClient.isc_seek_blob(
			_statusVector,
			ref _blobHandle,
			(short)seekOperation,
			position,
			ref resultingPosition);

		_database.ProcessStatusVector(_statusVector);

		return ValueTask.CompletedTask;
	}

	public override void Close()
	{
		ClearStatusVector();

		_database.FbClient.isc_close_blob(_statusVector, ref _blobHandle);

		_database.ProcessStatusVector(_statusVector);
	}
	public override ValueTask CloseAsync(CancellationToken cancellationToken = default)
	{
		ClearStatusVector();

		_database.FbClient.isc_close_blob(_statusVector, ref _blobHandle);

		_database.ProcessStatusVector(_statusVector);

		return ValueTask.CompletedTask;
	}

	public override void Cancel()
	{
		ClearStatusVector();

		_database.FbClient.isc_cancel_blob(_statusVector, ref _blobHandle);

		_database.ProcessStatusVector(_statusVector);
	}
	public override ValueTask CancelAsync(CancellationToken cancellationToken = default)
	{
		ClearStatusVector();

		_database.FbClient.isc_cancel_blob(_statusVector, ref _blobHandle);

		_database.ProcessStatusVector(_statusVector);

		return ValueTask.CompletedTask;
	}

	#endregion

	#region Private Methods

	private void ClearStatusVector()
	{
		Array.Clear(_statusVector, 0, _statusVector.Length);
	}

	#endregion
}
