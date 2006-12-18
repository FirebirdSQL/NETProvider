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
using Borland.Data.Common;

namespace FirebirdSql.Data.Bdp.Tests
{
	[TestFixture]
	public class BdpCommandTest : BaseTest 
	{	
		public BdpCommandTest() : base(false)
		{		
		}

		[Test]
		public void ExecuteNonQueryTest()
		{							
			Transaction = Connection.BeginTransaction();

			BdpCommand command = Connection.CreateCommand();
			
			command.Transaction = Transaction;
			command.CommandText = "insert into TEST (INT_FIELD) values (?) ";
									
			command.Parameters.Add("@INT_FIELD", 100);
									
			int affectedRows = command.ExecuteNonQuery();
									
			Assert.AreEqual(affectedRows, 1);
								
			Transaction.Rollback();

            command.Close();
		}
		
		[Test]
		public void ExecuteReaderTest()
		{							
			BdpCommand command = Connection.CreateCommand();
			
			command.CommandText = "select * from TEST";
			
			BdpDataReader reader = command.ExecuteReader();
			reader.Close();

            command.Close();
		}

		[Test]
		public void ExecuteReaderWithBehaviorTest()
		{							
			BdpCommand command = new BdpCommand("select * from TEST", Connection);
			
			BdpDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);								
			reader.Close();

            command.Close();
		}
		
		[Test]
		public void ExecuteScalarTest()
		{							
			BdpCommand command = Connection.CreateCommand();
			
			command.CommandText = "select CHAR_FIELD from TEST where INT_FIELD = ?";									
			command.Parameters.Add("@INT_FIELD", 2);
						
			string charFieldValue = command.ExecuteScalar().ToString();
			
			Console.WriteLine("Scalar value: {0}", charFieldValue);

            command.Close();
		}
		
		[Test]
		public void PrepareTest()		
        {
            try
            {
                // Drop the table
                BdpCommand drop = new BdpCommand("drop table PrepareTest", Connection);
                drop.ExecuteNonQuery();
                drop.Close();
            }
            finally
            {
            }

            // Create a new test table
			BdpCommand create = new BdpCommand("create table PrepareTest(test_field varchar(20));", Connection);
			create.ExecuteNonQuery();
            create.Close();
		
			// Insert data using a prepared statement
			BdpCommand command = new BdpCommand(
				"INSERT INTO PrepareTest (test_field) VALUES (?);",
				Connection);
			
			command.Parameters.Add("@test_field", BdpType.String).Value = DBNull.Value;
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

            command.Close();

			try
			{
				// Check that data is correct
				BdpCommand select = new BdpCommand("select * from PrepareTest", Connection);
				BdpDataReader reader = select.ExecuteReader();
				int count = 0;
				while (reader.Read())
				{
					if (count == 0)
					{
						Assert.AreEqual(DBNull.Value, reader[0], "Invalid value.");
					}
					else
					{
						Assert.AreEqual(count.ToString(), reader.GetString(0).Trim(), "Invalid value.");
					}

					count++;
				}
				reader.Close();
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				// Drop table
				BdpCommand drop = new BdpCommand("drop table PrepareTest", Connection);
				drop.ExecuteNonQuery();
                drop.Close();
			}			
		}

		[Test]
		public void ExecuteStoredProcTest()
		{			
			BdpCommand command = new BdpCommand("EXECUTE PROCEDURE GETVARCHARFIELD(?)", Connection);
				
			command.CommandType = CommandType.StoredProcedure;

			command.Parameters.Add("@ID", BdpType.Int32).Direction = ParameterDirection.Input;
			command.Parameters.Add("@VARCHAR_FIELD", BdpType.String).Direction = ParameterDirection.Output;

			command.Parameters[0].Value = 1;

			// This will fill output parameters values
			command.ExecuteNonQuery();

			Console.WriteLine("Output Parameters");
			Console.WriteLine(command.Parameters[1].Value);
		}

