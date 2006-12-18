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
	public class FbDataAdapterTest : BaseTest 
	{
		FbConnection	connection;
		FbTransaction	transaction;

		public FbDataAdapterTest() : base()
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
			transaction.Commit();
			connection.Close();
		}
		
		[Test]
		public void FillTest()
		{
			FbCommand		command = new FbCommand("select * from TEST_TABLE_01 where DATE_FIELD = ?", connection, transaction);
			FbDataAdapter	adapter = new FbDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@DATE_FIELD", FbType.Date, "DATE_FIELD").Value = new DateTime(2003, 1, 5);
			
			FbCommandBuilder builder = new FbCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "TEST_TABLE_01");
			
			Console.WriteLine();
			Console.WriteLine("DataAdapter - Fill Method - Test");

			foreach (DataTable table in ds.Tables)
			{
				foreach (DataColumn col in table.Columns)
				{
					Console.Write(col.ColumnName + "\t\t");
				}
				
				Console.WriteLine();
				
				foreach (DataRow row in table.Rows)
				{
					for (int i = 0; i < table.Columns.Count; i++)
					{
						Console.Write(row[i] + "\t\t");
					}

					Console.WriteLine("");
				}
			}

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
		}

		[Test]
		public void FillMultipleTest()
		{
			FbCommand		command = new FbCommand("select * from TEST_TABLE_01 where DATE_FIELD = ?", connection, transaction);
			FbDataAdapter	adapter = new FbDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@DATE_FIELD", FbType.Date, "DATE_FIELD").Value = new DateTime(2003, 1, 5);
			
			FbCommandBuilder builder = new FbCommandBuilder(adapter);

			DataSet ds1 = new DataSet();
			DataSet ds2 = new DataSet();
			
			adapter.Fill(ds1, "TEST_TABLE_01");
			adapter.Fill(ds2, "TEST_TABLE_01");
			
			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
		}

		[Test]
		public void InsertTest()
		{
			FbCommand command		= new FbCommand("select * from TEST_TABLE_01", connection, transaction);
			FbDataAdapter adapter	= new FbDataAdapter(command);
			
			FbCommandBuilder builder = new FbCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "TEST_TABLE_01");

			Console.WriteLine();
			Console.WriteLine("DataAdapter - Insert Row Test");

			DataRow newRow = ds.Tables["TEST_TABLE_01"].NewRow();

			newRow["INT_FIELD"]			= 100;
			newRow["CHAR_FIELD"]		= "ONE THOUSAND";
			newRow["VARCHAR_FIELD"]		= ":;,.´ç{}`+^*[]ºª\\!|@·#$%&/()?¿_-<>ëÄëö";
			newRow["BIGINT_FIELD"]		= 100000;
			newRow["SMALLINT_FIELD"]	= 100;
			newRow["DOUBLE_FIELD"]		= 100.01;
			newRow["NUMERIC_FIELD"]		= 100.01;
			newRow["DECIMAL_FIELD"]		= 100.01;
			newRow["DATE_FIELD"]		= new DateTime(100, 10, 10);
			newRow["TIME_FIELD"]		= new DateTime(100, 10, 10, 10, 10, 10, 10);
			newRow["TIMESTAMP_FIELD"]	= new DateTime(100, 10, 10, 10, 10, 10, 10);
			newRow["CLOB_FIELD"]		= "ONE THOUSAND";

			ds.Tables["TEST_TABLE_01"].Rows.Add(newRow);

			adapter.Update(ds, "TEST_TABLE_01");

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
		}

		[Test]
		public void UpdateCharTest()
		{
			string			sql		= "select * from TEST_TABLE_01 where INT_FIELD = ?";
			FbCommand		command = new FbCommand(sql, connection, transaction);
			FbDataAdapter	adapter = new FbDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@INT_FIELD", FbType.Integer,
												"INT_FIELD").Value = 1;
			
			FbCommandBuilder builder = new FbCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "TEST_TABLE_01");

			ds.Tables["TEST_TABLE_01"].Rows[0]["CHAR_FIELD"] = "ONE THOUSAND";

			adapter.Update(ds, "TEST_TABLE_01");

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
		}

		[Test]
		public void UpdateVarCharTest()
		{
			string			sql		= "select * from TEST_TABLE_01 where INT_FIELD = ?";
			FbCommand		command = new FbCommand(sql, connection, transaction);
			FbDataAdapter	adapter = new FbDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@INT_FIELD", FbType.Integer,
												"INT_FIELD").Value = 1;
			
			FbCommandBuilder builder = new FbCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "TEST_TABLE_01");

			ds.Tables["TEST_TABLE_01"].Rows[0]["VARCHAR_FIELD"]	= "ONE THOUSAND";

			adapter.Update(ds, "TEST_TABLE_01");

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
		}

		[Test]
		public void UpdateBigIntTest()
		{
			string			sql		= "select * from TEST_TABLE_01 where INT_FIELD = ?";
			FbCommand		command = new FbCommand(sql, connection, transaction);
			FbDataAdapter	adapter = new FbDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@INT_FIELD", FbType.Integer,
												"INT_FIELD").Value = 1;
			
			FbCommandBuilder builder = new FbCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "TEST_TABLE_01");

			ds.Tables["TEST_TABLE_01"].Rows[0]["BIGINT_FIELD"]	= System.Int64.MaxValue;

			adapter.Update(ds, "TEST_TABLE_01");

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
		}

		[Test]
		public void UpdateSmallIntTest()
		{
			string			sql		= "select * from TEST_TABLE_01 where INT_FIELD = ?";
			FbCommand		command = new FbCommand(sql, connection, transaction);
			FbDataAdapter	adapter = new FbDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@INT_FIELD", FbType.Integer,
												"INT_FIELD").Value = 1;
			
			FbCommandBuilder builder = new FbCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "TEST_TABLE_01");

			ds.Tables["TEST_TABLE_01"].Rows[0]["SMALLINT_FIELD"] = System.Int16.MaxValue;

			adapter.Update(ds, "TEST_TABLE_01");

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
		}

		[Test]
		public void UpdateDoubleTest()
		{
			string			sql		= "select * from TEST_TABLE_01 where INT_FIELD = ?";
			FbCommand		command = new FbCommand(sql, connection, transaction);
			FbDataAdapter	adapter = new FbDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@INT_FIELD", FbType.Integer,
												"INT_FIELD").Value = 1;
			
			FbCommandBuilder builder = new FbCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "TEST_TABLE_01");

			ds.Tables["TEST_TABLE_01"].Rows[0]["DOUBLE_FIELD"]	= System.Decimal.MaxValue;

			adapter.Update(ds, "TEST_TABLE_01");

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
		}

		[Test]
		public void UpdateNumericTest()
		{
			string			sql		= "select * from TEST_TABLE_01 where INT_FIELD = ?";
			FbCommand		command = new FbCommand(sql, connection, transaction);
			FbDataAdapter	adapter = new FbDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@INT_FIELD", FbType.Integer,
												"INT_FIELD").Value = 1;
			
			FbCommandBuilder builder = new FbCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "TEST_TABLE_01");

			ds.Tables["TEST_TABLE_01"].Rows[0]["NUMERIC_FIELD"]	= System.Int32.MaxValue;

			adapter.Update(ds, "TEST_TABLE_01");

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
		}

		[Test]
		public void UpdateDecimalTest()
		{
			string			sql		= "select * from TEST_TABLE_01 where INT_FIELD = ?";
			FbCommand		command = new FbCommand(sql, connection, transaction);
			FbDataAdapter	adapter = new FbDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@INT_FIELD", FbType.Integer,
												"INT_FIELD").Value = 1;
			
			FbCommandBuilder builder = new FbCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "TEST_TABLE_01");

			ds.Tables["TEST_TABLE_01"].Rows[0]["NUMERIC_FIELD"]	= System.Int32.MaxValue;
			
			adapter.Update(ds, "TEST_TABLE_01");

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
		}

		[Test]
		public void UpdateDateTest()
		{
			string			sql		= "select * from TEST_TABLE_01 where INT_FIELD = ?";
			FbCommand		command = new FbCommand(sql, connection, transaction);
			FbDataAdapter	adapter = new FbDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@INT_FIELD", FbType.Integer,
												"INT_FIELD").Value = 1;
			
			FbCommandBuilder builder = new FbCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "TEST_TABLE_01");

			ds.Tables["TEST_TABLE_01"].Rows[0]["DATE_FIELD"] = new DateTime(100, 10, 10);

			adapter.Update(ds, "TEST_TABLE_01");

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
		}

		[Test]
		public void UpdateTimeTest()
		{
			string			sql		= "select * from TEST_TABLE_01 where INT_FIELD = ?";
			FbCommand		command = new FbCommand(sql, connection, transaction);
			FbDataAdapter	adapter = new FbDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@INT_FIELD", FbType.Integer,
												"INT_FIELD").Value = 1;
			
			FbCommandBuilder builder = new FbCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "TEST_TABLE_01");

			ds.Tables["TEST_TABLE_01"].Rows[0]["TIME_FIELD"] = new DateTime(100, 10, 10, 10, 10, 10, 10);

			adapter.Update(ds, "TEST_TABLE_01");

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
		}

		[Test]
		public void UpdateTimeStampTest()
		{
			string			sql		= "select * from TEST_TABLE_01 where INT_FIELD = ?";
			FbCommand		command = new FbCommand(sql, connection, transaction);
			FbDataAdapter	adapter = new FbDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@INT_FIELD", FbType.Integer,
													"INT_FIELD").Value = 1;
			
			FbCommandBuilder builder = new FbCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "TEST_TABLE_01");

			ds.Tables["TEST_TABLE_01"].Rows[0]["TIMESTAMP_FIELD"] = new DateTime(100, 10, 10, 10, 10, 10, 10);

			adapter.Update(ds, "TEST_TABLE_01");

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
		}

		[Test]
		public void UpdateClobTest()
		{
			string			sql		= "select * from TEST_TABLE_01 where INT_FIELD = ?";
			FbCommand		command = new FbCommand(sql, connection, transaction);
			FbDataAdapter	adapter = new FbDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@INT_FIELD", FbType.Integer,
				"INT_FIELD").Value = 1;
			
			FbCommandBuilder builder = new FbCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "TEST_TABLE_01");

			ds.Tables["TEST_TABLE_01"].Rows[0]["CLOB_FIELD"]	= "ONE THOUSAND";

			adapter.Update(ds, "TEST_TABLE_01");

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
		}
	
		[Test]
		public void DeleteTest()
		{
			string			sql		= "select * from TEST_TABLE_01 where INT_FIELD = ?";
			FbCommand		command = new FbCommand(sql, connection, transaction);
			FbDataAdapter	adapter = new FbDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@INT_FIELD", FbType.Integer, "INT_FIELD").Value = 100;
			
			FbCommandBuilder builder = new FbCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "TEST_TABLE_01");

			ds.Tables["TEST_TABLE_01"].Rows[0].Delete();

			adapter.Update(ds, "TEST_TABLE_01");

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
		}
	}
}
