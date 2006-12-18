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
using System.Net;
using System.Net.Sockets;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Gds
{
#if (SINGLE_DLL)
	internal
#else
	public
#endif
	abstract class GdsAttachment : IAttachment
	{
		#region Fields

		private AttachmentParams	parameters;
		private int					handle;
		private int					op;
		private Socket				socket;
		private NetworkStream		networkStream;
		private XdrStream			send;
		private XdrStream			receive;		

		#endregion

		#region Properties

		public AttachmentParams Parameters
		{
			get { return this.parameters; }
		}

		public int Handle
		{
			get { return this.handle; }
			set { this.handle = value; }
		}

		public int OP
		{
			get { return this.op; }
			set { this.op = value; }
		}

		public FactoryBase Factory
		{
			get { return GdsFactory.Instance; }
		}

		public bool IsLittleEndian
		{
			get { return false; }
		}

		#endregion

		#region Internal Properties

		internal XdrStream Receive
		{
			get { return this.receive; }
		}

		internal XdrStream Send
		{
			get { return this.send; }
		}

		#endregion

		#region Constructors

		protected GdsAttachment(AttachmentParams parameters)
		{
			this.op			= -1;
			this.parameters = parameters;
		}

		#endregion

		#region Methods

		public void Connect()
		{
			try
			{
				IPAddress hostadd = Dns.Resolve(this.parameters.DataSource).AddressList[0];
				IPEndPoint EPhost = new IPEndPoint(hostadd, this.parameters.Port);

				this.socket = new Socket(
					AddressFamily.InterNetwork,
					SocketType.Stream,
					ProtocolType.IP);

				// Set Receive Buffer size.
				this.socket.SetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.ReceiveBuffer, 
					this.parameters.PacketSize);

				// Set Send Buffer size.
				this.socket.SetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.SendBuffer, 
					this.parameters.PacketSize);
				
#if (!LINUX)
				// Disables the Nagle algorithm for send coalescing.
				// This seems to be not supported in Linux (using mono::)
				this.socket.SetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.NoDelay, 
					1);
#endif

				// Make the socket to connect to the Server
				this.socket.Connect(EPhost);
				this.networkStream = new NetworkStream(this.socket, true);

				this.send = new XdrStream(
					new BufferedStream(networkStream), 
					this.parameters.Charset);

				this.receive = new XdrStream(
					new BufferedStream(this.networkStream), 
					this.parameters.Charset);
			}
			catch (SocketException) 
			{
				throw new IscException(
					IscCodes.isc_arg_gds, 
					IscCodes.isc_network_error, 
					this.parameters.DataSource);
			}
		}

		public void Disconnect()
		{
			try
			{
				if (this.receive != null)
				{
					this.receive.Close();
				}
				if (this.send != null)
				{
					this.send.Close();
				}
				if (this.networkStream != null)
				{
					this.networkStream.Close();
				}
				if (this.socket != null)
				{
					this.socket.Close();
				}
			     
				this.receive		= null;
				this.send			= null;				
				this.socket			= null;
				this.networkStream	= null;
			}
			catch(IOException ex)
			{
				throw ex;
			}
		}

		public int VaxInteger(byte[] buffer, int index, int length) 
		{
			return IscHelper.VaxInteger(buffer, index, length) ;
		}

		#endregion

		#region Abstract Methods

		public abstract void SendWarning(IscException ex);

		#endregion

		#region Internal Methods

		internal void ReleaseObject(int op, int id)
		{
			lock (this)
			{
				try 
				{
					this.send.Write(op);
					this.send.Write(id);
					this.send.Flush();            
					
					GdsResponse r = this.ReceiveResponse();
				}
				catch (IOException) 
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		internal void ReadStatusVector()
		{
			try 
			{
				IscException exception = new IscException();

				while (true) 
				{
					int arg = this.receive.ReadInt32();
					switch (arg) 
					{
						case IscCodes.isc_arg_gds: 
							int er = this.receive.ReadInt32();
							if (er != 0) 
							{
								exception.Errors.Add(arg, er);
							}
							break;

						case IscCodes.isc_arg_end:
						{		
							if (exception.Errors.Count != 0 && !exception.IsWarning()) 
							{
								exception.BuildExceptionMessage();
								throw exception;
							}
							else
							{
								if (exception.Errors.Count != 0 && exception.IsWarning())
								{
									exception.BuildExceptionMessage();
									this.SendWarning(exception);
								}
							}
						}
						return;
						
						case IscCodes.isc_arg_interpreted:						
						case IscCodes.isc_arg_string:
						{
							string arg_value = this.receive.ReadString();
							exception.Errors.Add(arg, arg_value);
						}
						break;
						
						case IscCodes.isc_arg_number:
						{
							int arg_value = this.receive.ReadInt32();
							exception.Errors.Add(arg, arg_value);
						}
						break;
						
						default:
						{
							int e = this.receive.ReadInt32();
							if (e != 0) 
							{
								exception.Errors.Add(arg, e);
							}
						}
						break;
					}
				}
			}
			catch (IOException ioe)
			{
				/* ioe.getMessage() makes little sense here, it will not be displayed
				 * because error message for isc_net_read_err does not accept params
				 */
				throw new IscException(
							IscCodes.isc_arg_gds, 
							IscCodes.isc_net_read_err, 
							ioe.Message);
			}
		}

		internal int ReadOperation()
		{
			int op = (this.op >= 0) ? this.op : this.NextOperation();
			this.op = -1;

			return op;
		}

		internal int NextOperation()
		{
			do 
			{
				/* loop as long as we are receiving dummy packets, just
				 * throwing them away--note that if we are a server we won't
				 * be receiving them, but it is better to check for them at
				 * this level rather than try to catch them in all places where
				 * this routine is called 
				 */
				op = this.receive.ReadInt32();
			} while (op == IscCodes.op_dummy);

			return op;
		}

		internal GdsResponse ReceiveResponse()
		{
			try 
			{
				int op = this.ReadOperation();
				if (op == IscCodes.op_response) 
				{
					GdsResponse r = new GdsResponse(
						this.receive.ReadInt32(),
						this.receive.ReadInt64(),
						this.receive.ReadBuffer());

					this.ReadStatusVector();
			
					return r;
				} 
				else 
				{
					return null;
				}
			} 
			catch (IOException) 
			{
				throw new IscException(IscCodes.isc_net_read_err);
			}
		}

		#endregion
	}
}
