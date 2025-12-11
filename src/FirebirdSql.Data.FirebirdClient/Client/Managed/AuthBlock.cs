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

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Client.Managed.Srp;
using FirebirdSql.Data.Client.Managed.Sspi;
using FirebirdSql.Data.Common;
using WireCryptOption = FirebirdSql.Data.Client.Managed.Version13.WireCryptOption;

namespace FirebirdSql.Data.Client.Managed;

sealed class AuthBlock
{
	Srp256Client _srp256;
	SrpClient _srp;
	SspiHelper _sspi;

	public GdsConnection Connection { get; }
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

	private const int STACKALLOC_LIMIT = 512;

	public AuthBlock(GdsConnection connection, string user, string password, WireCryptOption wireCrypt)
	{
		_srp256 = new Srp256Client();
		_srp = new SrpClient();
		_sspi = new SspiHelper();

		Connection = connection;
		User = user;
		Password = password;
		WireCrypt = wireCrypt;
	}

	public byte[] UserIdentificationData()
	{
		using (var result = new MemoryStream(256))
		{
			{
				var userString = Environment.GetEnvironmentVariable("USERNAME") ?? Environment.GetEnvironmentVariable("USER") ?? string.Empty;
				var slen = Encoding.UTF8.GetByteCount(userString);
				byte[] rented = null;
				Span<byte> user = slen > STACKALLOC_LIMIT
					? (rented = System.Buffers.ArrayPool<byte>.Shared.Rent(slen)).AsSpan(0, slen)
					: stackalloc byte[slen];
				int real_len = Encoding.UTF8.GetBytes(userString, user);
				result.WriteByte(IscCodes.CNCT_user);
				result.WriteByte((byte)real_len);
				result.Write(user);
				if (rented != null)
				{
					System.Buffers.ArrayPool<byte>.Shared.Return(rented, clearArray: true);
				}
			}

			{
				var hostName = Dns.GetHostName();
				var slen = Encoding.UTF8.GetByteCount(hostName);
				byte[] rented = null;
				Span<byte> host = slen > STACKALLOC_LIMIT
					? (rented = System.Buffers.ArrayPool<byte>.Shared.Rent(slen)).AsSpan(0, slen)
					: stackalloc byte[slen];
				int real_len = Encoding.UTF8.GetBytes(hostName, host);
				result.WriteByte(IscCodes.CNCT_host);
				result.WriteByte((byte)real_len);
				result.Write(host);
				if (rented != null)
				{
					System.Buffers.ArrayPool<byte>.Shared.Return(rented, clearArray: true);
				}
			}

			result.WriteByte(IscCodes.CNCT_user_verification);
			result.WriteByte(0);

			if (!string.IsNullOrEmpty(User))
			{
				{
					var slen = Encoding.UTF8.GetByteCount(User);
					byte[] rented = null;
					Span<byte> bytes = slen > STACKALLOC_LIMIT
						? (rented = System.Buffers.ArrayPool<byte>.Shared.Rent(slen)).AsSpan(0, slen)
						: stackalloc byte[slen];
					int real_len = Encoding.UTF8.GetBytes(User, bytes);
					result.WriteByte(IscCodes.CNCT_login);
					result.WriteByte((byte)real_len);
					result.Write(bytes);
					if (rented != null)
					{
						System.Buffers.ArrayPool<byte>.Shared.Return(rented, clearArray: true);
					}
				}
				{
					var slen = Encoding.UTF8.GetByteCount(_srp256.Name);
					byte[] rented = null;
					Span<byte> bytes = slen > STACKALLOC_LIMIT
						? (rented = System.Buffers.ArrayPool<byte>.Shared.Rent(slen)).AsSpan(0, slen)
						: stackalloc byte[slen];
					int real_len = Encoding.UTF8.GetBytes(_srp256.Name, bytes);
					result.WriteByte(IscCodes.CNCT_plugin_name);
					result.WriteByte((byte)real_len);
					result.Write(bytes[..real_len]);
					if (rented != null)
					{
						System.Buffers.ArrayPool<byte>.Shared.Return(rented, clearArray: true);
					}
				}
				{
					var slen = Encoding.UTF8.GetByteCount(_srp256.PublicKeyHex);
					byte[] rented = null;
					Span<byte> specificData = slen > STACKALLOC_LIMIT
						? (rented = System.Buffers.ArrayPool<byte>.Shared.Rent(slen)).AsSpan(0, slen)
						: stackalloc byte[slen];
					Encoding.UTF8.GetBytes(_srp256.PublicKeyHex.AsSpan(), specificData);
					WriteMultiPartHelper(result, IscCodes.CNCT_specific_data, specificData);
					if (rented != null)
					{
						System.Buffers.ArrayPool<byte>.Shared.Return(rented, clearArray: true);
					}
				}
				{
					var slen1 = Encoding.UTF8.GetByteCount(_srp256.Name);
					byte[] rented1 = null;
					Span<byte> bytes1 = slen1 > STACKALLOC_LIMIT
						? (rented1 = System.Buffers.ArrayPool<byte>.Shared.Rent(slen1)).AsSpan(0, slen1)
						: stackalloc byte[slen1];
					Span<byte> bytes2 = stackalloc byte[1];
					var slen3 = Encoding.UTF8.GetByteCount(_srp.Name);
					byte[] rented3 = null;
					Span<byte> bytes3 = slen3 > STACKALLOC_LIMIT
						? (rented3 = System.Buffers.ArrayPool<byte>.Shared.Rent(slen3)).AsSpan(0, slen3)
						: stackalloc byte[slen3];
					int l1 = Encoding.UTF8.GetBytes(_srp256.Name.AsSpan(), bytes1);
					int l2 = Encoding.UTF8.GetBytes(",".AsSpan(), bytes2);
					int l3 = Encoding.UTF8.GetBytes(_srp.Name.AsSpan(), bytes3);
					result.WriteByte(IscCodes.CNCT_plugin_list);
					result.WriteByte((byte)(l1+l2+l3));
					result.Write(bytes1);
					result.Write(bytes2);
					result.Write(bytes3);
					if (rented1 != null)
					{
						System.Buffers.ArrayPool<byte>.Shared.Return(rented1, clearArray: true);
					}
					if (rented3 != null)
					{
						System.Buffers.ArrayPool<byte>.Shared.Return(rented3, clearArray: true);
					}
				}

				{
					result.WriteByte(IscCodes.CNCT_client_crypt);
					result.WriteByte(4);
					Span<byte> bytes = stackalloc byte[4];
					if (!BitConverter.TryWriteBytes(bytes, IPAddress.NetworkToHostOrder(WireCryptOptionValue(WireCrypt))))
					{
						throw new InvalidOperationException("Failed to write wire crypt option bytes.");
					}
					result.Write(bytes);
				}
			}
			else
			{
				var slen = Encoding.UTF8.GetByteCount(_sspi.Name);
				byte[] rented = null;
				Span<byte> pluginNameBytes = slen > STACKALLOC_LIMIT
					? (rented = System.Buffers.ArrayPool<byte>.Shared.Rent(slen)).AsSpan(0, slen)
					: stackalloc byte[slen];
				int pluginNameLen = Encoding.UTF8.GetBytes(_sspi.Name.AsSpan(), pluginNameBytes);
				result.WriteByte(IscCodes.CNCT_plugin_name);
				result.WriteByte((byte)pluginNameLen);
				result.Write(pluginNameBytes[..pluginNameLen]);

				var specificData = _sspi.InitializeClientSecurity();
				WriteMultiPartHelper(result, IscCodes.CNCT_specific_data, specificData);

				result.WriteByte(IscCodes.CNCT_plugin_list);
				result.WriteByte((byte)pluginNameLen);
				result.Write(pluginNameBytes[..pluginNameLen]);

				result.WriteByte(IscCodes.CNCT_client_crypt);
				result.WriteByte(4);
				Span<byte> wireCryptBytes = stackalloc byte[4];
				if (!BitConverter.TryWriteBytes(wireCryptBytes, IPAddress.NetworkToHostOrder(IscCodes.WIRE_CRYPT_DISABLED)))
				{
					throw new InvalidOperationException("Failed to write wire crypt disabled bytes.");
				}
				result.Write(wireCryptBytes);
				if (rented != null)
				{
					System.Buffers.ArrayPool<byte>.Shared.Return(rented, clearArray: true);
				}
			}

			return result.ToArray();
		}
	}

