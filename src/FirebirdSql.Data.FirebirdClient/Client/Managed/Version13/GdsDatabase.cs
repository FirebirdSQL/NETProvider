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

//$Authors = Hajime Nakagami, Jiri Cincura (jiri@cincura.net)

using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version13;

internal class GdsDatabase : Version12.GdsDatabase
{
	public GdsDatabase(GdsConnection connection)
		: base(connection)
	{ }

	public override void Attach(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey)
	{
		try
		{
			SendAttachToBuffer(dpb, database);
			Xdr.Flush();
			var response = ReadResponse();
			if (response is ContAuthResponse)
			{
				while (response is ContAuthResponse contAuthResponse)
				{
					AuthBlock.Start(contAuthResponse.ServerData, contAuthResponse.AcceptPluginName, contAuthResponse.IsAuthenticated, contAuthResponse.ServerKeys);

					AuthBlock.SendContAuthToBuffer();
					Xdr.Flush();
					response = AuthBlock.ProcessContAuthResponse();
					response = ProcessCryptCallbackResponseIfNeeded(response, cryptKey);
				}
				var genericResponse = (GenericResponse)response;
				ProcessAttachResponse(genericResponse);

				if (genericResponse.Data.Any())
				{
					AuthBlock.SendWireCryptToBuffer();
					Xdr.Flush();
					AuthBlock.ProcessWireCryptResponse();
				}
			}
			else
			{
				response = ProcessCryptCallbackResponseIfNeeded(response, cryptKey);
				ProcessAttachResponse((GenericResponse)response);
				AuthBlock.Complete();
			}
			AuthBlock.WireCryptValidate(IscCodes.PROTOCOL_VERSION13);
		}
		catch (IscException)
		{
			SafelyDetach();
			throw;
		}
		catch (IOException ex)
		{
			SafelyDetach();
			throw IscException.ForIOException(ex);
		}

		AfterAttachActions();
	}
	public override async ValueTask AttachAsync(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey, CancellationToken cancellationToken = default)
	{
		try
		{
			await SendAttachToBufferAsync(dpb, database, cancellationToken).ConfigureAwait(false);
			await Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
			var response = await ReadResponseAsync(cancellationToken).ConfigureAwait(false);
			if (response is ContAuthResponse)
			{
				while (response is ContAuthResponse contAuthResponse)
				{
					AuthBlock.Start(contAuthResponse.ServerData, contAuthResponse.AcceptPluginName, contAuthResponse.IsAuthenticated, contAuthResponse.ServerKeys);

					await AuthBlock.SendContAuthToBufferAsync(cancellationToken).ConfigureAwait(false);
					await Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
					response = await AuthBlock.ProcessContAuthResponseAsync(cancellationToken).ConfigureAwait(false);
					response = await ProcessCryptCallbackResponseIfNeededAsync(response, cryptKey, cancellationToken).ConfigureAwait(false);
				}
				var genericResponse = (GenericResponse)response;
				await ProcessAttachResponseAsync(genericResponse, cancellationToken).ConfigureAwait(false);

				if (genericResponse.Data.Any())
				{
					await AuthBlock.SendWireCryptToBufferAsync(cancellationToken).ConfigureAwait(false);
					await Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
					await AuthBlock.ProcessWireCryptResponseAsync(cancellationToken).ConfigureAwait(false);
				}
			}
			else
			{
				response = await ProcessCryptCallbackResponseIfNeededAsync(response, cryptKey, cancellationToken).ConfigureAwait(false);
				await ProcessAttachResponseAsync((GenericResponse)response, cancellationToken).ConfigureAwait(false);
				AuthBlock.Complete();
			}
			AuthBlock.WireCryptValidate(IscCodes.PROTOCOL_VERSION13);
		}
		catch (IscException)
		{
			await SafelyDetachAsync(cancellationToken).ConfigureAwait(false);
			throw;
		}
		catch (IOException ex)
		{
			await SafelyDetachAsync(cancellationToken).ConfigureAwait(false);
			throw IscException.ForIOException(ex);
		}

		await AfterAttachActionsAsync(cancellationToken).ConfigureAwait(false);
	}

