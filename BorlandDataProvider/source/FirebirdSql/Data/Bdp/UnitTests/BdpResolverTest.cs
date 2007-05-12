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
 *  Copyright (c) 2006 Rick Hoover
 *  All Rights Reserved.
 */

/// <summary>
/// CSharp NUnit Test Case
/// This unit contains tests to exercise the FbResolver unit.
/// </summary>

namespace FirebirdSql.Data.Bdp.UnitTests
{
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	using System.ComponentModel;
	using System.Configuration;
	using System.Data;
	using System.Text;
	using System.Windows.Forms;
	using Borland.Data.Common;
	using Borland.Data.Provider;
	using Borland.Data.Schema;
	using NUnit.Framework;
	using FirebirdSql.Data.Bdp;



	// Test methods for class FbResolver
	[TestFixture]
	public class TestFbResolver : BaseTest
	{
		#region Fields

		private	BdpConnection fConnection;
		private	BdpUpdateMode fUpdateMode;
		private	DataRowCollection fColumns;
		private	DataTable fTestTable;
		private	FbMetaData fMetaData;
		private FbResolver fFbResolver;
		private	IDbCommand iCommand;
		private int fColumnCount;
		private string fActualValue;
		private string fKeyField;
		private string fTableName;
		private StringBuilder fExpectedValue;
		private	StringBuilder fSelectCmd;

		#endregion

		#region NUnit Methods
		public override void SetUp()
		{
			base.SetUp();       // Run inherited method first

								// Get connection to database
			fConnection = this.Connection;

								// Create IDbCommand object
			iCommand = fConnection.CreateCommand();

								// Create a resolver object
			fFbResolver = (FbResolver) fConnection.GetResolver();

								// Define table name for tests
			fTableName = "TEST";

			fColumnCount = 15;  // Number of columns in the TEST table

								// Define name for Primary Key field in TEST
			fKeyField  = "INT_FIELD";

								// Define default BdpUpdateMode
			fUpdateMode = BdpUpdateMode.Key;

								// Get list of columns for table
			fSelectCmd = new StringBuilder();
			fSelectCmd.AppendFormat("SELECT * FROM {0}", fTableName);
			iCommand.CommandText = fSelectCmd.ToString();
			fMetaData = (FbMetaData) fConnection.GetMetaData();
			fTestTable = fMetaData.GetSchemaTable(null, iCommand, null);
			fColumns = fTestTable.Rows;

								// Create expectedValue object
			fExpectedValue = new StringBuilder();
		}
		#endregion

		#region Private Methods

		private string GetQuotedIdentifier(object identifier)
		{
			return "\"" + identifier.ToString() + "\"";
		}

		private StringCollection GetTestTableFields()
		/*
		   Return collection of fields for table TEST.
		*/
		{
			StringCollection result = new StringCollection();

			result.Add(fKeyField);
			result.Add("CHAR_FIELD");
			result.Add("VARCHAR_FIELD");
			result.Add("BIGINT_FIELD");
			result.Add("SMALLINT_FIELD");
			result.Add("DOUBLE_FIELD");
			result.Add("FLOAT_FIELD");
			result.Add("NUMERIC_FIELD");
			result.Add("DECIMAL_FIELD");
			result.Add("DATE_FIELD");
			result.Add("TIME_FIELD");
			result.Add("TIMESTAMP_FIELD");
			result.Add("CLOB_FIELD");
			result.Add("BLOB_FIELD");
			result.Add("EXPR_FIELD");

			return result;
		}

		private string BuildFieldList(StringCollection collection)
		/*
		   Returns field names in collection as a list of fields.
		*/
		{
			StringBuilder result = new StringBuilder();

			foreach (string item in collection)
			{
				if (result.Length > 0)
					result.AppendFormat(", {0}", GetQuotedIdentifier (item));
				else
					result.AppendFormat("{0}", GetQuotedIdentifier (item));
			}

			return result.ToString();
		}

		private void DeleteColumns (int first, int last,
									StringCollection fieldNames)
		/*
			Method that deletes the columns between first and last
			in fColumns, and fieldNames.
		*/
		{
			int finalCount = fColumns.Count - (last - first + 1);

			while (fColumns.Count > finalCount)
			{
				fColumns.RemoveAt(first);
				fieldNames.RemoveAt(first);
			}
		}

