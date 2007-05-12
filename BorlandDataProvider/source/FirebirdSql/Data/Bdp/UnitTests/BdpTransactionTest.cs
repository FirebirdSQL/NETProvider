/*
 *  Firebird BDP - Borland Data provider Firebird
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Text;

using NUnit.Framework;
using Borland.Data.Provider;

namespace FirebirdSql.Data.Bdp.Tests
{
	[TestFixture]
	public class BdpTransactionTest : BaseTest 
	{
		public BdpTransactionTest() : base(false)
		{		
		}
				
		[Test]
		public void CommitTest()
		{			
			Transaction = Connection.BeginTransaction();
			Transaction.Commit();
		}
		
		[Test]
		public void RollbackTest()
		{
			Transaction = Connection.BeginTransaction();
			Transaction.Rollback();
		}

		[Test]
		public void AbortTransaction()
		{
			StringBuilder b1 = new StringBuilder();
			b1.AppendFormat("ALTER TABLE \"{0}\" drop \"INT_FIELD\"", "TEST");

			BdpTransaction	transaction = null;
			BdpCommand		command		= null;

			try
			{
				transaction = this.Connection.BeginTransaction();

				command = new BdpCommand(b1.ToString(), this.Connection, transaction);
				command.ExecuteNonQuery();

				transaction.Commit();
				transaction = null;
			}
			catch (Exception)
			{
				transaction.Rollback();
				transaction = null;
			}
			finally
			{
				if (command != null)
				{
					command.Dispose();
				}
			}
		}
	}
}