	public void SendContAuthToBuffer()
	{
		Connection.Xdr.Write(IscCodes.op_cont_auth);
		Connection.Xdr.WriteBuffer(HasClientData ? ClientData : PublicClientData); // p_data
		Connection.Xdr.Write(AcceptPluginName); // p_name
		Connection.Xdr.Write(AcceptPluginName); // p_list
		Connection.Xdr.WriteBuffer(ServerKeys); // p_keys
	}
	public async ValueTask SendContAuthToBufferAsync(CancellationToken cancellationToken = default)
	{
		await Connection.Xdr.WriteAsync(IscCodes.op_cont_auth, cancellationToken).ConfigureAwait(false);
		await Connection.Xdr.WriteBufferAsync(HasClientData ? ClientData : PublicClientData, cancellationToken).ConfigureAwait(false); // p_data
		await Connection.Xdr.WriteAsync(AcceptPluginName, cancellationToken).ConfigureAwait(false); // p_name
		await Connection.Xdr.WriteAsync(AcceptPluginName, cancellationToken).ConfigureAwait(false); // p_list
		await Connection.Xdr.WriteBufferAsync(ServerKeys, cancellationToken).ConfigureAwait(false); // p_keys
	}

	// TODO: maybe more logic can be pulled up here
	public IResponse ProcessContAuthResponse()
	{
		var operation = Connection.Xdr.ReadOperation();
		var response = Connection.ProcessOperation(operation);
		response.HandleResponseException();
		if (response is Version13.ContAuthResponse)
		{
			return response;
		}
		else if (response is Version13.CryptKeyCallbackResponse || response is Version15.CryptKeyCallbackResponse)
		{
			return response;
		}
		else if (response is GenericResponse genericResponse)
		{
			ServerKeys = genericResponse.Data;
			Complete();
		}
		else
		{
			throw new InvalidOperationException($"Unexpected response ({operation}).");
		}
		return response;
	}
	public async ValueTask<IResponse> ProcessContAuthResponseAsync(CancellationToken cancellationToken = default)
	{
		var operation = await Connection.Xdr.ReadOperationAsync(cancellationToken).ConfigureAwait(false);
		var response = await Connection.ProcessOperationAsync(operation, cancellationToken).ConfigureAwait(false);
		response.HandleResponseException();
		if (response is Version13.ContAuthResponse)
		{
			return response;
		}
		else if (response is Version13.CryptKeyCallbackResponse || response is Version15.CryptKeyCallbackResponse)
		{
			return response;
		}
		else if (response is GenericResponse genericResponse)
		{
			ServerKeys = genericResponse.Data;
			Complete();
		}
		else
		{
			throw new InvalidOperationException($"Unexpected response ({operation}).");
		}
		return response;
	}

