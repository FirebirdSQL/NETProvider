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
using System.Data.Common;
using System.Text;

using Borland.Data.Common;
using Borland.Data.Schema;

namespace FirebirdSql.Data.Bdp
{
	public class FbResolver : ISQLResolver
	{
		#region Fields

		private FbConnection	connection;
		private DataRow			row;
		private string[]		excludeFilter;
		private string[]		readOnly;
		private string			quotePrefix;
		private string			quoteSuffix;
		private	string			separator;
		
		#endregion

		#region Properties

		public string[] ExcludeFilter 
		{ 
			get { return this.excludeFilter; }
			set	{ this.excludeFilter = value; }
		} 

		public string QuotePrefix 
		{ 
			get { return this.quotePrefix; }
			set	{ this.quotePrefix = value; }
		}

		public string QuoteSuffix
		{ 
			get { return this.quoteSuffix; }
			set	{ this.quoteSuffix = value; }
		}

		public string[] ReadOnly 
		{ 
			get { return this.readOnly; }
			set	{ this.readOnly = value; }
		}

		public DataRow Row 
		{ 
			get { return this.row; }
			set	{ this.row = value; }
		}

		#endregion

		#region Constructors

		public FbResolver(FbConnection connection)
		{
			this.connection		= connection;
			this.quotePrefix	= "\"";
			this.quoteSuffix	= "\"";
			this.separator		= ",";
		}

		#endregion

		#region ISQLResolver Methods

		public string GetSelectSQL(
			IDbConnection		connection, 
			DataRowCollection	columns, 
			string				tableName) 
		{
			StringBuilder sql = new StringBuilder();
			
			foreach (DataRow column in columns)
			{
				if (sql.Length > 0)
				{
					sql.Append(", ");
				}
				sql.AppendFormat("{0}{1}{2}", this.QuotePrefix, column["ColumnName"], this.QuoteSuffix);
			}

			sql.AppendFormat(" FROM {0}{1}{2}", this.QuotePrefix, tableName, this.QuoteSuffix);

			return String.Format("SELECT {0}", sql.ToString());
		}

		public string GetInsertSQL(
			IDbConnection		connection, 
			IDbCommand			command, 
			DataRowCollection	columns, 
			string				tableName)
		{
			StringBuilder	sql		= new StringBuilder();
			StringBuilder	fields	= new StringBuilder();
			StringBuilder	values	= new StringBuilder();

			if (this.CheckColumns(columns))
			{
				int i = 0;
				foreach (DataRow schemaRow in columns)
				{
					if (this.IsUpdatable(schemaRow))
					{
						if (fields.Length > 0)
						{
							fields.Append(this.separator);
						}
						if (values.Length > 0)
						{
							values.Append(this.separator);
						}

						fields.Append(this.GetQuotedIdentifier(schemaRow["BaseColumnName"]));

						IDataParameter parameter = this.CreateParameter(command, schemaRow, i, false);

						// values.Append(parameter.ParameterName);
						values.Append("?");

						if (this.row != null)
						{
							parameter.Value = this.row[schemaRow["BaseColumnName"].ToString()];
						}

						i++;

						command.Parameters.Add(parameter);
					}
				}

				sql.AppendFormat(
					"INSERT INTO {0} ({1}) VALUES ({2})",
					this.GetQuotedIdentifier(tableName),
					fields.ToString(),
					values.ToString());
			}

			return sql.ToString();
		}

		public string GetUpdateSQL(
			IDbConnection		connection, 
			IDbCommand			command, 
			DataRowCollection	columns, 
			string				tableName, 
			BdpUpdateMode		updateMode)
		{
			StringBuilder sql = new StringBuilder();
			StringBuilder sets = new StringBuilder();
			StringBuilder where = new StringBuilder();

			if (this.CheckColumns(columns))
			{
#warning "BdpUpdateMode will be unused for now"
				int i = 0;
				foreach (DataRow schemaRow in columns)
				{
					if (this.IsUpdatable(schemaRow))
					{
						if (sets.Length > 0)
						{
							sets.Append(separator);
						}

						IDataParameter parameter = this.CreateParameter(command, schemaRow, i, false);

						// Update SET clausule
						sets.AppendFormat(
							"{0} = {1}",
							this.GetQuotedIdentifier(schemaRow["BaseColumnName"]),
							"?");

						if (row != null)
						{
							parameter.Value = row[schemaRow["BaseColumnName"].ToString()];
						}

						i++;

						command.Parameters.Add(parameter);
					}
				}

				// Build where clausule
				foreach (DataRow schemaRow in columns)
				{
					if (this.IncludedInWhereClause(schemaRow))
					{
						if (where.Length > 0)
						{
							where.Append(" AND ");
						}

						string quotedId = this.GetQuotedIdentifier(schemaRow["BaseColumnName"]);

						// Create parameters for this field
						IDataParameter parameter = this.CreateParameter(command, schemaRow, i, true);

						// Add where clausule for this field
						if ((bool)schemaRow["IsKeyColumn"])
						{
							where.AppendFormat("({0} = {1})", quotedId, "?");
						}

						if (row != null)
						{
							parameter.Value = row[schemaRow["BaseColumnName"].ToString(), DataRowVersion.Original];
						}

						command.Parameters.Add(parameter);

						i++;
					}
				}

				sql.AppendFormat(
					"UPDATE {0} SET {1} WHERE ({2})",
					this.GetQuotedIdentifier(tableName),
					sets.ToString(),
					where.ToString());
			}
			
			return sql.ToString();
		}

