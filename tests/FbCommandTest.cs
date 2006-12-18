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
using FirebirdSql.Data.Firebird;


namespace FirebirdSql.Data.Firebird.Tests
{
	[TestFixture]
	public class FbCommandTest : BaseTest 
	{
		FbConnection  connection;
		FbTransaction transaction;
		
		public FbCommandTest() : base()
		{		
		}

		[SetUp]
		public void Setup()
		{		
			connection = new FbConnection(GetConnectionString());
			connection.Open();

			transaction = connection.BeginTransaction();
		}
		
		[TearDown]
		public void TearDown()
		{
			connection.Close();
		}

		[Test]
		public void ExecuteNonQueryTest()
		{							
			FbCommand command = connection.CreateCommand();
			
			command.Transaction = transaction;
			command.CommandText = "insert into TEST_TABLE_01 (INT_FIELD) values (?) ";
									
			command.Parameters.Add("@INT_FIELD", 100);
									
			int affectedRows = command.ExecuteNonQuery();
									
			Assertion.AssertEquals(affectedRows, 1);
								
			transaction.Rollback();

			command.Dispose();
		}
		
		[Test]
		public void ExecuteReaderTest()
		{							
			FbCommand command = connection.CreateCommand();
			
			command.Transaction = transaction;
			command.CommandText = "select * from TEST_TABLE_01";
						
			FbDataReader reader = command.ExecuteReader();
									
			reader.Close();

			command.Dispose();
		}

		[Test]
		public void ExecuteReaderWithBehaviorTest()
		{							
			FbCommand command = connection.CreateCommand();
			
			command.Transaction = transaction;
			command.CommandText = "select * from TEST_TABLE_01";
						
			FbDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
									
			reader.Close();

			command.Dispose();
		}
		
		[Test]
		public void ExecuteScalarTest()
		{							
			FbCommand command = connection.CreateCommand();
			
			command.Transaction = transaction;
			command.CommandText = "select CHAR_FIELD from TEST_TABLE_01 where INT_FIELD = ?";
									
			command.Parameters.Add("@INT_FIELD", 2);
						
			string charFieldValue = command.ExecuteScalar().ToString();
			
			Console.WriteLine("Scalar value: {0}", charFieldValue);

			command.Dispose();
		}
		
		[Test]
		public void PrepareTest()
		{							
			FbCommand command = connection.CreateCommand();
			
			command.Transaction = transaction;
			command.CommandText = "select CHAR_FIELD from TEST_TABLE_01 where INT_FIELD = ?";
									
			command.Parameters.Add("@INT_FIELD", 2);
						
			command.Prepare();

			command.Dispose();
		}

		[Test]
		public void NamedParametersTest()
		{
			FbCommand command = connection.CreateCommand();
			
			command.Transaction = transaction;
			command.CommandText = "select CHAR_FIELD from TEST_TABLE_01 where INT_FIELD = @int_field or CHAR_FIELD = @char_field";
									
			command.Parameters.Add("@int_field", 2);
			command.Parameters.Add("@char_field", "TWO");
						
			FbDataReader reader = command.ExecuteReader();
			
			int count = 0;

			while (reader.Read())
			{
				count++;
			}

			Console.WriteLine("\r\n Record fetched {0} \r\n", ++count);


			reader.Close();
			command.Dispose();
		}


		[Test]
		public void ExecuteStoredProcTest()
		{			
			FbCommand command = new FbCommand("EXECUTE PROCEDURE GETVARCHARFIELD(?)", connection, transaction);
				
			command.CommandType = CommandType.StoredProcedure;

			command.Parameters.Add("@ID", FbType.VarChar).Direction = ParameterDirection.Input;
			command.Parameters.Add("@VARCHAR_FIELD", FbType.VarChar).Direction = ParameterDirection.Output;

			command.Parameters[0].Value = 1;

			// This will fill output parameters values
			command.ExecuteNonQuery();

			Console.WriteLine("Output Parameters");
			Console.WriteLine(command.Parameters[1].Value);
		}

		[Test]
		public void RecordsAffectedTest()
		{
			FbCommand selectCommand = new FbCommand("SELECT * FROM TEST_TABLE_01 WHERE INT_FIELD = -1", connection, transaction);
			int recordsAffected = selectCommand.ExecuteNonQuery();
			Assertion.Assert(recordsAffected == -1);
			selectCommand.Dispose();

			FbCommand deleteCommand = new FbCommand("DELETE FROM TEST_TABLE_01 WHERE INT_FIELD = -1", connection, transaction);	
			recordsAffected = deleteCommand.ExecuteNonQuery();
			Assertion.Assert(recordsAffected == 0);
			deleteCommand.Dispose();
		}
	}
}
