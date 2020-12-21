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
using System.Threading;
using System.Threading.Tasks;
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

		private FirebirdNetworkStream  _firebirdNetworkStream;
		private string _userID;
		private string _dataSource;
		private int _portNumber;
		private int _packetSize;
		private Charset _charset;
		private bool _compression;
		private WireCryptOption _wireCrypt;

		#endregion

		#region Properties

		public int ProtocolVersion { get; private set; }
		public int ProtocolArchitecture { get; private set; }
		public int ProtocolMinimunType { get; private set; }
		public string Password { get; private set; }
		public byte[] AuthData { get; private set; }
		public bool ConnectionBroken => _firebirdNetworkStream?.IOFailed ?? false;

		internal IPAddress IPAddress { get; private set; }
		internal XdrReaderWriter Xdr { get; private set; }

		#endregion

		#region Constructors

		public GdsConnection(string dataSource, int port)
			: this(null, null, dataSource, port, 8192, Charset.DefaultCharset, false, WireCryptOption.Enabled)
		{ }

		public GdsConnection(string userID, string password, string dataSource, int portNumber, int packetSize, Charset charset, bool compression, WireCryptOption wireCrypt)
		{
			_userID = userID;
			Password = password;
			_dataSource = dataSource;
			_portNumber = portNumber;
			_packetSize = packetSize;
			_charset = charset;
			_compression = compression;
			_wireCrypt = wireCrypt;
		}

		#endregion

		#region Methods

		public async Task Connect(AsyncWrappingCommonArgs async)
		{
			try
			{
				IPAddress = await GetIPAddress(_dataSource, async).ConfigureAwait(false);
				var endPoint = new IPEndPoint(IPAddress, _portNumber);

				var socket = new Socket(IPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, _packetSize);
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, _packetSize);
				socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);
				socket.TrySetKeepAlive(KeepAliveTime, KeepAliveInterval);
				socket.TryEnableLoopbackFastPath();
#if NET48 || NETSTANDARD2_0 || NETSTANDARD2_1
				static Func<IPEndPoint, CancellationToken, Task> ConnectHelper(Socket socket) => (e, ct) => Task.Factory.FromAsync(socket.BeginConnect, socket.EndConnect, e, null);
#else
				static Func<IPEndPoint, CancellationToken, Task> ConnectHelper(Socket socket) => (e, ct) => SocketTaskExtensions.ConnectAsync(socket, e, ct).AsTask();
