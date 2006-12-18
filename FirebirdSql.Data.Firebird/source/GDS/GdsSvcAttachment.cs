/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.ibphoenix.com/main.nfs?a=ibphoenix&l=;PAGES;NAME='ibp_idpl'
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.IO;

using FirebirdSql.Data.Firebird.Gds;

namespace FirebirdSql.Data.Firebird.Gds
{
	internal class GdsSvcAttachment : GdsAttachment
	{
		#region FIELDS

		private string			service;
		private GdsSpbBuffer	spb;

		#endregion

		#region CONSTRUCTORS

		public GdsSvcAttachment(GdsAttachParams parameters) : base(parameters)
		{
		}

		#endregion

		#region METHODS

		public void Attach(string service, GdsSpbBuffer spb)
		{
			this.service	= service;
			this.spb		= spb;
			Attach();
		}

		public override void Detach()
		{
			lock (this) 
			{	        
				try 
				{
					Send.WriteInt(GdsCodes.op_service_detach);
					Send.WriteInt(Handle);
					Send.Flush();            
					
					GdsResponse r = ReceiveResponse();
				} 
				catch (IOException) 
				{
					throw new GdsException(GdsCodes.isc_network_error);
				}
				finally
				{
					try 
					{
						Disconnect();
					}
					catch (IOException) 
					{
						throw new GdsException(GdsCodes.isc_network_error);
					} 
				}
			}
		}				

		public void Start(GdsSpbBuffer	spb)
		{
			lock (this) 
			{
				try 
				{
					Send.WriteInt(GdsCodes.op_service_start);
					Send.WriteInt(Handle);
					Send.WriteInt(0);
					Send.WriteBuffer(spb.ToArray(), spb.Length);
					Send.Flush();

					try 
					{
						GdsResponse r = ReceiveResponse();
					} 
					catch (GdsException g) 
					{
						throw g;
					}
				} 
				catch (IOException)
				{
					throw new GdsException(GdsCodes.isc_net_write_err);
				}
			}
		}

		public void Query(GdsSpbBuffer	spb		,
			int 		request_length	,
			byte[] 		request_buffer	,
			int 		buffer_length	,
			byte[] 		buffer)
		{
			lock (this) 
			{			
				try 
				{					
					Send.WriteInt(GdsCodes.op_service_info);		//	operation
					Send.WriteInt(Handle);							//	db_handle
					Send.WriteInt(0);								//	incarnation					
					Send.WriteTyped(GdsCodes.isc_spb_version, 
									spb.ToArray());					//	spb
					Send.WriteBuffer(request_buffer, 
									request_length);				//	request buffer
					Send.WriteInt(buffer_length);					//	result buffer length

					Send.Flush();

					GdsResponse r = ReceiveResponse();

					System.Array.Copy(r.Data, 0, buffer, 0, buffer_length);
				}
				catch (IOException) 
				{
					throw new GdsException(GdsCodes.isc_network_error);
				}
			}
		}

		public override void Attach()
		{
			lock (this) 
			{
				try 
				{
					Connect();
					
					Send.WriteInt(GdsCodes.op_service_attach);
					Send.WriteInt(0);
					Send.WriteString(service);
					Send.WriteTyped(GdsCodes.isc_spb_version, 
									spb.ToArray());
					Send.Flush();

					try 
					{
						GdsResponse r = ReceiveResponse();
						Handle = r.ObjectHandle;
					} 
					catch (GdsException g) 
					{
						Disconnect();
						throw g;
					}
				} 
				catch (IOException)
				{
					Disconnect();
					throw new GdsException(GdsCodes.isc_net_write_err);
				}
			}
		}

		public override void SendWarning(GdsException ex)
		{
		}
		
		#endregion
	}
}
