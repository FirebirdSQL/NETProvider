/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using FirebirdSql.Data.Client.Managed.Version11;
using FirebirdSql.Data.Client.Managed.Version13;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed
{
	internal sealed class GdsConnection
	{
		const ulong KeepAliveTime = 1800000; // 30min
		const ulong KeepAliveInterval = 1800000; // 30min

		#region Fields

		private NetworkStream _networkStream;
		private string _userID;
		private string _password;
		private string _dataSource;
		private int _portNumber;
		private int _packetSize;
		private Charset _characterSet;
		private bool _compression;
		private int _protocolVersion;
		private int _protocolArchitecture;
		private int _protocolMinimunType;
		private byte[] _authData;

		private SrpClient _srp;
		private SspiHelper _sspi;

		#endregion

		#region Properties

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
			: this(null, null, dataSource, port, 8192, Charset.DefaultCharset, false)
		{
		}

		public GdsConnection(string userID, string password, string dataSource, int portNumber, int packetSize, Charset characterSet, bool compression)
		{
			_userID = userID;
			_password = password;
			_dataSource = dataSource;
			_portNumber = portNumber;
			_packetSize = packetSize;
			_characterSet = characterSet;
			_compression = compression;
		}

		#endregion

		#region Methods

		public void Connect()
		{
			try
			{
				IPAddress = GetIPAddress(_dataSource, AddressFamily.InterNetwork);
				var endPoint = new IPEndPoint(IPAddress, _portNumber);

				var socket = new Socket(IPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, _packetSize);
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, _packetSize);
				socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);
				socket.TrySetKeepAlive(KeepAliveTime, KeepAliveInterval);
				socket.TryEnableLoopbackFastPath();
				socket.Connect(endPoint);

				_networkStream = new NetworkStream(socket, true);
			}
			catch (SocketException ex)
			{
				throw IscException.ForTypeErrorCodeStrParam(IscCodes.isc_arg_gds, IscCodes.isc_network_error, _dataSource, ex);
			}
		}

		public void Identify(string database)
		{
			using (var xdrStream = CreateXdrStreamImpl(false))
			{
				try
				{
					xdrStream.Write(IscCodes.op_connect);
					xdrStream.Write(IscCodes.op_attach);
					xdrStream.Write(IscCodes.CONNECT_VERSION3);
					xdrStream.Write(IscCodes.GenericAchitectureClient);

					xdrStream.Write(database);

					var protocols = ProtocolsSupported.Get(_compression);
					xdrStream.Write(protocols.Count());
					xdrStream.WriteBuffer(UserIdentificationData());

					var priority = 0;
					foreach (var protocol in protocols)
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

						if (_compression && !((_protocolMinimunType & IscCodes.pflag_compress) != 0))
						{
							_compression = false;
						}

						if (operation == IscCodes.op_cond_accept || operation == IscCodes.op_accept_data)
						{
							var data = xdrStream.ReadBuffer();
							var acceptPluginName = xdrStream.ReadString();
							var isAuthenticated = xdrStream.ReadBoolean();
							var keys = xdrStream.ReadString();
							if (!isAuthenticated)
							{
								switch (acceptPluginName)
								{
									case SrpClient.PluginName:
										_authData = Encoding.ASCII.GetBytes(_srp.ClientProof(_userID, _password, data).ToHexString());
										break;
									case SspiHelper.PluginName:
										_authData = _sspi.GetClientSecurity(data);
										break;
									default:
										throw new ArgumentOutOfRangeException(nameof(acceptPluginName), $"{nameof(acceptPluginName)}={acceptPluginName}");
								}
							}
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
				}
				catch (IOException ex)
				{
					throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
				}
				finally
				{
					// UserIdentificationData might allocate these
					_srp = null;
					_sspi?.Dispose();
					_sspi = null;
				}
			}
		}

		public XdrStream CreateXdrStream()
		{
			return CreateXdrStreamImpl(_compression);
		}

		public void Disconnect()
		{
			_networkStream?.Dispose();
			_networkStream = null;
		}

		#endregion

		#region Private Methods

		private IPAddress GetIPAddress(string dataSource, AddressFamily addressFamily)
		{
			if (IPAddress.TryParse(dataSource, out var ipaddress))
			{
				return ipaddress;
			}

#if NETSTANDARD1_6
			var addresses = Dns.GetHostEntryAsync(dataSource).GetAwaiter().GetResult().AddressList;
#else
			var addresses = Dns.GetHostEntry(dataSource).AddressList;
#endif

			// try to avoid problems with IPv6 addresses
			foreach (var address in addresses)
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
				var userString = Environment.GetEnvironmentVariable("USERNAME") ?? Environment.GetEnvironmentVariable("USER") ?? string.Empty;
				var user = Encoding.UTF8.GetBytes(userString);
				result.WriteByte(IscCodes.CNCT_user);
				result.WriteByte((byte)user.Length);
				result.Write(user, 0, user.Length);

				var host = Encoding.UTF8.GetBytes(Dns.GetHostName());
				result.WriteByte(IscCodes.CNCT_host);
				result.WriteByte((byte)host.Length);
				result.Write(host, 0, host.Length);

				result.WriteByte(IscCodes.CNCT_user_verification);
				result.WriteByte(0);

				if (!string.IsNullOrEmpty(_userID))
				{
					_srp = new SrpClient();

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

					var specificData = Encoding.ASCII.GetBytes(_srp.PublicKeyHex);
					WriteMultiPartHelper(result, IscCodes.CNCT_specific_data, specificData);
				}
				else
				{
					_sspi = new SspiHelper();

					var pluginName = Encoding.ASCII.GetBytes(SspiHelper.PluginName);
					result.WriteByte(IscCodes.CNCT_plugin_name);
					result.WriteByte((byte)pluginName.Length);
					result.Write(pluginName, 0, pluginName.Length);
					result.WriteByte(IscCodes.CNCT_plugin_list);
					result.WriteByte((byte)pluginName.Length);
					result.Write(pluginName, 0, pluginName.Length);

					var specificData = _sspi.InitializeClientSecurity();
					WriteMultiPartHelper(result, IscCodes.CNCT_specific_data, specificData);
				}

				result.WriteByte(IscCodes.CNCT_client_crypt);
				result.WriteByte(4);
				result.Write(new byte[] { 0, 0, 0, 0 }, 0, 4);

				return result.ToArray();
			}
		}

		private XdrStream CreateXdrStreamImpl(bool compression)
		{
			return new XdrStream(_networkStream, _characterSet, compression, false);
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

				case IscCodes.op_crypt_key_callback:
					return new CryptKeyCallbackResponse(xdr.ReadBuffer());

				default:
					throw new ArgumentOutOfRangeException(nameof(operation), $"{nameof(operation)}={operation}");
			}
		}

		private static void WriteMultiPartHelper(Stream stream, byte code, byte[] data)
		{
			const int MaxLength = 255 - 1;
			var part = 0;
			for (var i = 0; i < data.Length; i += MaxLength)
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
