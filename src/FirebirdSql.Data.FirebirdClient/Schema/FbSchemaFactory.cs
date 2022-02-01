/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Schema;

internal sealed class FbSchemaFactory
{
	#region Static Members

	private static readonly string ResourceName = "FirebirdSql.Data.Schema.FbMetaData.xml";

	#endregion

	#region Constructors

	private FbSchemaFactory()
	{ }

	#endregion

	#region Methods

	public static DataTable GetSchema(FbConnection connection, string collectionName, string[] restrictions)
	{
		var filter = string.Format("CollectionName = '{0}'", collectionName);
		var ds = new DataSet();
		using (var xmlStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName))
		{
			var oldCulture = Thread.CurrentThread.CurrentCulture;
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

		var collection = ds.Tables[DbMetaDataCollectionNames.MetaDataCollections].Select(filter);

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
	public static Task<DataTable> GetSchemaAsync(FbConnection connection, string collectionName, string[] restrictions, CancellationToken cancellationToken = default)
	{
		var filter = string.Format("CollectionName = '{0}'", collectionName);
		var ds = new DataSet();
		using (var xmlStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName))
		{
			var oldCulture = Thread.CurrentThread.CurrentCulture;
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

		var collection = ds.Tables[DbMetaDataCollectionNames.MetaDataCollections].Select(filter);

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
				return PrepareCollectionAsync(connection, collectionName, restrictions, cancellationToken);

			case "DataTable":
				return Task.FromResult(ds.Tables[collection[0]["PopulationString"].ToString()].Copy());

			case "SQLCommand":
				return SqlCommandSchemaAsync(connection, collectionName, restrictions, cancellationToken);

			default:
				throw new NotSupportedException("Unsupported population mechanism");
		}
	}

	#endregion

	#region Private Methods

	private static DataTable PrepareCollection(FbConnection connection, string collectionName, string[] restrictions)
	{
		FbSchema returnSchema = collectionName.ToUpperInvariant() switch
		{
			"CHARACTERSETS" => new FbCharacterSets(),
			"CHECKCONSTRAINTS" => new FbCheckConstraints(),
			"CHECKCONSTRAINTSBYTABLE" => new FbChecksByTable(),
			"COLLATIONS" => new FbCollations(),
			"COLUMNS" => new FbColumns(),
			"COLUMNPRIVILEGES" => new FbColumnPrivileges(),
			"DOMAINS" => new FbDomains(),
			"FOREIGNKEYCOLUMNS" => new FbForeignKeyColumns(),
			"FOREIGNKEYS" => new FbForeignKeys(),
			"FUNCTIONS" => new FbFunctions(),
			"FUNCTIONARGUMENTS" => new FbFunctionArguments(),
			"FUNCTIONPRIVILEGES" => new FbFunctionPrivileges(),
			"GENERATORS" => new FbGenerators(),
			"INDEXCOLUMNS" => new FbIndexColumns(),
			"INDEXES" => new FbIndexes(),
			"PRIMARYKEYS" => new FbPrimaryKeys(),
			"PROCEDURES" => new FbProcedures(),
			"PROCEDUREPARAMETERS" => new FbProcedureParameters(),
			"PROCEDUREPRIVILEGES" => new FbProcedurePrivileges(),
			"ROLES" => new FbRoles(),
			"TABLES" => new FbTables(),
			"TABLECONSTRAINTS" => new FbTableConstraints(),
			"TABLEPRIVILEGES" => new FbTablePrivileges(),
			"TRIGGERS" => new FbTriggers(),
			"UNIQUEKEYS" => new FbUniqueKeys(),
			"VIEWCOLUMNS" => new FbViewColumns(),
			"VIEWS" => new FbViews(),
			"VIEWPRIVILEGES" => new FbViewPrivileges(),
			_ => throw new NotSupportedException("The specified metadata collection is not supported."),
		};
		return returnSchema.GetSchema(connection, collectionName, restrictions);
	}
	private static Task<DataTable> PrepareCollectionAsync(FbConnection connection, string collectionName, string[] restrictions, CancellationToken cancellationToken = default)
	{
		FbSchema returnSchema = collectionName.ToUpperInvariant() switch
		{
			"CHARACTERSETS" => new FbCharacterSets(),
			"CHECKCONSTRAINTS" => new FbCheckConstraints(),
			"CHECKCONSTRAINTSBYTABLE" => new FbChecksByTable(),
			"COLLATIONS" => new FbCollations(),
			"COLUMNS" => new FbColumns(),
			"COLUMNPRIVILEGES" => new FbColumnPrivileges(),
			"DOMAINS" => new FbDomains(),
			"FOREIGNKEYCOLUMNS" => new FbForeignKeyColumns(),
			"FOREIGNKEYS" => new FbForeignKeys(),
			"FUNCTIONS" => new FbFunctions(),
			"FUNCTIONARGUMENTS" => new FbFunctionArguments(),
			"FUNCTIONPRIVILEGES" => new FbFunctionPrivileges(),
			"GENERATORS" => new FbGenerators(),
			"INDEXCOLUMNS" => new FbIndexColumns(),
			"INDEXES" => new FbIndexes(),
			"PRIMARYKEYS" => new FbPrimaryKeys(),
			"PROCEDURES" => new FbProcedures(),
			"PROCEDUREPARAMETERS" => new FbProcedureParameters(),
			"PROCEDUREPRIVILEGES" => new FbProcedurePrivileges(),
			"ROLES" => new FbRoles(),
			"TABLES" => new FbTables(),
			"TABLECONSTRAINTS" => new FbTableConstraints(),
			"TABLEPRIVILEGES" => new FbTablePrivileges(),
			"TRIGGERS" => new FbTriggers(),
			"UNIQUEKEYS" => new FbUniqueKeys(),
			"VIEWCOLUMNS" => new FbViewColumns(),
			"VIEWS" => new FbViews(),
			"VIEWPRIVILEGES" => new FbViewPrivileges(),
			_ => throw new NotSupportedException("The specified metadata collection is not supported."),
		};
		return returnSchema.GetSchemaAsync(connection, collectionName, restrictions, cancellationToken);
	}

	private static DataTable SqlCommandSchema(FbConnection connection, string collectionName, string[] restrictions)
	{
		throw new NotImplementedException();
	}
	private static Task<DataTable> SqlCommandSchemaAsync(FbConnection connection, string collectionName, string[] restrictions, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	#endregion
}
