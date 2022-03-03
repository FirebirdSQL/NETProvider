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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version12;

internal class GdsDatabase : Version11.GdsDatabase
{
	public override bool UseUtf8ParameterBuffer => true;

	public GdsDatabase(GdsConnection connection)
		: base(connection)
	{ }

	public override StatementBase CreateStatement()
	{
		return new GdsStatement(this);
	}

	public override StatementBase CreateStatement(TransactionBase transaction)
	{
		return new GdsStatement(this, (Version10.GdsTransaction)transaction);
	}

	public override void CancelOperation(short kind)
	{
		try
		{
			SendCancelOperationToBuffer(kind);
			Xdr.Flush();
			// no response, this is out-of-band
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	public override async ValueTask CancelOperationAsync(short kind, CancellationToken cancellationToken = default)
	{
		try
		{
			await SendCancelOperationToBufferAsync(kind, cancellationToken).ConfigureAwait(false);
			await Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
			// no response, this is out-of-band
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	protected void SendCancelOperationToBuffer(short kind)
	{
		Xdr.Write(IscCodes.op_cancel);
		Xdr.Write(kind);
	}
	protected async ValueTask SendCancelOperationToBufferAsync(int kind, CancellationToken cancellationToken = default)
	{
		await Xdr.WriteAsync(IscCodes.op_cancel, cancellationToken).ConfigureAwait(false);
		await Xdr.WriteAsync(kind, cancellationToken).ConfigureAwait(false);
	}
}
