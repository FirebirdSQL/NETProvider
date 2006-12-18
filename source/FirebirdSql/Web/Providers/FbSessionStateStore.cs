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
 *	Copyright (c) 2005 Alessandro Petrelli
 *	All Rights Reserved.
 */

using System;
using System.Configuration;
using System.Configuration.Provider;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;

using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Web.Providers
{
	public sealed class FbSessionStateStore : SessionStateStoreProviderBase
	{
		#region · Fields ·

		private SessionStateSection config;
		private string connectionString;
		private string applicationName;

		#endregion

		#region · Properties ·

		public string ApplicationName
		{
			get { return this.applicationName; }
		}

		#endregion

		#region · Dispose Methods ·

		public override void Dispose()
		{
		}

		#endregion
		
		#region · Methods ·

		public override void Initialize(string name, NameValueCollection config)
		{
			if (config == null)
			{
				throw new ArgumentNullException("config");
			}

			if (String.IsNullOrEmpty(name))
			{
				name = "FbSessionStateStore";
			}

			base.Initialize(name, config);

			this.applicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;

			Configuration cfg = WebConfigurationManager.OpenWebConfiguration(ApplicationName);
			this.config = (SessionStateSection)cfg.GetSection("system.web/sessionState");

			ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings[config["connectionStringName"]];

			if (connectionStringSettings == null || String.IsNullOrEmpty(connectionStringSettings.ConnectionString))
			{
				throw new ProviderException("Connection string cannot be blank.");
			}

			this.connectionString = connectionStringSettings.ConnectionString;
		}

		public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
		{
			return false;
		}

		public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
		{
			string sessItems = Serialize((SessionStateItemCollection)item.Items);

			using (FbConnection cn = new FbConnection(this.connectionString))
			{
				cn.Open();

				if (newItem)
				{
					using (FbCommand cmd = cn.CreateCommand())
					{
						cmd.CommandText = "DELETE FROM SESSIONS WHERE SESSION_ID = @SESSION_ID AND APPLICATION_NAME = @APPLICATION_NAME AND EXPIRES < @EXPIRES";
						cmd.Parameters.Add("@SESSION_ID", FbDbType.VarChar, 80).Value = id;
						cmd.Parameters.Add("@APPLICATION_NAME", FbDbType.VarChar, 100).Value = ApplicationName;
						cmd.Parameters.Add("@EXPIRES", FbDbType.TimeStamp).Value = DateTime.Now;
						cmd.ExecuteNonQuery();
					}

					using (FbCommand cmd = cn.CreateCommand())
					{
						cmd.CommandText = "INSERT INTO SESSIONS (SESSION_ID, APPLICATION_NAME, CREATED, EXPIRES, " +
							"LOCK_DATE, LOCK_ID, TIMEOUT, LOCKED, SESSION_ITEMS, FLAGS) VALUES(@SESSION_ID, " +
							"@APPLICATION_NAME, @CREATED, @EXPIRES, @LOCK_DATE, @LOCK_ID , @TIMEOUT, @LOCKED, " +
							"@SESSION_ITEMS, @FLAGS)";
						cmd.Parameters.Add("@SESSION_ID", FbDbType.VarChar, 80).Value = id;
						cmd.Parameters.Add("@APPLICATION_NAME", FbDbType.VarChar, 100).Value = ApplicationName;
						cmd.Parameters.Add("@CREATED", FbDbType.TimeStamp).Value = DateTime.Now;
						cmd.Parameters.Add("@EXPIRES", FbDbType.TimeStamp).Value = DateTime.Now.AddMinutes((Double)item.Timeout);
						cmd.Parameters.Add("@LOCK_DATE", FbDbType.TimeStamp).Value = DateTime.Now;
						cmd.Parameters.Add("@LOCK_ID", FbDbType.Integer).Value = 0;
						cmd.Parameters.Add("@TIMEOUT", FbDbType.Integer).Value = item.Timeout;
						cmd.Parameters.Add("@LOCKED", FbDbType.SmallInt).Value = false;
						cmd.Parameters.Add("@SESSION_ITEMS", FbDbType.Text, sessItems.Length).Value = sessItems;
						cmd.Parameters.Add("@FLAGS", FbDbType.Integer).Value = 0;
						cmd.ExecuteNonQuery();
					}
				}
				else
				{
					using (FbCommand cmd = cn.CreateCommand())
					{
						cmd.CommandText = "UPDATE SESSIONS SET EXPIRES = @EXPIRES, SESSION_ITEMS = @SESSION_ITEMS, " +
							"LOCKED = @LOCKED WHERE SESSION_ID = @SESSION_ID AND APPLICATION_NAME = @APPLICATION_NAME AND " +
							"LOCK_ID = @LOCK_ID";
						cmd.Parameters.Add("@EXPIRES", FbDbType.TimeStamp).Value = DateTime.Now.AddMinutes((Double)item.Timeout);
						cmd.Parameters.Add("@SESSION_ITEMS", FbDbType.Text, sessItems.Length).Value = sessItems;
						cmd.Parameters.Add("@LOCKED", FbDbType.SmallInt).Value = false;
						cmd.Parameters.Add("@SESSION_ID", FbDbType.VarChar, 80).Value = id;
						cmd.Parameters.Add("@APPLICATION_NAME", FbDbType.VarChar, 100).Value = ApplicationName;
						cmd.Parameters.Add("@LOCK_ID", FbDbType.Integer).Value = lockId;
						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked,
			out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
		{
			return GetSessionStoreItem(false, context, id, out locked, out lockAge, out lockId, out actions);
		}

		public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked,
			out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
		{
			return GetSessionStoreItem(true, context, id, out locked, out lockAge, out lockId, out actions);
		}

		public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
		{
			using (FbConnection cn = new FbConnection(this.connectionString))
			{
				cn.Open();

				using (FbCommand cmd = cn.CreateCommand())
				{
					cmd.CommandText = "UPDATE SESSIONS SET LOCKED = 0, EXPIRES = @EXPIRES " +
						"WHERE SESSION_ID = @SESSION_ID AND APPLICATION_NAME = @APPLICATION_NAME AND LOCK_ID = @LOCK_ID";
					cmd.Parameters.Add("@EXPIRES", FbDbType.TimeStamp).Value = DateTime.Now.AddMinutes(this.config.Timeout.Minutes);
					cmd.Parameters.Add("@SESSION_ID", FbDbType.VarChar, 80).Value = id;
					cmd.Parameters.Add("@APPLICATION_NAME", FbDbType.VarChar, 100).Value = ApplicationName;
					cmd.Parameters.Add("@LOCK_ID", FbDbType.Integer).Value = lockId;
					cmd.ExecuteNonQuery();
				}
			}
		}

		public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
		{
			using (FbConnection cn = new FbConnection(this.connectionString))
			{
				cn.Open();

				using (FbCommand cmd = cn.CreateCommand())
				{
					cmd.CommandText = "DELETE FROM SESSIONS WHERE SESSION_ID = @SESSION_ID AND " +
						"APPLICATION_NAME = @APPLICATION_NAME AND LOCK_ID = @LOCK_ID";
					cmd.Parameters.Add("@SESSION_ID", FbDbType.VarChar, 80).Value = id;
					cmd.Parameters.Add("@APPLICATION_NAME", FbDbType.VarChar, 100).Value = ApplicationName;
					cmd.Parameters.Add("@LOCK_ID", FbDbType.Integer).Value = lockId;

					cmd.ExecuteNonQuery();
				}
			}
		}

		public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
		{
			using (FbConnection cn = new FbConnection(this.connectionString))
			{
				cn.Open();

				using (FbCommand cmd = cn.CreateCommand())
				{
					cmd.CommandText = "INSERT INTO SESSIONS(SESSION_ID, APPLICATION_NAME, CREATED, EXPIRES, " +
						"LOCK_DATE, LOCK_ID, TIMEOUT, LOCKED, SESSION_ITEMS, FLAGS) " +
						"VALUES (@SESSION_ID, @APPLICATION_NAME, @CREATED, @EXPIRES, @LOCK_DATE, @LOCK_ID, " +
						"@TIMEOUT, @LOCKED, @SESSION_ITEMS, @FLAGS)";
					cmd.Parameters.Add("@SESSION_ID", FbDbType.VarChar, 80).Value = id;
					cmd.Parameters.Add("@APPLICATION_NAME", FbDbType.VarChar, 100).Value = ApplicationName;
					cmd.Parameters.Add("@CREATED", FbDbType.TimeStamp).Value = DateTime.Now;
					cmd.Parameters.Add("@EXPIRES", FbDbType.TimeStamp).Value = DateTime.Now.AddMinutes((Double)timeout);
					cmd.Parameters.Add("@LOCK_DATE", FbDbType.TimeStamp).Value = DateTime.Now;
					cmd.Parameters.Add("@LOCK_ID", FbDbType.Integer).Value = 0;
					cmd.Parameters.Add("@TIMEOUT", FbDbType.Integer).Value = timeout;
					cmd.Parameters.Add("@LOCKED", FbDbType.SmallInt).Value = false;
					cmd.Parameters.Add("@SESSION_ITEMS", FbDbType.Text, 0).Value = "";
					cmd.Parameters.Add("@FLAGS", FbDbType.Integer).Value = 1;

					cmd.ExecuteNonQuery();
				}
			}
		}

		public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
		{
			return new SessionStateStoreData(new SessionStateItemCollection(), SessionStateUtility.GetSessionStaticObjects(context), timeout);
		}

		public override void ResetItemTimeout(HttpContext context, string id)
		{
			using (FbConnection cn = new FbConnection(this.connectionString))
			{
				cn.Open();

				using (FbCommand cmd = cn.CreateCommand())
				{
					cmd.CommandText = "UPDATE SESSIONS SET EXPIRES = @EXPIRES WHERE SESSION_ID = @SESSION_ID AND " + 
						"APPLICATION_NAME = @APPLICATION_NAME";
					cmd.Parameters.Add("@EXPIRES", FbDbType.TimeStamp).Value = DateTime.Now.AddMinutes(this.config.Timeout.Minutes);
					cmd.Parameters.Add("@SESSION_ID", FbDbType.VarChar, 80).Value = id;
					cmd.Parameters.Add("@APPLICATION_NAME", FbDbType.VarChar, 100).Value = ApplicationName;

					cmd.ExecuteNonQuery();
				}
			}
		}

		public override void InitializeRequest(HttpContext context)
		{
		}

		public override void EndRequest(HttpContext context)
		{
		}

		#endregion

		#region · Private Methods ·

		private SessionStateStoreData GetSessionStoreItem(bool lockRecord, HttpContext context, string id,
			out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
		{
			locked = false;
			lockAge = TimeSpan.Zero;
			lockId = null;
			actionFlags = 0;

			SessionStateStoreData item = null;

			using (FbConnection cn = new FbConnection(this.connectionString))
			{
				DateTime expires;
				string serializedItems = "";
				bool foundRecord = false;
				bool deleteData = false;
				int timeout = 0;

				cn.Open();

				if (lockRecord)
				{
					using (FbCommand cmd = cn.CreateCommand())
					{
						cmd.CommandText = "UPDATE SESSIONS SET LOCKED = @LOCKED, LOCK_DATE = @LOCK_DATE " +
							"WHERE SESSION_ID = @SESSION_ID AND APPLICATION_NAME = @APPLICATION_NAME AND " +
							"LOCKED = @LOCKED2 AND EXPIRES > @EXPIRES";
						cmd.Parameters.Add("@LOCKED", FbDbType.SmallInt).Value = true;
						cmd.Parameters.Add("@LOCK_DATE", FbDbType.TimeStamp).Value = DateTime.Now;
						cmd.Parameters.Add("@SESSION_ID", FbDbType.VarChar, 80).Value = id;
						cmd.Parameters.Add("@APPLICATION_NAME", FbDbType.VarChar, 100).Value = ApplicationName;
						cmd.Parameters.Add("@LOCKED2", FbDbType.Integer).Value = false;
						cmd.Parameters.Add("@EXPIRES", FbDbType.TimeStamp).Value = DateTime.Now;

						if (cmd.ExecuteNonQuery() == 0)
						{
							locked = true;
						}
						else
						{
							locked = false;
						}
					}
				}

				using (FbCommand cmd = cn.CreateCommand())
				{
					cmd.CommandText = "SELECT EXPIRES, SESSION_ITEMS, LOCK_ID, LOCK_DATE, FLAGS, TIMEOUT " +
						"FROM SESSIONS WHERE SESSION_ID = @SESSION_ID AND APPLICATION_NAME = @APPLICATION_NAME";
					cmd.Parameters.Add("@SESSION_ID", FbDbType.VarChar, 80).Value = id;
					cmd.Parameters.Add("@APPLICATION_NAME", FbDbType.VarChar, 100).Value = ApplicationName;

					using (FbDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
					{
						while (reader.Read())
						{
							expires = reader.GetDateTime(0);

							if (expires < DateTime.Now)
							{
								locked = false;
								deleteData = true;
							}
							else
							{
								foundRecord = true;
							}

							serializedItems = reader.GetString(1);
							lockId = reader.GetInt32(2);
							lockAge = DateTime.Now.Subtract(reader.GetDateTime(3));
							actionFlags = (SessionStateActions)reader.GetInt32(4);
							timeout = reader.GetInt32(5);
						}

						if (!foundRecord)
						{
							locked = false;
						}
					}
				}

				if (deleteData)
				{
					using (FbCommand cmd = cn.CreateCommand())
					{
						cmd.CommandText = "DELETE FROM SESSIONS WHERE SESSION_ID = @SESSION_ID AND APPLICATION_NAME = @APPLICATION_NAME";
						cmd.Parameters.Add("@SESSION_ID", FbDbType.VarChar, 80).Value = id;
						cmd.Parameters.Add("@APPLICATION_NAME", FbDbType.VarChar, 100).Value = ApplicationName;
						cmd.ExecuteNonQuery();
					}
				}

				if (foundRecord && !locked)
				{
					lockId = (int)lockId + 1;

					using (FbCommand cmd = cn.CreateCommand())
					{

						cmd.CommandText = "UPDATE SESSIONS SET LOCK_ID = @LOCK_ID, Flags = 0 " +
							"WHERE SESSION_ID = @SESSION_ID AND APPLICATION_NAME = @APPLICATION_NAME";
						cmd.Parameters.Add("@LOCK_ID", FbDbType.Integer).Value = lockId;
						cmd.Parameters.Add("@SESSION_ID", FbDbType.VarChar, 80).Value = id;
						cmd.Parameters.Add("@APPLICATION_NAME", FbDbType.VarChar, 100).Value = ApplicationName;

						cmd.ExecuteNonQuery();
					}

					if (actionFlags == SessionStateActions.InitializeItem)
					{
						item = CreateNewStoreData(context, this.config.Timeout.Minutes);
					}
					else
					{
						item = Deserialize(context, serializedItems, timeout);
					}
				}
			}

			return item;
		}

		private string Serialize(SessionStateItemCollection items)
		{
			MemoryStream ms = new MemoryStream();

			using (BinaryWriter writer = new BinaryWriter(ms))
			{
				if (items != null)
				{
					items.Serialize(writer);
				}
			}

			return Convert.ToBase64String(ms.ToArray());
		}

		private SessionStateStoreData Deserialize(HttpContext context, string serializedItems, int timeout)
		{
			MemoryStream ms = new MemoryStream(Convert.FromBase64String(serializedItems));

			SessionStateItemCollection sessionItems;

			using (BinaryReader reader = new BinaryReader(ms))
			{
				sessionItems = SessionStateItemCollection.Deserialize(reader);
			}

			return new SessionStateStoreData(sessionItems, SessionStateUtility.GetSessionStaticObjects(context), timeout);
		}

		#endregion
	}
}