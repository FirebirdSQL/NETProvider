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
using FirebirdSql.Data.Firebird;

namespace FirebirdSql.Data.Firebird.Tests
{
	[TestFixture]
	public class FbCommandTest : BaseTest 
	{	
		public FbCommandTest() : base(false)
		{		
		}

		[Test]
		public void ExecuteNonQueryTest()
		{							
			Transaction = Connection.BeginTransaction();

			FbCommand command = Connection.CreateCommand();
			
			command.Transaction = Transaction;
			command.CommandText = "insert into TEST (INT_FIELD) values (?) ";
									
			command.Parameters.Add("@INT_FIELD", 100);
									
			int affectedRows = command.ExecuteNonQuery();
									
			Assertion.AssertEquals(affectedRows, 1);
								
			Transaction.Rollback();

			command.Dispose();
		}
		
		[Test]
		public void ExecuteReaderTest()
		{							
			FbCommand command = Connection.CreateCommand();
			
			command.CommandText = "select * from TEST";
			
			FbDataReader reader = command.ExecuteReader();
			reader.Close();

			command.Dispose();
		}

		[Test]
		public void ExecuteReaderWithBehaviorTest()
		{							
			FbCommand command = new FbCommand("select * from TEST", Connection);
			
			FbDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);								
			reader.Close();

			command.Dispose();
		}
		
		[Test]
		public void ExecuteScalarTest()
		{							
			FbCommand command = Connection.CreateCommand();
			
			command.CommandText = "select CHAR_FIELD from TEST where INT_FIELD = ?";									
			command.Parameters.Add("@INT_FIELD", 2);
						
			string charFieldValue = command.ExecuteScalar().ToString();
			
			Console.WriteLine("Scalar value: {0}", charFieldValue);

			command.Dispose();
		}
		
		[Test]
		public void PrepareTest()
		{					
			// Create a new test table
			FbCommand create = new FbCommand("create table PrepareTest(test_field varchar(20));", Connection);
			create.ExecuteNonQuery();
			create.Dispose();
		
			// Insert data using a prepared statement
			FbCommand command = new FbCommand(
				"insert into PrepareTest(test_field) values(@test_field);",
				Connection);
			
			command.Parameters.Add("@test_field", FbDbType.VarChar).Value = DBNull.Value;
			command.Prepare();

			for (int i = 0; i < 5; i++) 
			{
				if (i < 1)
				{
					command.Parameters[0].Value = DBNull.Value;
				}
				else
				{
					command.Parameters[0].Value = i.ToString();
				}
				command.ExecuteNonQuery();
			}

			command.Dispose();

			try
			{
				// Check that data is correct
				FbCommand select = new FbCommand("select * from PrepareTest", Connection);
				FbDataReader reader = select.ExecuteReader();
				int count = 0;
				while (reader.Read())
				{
					if (count == 0)
					{
						Assertion.AssertEquals("Ivalid value.", DBNull.Value, reader[0]);
					}
					else
					{
						Assertion.AssertEquals("Ivalid value.", count, reader.GetInt32(0));
					}

					count++;
				}
				reader.Close();
				select.Dispose();
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				// Drop table
				FbCommand drop = new FbCommand("drop table PrepareTest", Connection);
				drop.ExecuteNonQuery();
				drop.Dispose();
			}
		}

		[Test]
		public void NamedParametersTest()
		{
			FbCommand command = Connection.CreateCommand();
			
			command.CommandText = "select CHAR_FIELD from TEST where INT_FIELD = @int_field or CHAR_FIELD = @char_field";
									
			command.Parameters.Add("@int_field", 2);
			command.Parameters.Add("@char_field", "TWO");
						
			FbDataReader reader = command.ExecuteReader();
			
			int count = 0;

			while (reader.Read())
			{
				count++;
			}

			Console.WriteLine("\r\n Record fetched {0} \r\n", count);

			reader.Close();
			command.Dispose();
		}

		[Test]
		public void NamedParametersAndLiterals()
		{
			string sql = "update test set char_field = 'carlos@firebird.org', bigint_field = @bigint, varchar_field = 'carlos@ado.net' where int_field = @integer";

			FbCommand command = new FbCommand(sql, this.Connection);
			command.Parameters.Add("@bigint", FbDbType.BigInt).Value = 200;
			command.Parameters.Add("@integer", FbDbType.Integer).Value = 1;

			int recordsAffected = command.ExecuteNonQuery();

			command.Dispose();

			Assertion.AssertEquals("Invalid number of records affected.", recordsAffected, 1);
		}

