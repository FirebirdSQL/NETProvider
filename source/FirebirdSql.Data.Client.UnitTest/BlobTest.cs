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
using System.Text;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.UnitTest
{
	[TestFixture]
	public class BlobTest : BaseTest
	{
		[Test]
		public void ReadAsciiBlob()
		{
			string sql = "select clob_field from test where int_field = ?";

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
			stmt.Parameters[0].Value = 1;

			// Execute statement
			stmt.Execute();

			// Fetch data row
			DbValue[] row	= stmt.Fetch();

			Assert.IsNotNull(row, "Fetched row values are not valid");
			Assert.IsNotNull(row[0], "Fetched row values are not valid");

			Assert.AreEqual("IRow Number1", row[0].Value.ToString(), "Indvalid value fetched for field clob_field");

			// Commit changes
			transaction.Commit();
		
			// Drop the statement
			stmt.Release();
		}

		[Test]
		public void ReadBinaryBlob()
		{
			string sql = "select blob_field from test where int_field = ?";

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
			stmt.Parameters[0].Value = 1;

			// Execute statement
			stmt.Execute();

			// Fetch data row
			DbValue[] row	= stmt.Fetch();

			Assert.IsNotNull(row, "Fetched row values are not valid");
			Assert.IsNotNull(row[0], "Fetched row values are not valid");

			byte[] bytes = (byte[])row[0].Value;

			Assert.AreEqual("IRow Number1", Encoding.Default.GetString(bytes), "Indvalid value fetched for field blob_field");

			// Commit changes
			transaction.Commit();
		
			// Drop the statement
			stmt.Release();
		}
	}
}
