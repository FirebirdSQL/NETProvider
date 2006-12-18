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

namespace FirebirdSql.Data.Firebird.Gds
{
	internal abstract class GdsAttachment
	{
		#region FIELDS

		private GdsAttachParams	parameters;
		private int				handle;
		private int				op;
		private Socket			socket;
		private NetworkStream	networkStream;
		private GdsInetWriter	send;
		private GdsInetReader	receive;		

		#endregion

		#region PROPERTIES

		public GdsAttachParams Parameters
		{
			get { return parameters; }
			set { parameters = value; }
		}

		public int Handle
		{
			get { return handle; }
			set { handle = value; }
		}

		public int OP
		{
			get { return op; }
			set { op = value; }
		}

		public GdsInetReader Receive
		{
			get { return receive; }
		}

		public GdsInetWriter Send
		{
			get { return send; }
		}

		#endregion

		#region CONSTRUCTORS

		protected GdsAttachment(GdsAttachParams parameters)
		{
			this.op			= -1;	
			this.parameters = parameters;
		}

		#endregion

		#region ABSTRACT_METHODS

		public abstract void Attach();
		public abstract void Detach();
		public abstract void SendWarning(GdsException warning);

		#endregion

		#region METHODS

		public int VaxInteger(byte[] buffer, int pos, int length) 
		{
			int newValue;
			int shift;

			newValue = shift = 0;

			int i = pos;
			while (--length >= 0) 
			{
				newValue += (buffer[i++] & 0xff) << shift;
				shift += 8;
			}
			
			return newValue;
		}

		public void ReleaseObject(int op, int id)
		{
			lock (this)
			{
				try 
				{
					Send.WriteInt(op);
					Send.WriteInt(id);
					Send.Flush();            
					
					GdsResponse r = ReceiveResponse();
				}
				catch (IOException) 
				{
					throw new GdsException(GdsCodes.isc_net_read_err);
				}
			}
		}

		protected void Connect()
		{
			try
			{
				IPAddress hostadd = Dns.Resolve(parameters.DataSource).AddressList[0];
				IPEndPoint EPhost = new IPEndPoint(hostadd, parameters.Port);

				socket = new Socket(AddressFamily.InterNetwork,
					SocketType.Stream,
					ProtocolType.IP);

				// Set Receive Buffer size.
				socket.SetSocketOption(SocketOptionLevel.Socket,
					SocketOptionName.ReceiveBuffer, parameters.PacketSize);

				// Set Send Buffer size.
				socket.SetSocketOption(SocketOptionLevel.Socket,
					SocketOptionName.SendBuffer, parameters.PacketSize);
				
				// Disables the Nagle algorithm for send coalescing.
				#if (!_MONO)
					// This seems to be not supported by MONO
					socket.SetSocketOption(SocketOptionLevel.Socket,
						SocketOptionName.NoDelay, 1);
				#endif

				// Make the socket to connect to the Server
				socket.Connect(EPhost);
				networkStream = new NetworkStream(socket, true);

				send = new GdsInetWriter(
					new BufferedStream(networkStream), 
					parameters.Charset.Encoding);

				receive = new GdsInetReader(
					new BufferedStream(networkStream), 
					parameters.Charset.Encoding,
					parameters.Charset.Name);
			}
			catch (SocketException) 
			{
				throw new GdsException(
					GdsCodes.isc_arg_gds, 
					GdsCodes.isc_network_error, 
					parameters.DataSource);
			}
		}

		protected virtual void Disconnect()
		{
			try
			{
				if (receive != null)
				{
					receive.Close();
				}
				if (send != null)
				{
					send.Close();
				}
				if (networkStream != null)
				{
					networkStream.Close();
				}
				if (socket != null)
				{
					socket.Close();
				}
			     
				receive			= null;
				send			= null;				
				socket			= null;
				networkStream	= null;
			}
			catch(IOException ex)
			{
				throw ex;
			}
		}

		#endregion

		#region RESPONSE_METHODS

		public void ReadStatusVector()
		{
			try 
			{
				GdsException exception = new GdsException();

				while (true) 
				{
					int arg = receive.ReadInt();
					switch (arg) 
					{
						case GdsCodes.isc_arg_gds: 
							int er = receive.ReadInt();
							if (er != 0) 
							{
								exception.Errors.Add(arg, er);
							}
							break;

						case GdsCodes.isc_arg_end:
						{		
							if (exception.Errors.Count != 0 && 
								!exception.IsWarning()) 
							{
								exception.BuildExceptionMessage();
								throw exception;
							}
							else
							{
								if (exception.Errors.Count != 0 &&
									exception.IsWarning())
								{
									exception.BuildExceptionMessage();
									SendWarning(exception);
								}
							}
						}
						return;
						
						case GdsCodes.isc_arg_interpreted:						
						case GdsCodes.isc_arg_string:
						{
							string arg_value = receive.ReadString();
							exception.Errors.Add(arg, arg_value);
						}
						break;
						
						case GdsCodes.isc_arg_number:
						{
							int arg_value = receive.ReadInt();
							exception.Errors.Add(arg, arg_value);
						}
						break;
						
						default:
						{
							int e = receive.ReadInt();
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
				throw new GdsException(
							GdsCodes.isc_arg_gds, 
							GdsCodes.isc_net_read_err, 
							ioe.Message);
			}
		}

		public int ReadOperation()
		{
			int op = (this.op >= 0) ? this.op : NextOperation();
			this.op = -1;

			return op;
		}

		public int NextOperation()
		{
			do 
			{
				/* loop as long as we are receiving dummy packets, just
				 * throwing them away--note that if we are a server we won't
				 * be receiving them, but it is better to check for them at
				 * this level rather than try to catch them in all places where
				 * this routine is called 
				 */
				op = receive.ReadInt();
			} while (op == GdsCodes.op_dummy);

			return op;
		}

		public GdsResponse ReceiveResponse()
		{
			try 
			{
				int op = ReadOperation();
				if (op == GdsCodes.op_response) 
				{
					GdsResponse r = new GdsResponse(
						receive.ReadInt()	,
						receive.ReadLong()	,
						receive.ReadBuffer());

					ReadStatusVector();
			
					return r;
				} 
				else 
				{
					return null;
				}
			} 
			catch (IOException ex) 
			{
				// ex.getMessage() makes little sense here, it will not be displayed
				// because error message for isc_net_read_err does not accept params
				throw new GdsException(GdsCodes.isc_arg_gds, GdsCodes.isc_net_read_err, ex.Message);
			}
		}

		#endregion
	}
}
