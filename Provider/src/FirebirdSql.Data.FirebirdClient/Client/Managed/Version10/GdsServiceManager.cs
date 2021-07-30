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
	internal class GdsServiceManager : ServiceManagerBase
	{
		#region Fields

		private GdsConnection _connection;
		private GdsDatabase _database;

		#endregion

		#region Properties

		public GdsConnection Connection
		{
			get { return _connection; }
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

		public override async ValueTask Attach(ServiceParameterBufferBase spb, string dataSource, int port, string service, byte[] cryptKey, AsyncWrappingCommonArgs async)
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
				throw IscException.ForIOException(ex);
			}
		}

		protected virtual async ValueTask SendAttachToBuffer(ServiceParameterBufferBase spb, string service, AsyncWrappingCommonArgs async)
		{
			await _database.Xdr.Write(IscCodes.op_service_attach, async).ConfigureAwait(false);
			await _database.Xdr.Write(0, async).ConfigureAwait(false);
			await _database.Xdr.Write(service, async).ConfigureAwait(false);
			await _database.Xdr.WriteBuffer(spb.ToArray(), async).ConfigureAwait(false);
		}

		protected virtual ValueTask ProcessAttachResponse(GenericResponse response, AsyncWrappingCommonArgs async)
		{
			Handle = response.ObjectHandle;
			return ValueTask2.CompletedTask;
		}

		public override async ValueTask Detach(AsyncWrappingCommonArgs async)
		{
			try
			{
				await _database.Xdr.Write(IscCodes.op_service_detach, async).ConfigureAwait(false);
				await _database.Xdr.Write(Handle, async).ConfigureAwait(false);
				await _database.Xdr.Write(IscCodes.op_disconnect, async).ConfigureAwait(false);
				await _database.Xdr.Flush(async).ConfigureAwait(false);

				Handle = 0;
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
			finally
			{
				try
				{
					await _connection.Disconnect(async).ConfigureAwait(false);
				}
				catch (IOException ex)
				{
					throw IscException.ForIOException(ex);
				}
				finally
				{
					_database = null;
					_connection = null;
				}
			}
		}

		public override async ValueTask Start(ServiceParameterBufferBase spb, AsyncWrappingCommonArgs async)
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
				throw IscException.ForIOException(ex);
			}
		}

		public override async ValueTask Query(ServiceParameterBufferBase spb, int requestLength, byte[] requestBuffer, int bufferLength, byte[] buffer, AsyncWrappingCommonArgs async)
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
				throw IscException.ForIOException(ex);
			}
		}

		public override ServiceParameterBufferBase CreateServiceParameterBuffer()
		{
			return new ServiceParameterBuffer2();
		}

		protected virtual GdsDatabase CreateDatabase(GdsConnection connection)
		{
			return new GdsDatabase(connection);
		}

		private void RewireWarningMessage()
		{
			_database.WarningMessage = ex => WarningMessage?.Invoke(ex);
		}

		#endregion
	}
}