		private void DeleteKeyColumn (StringCollection fieldNames)
		/*
			Method that deletes the key column from fColumns and fieldNames.
		*/	
		{
			int keyIndex;

			keyIndex = fieldNames.IndexOf(fKeyField);
			fColumns.RemoveAt(keyIndex);
			fieldNames.RemoveAt(keyIndex); 
		}

		#endregion

		#region FbResolver Unit Tests

		[Test]
		public void TestGetSelectSQL()
		/*
			Test that exercises the GetSelectSQL method.
		*/
		{                       // Create variables used in test
			StringBuilder selectCmd = new StringBuilder();
			StringCollection fields = new StringCollection();

							   // Check proper number of columns in table
			Assert.AreEqual(fColumnCount, fColumns.Count,
				"Number of columns is incorrect.");

								// Create select command
			selectCmd.AppendFormat("SELECT * FROM {0}",
			  GetQuotedIdentifier (fTableName));
			iCommand.CommandText = selectCmd.ToString();

								// Get SQL select command from interface
			fActualValue = fFbResolver.GetSelectSQL(fConnection, fColumns,
							 fTableName);

								// Calculate expected value
			fields = GetTestTableFields();
			fExpectedValue.AppendFormat("SELECT {0} FROM {1}",
			  BuildFieldList(fields), GetQuotedIdentifier(fTableName));

								// Compare against expected SQL value
			Assert.AreEqual(fExpectedValue.ToString(), fActualValue,
			  "SQL select command is incorrect");

								// Now delete some fields and try again
			DeleteKeyColumn(fields);
            DeleteColumns(2, fColumns.Count - 1, fields);

								// Get SQL select command from interface
			fActualValue = fFbResolver.GetSelectSQL(fConnection, fColumns,
							 fTableName);

								// Calculate expected value
			fExpectedValue = new StringBuilder();
			fExpectedValue.AppendFormat("SELECT {0} FROM {1}",
			  BuildFieldList(fields), GetQuotedIdentifier(fTableName));

								// Compare against expected SQL value
			Assert.AreEqual(fExpectedValue.ToString(), fActualValue,
			  "Specific SQL select command is incorrect");

		}

		[Test]
		public void TestGetRefreshSQL()
		/*
			Test that exercises the GetRefreshSQL method.
		*/

		{                       // Create variables used in test
			StringBuilder selectCmd = new StringBuilder();
			StringBuilder where 	= new StringBuilder();
			StringCollection fields = new StringCollection();

							   // Check proper number of columns in table
			Assert.AreEqual(fColumnCount, fColumns.Count,
				"Number of columns is incorrect.");

								// Create select command
			selectCmd.AppendFormat("SELECT * FROM {0}",
			  GetQuotedIdentifier (fTableName));
			iCommand.CommandText = selectCmd.ToString();

								// Get SQL refresh command from interface
			fActualValue = fFbResolver.GetRefreshSQL(fConnection, iCommand,
							 fColumns, fTableName);

								// Calculate expected value
			fields = GetTestTableFields();
			where.AppendFormat("({0} = ?)", GetQuotedIdentifier(fKeyField));
			fExpectedValue.AppendFormat("SELECT {0} FROM {1} WHERE {2}",
			  BuildFieldList(fields), GetQuotedIdentifier(fTableName), where);

								// Compare against expected SQL value
			Assert.AreEqual(fExpectedValue.ToString(), fActualValue,
			  "SQL refresh command is incorrect");
		}

		[Test]
		public void TestGetInsertSQL()
		/*
			Test that exercises the GetInsertSQL method.
		*/

