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
 *  Copyright (c) 2007 Jiri Cincura (jiri@cincura.net)
 *	All Rights Reserved.
 *	 
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

			if (string.IsNullOrEmpty(name))
			{
				name = "FbSessionStateStore";
			}

            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "FB Session State Store");
            }

			base.Initialize(name, config);

			if (config["applicationName"] == null || config["applicationName"].Trim() == "")
            {
                this.applicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
            }
            else
            {
                this.applicationName = config["applicationName"];
            }


			Configuration cfg = WebConfigurationManager.OpenWebConfiguration(ApplicationName);
			this.config = (SessionStateSection)cfg.GetSection("system.web/sessionState");

			ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings[config["connectionStringName"]];

			if (connectionStringSettings == null || string.IsNullOrEmpty(connectionStringSettings.ConnectionString))
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
			string sessionItems = Serialize((SessionStateItemCollection)item.Items);

			using (FbConnection conn = new FbConnection(this.connectionString))
			{
				conn.Open();

				if (newItem)
				{
					using (FbCommand cmd = conn.CreateCommand())
					{
						cmd.CommandText = "DELETE FROM SESSIONS WHERE SESSIONID = @SESSIONID AND APPLICATIONNAME = @APPLICATIONNAME AND EXPIRES < @EXPIRES";
						cmd.Parameters.Add("@SESSIONID", FbDbType.VarChar, 80).Value = id;
						cmd.Parameters.Add("@APPLICATIONNAME", FbDbType.VarChar, 100).Value = ApplicationName;
						cmd.Parameters.Add("@EXPIRES", FbDbType.TimeStamp).Value = DateTime.Now;
						cmd.ExecuteNonQuery();
					}

					using (FbCommand cmd = conn.CreateCommand())
					{
						cmd.CommandText = "INSERT INTO SESSIONS (SESSIONID, APPLICATIONNAME, CREATED, EXPIRES, " +
							"LOCKDATE, LOCKID, TIMEOUT, LOCKED, SESSIONITEMS, FLAGS) VALUES(@SESSIONID, " +
							"@APPLICATIONNAME, @CREATED, @EXPIRES, @LOCKDATE, @LOCKID , @TIMEOUT, @LOCKED, " +
							"@SESSIONITEMS, @FLAGS)";
						cmd.Parameters.Add("@SESSIONID", FbDbType.VarChar, 80).Value = id;
						cmd.Parameters.Add("@APPLICATIONNAME", FbDbType.VarChar, 100).Value = ApplicationName;
						cmd.Parameters.Add("@CREATED", FbDbType.TimeStamp).Value = DateTime.Now;
						cmd.Parameters.Add("@EXPIRES", FbDbType.TimeStamp).Value = DateTime.Now.AddMinutes((Double)item.Timeout);
						cmd.Parameters.Add("@LOCKDATE", FbDbType.TimeStamp).Value = DateTime.Now;
						cmd.Parameters.Add("@LOCKID", FbDbType.Integer).Value = 0;
						cmd.Parameters.Add("@TIMEOUT", FbDbType.Integer).Value = item.Timeout;
						cmd.Parameters.Add("@LOCKED", FbDbType.SmallInt).Value = false;
						cmd.Parameters.Add("@SESSIONITEMS", FbDbType.Text, sessionItems.Length).Value = sessionItems;
						cmd.Parameters.Add("@FLAGS", FbDbType.Integer).Value = 0;
						cmd.ExecuteNonQuery();
					}
				}
				else
				{
					using (FbCommand cmd = conn.CreateCommand())
					{
						cmd.CommandText = "UPDATE SESSIONS SET EXPIRES = @EXPIRES, SESSIONITEMS = @SESSIONITEMS, " +
							"LOCKED = @LOCKED WHERE SESSIONID = @SESSIONID AND APPLICATIONNAME = @APPLICATIONNAME AND " +
							"LOCKID = @LOCKID";
						cmd.Parameters.Add("@EXPIRES", FbDbType.TimeStamp).Value = DateTime.Now.AddMinutes((Double)item.Timeout);
						cmd.Parameters.Add("@SESSIONITEMS", FbDbType.Text, sessionItems.Length).Value = sessionItems;
						cmd.Parameters.Add("@LOCKED", FbDbType.SmallInt).Value = false;
						cmd.Parameters.Add("@SESSIONID", FbDbType.VarChar, 80).Value = id;
                        cmd.Parameters.Add("@APPLICATIONNAME", FbDbType.VarChar, 100).Value = ApplicationName;
						cmd.Parameters.Add("@LOCKID", FbDbType.Integer).Value = lockId;
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
			using (FbConnection conn = new FbConnection(this.connectionString))
			{
				conn.Open();

				using (FbCommand cmd = conn.CreateCommand())
				{
					cmd.CommandText = "UPDATE SESSIONS SET LOCKED = 0, EXPIRES = @EXPIRES " +
						"WHERE SESSIONID = @SESSIONID AND APPLICATIONNAME = @APPLICATIONNAME AND LOCKID = @LOCKID";
					cmd.Parameters.Add("@EXPIRES", FbDbType.TimeStamp).Value = DateTime.Now.AddMinutes(this.config.Timeout.Minutes);
					cmd.Parameters.Add("@SESSIONID", FbDbType.VarChar, 80).Value = id;
                    cmd.Parameters.Add("@APPLICATIONNAME", FbDbType.VarChar, 100).Value = ApplicationName;
					cmd.Parameters.Add("@LOCKID", FbDbType.Integer).Value = lockId;
					cmd.ExecuteNonQuery();
				}
			}
		}

		public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
		{
			using (FbConnection conn = new FbConnection(this.connectionString))
			{
				conn.Open();

				using (FbCommand cmd = conn.CreateCommand())
				{
					cmd.CommandText = "DELETE FROM SESSIONS WHERE SESSIONID = @SESSIONID AND " +
						"APPLICATIONNAME = @APPLICATIONNAME AND LOCKID = @LOCKID";
					cmd.Parameters.Add("@SESSIONID", FbDbType.VarChar, 80).Value = id;
                    cmd.Parameters.Add("@APPLICATIONNAME", FbDbType.VarChar, 100).Value = ApplicationName;
					cmd.Parameters.Add("@LOCKID", FbDbType.Integer).Value = lockId;

					cmd.ExecuteNonQuery();
				}
			}
		}

		public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
		{
			using (FbConnection conn = new FbConnection(this.connectionString))
			{
				conn.Open();

				using (FbCommand cmd = conn.CreateCommand())
				{
					cmd.CommandText = "INSERT INTO SESSIONS(SESSIONID, APPLICATIONNAME, CREATED, EXPIRES, " +
						"LOCKDATE, LOCKID, TIMEOUT, LOCKED, SESSIONITEMS, FLAGS) " +
						"VALUES (@SESSIONID, @APPLICATIONNAME, @CREATED, @EXPIRES, @LOCKDATE, @LOCKID, " +
						"@TIMEOUT, @LOCKED, @SESSIONITEMS, @FLAGS)";
					cmd.Parameters.Add("@SESSIONID", FbDbType.VarChar, 80).Value = id;
                    cmd.Parameters.Add("@APPLICATIONNAME", FbDbType.VarChar, 100).Value = ApplicationName;
					cmd.Parameters.Add("@CREATED", FbDbType.TimeStamp).Value = DateTime.Now;
					cmd.Parameters.Add("@EXPIRES", FbDbType.TimeStamp).Value = DateTime.Now.AddMinutes((Double)timeout);
					cmd.Parameters.Add("@LOCKDATE", FbDbType.TimeStamp).Value = DateTime.Now;
					cmd.Parameters.Add("@LOCKID", FbDbType.Integer).Value = 0;
					cmd.Parameters.Add("@TIMEOUT", FbDbType.Integer).Value = timeout;
					cmd.Parameters.Add("@LOCKED", FbDbType.SmallInt).Value = false;
					cmd.Parameters.Add("@SESSIONITEMS", FbDbType.Text, 0).Value = "";
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
			using (FbConnection conn = new FbConnection(this.connectionString))
			{
				conn.Open();

				using (FbCommand cmd = conn.CreateCommand())
				{
					cmd.CommandText = "UPDATE SESSIONS SET EXPIRES = @EXPIRES WHERE SESSIONID = @SESSIONID AND " + 
						"APPLICATIONNAME = @APPLICATIONNAME";
					cmd.Parameters.Add("@EXPIRES", FbDbType.TimeStamp).Value = DateTime.Now.AddMinutes(this.config.Timeout.Minutes);
					cmd.Parameters.Add("@SESSIONID", FbDbType.VarChar, 80).Value = id;
                    cmd.Parameters.Add("@APPLICATIONNAME", FbDbType.VarChar, 100).Value = ApplicationName;

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

			using (FbConnection conn = new FbConnection(this.connectionString))
			{
				DateTime expires;
				string serializedItems = "";
				bool foundRecord = false;
				bool deleteData = false;
				int timeout = 0;

				conn.Open();

				if (lockRecord)
				{
					using (FbCommand cmd = conn.CreateCommand())
					{
						cmd.CommandText = "UPDATE SESSIONS SET LOCKED = @LOCKED, LOCKDATE = @LOCKDATE " +
							"WHERE SESSIONID = @SESSIONID AND APPLICATIONNAME = @APPLICATIONNAME AND " +
							"LOCKED = @LOCKED2 AND EXPIRES > @EXPIRES";
						cmd.Parameters.Add("@LOCKED", FbDbType.SmallInt).Value = true;
						cmd.Parameters.Add("@LOCKDATE", FbDbType.TimeStamp).Value = DateTime.Now;
						cmd.Parameters.Add("@SESSIONID", FbDbType.VarChar, 80).Value = id;
                        cmd.Parameters.Add("@APPLICATIONNAME", FbDbType.VarChar, 100).Value = ApplicationName;
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

				using (FbCommand cmd = conn.CreateCommand())
				{
					cmd.CommandText = "SELECT EXPIRES, SESSIONITEMS, LOCKID, LOCKDATE, FLAGS, TIMEOUT " +
						"FROM SESSIONS WHERE SESSIONID = @SESSIONID AND APPLICATIONNAME = @APPLICATIONNAME";
					cmd.Parameters.Add("@SESSIONID", FbDbType.VarChar, 80).Value = id;
                    cmd.Parameters.Add("@APPLICATIONNAME", FbDbType.VarChar, 100).Value = ApplicationName;

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
					using (FbCommand cmd = conn.CreateCommand())
					{
						cmd.CommandText = "DELETE FROM SESSIONS WHERE SESSIONID = @SESSIONID AND APPLICATIONNAME = @APPLICATIONNAME";
						cmd.Parameters.Add("@SESSIONID", FbDbType.VarChar, 80).Value = id;
                        cmd.Parameters.Add("@APPLICATIONNAME", FbDbType.VarChar, 100).Value = ApplicationName;
						cmd.ExecuteNonQuery();
					}
				}

				if (foundRecord && !locked)
				{
					lockId = (int)lockId + 1;

					using (FbCommand cmd = conn.CreateCommand())
					{

						cmd.CommandText = "UPDATE SESSIONS SET LOCKID = @LOCKID, FLAGS = 0 " +
							"WHERE SESSIONID = @SESSIONID AND APPLICATIONNAME = @APPLICATIONNAME";
						cmd.Parameters.Add("@LOCKID", FbDbType.Integer).Value = lockId;
						cmd.Parameters.Add("@SESSIONID", FbDbType.VarChar, 80).Value = id;
                        cmd.Parameters.Add("@APPLICATIONNAME", FbDbType.VarChar, 100).Value = ApplicationName;

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
            SessionStateItemCollection sessionItems;

            using (MemoryStream memory = new MemoryStream(Convert.FromBase64String(serializedItems)))
            {
                using (BinaryReader reader = new BinaryReader(memory))
                {
                    sessionItems = SessionStateItemCollection.Deserialize(reader);
                }
            }

			return new SessionStateStoreData(sessionItems, SessionStateUtility.GetSessionStaticObjects(context), timeout);
		}

		#endregion
	}
}