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

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Threading.Tasks;

namespace FirebirdSql.Data.Common
{
	internal abstract class TransactionBase
	{
		public abstract int Handle { get; }

		public TransactionState State { get; protected set; }
		public event EventHandler Update;

		public abstract ValueTask BeginTransaction(TransactionParameterBuffer tpb, AsyncWrappingCommonArgs async);
		public abstract ValueTask Commit(AsyncWrappingCommonArgs async);
		public abstract ValueTask CommitRetaining(AsyncWrappingCommonArgs async);
		public abstract ValueTask Rollback(AsyncWrappingCommonArgs async);
		public abstract ValueTask RollbackRetaining(AsyncWrappingCommonArgs async);
		public abstract ValueTask Prepare(AsyncWrappingCommonArgs async);
		public abstract ValueTask Prepare(byte[] buffer, AsyncWrappingCommonArgs async);

		public virtual ValueTask Dispose2(AsyncWrappingCommonArgs async) => ValueTask2.CompletedTask;

		protected void EnsureActiveTransactionState()
		{
			if (State != TransactionState.Active)
			{
				throw IscException.ForTypeErrorCodeIntParamStrParam(IscCodes.isc_arg_gds, IscCodes.isc_tra_state, Handle, "no valid");
			}
		}

		protected void OnUpdate(EventArgs e)
		{
			Update?.Invoke(this, e);
		}
	}
}
