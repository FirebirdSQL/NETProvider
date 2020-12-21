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

//$Authors = Hajime Nakagami, Jiri Cincura (jiri@cincura.net)

using System.IO;
using System.Text;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version13
{
	internal class GdsDatabase : Version12.GdsDatabase
	{
		public GdsDatabase(GdsConnection connection)
			: base(connection)
		{ }

		public override async Task Attach(DatabaseParameterBufferBase dpb, string dataSource, int port, string database, byte[] cryptKey, AsyncWrappingCommonArgs async)
		{
			try
			{
				await SendAttachToBuffer(dpb, database, async).ConfigureAwait(false);
				await Xdr.Flush(async).ConfigureAwait(false);
				var response = await ReadResponse(async).ConfigureAwait(false);
				response = await ProcessCryptCallbackResponseIfNeeded(response, cryptKey, async).ConfigureAwait(false);
				await ProcessAttachResponse((GenericResponse)response, async).ConfigureAwait(false);
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

		protected override async Task SendAttachToBuffer(DatabaseParameterBufferBase dpb, string database, AsyncWrappingCommonArgs async)
		{
			await Xdr.Write(IscCodes.op_attach, async).ConfigureAwait(false);
			await Xdr.Write(0, async).ConfigureAwait(false);
			if (AuthData != null)
			{
				dpb.Append(IscCodes.isc_dpb_specific_auth_data, AuthData);
			}
			dpb.Append(IscCodes.isc_dpb_utf8_filename, 0);
			await Xdr.WriteBuffer(Encoding.UTF8.GetBytes(database), async).ConfigureAwait(false);
			await Xdr.WriteBuffer(dpb.ToArray(), async).ConfigureAwait(false);
		}

		public override async Task CreateDatabase(DatabaseParameterBufferBase dpb, string dataSource, int port, string database, byte[] cryptKey, AsyncWrappingCommonArgs async)
		{

			try
			{
				await SendCreateToBuffer(dpb, database, async).ConfigureAwait(false);
				await Xdr.Flush(async).ConfigureAwait(false);
				var response = await ReadResponse(async).ConfigureAwait(false);
				response = await ProcessCryptCallbackResponseIfNeeded(response, cryptKey, async).ConfigureAwait(false);
				await ProcessCreateResponse((GenericResponse)response, async).ConfigureAwait(false);
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		protected override async Task SendCreateToBuffer(DatabaseParameterBufferBase dpb, string database, AsyncWrappingCommonArgs async)
		{
			await Xdr.Write(IscCodes.op_create, async).ConfigureAwait(false);
			await Xdr.Write(0, async).ConfigureAwait(false);
			if (AuthData != null)
			{
				dpb.Append(IscCodes.isc_dpb_specific_auth_data, AuthData);
			}
			dpb.Append(IscCodes.isc_dpb_utf8_filename, 0);
			await Xdr.WriteBuffer(Encoding.UTF8.GetBytes(database), async).ConfigureAwait(false);
			await Xdr.WriteBuffer(dpb.ToArray(), async).ConfigureAwait(false);
		}

		public override Task AttachWithTrustedAuth(DatabaseParameterBufferBase dpb, string dataSource, int port, string database, byte[] cryptKey, AsyncWrappingCommonArgs async)
		{
			return Attach(dpb, dataSource, port, database, cryptKey, async);
		}

		public override Task CreateDatabaseWithTrustedAuth(DatabaseParameterBufferBase dpb, string dataSource, int port, string database, byte[] cryptKey, AsyncWrappingCommonArgs async)
		{
			return CreateDatabase(dpb, dataSource, port, database, cryptKey, async);
		}

		internal async Task<IResponse> ProcessCryptCallbackResponseIfNeeded(IResponse response, byte[] cryptKey, AsyncWrappingCommonArgs async)
		{
			while (response is CryptKeyCallbackResponse cryptResponse)
			{
				await Xdr.Write(IscCodes.op_crypt_key_callback, async).ConfigureAwait(false);
				await Xdr.WriteBuffer(cryptKey, async).ConfigureAwait(false);
				await Xdr.Flush(async).ConfigureAwait(false);
				response = await ReadResponse(async).ConfigureAwait(false);
			}
			return response;
		}

		public override StatementBase CreateStatement()
		{
			return new GdsStatement(this);
		}

		public override StatementBase CreateStatement(TransactionBase transaction)
		{
			return new GdsStatement(this, transaction);
		}

		public override DatabaseParameterBufferBase CreateDatabaseParameterBuffer()
		{
			return new DatabaseParameterBuffer2();
		}
	}
}
