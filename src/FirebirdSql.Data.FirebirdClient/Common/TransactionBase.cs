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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FirebirdSql.Data.Common;

internal abstract class TransactionBase
{
	public abstract int Handle { get; }

	public TransactionState State { get; protected set; }
	public event EventHandler Update;

	public abstract void BeginTransaction(TransactionParameterBuffer tpb);
	public abstract ValueTask BeginTransactionAsync(TransactionParameterBuffer tpb, CancellationToken cancellationToken = default);

	public abstract void Commit();
	public abstract ValueTask CommitAsync(CancellationToken cancellationToken = default);

	public abstract void CommitRetaining();
	public abstract ValueTask CommitRetainingAsync(CancellationToken cancellationToken = default);

	public abstract void Rollback();
	public abstract ValueTask RollbackAsync(CancellationToken cancellationToken = default);

	public abstract void RollbackRetaining();
	public abstract ValueTask RollbackRetainingAsync(CancellationToken cancellationToken = default);

	public abstract void Prepare();
	public abstract ValueTask PrepareAsync(CancellationToken cancellationToken = default);

	public abstract void Prepare(byte[] buffer);
	public abstract ValueTask PrepareAsync(byte[] buffer, CancellationToken cancellationToken = default);

	public abstract List<object> GetTransactionInfo(byte[] items);
	public abstract ValueTask<List<object>> GetTransactionInfoAsync(byte[] items, CancellationToken cancellationToken = default);

	public abstract List<object> GetTransactionInfo(byte[] items, int bufferLength);
	public abstract ValueTask<List<object>> GetTransactionInfoAsync(byte[] items, int bufferLength, CancellationToken cancellationToken = default);

	public virtual void Dispose2()
	{ }
	public virtual ValueTask Dispose2Async(CancellationToken cancellationToken = default)
	{
		return ValueTask2.CompletedTask;
	}

	protected void EnsureActiveTransactionState()
	{
		if (State != TransactionState.Active)
			throw IscException.ForTypeErrorCodeIntParamStrParam(IscCodes.isc_arg_gds, IscCodes.isc_tra_state, Handle, "no valid");
	}

	protected void OnUpdate(EventArgs e)
	{
		Update?.Invoke(this, e);
	}
}
