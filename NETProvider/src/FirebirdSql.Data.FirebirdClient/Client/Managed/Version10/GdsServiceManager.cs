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

		private int             _handle;
		private GdsDatabase     _database;
		private GdsConnection   _connection;

		#endregion

		#region Properties

		public int Handle
		{
			get { return _handle; }
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

			lock (this)
			{
				try
				{
					_database.Write(IscCodes.op_service_attach);
					_database.Write(0);
					_database.Write(service);
					_database.WriteBuffer(spb.ToArray());
					_database.Flush();

					response = _database.ReadGenericResponse();

					_handle = response.ObjectHandle;
				}
				catch (IOException)
				{
					_database.Detach();

					throw new IscException(IscCodes.isc_net_write_err);
				}
			}
		}

		public void Detach()
		{
			lock (this)
			{
				try
				{
					_database.Write(IscCodes.op_service_detach);
					_database.Write(Handle);
					_database.Write(IscCodes.op_disconnect);
					_database.Flush();

					_handle = 0;
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_network_error);
				}
				finally
				{
					try
					{
						_connection.Disconnect();
					}
					catch (IOException)
					{
						throw new IscException(IscCodes.isc_network_error);
					}
					finally
					{
						_database = null;
						_connection = null;
					}
				}
			}
		}

		public void Start(ServiceParameterBuffer spb)
		{
			lock (this)
			{
				try
				{
					_database.Write(IscCodes.op_service_start);
					_database.Write(Handle);
					_database.Write(0);
					_database.WriteBuffer(spb.ToArray(), spb.Length);
					_database.Flush();

					try
					{
						_database.ReadResponse();
					}
					catch (IscException)
					{
						throw;
					}
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_write_err);
				}
			}
		}

		public void Query(
			ServiceParameterBuffer	spb,
			int						requestLength,
			byte[]					requestBuffer,
			int						bufferLength,
			byte[]					buffer)
		{
			lock (this)
			{
				try
				{
					_database.Write(IscCodes.op_service_info);  //	operation
					_database.Write(Handle);                //	db_handle
					_database.Write(0);                                     //	incarnation
					_database.WriteBuffer(spb.ToArray(), spb.Length);       //	Service parameter buffer
					_database.WriteBuffer(requestBuffer, requestLength);    //	request	buffer
					_database.Write(bufferLength);              //	result buffer length

					_database.Flush();

					GenericResponse response = _database.ReadGenericResponse();

					int responseLength = bufferLength;

					if (response.Data.Length < bufferLength)
					{
						responseLength = response.Data.Length;
					}

					Buffer.BlockCopy(response.Data, 0, buffer, 0, responseLength);
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_network_error);
				}
			}
		}

		#endregion
	}
}
