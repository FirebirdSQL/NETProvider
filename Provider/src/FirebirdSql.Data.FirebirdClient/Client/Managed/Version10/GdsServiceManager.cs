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
	internal class GdsServiceManager : IServiceManager
	{
		#region Callbacks

		public Action<IscException> WarningMessage
		{
			get { return _warningMessage; }
			set { _warningMessage = value; }
		}

		#endregion

		#region Fields

		private Action<IscException> _warningMessage;

		private int _handle;
		private GdsConnection _connection;
		private GdsDatabase _database;

		#endregion

		#region Properties

		public int Handle
		{
			get { return _handle; }
		}

		public byte[] AuthData
		{
			get { return _connection.AuthData; }
		}

		public GdsDatabase Database
		{
			get { return _database; }
		}

		#endregion

		#region Constructors

		public GdsServiceManager(GdsConnection connection)
		{
			_connection = connection;
			_database = CreateDatabase(_connection);
			RewireWarningMessage();
		}

		#endregion

		#region Methods

		public virtual async Task Attach(ServiceParameterBuffer spb, string dataSource, int port, string service, byte[] cryptKey, AsyncWrappingCommonArgs async)
		{
			try
			{
				await SendAttachToBuffer(spb, service, async).ConfigureAwait(false);
				await _database.Xdr.Flush(async).ConfigureAwait(false);
				await ProcessAttachResponse((GenericResponse)await _database.ReadResponse(async).ConfigureAwait(false), async).ConfigureAwait(false);
			}
			catch (IOException ex)
			{
				await _database.Detach(async).ConfigureAwait(false);
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		protected virtual async Task SendAttachToBuffer(ServiceParameterBuffer spb, string service, AsyncWrappingCommonArgs async)
		{
			await _database.Xdr.Write(IscCodes.op_service_attach, async).ConfigureAwait(false);
			await _database.Xdr.Write(0, async).ConfigureAwait(false);
			await _database.Xdr.Write(service, async).ConfigureAwait(false);
			await _database.Xdr.WriteBuffer(spb.ToArray(), async).ConfigureAwait(false);
		}

		protected virtual Task ProcessAttachResponse(GenericResponse response, AsyncWrappingCommonArgs async)
		{
			_handle = response.ObjectHandle;
			return Task.CompletedTask;
		}

		public virtual async Task Detach(AsyncWrappingCommonArgs async)
		{
			try
			{
				await _database.Xdr.Write(IscCodes.op_service_detach, async).ConfigureAwait(false);
				await _database.Xdr.Write(Handle, async).ConfigureAwait(false);
				await _database.Xdr.Write(IscCodes.op_disconnect, async).ConfigureAwait(false);
				await _database.Xdr.Flush(async).ConfigureAwait(false);

				_handle = 0;
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
			finally
			{
				try
				{
					await _connection.Disconnect(async).ConfigureAwait(false);
				}
				catch (IOException ex)
				{
					throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
				}
				finally
				{
					_database = null;
					_connection = null;
				}
			}
		}

		public virtual async Task Start(ServiceParameterBuffer spb, AsyncWrappingCommonArgs async)
		{
			try
			{
				await _database.Xdr.Write(IscCodes.op_service_start, async).ConfigureAwait(false);
				await _database.Xdr.Write(Handle, async).ConfigureAwait(false);
				await _database.Xdr.Write(0, async).ConfigureAwait(false);
				await _database.Xdr.WriteBuffer(spb.ToArray(), spb.Length, async).ConfigureAwait(false);
				await _database.Xdr.Flush(async).ConfigureAwait(false);

				try
				{
					await _database.ReadResponse(async).ConfigureAwait(false);
				}
				catch (IscException)
				{
					throw;
				}
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		public virtual async Task Query(ServiceParameterBuffer spb, int requestLength, byte[] requestBuffer, int bufferLength, byte[] buffer, AsyncWrappingCommonArgs async)
		{
			try
			{
				await _database.Xdr.Write(IscCodes.op_service_info, async).ConfigureAwait(false);
				await _database.Xdr.Write(Handle, async).ConfigureAwait(false);
				await _database.Xdr.Write(GdsDatabase.Incarnation, async).ConfigureAwait(false);
				await _database.Xdr.WriteBuffer(spb.ToArray(), spb.Length, async).ConfigureAwait(false);
				await _database.Xdr.WriteBuffer(requestBuffer, requestLength, async).ConfigureAwait(false);
				await _database.Xdr.Write(bufferLength, async).ConfigureAwait(false);

				await _database.Xdr.Flush(async).ConfigureAwait(false);

				var response = (GenericResponse)await _database.ReadResponse(async).ConfigureAwait(false);

				var responseLength = bufferLength;

				if (response.Data.Length < bufferLength)
				{
					responseLength = response.Data.Length;
				}

				Buffer.BlockCopy(response.Data, 0, buffer, 0, responseLength);
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		protected virtual GdsDatabase CreateDatabase(GdsConnection connection)
		{
			return new GdsDatabase(connection);
		}

		private void RewireWarningMessage()
		{
			_database.WarningMessage = ex => _warningMessage?.Invoke(ex);
		}

		#endregion
	}
}
