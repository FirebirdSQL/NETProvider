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
	public class FbDataReaderTest : BaseTest 
	{	
		public FbDataReaderTest() : base(false)
		{		
		}

		[Test]
		public void ReadTest()
		{
			FbTransaction transaction = Connection.BeginTransaction();
						
			FbCommand command = new FbCommand("select * from TEST", Connection, transaction);
			
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
			FbTransaction transaction = Connection.BeginTransaction();
						
			FbCommand command = new FbCommand("select * from TEST", Connection, transaction);
			
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
			FbTransaction transaction = Connection.BeginTransaction();
						
			FbCommand command = new FbCommand("select * from TEST", Connection, transaction);
			
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
			FbTransaction transaction = Connection.BeginTransaction();
						
			FbCommand command = new FbCommand("select * from TEST", Connection, transaction);
			
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
			FbTransaction transaction	= Connection.BeginTransaction();
			FbCommand	  command		= new FbCommand("select * from TEST", Connection, transaction);
	
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
			FbTransaction transaction	= Connection.BeginTransaction();
			FbCommand	  command		= new FbCommand("select TEST.*, 0 AS VALOR from TEST", Connection, transaction);
	
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
			string querys = "select * from TEST order by INT_FIELD asc;" +
							"select * from TEST order by INT_FIELD desc;";

			FbTransaction	transaction = Connection.BeginTransaction();
			FbCommand		command		= new FbCommand(querys, Connection, transaction);
	
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


		[Test]
		public void RecordAffectedTest()
		{
			string sql = "insert into test (int_field) values (100000)";

			FbCommand command = new FbCommand(sql, this.Connection);

			FbDataReader reader = command.ExecuteReader();

			bool nextResult = true;

			while (nextResult)
			{
				while (reader.Read())
				{
				}

				nextResult = reader.NextResult();
			}

			reader.Close();

			Assertion.AssertEquals("RecordsAffected value is incorrect", 1, reader.RecordsAffected);
		}
	}
}