		{                       // Create variables used in test
			StringBuilder insertCmd = new StringBuilder();
			StringBuilder values	= new StringBuilder();
			StringCollection fields = new StringCollection();

							   // Check proper number of columns in table
			Assert.AreEqual(fColumnCount, fColumns.Count,
				"Number of columns is incorrect.");

								// Reduce number of columns to insert
			fields = GetTestTableFields();
//			DeleteKeyColumn(fields);
			DeleteColumns(3, fields.Count - 1, fields);

								// Create an insert command
			insertCmd.AppendFormat("INSERT INTO {0}",
			  GetQuotedIdentifier (fTableName));
			iCommand.CommandText = insertCmd.ToString();

								// Get SQL insert command from interface
			fActualValue = fFbResolver.GetInsertSQL(fConnection, iCommand,
							 fColumns, fTableName);

								// Calculate expected value
			for (int index = 0; index < fields.Count; index++)
			{
				if (values.Length > 0)
					values.AppendFormat(", ?");
				else
					values.AppendFormat("?");
			}
			fExpectedValue.AppendFormat("INSERT INTO {0} ({1}) VALUES ({2})",
			  GetQuotedIdentifier(fTableName), BuildFieldList(fields),
			  values);
								// Compare against expected SQL value
			Assert.AreEqual(fExpectedValue.ToString(), fActualValue,
			  "SQL insert command is incorrect");
		}

		[Test]
		public void TestGetUpdateSQL()
		/*
			Test that exercises the GetUpdateSQL method.
		*/

		{                       // Create variables used in test
			StringBuilder updateCmd = new StringBuilder();
			StringBuilder sets		= new StringBuilder();
			StringCollection fields = new StringCollection();

							   // Check proper number of columns in table
			Assert.AreEqual(fColumnCount, fColumns.Count,
				"Number of columns is incorrect.");

								// Reduce number of columns to update
			fields = GetTestTableFields();
//			DeleteKeyColumn(fields);
			DeleteColumns(3, fields.Count - 1, fields);

								// Create an update command
			updateCmd.AppendFormat("UPDATE {0} SET",
			  GetQuotedIdentifier (fTableName));
			iCommand.CommandText = updateCmd.ToString();

								// Get SQL update command from interface
			fActualValue = fFbResolver.GetUpdateSQL(fConnection, iCommand,
							 fColumns, fTableName, fUpdateMode);

								// Build SET clause
			for (int index = 0; index < fields.Count; index++)
			{
				if (sets.Length > 0)
					sets.AppendFormat(",{0} = ?",
					  GetQuotedIdentifier(fields[index]));
				else
					sets.AppendFormat("{0} = ?",
					  GetQuotedIdentifier(fields[index]));
			}

								// Calculate expected result
			fExpectedValue.AppendFormat(
			  "UPDATE {0} SET {1} WHERE (({2} = ?))",
			  GetQuotedIdentifier(fTableName), sets,
			  GetQuotedIdentifier(fKeyField));

								// Compare against expected SQL value
			Assert.AreEqual(fExpectedValue.ToString(), fActualValue,
			  "SQL update command is incorrect");
		}

		[Test]
		public void TestGetDeleteSQL()
		/*
			Test that exercises the GetDeleteSQL method.

			Note:
			BdpResolverHelper, BdpMetaDataHelper - Two interfaces
			that can be of help...
		*/

		{                       // Create extra variables used in test
			StringBuilder deleteCmd = new StringBuilder();

							   // Check proper number of columns in table
			Assert.AreEqual(fColumnCount, fColumns.Count,
				"Number of columns is incorrect.");

								// Create delete command
			deleteCmd.AppendFormat("DELETE FROM {0}", fTableName);
			iCommand.CommandText = deleteCmd.ToString();

								// Get SQL delete command from interface
			fActualValue = fFbResolver.GetDeleteSQL (fConnection, iCommand,
							 fColumns, fTableName, fUpdateMode);

								// Calculate expected value
			fExpectedValue.AppendFormat(
			  "DELETE FROM {0} WHERE (({1} = ?))", GetQuotedIdentifier(fTableName),
			  GetQuotedIdentifier(fKeyField));
#warning "Test comparison string used above should be as shown below"
// "DELETE FROM {0} WHERE ({1} = ?)"

								// Compare against expected SQL value
			Assert.AreEqual(fExpectedValue.ToString(), fActualValue,
			  "SQL delete command is incorrect");

/*
			StringBuilder message = new StringBuilder();
			fActualValue = BdpResolverHelper.Delete;
								// Show SQL delete command for table
			MessageBox.Show (fActualValue, "Information", MessageBoxButtons.OK,
							 MessageBoxIcon.Information);
*/
		}
		#endregion

	}
}
