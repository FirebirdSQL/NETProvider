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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net), Vladimir Bodecek

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version11
{
	internal class GdsDatabase : Version10.GdsDatabase
	{
		public GdsDatabase(GdsConnection connection)
			: base(connection)
		{
			DeferredPackets = new Queue<Func<IResponse, AsyncWrappingCommonArgs, Task>>();
		}

		public Queue<Func<IResponse, AsyncWrappingCommonArgs, Task>> DeferredPackets { get; private set; }

		public override StatementBase CreateStatement()
		{
			return new GdsStatement(this);
		}

		public override StatementBase CreateStatement(TransactionBase transaction)
		{
			return new GdsStatement(this, transaction);
		}

		public override async Task AttachWithTrustedAuth(DatabaseParameterBufferBase dpb, string dataSource, int port, string database, byte[] cryptKey, AsyncWrappingCommonArgs async)
		{
			try
			{
				using (var sspiHelper = new SspiHelper())
				{
					var authData = sspiHelper.InitializeClientSecurity();
					await SendTrustedAuthToBuffer(dpb, authData, async).ConfigureAwait(false);
					await SendAttachToBuffer(dpb, database, async).ConfigureAwait(false);
					await Xdr.Flush(async).ConfigureAwait(false);

					var response = await ReadResponse(async).ConfigureAwait(false);
					response = await ProcessTrustedAuthResponse(sspiHelper, response, async).ConfigureAwait(false);
					await ProcessAttachResponse((GenericResponse)response, async).ConfigureAwait(false);
				}
			}
			catch (IscException)
			{
				await SafelyDetach(async).ConfigureAwait(false);
				throw;
			}
			catch (IOException ex)
			{
				await SafelyDetach(async).ConfigureAwait(false);
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}

			await AfterAttachActions(async).ConfigureAwait(false);
		}

		protected virtual Task SendTrustedAuthToBuffer(DatabaseParameterBufferBase dpb, byte[] authData, AsyncWrappingCommonArgs async)
		{
			dpb.Append(IscCodes.isc_dpb_trusted_auth, authData);
			return Task.CompletedTask;
		}

		protected async Task<IResponse> ProcessTrustedAuthResponse(SspiHelper sspiHelper, IResponse response, AsyncWrappingCommonArgs async)
		{
			while (response is AuthResponse)
			{
				var authData = sspiHelper.GetClientSecurity(((AuthResponse)response).Data);
				await Xdr.Write(IscCodes.op_trusted_auth, async).ConfigureAwait(false);
				await Xdr.WriteBuffer(authData, async).ConfigureAwait(false);
				await Xdr.Flush(async).ConfigureAwait(false);
				response = await ReadResponse(async).ConfigureAwait(false);
			}
			return response;
		}

		public override async Task CreateDatabaseWithTrustedAuth(DatabaseParameterBufferBase dpb, string dataSource, int port, string database, byte[] cryptKey, AsyncWrappingCommonArgs async)
		{
			using (var sspiHelper = new SspiHelper())
			{
				var authData = sspiHelper.InitializeClientSecurity();
				await SendTrustedAuthToBuffer(dpb, authData, async).ConfigureAwait(false);
				await SendCreateToBuffer(dpb, database, async).ConfigureAwait(false);
				await Xdr.Flush(async).ConfigureAwait(false);

				var response = await ReadResponse(async).ConfigureAwait(false);
				response = await ProcessTrustedAuthResponse(sspiHelper, response, async).ConfigureAwait(false);
				await ProcessCreateResponse((GenericResponse)response, async).ConfigureAwait(false);
			}
		}

		public override async Task ReleaseObject(int op, int id, AsyncWrappingCommonArgs async)
		{
			try
			{
				await SendReleaseObjectToBuffer(op, id, async).ConfigureAwait(false);
				DeferredPackets.Enqueue(ProcessReleaseObjectResponse);
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		public override async Task<int> ReadOperation(AsyncWrappingCommonArgs async)
		{
			await ProcessDeferredPackets(async).ConfigureAwait(false);
			return await base.ReadOperation(async).ConfigureAwait(false);
		}

		private async Task ProcessDeferredPackets(AsyncWrappingCommonArgs async)
		{
			if (DeferredPackets.Count > 0)
			{
				// copy it to local collection and clear to not get same processing when the method is hit again from ReadSingleResponse
				var methods = DeferredPackets.ToArray();
				DeferredPackets.Clear();
				foreach (var method in methods)
				{
					var response = await ReadSingleResponse(async).ConfigureAwait(false);
					await method(response, async).ConfigureAwait(false);
				}
			}
		}
	}
}
