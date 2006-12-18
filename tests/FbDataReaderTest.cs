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
	public class FbDataReaderTest : BaseTest 
	{
		FbConnection connection;
		
		public FbDataReaderTest() : base()
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
		public void ReadTest()
		{
			FbTransaction transaction = connection.BeginTransaction();
						
			FbCommand command = new FbCommand("select * from TEST_TABLE_01", connection, transaction);
			
			Console.WriteLine();
			Console.WriteLine("DataReader - Read Method - Test");
			
			IDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				for(int i = 0; i < reader.FieldCount; i++)
				{
					Console.Write(reader.GetValue(i) + "\t");					
				}
			
				Console.WriteLine();
			}

			reader.Close();
			command.Dispose();
			transaction.Rollback();
		}

		[Test]
		public void GetValuesTest()
		{
			FbTransaction transaction = connection.BeginTransaction();
						
			FbCommand command = new FbCommand("select * from TEST_TABLE_01", connection, transaction);
			
			Console.WriteLine();
			Console.WriteLine("DataReader - Read Method - Test");
			
			IDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				object[] values = new object[reader.FieldCount];
				reader.GetValues(values);

				for(int i = 0; i < values.Length; i++)
				{
					Console.Write(values[i] + "\t");					
				}
			
				Console.WriteLine();
			}

			reader.Close();
			transaction.Rollback();	
			command.Dispose();
		}

		[Test]
		public void IndexerByIndexTest()
		{
			FbTransaction transaction = connection.BeginTransaction();
						
			FbCommand command = new FbCommand("select * from TEST_TABLE_01", connection, transaction);
			
			Console.WriteLine();
			Console.WriteLine("DataReader - Read Method - Test");
			
			IDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				for(int i = 0; i < reader.FieldCount; i++)
				{
					Console.Write(reader[i] + "\t");					
				}
			
				Console.WriteLine();
			}

			reader.Close();
			transaction.Rollback();				
			command.Dispose();
		}

		[Test]
		public void IndexerByNameTest()
		{
			FbTransaction transaction = connection.BeginTransaction();
						
			FbCommand command = new FbCommand("select * from TEST_TABLE_01", connection, transaction);
			
			Console.WriteLine();
			Console.WriteLine("DataReader - Read Method - Test");
			
			IDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				for(int i = 0; i < reader.FieldCount; i++)
				{
					Console.Write(reader[reader.GetName(i)] + "\t");					
				}
			
				Console.WriteLine();
			}

			reader.Close();
			transaction.Rollback();				
			command.Dispose();
		}

		[Test]
		public void GetSchemaTableTest()
		{
			FbTransaction transaction	= connection.BeginTransaction();
			FbCommand	  command		= new FbCommand("select * from TEST_TABLE_01", connection, transaction);
	
			FbDataReader reader = command.ExecuteReader(CommandBehavior.SchemaOnly);		
		
			DataTable schema = reader.GetSchemaTable();
			
			Console.WriteLine();
			Console.WriteLine("DataReader - GetSchemaTable Method- Test");

			DataRow[] currRows = schema.Select(null, null, DataViewRowState.CurrentRows);

			foreach (DataColumn myCol in schema.Columns)
			{
				Console.Write("{0}\t\t", myCol.ColumnName);
			}

			Console.WriteLine();
			
			foreach (DataRow myRow in currRows)
			{
				foreach (DataColumn myCol in schema.Columns)
				{
					Console.Write("{0}\t\t", myRow[myCol]);
				}
				
				Console.WriteLine();
			}
			
			reader.Close();
			transaction.Rollback();
			command.Dispose();
		}
		
		[Test]
		public void GetSchemaTableWithExpressionFieldTest()
		{
			FbTransaction transaction	= connection.BeginTransaction();
			FbCommand	  command		= new FbCommand("select TEST_TABLE_01.*, 0 AS VALOR from TEST_TABLE_01", connection, transaction);
	
			FbDataReader reader = command.ExecuteReader(CommandBehavior.SchemaOnly);		
		
			DataTable schema = reader.GetSchemaTable();
			
			Console.WriteLine();
			Console.WriteLine("DataReader - GetSchemaTable Method- Test");

			DataRow[] currRows = schema.Select(null, null, DataViewRowState.CurrentRows);

			foreach (DataColumn myCol in schema.Columns)
			{
				Console.Write("{0}\t\t", myCol.ColumnName);
			}

			Console.WriteLine();
			
			foreach (DataRow myRow in currRows)
			{
				foreach (DataColumn myCol in schema.Columns)
				{
					Console.Write("{0}\t\t", myRow[myCol]);
				}
				
				Console.WriteLine();
			}
			
			reader.Close();
			transaction.Rollback();
			command.Dispose();
		}

		[Test]
		public void NextResultTest()
		{
			string querys = "select * from TEST_TABLE_01 order by INT_FIELD asc;" +
							"select * from TEST_TABLE_01 order by INT_FIELD desc;";

			FbTransaction	transaction = connection.BeginTransaction();
			FbCommand		command		= new FbCommand(querys, connection, transaction);
	
			FbDataReader reader = command.ExecuteReader();		

			Console.WriteLine();
			Console.WriteLine("DataReader - NextResult Method - Test ( First Result )");

			while (reader.Read())
			{
				for(int i = 0; i < reader.FieldCount; i++)
				{
					Console.Write(reader.GetValue(i) + "\t");					
				}
			
				Console.WriteLine();
			}

			if(reader.NextResult())
			{
				Console.WriteLine("DataReader - NextResult Method - Test ( Second Result )");
		
				while (reader.Read())
				{
					for(int i = 0; i < reader.FieldCount; i++)
					{
						Console.Write(reader.GetValue(i) + "\t");					
					}
				
					Console.WriteLine();
				}
			}

			reader.Close();
			transaction.Rollback();
			command.Dispose();
		}
	}
}