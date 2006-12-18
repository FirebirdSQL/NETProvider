/*
 *	Firebird ADO.NET Data provider for .NET	and	Mono 
 * 
 *	   The contents	of this	file are subject to	the	Initial	
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this	file except	in compliance with the 
 *	   License.	You	may	obtain a copy of the License at	
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software	distributed	under the License is distributed on	
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied.	See	the	License	for	the	specific 
 *	   language	governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002,	2004 Carlos	Guzman Alvarez
 *	All	Rights Reserved.
 */

using NUnit.Framework;
using System;
using System.Collections;
using System.Data;
using FirebirdSql.Data.Firebird;

namespace FirebirdSql.Data.Firebird.Tests
{
	[TestFixture]
	public class CharacterSetsTests : BaseTest
	{
		public CharacterSetsTests()
			: base(false)
		{
		}

		[Test]
		public void SimplifiedChineseTest()
		{
			string createTable = "CREATE TABLE TABLE1 (FIELD1 varchar(20))";
			FbCommand create = new FbCommand(createTable, this.Connection);
			create.ExecuteNonQuery();
			create.Dispose();

			// insert using	parametrized SQL
			string sql = "INSERT INTO Table1 VALUES	(@value)";
			FbCommand command = new FbCommand(sql, this.Connection);
			command.Parameters.Add("@value", FbDbType.VarChar).Value = "中文";
			command.ExecuteNonQuery();
			command.Dispose();

			sql = "SELECT *	FROM TABLE1";
			FbCommand select = new FbCommand(sql, this.Connection);
			string result = select.ExecuteScalar().ToString();
			select.Dispose();

			Assert.AreEqual("中文", result, "Incorrect results in	parametrized insert");

			sql = "DELETE FROM TABLE1";
			FbCommand delete = new FbCommand(sql, this.Connection);
			delete.ExecuteNonQuery();
			delete.Dispose();

			// insert using	plain SQL
			sql = "INSERT INTO Table1 VALUES ('中文')";
			FbCommand plainCommand = new FbCommand(sql, this.Connection);
			plainCommand.ExecuteNonQuery();
			plainCommand.Dispose();

			sql = "SELECT *	FROM TABLE1";
			select = new FbCommand(sql, this.Connection);
			result = select.ExecuteScalar().ToString();
			select.Dispose();

			Assert.AreEqual("中文", result, "Incorrect results in insert");
		}

		[Test]
		public void SimplifiedJapaneseTest()
		{
			string createTable = "CREATE TABLE \"漢字\" (\"漢字２\" VARCHAR(10));";

			FbCommand ct = new FbCommand(createTable, this.Connection);
			ct.ExecuteNonQuery();
			ct.Dispose();

			DataTable tb = Connection.GetDbSchemaTable(FbDbSchemaType.Tables, new
			 object[] { });
			for (int i = 0; i < tb.Rows.Count; i++)
			{
				DataRow r = tb.Rows[i];
				Console.WriteLine(r["TABLE_NAME"].ToString());
			}
			FbCommand sel_cmd = new FbCommand("select * from \"漢字\"",
			 Connection);
			sel_cmd.Prepare();
			string plan = sel_cmd.CommandPlan;
			Assert.IsTrue(plan.IndexOf("漢字") >= 0);

			Console.WriteLine(plan);

			FbCommand count_cmd =
			 new FbCommand("select count(*) from \"漢字\"", Connection);
			count_cmd.Prepare();
			int n = (int)count_cmd.ExecuteScalar();

			FbDataAdapter adapter = new FbDataAdapter(sel_cmd);

			FbCommandBuilder builder = new FbCommandBuilder(adapter);

			Console.WriteLine();
			Console.WriteLine("CommandBuilder - GetInsertCommand Method Test");
			FbCommand ins_cmd = builder.GetInsertCommand();
			Console.WriteLine(ins_cmd.CommandText);
			Assert.IsTrue(ins_cmd.CommandText.IndexOf("漢字") >= 0);

			DataTable tb2 = new DataTable();
			int read_n = adapter.Fill(tb2);
			string columnsName = tb2.Columns[0].ColumnName;
			Assert.IsTrue(columnsName == "漢字２");

			builder.Dispose();
			adapter.Dispose();
		}
	}
}
