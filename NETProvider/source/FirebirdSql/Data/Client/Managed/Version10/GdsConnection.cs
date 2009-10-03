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
 *	Copyright (c) 2002 - 2007 Carlos Guzman Alvarez
 *	Copyright (c) 2007 - 2008 Jiri Cincura (jiri@cincura.net)
 *	All Rights Reserved.
 */

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Permissions;

using FirebirdSql.Data.Common;
using System.Text;

namespace FirebirdSql.Data.Client.Managed.Version10
{
    internal class GdsConnection
    {
        #region · Fields ·

        private Socket socket;
        private NetworkStream networkStream;
        private string dataSource;
        private int portNumber;
        private int packetSize;
        private Charset characterSet;
        private int protocolVersion;
        private int protocolArchitecture;
        private int protocolMinimunType;

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

        public bool DataAvailable
        {
            get { return this.networkStream.DataAvailable; }
        }

        internal IPAddress IPAddress { get; private set; }

        #endregion

        #region · Constructors ·

        public GdsConnection(string dataSource, int port)
            : this(dataSource, port, 8192, Charset.DefaultCharset)
        {
        }

        public GdsConnection(string dataSource, int portNumber, int packetSize, Charset characterSet)
        {
            this.dataSource = dataSource;
            this.portNumber = portNumber;
            this.packetSize = packetSize;
            this.characterSet = characterSet;

            GC.SuppressFinalize(this);
        }

        #endregion

        #region · Methods ·

        public virtual void Connect()
        {
            try
            {
                this.IPAddress = this.GetIPAddress(this.dataSource, AddressFamily.InterNetwork);
                IPEndPoint endPoint = new IPEndPoint(this.IPAddress, this.portNumber);

                this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

#if	(!NET_CF)
                // Set Receive Buffer size.
                this.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, packetSize);

                // Set Send	Buffer size.
                this.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, packetSize);
#endif
                // Disables	the	Nagle algorithm	for	send coalescing.
                this.socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);

                // Make	the	socket to connect to the Server
                this.socket.Connect(endPoint);
                this.networkStream = new NetworkStream(this.socket, true);

                GC.SuppressFinalize(this.socket);
                GC.SuppressFinalize(this.networkStream);
            }
            catch (SocketException)
            {
                throw new IscException(IscCodes.isc_arg_gds, IscCodes.isc_network_error, dataSource);
            }
        }

        public virtual void Identify(string database)
        {
            // handles this.networkStream
            XdrStream inputStream = this.CreateXdrStream();
            XdrStream outputStream = this.CreateXdrStream();

            try
            {
                // Here	we identify	the	user to	the	engine.	 
                // This	may	or may not be used as login	info to	a database.				
#if	(!NET_CF)
                byte[] user = Encoding.Default.GetBytes(System.Environment.UserName);
                byte[] host = Encoding.Default.GetBytes(System.Net.Dns.GetHostName());
#else
				byte[] user = Encoding.Default.GetBytes("fbnetcf");
				byte[] host = Encoding.Default.GetBytes(System.Net.Dns.GetHostName());
#endif

                using (MemoryStream user_id = new MemoryStream())
                {
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

                    outputStream.Write(IscCodes.op_connect);
                    outputStream.Write(IscCodes.op_attach);
                    outputStream.Write(IscCodes.CONNECT_VERSION2);	// CONNECT_VERSION2
                    outputStream.Write(1);							// Architecture	of client -	Generic

                    outputStream.Write(database);					// Database	path
#if (PROTOCOL_VERSION12)
                    outputStream.Write(3);							// Protocol	versions understood
#endif
                    outputStream.Write(2);							// Protocol	versions understood
                    outputStream.WriteBuffer(user_id.ToArray());	// User	identification Stuff

                    outputStream.Write(IscCodes.PROTOCOL_VERSION10);//	Protocol version
                    outputStream.Write(1);							// Architecture	of client -	Generic
                    outputStream.Write(2);							// Minimum type (ptype_rpc)
                    outputStream.Write(3);							// Maximum type (ptype_batch_send)
                    outputStream.Write(0);							// Preference weight

                    outputStream.Write(IscCodes.PROTOCOL_VERSION11);//	Protocol version
                    outputStream.Write(1);							// Architecture	of client -	Generic
                    outputStream.Write(2);							// Minumum type (ptype_rpc)
                    outputStream.Write(5);							// Maximum type (ptype_lazy_send)
                    outputStream.Write(1);							// Preference weight

#if (PROTOCOL_VERSION12)
					outputStream.Write(IscCodes.PROTOCOL_VERSION12);//	Protocol version
					outputStream.Write(1);							// Architecture	of client -	Generic
					outputStream.Write(2);							// Minumum type (ptype_rpc)
					outputStream.Write(5);							// Maximum type (ptype_lazy_send)
					outputStream.Write(2);							// Preference weight
#endif
				}
                outputStream.Flush();

                if (inputStream.ReadOperation() == IscCodes.op_accept)
                {
                    this.protocolVersion = inputStream.ReadInt32();	// Protocol	version
                    this.protocolArchitecture = inputStream.ReadInt32();	// Architecture	for	protocol
                    this.protocolMinimunType = inputStream.ReadInt32();	// Minimum type

                    if (this.protocolVersion < 0)
                    {
                        this.protocolVersion = (ushort)(this.protocolVersion & IscCodes.FB_PROTOCOL_MASK) | IscCodes.FB_PROTOCOL_FLAG;
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

        public virtual void Disconnect()
        {
            // socket is owned by network stream, so it'll be closed automatically
            if (this.networkStream != null)
            {
                this.networkStream.Close();
            }

            this.socket = null;
            this.networkStream = null;
        }

        #endregion

        #region · Private Methods ·

        private IPAddress GetIPAddress(string dataSource, AddressFamily addressFamily)
        {
#if (!NET_CF)

            IPAddress ipaddress = null;

            if (IPAddress.TryParse(dataSource, out ipaddress))
            {
                return ipaddress;
            }

            IPAddress[] addresses = Dns.GetHostEntry(dataSource).AddressList;

            // Try to avoid problems with IPV6 addresses
            foreach (IPAddress address in addresses)
            {
                if (address.AddressFamily == addressFamily)
                {
                    return address;
                }
            }

            return addresses[0];
#else

            try
            {
                IPAddress[] addresses = Dns.GetHostEntry(dataSource).AddressList;

                // Try to avoid problems with IPV6 addresses
                foreach (IPAddress address in addresses)
                {
                    if (address.AddressFamily == addressFamily)
                    {
                        return address;
                    }
                }

                return addresses[0];
            }
            catch (Exception ex)
            {
                // If it's not possible to get the list of IP adress associated to 
                // the Data Source we try to check if Data Source is already an IP Address
                // and return it
                try
                {
                    return IPAddress.Parse(dataSource);
                }
                catch
                {
                    // In this case we want to rethrow the first exception
                    throw ex;
                }
            }

#endif
        }

        #endregion
    }
}
