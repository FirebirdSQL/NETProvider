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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Threading;
using System.Transactions;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.FirebirdClient;

internal sealed class FbEnlistmentNotification : IEnlistmentNotification
{
	#region Events

	public event EventHandler Completed;

	#endregion

	#region Fields

	private FbConnectionInternal _connection;
	private FbTransaction _transaction;
	private Transaction _systemTransaction;

	#endregion

	#region Properties

	public bool IsCompleted
	{
		get { return (_transaction == null); }
	}

	public Transaction SystemTransaction
	{
		get { return _systemTransaction; }
	}

	#endregion

	#region Constructors

	public FbEnlistmentNotification(FbConnectionInternal connection, Transaction systemTransaction)
	{
		_connection = connection;
		_transaction = connection.BeginTransaction(systemTransaction.IsolationLevel);
		_systemTransaction = systemTransaction;

		_systemTransaction.EnlistVolatile(this, EnlistmentOptions.None);
	}

	#endregion

	#region IEnlistmentNotification Members

	public void Commit(Enlistment enlistment)
	{
		if (_transaction != null && !_transaction.IsCompleted)
		{
			_transaction.Commit();
			_transaction = null;

			Completed?.Invoke(this, new EventArgs());

			if (_connection != null)
			{
				if (!_connection.ConnectionStringOptions.Pooling && (_connection.OwningConnection == null || _connection.OwningConnection.IsClosed))
				{
					_connection.Disconnect();
				}
			}
			_connection = null;
			_systemTransaction = null;

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
		if (_transaction != null && !_transaction.IsCompleted)
		{
			_transaction.Rollback();
			_transaction = null;

			Completed?.Invoke(this, new EventArgs());

			if (_connection != null)
			{
				if (!_connection.ConnectionStringOptions.Pooling && (_connection.OwningConnection == null || _connection.OwningConnection.IsClosed))
				{
					_connection.Disconnect();
				}
			}
			_connection = null;
			_systemTransaction = null;

			// Declare done on the enlistment
			enlistment.Done();
		}
	}

	#endregion
}
