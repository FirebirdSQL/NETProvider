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
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

namespace FirebirdSql.Data.Firebird.DbSchema
{
	#region STRUCTS

	internal struct FbColumn
	{
		public string ColumnName;
		public string ColumnAlias;
		public string WhereColumnName;
	}

	internal struct FbTableJoin
	{
		public string JoinType;
		public string RightTable;
		public string Expression;
	}

	internal struct FbPrivilege
	{
		public string			User;
		public MatchCollection	Privileges;
	}

	#endregion

	internal abstract class FbAbstractDbSchema : IDbSchema
	{
		#region FIELDS

		private ArrayList	restrictionColumns;
		private ArrayList	dataColumns;
		private ArrayList	tables;
		private ArrayList	joins;
		private ArrayList	orderByColumns;
		private ArrayList	whereFilters;

		private string		tableName;
		
		#endregion

		#region PROPERTIES

		public ArrayList RestrictionColumns
		{
			get { return restrictionColumns; }
		}

		#endregion

		#region CONSTRUCTORS

		public FbAbstractDbSchema()
		{
			restrictionColumns	= new ArrayList();
			dataColumns			= new ArrayList();
			tables				= new ArrayList();
			joins				= new ArrayList();
			orderByColumns		= new ArrayList();
			whereFilters		= new ArrayList();

			AddTables();
			AddRestrictionColumns();
			AddDataColumns();
			AddJoins();
			AddOrderByColumns();
			AddWhereFilters();
		}

		public FbAbstractDbSchema(string tableName) : this()
		{
			this.tableName = tableName;
		}

		#endregion

		#region ABSTRACT_METHODS

		public abstract void AddTables();
		public abstract void AddRestrictionColumns();
		public abstract void AddDataColumns();
		public abstract void AddJoins();
		public abstract void AddOrderByColumns();
		public abstract void AddWhereFilters();
		public abstract object[] ParseRestrictions(object[] restrictions);

		#endregion

		#region ADD_METHODS

		public void AddTable(string tableName)
		{
			tables.Add(tableName);
		}

		public void AddRestrictionColumn(string columnName, string columnAlias, string whereColumnName)
		{
			FbColumn column = new FbColumn();

			column.ColumnName	= columnName;
			column.ColumnAlias	= columnAlias;
			if (whereColumnName != null)
			{
				column.WhereColumnName = whereColumnName;
			}
			else
			{
				column.WhereColumnName = columnName;
			}


			restrictionColumns.Add(column);
		}

		public void AddDataColumn(string columnName, string columnAlias)
		{
			FbColumn column = new FbColumn();

			column.ColumnName	= columnName;
			column.ColumnAlias	= columnAlias;

			dataColumns.Add(column);
		}

		public void AddJoin(string joinType, string rightTable, string expression)
		{
			FbTableJoin join = new FbTableJoin();

			join.JoinType	= joinType;
			join.RightTable	= rightTable;
			join.Expression	= expression;			

			joins.Add(join);
		}

		public void AddOrderBy(string column)
		{
			orderByColumns.Add(column);
		}

		public void AddWhereFilter(string filter)
		{
			whereFilters.Add(filter);
		}

		#endregion

		#region METHODS

		public virtual DataTable GetDbSchemaTable(FbConnection connection, object[] restrictions)
		{
			restrictions = ParseRestrictions(restrictions);

			FbCommand		command = new FbCommand();
			FbDataAdapter	adapter = new FbDataAdapter();
			DataSet			dataSet = new DataSet(tableName);

			try
			{
				command.Connection	= connection;
				command.CommandText = GetCommandText(restrictions);
				if (connection.ActiveTransaction != null &&
					!connection.ActiveTransaction.IsUpdated)
				{
					command.Transaction = connection.ActiveTransaction;
				}

				adapter.SelectCommand = command;
				adapter.Fill(dataSet, tableName);
			}
			catch (Exception ex)
			{
				throw new FbException(ex.Message);
			}
			finally
			{
				adapter.Dispose();
				command.Dispose();
			}

			DataTable schema = dataSet.Tables[tableName];

			this.trimStringFields(schema);

			return this.ProcessResult(schema);
		}

		public string GetCommandText(object[] restrictions)
		{
			StringBuilder sql = new StringBuilder();

			// Add restriction columns
			sql.Append("SELECT ");
			foreach (FbColumn column in restrictionColumns)
			{
				sql.AppendFormat("{0} AS {1}", column.ColumnName, column.ColumnAlias);					
				if ((restrictionColumns.IndexOf(column) + 1) < restrictionColumns.Count)
				{
					sql.Append(", ");
				}
			}

			// Add DataColumns
			if (restrictionColumns.Count > 0 && dataColumns.Count > 0)
			{
				sql.Append(", ");
			}
			foreach (FbColumn column in dataColumns)
			{
				sql.AppendFormat("{0} AS {1}", column.ColumnName, column.ColumnAlias);					
				if ((dataColumns.IndexOf(column) + 1) < dataColumns.Count)
				{
					sql.Append(", ");
				}
			}

			// Add tables
			sql.Append(" FROM ");
			foreach (string table in tables)
			{
				sql.Append(table);
				if ((tables.IndexOf(table) + 1) < tables.Count)
				{
					sql.Append(", ");
				}
			}

			if (joins.Count != 0)
			{
				foreach (FbTableJoin join in joins)
				{
					sql.AppendFormat(" {0} {1} ON {2}", 
						join.JoinType,
						join.RightTable,
						join.Expression);
				}
			}

			// Add restrictions
			StringBuilder whereFilter = new StringBuilder();

			if (restrictions != null && restrictions.Length > 0)
			{
				for (int i = 0; i < restrictions.Length; i++)
				{
					if (restrictions[i] != null)
					{
						if (whereFilter.Length > 0)
						{
							whereFilter.Append(" AND ");
						}
						whereFilter.AppendFormat("{0} = '{1}'", 
							((FbColumn)restrictionColumns[i]).WhereColumnName, 
							restrictions[i]);
					}
				}
			}

			if (whereFilters != null && whereFilters.Count > 0)
			{
				foreach (string condition in whereFilters)
				{
					if (whereFilter.Length > 0)
					{
						whereFilter.Append(" AND ");
					}
					whereFilter.Append(condition);
				}
			}

			if (whereFilter.Length > 0)
			{
				sql.AppendFormat(" WHERE {0}", whereFilter);
			}

			// Add Order By
			if (orderByColumns.Count > 0)
			{
				sql.Append(" ORDER BY ");

				foreach (string columnName in orderByColumns)
				{
					sql.Append(columnName);
					
					if ((orderByColumns.IndexOf(columnName) + 1) < orderByColumns.Count)
					{
						sql.Append(", ");
					}
				}				
			}

			return sql.ToString();
		}

		#endregion

		#region PROTECTED_METHODS

		protected virtual DataTable ProcessResult(DataTable schema)
		{
			return schema;
		}

		#endregion

		#region Private Methods

		private void trimStringFields(DataTable schema)
		{
			schema.BeginLoadData();

			foreach (DataRow row in schema.Rows)
			{
				for (int i = 0; i < schema.Columns.Count; i++)
				{
					if (schema.Columns[i].DataType == typeof(System.String))
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
