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
using FirebirdSql.Data.Firebird;


namespace FirebirdSql.Data.Firebird.Tests
{
	[TestFixture]
	public class FbTransactionTest : BaseTest 
	{
		FbConnection	connection;
		FbTransaction	transaction;
		
		public FbTransactionTest() : base()
		{		
		}
		
		[SetUp]
		public void Setup()
		{		
			connection = new FbConnection(GetConnectionString());
			connection.Open();			
		}
		
		[TearDown]
		public void TearDown()
		{
			connection.Close();			
		}
		
		[Test]
		public void CommitTest()
		{			
			transaction = connection.BeginTransaction();
			transaction.Commit();
		}
		
		[Test]
		public void RollbackTest()
		{
			transaction = connection.BeginTransaction();
			transaction.Rollback();
		}		

		[Test]
		public void SavePointTest()
		{
			FbCommand command = new FbCommand();

			Console.WriteLine("Iniciada nueva transaccion");
			
			transaction = connection.BeginTransaction("InitialSavePoint");
			
			command.Connection	= connection;
			command.Transaction	= transaction;

			command.CommandText = "insert into TEST_TABLE_01 (INT_FIELD) values (200) ";
			command.ExecuteNonQuery();			

			transaction.Save("FirstSavePoint");

			command.CommandText = "insert into TEST_TABLE_01 (INT_FIELD) values (201) ";
			command.ExecuteNonQuery();			
			transaction.Save("SecondSavePoint");

			command.CommandText = "insert into TEST_TABLE_01 (INT_FIELD) values (202) ";
			command.ExecuteNonQuery();			
			transaction.Rollback("InitialSavePoint");

			transaction.Commit();
			command.Dispose();
		}
	}
}
