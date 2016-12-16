/*
 *	Firebird ADO.NET Data provider for .NET and Mono
 *
 *	   The contents of this file are subject to the Initial
 *	   Developer's Public License Version 1.0 (the "License");
 *	   you may not use this file except in compliance with the
 *	   License. You may obtain a copy of the License at
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *	   express or implied. See the License for the specific
 *	   language governing rights and limitations under the License.
 *
 *	Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *	All Rights Reserved.
 *
 *  Contributors:
 *      Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.IO;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version10
{
	internal sealed class GdsServiceManager : IServiceManager
	{
		#region Fields

		private int _handle;
		private GdsDatabase _database;
		private GdsConnection _connection;

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

		#endregion

		#region Constructors

		public GdsServiceManager(GdsConnection connection)
		{
			_connection = connection;
			_database = new GdsDatabase(_connection);
		}

		#endregion

		#region Methods

		public void Attach(ServiceParameterBuffer spb, string dataSource, int port, string service)
		{
			GenericResponse response = null;

			try
			{
#warning Separate method for op_service_attach as for i.e. op_attach
				_database.XdrStream.Write(IscCodes.op_service_attach);
				_database.XdrStream.Write(0);
				_database.XdrStream.Write(service);
				_database.XdrStream.WriteBuffer(spb.ToArray());
				_database.XdrStream.Flush();

				response = _database.ReadGenericResponse();

				_handle = response.ObjectHandle;
			}
			catch (IOException ex)
			{
				_database.Detach();
				throw IscException.ForErrorCode(IscCodes.isc_net_write_err, ex);
			}
		}

		public void Detach()
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

		public void Start(ServiceParameterBuffer spb)
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

		public void Query(ServiceParameterBuffer spb, int requestLength, byte[] requestBuffer, int bufferLength, byte[] buffer)
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

				GenericResponse response = _database.ReadGenericResponse();

				int responseLength = bufferLength;

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

		#endregion
	}
}
