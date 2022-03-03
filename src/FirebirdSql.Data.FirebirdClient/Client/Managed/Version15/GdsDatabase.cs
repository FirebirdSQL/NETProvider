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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version15;

internal class GdsDatabase : Version13.GdsDatabase
{
	public GdsDatabase(GdsConnection connection)
		: base(connection)
	{ }

	protected internal override IResponse ProcessCryptCallbackResponseIfNeeded(IResponse response, byte[] cryptKey)
	{
		while (response is CryptKeyCallbackResponse cryptKeyCallbackResponse)
		{
			Xdr.Write(IscCodes.op_crypt_key_callback);
			Xdr.WriteBuffer(cryptKey);
			Xdr.Write(cryptKeyCallbackResponse.Size);
			Xdr.Flush();
			response = ReadResponse();
		}
		return response;
	}
	protected internal override async ValueTask<IResponse> ProcessCryptCallbackResponseIfNeededAsync(IResponse response, byte[] cryptKey, CancellationToken cancellationToken = default)
	{
		while (response is CryptKeyCallbackResponse cryptKeyCallbackResponse)
		{
			await Xdr.WriteAsync(IscCodes.op_crypt_key_callback, cancellationToken).ConfigureAwait(false);
			await Xdr.WriteBufferAsync(cryptKey, cancellationToken).ConfigureAwait(false);
			await Xdr.WriteAsync(cryptKeyCallbackResponse.Size).ConfigureAwait(false);
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
}
