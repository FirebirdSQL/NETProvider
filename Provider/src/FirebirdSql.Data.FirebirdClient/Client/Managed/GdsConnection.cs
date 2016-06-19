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
using System.Text;
using FirebirdSql.Data.Client.Managed.Version11;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed
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
		private SrpClient _srpClient;
		private byte[] _authData;

		#endregion

		#region Properties

		public bool IsConnected
		{
			get { return _socket?.Connected ?? false; }
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
			_srpClient = new SrpClient();
		}

		#endregion

		#region Methods

		public virtual void Connect()
		{
			try
			{
				IPAddress = GetIPAddress(_dataSource, AddressFamily.InterNetwork);
				var endPoint = new IPEndPoint(IPAddress, _portNumber);

				_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, _packetSize);
				_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, _packetSize);
				_socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);
				_socket.SetKeepAlive(KeepAliveTime, KeepAliveInterval);

				_socket.Connect(endPoint);
				_networkStream = new NetworkStream(_socket, true);
			}
			catch (SocketException ex)
			{
				throw IscException.ForTypeErrorCodeStrParam(IscCodes.isc_arg_gds, IscCodes.isc_network_error, _dataSource, ex);
			}
		}

		public virtual void Identify(string database)
		{
			using (var xdrStream = CreateXdrStream(false))
			{
				try
				{
					xdrStream.Write(IscCodes.op_connect);
					xdrStream.Write(IscCodes.op_attach);
					xdrStream.Write(IscCodes.CONNECT_VERSION3);
					xdrStream.Write(IscCodes.GenericAchitectureClient);

					xdrStream.Write(database);
					xdrStream.Write(ProtocolsSupported.Protocols.Count);
					xdrStream.WriteBuffer(UserIdentificationData());

					var priority = 0;
					foreach (var protocol in ProtocolsSupported.Protocols)
					{
						xdrStream.Write(protocol.Version);
						xdrStream.Write(IscCodes.GenericAchitectureClient);
						xdrStream.Write(protocol.MinPType);
						xdrStream.Write(protocol.MaxPType);
						xdrStream.Write(priority);

						priority++;
					}

					xdrStream.Flush();

					var operation = xdrStream.ReadOperation();
					if (operation == IscCodes.op_accept || operation == IscCodes.op_cond_accept || operation == IscCodes.op_accept_data)
					{
						_protocolVersion = xdrStream.ReadInt32();
						_protocolArchitecture = xdrStream.ReadInt32();
						_protocolMinimunType = xdrStream.ReadInt32();

						if (_protocolVersion < 0)
						{
							_protocolVersion = (ushort)(_protocolVersion & IscCodes.FB_PROTOCOL_MASK) | IscCodes.FB_PROTOCOL_FLAG;
						}
					}
					else if (operation == IscCodes.op_response)
					{
						var response = (GenericResponse)ProcessOperation(operation, xdrStream);
						throw response.Exception;
					}
					else
					{
						try
						{
							Disconnect();
						}
						catch
						{ }
						finally
						{
							throw IscException.ForErrorCode(IscCodes.isc_connect_reject);
						}
					}

					if (operation == IscCodes.op_cond_accept || operation == IscCodes.op_accept_data)
					{
						var data = xdrStream.ReadBuffer();
						var acceptPluginName = xdrStream.ReadString();
						var isAuthenticated = xdrStream.ReadInt32();
						var keys = xdrStream.ReadString();
						if (isAuthenticated == 0)
						{
							_authData = _srpClient.ClientProof(_userID, _password, data);
						}
					}
				}
				catch (IOException ex)
				{
					throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
				}
			}
		}

		public XdrStream CreateXdrStream(bool compress)
		{
			return new XdrStream(new BufferedStream(_networkStream, 32 * 1024), _characterSet, compress, false);
		}

		public virtual void Disconnect()
		{
			// socket is owned by network stream, so it'll be closed automatically
			_networkStream?.Close();
			_networkStream = null;
			_socket = null;
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

		private byte[] UserIdentificationData()
		{
			using (var result = new MemoryStream())
			{
				if (_userID != null)
				{
					var login = Encoding.UTF8.GetBytes(_userID);
					result.WriteByte(IscCodes.CNCT_login);
					result.WriteByte((byte)login.Length);
					result.Write(login, 0, login.Length);

					var pluginName = Encoding.ASCII.GetBytes(SrpClient.PluginName);
					result.WriteByte(IscCodes.CNCT_plugin_name);
					result.WriteByte((byte)pluginName.Length);
					result.Write(pluginName, 0, pluginName.Length);
					result.WriteByte(IscCodes.CNCT_plugin_list);
					result.WriteByte((byte)pluginName.Length);
					result.Write(pluginName, 0, pluginName.Length);

					var specificData = Encoding.ASCII.GetBytes(_srpClient.PublicKeyHex);
					WriteMultiPartHelper(result, IscCodes.CNCT_specific_data, specificData);

					result.WriteByte(IscCodes.CNCT_client_crypt);
					result.WriteByte(4);
					result.Write(new byte[] { 0, 0, 0, 0 }, 0, 4);
				}

				var user = Encoding.UTF8.GetBytes(Environment.UserName);
				result.WriteByte(IscCodes.CNCT_user);
				result.WriteByte((byte)user.Length);
				result.Write(user, 0, user.Length);

				var host = Encoding.UTF8.GetBytes(Dns.GetHostName());
				result.WriteByte(IscCodes.CNCT_host);
				result.WriteByte((byte)host.Length);
				result.Write(host, 0, host.Length);

				result.WriteByte(IscCodes.CNCT_user_verification);
				result.WriteByte(0);

				return result.ToArray();
			}
		}

		#endregion

		#region Static Methods

		public static IResponse ProcessOperation(int operation, XdrStream xdr)
		{
			switch (operation)
			{
				case IscCodes.op_response:
					return new GenericResponse(
						xdr.ReadInt32(),
						xdr.ReadInt64(),
						xdr.ReadBuffer(),
						xdr.ReadStatusVector());

				case IscCodes.op_fetch_response:
					return new FetchResponse(xdr.ReadInt32(), xdr.ReadInt32());

				case IscCodes.op_sql_response:
					return new SqlResponse(xdr.ReadInt32());

				case IscCodes.op_trusted_auth:
					return new AuthResponse(xdr.ReadBuffer());

				default:
					return null;
			}
		}

		private static void WriteMultiPartHelper(Stream stream, byte code, byte[] data)
		{
			const int MaxLength = 255 - 1;
			var part = 0;
			for (int i = 0; i < data.Length; i += MaxLength)
			{
				stream.WriteByte(code);
				var length = Math.Min(data.Length - i, MaxLength);
				stream.WriteByte((byte)(length + 1));
				stream.WriteByte((byte)part);
				stream.Write(data, i, length);
				part++;
			}
		}

		#endregion
	}
}