		[Test]
		public void RecordsAffectedTest()
		{
			BdpCommand selectCommand = new BdpCommand("SELECT * FROM TEST WHERE INT_FIELD = -1", Connection);
			int recordsAffected = selectCommand.ExecuteNonQuery();
			Console.WriteLine("\r\nRecords Affected: {0}", recordsAffected);
			Assert.IsTrue(recordsAffected == -1);
            selectCommand.Close();

			BdpCommand deleteCommand = new BdpCommand("DELETE FROM TEST WHERE INT_FIELD = -1", Connection);	
			recordsAffected = deleteCommand.ExecuteNonQuery();
			Console.WriteLine("\r\nRecords Affected: {0}", recordsAffected);
			Assert.IsTrue(recordsAffected == 0);
            deleteCommand.Close();
		}

		[Test]
		public void ExecuteNonQueryWithOutputParameters()
		{
			BdpCommand command = new BdpCommand("EXECUTE PROCEDURE GETASCIIBLOB(?)", Connection);
				
			command.CommandType = CommandType.StoredProcedure;

			command.Parameters.Add("@ID", BdpType.String).Direction = ParameterDirection.Input;
			command.Parameters.Add("@CLOB_FIELD", BdpType.Blob, BdpType.stMemo).Direction = ParameterDirection.Output;

			command.Parameters[0].Value = 1;

			// This will fill output parameters values
			command.ExecuteNonQuery();

			// Check that the output parameter has a correct value
			Assert.AreEqual("IRow Number 1", command.Parameters[1].Value, "Output parameter value is not valid");

			// Close command - this will do a transaction commit
            command.Close();
		}

		[Test]
		public void InvalidParameterFormat()
		{
			string sql = "update test set timestamp_field = ? where int_field = ?";

			BdpTransaction transaction = this.Connection.BeginTransaction();
			try
			{
				BdpCommand command = new BdpCommand(sql, this.Connection, transaction);
				command.Parameters.Add("@timestamp", BdpType.DateTime).Value = 1;
				command.Parameters.Add("@integer", BdpType.Int32).Value = 1;

				command.ExecuteNonQuery();

                command.Close();

				transaction.Commit();
			}
			catch
			{
				transaction.Rollback();
			}
		}

		[Test]
		[Ignore("Named parameters are not support in teh Borland Data Provider")]
		public void NamedParametersTest()
		{
			BdpCommand command = Connection.CreateCommand();
			
			command.CommandText = "select CHAR_FIELD from TEST where INT_FIELD = @int_field or CHAR_FIELD = @char_field";
									
			command.Parameters.Add("@int_field", 2);
			command.Parameters.Add("@char_field", "TWO");
						
			BdpDataReader reader = command.ExecuteReader();
			
			int count = 0;

			while (reader.Read())
			{
				count++;
			}

			Assert.AreEqual(1, count, "Invalid number of records fetched.");

			reader.Close();
            command.Close();
        }

		[Test]
		[Ignore("Named parameters are not support in teh Borland Data Provider")]
		public void NamedParametersAndLiterals()
		{
			string sql = "update test set char_field = 'carlos@firebird.org', bigint_field = @bigint, varchar_field = 'carlos@ado.net' where int_field = @integer";

			BdpCommand command = new BdpCommand(sql, this.Connection);
			command.Parameters.Add("@bigint", BdpType.Int64).Value = 200;
			command.Parameters.Add("@integer",BdpType.Int32).Value = 1;

			int recordsAffected = command.ExecuteNonQuery();

			command.Close();

			Assert.AreEqual(recordsAffected, 1, "Invalid number of records affected.");
		}

		[Test]
		[Ignore("Named parameters are not support in teh Borland Data Provider")]
		public void NamedParametersReuseTest()
		{
			string sql = "select * from test where int_field >= @lang and int_field <= @lang";

			BdpCommand command = new BdpCommand(sql, this.Connection);
			command.Parameters.Add("@lang", BdpType.Int32).Value = 10;
						
			BdpDataReader reader = command.ExecuteReader();
			
			int count		= 0;
			int intValue	= 0;

			while (reader.Read())
			{
				if (count == 0)
				{
					intValue = reader.GetInt32(0);
				}
				count++;
			}

			Assert.AreEqual(1, count, "Invalid number of records fetched.");
			Assert.AreEqual(10, intValue, "Invalid record fetched.");

			reader.Close();
			command.Close();
		}
	}
}
