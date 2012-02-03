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
 *  Copyright (c) 2004, 2005 Carlos Guzman Alvarez
 *  All Rights Reserved.
 *  
 *  Contributors:
 *		Jiri Cincura (jiri@cincura.net)
 */

#if (!NET_CF)

using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text;

namespace FirebirdSql.Data.FirebirdClient
{
	public class FbConnectionStringBuilder : DbConnectionStringBuilder
	{
		#region · Properties ·

		public string UserID
		{
			get { return this.GetString("User ID"); }
			set { this.SetValue("User ID", value); }
		}

		public string Password
		{
			get { return this.GetString("Password"); }
			set { this.SetValue("Password", value); }
		}

		public string DataSource
		{
			get { return this.GetString("Data Source"); }
			set { this.SetValue("Data Source", value); }
		}

		public string Database
		{
			get { return this.GetString("Initial Catalog"); }
			set { this.SetValue("Initial Catalog", value); }
		}

		public int Port
		{
			get { return this.GetInt32("Port Number"); }
			set { this.SetValue("Port Number", value); }
		}

		public int PacketSize
		{
			get { return this.GetInt32("Packet Size"); }
			set { this.SetValue("Packet Size", value); }
		}

		public string Role
		{
			get { return this.GetString("Role Name"); }
			set { this.SetValue("Role Name", value); }
		}

		public int Dialect
		{
			get { return this.GetInt32("Dialect"); }
			set { this.SetValue("Dialect", value); }
		}

		public string Charset
		{
			get { return this.GetString("Character Set"); }
			set { this.SetValue("Character Set", value); }
		}

		public int ConnectionTimeout
		{
			get { return this.GetInt32("Connection Timeout"); }
			set { this.SetValue("Connection Timeout", value); }
		}

		public bool Pooling
		{
			get { return this.GetBoolean("Pooling"); }
			set { this.SetValue("Pooling", value); }
		}

		public int ConnectionLifeTime
		{
			get { return this.GetInt32("Connection Lifetime"); }
			set { this.SetValue("Connection Lifetime", value); }
		}

		public int MinPoolSize
		{
			get { return this.GetInt32("Min Pool Size"); }
			set { this.SetValue("Min Pool Size", value); }
		}

		public int MaxPoolSize
		{
			get { return this.GetInt32("Max Pool Size"); }
			set { this.SetValue("Max Pool Size", value); }
		}

		public int FetchSize
		{
			get { return this.GetInt32("Fetch Size"); }
			set { this.SetValue("Fetch Size", value); }
		}

		public FbServerType ServerType
		{
			get { return this.GetServerType("Server Type"); }
			set { this.SetValue("Server Type", value); }
		}

		public IsolationLevel IsolationLevel
		{
			get { return (IsolationLevel)this.GetInt32("Isolation Level"); }
			set { this.SetValue("Isolation Level", value); }
		}

		public bool ReturnRecordsAffected
		{
			get { return this.GetBoolean("Records Affected"); }
			set { this.SetValue("Records Affected", value); }
		}

		public bool ContextConnection
		{
			get { return this.GetBoolean("Context Connection"); }
			set { this.SetValue("Context Connection", value); }
		}

		public bool Enlist
		{            
			get { return this.GetBoolean("Enlist"); }
			set { this.SetValue("Enlist", value); }
		}

		public string ClientLibrary
		{
			get { return this.GetString("Client Library"); }
			set { this.SetValue("Client Library", value); }
		}

		public int DbCachePages
		{
			get { return this.GetInt32("Cache Pages"); }
			set { this.SetValue("Cache Pages", value); }
		}

		#endregion

		#region · Constructors ·

		public FbConnectionStringBuilder()
		{
		}

		public FbConnectionStringBuilder(string connectionString)
		{
			this.ConnectionString = connectionString;
		}

		#endregion

		#region · Private methods ·

		private int GetInt32(string keyword)
		{
			return Convert.ToInt32(this[this.GetKey(keyword)]);
		}

		private FbServerType GetServerType(string keyword)
		{
			object value = this[this.GetKey(keyword)];

			if (value is FbServerType)
			{
				return (FbServerType)value;
			}
			else if (value is string)
			{
				switch (value.ToString())
				{
					case "Default":
						return FbServerType.Default;

					case "Embedded":
						return FbServerType.Embedded;

					case "Context":
						return FbServerType.Context;
				}
			}

			return (FbServerType)this.GetInt32(keyword);
		}

		private string GetString(string keyword)
		{           
			return Convert.ToString(this[this.GetKey(keyword)]);
		}

		private bool GetBoolean(string keyword)
		{
			return Convert.ToBoolean(this[this.GetKey(keyword)]);
		}

		private void SetValue(string keyword, object value)
		{
			this[this.GetKey(keyword)] = value;
		}

		private string GetKey(string keyword)
		{
			string synonymKey = (string)FbConnectionString.Synonyms[keyword];

			// First check if there are yet a property for the requested keyword
			foreach (string key in this.Keys)
			{
				if (FbConnectionString.Synonyms.ContainsKey(key) && (string)FbConnectionString.Synonyms[key] == synonymKey)
				{
					synonymKey = key;
					break;
				}
			}

			return synonymKey;
		}

		#endregion
	}
}

#endif
