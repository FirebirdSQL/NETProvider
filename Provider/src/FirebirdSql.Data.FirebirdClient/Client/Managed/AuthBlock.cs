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

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FirebirdSql.Data.Client.Managed.Version13;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed
{
	sealed class AuthBlock
	{
		Srp256Client _srp256;
		SrpClient _srp;
		SspiHelper _sspi;

		public string User { get; }
		public string Password { get; }
		public WireCryptOption WireCrypt { get; }

		public byte[] ServerData { get; private set; }
		public string AcceptPluginName { get; private set; }
		public bool IsAuthenticated { get; private set; }
		public byte[] ServerKeys { get; private set; }

		public byte[] PublicClientData { get; private set; }
		public bool HasClientData => ClientData != null;
		public byte[] ClientData { get; private set; }
		public byte[] SessionKey { get; private set; }
		public string SessionKeyName { get; private set; }

		public bool WireCryptInitialized { get; private set; }

		public AuthBlock(string user, string password, WireCryptOption wireCrypt)
		{
			_srp256 = new Srp256Client();
			_srp = new SrpClient();
			_sspi = new SspiHelper();

			User = user;
			Password = password;
			WireCrypt = wireCrypt;
		}

		public byte[] UserIdentificationData()
		{
			using (var result = new MemoryStream(1024))
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

				if (!string.IsNullOrEmpty(User))
				{
					var login = Encoding.UTF8.GetBytes(User);
					result.WriteByte(IscCodes.CNCT_login);
					result.WriteByte((byte)login.Length);
					result.Write(login, 0, login.Length);

					var pluginNameBytes = Encoding.ASCII.GetBytes(_srp256.Name);
					result.WriteByte(IscCodes.CNCT_plugin_name);
					result.WriteByte((byte)pluginNameBytes.Length);
					result.Write(pluginNameBytes, 0, pluginNameBytes.Length);
					var specificData = Encoding.ASCII.GetBytes(_srp256.PublicKeyHex);
					WriteMultiPartHelper(result, IscCodes.CNCT_specific_data, specificData);

					var plugins = string.Join(",", new[] { _srp256.Name, _srp.Name });
					var pluginsBytes = Encoding.ASCII.GetBytes(plugins);
					result.WriteByte(IscCodes.CNCT_plugin_list);
					result.WriteByte((byte)pluginsBytes.Length);
					result.Write(pluginsBytes, 0, pluginsBytes.Length);

					result.WriteByte(IscCodes.CNCT_client_crypt);
					result.WriteByte(4);
					result.Write(TypeEncoder.EncodeInt32(WireCryptOptionValue(WireCrypt)), 0, 4);
				}
				else
				{
					var pluginNameBytes = Encoding.ASCII.GetBytes(_sspi.Name);
					result.WriteByte(IscCodes.CNCT_plugin_name);
					result.WriteByte((byte)pluginNameBytes.Length);
					result.Write(pluginNameBytes, 0, pluginNameBytes.Length);
					var specificData = _sspi.InitializeClientSecurity();
					WriteMultiPartHelper(result, IscCodes.CNCT_specific_data, specificData);

					result.WriteByte(IscCodes.CNCT_plugin_list);
					result.WriteByte((byte)pluginNameBytes.Length);
					result.Write(pluginNameBytes, 0, pluginNameBytes.Length);

					result.WriteByte(IscCodes.CNCT_client_crypt);
					result.WriteByte(4);
					result.Write(TypeEncoder.EncodeInt32(IscCodes.WIRE_CRYPT_DISABLED), 0, 4);
				}

				return result.ToArray();
			}
		}

		public async Task SendContAuthToBuffer(IXdrWriter xdr, AsyncWrappingCommonArgs async)
		{
			await xdr.Write(IscCodes.op_cont_auth, async).ConfigureAwait(false);
			await xdr.WriteBuffer(ClientData, async).ConfigureAwait(false); // p_data
			await xdr.Write(AcceptPluginName, async).ConfigureAwait(false); // p_name
			await xdr.Write(AcceptPluginName, async).ConfigureAwait(false); // p_list
			await xdr.WriteBuffer(ServerKeys, async).ConfigureAwait(false); // p_keys
		}

		// TODO: maybe more logic can be pulled up here
		public async Task<IResponse> ProcessContAuthResponse(IXdrReader xdr, AsyncWrappingCommonArgs async)
		{
			var operation = await xdr.ReadOperation(async).ConfigureAwait(false);
			var response = await GdsConnection.ProcessOperation(operation, xdr, async).ConfigureAwait(false);
			GdsConnection.ProcessResponse(response);
			if (response is ContAuthResponse)
			{
				return response;
			}
			else if (response is CryptKeyCallbackResponse)
			{
				return response;
			}
			else if (response is GenericResponse genericResponse)
			{
				ServerKeys = genericResponse.Data;
				IsAuthenticated = true;
				Complete();
			}
			else
			{
				throw new InvalidOperationException($"Unexpected response ({operation}).");
			}
			return response;
		}

		public async Task SendWireCryptToBuffer(IXdrWriter xdr, AsyncWrappingCommonArgs async)
		{
			if (WireCrypt == WireCryptOption.Disabled)
				return;

			await xdr.Write(IscCodes.op_crypt, async).ConfigureAwait(false);
			await xdr.Write(FirebirdNetworkHandlingWrapper.EncryptionName, async).ConfigureAwait(false);
			await xdr.Write(SessionKeyName, async).ConfigureAwait(false);
		}

		public async Task ProcessWireCryptResponse(IXdrReader xdr, GdsConnection connection, AsyncWrappingCommonArgs async)
		{
			if (WireCrypt == WireCryptOption.Disabled)
				return;

			// after writing before reading
			connection.StartEncryption();

			var response = await GdsConnection.ProcessOperation(await xdr.ReadOperation(async).ConfigureAwait(false), xdr, async).ConfigureAwait(false);
			GdsConnection.ProcessResponse(response);

			WireCryptInitialized = true;
		}

		public void WireCryptValidate(int protocolVersion)
		{
			if (protocolVersion == IscCodes.PROTOCOL_VERSION13 && WireCrypt == WireCryptOption.Required && IsAuthenticated && !WireCryptInitialized)
			{
				throw IscException.ForErrorCode(IscCodes.isc_wirecrypt_incompatible);
			}
		}

		public void Start(byte[] serverData, string acceptPluginName, bool isAuthenticated, byte[] serverKeys)
		{
			ServerData = serverData;
			AcceptPluginName = acceptPluginName;
			IsAuthenticated = isAuthenticated;
			ServerKeys = serverKeys;

			var hasServerData = ServerData.Length != 0;
			if (AcceptPluginName.Equals(_srp256.Name, StringComparison.Ordinal))
			{
				PublicClientData = Encoding.ASCII.GetBytes(_srp256.PublicKeyHex);
				if (hasServerData)
				{
					ClientData = Encoding.ASCII.GetBytes(_srp256.ClientProof(NormalizeLogin(User), Password, ServerData).ToHexString());
				}
				SessionKey = _srp256.SessionKey;
				SessionKeyName = _srp256.SessionKeyName;
			}
			else if (AcceptPluginName.Equals(_srp.Name, StringComparison.Ordinal))
			{
				PublicClientData = Encoding.ASCII.GetBytes(_srp.PublicKeyHex);
				if (hasServerData)
				{
					ClientData = Encoding.ASCII.GetBytes(_srp.ClientProof(NormalizeLogin(User), Password, ServerData).ToHexString());
				}
				SessionKey = _srp.SessionKey;
				SessionKeyName = _srp.SessionKeyName;
			}
			else if (AcceptPluginName.Equals(_sspi.Name, StringComparison.Ordinal))
			{
				if (hasServerData)
				{
					ClientData = _sspi.GetClientSecurity(ServerData);
				}
			}
			else
			{
				throw new NotSupportedException($"Not supported plugin '{AcceptPluginName}'.");
			}
		}

		void Complete()
		{
			_srp256 = null;
			_srp = null;
			_sspi.Dispose();
			_sspi = null;
		}

		static void WriteMultiPartHelper(Stream stream, byte code, byte[] data)
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

		static int WireCryptOptionValue(WireCryptOption wireCrypt)
		{
			return wireCrypt switch
			{
				WireCryptOption.Disabled => IscCodes.WIRE_CRYPT_DISABLED,
				WireCryptOption.Enabled => IscCodes.WIRE_CRYPT_ENABLED,
				WireCryptOption.Required => IscCodes.WIRE_CRYPT_REQUIRED,
				_ => throw new ArgumentOutOfRangeException(nameof(wireCrypt), $"{nameof(wireCrypt)}={wireCrypt}"),
			};
		}

		internal static string NormalizeLogin(string login)
		{
			if (string.IsNullOrEmpty(login))
			{
				return login;
			}
			if (login.Length > 2 && login[0] == '"' && login[login.Length - 1] == '"')
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
			return login.ToUpperInvariant();
		}
	}
}
