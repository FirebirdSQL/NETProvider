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

		protected GdsConnection _connection;
		protected GdsEventManager _eventManager;
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

		public AuthBlock AuthBlock
		{
			get { return _connection.AuthBlock; }
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

		public override async ValueTask AttachAsync(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey, AsyncWrappingCommonArgs async)
		{
			try
			{
				await SendAttachToBufferAsync(dpb, database, async).ConfigureAwait(false);
				await Xdr.FlushAsync(async).ConfigureAwait(false);
				await ProcessAttachResponseAsync((GenericResponse)await ReadResponseAsync(async).ConfigureAwait(false), async).ConfigureAwait(false);
			}
			catch (IscException)
			{
				await SafelyDetachAsync(async).ConfigureAwait(false);
				throw;
			}
			catch (IOException ex)
			{
				await SafelyDetachAsync(async).ConfigureAwait(false);
				throw IscException.ForIOException(ex);
			}

			await AfterAttachActionsAsync(async).ConfigureAwait(false);
		}

		protected virtual async ValueTask SendAttachToBufferAsync(DatabaseParameterBufferBase dpb, string database, AsyncWrappingCommonArgs async)
		{
			await Xdr.WriteAsync(IscCodes.op_attach, async).ConfigureAwait(false);
			await Xdr.WriteAsync(0, async).ConfigureAwait(false);
			if (!string.IsNullOrEmpty(AuthBlock.Password))
			{
				dpb.Append(IscCodes.isc_dpb_password, AuthBlock.Password);
			}
			await Xdr.WriteBufferAsync(Encoding2.Default.GetBytes(database), async).ConfigureAwait(false);
			await Xdr.WriteBufferAsync(dpb.ToArray(), async).ConfigureAwait(false);
		}

		protected virtual ValueTask ProcessAttachResponseAsync(GenericResponse response, AsyncWrappingCommonArgs async)
		{
			_handle = response.ObjectHandle;
			return ValueTask2.CompletedTask;
		}

		protected async ValueTask AfterAttachActionsAsync(AsyncWrappingCommonArgs async)
		{
			ServerVersion = await GetServerVersionAsync(async).ConfigureAwait(false);
		}

		public override ValueTask AttachWithTrustedAuthAsync(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey, AsyncWrappingCommonArgs async)
		{
			throw new NotSupportedException("Trusted Auth isn't supported on < FB2.1.");
		}

		public override async ValueTask DetachAsync(AsyncWrappingCommonArgs async)
		{
			if (TransactionCount > 0)
			{
				throw IscException.ForErrorCodeIntParam(IscCodes.isc_open_trans, TransactionCount);
			}

			try
			{
				await CloseEventManagerAsync(async).ConfigureAwait(false);

				var detach = _handle != -1;
				if (detach)
				{
					await Xdr.WriteAsync(IscCodes.op_detach, async).ConfigureAwait(false);
					await Xdr.WriteAsync(_handle, async).ConfigureAwait(false);
				}
				await Xdr.WriteAsync(IscCodes.op_disconnect, async).ConfigureAwait(false);
				await Xdr.FlushAsync(async).ConfigureAwait(false);
				if (detach)
				{
					await ReadResponseAsync(async).ConfigureAwait(false);
				}

				await CloseConnectionAsync(async).ConfigureAwait(false);
			}
			catch (IOException ex)
			{
				try
				{
					await CloseConnectionAsync(async).ConfigureAwait(false);
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

		protected internal async ValueTask SafelyDetachAsync(AsyncWrappingCommonArgs async)
		{
			try
			{
				await DetachAsync(async).ConfigureAwait(false);
			}
			catch
			{ }
		}

		#endregion

		#region Database Methods

		public override async ValueTask CreateDatabaseAsync(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey, AsyncWrappingCommonArgs async)
		{
			try
			{
				await SendCreateToBufferAsync(dpb, database, async).ConfigureAwait(false);
				await Xdr.FlushAsync(async).ConfigureAwait(false);
				await ProcessCreateResponseAsync((GenericResponse)await ReadResponseAsync(async).ConfigureAwait(false), async).ConfigureAwait(false);
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}

		protected virtual async ValueTask SendCreateToBufferAsync(DatabaseParameterBufferBase dpb, string database, AsyncWrappingCommonArgs async)
		{
			await Xdr.WriteAsync(IscCodes.op_create, async).ConfigureAwait(false);
			await Xdr.WriteAsync(DatabaseObjectId, async).ConfigureAwait(false);
			if (!string.IsNullOrEmpty(AuthBlock.Password))
			{
				dpb.Append(IscCodes.isc_dpb_password, AuthBlock.Password);
			}
			await Xdr.WriteBufferAsync(Encoding2.Default.GetBytes(database), async).ConfigureAwait(false);
			await Xdr.WriteBufferAsync(dpb.ToArray(), async).ConfigureAwait(false);
		}

		protected ValueTask ProcessCreateResponseAsync(GenericResponse response, AsyncWrappingCommonArgs async)
		{
			_handle = response.ObjectHandle;
			return ValueTask2.CompletedTask;
		}

		public override ValueTask CreateDatabaseWithTrustedAuthAsync(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey, AsyncWrappingCommonArgs async)
		{
			throw new NotSupportedException("Trusted Auth isn't supported on < FB2.1.");
		}

		public override async ValueTask DropDatabaseAsync(AsyncWrappingCommonArgs async)
		{
			try
			{
				await Xdr.WriteAsync(IscCodes.op_drop_database, async).ConfigureAwait(false);
				await Xdr.WriteAsync(_handle, async).ConfigureAwait(false);
				await Xdr.FlushAsync(async).ConfigureAwait(false);

				await ReadResponseAsync(async).ConfigureAwait(false);

				_handle = -1;
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}

		#endregion

		#region Auxiliary Connection Methods

		public virtual async ValueTask<(int auxHandle, string ipAddress, int portNumber, int timeout)> ConnectionRequestAsync(AsyncWrappingCommonArgs async)
		{
			try
			{
				await Xdr.WriteAsync(IscCodes.op_connect_request, async).ConfigureAwait(false);
				await Xdr.WriteAsync(IscCodes.P_REQ_async, async).ConfigureAwait(false);
				await Xdr.WriteAsync(_handle, async).ConfigureAwait(false);
				await Xdr.WriteAsync(PartnerIdentification, async).ConfigureAwait(false);

				await Xdr.FlushAsync(async).ConfigureAwait(false);

				await ReadOperationAsync(async).ConfigureAwait(false);

				var auxHandle = await Xdr.ReadInt32Async(async).ConfigureAwait(false);

				var garbage1 = new byte[8];
				await Xdr.ReadBytesAsync(garbage1, 8, async).ConfigureAwait(false);

				var respLen = await Xdr.ReadInt32Async(async).ConfigureAwait(false);
				respLen += respLen % 4;

				var sin_family = new byte[2];
				await Xdr.ReadBytesAsync(sin_family, 2, async).ConfigureAwait(false);
				respLen -= 2;

				var sin_port = new byte[2];
				await Xdr.ReadBytesAsync(sin_port, 2, async).ConfigureAwait(false);
				var portNumber = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(sin_port, 0));
				respLen -= 2;

				// * The address returned by the server may be incorrect if it is behind a NAT box
				// * so we must use the address that was used to connect the main socket, not the
				// * address reported by the server.
				var sin_addr = new byte[4];
				await Xdr.ReadBytesAsync(sin_addr, 4, async).ConfigureAwait(false);
				var ipAddress = _connection.IPAddress.ToString();
				respLen -= 4;

				var garbage2 = new byte[respLen];
				await Xdr.ReadBytesAsync(garbage2, respLen, async).ConfigureAwait(false);

				await Xdr.ReadStatusVectorAsync(async).ConfigureAwait(false);

				return (auxHandle, ipAddress, portNumber, _connection.Timeout);
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}

		#endregion

		#region Connection Methods

		public ValueTask CloseConnectionAsync(AsyncWrappingCommonArgs async)
		{
			return _connection.DisconnectAsync(async);
		}

		#endregion

		#region Remote Events Methods

		public override async ValueTask CloseEventManagerAsync(AsyncWrappingCommonArgs async)
		{
			if (_eventManager != null)
			{
				await _eventManager.CloseAsync(async).ConfigureAwait(false);
				_eventManager = null;
			}
		}

		public override async ValueTask QueueEventsAsync(RemoteEvent remoteEvent, AsyncWrappingCommonArgs async)
		{
			try
			{
				if (_eventManager == null)
				{
					var (auxHandle, ipAddress, portNumber, timeout) = await ConnectionRequestAsync(async).ConfigureAwait(false);
					_eventManager = new GdsEventManager(auxHandle, ipAddress, portNumber, timeout);
					await _eventManager.OpenAsync(async).ConfigureAwait(false);
					var dummy = _eventManager.WaitForEventsAsync(remoteEvent, new AsyncWrappingCommonArgs(true));
				}

				remoteEvent.LocalId++;

				var epb = remoteEvent.BuildEpb();
				var epbData = epb.ToArray();

				await Xdr.WriteAsync(IscCodes.op_que_events, async).ConfigureAwait(false);
				await Xdr.WriteAsync(_handle, async).ConfigureAwait(false);
				await Xdr.WriteBufferAsync(epbData, async).ConfigureAwait(false);
				await Xdr.WriteAsync(AddressOfAstRoutine, async).ConfigureAwait(false);
				await Xdr.WriteAsync(ArgumentToAstRoutine, async).ConfigureAwait(false);
				await Xdr.WriteAsync(remoteEvent.LocalId, async).ConfigureAwait(false);

				await Xdr.FlushAsync(async).ConfigureAwait(false);

				var response = (GenericResponse)await ReadResponseAsync(async).ConfigureAwait(false);

				remoteEvent.RemoteId = response.ObjectHandle;
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}

		public override async ValueTask CancelEventsAsync(RemoteEvent events, AsyncWrappingCommonArgs async)
		{
			try
			{
				await Xdr.WriteAsync(IscCodes.op_cancel_events, async).ConfigureAwait(false);
				await Xdr.WriteAsync(_handle, async).ConfigureAwait(false);
				await Xdr.WriteAsync(events.LocalId, async).ConfigureAwait(false);

				await Xdr.FlushAsync(async).ConfigureAwait(false);

				await ReadResponseAsync(async).ConfigureAwait(false);
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}

		#endregion

		#region Transaction Methods

		public override async ValueTask<TransactionBase> BeginTransactionAsync(TransactionParameterBuffer tpb, AsyncWrappingCommonArgs async)
		{
			var transaction = new GdsTransaction(this);

			await transaction.BeginTransactionAsync(tpb, async).ConfigureAwait(false);

			return transaction;
		}

		#endregion

		#region Cancel Methods

		public override ValueTask CancelOperationAsync(int kind, AsyncWrappingCommonArgs async)
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

		public override ValueTask<List<object>> GetDatabaseInfoAsync(byte[] items, AsyncWrappingCommonArgs async)
		{
			return GetDatabaseInfoAsync(items, IscCodes.DEFAULT_MAX_BUFFER_SIZE, async);
		}

		public override async ValueTask<List<object>> GetDatabaseInfoAsync(byte[] items, int bufferLength, AsyncWrappingCommonArgs async)
		{
			var buffer = new byte[bufferLength];
			await DatabaseInfoAsync(items, buffer, buffer.Length, async).ConfigureAwait(false);
			return IscHelper.ParseDatabaseInfo(buffer);
		}

		#endregion

		#region Release Object

		public virtual async ValueTask ReleaseObjectAsync(int op, int id, AsyncWrappingCommonArgs async)
		{
			try
			{
				await SendReleaseObjectToBufferAsync(op, id, async).ConfigureAwait(false);
				await Xdr.FlushAsync(async).ConfigureAwait(false);
				await ProcessReleaseObjectResponseAsync(await ReadResponseAsync(async).ConfigureAwait(false), async).ConfigureAwait(false);
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}

		protected virtual async ValueTask SendReleaseObjectToBufferAsync(int op, int id, AsyncWrappingCommonArgs async)
		{
			await Xdr.WriteAsync(op, async).ConfigureAwait(false);
			await Xdr.WriteAsync(id, async).ConfigureAwait(false);
		}

		protected virtual ValueTask ProcessReleaseObjectResponseAsync(IResponse response, AsyncWrappingCommonArgs async)
		{
			return ValueTask2.CompletedTask;
		}

		#endregion

		#region Response Methods

		public virtual ValueTask<int> ReadOperationAsync(AsyncWrappingCommonArgs async)
		{
			return Xdr.ReadOperationAsync(async);
		}

		public virtual async ValueTask<IResponse> ReadResponseAsync(AsyncWrappingCommonArgs async)
		{
			var response = await ReadSingleResponseAsync(async).ConfigureAwait(false);
			GdsConnection.ProcessResponse(response);
			return response;
		}

		public virtual async ValueTask<IResponse> ReadResponseAsync(int operation, AsyncWrappingCommonArgs async)
		{
			var response = await ReadSingleResponseAsync(operation, async).ConfigureAwait(false);
			GdsConnection.ProcessResponse(response);
			return response;
		}

		#endregion

		#region Protected Methods

		protected async ValueTask<IResponse> ReadSingleResponseAsync(AsyncWrappingCommonArgs async) => await ReadSingleResponseAsync(await ReadOperationAsync(async).ConfigureAwait(false), async).ConfigureAwait(false);
		protected virtual async ValueTask<IResponse> ReadSingleResponseAsync(int operation, AsyncWrappingCommonArgs async)
		{
			var response = await GdsConnection.ProcessOperationAsync(operation, Xdr, async).ConfigureAwait(false);
			GdsConnection.ProcessResponseWarnings(response, WarningMessage);
			return response;
		}

		private async ValueTask DatabaseInfoAsync(byte[] items, byte[] buffer, int bufferLength, AsyncWrappingCommonArgs async)
		{
			try
			{
				await Xdr.WriteAsync(IscCodes.op_info_database, async).ConfigureAwait(false);
				await Xdr.WriteAsync(_handle, async).ConfigureAwait(false);
				await Xdr.WriteAsync(Incarnation, async).ConfigureAwait(false);
				await Xdr.WriteBufferAsync(items, items.Length, async).ConfigureAwait(false);
				await Xdr.WriteAsync(bufferLength, async).ConfigureAwait(false);

				await Xdr.FlushAsync(async).ConfigureAwait(false);

				var response = (GenericResponse)await ReadResponseAsync(async).ConfigureAwait(false);

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
