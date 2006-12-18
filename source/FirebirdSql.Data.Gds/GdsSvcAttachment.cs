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
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Gds
{
#if (SINGLE_DLL)
	internal
#else
	public
#endif
	sealed class GdsSvcAttachment : GdsAttachment, ISvcAttachment
	{
		#region Constructors

		public GdsSvcAttachment(AttachmentParams parameters) : base(parameters)
		{
		}

		#endregion

		#region Methods

		public void Attach(string service, SpbBuffer spb)
		{
			lock (this) 
			{
				try 
				{
					this.Connect();
					
					this.Send.Write(IscCodes.op_service_attach);
					this.Send.Write(0);
					this.Send.Write(service);
					this.Send.WriteBuffer(spb.ToArray());
					this.Send.Flush();

					try 
					{
						GdsResponse r = this.ReceiveResponse();
						this.Handle = r.ObjectHandle;
					} 
					catch (IscException g) 
					{
						this.Disconnect();
						throw g;
					}
				} 
				catch (IOException)
				{
					this.Disconnect();
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
					this.Send.Write(IscCodes.op_service_detach);
					this.Send.Write(this.Handle);
					this.Send.Flush();            
					
					GdsResponse r = this.ReceiveResponse();
				} 
				catch (IOException) 
				{
					throw new IscException(IscCodes.isc_network_error);
				}
				finally
				{
					try 
					{
						this.Disconnect();
					}
					catch (IOException) 
					{
						throw new IscException(IscCodes.isc_network_error);
					} 
				}
			}
		}				

		public void Start(SpbBuffer spb)
		{
			lock (this) 
			{
				try 
				{
					this.Send.Write(IscCodes.op_service_start);
					this.Send.Write(this.Handle);
					this.Send.Write(0);
					this.Send.WriteBuffer(spb.ToArray(), spb.Length);
					this.Send.Flush();

					try 
					{
						GdsResponse r = this.ReceiveResponse();
					} 
					catch (IscException g) 
					{
						throw g;
					}
				} 
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_write_err);
				}
			}
		}

		public void Query(
			SpbBuffer	spb				,
			int 		requestLength	,
			byte[] 		requestBuffer	,
			int 		bufferLength	,
			byte[] 		buffer)
		{
			lock (this) 
			{			
				try 
				{					
					this.Send.Write(IscCodes.op_service_info);	//	operation
					this.Send.Write(this.Handle);				//	db_handle
					this.Send.Write(0);							//	incarnation					
					this.Send.WriteTyped(
						IscCodes.isc_spb_version, 
						spb.ToArray());							//	spb
					this.Send.WriteBuffer(
						requestBuffer, 
						requestLength);							//	request buffer
					this.Send.Write(bufferLength);				//	result buffer length

					this.Send.Flush();

					GdsResponse r = this.ReceiveResponse();

					Buffer.BlockCopy(r.Data, 0, buffer, 0, bufferLength);
				}
				catch (IOException) 
				{
					throw new IscException(IscCodes.isc_network_error);
				}
			}
		}
		
		public override void SendWarning(IscException warning) 
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}
