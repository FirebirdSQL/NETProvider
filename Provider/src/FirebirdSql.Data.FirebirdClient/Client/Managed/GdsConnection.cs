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
		#region Fields

		private NetworkStream _networkStream;
		private FirebirdNetworkHandlingWrapper _firebirdNetworkHandlingWrapper;

		#endregion

		#region Properties

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

		#endregion

		#region Constructors

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

		#endregion

		#region Methods

		public async Task Connect(AsyncWrappingCommonArgs async)
		{
			try
			{
				IPAddress = await GetIPAddress(DataSource, async).ConfigureAwait(false);
				var endPoint = new IPEndPoint(IPAddress, PortNumber);

				var socket = new Socket(IPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, PacketSize);
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, PacketSize);
				socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);

				if (async.IsAsync)
				{
					using (var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(Timeout)))
					{
						using (var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, async.CancellationToken))
						{
							await ConnectAsyncHelper(socket)(endPoint, combinedCts.Token).ConfigureAwait(false);
						}
					}
#if NET48 || NETSTANDARD2_0 || NETSTANDARD2_1
					static Func<IPEndPoint, CancellationToken, Task> ConnectAsyncHelper(Socket socket) => (e, ct) => Task.Factory.FromAsync(socket.BeginConnect, socket.EndConnect, e, null);
#else
					static Func<IPEndPoint, CancellationToken, Task> ConnectAsyncHelper(Socket socket) => (e, ct) => SocketTaskExtensions.ConnectAsync(socket, e, ct).AsTask();
#endif
				}
				else
				{
					socket.Connect(endPoint);
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

		public async Task Identify(string database, AsyncWrappingCommonArgs async)
		{
			try
			{
				await Xdr.Write(IscCodes.op_connect, async).ConfigureAwait(false);
				await Xdr.Write(IscCodes.op_attach, async).ConfigureAwait(false);
				await Xdr.Write(IscCodes.CONNECT_VERSION3, async).ConfigureAwait(false);
				await Xdr.Write(IscCodes.GenericAchitectureClient, async).ConfigureAwait(false);

				await Xdr.Write(database, async).ConfigureAwait(false);

				var protocols = ProtocolsSupported.Get(Compression);
				await Xdr.Write(protocols.Count(), async).ConfigureAwait(false);

				AuthBlock = new AuthBlock(User, Password, WireCrypt);

				await Xdr.WriteBuffer(AuthBlock.UserIdentificationData(), async).ConfigureAwait(false);

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
					ProtocolVersion = await Xdr.ReadInt32(async).ConfigureAwait(false);
					ProtocolArchitecture = await Xdr.ReadInt32(async).ConfigureAwait(false);
					ProtocolMinimunType = await Xdr.ReadInt32(async).ConfigureAwait(false);

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
							await Xdr.ReadBuffer(async).ConfigureAwait(false),
							await Xdr.ReadString(async).ConfigureAwait(false),
							await Xdr.ReadBoolean(async).ConfigureAwait(false),
							await Xdr.ReadBuffer(async).ConfigureAwait(false));

						if (Compression)
						{
							// after reading before writing
							StartCompression();
						}

						if (operation == IscCodes.op_cond_accept && AuthBlock.HasClientData)
						{
							while (true)
							{
								await AuthBlock.SendContAuthToBuffer(Xdr, async).ConfigureAwait(false);
								await Xdr.Flush(async).ConfigureAwait(false);
								var response = await AuthBlock.ProcessContAuthResponse(Xdr, async).ConfigureAwait(false);
								if (response is ContAuthResponse contAuthResponse)
								{
									AuthBlock.Start(contAuthResponse.ServerData, contAuthResponse.AcceptPluginName, contAuthResponse.IsAuthenticated, contAuthResponse.ServerKeys);
									continue;
								}
								break;
							}

							await AuthBlock.SendWireCryptToBuffer(Xdr, async).ConfigureAwait(false);
							await Xdr.Flush(async).ConfigureAwait(false);
							await AuthBlock.ProcessWireCryptResponse(Xdr, this, async).ConfigureAwait(false);
						}
					}

					AuthBlock.WireCryptValidate(ProtocolVersion);
				}
				else if (operation == IscCodes.op_response)
				{
					var response = (GenericResponse)await ProcessOperation(operation, Xdr, async).ConfigureAwait(false);
					ProcessResponse(response);
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

		public async Task Disconnect(AsyncWrappingCommonArgs async)
		{
			if (_networkStream != null)
			{
#if NET48 || NETSTANDARD2_0
				_networkStream.Dispose();
				await Task.CompletedTask.ConfigureAwait(false);
#else
				await async.AsyncSyncCallNoCancellation(_networkStream.DisposeAsync, _networkStream.Dispose).ConfigureAwait(false);
#endif
				_networkStream = null;
			}
		}

		#endregion

		#region Internal Methods

		internal void StartCompression()
		{
			_firebirdNetworkHandlingWrapper.StartCompression();
		}

		internal void StartEncryption()
		{
			_firebirdNetworkHandlingWrapper.StartEncryption(AuthBlock.SessionKey);
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
					return new FetchResponse(
						await xdr.ReadInt32(async).ConfigureAwait(false),
						await xdr.ReadInt32(async).ConfigureAwait(false));

				case IscCodes.op_sql_response:
					return new SqlResponse(
						await xdr.ReadInt32(async).ConfigureAwait(false));

				case IscCodes.op_trusted_auth:
					return new AuthResponse(
						await xdr.ReadBuffer(async).ConfigureAwait(false));

				case IscCodes.op_crypt_key_callback:
					return new CryptKeyCallbackResponse(
						await xdr.ReadBuffer(async).ConfigureAwait(false));

				case IscCodes.op_cont_auth:
					return new ContAuthResponse(
						await xdr.ReadBuffer(async).ConfigureAwait(false),
						await xdr.ReadString(async).ConfigureAwait(false),
						await xdr.ReadBoolean(async).ConfigureAwait(false),
						await xdr.ReadBuffer(async).ConfigureAwait(false));

				default:
					throw new ArgumentOutOfRangeException(nameof(operation), $"{nameof(operation)}={operation}");
			}
		}

		public static void ProcessResponse(IResponse response)
		{
			if (response is GenericResponse genericResponse)
			{
				if (genericResponse.Exception != null && !genericResponse.Exception.IsWarning)
				{
					throw genericResponse.Exception;
				}
			}
		}

		public static void ProcessResponseWarnings(IResponse response, Action<IscException> onWarning)
		{
			if (response is GenericResponse genericResponse)
			{
				if (genericResponse.Exception != null && genericResponse.Exception.IsWarning)
				{
					onWarning?.Invoke(genericResponse.Exception);
				}
			}
		}

		#endregion
	}
}
