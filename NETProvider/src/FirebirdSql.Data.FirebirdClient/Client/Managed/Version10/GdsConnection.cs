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
 *	Copyright (c) 2007 - 2012 Jiri Cincura (jiri@cincura.net)
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
		const ulong KeepAliveTime = 1800000; //30min
		const ulong KeepAliveInterval = 1800000; //30min

		#region Fields

		private Socket _socket;
		private NetworkStream _networkStream;
		private string _userID;
		private string _password;
		private string _dataSource;
		private int _portNumber;
		private int _packetSize;
		private Charset _characterSet;
		private int _protocolVersion;
		private int _protocolArchitecture;
		private int _protocolMinimunType;
		private SrpClient _srpClient = new SrpClient();
		private byte[] _authData;

		#endregion

		#region Properties

		public bool IsConnected
		{
			get { return (_socket != null && _socket.Connected); }
		}

		public int ProtocolVersion
		{
			get { return _protocolVersion; }
		}

		public int ProtocolArchitecture
		{
			get { return _protocolArchitecture; }
		}

		public int ProtocolMinimunType
		{
			get { return _protocolMinimunType; }
		}

		public bool DataAvailable
		{
			get { return _networkStream.DataAvailable; }
		}

		public string UserID
		{
			get { return _userID; }
		}

		public string Password
		{
			get { return _password; }
		}

		public byte[] AuthData
		{
			get { return _authData; }
		}

		internal IPAddress IPAddress { get; private set; }

		#endregion

		#region Constructors

		public GdsConnection(string dataSource, int port)
			: this(null, null, dataSource, port, 8192, Charset.DefaultCharset)
		{
		}

		public GdsConnection(string userID, string password, string dataSource, int portNumber, int packetSize, Charset characterSet)
		{
			_userID = userID;
			_password = password;
			_dataSource = dataSource;
			_portNumber = portNumber;
			_packetSize = packetSize;
			_characterSet = characterSet;

			GC.SuppressFinalize(this);
		}

		#endregion

		#region Methods

		public virtual void Connect()
		{
			try
			{
				IPAddress = GetIPAddress(_dataSource, AddressFamily.InterNetwork);
				IPEndPoint endPoint = new IPEndPoint(IPAddress, _portNumber);

				_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				// Set Receive Buffer size.
				_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, _packetSize);
				// Set Send	Buffer size.
				_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, _packetSize);
				// Disables	the	Nagle algorithm	for	send coalescing.
				_socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);
				// Start sending keepalive packets every 30min after 30min of idle connection
				_socket.SetKeepAlive(KeepAliveTime, KeepAliveInterval);

				// Make	the	socket to connect to the Server
				_socket.Connect(endPoint);
				_networkStream = new NetworkStream(_socket, true);

				GC.SuppressFinalize(_socket);
				GC.SuppressFinalize(_networkStream);
			}
			catch (SocketException)
			{
				throw new IscException(IscCodes.isc_arg_gds, IscCodes.isc_network_error, _dataSource);
			}
		}

		public virtual void Identify(string database)
		{
			// handles this.networkStream
			XdrStream inputStream = CreateXdrStream();
			XdrStream outputStream = CreateXdrStream();

			try
			{
				outputStream.Write(IscCodes.op_connect);
				outputStream.Write(IscCodes.op_attach);
				outputStream.Write(IscCodes.CONNECT_VERSION3);	// CONNECT_VERSION2
				outputStream.Write(1);							// Architecture	of client -	Generic

				outputStream.Write(database);					// Database	path
				outputStream.Write(4);							// Protocol	versions understood
				outputStream.WriteBuffer(UserIdentificationStuff());	// User	identification Stuff

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

				outputStream.Write(IscCodes.PROTOCOL_VERSION12);//	Protocol version
				outputStream.Write(1);							// Architecture	of client -	Generic
				outputStream.Write(2);							// Minumum type (ptype_rpc)
				outputStream.Write(5);							// Maximum type (ptype_lazy_send)
				outputStream.Write(2);							// Preference weight

				outputStream.Write(IscCodes.PROTOCOL_VERSION13);//	Protocol version
				outputStream.Write(1);							// Architecture	of client -	Generic
				outputStream.Write(2);							// Minumum type (ptype_rpc)
				outputStream.Write(5);							// Maximum type (ptype_lazy_send)
				outputStream.Write(3);							// Preference weight

				outputStream.Flush();

				int op_code = inputStream.ReadOperation();
				if (op_code == IscCodes.op_accept || op_code == IscCodes.op_cond_accept || op_code == IscCodes.op_accept_data)
				{
					_protocolVersion = inputStream.ReadInt32(); // Protocol	version
					_protocolArchitecture = inputStream.ReadInt32();    // Architecture	for	protocol
					_protocolMinimunType = inputStream.ReadInt32();	// Minimum type

					if (_protocolVersion < 0)
					{
						_protocolVersion = (ushort)(_protocolVersion & IscCodes.FB_PROTOCOL_MASK) | IscCodes.FB_PROTOCOL_FLAG;
					}

					if (op_code ==	IscCodes.op_cond_accept || op_code == IscCodes.op_accept_data)
					{
						byte[] data = inputStream.ReadBuffer();
						string acceptPluginName = inputStream.ReadString();
						int is_authenticated = inputStream.ReadInt32();
						string keys = inputStream.ReadString();
						if (is_authenticated == 0) {
							_authData = _srpClient.clientProof(_userID, _password, data);
						}
					}
				}
				else
				{
					try
					{
						Disconnect();
					}
					catch
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
			return new XdrStream(new BufferedStream(_networkStream), _characterSet);
		}

		public virtual void Disconnect()
		{
			// socket is owned by network stream, so it'll be closed automatically
			if (_networkStream != null)
			{
				_networkStream.Close();
			}

			_socket = null;
			_networkStream = null;
		}

		#endregion

		#region Private Methods

		private IPAddress GetIPAddress(string dataSource, AddressFamily addressFamily)
		{
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
		}

		private byte[] UserIdentificationStuff()
		{
			// Here	we identify	the	user to	the	engine.
			// This	may	or may not be used as login	info to	a database.
			var user = Encoding.Default.GetBytes(System.Environment.UserName);
			var host = Encoding.Default.GetBytes(System.Net.Dns.GetHostName());

			using (var user_id = new MemoryStream())
			{
				if (_userID != null) {
					var login = Encoding.Default.GetBytes(_userID);
					var plugin_name = Encoding.Default.GetBytes("Srp");
					// Login
					user_id.WriteByte(9);
					user_id.WriteByte((byte)login.Length);
					user_id.Write(login, 0, login.Length);

					// Plugin Name
					user_id.WriteByte(8);
					user_id.WriteByte((byte)plugin_name.Length);
					user_id.Write(plugin_name, 0, plugin_name.Length);

					// Plugin List
					user_id.WriteByte(10);
					user_id.WriteByte((byte)plugin_name.Length);
					user_id.Write(plugin_name, 0, plugin_name.Length);

					// Specific Data
					byte[] specific_data = Encoding.Default.GetBytes(_srpClient.getPublicKeyHex());
					int remaining = specific_data.Length;
					int position = 0;
					int step = 0;
					while (remaining > 0) {
						user_id.WriteByte(7);
						int toWrite = Math.Min(remaining, 254);
						user_id.WriteByte((byte)(toWrite + 1));
						user_id.WriteByte((byte)step++);
						user_id.Write(specific_data, position, toWrite);
						remaining -= toWrite;
						position += toWrite;
					}

					// Client Crypt (Not Encrypt)
					user_id.WriteByte(11);
					user_id.WriteByte(4);
					user_id.WriteByte(0);
					user_id.WriteByte(0);
					user_id.WriteByte(0);
					user_id.WriteByte(0);
				}

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

				return user_id.ToArray();
			}
		}

		#endregion
	}
}
