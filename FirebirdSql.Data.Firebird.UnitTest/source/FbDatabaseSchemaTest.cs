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

using System;
using System.Data;
using System.Collections;
using FirebirdSql.Data.Firebird;
using FirebirdSql.Data.Firebird.Isql;
using NUnit.Framework;

namespace FirebirdSql.Data.Firebird.Tests
{
	[TestFixture]
	public class FbDatabaseSchemaTest : BaseTest 
	{
		public FbDatabaseSchemaTest() : base(false)
		{
		}

		[Test]
		public void CharacterSets()
		{
			DataTable characterSets = Connection.GetDbSchemaTable(
				FbDbSchemaType.Character_Sets, null);
		}
		
		[Test]
		public void CheckConstraints()
		{
			DataTable checkConstraints = Connection.GetDbSchemaTable(
				FbDbSchemaType.Check_Constraints, null);
		}

		[Test]
		public void CheckConstraintsByTable()
		{
			DataTable checkConstraintsByTable = Connection.GetDbSchemaTable(
				FbDbSchemaType.Check_Constraints_By_Table, null);
		}

		[Test]
		public void Collations()
		{
			DataTable collations = Connection.GetDbSchemaTable(
				FbDbSchemaType.Collations, null);
		}

		[Test]
		public void Columns()
		{
			DataTable columns = Connection.GetDbSchemaTable(
				FbDbSchemaType.Columns, null);
		}

		[Test]
		public void ColumnPrivileges()
		{
			DataTable columnPrivileges = Connection.GetDbSchemaTable(
				FbDbSchemaType.Column_Privileges, null);
		}

		[Test]
		public void Domains()
		{
			DataTable domains = Connection.GetDbSchemaTable(
				FbDbSchemaType.Domains, null);
		}

		[Test]
		public void ForeignKeys()
		{
			DataTable foreignKeys = Connection.GetDbSchemaTable(
				FbDbSchemaType.Foreign_Keys, null);
		}

		[Test]
		public void Functions()
		{
			DataTable functions = Connection.GetDbSchemaTable(
				FbDbSchemaType.Functions, null);
		}

		[Test]
		public void Generators()
		{
			DataTable generators = Connection.GetDbSchemaTable(
				FbDbSchemaType.Generators, null);
		}

		[Test]
		public void Indexes()
		{
			DataTable indexes = Connection.GetDbSchemaTable(
				FbDbSchemaType.Indexes, null);
		}

		[Test]
		public void PrimaryKeys()
		{
			DataTable primaryKeys = Connection.GetDbSchemaTable(
				FbDbSchemaType.Primary_Keys, null);
		}

		[Test]
		public void ProcedureParameters()
		{
			DataTable procedureParameters = Connection.GetDbSchemaTable(
				FbDbSchemaType.Procedure_Parameters, null);
		}

		[Test]
		public void ProcedurePrivileges()
		{
			DataTable procedurePrivileges = Connection.GetDbSchemaTable(
				FbDbSchemaType.Procedure_Privileges, null);
		}

		[Test]
		public void Procedures()
		{
			DataTable procedures = Connection.GetDbSchemaTable(
				FbDbSchemaType.Procedures, null);
		}

		[Test]
		public void ProviderTypes()
		{
			DataTable providerTypes = Connection.GetDbSchemaTable(
				FbDbSchemaType.Provider_Types, null);
		}

		[Test]
		public void Roles()
		{
			DataTable roles = Connection.GetDbSchemaTable(
				FbDbSchemaType.Roles, null);
		}

		[Test]
		[Ignore("Not implemented.")]
		public void Statistics()
		{
			DataTable statistics = Connection.GetDbSchemaTable(
				FbDbSchemaType.Statistics, null);
		}

		[Test]
		public void Tables()
		{
			DataTable tables = Connection.GetDbSchemaTable(
				FbDbSchemaType.Tables, null);
		}

		[Test]
		public void TableConstraint()
		{
			DataTable tableConstraint = Connection.GetDbSchemaTable(
				FbDbSchemaType.Table_Constraint, null);
		}

		[Test]
		public void TablePrivileges()
		{
			DataTable tablePrivileges = Connection.GetDbSchemaTable(
				FbDbSchemaType.Table_Privileges, null);
		}

		[Test]
		[Ignore("Not implemented.")]
		public void TableStatistics()
		{
			DataTable table_Statistics = Connection.GetDbSchemaTable(
				FbDbSchemaType.Table_Statistics, null);
		}

		[Test]
		public void Triggers()
		{
			DataTable triggers = Connection.GetDbSchemaTable(
				FbDbSchemaType.Triggers, null);
		}

		[Test]
		[Ignore("Not implemented.")]
		public void UsagePrivileges()
		{
			DataTable usagePrivileges = Connection.GetDbSchemaTable(
				FbDbSchemaType.Usage_Privileges, null);
		}

		[Test]
		public void ViewColumnUsage()
		{
			DataTable viewColumnUsage = Connection.GetDbSchemaTable(
				FbDbSchemaType.View_Column_Usage, null);
		}

		[Test]
		public void Views()
		{
			DataTable views = Connection.GetDbSchemaTable(
				FbDbSchemaType.Views, null);
		}

		[Test]
		public void ViewPrivileges()
		{
			DataTable viewPrivileges = Connection.GetDbSchemaTable(
				FbDbSchemaType.View_Privileges, null);
		}
	}
}
