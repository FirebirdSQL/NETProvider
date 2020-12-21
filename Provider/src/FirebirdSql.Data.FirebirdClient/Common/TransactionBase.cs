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
		protected void EnsureActiveTransactionState()
		{
			if (State != TransactionState.Active)
			{
				throw IscException.ForTypeErrorCodeIntParamStrParam(IscCodes.isc_arg_gds, IscCodes.isc_tra_state, Handle, "no valid");
			}
		}

#warning These do not have to be abstract, probably
		public abstract int Handle { get; }
		public abstract TransactionState State { get; }

#warning This does not have to be abstract, probably
		public abstract event EventHandler Update;

		public abstract Task BeginTransaction(TransactionParameterBuffer tpb, AsyncWrappingCommonArgs async);
		public abstract Task Commit(AsyncWrappingCommonArgs async);
		public abstract Task CommitRetaining(AsyncWrappingCommonArgs async);
		public abstract Task Rollback(AsyncWrappingCommonArgs async);
		public abstract Task RollbackRetaining(AsyncWrappingCommonArgs async);
		public abstract Task Prepare(AsyncWrappingCommonArgs async);
		public abstract Task Prepare(byte[] buffer, AsyncWrappingCommonArgs async);

#warning Find better name
		public virtual Task Dispose2(AsyncWrappingCommonArgs async) => Task.CompletedTask;
	}
}
