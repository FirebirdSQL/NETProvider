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
	public class BdpDataReaderTest : BaseTest 
	{	
		public BdpDataReaderTest() : base(false)
		{		
		}

		[Test]
		public void ReadTest()
		{
			BdpTransaction transaction = Connection.BeginTransaction();
						
			BdpCommand command = new BdpCommand("select * from TEST", Connection, transaction);
			
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
			BdpTransaction transaction = Connection.BeginTransaction();
						
			BdpCommand command = new BdpCommand("select * from TEST", Connection, transaction);
			
			Console.WriteLine();
			Console.WriteLine("DataReader - Read Method - Test");
			
			IDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				object[] values = new object[reader.FieldCount];
				reader.GetValues(values);

				for (int i = 0; i < values.Length; i++)
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
			BdpTransaction transaction = Connection.BeginTransaction();
						
			BdpCommand command = new BdpCommand("select * from TEST", Connection, transaction);
			
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
			BdpTransaction transaction = Connection.BeginTransaction();
						
			BdpCommand command = new BdpCommand("select * from TEST", Connection, transaction);
			
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
			BdpTransaction transaction	= Connection.BeginTransaction();
			BdpCommand	  command		= new BdpCommand("select * from TEST", Connection, transaction);
	
			BdpDataReader reader = command.ExecuteReader(CommandBehavior.SchemaOnly);		
		
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
			BdpTransaction transaction	= Connection.BeginTransaction();
			BdpCommand	  command		= new BdpCommand("select TEST.*, 0 AS VALOR from TEST", Connection, transaction);
	
			BdpDataReader reader = command.ExecuteReader(CommandBehavior.SchemaOnly);		
		
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
		[Ignore("Multiple resultsets are not supported")]
		public void NextResultTest()
		{
			string querys = "select * from TEST order by INT_FIELD asc;" +
							"select * from TEST order by INT_FIELD desc;";

			BdpTransaction	transaction = Connection.BeginTransaction();
			BdpCommand		command		= new BdpCommand(querys, Connection, transaction);
	
			BdpDataReader reader = command.ExecuteReader();		

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
			string sql = "insert into test (int_field) values (100000);";

			BdpCommand command = new BdpCommand(sql, this.Connection);

			BdpDataReader reader = command.ExecuteReader();
			reader.Close();

			Assert.AreEqual(1, reader.RecordsAffected, "RecordsAffected value is incorrect");
		}

		[Test]
		public void GetBytesLengthTest()
		{
			string sql = "select blob_field from TEST where int_field = ?";

			BdpCommand command = new BdpCommand(sql, this.Connection);
			command.Parameters.Add("@int_field", BdpType.Int32).Value = 2;

			BdpDataReader reader = command.ExecuteReader();

			reader.Read();

			long length = reader.GetBytes(0, 0, null, 0, 0);

			reader.Close();

			Assert.AreEqual(13, length, "Incorrect blob length");
		}

		[Test]
		public void GetCharsLengthTest()
		{
			string sql = "select clob_field from TEST where int_field = ?";

			BdpCommand command = new BdpCommand(sql, this.Connection);
			command.Parameters.Add("@int_field", BdpType.Int32).Value = 50;

			BdpDataReader reader = command.ExecuteReader();

			reader.Read();

			long length = reader.GetChars(0, 0, null, 0, 0);

			reader.Close();

			Assert.AreEqual(14, length, "Incorrect clob length");
		}
	}
}