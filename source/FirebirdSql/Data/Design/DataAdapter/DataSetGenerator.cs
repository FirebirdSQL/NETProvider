/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002, 2006 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

#if	(NET)

using System;
using System.Collections;
using System.Data;
using System.Globalization;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Design.DataAdapter
{
	internal class DataSetGenerator
	{
        #region · Inner classes ·

        private class ForeignKey
        {
            #region · Fields ·

            private string pkTable;
            private ArrayList pkColumns;
            private string fkTable;
            private ArrayList fkColumns;
            private string name;
            private string updateRule;
            private string deleteRule;

            #endregion

            #region · Properties ·

            public string Name
            {
                get { return this.name; }
            }

            public string PkTable
            {
                get { return this.pkTable; }
            }

            public ArrayList PkColumns
            {
                get
                {
                    if (this.pkColumns == null)
                    {
                        this.pkColumns = new ArrayList();
                    }

                    return this.pkColumns;
                }
            }

            public string FkTable
            {
                get { return this.fkTable; }
            }

            public ArrayList FkColumns
            {
                get
                {
                    if (this.fkColumns == null)
                    {
                        this.fkColumns = new ArrayList();
                    }

                    return this.fkColumns;
                }
            }

            #endregion

            #region · Constructors ·

            public ForeignKey(
                string name,
                string pkTable,
                string fkTable,
                string updateRule,
                string deleteRule)
            {
                this.name = name;
                this.pkTable = pkTable;
                this.fkTable = fkTable;
                this.updateRule = updateRule;
                this.deleteRule = deleteRule;
            }

            #endregion
        }

        #endregion

        #region · Private Static Methods ·

        private static string GetFileName(string path, string name, string extension)
        {
            if (!path.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
            {
                path += System.IO.Path.DirectorySeparatorChar;
            }

            return path + name + "." + extension;
        }

        private static string FormatIdentifier(object value, bool properCase)
        {
            return FormatIdentifier((string)value, properCase);
        }

        private static string FormatIdentifier(string value, bool properCase)
        {
            if (properCase)
            {
                string result = CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(value.ToLower());

                return result.Replace("_", "");
            }
            else
            {
                return value;
            }
        }

        #endregion
        
		#region · Constructors ·

		private DataSetGenerator()
		{
		}

		#endregion

		#region · DataSet Generation ·

		/// <summary>
		/// Generates a	DataSet	using the given	<see cref="FbDataAdapter"/>
		/// </summary>
		/// <param name="adapter">A <see cref="FbDataAdapter"/> objects.</param>
		/// <param name="name">The DataSet name.</param>
		/// <param name="properCase">Indicates wheter table	and	columns	names should be	generated using	a proper case.</param>
		/// <returns>A DataSet object.</returns>
		public static DataSet GenerateDataset(FbDataAdapter adapter, string name, bool properCase)
		{
			DataSet			ds			= new DataSet(name);
			FbConnection	connection	= new FbConnection();
			FbCommand		select		= connection.CreateCommand();

			try
			{
				ds.Namespace = String.Format("http://www.tempuri.org/{0}.xsd", name);

				connection.ConnectionString = adapter.SelectCommand.Connection.ConnectionString;
				connection.Open();

				select.CommandText = adapter.SelectCommand.CommandText;

				FbDataAdapter tmpAdapter = new FbDataAdapter(select);
				tmpAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
				tmpAdapter.FillSchema(ds, SchemaType.Source);
				tmpAdapter.Dispose();

				foreach (DataColumn column in ds.Tables[0].Columns)
				{
					// Add proper case to the column names
					column.ColumnName = FormatIdentifier(column.ColumnName, properCase);
				}
			}
			catch
			{
				throw;
			}
			finally
			{
				if (select != null)
				{
					select.Dispose();
				}
				if (connection != null)
				{
					connection.Close();
				}
			}

			return ds;
		}

		/// <summary>
		/// Generates a	DataSet	using the given	connection string and tables.
		/// </summary>
		/// <param name="connectionString">A connection	string.</param>
		/// <param name="tables">The list of tables	to add to the DataSet.</param>
		/// <param name="name">The DataSet name.</param>
		/// <param name="properCase">Indicates wheter table	and	columns	names should be	generated using	a proper case.</param>
		/// <returns>A DataSet object.</returns>
		public static DataSet GenerateDataset(string connectionString, ArrayList tables, string name, bool properCase)
		{
			DataSet			ds			= new DataSet(name);
			FbConnection	connection	= new FbConnection(connectionString);

			connection.Open();

			ds.Namespace = String.Format("http://www.tempuri.org/{0}.xsd", name);

			foreach (string tableName in tables)
			{
				FbCommand select = new FbCommand("select * from " + tableName, connection);

				FbDataAdapter tmpAdapter = new FbDataAdapter(select);
				tmpAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
				tmpAdapter.FillSchema(ds, SchemaType.Source, FormatIdentifier(tableName, properCase));
				tmpAdapter.Dispose();
				select.Dispose();

				foreach (DataColumn column in ds.Tables[FormatIdentifier(tableName, properCase)].Columns)
				{
					// Add proper case to the column names
					column.ColumnName = FormatIdentifier(column.ColumnName, properCase);
				}
			}

			// Determine the Foreign Keys
			ArrayList relations = new ArrayList();
			foreach (string tableName in tables)
			{
				DataTable foreignKeys = connection.GetSchema("ForeignKeys", new string[] { null, null, tableName.ToUpper() });
				DataRowCollection rows = foreignKeys.Rows;

				foreach (DataRow foreignKey in foreignKeys.Rows)
				{
					ForeignKey fk = new ForeignKey(
                        FormatIdentifier((string)foreignKey["CONSTRAINT_NAME"], properCase),
                        FormatIdentifier((string)foreignKey["TABLE_NAME"], properCase),
                        FormatIdentifier((string)foreignKey["REFERENCED_TABLE_NAME"], properCase),
                        FormatIdentifier((string)foreignKey["UPDATE_RULE"], properCase),
                        FormatIdentifier((string)foreignKey["DELETE_RULE"], properCase));

                    DataTable foreignKeyColumns = connection.GetSchema(
                        "ForeignKeyColumns",
                        new string[] { null, null, (string)foreignKey["TABLE_NAME"], (string)foreignKey["CONSTRAINT_NAME"] });

					foreach (DataRow foreignKeyColumn in foreignKeyColumns.Rows)
					{
                        fk.PkColumns.Add(FormatIdentifier(foreignKeyColumn["COLUMN_NAME"], properCase));
                        fk.FkColumns.Add(FormatIdentifier(foreignKeyColumn["REFERENCED_COLUMN_NAME"], properCase));
					}

					relations.Add(fk);
				}
			}

			connection.Close();

			// Add relations to	the	DataSet
			foreach (ForeignKey fk in relations)
			{
				if (ds.Tables.Contains(fk.PkTable) && ds.Tables.Contains(fk.FkTable))
				{
					DataColumn[] parentColumns = new DataColumn[fk.PkColumns.Count];
					DataColumn[] childColumns = new DataColumn[fk.FkColumns.Count];

					// Get parent columns
					for (int i = 0; i < parentColumns.Length; i++)
					{
						parentColumns[i] = ds.Tables[fk.PkTable].Columns[fk.PkColumns[i].ToString()];
					}

					// Get child columns
					for (int i = 0; i < childColumns.Length; i++)
					{
						childColumns[i] = ds.Tables[fk.FkTable].Columns[fk.FkColumns[i].ToString()];
					}

					ds.Relations.Add(new DataRelation(fk.Name, parentColumns, childColumns));
				}
			}

			return ds;
		}

		#endregion

		#region · DataSet Serialization ·

#if	(VISUAL_STUDIO)

		/// <summary>
		/// Generates the schema (in XSD format) and C#	or VB.NET source code of a DataSet.
		/// </summary>
		/// <param name="dataSet">The source DataSet.</param>
		/// <param name="addToProject">Indicates wheter	the	resulting files	should be added	to the active Visual Studio	project.</param>
		/// <param name="productName">The Visual Studio	product	name we	are	working	on.</param>
		/// <returns>An	string with	the	class name of the generated	DataSet	(namespace + class name)</returns>
		public static string SerializeTypedDataSet(DataSet dataSet, bool addToProject, string productName)
		{
			VSExtensibility vs = new VSExtensibility(productName);

			string path = vs.GetActiveDocumentPath();
			string schemaFile = GetFileName(path, dataSet.DataSetName, "xsd");

			// Generate	the	Schema File
			// dataSet.Namespace = vs.GetCurrentNamespace();
			dataSet.WriteXmlSchema(schemaFile);

			// Add the file	to the Visual Studio project
			string codeNamespace = vs.AddTypedDataSet(schemaFile);

			return codeNamespace + "." + dataSet.DataSetName;
		}

#endif
		#endregion
	}
}

#endif