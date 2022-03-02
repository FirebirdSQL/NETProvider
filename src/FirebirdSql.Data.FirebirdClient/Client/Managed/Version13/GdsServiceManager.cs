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

using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version13;

internal class GdsServiceManager : Version12.GdsServiceManager
{
	public GdsServiceManager(GdsConnection connection)
		: base(connection)
	{ }

	public override bool UseUtf8ParameterBuffer => true;

	public override void Attach(ServiceParameterBufferBase spb, string dataSource, int port, string service, byte[] cryptKey)
	{
		try
		{
			SendAttachToBuffer(spb, service);
			Database.Xdr.Flush();
			var response = Database.ReadResponse();
			if (response is ContAuthResponse)
			{
				while (response is ContAuthResponse contAuthResponse)
				{
					Connection.AuthBlock.Start(contAuthResponse.ServerData, contAuthResponse.AcceptPluginName, contAuthResponse.IsAuthenticated, contAuthResponse.ServerKeys);

					Connection.AuthBlock.SendContAuthToBuffer();
					Database.Xdr.Flush();
					response = Connection.AuthBlock.ProcessContAuthResponse();
					response = (Database as GdsDatabase).ProcessCryptCallbackResponseIfNeeded(response, cryptKey);
				}
				var genericResponse = (GenericResponse)response;
				base.ProcessAttachResponse(genericResponse);

				if (genericResponse.Data.Any())
				{
					Database.AuthBlock.SendWireCryptToBuffer();
					Database.Xdr.Flush();
					Database.AuthBlock.ProcessWireCryptResponse();
				}
			}
			else
			{
				response = (Database as GdsDatabase).ProcessCryptCallbackResponseIfNeeded(response, cryptKey);
				ProcessAttachResponse((GenericResponse)response);
				Database.AuthBlock.Complete();
			}
		}
		catch (IscException)
		{
			Database.SafelyDetach();
			throw;
		}
		catch (IOException ex)
		{
			Database.SafelyDetach();
			throw IscException.ForIOException(ex);
		}
	}
	public override async ValueTask AttachAsync(ServiceParameterBufferBase spb, string dataSource, int port, string service, byte[] cryptKey, CancellationToken cancellationToken = default)
	{
		try
		{
			await SendAttachToBufferAsync(spb, service, cancellationToken).ConfigureAwait(false);
			await Database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
			var response = await Database.ReadResponseAsync(cancellationToken).ConfigureAwait(false);
			if (response is ContAuthResponse)
			{
				while (response is ContAuthResponse contAuthResponse)
				{
					Connection.AuthBlock.Start(contAuthResponse.ServerData, contAuthResponse.AcceptPluginName, contAuthResponse.IsAuthenticated, contAuthResponse.ServerKeys);

					await Connection.AuthBlock.SendContAuthToBufferAsync(cancellationToken).ConfigureAwait(false);
					await Database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
					response = await Connection.AuthBlock.ProcessContAuthResponseAsync(cancellationToken).ConfigureAwait(false);
					response = await (Database as GdsDatabase).ProcessCryptCallbackResponseIfNeededAsync(response, cryptKey, cancellationToken).ConfigureAwait(false);
				}
				var genericResponse = (GenericResponse)response;
				await base.ProcessAttachResponseAsync(genericResponse, cancellationToken).ConfigureAwait(false);

				if (genericResponse.Data.Any())
				{
					await Database.AuthBlock.SendWireCryptToBufferAsync(cancellationToken).ConfigureAwait(false);
					await Database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
					await Database.AuthBlock.ProcessWireCryptResponseAsync(cancellationToken).ConfigureAwait(false);
				}
			}
			else
			{
				response = await (Database as GdsDatabase).ProcessCryptCallbackResponseIfNeededAsync(response, cryptKey, cancellationToken).ConfigureAwait(false);
				await ProcessAttachResponseAsync((GenericResponse)response, cancellationToken).ConfigureAwait(false);
				Database.AuthBlock.Complete();
			}
		}
		catch (IscException)
		{
			await Database.SafelyDetachAsync(cancellationToken).ConfigureAwait(false);
			throw;
		}
		catch (IOException ex)
		{
			await Database.SafelyDetachAsync(cancellationToken).ConfigureAwait(false);
			throw IscException.ForIOException(ex);
		}
	}

	public override ServiceParameterBufferBase CreateServiceParameterBuffer()
	{
		return new ServiceParameterBuffer3(Database.ParameterBufferEncoding);
	}

	protected override Version10.GdsDatabase CreateDatabase(GdsConnection connection)
	{
		return new GdsDatabase(connection);
	}
}
