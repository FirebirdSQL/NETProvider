/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
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
 *  Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Configuration;
using System.Data;
using System.Reflection;

using FirebirdSql.Data.FirebirdClient;
using NUnit.Framework;

namespace FirebirdSql.Data.UnitTests
{
	[TestFixture]
	public class FbConnectionTests : TestsBase
	{
		#region · Constructors ·

		public FbConnectionTests()
			: base(false)
		{
		}

		#endregion

		#region · Unit Tests ·

		[Test]
		public void BeginTransactionILUnspecifiedTest()
		{
			BeginTransactionILTestsHelper(IsolationLevel.Unspecified);
		}

		[Test]
		public void BeginTransactionILReadCommittedTest()
		{
			BeginTransactionILTestsHelper(IsolationLevel.ReadCommitted);
		}

		[Test]
		public void BeginTransactionILReadUncommittedTest()
		{
			BeginTransactionILTestsHelper(IsolationLevel.ReadUncommitted);
		}

		[Test]
		public void BeginTransactionILRepeatableReadTest()
		{
			BeginTransactionILTestsHelper(IsolationLevel.RepeatableRead);
		}

		[Test]
		public void BeginTransactionILSerializableTest()
		{
			BeginTransactionILTestsHelper(IsolationLevel.Serializable);
		}

		[Test]
		public void BeginTransactionNoWaitTimeoutTest()
		{
			using (FbConnection conn = new FbConnection(this.BuildConnectionString()))
			{
				conn.Open();
				FbTransaction tx = conn.BeginTransaction(new FbTransactionOptions() { WaitTimeout = null });
				Assert.NotNull(tx);
				tx.Rollback();
			}
		}

		[Test]
		public void BeginTransactionWithWaitTimeoutTest()
		{
			using (FbConnection conn = new FbConnection(this.BuildConnectionString()))
			{
				conn.Open();
				FbTransaction tx = conn.BeginTransaction(new FbTransactionOptions() { WaitTimeout = TimeSpan.FromSeconds(10) });
				Assert.NotNull(tx);
				tx.Rollback();
			}
		}

		[Test]
		public void BeginTransactionWithWaitTimeoutInvalidValue1Test()
		{
			using (FbConnection conn = new FbConnection(this.BuildConnectionString()))
			{
				conn.Open();
				Assert.Throws<ArgumentException>(() => conn.BeginTransaction(new FbTransactionOptions() { WaitTimeout = TimeSpan.FromDays(9999) }));
			}
		}

		[Test]
		public void BeginTransactionWithWaitTimeoutInvalidValue2Test()
		{
			using (FbConnection conn = new FbConnection(this.BuildConnectionString()))
			{
				conn.Open();
				Assert.Throws<ArgumentException>(() => conn.BeginTransaction(new FbTransactionOptions() { WaitTimeout = TimeSpan.FromMilliseconds(1) }));
			}
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
			string cs = this.BuildConnectionString(true);

			FbConnection myConnection1 = new FbConnection(cs);
			FbConnection myConnection2 = new FbConnection(cs);
			FbConnection myConnection3 = new FbConnection(cs);

			// Open two connections.
			Console.WriteLine("Open two connections.");
			myConnection1.Open();
			myConnection2.Open();

			// Now there are two connections in the pool that matches the connection string.
			// Return the both connections to the pool. 
			Console.WriteLine("Return both of the connections to the pool.");
			myConnection1.Close();
			myConnection2.Close();

			// Get a connection out of the pool.
			Console.WriteLine("Open a connection from the pool.");
			myConnection1.Open();

			// Get a second connection out of the pool.
			Console.WriteLine("Open a second connection from the pool.");
			myConnection2.Open();

			// Open a third connection.
			Console.WriteLine("Open a third connection.");
			myConnection3.Open();

			// Return the all connections to the pool.  
			Console.WriteLine("Return all three connections to the pool.");
			myConnection1.Close();
			myConnection2.Close();
			myConnection3.Close();

			// Clear pools
			FbConnection.ClearAllPools();
		}

		[Test]
		public void FbConnectionStringBuilderTest()
		{
			FbConnectionStringBuilder cs = new FbConnectionStringBuilder();

			cs.DataSource = ConfigurationManager.AppSettings["DataSource"];
			cs.Database = ConfigurationManager.AppSettings["Database"];
			cs.Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
			cs.UserID = ConfigurationManager.AppSettings["User"];
			cs.Password = ConfigurationManager.AppSettings["Password"];
			cs.ServerType = (FbServerType)Convert.ToInt32(ConfigurationManager.AppSettings["ServerType"]);
			cs.Charset = ConfigurationManager.AppSettings["Charset"];
			cs.Pooling = Convert.ToBoolean(ConfigurationManager.AppSettings["Pooling"]);

			using (FbConnection c = new FbConnection(cs.ToString()))
			{
				c.Open();
			}
		}

		[Test]
		public void ConnectionPoolingTimeOutTest()
		{
			// Using ActiveUsers as proxy for number of connections
			FbConnectionStringBuilder csb = this.BuildConnectionStringBuilder();
			csb.Pooling = true;
			csb.ConnectionLifeTime = 5;
			string cs = csb.ToString();

			int ActiveUsersAtStart = ActiveUserCount();

			using (FbConnection
				myConnection1 = new FbConnection(cs),
				myConnection2 = new FbConnection(cs))
			{
				myConnection1.Open();
				myConnection2.Open();

				myConnection1.Close();
				myConnection2.Close();
			}

			System.Threading.Thread.Sleep(csb.ConnectionLifeTime * 2 * 1000);

			Assert.AreEqual(ActiveUsersAtStart, ActiveUserCount());
		}

		#endregion

		#region · Methods ·

		public FbTransaction BeginTransaction(IsolationLevel level)
		{
			switch (level)
			{
				case IsolationLevel.Unspecified:
					return Connection.BeginTransaction();

				default:
					return Connection.BeginTransaction(level);
			}
		}

		private int ActiveUserCount()
		{
			using (FbConnection dbinfo_connection = new FbConnection(this.BuildConnectionString(false)))
			{
				dbinfo_connection.Open();
				FbDatabaseInfo dbinfo = new FbDatabaseInfo(dbinfo_connection);
				return dbinfo.ActiveUsers.Count;
			}
		}

		private void BeginTransactionILTestsHelper(IsolationLevel level)
		{
			using (FbConnection conn = new FbConnection(this.BuildConnectionString()))
			{
				conn.Open();
				FbTransaction tx = conn.BeginTransaction(level);
				Assert.NotNull(tx);
				tx.Rollback();
			}
		}

		#endregion

		#region · Event Handlers ·

		public void OnStateChange(object sender, StateChangeEventArgs e)
		{
			Console.WriteLine("OnStateChange");
			Console.WriteLine("  event args: (" +
				   "originalState=" + e.OriginalState +
				   " currentState=" + e.CurrentState + ")");
		}

		#endregion
	}
}
