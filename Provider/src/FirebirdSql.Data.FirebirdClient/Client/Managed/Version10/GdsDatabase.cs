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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version10
{
	internal class GdsDatabase : DatabaseBase
	{
		const int DatabaseObjectId = 0;
		const int PartnerIdentification = 0;
		const int AddressOfAstRoutine = 0;
		const int ArgumentToAstRoutine = 0;
		internal const int Incarnation = 0;

		#region Fields

		private GdsConnection _connection;
		private GdsEventManager _eventManager;
		protected int _handle;

		#endregion

		#region Properties

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

		public string Password
		{
			get { return _connection.Password; }
		}

		public byte[] AuthData
		{
			get { return _connection.AuthData; }
		}

		#endregion

		#region Constructors

		public GdsDatabase(GdsConnection connection)
		{
			_connection = connection;
			_handle = -1;
			Charset = Charset.DefaultCharset;
			Dialect = 3;
			PacketSize = 8192;
		}

		#endregion

		#region Attach/Detach Methods

		public override async Task Attach(DatabaseParameterBufferBase dpb, string dataSource, int port, string database, byte[] cryptKey, AsyncWrappingCommonArgs async)
		{
			try
			{
				await SendAttachToBuffer(dpb, database, async).ConfigureAwait(false);
				await Xdr.Flush(async).ConfigureAwait(false);
				await ProcessAttachResponse((GenericResponse)await ReadResponse(async).ConfigureAwait(false), async).ConfigureAwait(false);
			}
			catch (IscException)
			{
				await SafelyDetach(async).ConfigureAwait(false);
				throw;
			}
			catch (IOException ex)
			{
				await SafelyDetach(async).ConfigureAwait(false);
				throw IscException.ForIOException(ex);
			}

			await AfterAttachActions(async).ConfigureAwait(false);
		}

		protected virtual async Task SendAttachToBuffer(DatabaseParameterBufferBase dpb, string database, AsyncWrappingCommonArgs async)
		{
			await Xdr.Write(IscCodes.op_attach, async).ConfigureAwait(false);
			await Xdr.Write(0, async).ConfigureAwait(false);
			if (!string.IsNullOrEmpty(Password))
			{
				dpb.Append(IscCodes.isc_dpb_password, Password);
			}
			await Xdr.WriteBuffer(Encoding2.Default.GetBytes(database), async).ConfigureAwait(false);
			await Xdr.WriteBuffer(dpb.ToArray(), async).ConfigureAwait(false);
		}

		protected virtual Task ProcessAttachResponse(GenericResponse response, AsyncWrappingCommonArgs async)
		{
			_handle = response.ObjectHandle;
			return Task.CompletedTask;
		}

		protected async Task AfterAttachActions(AsyncWrappingCommonArgs async)
		{
			ServerVersion = await GetServerVersion(async).ConfigureAwait(false);
		}

		public override Task AttachWithTrustedAuth(DatabaseParameterBufferBase dpb, string dataSource, int port, string database, byte[] cryptKey, AsyncWrappingCommonArgs async)
		{
			throw new NotSupportedException("Trusted Auth isn't supported on < FB2.1.");
		}

		public override async Task Detach(AsyncWrappingCommonArgs async)
		{
			if (TransactionCount > 0)
			{
				throw IscException.ForErrorCodeIntParam(IscCodes.isc_open_trans, TransactionCount);
			}

			try
			{
				await CloseEventManager(async).ConfigureAwait(false);

				var detach = _handle != -1;
				if (detach)
				{
					await Xdr.Write(IscCodes.op_detach, async).ConfigureAwait(false);
					await Xdr.Write(_handle, async).ConfigureAwait(false);
				}
				await Xdr.Write(IscCodes.op_disconnect, async).ConfigureAwait(false);
				await Xdr.Flush(async).ConfigureAwait(false);
				if (detach)
				{
					await ReadResponse(async).ConfigureAwait(false);
				}

				await CloseConnection(async).ConfigureAwait(false);
			}
			catch (IOException ex)
			{
				try
				{
					await CloseConnection(async).ConfigureAwait(false);
				}
				catch (IOException)
				{ }
				throw IscException.ForIOException(ex);
			}
			finally
			{
				_connection = null;
				Charset = null;
				_eventManager = null;
				ServerVersion = null;
				Dialect = 0;
				_handle = -1;
				PacketSize = 0;
				WarningMessage = null;
				TransactionCount = 0;
			}
		}

		protected async Task SafelyDetach(AsyncWrappingCommonArgs async)
		{
			try
			{
				await Detach(async).ConfigureAwait(false);
			}
			catch
			{ }
		}

		#endregion

		#region Database Methods

		public override async Task CreateDatabase(DatabaseParameterBufferBase dpb, string dataSource, int port, string database, byte[] cryptKey, AsyncWrappingCommonArgs async)
		{
			try
			{
				await SendCreateToBuffer(dpb, database, async).ConfigureAwait(false);
				await Xdr.Flush(async).ConfigureAwait(false);
				await ProcessCreateResponse((GenericResponse)await ReadResponse(async).ConfigureAwait(false), async).ConfigureAwait(false);
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}

		protected virtual async Task SendCreateToBuffer(DatabaseParameterBufferBase dpb, string database, AsyncWrappingCommonArgs async)
		{
			await Xdr.Write(IscCodes.op_create, async).ConfigureAwait(false);
			await Xdr.Write(DatabaseObjectId, async).ConfigureAwait(false);
			if (!string.IsNullOrEmpty(Password))
			{
				dpb.Append(IscCodes.isc_dpb_password, Password);
			}
			await Xdr.WriteBuffer(Encoding2.Default.GetBytes(database), async).ConfigureAwait(false);
			await Xdr.WriteBuffer(dpb.ToArray(), async).ConfigureAwait(false);
		}

		protected Task ProcessCreateResponse(GenericResponse response, AsyncWrappingCommonArgs async)
		{
			_handle = response.ObjectHandle;
			return Task.CompletedTask;
		}

		public override Task CreateDatabaseWithTrustedAuth(DatabaseParameterBufferBase dpb, string dataSource, int port, string database, byte[] cryptKey, AsyncWrappingCommonArgs async)
		{
			throw new NotSupportedException("Trusted Auth isn't supported on < FB2.1.");
		}

		public override async Task DropDatabase(AsyncWrappingCommonArgs async)
		{
			try
			{
				await Xdr.Write(IscCodes.op_drop_database, async).ConfigureAwait(false);
				await Xdr.Write(_handle, async).ConfigureAwait(false);
				await Xdr.Flush(async).ConfigureAwait(false);

				await ReadResponse(async).ConfigureAwait(false);

				_handle = -1;
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}

		#endregion

		#region Auxiliary Connection Methods

		public virtual async Task<(int auxHandle, string ipAddress, int portNumber, int timeout)> ConnectionRequest(AsyncWrappingCommonArgs async)
		{
			try
			{
				await Xdr.Write(IscCodes.op_connect_request, async).ConfigureAwait(false);
				await Xdr.Write(IscCodes.P_REQ_async, async).ConfigureAwait(false);
				await Xdr.Write(_handle, async).ConfigureAwait(false);
				await Xdr.Write(PartnerIdentification, async).ConfigureAwait(false);

				await Xdr.Flush(async).ConfigureAwait(false);

				await ReadOperation(async).ConfigureAwait(false);

				var auxHandle = await Xdr.ReadInt32(async).ConfigureAwait(false);

				var garbage1 = new byte[8];
				await Xdr.ReadBytes(garbage1, 8, async).ConfigureAwait(false);

				var respLen = await Xdr.ReadInt32(async).ConfigureAwait(false);
				respLen += respLen % 4;

				var sin_family = new byte[2];
				await Xdr.ReadBytes(sin_family, 2, async).ConfigureAwait(false);
				respLen -= 2;

				var sin_port = new byte[2];
				await Xdr.ReadBytes(sin_port, 2, async).ConfigureAwait(false);
				var portNumber = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(sin_port, 0));
				respLen -= 2;

				// * The address returned by the server may be incorrect if it is behind a NAT box
				// * so we must use the address that was used to connect the main socket, not the
				// * address reported by the server.
				var sin_addr = new byte[4];
				await Xdr.ReadBytes(sin_addr, 4, async).ConfigureAwait(false);
				var ipAddress = _connection.IPAddress.ToString();
				respLen -= 4;

				var garbage2 = new byte[respLen];
				await Xdr.ReadBytes(garbage2, respLen, async).ConfigureAwait(false);

				await Xdr.ReadStatusVector(async).ConfigureAwait(false);

				return (auxHandle, ipAddress, portNumber, _connection.Timeout);
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}

		#endregion

		#region Connection Methods

		public Task CloseConnection(AsyncWrappingCommonArgs async)
		{
			return _connection.Disconnect(async);
		}

		#endregion

		#region Remote Events Methods

		public override async Task CloseEventManager(AsyncWrappingCommonArgs async)
		{
			if (_eventManager != null)
			{
				await _eventManager.Close(async).ConfigureAwait(false);
				_eventManager = null;
			}
		}

		public override async Task QueueEvents(RemoteEvent remoteEvent, AsyncWrappingCommonArgs async)
		{
			try
			{
				if (_eventManager == null)
				{
					var (auxHandle, ipAddress, portNumber, timeout) = await ConnectionRequest(async).ConfigureAwait(false);
					_eventManager = new GdsEventManager(auxHandle, ipAddress, portNumber, timeout);
					await _eventManager.Open(async).ConfigureAwait(false);
					var dummy = _eventManager.WaitForEvents(remoteEvent, new AsyncWrappingCommonArgs(true));
				}

				remoteEvent.LocalId++;

				var epb = remoteEvent.BuildEpb();
				var epbData = epb.ToArray();

				await Xdr.Write(IscCodes.op_que_events, async).ConfigureAwait(false);
				await Xdr.Write(_handle, async).ConfigureAwait(false);
				await Xdr.WriteBuffer(epbData, async).ConfigureAwait(false);
				await Xdr.Write(AddressOfAstRoutine, async).ConfigureAwait(false);
				await Xdr.Write(ArgumentToAstRoutine, async).ConfigureAwait(false);
				await Xdr.Write(remoteEvent.LocalId, async).ConfigureAwait(false);

				await Xdr.Flush(async).ConfigureAwait(false);

				var response = (GenericResponse)await ReadResponse(async).ConfigureAwait(false);

				remoteEvent.RemoteId = response.ObjectHandle;
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}

		public override async Task CancelEvents(RemoteEvent events, AsyncWrappingCommonArgs async)
		{
			try
			{
				await Xdr.Write(IscCodes.op_cancel_events, async).ConfigureAwait(false);
				await Xdr.Write(_handle, async).ConfigureAwait(false);
				await Xdr.Write(events.LocalId, async).ConfigureAwait(false);

				await Xdr.Flush(async).ConfigureAwait(false);

				await ReadResponse(async).ConfigureAwait(false);
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}

		#endregion

		#region Transaction Methods

		public override async Task<TransactionBase> BeginTransaction(TransactionParameterBuffer tpb, AsyncWrappingCommonArgs async)
		{
			var transaction = new GdsTransaction(this);

			await transaction.BeginTransaction(tpb, async).ConfigureAwait(false);

			return transaction;
		}

		#endregion

		#region Cancel Methods

		public override Task CancelOperation(int kind, AsyncWrappingCommonArgs async)
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
			return new GdsStatement(this, transaction);
		}

		#endregion

		#region DPB

		public override DatabaseParameterBufferBase CreateDatabaseParameterBuffer()
		{
			return new DatabaseParameterBuffer1();
		}

		#endregion

		#region Database Information Methods

		public override Task<List<object>> GetDatabaseInfo(byte[] items, AsyncWrappingCommonArgs async)
		{
			return GetDatabaseInfo(items, IscCodes.DEFAULT_MAX_BUFFER_SIZE, async);
		}

		public override async Task<List<object>> GetDatabaseInfo(byte[] items, int bufferLength, AsyncWrappingCommonArgs async)
		{
			var buffer = new byte[bufferLength];
			await DatabaseInfo(items, buffer, buffer.Length, async).ConfigureAwait(false);
			return IscHelper.ParseDatabaseInfo(buffer);
		}

		#endregion

		#region Release Object

		public virtual async Task ReleaseObject(int op, int id, AsyncWrappingCommonArgs async)
		{
			try
			{
				await SendReleaseObjectToBuffer(op, id, async).ConfigureAwait(false);
				await Xdr.Flush(async).ConfigureAwait(false);
				await ProcessReleaseObjectResponse(await ReadResponse(async).ConfigureAwait(false), async).ConfigureAwait(false);
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}

		protected virtual async Task SendReleaseObjectToBuffer(int op, int id, AsyncWrappingCommonArgs async)
		{
			await Xdr.Write(op, async).ConfigureAwait(false);
			await Xdr.Write(id, async).ConfigureAwait(false);
		}

		protected virtual Task ProcessReleaseObjectResponse(IResponse response, AsyncWrappingCommonArgs async)
		{
			return Task.CompletedTask;
		}

		#endregion

		#region Response Methods

		public virtual Task<int> ReadOperation(AsyncWrappingCommonArgs async)
		{
			return Xdr.ReadOperation(async);
		}

		public virtual async Task<IResponse> ReadResponse(AsyncWrappingCommonArgs async)
		{
			var response = await ReadSingleResponse(async).ConfigureAwait(false);
			GdsConnection.ProcessResponse(response);
			return response;
		}

		public virtual async Task<IResponse> ReadResponse(int operation, AsyncWrappingCommonArgs async)
		{
			var response = await ReadSingleResponse(operation, async).ConfigureAwait(false);
			GdsConnection.ProcessResponse(response);
			return response;
		}

		#endregion

		#region Protected Methods

		protected async Task<IResponse> ReadSingleResponse(AsyncWrappingCommonArgs async) => await ReadSingleResponse(await ReadOperation(async).ConfigureAwait(false), async).ConfigureAwait(false);
		protected virtual async Task<IResponse> ReadSingleResponse(int operation, AsyncWrappingCommonArgs async)
		{
			var response = await GdsConnection.ProcessOperation(operation, Xdr, async).ConfigureAwait(false);
			GdsConnection.ProcessResponseWarnings(response, WarningMessage);
			return response;
		}

		private async Task DatabaseInfo(byte[] items, byte[] buffer, int bufferLength, AsyncWrappingCommonArgs async)
		{
			try
			{
				await Xdr.Write(IscCodes.op_info_database, async).ConfigureAwait(false);
				await Xdr.Write(_handle, async).ConfigureAwait(false);
				await Xdr.Write(Incarnation, async).ConfigureAwait(false);
				await Xdr.WriteBuffer(items, items.Length, async).ConfigureAwait(false);
				await Xdr.Write(bufferLength, async).ConfigureAwait(false);

				await Xdr.Flush(async).ConfigureAwait(false);

				var response = (GenericResponse)await ReadResponse(async).ConfigureAwait(false);

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
}
