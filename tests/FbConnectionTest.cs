//
// Firebird .NET Data Provider - Firebird managed data provider for .NET and Mono
// Copyright (C) 2002-2003  Carlos Guzman Alvarez
//
// Distributable under LGPL license.
// You may obtain a copy of the License at http://www.gnu.org/copyleft/lesser.html
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//

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
		public FbConnection conn;
		
		public FbConnectionTest() : base()
		{
			conn = new FbConnection();			
			conn.ConnectionString = GetConnectionString();
			conn.StateChange += new StateChangeEventHandler(OnStateChange);
		}
		
		[Test]
		public void CreateDatabaseTest()
		{
			// FbConnection.CreateDatabase("localhost", 3050, @"c:\test.FDB", "SYSDBA", "masterkey", 3, false, 16384, "ISO8859_1");
		}

		[Test]
		public void OpenConnectionTest()
		{			
			conn.Open();			
		}
		
		[Test]
		public void BeginTrasactionTest()
		{			
			FbConnection conn01 = new FbConnection(GetConnectionString());
			conn01.Open();
			FbTransaction txn01 = conn01.BeginTransaction(IsolationLevel.Unspecified);
			conn01.Close();

			FbConnection conn02 = new FbConnection(GetConnectionString());
			conn02.Open();
			FbTransaction txn02 = conn02.BeginTransaction(IsolationLevel.ReadCommitted);
			conn02.Close();

			FbConnection conn03 = new FbConnection(GetConnectionString());
			conn03.Open();
			FbTransaction txn03 = conn03.BeginTransaction(IsolationLevel.ReadUncommitted);
			conn03.Close();

			FbConnection conn04 = new FbConnection(GetConnectionString());
			conn04.Open();
			FbTransaction txn04 = conn04.BeginTransaction(IsolationLevel.RepeatableRead);
			conn04.Close();
			
			FbConnection conn05 = new FbConnection(GetConnectionString());
			conn05.Open();
			FbTransaction txn05 = conn05.BeginTransaction(IsolationLevel.Serializable);
			conn05.Close();			
		}
		
		[Test]
		public void CreateCommandTest()
		{
			FbCommand command = conn.CreateCommand();

			Assertion.AssertEquals(command.Connection, conn);
		}

		[Test]		
		public void CloseConnectionTest()
		{			
			conn.Close();
		}

		[Test]
		public void ConnectionPoolingTest()
		{
			FbConnection myConnection1 = new FbConnection(GetConnectionString());
			FbConnection myConnection2 = new FbConnection(GetConnectionString());
			FbConnection myConnection3 = new FbConnection(GetConnectionString());

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
					return conn.BeginTransaction();
				
				default:
					return conn.BeginTransaction(level);
			}
		}		
	}
}
