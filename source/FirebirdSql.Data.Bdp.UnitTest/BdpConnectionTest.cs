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
using System.Data;

using NUnit.Framework;
using Borland.Data.Provider;
using Borland.Data.Schema;

namespace FirebirdSql.Data.Bdp.Tests
{
	[TestFixture]
	public class BdpConnectionTest : BaseTest 
	{
		public BdpConnectionTest() : base(false)
		{
		}
				
		[Test]
		public void BeginTrasactionTest()
		{			
			BdpConnection conn01 = new BdpConnection(Connection.ConnectionString);
			conn01.Open();
			BdpTransaction txn01 = conn01.BeginTransaction(IsolationLevel.Unspecified);
			txn01.Commit();
			conn01.Close();

			BdpConnection conn02 = new BdpConnection(Connection.ConnectionString);
			conn02.Open();
			BdpTransaction txn02 = conn02.BeginTransaction(IsolationLevel.ReadCommitted);
			txn02.Commit();
			conn02.Close();

			BdpConnection conn03 = new BdpConnection(Connection.ConnectionString);
			conn03.Open();
			BdpTransaction txn03 = conn03.BeginTransaction(IsolationLevel.ReadUncommitted);
			txn03.Commit();
			conn03.Close();

			BdpConnection conn04 = new BdpConnection(Connection.ConnectionString);
			conn04.Open();
			BdpTransaction txn04 = conn04.BeginTransaction(IsolationLevel.RepeatableRead);
			txn04.Commit();
			conn04.Close();
			
			BdpConnection conn05 = new BdpConnection(Connection.ConnectionString);
			conn05.Open();
			BdpTransaction txn05 = conn05.BeginTransaction(IsolationLevel.Serializable);
			txn05.Commit();
			conn05.Close();			
		}
		
		[Test]
		public void CreateCommandTest()
		{
			BdpCommand command = Connection.CreateCommand();

			Assert.AreEqual(command.Connection, Connection);
		}

        [Test]
        public void GetTables()
        {
            BdpTransaction t = this.Connection.BeginTransaction();

            DataTable tables = this.Connection.GetMetaData().GetTables("TEST", TableType.Table);

            Assert.AreEqual(1, tables.Rows.Count);

            t.Commit();
        }

        [Test]
		[Ignore("Borland Data Provider doesn't support Connection Pooling yet.")]
		public void ConnectionPoolingTest()
		{
			BdpConnection myConnection1 = new BdpConnection(Connection.ConnectionString);
			BdpConnection myConnection2 = new BdpConnection(Connection.ConnectionString);
			BdpConnection myConnection3 = new BdpConnection(Connection.ConnectionString);

			// Open two connections.
			Console.WriteLine ("Open two connections.");
			myConnection1.Open();
			myConnection2.Open();

			// Now there are two connections in the pool that matches the connection string.
			// Return the both connections to the pool. 
			Console.WriteLine ("Return both of the connections to the pool.");
			myConnection1.Close();
			myConnection2.Close();

			// Get a connection out of the pool.
			Console.WriteLine ("Open a connection from the pool.");
			myConnection1.Open();

			// Get a second connection out of the pool.
			Console.WriteLine ("Open a second connection from the pool.");
			myConnection2.Open();

			// Open a third connection.
			Console.WriteLine ("Open a third connection.");
			myConnection3.Open();

			// Return the all connections to the pool.  
			Console.WriteLine ("Return all three connections to the pool.");
			myConnection1.Close();
			myConnection2.Close();
			myConnection3.Close();
		}

		public void OnStateChange(object sender, StateChangeEventArgs e)
		{		
			Console.WriteLine("OnStateChange");
			Console.WriteLine("  event args: ("+
				   "originalState=" + e.OriginalState +
				   " currentState=" + e.CurrentState +")");
		}
						
		public BdpTransaction BeginTransaction(IsolationLevel level)
		{	
			switch(level)
			{
				case IsolationLevel.Unspecified:
					return Connection.BeginTransaction();
				
				default:
					return Connection.BeginTransaction(level);
			}
		}		
	}
}
