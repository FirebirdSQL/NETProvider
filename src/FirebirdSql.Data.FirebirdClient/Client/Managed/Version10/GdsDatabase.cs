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
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version10;

internal class GdsDatabase : DatabaseBase
{
	protected const int PartnerIdentification = 0;
	protected const int AddressOfAstRoutine = 0;
	protected const int ArgumentToAstRoutine = 0;
	protected internal const int DatabaseObjectId = 0;
	protected internal const int Incarnation = 0;

	#region Fields

	protected GdsConnection _connection;
	protected GdsEventManager _eventManager;
	protected int _handle;

	#endregion

	#region Properties

	public override bool UseUtf8ParameterBuffer => false;

	public override int Handle
	{
		get { return _handle; }
	}

	public override bool HasRemoteEventSupport
	{
		get { return true; }
	}

	public override bool ConnectionBroken
	{
		get { return _connection.ConnectionBroken; }
	}

	public XdrReaderWriter Xdr
	{
		get { return _connection.Xdr; }
	}

	public AuthBlock AuthBlock
	{
		get { return _connection.AuthBlock; }
	}

	#endregion

	#region Constructors

	public GdsDatabase(GdsConnection connection)
		: base(connection.Charset, connection.PacketSize, connection.Dialect)
	{
		_connection = connection;
		_handle = -1;
	}

	#endregion

	#region Attach/Detach Methods

