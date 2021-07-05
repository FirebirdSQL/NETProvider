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
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version13
{
	internal class GdsServiceManager : Version12.GdsServiceManager
	{
		public GdsServiceManager(GdsConnection connection)
			: base(connection)
		{ }

		public override async Task Attach(ServiceParameterBufferBase spb, string dataSource, int port, string service, byte[] cryptKey, AsyncWrappingCommonArgs async)
		{
			try
			{
				await SendAttachToBuffer(spb, service, async).ConfigureAwait(false);
				await Database.Xdr.Flush(async).ConfigureAwait(false);
				var response = await Database.ReadResponse(async).ConfigureAwait(false);
				if (response is ContAuthResponse)
				{
					while (response is ContAuthResponse contAuthResponse)
					{
						Connection.AuthBlock.Start(contAuthResponse.ServerData, contAuthResponse.AcceptPluginName, contAuthResponse.IsAuthenticated, contAuthResponse.ServerKeys);

						await Connection.AuthBlock.SendContAuthToBuffer(Database.Xdr, async).ConfigureAwait(false);
						await Database.Xdr.Flush(async).ConfigureAwait(false);
						response = await Connection.AuthBlock.ProcessContAuthResponse(Database.Xdr, async).ConfigureAwait(false);
					}
#warning ProcessCryptCallbackResponseIfNeeded
					await ProcessAttachResponse((GenericResponse)response, async).ConfigureAwait(false);

					await Connection.AuthBlock.SendWireCryptToBuffer(Database.Xdr, async).ConfigureAwait(false);
					await Database.Xdr.Flush(async).ConfigureAwait(false);
					await Connection.AuthBlock.ProcessWireCryptResponse(Database.Xdr, Connection, async).ConfigureAwait(false);
				}
				else
				{
					response = await (Database as GdsDatabase).ProcessCryptCallbackResponseIfNeeded(response, cryptKey, async).ConfigureAwait(false);
					await ProcessAttachResponse((GenericResponse)response, async).ConfigureAwait(false);
				}
			}
			catch (IscException)
			{
				await Database.SafelyDetach(async).ConfigureAwait(false);
				throw;
			}
			catch (IOException ex)
			{
				await Database.SafelyDetach(async).ConfigureAwait(false);
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