#endif
				await async.AsyncSyncCall(ConnectHelper(socket), socket.Connect, endPoint).ConfigureAwait(false);

				_firebirdNetworkStream = new FirebirdNetworkStream(new NetworkStream(socket, true));
				Xdr = new XdrReaderWriter(_firebirdNetworkStream, _charset);
			}
			catch (SocketException ex)
			{
				throw IscException.ForTypeErrorCodeStrParam(IscCodes.isc_arg_gds, IscCodes.isc_network_error, _dataSource, ex);
			}
		}

		public async Task Identify(string database, AsyncWrappingCommonArgs async)
		{
			try
			{
				await Xdr.Write(IscCodes.op_connect, async).ConfigureAwait(false);
				await Xdr.Write(IscCodes.op_attach, async).ConfigureAwait(false);
				await Xdr.Write(IscCodes.CONNECT_VERSION3, async).ConfigureAwait(false);
				await Xdr.Write(IscCodes.GenericAchitectureClient, async).ConfigureAwait(false);

				await Xdr.Write(database, async).ConfigureAwait(false);

				var protocols = ProtocolsSupported.Get(_compression);
				await Xdr.Write(protocols.Count(), async).ConfigureAwait(false);

				var (userIdentificationData, srp, sspi) = UserIdentificationData();
				using (sspi)
				{
					await Xdr.WriteBuffer(userIdentificationData, async).ConfigureAwait(false);

					var priority = 0;
					foreach (var protocol in protocols)
					{
						await Xdr.Write(protocol.Version, async).ConfigureAwait(false);
						await Xdr.Write(IscCodes.GenericAchitectureClient, async).ConfigureAwait(false);
						await Xdr.Write(protocol.MinPType, async).ConfigureAwait(false);
						await Xdr.Write(protocol.MaxPType, async).ConfigureAwait(false);
						await Xdr.Write(priority, async).ConfigureAwait(false);

						priority++;
					}

					await Xdr.Flush(async).ConfigureAwait(false);

					var operation = await Xdr.ReadOperation(async).ConfigureAwait(false);
					if (operation == IscCodes.op_accept || operation == IscCodes.op_cond_accept || operation == IscCodes.op_accept_data)
					{
						var wireCryptInitialized = false;

						ProtocolVersion = await Xdr.ReadInt32(async).ConfigureAwait(false);
						ProtocolArchitecture = await Xdr.ReadInt32(async).ConfigureAwait(false);
						ProtocolMinimunType = await Xdr.ReadInt32(async).ConfigureAwait(false);

						if (ProtocolVersion < 0)
						{
							ProtocolVersion = (ushort)(ProtocolVersion & IscCodes.FB_PROTOCOL_MASK) | IscCodes.FB_PROTOCOL_FLAG;
						}

						if (_compression && !((ProtocolMinimunType & IscCodes.pflag_compress) != 0))
						{
							_compression = false;
						}

						if (operation == IscCodes.op_cond_accept || operation == IscCodes.op_accept_data)
						{
							var serverData = await Xdr.ReadBuffer(async).ConfigureAwait(false);
							var acceptPluginName = await Xdr.ReadString(async).ConfigureAwait(false);
							var isAuthenticated = await Xdr.ReadBoolean(async).ConfigureAwait(false);
							var serverKeys = await Xdr.ReadBuffer(async).ConfigureAwait(false);
							if (!isAuthenticated)
							{
								switch (acceptPluginName)
								{
									case SrpClient.PluginName:
										AuthData = Encoding.ASCII.GetBytes(srp.ClientProof(NormalizeLogin(_userID), Password, serverData).ToHexString());
										break;
									case SspiHelper.PluginName:
										AuthData = sspi.GetClientSecurity(serverData);
										break;
									default:
										throw new ArgumentOutOfRangeException(nameof(acceptPluginName), $"{nameof(acceptPluginName)}={acceptPluginName}");
								}
							}

							if (_compression)
							{
								// after reading before writing
								_firebirdNetworkStream.StartCompression();
							}

							if (operation == IscCodes.op_cond_accept)
							{
								await Xdr.Write(IscCodes.op_cont_auth, async).ConfigureAwait(false);
								await Xdr.WriteBuffer(AuthData, async).ConfigureAwait(false);
								await Xdr.Write(acceptPluginName, async).ConfigureAwait(false); // like CNCT_plugin_name
								await Xdr.Write(acceptPluginName, async).ConfigureAwait(false); // like CNCT_plugin_list
								await Xdr.WriteBuffer(serverKeys, async).ConfigureAwait(false);
								await Xdr.Flush(async).ConfigureAwait(false);
								var response = (GenericResponse)await ProcessOperation(await Xdr.ReadOperation(async).ConfigureAwait(false), Xdr, async).ConfigureAwait(false);
								serverKeys = response.Data;
								isAuthenticated = true;

								if (_wireCrypt != WireCryptOption.Disabled)
								{
									await Xdr.Write(IscCodes.op_crypt, async).ConfigureAwait(false);
									await Xdr.Write(FirebirdNetworkStream.EncryptionName, async).ConfigureAwait(false);
									await Xdr.Write(SrpClient.SessionKeyName, async).ConfigureAwait(false);
									await Xdr.Flush(async).ConfigureAwait(false);

									// after writing before reading
									_firebirdNetworkStream.StartEncryption(srp.SessionKey);

									await ProcessOperation(await Xdr.ReadOperation(async).ConfigureAwait(false), Xdr, async).ConfigureAwait(false);

									wireCryptInitialized = true;
								}
							}
						}

						// fbclient does not care about wirecrypt in older protocols either
						if (ProtocolVersion == IscCodes.PROTOCOL_VERSION13 && _wireCrypt == WireCryptOption.Required && !wireCryptInitialized)
						{
							throw IscException.ForErrorCode(IscCodes.isc_wirecrypt_incompatible);
						}
					}
					else if (operation == IscCodes.op_response)
					{
						var response = (GenericResponse)await ProcessOperation(operation, Xdr, async).ConfigureAwait(false);
						throw response.Exception;
					}
					else
					{
						try
						{
							await Disconnect(async).ConfigureAwait(false);
						}
						catch
						{ }
						finally
						{
							throw IscException.ForErrorCode(IscCodes.isc_connect_reject);
						}
					}
				}
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		public async Task Disconnect(AsyncWrappingCommonArgs async)
		{
			if (_firebirdNetworkStream != null)
			{
#if NET48 || NETSTANDARD2_0
				_firebirdNetworkStream.Dispose();
				await Task.CompletedTask.ConfigureAwait(false);
#else
				await async.AsyncSyncCallNoCancellation(_firebirdNetworkStream.DisposeAsync, _firebirdNetworkStream.Dispose).ConfigureAwait(false);
#endif
				_firebirdNetworkStream = null;
			}
		}

#endregion

#region Private Methods

		private async Task<IPAddress> GetIPAddress(string dataSource, AsyncWrappingCommonArgs async)
		{
			if (IPAddress.TryParse(dataSource, out var ipaddress))
			{
				return ipaddress;
			}

			var addresses = (await async.AsyncSyncCallNoCancellation(Dns.GetHostEntryAsync, Dns.GetHostEntry, dataSource).ConfigureAwait(false)).AddressList;
			foreach (var address in addresses)
			{
				// IPv4 priority
				if (address.AddressFamily == AddressFamily.InterNetwork)
				{
					return address;
				}
			}
			return addresses[0];
		}

		private (byte[], SrpClient, SspiHelper) UserIdentificationData()
		{
			var srp = default(SrpClient);
			var sspi = default(SspiHelper);

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
					srp = new SrpClient();

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

					var specificData = Encoding.ASCII.GetBytes(srp.PublicKeyHex);
					WriteMultiPartHelper(result, IscCodes.CNCT_specific_data, specificData);

					result.WriteByte(IscCodes.CNCT_client_crypt);
					result.WriteByte(4);
					result.Write(TypeEncoder.EncodeInt32(WireCryptOptionValue(_wireCrypt)), 0, 4);
				}
				else
				{
					sspi = new SspiHelper();

					var pluginName = Encoding.ASCII.GetBytes(SspiHelper.PluginName);
					result.WriteByte(IscCodes.CNCT_plugin_name);
					result.WriteByte((byte)pluginName.Length);
					result.Write(pluginName, 0, pluginName.Length);
					result.WriteByte(IscCodes.CNCT_plugin_list);
					result.WriteByte((byte)pluginName.Length);
					result.Write(pluginName, 0, pluginName.Length);

					var specificData = sspi.InitializeClientSecurity();
					WriteMultiPartHelper(result, IscCodes.CNCT_specific_data, specificData);

					result.WriteByte(IscCodes.CNCT_client_crypt);
					result.WriteByte(4);
					result.Write(TypeEncoder.EncodeInt32(IscCodes.WIRE_CRYPT_DISABLED), 0, 4);
				}

				return (result.ToArray(), srp, sspi);
			}
		}

