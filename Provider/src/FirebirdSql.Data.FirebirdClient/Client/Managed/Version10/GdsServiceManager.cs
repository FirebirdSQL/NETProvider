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

		public virtual void Attach(ServiceParameterBuffer spb, string dataSource, int port, string service, byte[] cryptKey)
		{
			try
			{
				SendAttachToBuffer(spb, service);
				_database.XdrStream.Flush();
				ProcessAttachResponse(_database.ReadGenericResponse());
			}
			catch (IOException ex)
			{
				_database.Detach();
				throw IscException.ForErrorCode(IscCodes.isc_net_write_err, ex);
			}
		}

		protected virtual void SendAttachToBuffer(ServiceParameterBuffer spb, string service)
		{
			_database.XdrStream.Write(IscCodes.op_service_attach);
			_database.XdrStream.Write(0);
			_database.XdrStream.Write(service);
			_database.XdrStream.WriteBuffer(spb.ToArray());
		}

		protected virtual void ProcessAttachResponse(GenericResponse response)
		{
			_handle = response.ObjectHandle;
		}

		public virtual void Detach()
		{
			try
			{
				_database.XdrStream.Write(IscCodes.op_service_detach);
				_database.XdrStream.Write(Handle);
				_database.XdrStream.Write(IscCodes.op_disconnect);
				_database.XdrStream.Flush();

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
					_connection.Disconnect();
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

		public virtual void Start(ServiceParameterBuffer spb)
		{
			try
			{
				_database.XdrStream.Write(IscCodes.op_service_start);
				_database.XdrStream.Write(Handle);
				_database.XdrStream.Write(0);
				_database.XdrStream.WriteBuffer(spb.ToArray(), spb.Length);
				_database.XdrStream.Flush();

				try
				{
					_database.ReadResponse();
				}
				catch (IscException)
				{
					throw;
				}
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_net_write_err, ex);
			}
		}

		public virtual void Query(ServiceParameterBuffer spb, int requestLength, byte[] requestBuffer, int bufferLength, byte[] buffer)
		{
			try
			{
				_database.XdrStream.Write(IscCodes.op_service_info);
				_database.XdrStream.Write(Handle);
				_database.XdrStream.Write(GdsDatabase.Incarnation);
				_database.XdrStream.WriteBuffer(spb.ToArray(), spb.Length);
				_database.XdrStream.WriteBuffer(requestBuffer, requestLength);
				_database.XdrStream.Write(bufferLength);

				_database.XdrStream.Flush();

				var response = _database.ReadGenericResponse();

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
