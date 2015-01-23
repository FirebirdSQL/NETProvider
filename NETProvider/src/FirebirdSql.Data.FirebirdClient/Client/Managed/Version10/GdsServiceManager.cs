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

		private int             handle;
		private GdsDatabase     database;
		private GdsConnection   connection;

		#endregion

		#region Properties

		public int Handle
		{
			get { return this.handle; }
		}

		#endregion

		#region Constructors

		public GdsServiceManager(GdsConnection connection)
		{
			this.connection = connection;
			this.database   = new GdsDatabase(this.connection);
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
					this.database.Write(IscCodes.op_service_attach);
					this.database.Write(0);
					this.database.Write(service);
					this.database.WriteBuffer(spb.ToArray());
					this.database.Flush();

					response = this.database.ReadGenericResponse();

					this.handle = response.ObjectHandle;
				}
				catch (IOException)
				{
					this.database.Detach();

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
					this.database.Write(IscCodes.op_service_detach);
					this.database.Write(this.Handle);
					this.database.Write(IscCodes.op_disconnect);
					this.database.Flush();

					this.handle = 0;
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_network_error);
				}
				finally
				{
					try
					{
						this.connection.Disconnect();
					}
					catch (IOException)
					{
						throw new IscException(IscCodes.isc_network_error);
					}
					finally
					{
						this.database   = null;
						this.connection = null;
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
					this.database.Write(IscCodes.op_service_start);
					this.database.Write(this.Handle);
					this.database.Write(0);
					this.database.WriteBuffer(spb.ToArray(), spb.Length);
					this.database.Flush();

					try
					{
						this.database.ReadResponse();
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
					this.database.Write(IscCodes.op_service_info);	//	operation
					this.database.Write(this.Handle);				//	db_handle
					this.database.Write(0);										//	incarnation
					this.database.WriteBuffer(spb.ToArray(), spb.Length);		//	Service parameter buffer
					this.database.WriteBuffer(requestBuffer, requestLength);	//	request	buffer
					this.database.Write(bufferLength);				//	result buffer length

					this.database.Flush();

					GenericResponse response = this.database.ReadGenericResponse();

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
