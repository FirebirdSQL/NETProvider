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
 *	Copyright (c) 2006 Carlos Guzman Alvarez
 *	All Rights Reserved.
 * 
 *  Contributors:
 *      Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Transactions;

namespace FirebirdSql.Data.FirebirdClient
{
	internal sealed class FbEnlistmentNotification : IEnlistmentNotification
	{
		#region Events

		public event EventHandler Completed;

		#endregion

		#region Fields

		private FbConnectionInternal    connection;
		private FbTransaction           transaction;
		private Transaction             systemTransaction;

		#endregion

		#region Properties

		public bool IsCompleted
		{
			get { return (this.transaction == null); }
		}

		public Transaction SystemTransaction
		{
			get { return this.systemTransaction; }
		}

		#endregion

		#region Constructors

		public FbEnlistmentNotification(FbConnectionInternal connection, Transaction systemTransaction)
		{            
			this.connection         = connection;
			this.transaction        = connection.BeginTransaction(systemTransaction.IsolationLevel);
			this.systemTransaction  = systemTransaction;

			this.systemTransaction.EnlistVolatile(this, System.Transactions.EnlistmentOptions.None);
		}

		#endregion

		#region IEnlistmentNotification Members

		public void Commit(Enlistment enlistment)
		{
			if (this.transaction != null && !this.transaction.IsUpdated)
			{
				this.transaction.Commit();
				this.transaction = null;

				if (this.Completed != null)
				{
					this.Completed(this, new EventArgs());
				}

				if (this.connection != null)
				{
					if (!this.connection.Options.Pooling && (this.connection.OwningConnection == null || this.connection.OwningConnection.IsClosed))
					{
						this.connection.Disconnect();
					}
				}
				this.connection         = null;
				this.systemTransaction  = null;

				// Declare done on the enlistment
				enlistment.Done();
			}
		}

		public void InDoubt(Enlistment enlistment)
		{
			throw new NotSupportedException("In Doubt transactions are not supported");
		}

		public void Prepare(PreparingEnlistment preparingEnlistment)
		{
			preparingEnlistment.Prepared();
		}

		public void Rollback(Enlistment enlistment)
		{
			if (this.transaction != null && !this.transaction.IsUpdated)
			{
				this.transaction.Rollback();
				this.transaction        = null;

				if (this.Completed != null)
				{
					this.Completed(this, new EventArgs());
				}

				if (this.connection != null)
				{
					if (!this.connection.Options.Pooling && (this.connection.OwningConnection == null || this.connection.OwningConnection.IsClosed))
					{
						this.connection.Disconnect();
					}
				}
				this.connection = null;
				this.systemTransaction  = null;

				// Declare done on the enlistment
				enlistment.Done();
			}
		}

		#endregion
	}
}