		[Test]
		public void NamedParametersReuseTest()
		{
			string sql = "select * from test where int_field >= @lang and int_field <= @lang";

			FbCommand command = new FbCommand(sql, this.Connection);
			command.Parameters.Add("@lang", FbDbType.Integer).Value = 10;
						
			FbDataReader reader = command.ExecuteReader();
			
			int count = 0;

			while (reader.Read())
			{
				count++;
			}

			Assertion.AssertEquals("Invalid number of records fetched.", 1, count);

			reader.Close();
			command.Dispose();
		}

		[Test]
		public void ExecuteStoredProcTest()
		{			
			FbCommand command = new FbCommand("EXECUTE PROCEDURE GETVARCHARFIELD(?)", Connection);
				
			command.CommandType = CommandType.StoredProcedure;

			command.Parameters.Add("@ID", FbDbType.VarChar).Direction = ParameterDirection.Input;
			command.Parameters.Add("@VARCHAR_FIELD", FbDbType.VarChar).Direction = ParameterDirection.Output;

			command.Parameters[0].Value = 1;

			// This will fill output parameters values
			command.ExecuteNonQuery();

			Console.WriteLine("Output Parameters");
			Console.WriteLine(command.Parameters[1].Value);
		}

		[Test]
		public void RecordsAffectedTest()
		{
			FbCommand selectCommand = new FbCommand("SELECT * FROM TEST WHERE INT_FIELD = -1", Connection);
			int recordsAffected = selectCommand.ExecuteNonQuery();
			Console.WriteLine("\r\nRecords Affected: {0}", recordsAffected);
			Assertion.Assert(recordsAffected == -1);
			selectCommand.Dispose();

			FbCommand deleteCommand = new FbCommand("DELETE FROM TEST WHERE INT_FIELD = -1", Connection);	
			recordsAffected = deleteCommand.ExecuteNonQuery();
			Console.WriteLine("\r\nRecords Affected: {0}", recordsAffected);
			Assertion.Assert(recordsAffected == 0);
			deleteCommand.Dispose();
		}

		[Test]
		public void ExecuteNonQueryWithOutputParameters()
		{
			FbCommand command = new FbCommand("EXECUTE PROCEDURE GETASCIIBLOB(?)", Connection);
				
			command.CommandType = CommandType.StoredProcedure;

			command.Parameters.Add("@ID", FbDbType.VarChar).Direction = ParameterDirection.Input;
			command.Parameters.Add("@CLOB_FIELD", FbDbType.Text).Direction = ParameterDirection.Output;

			command.Parameters[0].Value = 1;

			// This will fill output parameters values
			command.ExecuteNonQuery();

			// Check that the output parameter has a correct value
			Assertion.AssertEquals(
				"Output parameter value is not valid", 
				"IRow Number1", 
				command.Parameters[1].Value);

			// Dispose command - this will do a transaction commit
			command.Dispose();
		}

		[Test]
		public void InvalidParameterFormat()
		{
			string sql = "update test set timestamp_field = @timestamp where int_field = @integer";

			FbTransaction transaction = this.Connection.BeginTransaction();
			try
			{
				FbCommand command = new FbCommand(sql, this.Connection, transaction);
				command.Parameters.Add("@timestamp", FbDbType.TimeStamp).Value = 1;
				command.Parameters.Add("@integer", FbDbType.Integer).Value = 1;

				command.ExecuteNonQuery();

				transaction.Commit();
			}
			catch
			{
				transaction.Rollback();
			}
		}

		[Test]
		public void ExecutProcedureTest()
		{
			string sql = "execute procedure ";
		}

		[Test]
		public void UnicodeInsert()
		{
			FbCommand insert = new FbCommand("insert into test (int_field, varchar_field) values (@id, @desc)", this.Connection);

			insert.Parameters.Add("@id", FbDbType.Integer).Value = 10000;
			insert.Parameters.Add("@desc", FbDbType.VarChar, 255).Value = "Teflon® Coated";

			insert.ExecuteNonQuery();

			FbCommand select = new FbCommand("select varchar_field from test where int_field = @id", this.Connection);
			select.Parameters.Add("@id", FbDbType.Integer).Value = 10000;

			string s = select.ExecuteScalar() as string;

            Assertion.AssertEquals("Incorrect value fetched", "Teflon® Coated", s);
		}
	}
}