	public void SendWireCryptToBuffer()
	{
		if (WireCrypt == WireCryptOption.Disabled)
			return;

		Connection.Xdr.Write(IscCodes.op_crypt);
		Connection.Xdr.Write(FirebirdNetworkHandlingWrapper.EncryptionName);
		Connection.Xdr.Write(SessionKeyName);
	}
	public async ValueTask SendWireCryptToBufferAsync(CancellationToken cancellationToken = default)
	{
		if (WireCrypt == WireCryptOption.Disabled)
			return;

		await Connection.Xdr.WriteAsync(IscCodes.op_crypt, cancellationToken).ConfigureAwait(false);
		await Connection.Xdr.WriteAsync(FirebirdNetworkHandlingWrapper.EncryptionName, cancellationToken).ConfigureAwait(false);
		await Connection.Xdr.WriteAsync(SessionKeyName, cancellationToken).ConfigureAwait(false);
	}

	public void ProcessWireCryptResponse()
	{
		if (WireCrypt == WireCryptOption.Disabled)
			return;

		// after writing before reading
		Connection.StartEncryption();

		var operation = Connection.Xdr.ReadOperation();
		var response = Connection.ProcessOperation(operation);
		response.HandleResponseException();

		WireCryptInitialized = true;
	}
	public async ValueTask ProcessWireCryptResponseAsync(CancellationToken cancellationToken = default)
	{
		if (WireCrypt == WireCryptOption.Disabled)
			return;

		// after writing before reading
		Connection.StartEncryption();

		var operation = await Connection.Xdr.ReadOperationAsync(cancellationToken).ConfigureAwait(false);
		var response = await Connection.ProcessOperationAsync(operation, cancellationToken).ConfigureAwait(false);
		response.HandleResponseException();

		WireCryptInitialized = true;
	}

