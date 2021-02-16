/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System.Data;
using System.Threading.Tasks;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Default))]
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Embedded))]
	public class FbSchemaTests : FbTestsBase
	{
		public FbSchemaTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
			: base(serverType, compression, wireCrypt)
		{ }

		[Test]
		public async Task CharacterSets()
		{
			await Connection.GetSchemaAsync("CharacterSets");
		}

		[Test]
		public async Task CheckConstraints()
		{
			await Connection.GetSchemaAsync("CheckConstraints");
		}

		[Test]
		public async Task CheckConstraintsByTable()
		{
			await Connection.GetSchemaAsync("CheckConstraintsByTable");
		}

		[Test]
		public async Task Collations()
		{
			await Connection.GetSchemaAsync("Collations");
		}

		[Test]
		public async Task Columns()
		{
			var columns = await Connection.GetSchemaAsync("Columns");

			columns = await Connection.GetSchemaAsync("Columns", new string[] { null, null, "TEST", "INT_FIELD" });

			Assert.AreEqual(1, columns.Rows.Count);
		}

		[Test]
		public async Task ColumnPrivileges()
		{
			await Connection.GetSchemaAsync("ColumnPrivileges");
		}

		[Test]
		public async Task Domains()
		{
			await Connection.GetSchemaAsync("Domains");
		}

		[Test]
		public async Task ForeignKeys()
		{
			await Connection.GetSchemaAsync("ForeignKeys");
		}

		[Test]
		public async Task ForeignKeyColumns()
		{
			var foreignKeys = await Connection.GetSchemaAsync("ForeignKeys");

			foreach (DataRow row in foreignKeys.Rows)
			{
				var foreignKeyColumns = await Connection.GetSchemaAsync(
					"ForeignKeyColumns",
					new string[] { (string)row["TABLE_CATALOG"], (string)row["TABLE_SCHEMA"], (string)row["TABLE_NAME"], (string)row["CONSTRAINT_NAME"] });
			}
		}

		[Test]
		public async Task Functions()
		{
			await Connection.GetSchemaAsync("Functions");
		}

		[Test]
		public async Task Generators()
		{
			await Connection.GetSchemaAsync("Generators");
		}

		[Test]
		public async Task Indexes()
		{
			await Connection.GetSchemaAsync("Indexes");
		}

		[Test]
		public async Task IndexColumns()
		{
			await Connection.GetSchemaAsync("IndexColumns");
		}

		[Test]
		public async Task PrimaryKeys()
		{
			var primaryKeys = await Connection.GetSchemaAsync("PrimaryKeys");

			primaryKeys = Connection.GetSchema("PrimaryKeys", new string[] { null, null, "TEST" });

			Assert.AreEqual(1, primaryKeys.Rows.Count);
		}

		[Test]
		public async Task ProcedureParameters()
		{
			var procedureParameters = await Connection.GetSchemaAsync("ProcedureParameters");

			procedureParameters = Connection.GetSchema("ProcedureParameters", new string[] { null, null, "SELECT_DATA" });

			Assert.AreEqual(3, procedureParameters.Rows.Count);
		}

		[Test]
		public async Task ProcedurePrivileges()
		{
			await Connection.GetSchemaAsync("ProcedurePrivileges");
		}

		[Test]
		public async Task Procedures()
		{
			var procedures = await Connection.GetSchemaAsync("Procedures");

			procedures = Connection.GetSchema("Procedures", new string[] { null, null, "SELECT_DATA" });

			Assert.AreEqual(1, procedures.Rows.Count);
		}

		[Test]
		public async Task Procedures_ShouldSkipSchemaAndProperlyUseParametersForProcedureName()
		{
			var procedures = await Connection.GetSchemaAsync("Procedures");

			procedures = Connection.GetSchema("Procedures", new string[] { null, "DUMMY_SCHEMA", "SELECT_DATA" });

			Assert.AreEqual(1, procedures.Rows.Count);
		}

		[Test]
		public async Task DataTypes()
		{
			await Connection.GetSchemaAsync("DataTypes");
		}

		[Test]
		public async Task Roles()
		{
			await Connection.GetSchemaAsync("Roles");
		}

		[Test]
		public async Task Tables()
		{
			var tables = await Connection.GetSchemaAsync("Tables");

			tables = await Connection.GetSchemaAsync("Tables", new string[] { null, null, "TEST" });

			Assert.AreEqual(1, tables.Rows.Count);

			tables = await Connection.GetSchemaAsync("Tables", new string[] { null, null, null, "TABLE" });

			Assert.AreEqual(3, tables.Rows.Count);
		}

		[Test]
		public async Task TableConstraints()
		{
			await Connection.GetSchemaAsync("TableConstraints");
		}

		[Test]
		public async Task TablePrivileges()
		{
			await Connection.GetSchemaAsync("TablePrivileges");
		}

		[Test]
		public async Task Triggers()
		{
			await Connection.GetSchemaAsync("Triggers");
		}

		[Test]
		public async Task UniqueKeys()
		{
			await Connection.GetSchemaAsync("UniqueKeys");
		}

		[Test]
		public async Task ViewColumns()
		{
			await Connection.GetSchemaAsync("ViewColumns");
		}

		[Test]
		public async Task Views()
		{
			await Connection.GetSchemaAsync("Views");
		}

		[Test]
		public async Task ViewPrivileges()
		{
			await Connection.GetSchemaAsync("ViewPrivileges");
		}
	}
}
