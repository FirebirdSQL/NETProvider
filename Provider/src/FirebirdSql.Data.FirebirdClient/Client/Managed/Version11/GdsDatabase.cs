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
			DeferredPackets = new Queue<Func<IResponse, AsyncWrappingCommonArgs, ValueTask>>();
		}

		public Queue<Func<IResponse, AsyncWrappingCommonArgs, ValueTask>> DeferredPackets { get; private set; }

		public override StatementBase CreateStatement()
		{
			return new GdsStatement(this);
		}

		public override StatementBase CreateStatement(TransactionBase transaction)
		{
			return new GdsStatement(this, transaction);
		}

		public override async ValueTask AttachWithTrustedAuthAsync(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey, AsyncWrappingCommonArgs async)
		{
			try
			{
				using (var sspiHelper = new SspiHelper())
				{
					var authData = sspiHelper.InitializeClientSecurity();
					await SendTrustedAuthToBufferAsync(dpb, authData, async).ConfigureAwait(false);
					await SendAttachToBufferAsync(dpb, database, async).ConfigureAwait(false);
					await Xdr.FlushAsync(async).ConfigureAwait(false);

					var response = await ReadResponseAsync(async).ConfigureAwait(false);
					response = await ProcessTrustedAuthResponseAsync(sspiHelper, response, async).ConfigureAwait(false);
					await ProcessAttachResponseAsync((GenericResponse)response, async).ConfigureAwait(false);
				}
			}
			catch (IscException)
			{
				await SafelyDetachAsync(async).ConfigureAwait(false);
				throw;
			}
			catch (IOException ex)
			{
				await SafelyDetachAsync(async).ConfigureAwait(false);
				throw IscException.ForIOException(ex);
			}

			await AfterAttachActionsAsync(async).ConfigureAwait(false);
		}

		protected virtual ValueTask SendTrustedAuthToBufferAsync(DatabaseParameterBufferBase dpb, byte[] authData, AsyncWrappingCommonArgs async)
		{
			dpb.Append(IscCodes.isc_dpb_trusted_auth, authData);
			return ValueTask2.CompletedTask;
		}

		protected async ValueTask<IResponse> ProcessTrustedAuthResponseAsync(SspiHelper sspiHelper, IResponse response, AsyncWrappingCommonArgs async)
		{
			while (response is AuthResponse)
			{
				var authData = sspiHelper.GetClientSecurity(((AuthResponse)response).Data);
				await Xdr.WriteAsync(IscCodes.op_trusted_auth, async).ConfigureAwait(false);
				await Xdr.WriteBufferAsync(authData, async).ConfigureAwait(false);
				await Xdr.FlushAsync(async).ConfigureAwait(false);
				response = await ReadResponseAsync(async).ConfigureAwait(false);
			}
			return response;
		}

		public override async ValueTask CreateDatabaseWithTrustedAuthAsync(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey, AsyncWrappingCommonArgs async)
		{
			using (var sspiHelper = new SspiHelper())
			{
				var authData = sspiHelper.InitializeClientSecurity();
				await SendTrustedAuthToBufferAsync(dpb, authData, async).ConfigureAwait(false);
				await SendCreateToBufferAsync(dpb, database, async).ConfigureAwait(false);
				await Xdr.FlushAsync(async).ConfigureAwait(false);

				var response = await ReadResponseAsync(async).ConfigureAwait(false);
				response = await ProcessTrustedAuthResponseAsync(sspiHelper, response, async).ConfigureAwait(false);
				await ProcessCreateResponseAsync((GenericResponse)response, async).ConfigureAwait(false);
			}
		}

		public override async ValueTask ReleaseObjectAsync(int op, int id, AsyncWrappingCommonArgs async)
		{
			try
			{
				await SendReleaseObjectToBufferAsync(op, id, async).ConfigureAwait(false);
				DeferredPackets.Enqueue(ProcessReleaseObjectResponseAsync);
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}

		public override async ValueTask<int> ReadOperationAsync(AsyncWrappingCommonArgs async)
		{
			await ProcessDeferredPacketsAsync(async).ConfigureAwait(false);
			return await base.ReadOperationAsync(async).ConfigureAwait(false);
		}

		private async ValueTask ProcessDeferredPacketsAsync(AsyncWrappingCommonArgs async)
		{
			if (DeferredPackets.Count > 0)
			{
				// copy it to local collection and clear to not get same processing when the method is hit again from ReadSingleResponse
				var methods = DeferredPackets.ToArray();
				DeferredPackets.Clear();
				foreach (var method in methods)
				{
					var response = await ReadSingleResponseAsync(async).ConfigureAwait(false);
					await method(response, async).ConfigureAwait(false);
				}
			}
		}
	}
}
