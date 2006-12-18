/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.ibphoenix.com/main.nfs?a=ibphoenix&l=;PAGES;NAME='ibp_idpl'
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using NUnit.Framework;
using System;
using System.Data;
using System.Reflection;
using FirebirdSql.Data.Firebird;

namespace FirebirdSql.Data.Firebird.Tests
{
	[TestFixture]
	public class FbConnectionTest : BaseTest 
	{
		public FbConnectionTest() : base(false)
		{
		}
				
		[Test]
		public void BeginTrasactionTest()
		{			
			FbConnection conn01 = new FbConnection(Connection.ConnectionString);
			conn01.Open();
			FbTransaction txn01 = conn01.BeginTransaction(IsolationLevel.Unspecified);
			conn01.Close();

			FbConnection conn02 = new FbConnection(Connection.ConnectionString);
			conn02.Open();
			FbTransaction txn02 = conn02.BeginTransaction(IsolationLevel.ReadCommitted);
			conn02.Close();

			FbConnection conn03 = new FbConnection(Connection.ConnectionString);
			conn03.Open();
			FbTransaction txn03 = conn03.BeginTransaction(IsolationLevel.ReadUncommitted);
			conn03.Close();

			FbConnection conn04 = new FbConnection(Connection.ConnectionString);
			conn04.Open();
			FbTransaction txn04 = conn04.BeginTransaction(IsolationLevel.RepeatableRead);
			conn04.Close();
			
			FbConnection conn05 = new FbConnection(Connection.ConnectionString);
			conn05.Open();
			FbTransaction txn05 = conn05.BeginTransaction(IsolationLevel.Serializable);
			conn05.Close();			
		}
		
		[Test]
		public void CreateCommandTest()
		{
			FbCommand command = Connection.CreateCommand();

			Assert.AreEqual(command.Connection, Connection);
		}

		[Test]
		public void ConnectionPoolingTest()
		{
			FbConnection myConnection1 = new FbConnection(Connection.ConnectionString);
			FbConnection myConnection2 = new FbConnection(Connection.ConnectionString);
			FbConnection myConnection3 = new FbConnection(Connection.ConnectionString);

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
						
		public FbTransaction BeginTransaction(IsolationLevel level)
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
