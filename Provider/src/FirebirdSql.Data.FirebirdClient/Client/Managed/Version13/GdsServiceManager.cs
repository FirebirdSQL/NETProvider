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

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version13
{
	internal class GdsServiceManager : Version12.GdsServiceManager
	{
		public GdsServiceManager(GdsConnection connection)
			: base(connection)
		{ }

		public override async ValueTask AttachAsync(ServiceParameterBufferBase spb, string dataSource, int port, string service, byte[] cryptKey, AsyncWrappingCommonArgs async)
		{
			try
			{
				await SendAttachToBufferAsync(spb, service, async).ConfigureAwait(false);
				await Database.Xdr.FlushAsync(async).ConfigureAwait(false);
				var response = await Database.ReadResponseAsync(async).ConfigureAwait(false);
				if (response is ContAuthResponse)
				{
					while (response is ContAuthResponse contAuthResponse)
					{
						Connection.AuthBlock.Start(contAuthResponse.ServerData, contAuthResponse.AcceptPluginName, contAuthResponse.IsAuthenticated, contAuthResponse.ServerKeys);

						await Connection.AuthBlock.SendContAuthToBufferAsync(Database.Xdr, async).ConfigureAwait(false);
						await Database.Xdr.FlushAsync(async).ConfigureAwait(false);
						response = await Connection.AuthBlock.ProcessContAuthResponseAsync(Database.Xdr, async).ConfigureAwait(false);
						response = await (Database as GdsDatabase).ProcessCryptCallbackResponseIfNeededAsync(response, cryptKey, async).ConfigureAwait(false);
					}
					var genericResponse = (GenericResponse)response;
					await base.ProcessAttachResponseAsync(genericResponse, async).ConfigureAwait(false);

					await Connection.AuthBlock.SendWireCryptToBufferAsync(Database.Xdr, async).ConfigureAwait(false);
					await Database.Xdr.FlushAsync(async).ConfigureAwait(false);
					await Connection.AuthBlock.ProcessWireCryptResponseAsync(Database.Xdr, Connection, async).ConfigureAwait(false);

					if (genericResponse.Data.Any())
					{
						await Database.AuthBlock.SendWireCryptToBufferAsync(Database.Xdr, async).ConfigureAwait(false);
						await Database.Xdr.FlushAsync(async).ConfigureAwait(false);
						await Database.AuthBlock.ProcessWireCryptResponseAsync(Database.Xdr, Connection, async).ConfigureAwait(false);
					}
				}
				else
				{
					response = await (Database as GdsDatabase).ProcessCryptCallbackResponseIfNeededAsync(response, cryptKey, async).ConfigureAwait(false);
					await ProcessAttachResponseAsync((GenericResponse)response, async).ConfigureAwait(false);
					Database.AuthBlock.Complete();
				}
			}
			catch (IscException)
			{
				await Database.SafelyDetachAsync(async).ConfigureAwait(false);
				throw;
			}
			catch (IOException ex)
			{
				await Database.SafelyDetachAsync(async).ConfigureAwait(false);
				throw IscException.ForIOException(ex);
			}
		}

		public override ServiceParameterBufferBase CreateServiceParameterBuffer()
		{
			return new ServiceParameterBuffer3();
		}

		protected override Version10.GdsDatabase CreateDatabase(GdsConnection connection)
		{
			return new GdsDatabase(connection);
		}
	}
}
