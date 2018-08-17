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

#if !NETSTANDARD1_6
using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Schema
{
	internal abstract class FbSchema
	{
		#region Constructors

		public FbSchema()
		{
		}

		#endregion

		#region Abstract Methods

		protected abstract StringBuilder GetCommandText(string[] restrictions);

		#endregion

		#region Methods

		public DataTable GetSchema(FbConnection connection, string collectionName, string[] restrictions)
		{
			var dataTable = new DataTable(collectionName);
			using (var command = BuildCommand(connection, collectionName, ParseRestrictions(restrictions)))
			{
				using (var adapter = new FbDataAdapter(command))
				{
					try
					{
						adapter.Fill(dataTable);
					}
					catch (Exception ex)
					{
						throw new FbException(ex.Message);
					}
				}
			}
			TrimStringFields(dataTable);
			return ProcessResult(dataTable);
		}

		#endregion

		#region Protected Methods

		protected FbCommand BuildCommand(FbConnection connection, string collectionName, string[] restrictions)
		{
			var filter = string.Format("CollectionName='{0}'", collectionName);
			var builder = GetCommandText(restrictions);
			var restriction = connection.GetSchema(DbMetaDataCollectionNames.Restrictions).Select(filter);
			var transaction = connection.InnerConnection.ActiveTransaction;
			var command = new FbCommand(builder.ToString(), connection, transaction);

			if (restrictions != null && restrictions.Length > 0)
			{
				var index = 0;

				for (var i = 0; i < restrictions.Length; i++)
				{
					var rname = restriction[i]["RestrictionName"].ToString();
					if (restrictions[i] != null)
					{
						// Catalog, Schema and TableType are no real restrictions
						if (!rname.EndsWith("Catalog") && !rname.EndsWith("Schema") && rname != "TableType")
						{
							var pname = string.Format("@p{0}", index++);

							command.Parameters.Add(pname, FbDbType.VarChar, 255).Value = restrictions[i];
						}
					}
				}
			}

			return command;
		}

		protected virtual DataTable ProcessResult(DataTable schema)
		{
			return schema;
		}

		protected virtual string[] ParseRestrictions(string[] restrictions)
		{
			return restrictions;
		}

		#endregion

		#region Private Static Methods

		private static void TrimStringFields(DataTable schema)
		{
			schema.BeginLoadData();

			foreach (DataRow row in schema.Rows)
			{
				for (var i = 0; i < schema.Columns.Count; i++)
				{
					if (!row.IsNull(schema.Columns[i]) &&
						schema.Columns[i].DataType == typeof(System.String))
					{
						row[schema.Columns[i]] = row[schema.Columns[i]].ToString().Trim();
					}
				}
			}

			schema.EndLoadData();
			schema.AcceptChanges();
		}

		#endregion
	}
}
#endif
