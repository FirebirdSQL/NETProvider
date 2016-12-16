/*
 *	Firebird ADO.NET Data provider for .NET and Mono
 *
 *	   The contents of this file are subject to the Initial
 *	   Developer's Public License Version 1.0 (the "License");
 *	   you may not use this file except in compliance with the
 *	   License. You may obtain a copy of the License at
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *	   express or implied. See the License for the specific
 *	   language governing rights and limitations under the License.
 *
 *	Copyright (c) 2015 Jiri Cincura (jiri@cincura.net)
 *	All Rights Reserved.
 */

using System;
using FirebirdSql.Data.Client.Managed;

namespace FirebirdSql.Data.Common
{
	internal abstract class TransactionBase : IDisposable
	{
		protected void EnsureActiveTransactionState()
		{
			if (State != TransactionState.Active)
			{
				throw IscException.ForTypeErrorCodeIntParamStrParam(IscCodes.isc_arg_gds, IscCodes.isc_tra_state, Handle, "no valid");
			}
		}

		public abstract int Handle { get; }
		public abstract TransactionState State { get; }

		public abstract event EventHandler Update;

		public abstract void BeginTransaction(TransactionParameterBuffer tpb);
		public abstract void Commit();
		public abstract void CommitRetaining();
		public abstract void Rollback();
		public abstract void RollbackRetaining();
		public abstract void Prepare();
		public abstract void Prepare(byte[] buffer);

		public virtual void Dispose()
		{ }
	}
}
