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
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.UnitTest
{
	[TestFixture]
	public class StatementTest : BaseTest
	{
		[Test]
		public void Allocate()
		{
			// Start a new Transaction
			ITransaction transaction = this.Attachment.BeginTransaction(BaseTest.BuildTpb());

			// Create a new statement
			StatementBase stmt = this.Attachment.CreateStatement(transaction);

			// Prepare the statement
			stmt.Prepare("SELECT * FROM TEST");

			// Drop the Statement
			stmt.Release();

			// Commit the transaction
			transaction.Commit();
		}

		[Test]
		public void Close()
		{
			// Start a new Transaction
			ITransaction transaction = this.Attachment.BeginTransaction(BaseTest.BuildTpb());

			// Create a new statement
			StatementBase stmt = this.Attachment.CreateStatement(transaction);

			// Prepare the statement
			stmt.Prepare("SELECT * FROM TEST");

			// Drop the Statement
			stmt.Close();

			// Commit the transaction
			transaction.Commit();
		}

		[Test]
		public void Drop()
		{
			// Start a new Transaction
			ITransaction transaction = this.Attachment.BeginTransaction(BaseTest.BuildTpb());

			// Create a new statement
			StatementBase stmt = this.Attachment.CreateStatement(transaction);

			// Prepare the statement
			stmt.Prepare("SELECT * FROM TEST");

			// Drop the Statement
			stmt.Release();

			// Commit the transaction
			transaction.Commit();
		}

		[Test]
		public void Describe()
		{
			string message1 = "Differences betwwen actual and fetched fields received from server in response to a Describe call";
			string message2 = "Invalid number of fields received from the server in response to a Describe call.";
			string message3 = "Invalid field type received from the server in response to a Describe call.";
			string message4 = "Invalid field name received from the server in response to a Describe call.";
			string message5 = "Invalid table name received from the server in response to a Describe call.";
			string sql		= "select int_field, char_field, date_field from TEST";

			// Start a new Transaction
			ITransaction transaction = this.Attachment.BeginTransaction(BaseTest.BuildTpb());

			// Create a new statement
			StatementBase stmt = this.Attachment.CreateStatement(transaction);
				
			// Prepare the statement
			stmt.Prepare(sql);

			// Check Statement Type
			Assert.AreEqual(DbStatementType.Select, stmt.StatementType, "Invalid statement type");

			// Describe the statement
			stmt.Describe();

			// Check that sqld = sqln
			Assert.AreEqual(stmt.Fields.Count, stmt.Fields.ActualCount, message1);

			// Check that there are a correct number of fields
			Assert.AreEqual(3, stmt.Fields.ActualCount, message2);

			// Check field type
			Assert.AreEqual(IscCodes.SQL_LONG, stmt.Fields[0].SqlType, message3);
			Assert.AreEqual(IscCodes.SQL_TEXT, stmt.Fields[1].SqlType, message3);
			Assert.AreEqual(IscCodes.SQL_TYPE_DATE, stmt.Fields[2].SqlType, message3);

			// Check field name
			Assert.AreEqual("INT_FIELD", stmt.Fields[0].Name, message4);
			Assert.AreEqual("CHAR_FIELD", stmt.Fields[1].Name, message4);
			Assert.AreEqual("DATE_FIELD", stmt.Fields[2].Name, message4);

			// Check table name
			Assert.AreEqual("TEST", stmt.Fields[0].Relation, message5);
			Assert.AreEqual("TEST", stmt.Fields[1].Relation, message5);
			Assert.AreEqual("TEST", stmt.Fields[2].Relation, message5);

			// Commit transaction
			transaction.Commit();

			// Drop the statement
			stmt.Release();
		}

		[Test]
		public void DescribeParameters()
		{
			string message1 = "Differences betwwen actual and fetched fields received from server in response to a DescribeParameters call";
			string message2 = "Invalid number of fields received from the server in response to a DescribeParameters call.";
			string message3 = "Invalid parameter type received from the server in response to a Describe call.";
			string sql		= "select * from TEST where int_field >= ? and date_field between ? and ?";

			// Start a new Transaction
			ITransaction transaction = this.Attachment.BeginTransaction(BaseTest.BuildTpb());

			// Create a new statement
			StatementBase stmt = this.Attachment.CreateStatement(transaction);

			// Prepare the statement
			stmt.Prepare(sql);

			// Check Statement Type
			Assert.AreEqual(DbStatementType.Select, stmt.StatementType, "Invalid statement type");

			// Describe the statement
			stmt.DescribeParameters();

			// Check that sqld = sqln
			Assert.AreEqual(stmt.Fields.Count, stmt.Fields.ActualCount, message1);

			// Check that there are a correct number of fields
			Assert.AreEqual(3, stmt.Parameters.ActualCount, message2);

			// Check parameter types
			Assert.AreEqual(IscCodes.SQL_LONG, stmt.Parameters[0].SqlType, message3);
			Assert.AreEqual(IscCodes.SQL_TYPE_DATE, stmt.Parameters[1].SqlType, message3);
			Assert.AreEqual(IscCodes.SQL_TYPE_DATE, stmt.Parameters[2].SqlType, message3);

			// Commit transaction
			transaction.Commit();

			// Drop the statement
			stmt.Release();
		}

		[Test]
		public void Prepare()
		{
			string sql = "delete from test where int_field = ?";

			// Start a new Transaction
			ITransaction transaction = this.Attachment.BeginTransaction(BaseTest.BuildTpb());

			// Create a new statement
			StatementBase stmt = this.Attachment.CreateStatement(transaction);

			// Prepare statement & Describve parameters
			stmt.Prepare(sql);
			stmt.DescribeParameters();

			// Check Statement Type
			Assert.AreEqual(DbStatementType.Delete, stmt.StatementType, "Invalid statement type");

			// Delete all the records
			for (int i = 0; i < 100; i++)
			{
				// Set parameter value
				stmt.Parameters[0].Value = i;

				// Execute statement
				stmt.Execute();

				Assert.AreEqual(1, stmt.RecordsAffected, "Invalid number of deleted rows");
			}

			// Commit changes
			transaction.Commit();
		
			// Drop the statement
			stmt.Release();
		}

		[Test]
		public void Fetch()
		{
			int		records = 25;
			string	sql		= "select * from test where int_field < ?";

			// Start a new Transaction
			ITransaction transaction = this.Attachment.BeginTransaction(BaseTest.BuildTpb());

			// Create a new statement
			StatementBase stmt = this.Attachment.CreateStatement(transaction);

			// Prepare statement & Describve parameters
			stmt.Prepare(sql);
			stmt.DescribeParameters();

			// Check Statement Type
			Assert.AreEqual(DbStatementType.Select, stmt.StatementType, "Invalid statement type");

			// Set parameter value
			stmt.Parameters[0].Value = records;

			// Execute statement
			stmt.Execute();

			int counter		= 0;
			DbValue[] row	= new DbValue[0];
			while ((row = stmt.Fetch()) != null)
			{
				counter++;
			}

			Assert.AreEqual(records, counter, "Invalid number of fetched rows");

			// Commit changes
			transaction.Commit();
		
			// Drop the statement
			stmt.Release();
		}

		[Test]
		public void Execute()
		{
			string sql = "select * from test where int_field != ?";

			// Start a new Transaction
			ITransaction transaction = this.Attachment.BeginTransaction(BaseTest.BuildTpb());

			// Create a new statement
			StatementBase stmt = this.Attachment.CreateStatement(transaction);

			// Prepare statement & Describve parameters
			stmt.Prepare(sql);
			stmt.DescribeParameters();

			// Check Statement Type
			Assert.AreEqual(DbStatementType.Select, stmt.StatementType, "Invalid statement type");

			// Set parameter value
			stmt.Parameters[0].Value = 10;

			// Execute statement
			stmt.Execute();

			int counter		= 0;
			DbValue[] row	= new DbValue[0];
			while ((row = stmt.Fetch()) != null)
			{
				counter++;
			}

			// Commit changes
			transaction.Commit();
		
			// Drop the statement
			stmt.Release();
		}

		[Test]
		public void ExecuteImmediate()
		{
			string sql = "create table r1 (col1 smallint not null primary key, col2 smallint)";

			// Start a new Transaction
			ITransaction transaction = this.Attachment.BeginTransaction(BaseTest.BuildTpb());

			// Create a new statement
			StatementBase stmt = this.Attachment.CreateStatement(transaction);

			// Execute statement
			stmt.ExecuteImmediate(sql);

			// Commit changes
			transaction.Commit();
		}

		[Test]
		public void InsertRecordsAffected()
		{
			int		records = 1;
			string	sql		= "insert into test (int_field, char_field) values (?, ?)";

			// Start a new Transaction
			ITransaction transaction = this.Attachment.BeginTransaction(BaseTest.BuildTpb());

			// Create a new statement
			StatementBase stmt = this.Attachment.CreateStatement(transaction);

			// Prepare statement & Describve parameters
			stmt.Prepare(sql);
			stmt.DescribeParameters();

			// Check Statement Type
			Assert.AreEqual(DbStatementType.Insert, stmt.StatementType, "Invalid statement type");

			// Set parameter value
			stmt.Parameters[0].Value = 10000;
			stmt.Parameters[1].Value = "RecordsAffected";

			// Execute statement
			stmt.Execute();

			Assert.AreEqual(records, stmt.RecordsAffected, "Invalid number of updated rows");

			// Commit changes
			transaction.Commit();
		
			// Drop the statement
			stmt.Release();
		}

		[Test]
		public void UpdateRecordsAffected()
		{
			int		records = 15;
			string	sql		= "update test set date_field = ? where int_field < ?";

			// Start a new Transaction
			ITransaction transaction = this.Attachment.BeginTransaction(BaseTest.BuildTpb());

			// Create a new statement
			StatementBase stmt = this.Attachment.CreateStatement(transaction);

			// Prepare statement & Describve parameters
			stmt.Prepare(sql);
			stmt.DescribeParameters();

			// Check Statement Type
			Assert.AreEqual(DbStatementType.Update, stmt.StatementType, "Invalid statement type");

			// Set parameter value
			stmt.Parameters[0].Value = DateTime.Now;
			stmt.Parameters[1].Value = records;

			// Execute statement
			stmt.Execute();

			Assert.AreEqual(records, stmt.RecordsAffected, "Invalid number of updated rows");

			// Commit changes
			transaction.Commit();
		
			// Drop the statement
			stmt.Release();
		}

		[Test]
		public void DeleteRecordsAffected()
		{
			int		records = 25;
			string	sql		= "delete from test where int_field < ?";

			// Start a new Transaction
			ITransaction transaction = this.Attachment.BeginTransaction(BaseTest.BuildTpb());

			// Create a new statement
			StatementBase stmt = this.Attachment.CreateStatement(transaction);

			// Prepare statement & Describve parameters
			stmt.Prepare(sql);
			stmt.DescribeParameters();

			// Check Statement Type
			Assert.AreEqual(DbStatementType.Delete, stmt.StatementType, "Invalid statement type");

			// Set parameter value
			stmt.Parameters[0].Value = records;

			// Execute statement
			stmt.Execute();

			Assert.AreEqual(records, stmt.RecordsAffected, "Invalid number of deleted rows");

			// Commit changes
			transaction.Commit();
		
			// Drop the statement
			stmt.Release();
		}

		[Test]
		public void StoredProcRecordsAffected()
		{
			int		records = -1;
			string	sql		= "EXECUTE PROCEDURE DELETERECORD(?)";

			// Start a new Transaction
			ITransaction transaction = this.Attachment.BeginTransaction(BaseTest.BuildTpb());

			// Create a new statement
			StatementBase stmt = this.Attachment.CreateStatement(transaction);

			// Prepare statement & Describve parameters
			stmt.Prepare(sql);
			stmt.DescribeParameters();

			// Check Statement Type
			Assert.AreEqual(DbStatementType.StoredProcedure, stmt.StatementType, "Invalid statement type");

			// Set parameter value
			stmt.Parameters[0].Value = records;

			// Execute statement
			stmt.Execute();

			Assert.AreEqual(records, stmt.RecordsAffected, "Invalid number of deleted rows using a stored procedure");

			// Commit changes
			transaction.Commit();
		
			// Drop the statement
			stmt.Release();
		}
	}
}