#endregion

#region Static Methods

		public static async Task<IResponse> ProcessOperation(int operation, IXdrReader xdr, AsyncWrappingCommonArgs async)
		{
			switch (operation)
			{
				case IscCodes.op_response:
					return new GenericResponse(
						await xdr.ReadInt32(async).ConfigureAwait(false),
						await xdr.ReadInt64(async).ConfigureAwait(false),
						await xdr.ReadBuffer(async).ConfigureAwait(false),
						await xdr.ReadStatusVector(async).ConfigureAwait(false));

				case IscCodes.op_fetch_response:
					return new FetchResponse(await xdr.ReadInt32(async).ConfigureAwait(false), await xdr.ReadInt32(async).ConfigureAwait(false));

				case IscCodes.op_sql_response:
					return new SqlResponse(await xdr.ReadInt32(async).ConfigureAwait(false));

				case IscCodes.op_trusted_auth:
					return new AuthResponse(await xdr.ReadBuffer(async).ConfigureAwait(false));

				case IscCodes.op_crypt_key_callback:
					return new CryptKeyCallbackResponse(await xdr.ReadBuffer(async).ConfigureAwait(false));

				default:
					throw new ArgumentOutOfRangeException(nameof(operation), $"{nameof(operation)}={operation}");
			}
		}

		public static string NormalizeLogin(string login)
		{
			if (string.IsNullOrEmpty(login))
			{
				return login;
			}
			if (login.Length > 2 && login[0] == '"' && login[login.Length - 1] == '"')
			{
				return NormalizeQuotedLogin(login);
			}
			return login.ToUpperInvariant();
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

		private static string NormalizeQuotedLogin(string login)
		{
			var sb = new StringBuilder(login, 1, login.Length - 2, login.Length - 2);
			for (int idx = 0; idx < sb.Length; idx++)
			{
				// Double double quotes ("") escape a double quote in a quoted string
				if (sb[idx] == '"')
				{
					// Strip double quote escape
					sb.Remove(idx, 1);
					if (idx < sb.Length && sb[idx] == '"')
					{
						// Retain escaped double quote
						idx += 1;
					}
					else
					{
						// The character after escape is not a double quote, we terminate the conversion and truncate.
						// Firebird does this as well (see common/utils.cpp#dpbItemUpper)
						sb.Length = idx;
						return sb.ToString();
					}
				}
			}
			return sb.ToString();
		}

		private static int WireCryptOptionValue(WireCryptOption wireCrypt)
		{
			switch (wireCrypt)
			{
				case WireCryptOption.Disabled:
					return IscCodes.WIRE_CRYPT_DISABLED;
				case WireCryptOption.Enabled:
					return IscCodes.WIRE_CRYPT_ENABLED;
				case WireCryptOption.Required:
					return IscCodes.WIRE_CRYPT_REQUIRED;
				default:
					throw new ArgumentOutOfRangeException(nameof(wireCrypt), $"{nameof(wireCrypt)}={wireCrypt}");
			}
		}

#endregion
	}
}