	public void WireCryptValidate(int protocolVersion)
	{
		var validProtocolVersion = protocolVersion == IscCodes.PROTOCOL_VERSION13
			|| protocolVersion == IscCodes.PROTOCOL_VERSION15
			|| protocolVersion == IscCodes.PROTOCOL_VERSION16;
		if (validProtocolVersion && WireCrypt == WireCryptOption.Required && IsAuthenticated && !WireCryptInitialized)
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
			PublicClientData = Encoding.UTF8.GetBytes(_srp256.PublicKeyHex);
			if (hasServerData)
			{
				ClientData = Encoding.UTF8.GetBytes(_srp256.ClientProof(NormalizeLogin(User), Password, ServerData).ToHexString());
			}
			SessionKey = _srp256.SessionKey;
			SessionKeyName = _srp256.SessionKeyName;
		}
		else if (AcceptPluginName.Equals(_srp.Name, StringComparison.Ordinal))
		{
			PublicClientData = Encoding.UTF8.GetBytes(_srp.PublicKeyHex);
			if (hasServerData)
			{
				ClientData = Encoding.UTF8.GetBytes(_srp.ClientProof(NormalizeLogin(User), Password, ServerData).ToHexString());
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

	public void Complete()
	{
		IsAuthenticated = true;
		ReleaseAuth();
	}

	void ReleaseAuth()
	{
		_srp256 = null;
		_srp = null;
		_sspi?.Dispose();
		_sspi = null;
	}

	static void WriteMultiPartHelper(MemoryStream stream, byte code, byte[] data)
	{
		const int MaxLength = 255 - 1;
		var part = 0;
		for (var i = 0; i < data.Length; i += MaxLength) {
			stream.WriteByte(code);
			var length = Math.Min(data.Length - i, MaxLength);
			stream.WriteByte((byte)(length + 1));
			stream.WriteByte((byte)part);
			stream.Write(data, i, length);
			part++;
		}
	}

	static void WriteMultiPartHelper(MemoryStream stream, byte code, ReadOnlySpan<byte> data)
	{
		const int MaxLength = 255 - 1;
		var part = 0;
		for (var i = 0; i < data.Length; i += MaxLength)
		{
			stream.WriteByte(code);
			var length = Math.Min(data.Length - i, MaxLength);
			stream.WriteByte((byte)(length + 1));
			stream.WriteByte((byte)part);
			stream.Write(data[i..(i+length)]);
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