	protected override void SendAttachToBuffer(DatabaseParameterBufferBase dpb, string database)
	{
		Xdr.Write(IscCodes.op_attach);
		Xdr.Write(DatabaseObjectId);
		if (!AuthBlock.HasClientData)
		{
			dpb.Append(IscCodes.isc_dpb_auth_plugin_name, AuthBlock.AcceptPluginName);
			dpb.Append(IscCodes.isc_dpb_specific_auth_data, AuthBlock.PublicClientData);
		}
		else
		{
			dpb.Append(IscCodes.isc_dpb_specific_auth_data, AuthBlock.ClientData);
		}
		Xdr.WriteBuffer(dpb.Encoding.GetBytes(database));
		Xdr.WriteBuffer(dpb.ToArray());
	}
	protected override async ValueTask SendAttachToBufferAsync(DatabaseParameterBufferBase dpb, string database, CancellationToken cancellationToken = default)
	{
		await Xdr.WriteAsync(IscCodes.op_attach, cancellationToken).ConfigureAwait(false);
		await Xdr.WriteAsync(DatabaseObjectId, cancellationToken).ConfigureAwait(false);
		if (!AuthBlock.HasClientData)
		{
			dpb.Append(IscCodes.isc_dpb_auth_plugin_name, AuthBlock.AcceptPluginName);
			dpb.Append(IscCodes.isc_dpb_specific_auth_data, AuthBlock.PublicClientData);
		}
		else
		{
			dpb.Append(IscCodes.isc_dpb_specific_auth_data, AuthBlock.ClientData);
		}
		await Xdr.WriteBufferAsync(dpb.Encoding.GetBytes(database), cancellationToken).ConfigureAwait(false);
		await Xdr.WriteBufferAsync(dpb.ToArray(), cancellationToken).ConfigureAwait(false);
	}

