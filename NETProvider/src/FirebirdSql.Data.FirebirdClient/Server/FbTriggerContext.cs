/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
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
 *  Copyright (c) 2005-2006 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Server
{
	public sealed class FbTriggerContext
	{
		#region Properties

		public string TableName
		{
			get 
			{
				if (this.connection == null || this.connection.State != System.Data.ConnectionState.Open)
				{
					throw new InvalidOperationException("The connection should bevalid an open.");
				}

				return this.connection.InnerConnection.Database.GetTriggerContext().GetTableName(); 
			}
		}

		public FbTriggerActionType Action
		{
			get 
			{
				if (this.connection == null || this.connection.State != System.Data.ConnectionState.Open)
				{
					throw new InvalidOperationException("The connection should bevalid an open.");
				}

				return (FbTriggerActionType)this.connection.InnerConnection.Database.GetTriggerContext().GetTriggerAction(); 
			}
		}

		#endregion

		#region Fields

		private FbConnection connection;

		#endregion

		#region Constructors

		public FbTriggerContext(FbConnection connection)
		{
			if (connection == null)
			{
				throw new ArgumentNullException("connection cannot be null.");
			}

			this.connection = connection;
		}

		#endregion

		#region Methods

		public object GetOldValue(string name)
		{
			if (this.connection == null || this.connection.State != System.Data.ConnectionState.Open)
			{
				throw new InvalidOperationException("The connection should bevalid an open.");
			}

			return this.connection.InnerConnection.Database.GetTriggerContext().GetOldValue(name);
		}

		public object GetNewValue(string name)
		{
			if (this.connection == null || this.connection.State != System.Data.ConnectionState.Open)
			{
				throw new InvalidOperationException("The connection should bevalid an open.");
			}

			return this.connection.InnerConnection.Database.GetTriggerContext().GetNewValue(name);
		}

		public void SetNewValue(string name, object value)
		{
			if (this.connection == null || this.connection.State != System.Data.ConnectionState.Open)
			{
				throw new InvalidOperationException("The connection should bevalid an open.");
			}

			this.connection.InnerConnection.Database.GetTriggerContext().SetNewValue(name, value);
		}

		#endregion
	}
}