		public string GetDeleteSQL(
			IDbConnection		connection, 
			IDbCommand			command, 
			DataRowCollection	columns, 
			string				tableName, 
			BdpUpdateMode		updateMode)
		{
			StringBuilder sql	= new StringBuilder();
			StringBuilder where = new StringBuilder();

			if (this.CheckColumns(columns))
			{
#warning "BdpUpdateMode will be unused for now"

				int i = 0;
				foreach (DataRow schemaRow in columns)
				{
					if (this.IncludedInWhereClause(schemaRow))
					{
						if (where.Length > 0)
						{
							where.Append(" AND ");
						}

						string quotedId = this.GetQuotedIdentifier(schemaRow["BaseColumnName"]);

						// Create parameters for this field
						IDataParameter parameter = this.CreateParameter(command, schemaRow, i, true);

						if ((bool)schemaRow["IsKeyColumn"])
						{
							where.AppendFormat("({0} = {1})", quotedId, "?");
						}

						if (this.row != null)
						{
							parameter.Value = row[schemaRow["BaseColumnName"].ToString(), DataRowVersion.Original];
						}

						command.Parameters.Add(parameter);

						i++;
					}
				}

				sql.AppendFormat(
					"DELETE FROM {0} WHERE ({1})",
					this.GetQuotedIdentifier(tableName),
					where.ToString());
			}
			
			return sql.ToString();
		}

		#endregion

		#region Private Methods

		private bool CheckColumns(DataRowCollection columns)
		{
			bool	hasPk		= false;
			string	tableName	= String.Empty;

			foreach (DataRow column in columns)
			{
				if (tableName.Length == 0)
				{
					tableName = (string)column["BaseTableName"];
				}
				if (tableName != (string)column["BaseTableName"])
                {
					throw new InvalidOperationException("Dynamic SQL generation is not supported against multiple base tables.");
				}
				if ((bool)column["IsKeyColumn"])
				{
					hasPk = true;
				}
			}

			return (hasPk && columns.Count > 0);
		}

		private string GetQuotedIdentifier(object identifier)
		{
			return this.quotePrefix + identifier.ToString() + this.quoteSuffix;
		}

		private bool IsUpdatable(DataRow schemaRow)
		{
			/*
			if ((bool)schemaRow["IsExpression"])
			{
				return false;
			}
			*/
			if ((bool)schemaRow["IsAutoIncrement"])
			{
				return false;
			}
			if ((bool)schemaRow["IsRowVersion"])
			{
				return false;
			}
			if ((bool)schemaRow["IsReadOnly"])
			{
				return false;
			}

			return true;
		}

		private bool IncludedInWhereClause(DataRow schemaRow)
		{
			BdpType dbType = (BdpType)schemaRow["ProviderType"];

			if (dbType == BdpType.Array || dbType == BdpType.Blob)
			{
				return false;
			}

			if ((bool)schemaRow["IsLong"])
			{
				return false;
			}

			if (!(bool)schemaRow["IsKeyColumn"])
			{
				return false;
			}

			return true;
		}

		private IDataParameter CreateParameter(
			IDbCommand	command,
			DataRow		schemaRow, 
			int			index, 
			bool		isWhereParameter)
		{
			IDataParameter parameter = command.CreateParameter();
			
			parameter.ParameterName	= String.Format("@p{0}", index + 1);
			parameter.SourceColumn	= Convert.ToString(schemaRow["BaseColumnName"]);
			if (isWhereParameter)
			{
				parameter.SourceVersion	= DataRowVersion.Original;
			}
			else
			{
				parameter.SourceVersion	= DataRowVersion.Current;
			}

			((IDbDataParameter)parameter).Size = Convert.ToInt32(schemaRow["ColumnSize"]);
			if (schemaRow["NumericPrecision"] != DBNull.Value)
			{
				((IDbDataParameter)parameter).Precision	= Convert.ToByte(schemaRow["NumericPrecision"]);
			}

			if (schemaRow["NumericScale"] != DBNull.Value)
			{
				((IDbDataParameter)parameter).Scale	= Convert.ToByte(schemaRow["NumericScale"]);
			}
			
			return parameter;
		}

		#endregion
	}
}