	public override void CreateDatabase(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey)
	{
		try
		{
			SendCreateToBuffer(dpb, database);
			Xdr.Flush();
			var response = ReadResponse();
			if (response is ContAuthResponse)
			{
				while (response is ContAuthResponse contAuthResponse)
				{
					AuthBlock.Start(contAuthResponse.ServerData, contAuthResponse.AcceptPluginName, contAuthResponse.IsAuthenticated, contAuthResponse.ServerKeys);

					AuthBlock.SendContAuthToBuffer();
					Xdr.Flush();
					response = AuthBlock.ProcessContAuthResponse();
					response = ProcessCryptCallbackResponseIfNeeded(response, cryptKey);
				}
				var genericResponse = (GenericResponse)response;
				ProcessCreateResponse(genericResponse);

				if (genericResponse.Data.Any())
				{
					AuthBlock.SendWireCryptToBuffer();
					Xdr.Flush();
					AuthBlock.ProcessWireCryptResponse();
				}
			}
			else
			{
				response = ProcessCryptCallbackResponseIfNeeded(response, cryptKey);
				ProcessCreateResponse((GenericResponse)response);
				AuthBlock.Complete();
			}
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	public override async ValueTask CreateDatabaseAsync(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey, CancellationToken cancellationToken = default)
	{
		try
		{
			await SendCreateToBufferAsync(dpb, database, cancellationToken).ConfigureAwait(false);
			await Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
			var response = await ReadResponseAsync(cancellationToken).ConfigureAwait(false);
			if (response is ContAuthResponse)
			{
				while (response is ContAuthResponse contAuthResponse)
				{
					AuthBlock.Start(contAuthResponse.ServerData, contAuthResponse.AcceptPluginName, contAuthResponse.IsAuthenticated, contAuthResponse.ServerKeys);

					await AuthBlock.SendContAuthToBufferAsync(cancellationToken).ConfigureAwait(false);
					await Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
					response = await AuthBlock.ProcessContAuthResponseAsync(cancellationToken).ConfigureAwait(false);
					response = await ProcessCryptCallbackResponseIfNeededAsync(response, cryptKey, cancellationToken).ConfigureAwait(false);
				}
				var genericResponse = (GenericResponse)response;
				await ProcessCreateResponseAsync(genericResponse, cancellationToken).ConfigureAwait(false);

				if (genericResponse.Data.Any())
				{
					await AuthBlock.SendWireCryptToBufferAsync(cancellationToken).ConfigureAwait(false);
					await Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
					await AuthBlock.ProcessWireCryptResponseAsync(cancellationToken).ConfigureAwait(false);
				}
			}
			else
			{
				response = await ProcessCryptCallbackResponseIfNeededAsync(response, cryptKey, cancellationToken).ConfigureAwait(false);
				await ProcessCreateResponseAsync((GenericResponse)response, cancellationToken).ConfigureAwait(false);
				AuthBlock.Complete();
			}
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	protected override void SendCreateToBuffer(DatabaseParameterBufferBase dpb, string database)
	{
		Xdr.Write(IscCodes.op_create);
		Xdr.Write(DatabaseObjectId);
		if (!AuthBlock.HasClientData)
		{
			dpb.Append(IscCodes.isc_dpb_auth_plugin_name, AuthBlock.AcceptPluginName);
			dpb.Append(IscCodes.isc_dpb_specific_auth_data, AuthBlock.PublicClientData);
		}
		else
		{
			dpb.Append(IscCodes.isc_dpb_specific_auth_data, AuthBlock.ClientData);
		}
		Xdr.WriteBuffer(dpb.Encoding.GetBytes(database));
		Xdr.WriteBuffer(dpb.ToArray());
	}
	protected override async ValueTask SendCreateToBufferAsync(DatabaseParameterBufferBase dpb, string database, CancellationToken cancellationToken = default)
	{
		await Xdr.WriteAsync(IscCodes.op_create, cancellationToken).ConfigureAwait(false);
		await Xdr.WriteAsync(DatabaseObjectId, cancellationToken).ConfigureAwait(false);
		if (!AuthBlock.HasClientData)
		{
			dpb.Append(IscCodes.isc_dpb_auth_plugin_name, AuthBlock.AcceptPluginName);
			dpb.Append(IscCodes.isc_dpb_specific_auth_data, AuthBlock.PublicClientData);
		}
		else
		{
			dpb.Append(IscCodes.isc_dpb_specific_auth_data, AuthBlock.ClientData);
		}
		await Xdr.WriteBufferAsync(dpb.Encoding.GetBytes(database), cancellationToken).ConfigureAwait(false);
		await Xdr.WriteBufferAsync(dpb.ToArray(), cancellationToken).ConfigureAwait(false);
	}

	public override void AttachWithTrustedAuth(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey)
	{
		Attach(dpb, database, cryptKey);
	}
	public override ValueTask AttachWithTrustedAuthAsync(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey, CancellationToken cancellationToken = default)
	{
		return AttachAsync(dpb, database, cryptKey, cancellationToken);
	}

	public override void CreateDatabaseWithTrustedAuth(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey)
	{
		CreateDatabase(dpb, database, cryptKey);
	}
	public override ValueTask CreateDatabaseWithTrustedAuthAsync(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey, CancellationToken cancellationToken = default)
	{
		return CreateDatabaseAsync(dpb, database, cryptKey, cancellationToken);
	}

	protected internal virtual IResponse ProcessCryptCallbackResponseIfNeeded(IResponse response, byte[] cryptKey)
	{
		while (response is CryptKeyCallbackResponse)
		{
			Xdr.Write(IscCodes.op_crypt_key_callback);
			Xdr.WriteBuffer(cryptKey);
			Xdr.Flush();
			response = ReadResponse();
		}
		return response;
	}
	protected internal virtual async ValueTask<IResponse> ProcessCryptCallbackResponseIfNeededAsync(IResponse response, byte[] cryptKey, CancellationToken cancellationToken = default)
	{
		while (response is CryptKeyCallbackResponse)
		{
			await Xdr.WriteAsync(IscCodes.op_crypt_key_callback, cancellationToken).ConfigureAwait(false);
			await Xdr.WriteBufferAsync(cryptKey, cancellationToken).ConfigureAwait(false);
			await Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
			response = await ReadResponseAsync(cancellationToken).ConfigureAwait(false);
		}
		return response;
	}

	public override StatementBase CreateStatement()
	{
		return new GdsStatement(this);
	}

	public override StatementBase CreateStatement(TransactionBase transaction)
	{
		return new GdsStatement(this, (Version10.GdsTransaction)transaction);
	}

	public override DatabaseParameterBufferBase CreateDatabaseParameterBuffer()
	{
		return new DatabaseParameterBuffer2(ParameterBufferEncoding);
	}
}
