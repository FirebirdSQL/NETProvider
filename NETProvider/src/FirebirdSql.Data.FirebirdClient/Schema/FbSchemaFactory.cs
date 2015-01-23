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
 *	Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *	All Rights Reserved.
 *	
 *  Contributors:
 *    Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;

using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Schema
{
	internal sealed class FbSchemaFactory
	{
		#region Static Members

		private static readonly string ResourceName = "FirebirdSql.Data.Schema.FbMetaData.xml";

		#endregion

		#region Constructors

		private FbSchemaFactory()
		{
		}

		#endregion

		#region Methods

		public static DataTable GetSchema(FbConnection connection, string collectionName, string[] restrictions)
		{
			string filter = String.Format("CollectionName = '{0}'", collectionName);
			DataSet ds = new DataSet();
			using (var xmlStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName))
			{
				CultureInfo oldCulture = Thread.CurrentThread.CurrentCulture;
				try
				{
					Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
					// ReadXml contains error: http://connect.microsoft.com/VisualStudio/feedback/Validation.aspx?FeedbackID=95116
					// that's the reason for temporarily changing culture
					ds.ReadXml(xmlStream);
				}
				finally
				{
					Thread.CurrentThread.CurrentCulture = oldCulture;
				}
			}

			DataRow[] collection = ds.Tables[DbMetaDataCollectionNames.MetaDataCollections].Select(filter);

			if (collection.Length != 1)
			{
				throw new NotSupportedException("Unsupported collection name.");
			}

			if (restrictions != null && restrictions.Length > (int)collection[0]["NumberOfRestrictions"])
			{
				throw new InvalidOperationException("The number of specified restrictions is not valid.");
			}

			if (ds.Tables[DbMetaDataCollectionNames.Restrictions].Select(filter).Length != (int)collection[0]["NumberOfRestrictions"])
			{
				throw new InvalidOperationException("Incorrect restriction definition.");
			}

			switch (collection[0]["PopulationMechanism"].ToString())
			{
				case "PrepareCollection":
					return PrepareCollection(connection, collectionName, restrictions);

				case "DataTable":
					return ds.Tables[collection[0]["PopulationString"].ToString()].Copy();

				case "SQLCommand":
					return SqlCommandSchema(connection, collectionName, restrictions);

				default:
					throw new NotSupportedException("Unsupported population mechanism");
			}
		}

		#endregion

		#region Private Methods

		private static DataTable PrepareCollection(FbConnection connection, string collectionName, string[] restrictions)
		{
			FbSchema returnSchema = null;

			switch (collectionName.ToLower(CultureInfo.InvariantCulture))
			{
				case "charactersets":
					returnSchema = new FbCharacterSets();
					break;

				case "checkconstraints":
					returnSchema = new FbCheckConstraints();
					break;

				case "checkconstraintsbytable":
					returnSchema = new FbChecksByTable();
					break;

				case "collations":
					returnSchema = new FbCollations();
					break;

				case "columns":
					returnSchema = new FbColumns();
					break;

				case "columnprivileges":
					returnSchema = new FbColumnPrivileges();
					break;

				case "domains":
					returnSchema = new FbDomains();
					break;

				case "foreignkeycolumns":
					returnSchema = new FbForeignKeyColumns();
					break;

				case "foreignkeys":
					returnSchema = new FbForeignKeys();
					break;

				case "functions":
					returnSchema = new FbFunctions();
					break;

				case "generators":
					returnSchema = new FbGenerators();
					break;

				case "indexcolumns":
					returnSchema = new FbIndexColumns();
					break;

				case "indexes":
					returnSchema = new FbIndexes();
					break;

				case "primarykeys":
					returnSchema = new FbPrimaryKeys();
					break;

				case "procedures":
					returnSchema = new FbProcedures();
					break;

				case "procedureparameters":
					returnSchema = new FbProcedureParameters();
					break;

				case "procedureprivileges":
					returnSchema = new FbProcedurePrivilegesSchema();
					break;

				case "roles":
					returnSchema = new FbRoles();
					break;

				case "tables":
					returnSchema = new FbTables();
					break;

				case "tableconstraints":
					returnSchema = new FbTableConstraints();
					break;

				case "tableprivileges":
					returnSchema = new FbTablePrivileges();
					break;

				case "triggers":
					returnSchema = new FbTriggers();
					break;

				case "uniquekeys":
					returnSchema = new FbUniqueKeys();
					break;

				case "viewcolumns":
					returnSchema = new FbViewColumns();
					break;

				case "views":
					returnSchema = new FbViews();
					break;

				case "viewprivileges":
					returnSchema = new FbViewPrivileges();
					break;

				default:
					throw new NotSupportedException("The specified metadata collection is not supported.");
			}

			return returnSchema.GetSchema(connection, collectionName, restrictions);
		}

		private static DataTable SqlCommandSchema(FbConnection connection, string collectionName, string[] restrictions)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
