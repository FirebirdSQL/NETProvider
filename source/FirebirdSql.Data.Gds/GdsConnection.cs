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
 *	Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Gds
{
	internal sealed class GdsConnection
    {
        #region · Fields ·

        private Socket			socket;
		private NetworkStream	networkStream;
		private XdrStream		send;
		private XdrStream		receive;
        private string          dataSource;
        private int             portNumber;
        private int             packetSize;
        private Charset         characterSet;
        private int             protocolVersion;
        private int             protocolArchitecture;
        private int             protocolMinimunType;

        #endregion

        #region · Properties ·

        public bool IsConnected
        {
            get { return (this.socket != null && this.socket.Connected); }
        }

        public int ProtocolVersion
        {
            get { return this.protocolVersion; }
        }

        public int ProtocolArchitecture
        {
            get { return this.protocolArchitecture; }
        }

        public int ProtocolMinimunType
        {
            get { return this.protocolMinimunType; }
        }

        public Boolean DataAvailable
        {
            get { return this.networkStream.DataAvailable; }
        }

        #endregion

		#region · Internal Properties ·

		internal XdrStream Receive
		{
			get { return this.receive; }
		}

		internal XdrStream Send
		{
			get { return this.send; }
		}

		#endregion

		#region · Constructors ·

        public GdsConnection(string dataSource, int port)
            : this(dataSource, port, 8192, Charset.DefaultCharset)
        {
        }

        public GdsConnection(string dataSource, int portNumber, int packetSize, Charset characterSet)
        {
            this.dataSource     = dataSource;
            this.portNumber     = portNumber;
            this.packetSize     = packetSize;
            this.characterSet   = characterSet;

            GC.SuppressFinalize(this);
        }

		#endregion

		#region · Methods ·

		public void Connect()
		{
			try
			{
				IPAddress hostadd = Dns.Resolve(dataSource).AddressList[0];
				IPEndPoint EPhost = new IPEndPoint(hostadd, this.portNumber);

				this.socket = new Socket(
					AddressFamily.InterNetwork,
					SocketType.Stream,
					ProtocolType.Tcp);

#if (!NETCF)

				// Set Receive Buffer size.
				this.socket.SetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.ReceiveBuffer,
					packetSize);

				// Set Send	Buffer size.
				this.socket.SetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.SendBuffer,
					packetSize);
#endif

				// Disables	the	Nagle algorithm	for	send coalescing.
				this.socket.SetSocketOption(
					SocketOptionLevel.Tcp,
					SocketOptionName.NoDelay,
					1);

				// Make	the	socket to connect to the Server
				this.socket.Connect(EPhost);
				this.networkStream = new NetworkStream(this.socket, true);

#if	(NETCF)
				this.send	 = new XdrStream(this.networkStream, this.characterSet);
				this.receive = new XdrStream(this.networkStream, this.characterSet);
#else
                this.send       = new XdrStream(new BufferedStream(this.networkStream), this.characterSet);
                this.receive    = new XdrStream(new BufferedStream(this.networkStream), this.characterSet);
#endif

				GC.SuppressFinalize(this.socket);
				GC.SuppressFinalize(this.networkStream);
				GC.SuppressFinalize(this.send);
				GC.SuppressFinalize(this.receive);
			}
			catch (SocketException)
			{
				throw new IscException(IscCodes.isc_arg_gds, IscCodes.isc_network_error, dataSource);
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
			catch (IOException)
			{
				throw;
			}
		}

        public void Identify(string database)
        {
            try
            {
                // Here	we identify	the	user to	the	engine.	 
                // This	may	or may not be used as login	info to	a database.				
                byte[] user = Encoding.Default.GetBytes(System.Environment.UserName);
                byte[] host = Encoding.Default.GetBytes(System.Net.Dns.GetHostName());

                MemoryStream user_id = new MemoryStream();

                // User	Name
                user_id.WriteByte(1);
                user_id.WriteByte((byte)user.Length);
                user_id.Write(user, 0, user.Length);

                // Host	name
                user_id.WriteByte(4);
                user_id.WriteByte((byte)host.Length);
                user_id.Write(host, 0, host.Length);

                // Attach/create using this connection will use user verification
                user_id.WriteByte(6);
                user_id.WriteByte(0);

                this.send.Write(IscCodes.op_connect);
                this.send.Write(IscCodes.op_attach);
                this.send.Write(IscCodes.CONNECT_VERSION2);	// CONNECT_VERSION2
                this.send.Write(1);							// Architecture	of client -	Generic

                this.send.Write(database);					// Database	path
                this.send.Write(2);							// Protocol	versions understood
                this.send.WriteBuffer(user_id.ToArray());	// User	identification Stuff

                this.send.Write(IscCodes.PROTOCOL_VERSION10);//	Protocol version
                this.send.Write(1);							// Architecture	of client -	Generic
                this.send.Write(2);							// Minumum type
                this.send.Write(3);							// Maximum type
                this.send.Write(2);							// Preference weight

                this.send.Write(IscCodes.PROTOCOL_VERSION11);//	Protocol version
                this.send.Write(1);							// Architecture	of client -	Generic
                this.send.Write(2);							// Minumum type
                this.send.Write(3);							// Maximum type
                this.send.Write(2);							// Preference weight

                this.send.Flush();

                if (this.receive.ReadOperation() == IscCodes.op_accept)
                {
                    this.protocolVersion        = this.receive.ReadInt32();	// Protocol	version
                    this.protocolArchitecture   = this.receive.ReadInt32();	// Architecture	for	protocol
                    this.protocolMinimunType    = this.receive.ReadInt32();	// Minimum type

                    if (this.protocolVersion < 0)
                    {
                        this.protocolVersion = (this.protocolVersion & IscCodes.FB_PROTOCOL_FLAG) | 11;
                    }
                }
                else
                {
                    try
                    {
                        this.Disconnect();
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        throw new IscException(IscCodes.isc_connect_reject);
                    }
                }
            }
            catch (IOException)
            {
                throw new IscException(IscCodes.isc_network_error);
            }
        }

        public XdrStream CreateXdrStream()
        {
#if	(NET_CF)
            return new XdrStream(this.networkStream, this.characterSet);
#else
            return new XdrStream(new BufferedStream(this.networkStream), this.characterSet);
#endif
        }

		#endregion

		#region Internal Methods

		internal GdsResponse ReadGenericResponse()
		{
			try
			{
				if (this.Receive.ReadOperation() == IscCodes.op_response)
				{
					GdsResponse r = new GdsResponse(
						this.receive.ReadInt32(),
						this.receive.ReadInt64(),
						this.receive.ReadBuffer());

					r.Warning = this.ReadStatusVector();

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

		internal IscException ReadStatusVector()
		{
			IscException exception = null;
			bool eof = false;

			try
			{
				while (!eof)
				{
					int arg = this.receive.ReadInt32();

					switch (arg)
					{
						case IscCodes.isc_arg_gds:
							int er = this.receive.ReadInt32();
							if (er != 0)
							{
								if (exception == null)
								{
									exception = new IscException();
								}
								exception.Errors.Add(arg, er);
							}
							break;

						case IscCodes.isc_arg_end:
							if (exception != null && exception.Errors.Count != 0)
							{
								exception.BuildExceptionMessage();
							}
							eof = true;
							break;

						case IscCodes.isc_arg_interpreted:
						case IscCodes.isc_arg_string:
							exception.Errors.Add(arg, this.receive.ReadString());
							break;

						case IscCodes.isc_arg_number:
							exception.Errors.Add(arg, this.receive.ReadInt32());
							break;

						default:
							{
								int e = this.receive.ReadInt32();
								if (e != 0)
								{
									if (exception == null)
									{
										exception = new IscException();
									}
									exception.Errors.Add(arg, e);
								}
							}
							break;
					}
				}
			}
			catch (IOException)
			{
				throw new IscException(IscCodes.isc_arg_gds, IscCodes.isc_net_read_err);
			}

			if (exception != null && !exception.IsWarning)
			{
				throw exception;
			}

			return exception;
		}

		#endregion
	}
}
