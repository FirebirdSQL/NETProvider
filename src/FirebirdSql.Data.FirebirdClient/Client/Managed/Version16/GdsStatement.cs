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
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version16;

internal class GdsStatement : Version15.GdsStatement
{
	public GdsStatement(GdsDatabase database)
		: base(database)
	{ }

	public GdsStatement(GdsDatabase database, Version10.GdsTransaction transaction)
		: base(database, transaction)
	{ }

	protected override void SendExecuteToBuffer(int timeout, IDescriptorFiller descriptorFiller)
	{
		base.SendExecuteToBuffer(timeout, descriptorFiller);
		_database.Xdr.Write(timeout);
	}

	protected override async ValueTask SendExecuteToBufferAsync(int timeout, IDescriptorFiller descriptorFiller, CancellationToken cancellationToken = default)
	{
		await base.SendExecuteToBufferAsync(timeout, descriptorFiller, cancellationToken).ConfigureAwait(false);
		await _database.Xdr.WriteAsync(timeout, cancellationToken).ConfigureAwait(false);
	}

	public override BatchBase CreateBatch()
	{
		return new GdsBatch(this);
	}

	public override BatchParameterBuffer CreateBatchParameterBuffer()
	{
		return new BatchParameterBuffer(Database.Charset.Encoding);
	}
}