	public override void Attach(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey)
	{
		try
		{
			SendAttachToBuffer(dpb, database);
			Xdr.Flush();
			ProcessAttachResponse((GenericResponse)ReadResponse());
		}
		catch (IscException)
		{
			SafelyDetach();
			throw;
		}
		catch (IOException ex)
		{
			SafelyDetach();
			throw IscException.ForIOException(ex);
		}

		AfterAttachActions();
	}
	public override async ValueTask AttachAsync(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey, CancellationToken cancellationToken = default)
	{
		try
		{
			await SendAttachToBufferAsync(dpb, database, cancellationToken).ConfigureAwait(false);
			await Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
			await ProcessAttachResponseAsync((GenericResponse)await ReadResponseAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
		}
		catch (IscException)
		{
			await SafelyDetachAsync(cancellationToken).ConfigureAwait(false);
			throw;
		}
		catch (IOException ex)
		{
			await SafelyDetachAsync(cancellationToken).ConfigureAwait(false);
			throw IscException.ForIOException(ex);
		}

		await AfterAttachActionsAsync(cancellationToken).ConfigureAwait(false);
	}

	protected virtual void SendAttachToBuffer(DatabaseParameterBufferBase dpb, string database)
	{
		Xdr.Write(IscCodes.op_attach);
		Xdr.Write(DatabaseObjectId);
		if (!string.IsNullOrEmpty(AuthBlock.Password))
		{
			dpb.Append(IscCodes.isc_dpb_password, AuthBlock.Password);
		}
		Xdr.WriteBuffer(dpb.Encoding.GetBytes(database));
		Xdr.WriteBuffer(dpb.ToArray());
	}
	protected virtual async ValueTask SendAttachToBufferAsync(DatabaseParameterBufferBase dpb, string database, CancellationToken cancellationToken = default)
	{
		await Xdr.WriteAsync(IscCodes.op_attach, cancellationToken).ConfigureAwait(false);
		await Xdr.WriteAsync(DatabaseObjectId, cancellationToken).ConfigureAwait(false);
		if (!string.IsNullOrEmpty(AuthBlock.Password))
		{
			dpb.Append(IscCodes.isc_dpb_password, AuthBlock.Password);
		}
		await Xdr.WriteBufferAsync(dpb.Encoding.GetBytes(database), cancellationToken).ConfigureAwait(false);
		await Xdr.WriteBufferAsync(dpb.ToArray(), cancellationToken).ConfigureAwait(false);
	}

	protected virtual void ProcessAttachResponse(GenericResponse response)
	{
		_handle = response.ObjectHandle;
	}
	protected virtual ValueTask ProcessAttachResponseAsync(GenericResponse response, CancellationToken cancellationToken = default)
	{
		_handle = response.ObjectHandle;
		return ValueTask2.CompletedTask;
	}

	protected void AfterAttachActions()
	{
		ServerVersion = GetServerVersion();
	}
	protected async ValueTask AfterAttachActionsAsync(CancellationToken cancellationToken = default)
	{
		ServerVersion = await GetServerVersionAsync(cancellationToken).ConfigureAwait(false);
	}

	public override void AttachWithTrustedAuth(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey)
	{
		throw new NotSupportedException("Trusted Auth isn't supported on < FB2.1.");
	}
	public override ValueTask AttachWithTrustedAuthAsync(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey, CancellationToken cancellationToken = default)
	{
		throw new NotSupportedException("Trusted Auth isn't supported on < FB2.1.");
	}

	public override void Detach()
	{
		if (TransactionCount > 0)
		{
			throw IscException.ForErrorCodeIntParam(IscCodes.isc_open_trans, TransactionCount);
		}

		try
		{
			CloseEventManager();

			var detach = _handle != -1;
			if (detach)
			{
				Xdr.Write(IscCodes.op_detach);
				Xdr.Write(_handle);
			}
			Xdr.Write(IscCodes.op_disconnect);
			Xdr.Flush();
			if (detach)
			{
				ReadResponse();
			}

			CloseConnection();
		}
		catch (IOException ex)
		{
			try
			{
				CloseConnection();
			}
			catch (IOException)
			{ }
			throw IscException.ForIOException(ex);
		}
		finally
		{
			_connection = null;
			_eventManager = null;
			ServerVersion = null;
			_handle = -1;
			WarningMessage = null;
			TransactionCount = 0;
		}
	}
	public override async ValueTask DetachAsync(CancellationToken cancellationToken = default)
	{
		if (TransactionCount > 0)
		{
			throw IscException.ForErrorCodeIntParam(IscCodes.isc_open_trans, TransactionCount);
		}

		try
		{
			await CloseEventManagerAsync(cancellationToken).ConfigureAwait(false);

			var detach = _handle != -1;
			if (detach)
			{
				await Xdr.WriteAsync(IscCodes.op_detach, cancellationToken).ConfigureAwait(false);
				await Xdr.WriteAsync(_handle, cancellationToken).ConfigureAwait(false);
			}
			await Xdr.WriteAsync(IscCodes.op_disconnect, cancellationToken).ConfigureAwait(false);
			await Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
			if (detach)
			{
				await ReadResponseAsync(cancellationToken).ConfigureAwait(false);
			}

			await CloseConnectionAsync(cancellationToken).ConfigureAwait(false);
		}
		catch (IOException ex)
		{
			try
			{
				await CloseConnectionAsync(cancellationToken).ConfigureAwait(false);
			}
			catch (IOException)
			{ }
			throw IscException.ForIOException(ex);
		}
		finally
		{
			_connection = null;
			_eventManager = null;
			ServerVersion = null;
			_handle = -1;
			WarningMessage = null;
			TransactionCount = 0;
		}
	}

	protected internal void SafelyDetach()
	{
		try
		{
			Detach();
		}
		catch
		{ }
	}
	protected internal async ValueTask SafelyDetachAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			await DetachAsync(cancellationToken).ConfigureAwait(false);
		}
		catch
		{ }
	}

	#endregion

	#region Database Methods

	public override void CreateDatabase(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey)
	{
		try
		{
			SendCreateToBuffer(dpb, database);
			Xdr.Flush();
			ProcessCreateResponse((GenericResponse)ReadResponse());
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	public override async ValueTask CreateDatabaseAsync(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey, CancellationToken cancellationToken = default)
	{
		try
		{
			await SendCreateToBufferAsync(dpb, database, cancellationToken).ConfigureAwait(false);
			await Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
			await ProcessCreateResponseAsync((GenericResponse)await ReadResponseAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	protected virtual void SendCreateToBuffer(DatabaseParameterBufferBase dpb, string database)
	{
		Xdr.Write(IscCodes.op_create);
		Xdr.Write(DatabaseObjectId);
		if (!string.IsNullOrEmpty(AuthBlock.Password))
		{
			dpb.Append(IscCodes.isc_dpb_password, AuthBlock.Password);
		}
		Xdr.WriteBuffer(dpb.Encoding.GetBytes(database));
		Xdr.WriteBuffer(dpb.ToArray());
	}
	protected virtual async ValueTask SendCreateToBufferAsync(DatabaseParameterBufferBase dpb, string database, CancellationToken cancellationToken = default)
	{
		await Xdr.WriteAsync(IscCodes.op_create, cancellationToken).ConfigureAwait(false);
		await Xdr.WriteAsync(DatabaseObjectId, cancellationToken).ConfigureAwait(false);
		if (!string.IsNullOrEmpty(AuthBlock.Password))
		{
			dpb.Append(IscCodes.isc_dpb_password, AuthBlock.Password);
		}
		await Xdr.WriteBufferAsync(dpb.Encoding.GetBytes(database), cancellationToken).ConfigureAwait(false);
		await Xdr.WriteBufferAsync(dpb.ToArray(), cancellationToken).ConfigureAwait(false);
	}

	protected void ProcessCreateResponse(GenericResponse response)
	{
		_handle = response.ObjectHandle;
	}
	protected ValueTask ProcessCreateResponseAsync(GenericResponse response, CancellationToken cancellationToken = default)
	{
		_handle = response.ObjectHandle;
		return ValueTask2.CompletedTask;
	}

	public override void CreateDatabaseWithTrustedAuth(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey)
	{
		throw new NotSupportedException("Trusted Auth isn't supported on < FB2.1.");
	}
	public override ValueTask CreateDatabaseWithTrustedAuthAsync(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey, CancellationToken cancellationToken = default)
	{
		throw new NotSupportedException("Trusted Auth isn't supported on < FB2.1.");
	}

	public override void DropDatabase()
	{
		try
		{
			Xdr.Write(IscCodes.op_drop_database);
			Xdr.Write(_handle);
			Xdr.Flush();

			ReadResponse();

			_handle = -1;
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	public override async ValueTask DropDatabaseAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			await Xdr.WriteAsync(IscCodes.op_drop_database, cancellationToken).ConfigureAwait(false);
			await Xdr.WriteAsync(_handle, cancellationToken).ConfigureAwait(false);
			await Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			await ReadResponseAsync(cancellationToken).ConfigureAwait(false);

			_handle = -1;
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	#endregion

	#region Auxiliary Connection Methods

	public virtual (int auxHandle, string ipAddress, int portNumber, int timeout) ConnectionRequest()
	{
		try
		{
			Xdr.Write(IscCodes.op_connect_request);
			Xdr.Write(IscCodes.P_REQ_async);
			Xdr.Write(_handle);
			Xdr.Write(PartnerIdentification);

			Xdr.Flush();

			ReadOperation();

			var auxHandle = Xdr.ReadInt32();

			var garbage1 = new byte[8];
			Xdr.ReadBytes(garbage1, 8);

			var respLen = Xdr.ReadInt32();
			respLen += respLen % 4;

			var sin_family = new byte[2];
			Xdr.ReadBytes(sin_family, 2);
			respLen -= 2;

			var sin_port = new byte[2];
			Xdr.ReadBytes(sin_port, 2);
			var portNumber = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(sin_port, 0));
			respLen -= 2;

			// * The address returned by the server may be incorrect if it is behind a NAT box
			// * so we must use the address that was used to connect the main socket, not the
			// * address reported by the server.
			var sin_addr = new byte[4];
			Xdr.ReadBytes(sin_addr, 4);
			var ipAddress = _connection.IPAddress.ToString();
			respLen -= 4;

			var garbage2 = new byte[respLen];
			Xdr.ReadBytes(garbage2, respLen);

			Xdr.ReadStatusVector();

			return (auxHandle, ipAddress, portNumber, _connection.Timeout);
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	public virtual async ValueTask<(int auxHandle, string ipAddress, int portNumber, int timeout)> ConnectionRequestAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			await Xdr.WriteAsync(IscCodes.op_connect_request, cancellationToken).ConfigureAwait(false);
			await Xdr.WriteAsync(IscCodes.P_REQ_async, cancellationToken).ConfigureAwait(false);
			await Xdr.WriteAsync(_handle, cancellationToken).ConfigureAwait(false);
			await Xdr.WriteAsync(PartnerIdentification, cancellationToken).ConfigureAwait(false);

			await Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			await ReadOperationAsync(cancellationToken).ConfigureAwait(false);

			var auxHandle = await Xdr.ReadInt32Async(cancellationToken).ConfigureAwait(false);

			var garbage1 = new byte[8];
			await Xdr.ReadBytesAsync(garbage1, 8, cancellationToken).ConfigureAwait(false);

			var respLen = await Xdr.ReadInt32Async(cancellationToken).ConfigureAwait(false);
			respLen += respLen % 4;

			var sin_family = new byte[2];
			await Xdr.ReadBytesAsync(sin_family, 2, cancellationToken).ConfigureAwait(false);
			respLen -= 2;

			var sin_port = new byte[2];
			await Xdr.ReadBytesAsync(sin_port, 2, cancellationToken).ConfigureAwait(false);
			var portNumber = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(sin_port, 0));
			respLen -= 2;

			// * The address returned by the server may be incorrect if it is behind a NAT box
			// * so we must use the address that was used to connect the main socket, not the
			// * address reported by the server.
			var sin_addr = new byte[4];
			await Xdr.ReadBytesAsync(sin_addr, 4, cancellationToken).ConfigureAwait(false);
			var ipAddress = _connection.IPAddress.ToString();
			respLen -= 4;

			var garbage2 = new byte[respLen];
			await Xdr.ReadBytesAsync(garbage2, respLen, cancellationToken).ConfigureAwait(false);

			await Xdr.ReadStatusVectorAsync(cancellationToken).ConfigureAwait(false);

			return (auxHandle, ipAddress, portNumber, _connection.Timeout);
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	#endregion

	#region Connection Methods

	public void CloseConnection()
	{
		_connection.Disconnect();
	}
	public ValueTask CloseConnectionAsync(CancellationToken cancellationToken = default)
	{
		return _connection.DisconnectAsync(cancellationToken);
	}

	#endregion

	#region Remote Events Methods

	public override void CloseEventManager()
	{
		if (_eventManager != null)
		{
			_eventManager.Close();
			_eventManager = null;
		}
	}
	public override async ValueTask CloseEventManagerAsync(CancellationToken cancellationToken = default)
	{
		if (_eventManager != null)
		{
			await _eventManager.CloseAsync(cancellationToken).ConfigureAwait(false);
			_eventManager = null;
		}
	}

	public override void QueueEvents(RemoteEvent remoteEvent)
	{
		try
		{
			if (_eventManager == null)
			{
				var (auxHandle, ipAddress, portNumber, timeout) = ConnectionRequest();
				_eventManager = new GdsEventManager(auxHandle, ipAddress, portNumber, timeout);
				_eventManager.Open();
				var dummy = _eventManager.StartWaitingForEvents(remoteEvent);
			}

			remoteEvent.LocalId++;

			var epb = remoteEvent.BuildEpb();
			var epbData = epb.ToArray();

			Xdr.Write(IscCodes.op_que_events);
			Xdr.Write(_handle);
			Xdr.WriteBuffer(epbData);
			Xdr.Write(AddressOfAstRoutine);
			Xdr.Write(ArgumentToAstRoutine);
			Xdr.Write(remoteEvent.LocalId);

			Xdr.Flush();

			var response = (GenericResponse)ReadResponse();

			remoteEvent.RemoteId = response.ObjectHandle;
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	public override async ValueTask QueueEventsAsync(RemoteEvent remoteEvent, CancellationToken cancellationToken = default)
	{
		try
		{
			if (_eventManager == null)
			{
				var (auxHandle, ipAddress, portNumber, timeout) = await ConnectionRequestAsync(cancellationToken).ConfigureAwait(false);
				_eventManager = new GdsEventManager(auxHandle, ipAddress, portNumber, timeout);
				await _eventManager.OpenAsync(cancellationToken).ConfigureAwait(false);
				var dummy = _eventManager.StartWaitingForEvents(remoteEvent);
			}

			remoteEvent.LocalId++;

			var epb = remoteEvent.BuildEpb();
			var epbData = epb.ToArray();

			await Xdr.WriteAsync(IscCodes.op_que_events, cancellationToken).ConfigureAwait(false);
			await Xdr.WriteAsync(_handle, cancellationToken).ConfigureAwait(false);
			await Xdr.WriteBufferAsync(epbData, cancellationToken).ConfigureAwait(false);
			await Xdr.WriteAsync(AddressOfAstRoutine, cancellationToken).ConfigureAwait(false);
			await Xdr.WriteAsync(ArgumentToAstRoutine, cancellationToken).ConfigureAwait(false);
			await Xdr.WriteAsync(remoteEvent.LocalId, cancellationToken).ConfigureAwait(false);

			await Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			var response = (GenericResponse)await ReadResponseAsync(cancellationToken).ConfigureAwait(false);

			remoteEvent.RemoteId = response.ObjectHandle;
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	public override void CancelEvents(RemoteEvent events)
	{
		try
		{
			Xdr.Write(IscCodes.op_cancel_events);
			Xdr.Write(_handle);
			Xdr.Write(events.LocalId);

			Xdr.Flush();

			ReadResponse();
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	public override async ValueTask CancelEventsAsync(RemoteEvent events, CancellationToken cancellationToken = default)
	{
		try
		{
			await Xdr.WriteAsync(IscCodes.op_cancel_events, cancellationToken).ConfigureAwait(false);
			await Xdr.WriteAsync(_handle, cancellationToken).ConfigureAwait(false);
			await Xdr.WriteAsync(events.LocalId, cancellationToken).ConfigureAwait(false);

			await Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			await ReadResponseAsync(cancellationToken).ConfigureAwait(false);
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	#endregion

	#region Transaction Methods

	public override TransactionBase BeginTransaction(TransactionParameterBuffer tpb)
	{
		var transaction = new GdsTransaction(this);

		transaction.BeginTransaction(tpb);

		return transaction;
	}
	public override async ValueTask<TransactionBase> BeginTransactionAsync(TransactionParameterBuffer tpb, CancellationToken cancellationToken = default)
	{
		var transaction = new GdsTransaction(this);

		await transaction.BeginTransactionAsync(tpb, cancellationToken).ConfigureAwait(false);

		return transaction;
	}

	#endregion

	#region Cancel Methods

	public override void CancelOperation(short kind)
	{
		throw new NotSupportedException("Cancel Operation isn't supported on < FB2.5.");
	}
	public override ValueTask CancelOperationAsync(short kind, CancellationToken cancellationToken = default)
	{
		throw new NotSupportedException("Cancel Operation isn't supported on < FB2.5.");
	}

	#endregion

	#region Statement Creation Methods

	public override StatementBase CreateStatement()
	{
		return new GdsStatement(this);
	}

	public override StatementBase CreateStatement(TransactionBase transaction)
	{
		return new GdsStatement(this, (GdsTransaction)transaction);
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
	public override async ValueTask<List<object>> GetDatabaseInfoAsync(byte[] items, int bufferLength, CancellationToken cancellationToken = default)
	{
		var buffer = new byte[bufferLength];
		await DatabaseInfoAsync(items, buffer, buffer.Length, cancellationToken).ConfigureAwait(false);
		return IscHelper.ParseDatabaseInfo(buffer, Charset);
	}

	#endregion

	#region Release Object

	public virtual void ReleaseObject(int op, int id)
	{
		try
		{
			SendReleaseObjectToBuffer(op, id);
			Xdr.Flush();
			ProcessReleaseObjectResponse(ReadResponse());
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	public virtual async ValueTask ReleaseObjectAsync(int op, int id, CancellationToken cancellationToken = default)
	{
		try
		{
			await SendReleaseObjectToBufferAsync(op, id, cancellationToken).ConfigureAwait(false);
			await Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
			await ProcessReleaseObjectResponseAsync(await ReadResponseAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	protected virtual void SendReleaseObjectToBuffer(int op, int id)
	{
		Xdr.Write(op);
		Xdr.Write(id);
	}
	protected virtual async ValueTask SendReleaseObjectToBufferAsync(int op, int id, CancellationToken cancellationToken = default)
	{
		await Xdr.WriteAsync(op, cancellationToken).ConfigureAwait(false);
		await Xdr.WriteAsync(id, cancellationToken).ConfigureAwait(false);
	}

	protected virtual void ProcessReleaseObjectResponse(IResponse response)
	{ }
	protected virtual ValueTask ProcessReleaseObjectResponseAsync(IResponse response, CancellationToken cancellationToken = default)
	{
		return ValueTask2.CompletedTask;
	}

	#endregion

	#region Response Methods

	public virtual int ReadOperation()
	{
		return Xdr.ReadOperation();
	}
	public virtual ValueTask<int> ReadOperationAsync(CancellationToken cancellationToken = default)
	{
		return Xdr.ReadOperationAsync(cancellationToken);
	}

	public virtual IResponse ReadResponse()
	{
		var response = ReadSingleResponse();
		response.HandleResponseException();
		return response;
	}
	public virtual async ValueTask<IResponse> ReadResponseAsync(CancellationToken cancellationToken = default)
	{
		var response = await ReadSingleResponseAsync(cancellationToken).ConfigureAwait(false);
		response.HandleResponseException();
		return response;
	}

	public virtual IResponse ReadResponse(int operation)
	{
		var response = ReadSingleResponse(operation);
		response.HandleResponseException();
		return response;
	}
	public virtual async ValueTask<IResponse> ReadResponseAsync(int operation, CancellationToken cancellationToken = default)
	{
		var response = await ReadSingleResponseAsync(operation, cancellationToken).ConfigureAwait(false);
		response.HandleResponseException();
		return response;
	}

	public void SafeFinishFetching(int numberOfResponses)
	{
		while (numberOfResponses > 0)
		{
			numberOfResponses--;
			try
			{
				ReadResponse();
			}
			catch (IscException)
			{ }
		}
	}
	public async ValueTask SafeFinishFetchingAsync(int numberOfResponses, CancellationToken cancellationToken = default)
	{
		while (numberOfResponses > 0)
		{
			numberOfResponses--;
			try
			{
				await ReadResponseAsync(cancellationToken).ConfigureAwait(false);
			}
			catch (IscException)
			{ }
		}
	}

	#endregion

	#region Protected Methods

	protected IResponse ReadSingleResponse()
	{
		return ReadSingleResponse(ReadOperation());
	}
	protected async ValueTask<IResponse> ReadSingleResponseAsync(CancellationToken cancellationToken = default)
	{
		return await ReadSingleResponseAsync(await ReadOperationAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
	}

	protected virtual IResponse ReadSingleResponse(int operation)
	{
		var response = _connection.ProcessOperation(operation);
		response.HandleResponseWarning(WarningMessage);
		return response;
	}
	protected virtual async ValueTask<IResponse> ReadSingleResponseAsync(int operation, CancellationToken cancellationToken = default)
	{
		var response = await _connection.ProcessOperationAsync(operation, cancellationToken).ConfigureAwait(false);
		response.HandleResponseWarning(WarningMessage);
		return response;
	}

	#endregion

	#region Private Methods

	private void DatabaseInfo(byte[] items, byte[] buffer, int bufferLength)
	{
		try
		{
			Xdr.Write(IscCodes.op_info_database);
			Xdr.Write(_handle);
			Xdr.Write(Incarnation);
			Xdr.WriteBuffer(items, items.Length);
			Xdr.Write(bufferLength);

			Xdr.Flush();

			var response = (GenericResponse)ReadResponse();

			var responseLength = bufferLength;

			if (response.Data.Length < bufferLength)
			{
				responseLength = response.Data.Length;
			}

			Buffer.BlockCopy(response.Data, 0, buffer, 0, responseLength);
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	private async ValueTask DatabaseInfoAsync(byte[] items, byte[] buffer, int bufferLength, CancellationToken cancellationToken = default)
	{
		try
		{
			await Xdr.WriteAsync(IscCodes.op_info_database, cancellationToken).ConfigureAwait(false);
			await Xdr.WriteAsync(_handle, cancellationToken).ConfigureAwait(false);
			await Xdr.WriteAsync(Incarnation, cancellationToken).ConfigureAwait(false);
			await Xdr.WriteBufferAsync(items, items.Length, cancellationToken).ConfigureAwait(false);
			await Xdr.WriteAsync(bufferLength, cancellationToken).ConfigureAwait(false);

			await Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			var response = (GenericResponse)await ReadResponseAsync(cancellationToken).ConfigureAwait(false);

			var responseLength = bufferLength;

			if (response.Data.Length < bufferLength)
			{
				responseLength = response.Data.Length;
			}

			Buffer.BlockCopy(response.Data, 0, buffer, 0, responseLength);
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	#endregion
}
