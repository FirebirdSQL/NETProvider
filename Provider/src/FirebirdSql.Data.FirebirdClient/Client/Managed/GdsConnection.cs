/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
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
		private NetworkStream _networkStream;
		private FirebirdNetworkHandlingWrapper _firebirdNetworkHandlingWrapper;

		public string User { get; private set; }
		public string Password { get; private set; }
		public string DataSource { get; private set; }
		public int PortNumber { get; private set; }
		public int Timeout { get; private set; }
		public int PacketSize { get; private set; }
		public Charset Charset { get; private set; }
		public bool Compression { get; private set; }
		public WireCryptOption WireCrypt { get; private set; }

		public int ProtocolVersion { get; private set; }
		public int ProtocolArchitecture { get; private set; }
		public int ProtocolMinimunType { get; private set; }
		public bool ConnectionBroken => _firebirdNetworkHandlingWrapper?.IOFailed ?? false;

		internal IPAddress IPAddress { get; private set; }
		internal XdrReaderWriter Xdr { get; private set; }

		internal AuthBlock AuthBlock { get; private set; }

		public GdsConnection(string dataSource, int port, int timeout)
			: this(null, null, dataSource, port, timeout, 8192, Charset.DefaultCharset, false, WireCryptOption.Enabled)
		{ }

		public GdsConnection(string user, string password, string dataSource, int portNumber, int timeout, int packetSize, Charset charset, bool compression, WireCryptOption wireCrypt)
		{
			User = user;
			Password = password;
			DataSource = dataSource;
			PortNumber = portNumber;
			Timeout = timeout;
			PacketSize = packetSize;
			Charset = charset;
			Compression = compression;
			WireCrypt = wireCrypt;
		}

		public void Connect()
		{
			try
			{
				IPAddress = GetIPAddress(DataSource);
				var endPoint = new IPEndPoint(IPAddress, PortNumber);

				var socket = new Socket(IPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, PacketSize);
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, PacketSize);
				socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);

				socket.Connect(endPoint);

				_networkStream = new NetworkStream(socket, true);
				_firebirdNetworkHandlingWrapper = new FirebirdNetworkHandlingWrapper(new DataProviderStreamWrapper(_networkStream));
				Xdr = new XdrReaderWriter(_firebirdNetworkHandlingWrapper, Charset);
			}
			catch (SocketException ex)
			{
				throw IscException.ForTypeErrorCodeStrParam(IscCodes.isc_arg_gds, IscCodes.isc_network_error, DataSource, ex);
			}
		}
		public async ValueTask ConnectAsync(CancellationToken cancellationToken = default)
		{
			try
			{
				IPAddress = await GetIPAddressAsync(DataSource, cancellationToken).ConfigureAwait(false);
				var endPoint = new IPEndPoint(IPAddress, PortNumber);

				var socket = new Socket(IPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, PacketSize);
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, PacketSize);
				socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);

				using (var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(Timeout)))
				{
					using (var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken))
					{
#if NET48 || NETSTANDARD2_0 || NETSTANDARD2_1
						static Func<IPEndPoint, CancellationToken, Task> ConnectAsyncHelper(Socket socket) => (e, ct) => Task.Factory.FromAsync(socket.BeginConnect, socket.EndConnect, e, null);
#else
						static Func<IPEndPoint, CancellationToken, Task> ConnectAsyncHelper(Socket socket) => (e, ct) => SocketTaskExtensions.ConnectAsync(socket, e, ct).AsTask();
#endif
						await ConnectAsyncHelper(socket)(endPoint, combinedCts.Token).ConfigureAwait(false);
					}
				}

				_networkStream = new NetworkStream(socket, true);
				_firebirdNetworkHandlingWrapper = new FirebirdNetworkHandlingWrapper(new DataProviderStreamWrapper(_networkStream));
				Xdr = new XdrReaderWriter(_firebirdNetworkHandlingWrapper, Charset);
			}
			catch (SocketException ex)
			{
				throw IscException.ForTypeErrorCodeStrParam(IscCodes.isc_arg_gds, IscCodes.isc_network_error, DataSource, ex);
			}
		}

		public void Identify(string database)
		{
			try
			{
				Xdr.Write(IscCodes.op_connect);
				Xdr.Write(IscCodes.op_attach);
				Xdr.Write(IscCodes.CONNECT_VERSION3);
				Xdr.Write(IscCodes.GenericAchitectureClient);

				Xdr.Write(database);

				var protocols = ProtocolsSupported.Get(Compression);
				Xdr.Write(protocols.Count());

				AuthBlock = new AuthBlock(User, Password, WireCrypt);

				Xdr.WriteBuffer(AuthBlock.UserIdentificationData());

				var priority = 0;
				foreach (var protocol in protocols)
				{
					Xdr.Write(protocol.Version);
					Xdr.Write(IscCodes.GenericAchitectureClient);
					Xdr.Write(protocol.MinPType);
					Xdr.Write(protocol.MaxPType);
					Xdr.Write(priority);

					priority++;
				}

				Xdr.Flush();

				var operation = Xdr.ReadOperation();
				if (operation == IscCodes.op_accept || operation == IscCodes.op_cond_accept || operation == IscCodes.op_accept_data)
				{
					ProtocolVersion = Xdr.ReadInt32();
					ProtocolArchitecture = Xdr.ReadInt32();
					ProtocolMinimunType = Xdr.ReadInt32();

					if (ProtocolVersion < 0)
					{
						ProtocolVersion = (ushort)(ProtocolVersion & IscCodes.FB_PROTOCOL_MASK) | IscCodes.FB_PROTOCOL_FLAG;
					}

					if (Compression && !((ProtocolMinimunType & IscCodes.pflag_compress) != 0))
					{
						Compression = false;
					}

					if (operation == IscCodes.op_cond_accept || operation == IscCodes.op_accept_data)
					{
						AuthBlock.Start(
							Xdr.ReadBuffer(),
							Xdr.ReadString(),
							Xdr.ReadBoolean(),
							Xdr.ReadBuffer());

						if (Compression)
						{
							// after reading before writing
							StartCompression();
						}

						if (operation == IscCodes.op_cond_accept)
						{
							while (true)
							{
								AuthBlock.SendContAuthToBuffer(Xdr);
								Xdr.Flush();
								var response = AuthBlock.ProcessContAuthResponse(Xdr);
								if (response is ContAuthResponse contAuthResponse)
								{
									AuthBlock.Start(contAuthResponse.ServerData, contAuthResponse.AcceptPluginName, contAuthResponse.IsAuthenticated, contAuthResponse.ServerKeys);
									continue;
								}
								break;
							}

							if (AuthBlock.ServerKeys.Any())
							{
								AuthBlock.SendWireCryptToBuffer(Xdr);
								Xdr.Flush();
								AuthBlock.ProcessWireCryptResponse(Xdr, this);
							}
						}
					}

					AuthBlock.WireCryptValidate(ProtocolVersion);
				}
				else if (operation == IscCodes.op_response)
				{
					var response = (GenericResponse)ProcessOperation(operation, Xdr);
					HandleResponseException(response);
				}
				else
				{
					throw IscException.ForErrorCode(IscCodes.isc_connect_reject);
				}
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}
		public async ValueTask IdentifyAsync(string database, CancellationToken cancellationToken = default)
		{
			try
			{
				await Xdr.WriteAsync(IscCodes.op_connect, cancellationToken).ConfigureAwait(false);
				await Xdr.WriteAsync(IscCodes.op_attach, cancellationToken).ConfigureAwait(false);
				await Xdr.WriteAsync(IscCodes.CONNECT_VERSION3, cancellationToken).ConfigureAwait(false);
				await Xdr.WriteAsync(IscCodes.GenericAchitectureClient, cancellationToken).ConfigureAwait(false);

				await Xdr.WriteAsync(database, cancellationToken).ConfigureAwait(false);

				var protocols = ProtocolsSupported.Get(Compression);
				await Xdr.WriteAsync(protocols.Count(), cancellationToken).ConfigureAwait(false);

				AuthBlock = new AuthBlock(User, Password, WireCrypt);

				await Xdr.WriteBufferAsync(AuthBlock.UserIdentificationData(), cancellationToken).ConfigureAwait(false);

				var priority = 0;
				foreach (var protocol in protocols)
				{
					await Xdr.WriteAsync(protocol.Version, cancellationToken).ConfigureAwait(false);
					await Xdr.WriteAsync(IscCodes.GenericAchitectureClient, cancellationToken).ConfigureAwait(false);
					await Xdr.WriteAsync(protocol.MinPType, cancellationToken).ConfigureAwait(false);
					await Xdr.WriteAsync(protocol.MaxPType, cancellationToken).ConfigureAwait(false);
					await Xdr.WriteAsync(priority, cancellationToken).ConfigureAwait(false);

					priority++;
				}

				await Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

				var operation = await Xdr.ReadOperationAsync(cancellationToken).ConfigureAwait(false);
				if (operation == IscCodes.op_accept || operation == IscCodes.op_cond_accept || operation == IscCodes.op_accept_data)
				{
					ProtocolVersion = await Xdr.ReadInt32Async(cancellationToken).ConfigureAwait(false);
					ProtocolArchitecture = await Xdr.ReadInt32Async(cancellationToken).ConfigureAwait(false);
					ProtocolMinimunType = await Xdr.ReadInt32Async(cancellationToken).ConfigureAwait(false);

					if (ProtocolVersion < 0)
					{
						ProtocolVersion = (ushort)(ProtocolVersion & IscCodes.FB_PROTOCOL_MASK) | IscCodes.FB_PROTOCOL_FLAG;
					}

					if (Compression && !((ProtocolMinimunType & IscCodes.pflag_compress) != 0))
					{
						Compression = false;
					}

					if (operation == IscCodes.op_cond_accept || operation == IscCodes.op_accept_data)
					{
						AuthBlock.Start(
							await Xdr.ReadBufferAsync(cancellationToken).ConfigureAwait(false),
							await Xdr.ReadStringAsync(cancellationToken).ConfigureAwait(false),
							await Xdr.ReadBooleanAsync(cancellationToken).ConfigureAwait(false),
							await Xdr.ReadBufferAsync(cancellationToken).ConfigureAwait(false));

						if (Compression)
						{
							// after reading before writing
							StartCompression();
						}

						if (operation == IscCodes.op_cond_accept)
						{
							while (true)
							{
								await AuthBlock.SendContAuthToBufferAsync(Xdr, cancellationToken).ConfigureAwait(false);
								await Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
								var response = await AuthBlock.ProcessContAuthResponseAsync(Xdr, cancellationToken).ConfigureAwait(false);
								if (response is ContAuthResponse contAuthResponse)
								{
									AuthBlock.Start(contAuthResponse.ServerData, contAuthResponse.AcceptPluginName, contAuthResponse.IsAuthenticated, contAuthResponse.ServerKeys);
									continue;
								}
								break;
							}

							if (AuthBlock.ServerKeys.Any())
							{
								await AuthBlock.SendWireCryptToBufferAsync(Xdr, cancellationToken).ConfigureAwait(false);
								await Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
								await AuthBlock.ProcessWireCryptResponseAsync(Xdr, this, cancellationToken).ConfigureAwait(false);
							}
						}
					}

					AuthBlock.WireCryptValidate(ProtocolVersion);
				}
				else if (operation == IscCodes.op_response)
				{
					var response = (GenericResponse)await ProcessOperationAsync(operation, Xdr, cancellationToken).ConfigureAwait(false);
					HandleResponseException(response);
				}
				else
				{
					throw IscException.ForErrorCode(IscCodes.isc_connect_reject);
				}
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}

		public void Disconnect()
		{
			if (_networkStream != null)
			{
#if NET48 || NETSTANDARD2_0
				_networkStream.Dispose();
#else
				_networkStream.Dispose();
#endif
				_networkStream = null;
			}
		}
		public async ValueTask DisconnectAsync(CancellationToken cancellationToken = default)
		{
			if (_networkStream != null)
			{
#if NET48 || NETSTANDARD2_0
				_networkStream.Dispose();
				await ValueTask2.CompletedTask.ConfigureAwait(false);
#else
				await _networkStream.DisposeAsync().ConfigureAwait(false);
#endif
				_networkStream = null;
			}
		}

		internal void StartCompression()
		{
			_firebirdNetworkHandlingWrapper.StartCompression();
		}

		internal void StartEncryption()
		{
			_firebirdNetworkHandlingWrapper.StartEncryption(AuthBlock.SessionKey);
		}

		private static IPAddress GetIPAddress(string dataSource)
		{
			if (IPAddress.TryParse(dataSource, out var ipaddress))
			{
				return ipaddress;
			}

			var addresses = (Dns.GetHostEntry(dataSource)).AddressList;
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
		private static async ValueTask<IPAddress> GetIPAddressAsync(string dataSource, CancellationToken cancellationToken = default)
		{
			if (IPAddress.TryParse(dataSource, out var ipaddress))
			{
				return ipaddress;
			}

			var addresses = (await Dns.GetHostEntryAsync(dataSource).ConfigureAwait(false)).AddressList;
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

		public static IResponse ProcessOperation(int operation, IXdrReader xdr)
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
					return new FetchResponse(
						xdr.ReadInt32(),
						xdr.ReadInt32());

				case IscCodes.op_sql_response:
					return new SqlResponse(
						xdr.ReadInt32());

				case IscCodes.op_trusted_auth:
					return new AuthResponse(
						xdr.ReadBuffer());

				case IscCodes.op_crypt_key_callback:
					return new CryptKeyCallbackResponse(
						xdr.ReadBuffer());

				case IscCodes.op_cont_auth:
					return new ContAuthResponse(
						xdr.ReadBuffer(),
						xdr.ReadString(),
						xdr.ReadBoolean(),
						xdr.ReadBuffer());

				default:
					throw new ArgumentOutOfRangeException(nameof(operation), $"{nameof(operation)}={operation}");
			}
		}
		public static async ValueTask<IResponse> ProcessOperationAsync(int operation, IXdrReader xdr, CancellationToken cancellationToken = default)
		{
			switch (operation)
			{
				case IscCodes.op_response:
					return new GenericResponse(
						await xdr.ReadInt32Async(cancellationToken).ConfigureAwait(false),
						await xdr.ReadInt64Async(cancellationToken).ConfigureAwait(false),
						await xdr.ReadBufferAsync(cancellationToken).ConfigureAwait(false),
						await xdr.ReadStatusVectorAsync(cancellationToken).ConfigureAwait(false));

				case IscCodes.op_fetch_response:
					return new FetchResponse(
						await xdr.ReadInt32Async(cancellationToken).ConfigureAwait(false),
						await xdr.ReadInt32Async(cancellationToken).ConfigureAwait(false));

				case IscCodes.op_sql_response:
					return new SqlResponse(
						await xdr.ReadInt32Async(cancellationToken).ConfigureAwait(false));

				case IscCodes.op_trusted_auth:
					return new AuthResponse(
						await xdr.ReadBufferAsync(cancellationToken).ConfigureAwait(false));

				case IscCodes.op_crypt_key_callback:
					return new CryptKeyCallbackResponse(
						await xdr.ReadBufferAsync(cancellationToken).ConfigureAwait(false));

				case IscCodes.op_cont_auth:
					return new ContAuthResponse(
						await xdr.ReadBufferAsync(cancellationToken).ConfigureAwait(false),
						await xdr.ReadStringAsync(cancellationToken).ConfigureAwait(false),
						await xdr.ReadBooleanAsync(cancellationToken).ConfigureAwait(false),
						await xdr.ReadBufferAsync(cancellationToken).ConfigureAwait(false));

				default:
					throw new ArgumentOutOfRangeException(nameof(operation), $"{nameof(operation)}={operation}");
			}
		}

		public static void HandleResponseException(IResponse response)
		{
			if (response is GenericResponse genericResponse)
			{
				if (genericResponse.Exception != null && !genericResponse.Exception.IsWarning)
				{
					throw genericResponse.Exception;
				}
			}
		}

		public static void HandleResponseWarning(IResponse response, Action<IscException> onWarning)
		{
			if (response is GenericResponse genericResponse)
			{
				if (genericResponse.Exception != null && genericResponse.Exception.IsWarning)
				{
					onWarning?.Invoke(genericResponse.Exception);
				}
			}
		}
	}
}
