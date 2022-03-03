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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Client.Native.Handles;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Native;

internal sealed class FesDatabase : DatabaseBase
{
	#region Fields

	private static readonly Version Version25 = new Version(2, 5);

	private readonly IFbClient _fbClient;
	private readonly Version _fbClientVersion;
	private DatabaseHandle _handle;
	private IntPtr[] _statusVector;

	#endregion

	#region Properties

	public override bool UseUtf8ParameterBuffer => _fbClientVersion >= Version25;
	public override int Handle => _handle.DangerousGetHandle().AsInt();
	public override bool HasRemoteEventSupport => false;
	public override bool ConnectionBroken => false;
	public IFbClient FbClient => _fbClient;
	public Version FbClientVersion => _fbClientVersion;
	public DatabaseHandle HandlePtr => _handle;

	#endregion

	#region Constructors

	public FesDatabase(string dllName, Charset charset, int packetSize, short dialect)
		: base(charset, packetSize, dialect)
	{
		_fbClient = FbClientFactory.Create(dllName);
		_fbClientVersion = FesConnection.GetClientVersion(_fbClient);
		_handle = new DatabaseHandle();
		_statusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];
	}

	#endregion

	#region Database Methods

	public override void CreateDatabase(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey)
	{
		CheckCryptKeyForSupport(cryptKey);

		var databaseBuffer = dpb.Encoding.GetBytes(database);

		StatusVectorHelper.ClearStatusVector(_statusVector);

		_fbClient.isc_create_database(
			_statusVector,
			(short)databaseBuffer.Length,
			databaseBuffer,
			ref _handle,
			dpb.Length,
			dpb.ToArray(),
			0);

		ProcessStatusVector(Charset.DefaultCharset);
	}
	public override ValueTask CreateDatabaseAsync(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey, CancellationToken cancellationToken = default)
	{
		CheckCryptKeyForSupport(cryptKey);

		var databaseBuffer = dpb.Encoding.GetBytes(database);

		StatusVectorHelper.ClearStatusVector(_statusVector);

		_fbClient.isc_create_database(
			_statusVector,
			(short)databaseBuffer.Length,
			databaseBuffer,
			ref _handle,
			dpb.Length,
			dpb.ToArray(),
			0);

		ProcessStatusVector(Charset.DefaultCharset);

		return ValueTask2.CompletedTask;
	}

	public override void CreateDatabaseWithTrustedAuth(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey)
	{
		throw new NotSupportedException("Trusted Auth isn't supported on Firebird Embedded.");
	}
	public override ValueTask CreateDatabaseWithTrustedAuthAsync(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey, CancellationToken cancellationToken = default)
	{
		throw new NotSupportedException("Trusted Auth isn't supported on Firebird Embedded.");
	}

	public override void DropDatabase()
	{
		StatusVectorHelper.ClearStatusVector(_statusVector);

		_fbClient.isc_drop_database(_statusVector, ref _handle);

		ProcessStatusVector();

		_handle.Dispose();
	}
	public override ValueTask DropDatabaseAsync(CancellationToken cancellationToken = default)
	{
		StatusVectorHelper.ClearStatusVector(_statusVector);

		_fbClient.isc_drop_database(_statusVector, ref _handle);

		ProcessStatusVector();

		_handle.Dispose();

		return ValueTask2.CompletedTask;
	}

	#endregion

	#region Remote Events Methods

	public override void CloseEventManager()
	{
		throw new NotSupportedException();
	}
	public override ValueTask CloseEventManagerAsync(CancellationToken cancellationToken = default)
	{
		throw new NotSupportedException();
	}

	public override void QueueEvents(RemoteEvent events)
	{
		throw new NotSupportedException();
	}
	public override ValueTask QueueEventsAsync(RemoteEvent events, CancellationToken cancellationToken = default)
	{
		throw new NotSupportedException();
	}

	public override void CancelEvents(RemoteEvent events)
	{
		throw new NotSupportedException();
	}
	public override ValueTask CancelEventsAsync(RemoteEvent events, CancellationToken cancellationToken = default)
	{
		throw new NotSupportedException();
	}

	#endregion

	#region Methods

	public override void Attach(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey)
	{
		CheckCryptKeyForSupport(cryptKey);

		var databaseBuffer = dpb.Encoding.GetBytes(database);

		StatusVectorHelper.ClearStatusVector(_statusVector);

		_fbClient.isc_attach_database(
			_statusVector,
			(short)databaseBuffer.Length,
			databaseBuffer,
			ref _handle,
			dpb.Length,
			dpb.ToArray());

		ProcessStatusVector(Charset.DefaultCharset);

		ServerVersion = GetServerVersion();
	}
	public override async ValueTask AttachAsync(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey, CancellationToken cancellationToken = default)
	{
		CheckCryptKeyForSupport(cryptKey);

		var databaseBuffer = dpb.Encoding.GetBytes(database);

		StatusVectorHelper.ClearStatusVector(_statusVector);

		_fbClient.isc_attach_database(
			_statusVector,
			(short)databaseBuffer.Length,
			databaseBuffer,
			ref _handle,
			dpb.Length,
			dpb.ToArray());

		ProcessStatusVector(Charset.DefaultCharset);

		ServerVersion = await GetServerVersionAsync(cancellationToken).ConfigureAwait(false);
	}

	public override void AttachWithTrustedAuth(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey)
	{
		throw new NotSupportedException("Trusted Auth isn't supported on Firebird Embedded.");
	}
	public override ValueTask AttachWithTrustedAuthAsync(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey, CancellationToken cancellationToken = default)
	{
		throw new NotSupportedException("Trusted Auth isn't supported on Firebird Embedded.");
	}

	public override void Detach()
	{
		if (TransactionCount > 0)
		{
			throw IscException.ForErrorCodeIntParam(IscCodes.isc_open_trans, TransactionCount);
		}

		if (!_handle.IsInvalid)
		{
			StatusVectorHelper.ClearStatusVector(_statusVector);

			_fbClient.isc_detach_database(_statusVector, ref _handle);

			ProcessStatusVector();

			_handle.Dispose();
		}

		WarningMessage = null;
		ServerVersion = null;
		_statusVector = null;
		TransactionCount = 0;
	}
	public override ValueTask DetachAsync(CancellationToken cancellationToken = default)
	{
		if (TransactionCount > 0)
		{
			throw IscException.ForErrorCodeIntParam(IscCodes.isc_open_trans, TransactionCount);
		}

		if (!_handle.IsInvalid)
		{
			StatusVectorHelper.ClearStatusVector(_statusVector);

			_fbClient.isc_detach_database(_statusVector, ref _handle);

			ProcessStatusVector();

			_handle.Dispose();
		}

		WarningMessage = null;
		ServerVersion = null;
		_statusVector = null;
		TransactionCount = 0;

		return ValueTask2.CompletedTask;
	}

	#endregion

	#region Transaction Methods

	public override TransactionBase BeginTransaction(TransactionParameterBuffer tpb)
	{
		var transaction = new FesTransaction(this);
		transaction.BeginTransaction(tpb);
		return transaction;
	}
	public override async ValueTask<TransactionBase> BeginTransactionAsync(TransactionParameterBuffer tpb, CancellationToken cancellationToken = default)
	{
		var transaction = new FesTransaction(this);
		await transaction.BeginTransactionAsync(tpb, cancellationToken).ConfigureAwait(false);
		return transaction;
	}

	#endregion

	#region Cancel Methods

	public override void CancelOperation(short kind)
	{
		var localStatusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];

		_fbClient.fb_cancel_operation(localStatusVector, ref _handle, (ushort)kind);

		try
		{
			ProcessStatusVector(localStatusVector);
		}
		catch (IscException ex) when (ex.ErrorCode == IscCodes.isc_nothing_to_cancel)
		{ }
	}
	public override ValueTask CancelOperationAsync(short kind, CancellationToken cancellationToken = default)
	{
		var localStatusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];

		_fbClient.fb_cancel_operation(localStatusVector, ref _handle, (ushort)kind);

		try
		{
			ProcessStatusVector(localStatusVector);
		}
		catch (IscException ex) when (ex.ErrorCode == IscCodes.isc_nothing_to_cancel)
		{ }

		return ValueTask2.CompletedTask;
	}

	#endregion

	#region Statement Creation Methods

	public override StatementBase CreateStatement()
	{
		return new FesStatement(this);
	}

	public override StatementBase CreateStatement(TransactionBase transaction)
	{
		return new FesStatement(this, transaction as FesTransaction);
	}

	#endregion

	#region Parameter Buffers

	public override DatabaseParameterBufferBase CreateDatabaseParameterBuffer()
	{
		return new DatabaseParameterBuffer1(ParameterBufferEncoding);
	}

	public override EventParameterBuffer CreateEventParameterBuffer()
	{
		return new EventParameterBuffer(Charset.Encoding);
	}

	public override TransactionParameterBuffer CreateTransactionParameterBuffer()
	{
		return new TransactionParameterBuffer(Charset.Encoding);
	}

	#endregion

	#region Database Information Methods

	public override List<object> GetDatabaseInfo(byte[] items)
	{
		return GetDatabaseInfo(items, IscCodes.DEFAULT_MAX_BUFFER_SIZE);
	}
	public override ValueTask<List<object>> GetDatabaseInfoAsync(byte[] items, CancellationToken cancellationToken = default)
	{
		return GetDatabaseInfoAsync(items, IscCodes.DEFAULT_MAX_BUFFER_SIZE, cancellationToken);
	}

	public override List<object> GetDatabaseInfo(byte[] items, int bufferLength)
	{
		var buffer = new byte[bufferLength];

		DatabaseInfo(items, buffer, buffer.Length);

		return IscHelper.ParseDatabaseInfo(buffer, Charset);
	}
	public override ValueTask<List<object>> GetDatabaseInfoAsync(byte[] items, int bufferLength, CancellationToken cancellationToken = default)
	{
		var buffer = new byte[bufferLength];

		DatabaseInfo(items, buffer, buffer.Length);

		return ValueTask2.FromResult(IscHelper.ParseDatabaseInfo(buffer, Charset));
	}

	#endregion

	#region Internal Methods

	internal void ProcessStatusVector(IntPtr[] statusVector)
	{
		StatusVectorHelper.ProcessStatusVector(statusVector, Charset, WarningMessage);
	}

	#endregion

	#region Private Methods

	private void DatabaseInfo(byte[] items, byte[] buffer, int bufferLength)
	{
		StatusVectorHelper.ClearStatusVector(_statusVector);

		_fbClient.isc_database_info(
			_statusVector,
			ref _handle,
			(short)items.Length,
			items,
			(short)bufferLength,
			buffer);

		ProcessStatusVector();
	}

	private void ProcessStatusVector()
	{
		StatusVectorHelper.ProcessStatusVector(_statusVector, Charset, WarningMessage);
	}

	private void ProcessStatusVector(Charset charset)
	{
		StatusVectorHelper.ProcessStatusVector(_statusVector, charset, WarningMessage);
	}

	#endregion

	#region Internal Static Methods

	internal static void CheckCryptKeyForSupport(byte[] cryptKey)
	{
		// ICryptKeyCallbackImpl would have to be passed from C# for 'cryptKey' passing
		if (cryptKey?.Length > 0)
			throw new NotSupportedException("Passing Encryption Key isn't, yet, supported on Firebird Embedded.");
	}

	#endregion
}
