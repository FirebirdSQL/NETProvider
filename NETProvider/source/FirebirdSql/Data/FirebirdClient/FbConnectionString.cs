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
 *	Copyright (c) 2004-2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 *	
 *  Contributors:
 *      Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.FirebirdClient
{
	internal sealed class FbConnectionString
	{
		#region · Static Fields ·

		public static readonly IDictionary<string, string> Synonyms = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) 
		{
			{"data source", "data source"},
			{"datasource", "data source"},
			{"server", "data source"},
			{"host", "data source"},
			{"port", "port number"},
			{"port number", "port number"},
			{"database", "initial catalog"},
			{"initial catalog", "initial catalog"},
			{"user id", "user id"},
			{"userid", "user id"},
			{"uid", "user id"},
			{"user", "user id"},
			{"user name", "user id"},
			{"username", "user id"},
			{"password", "password"},
			{"user password", "password"},
			{"userpassword", "password"},
			{"dialect", "dialect"},
			{"pooling", "pooling"},
			{"max pool size", "max pool size"},
			{"maxpoolsize", "max pool size"},
			{"min pool size", "min pool size"},
			{"minpoolsize", "min pool size"},
			{"character set", "character set"},
			{"charset", "character set"},
			{"connection lifetime", "connection lifetime"},
			{"connectionlifetime", "connection lifetime"},
			{"timeout", "connection timeout"},
			{"connection timeout", "connection timeout"},
			{"connectiontimeout", "connection timeout"},
			{"packet size", "packet size"},
			{"packetsize", "packet size"},
			{"role", "role name"},
			{"role name", "role name"},
			{"fetch size", "fetch size"},
			{"fetchsize", "fetch size"},
			{"server type", "server type"},
			{"servertype", "server type"},
			{"isolation level", "isolation level"},
			{"isolationlevel", "isolation level"},
			{"records affected", "records affected"},
			{"context connection", "context connection"},
			{"enlist", "enlist"},
			{"clientlibrary", "client library"},
			{"client library", "client library"},
			{"cache pages", "cache pages"},
			{"cachepages", "cache pages"},
			{"pagebuffers", "cache pages"},
			{"page buffers", "cache pages"},
		};

		#endregion

		#region · Fields ·

		private Dictionary<string, object> options;
		private bool isServiceConnectionString;

		#endregion

		#region · Properties ·

		public string UserID
		{
			get { return this.GetString("user id"); }
		}

		public string Password
		{
			get { return this.GetString("password"); }
		}

		public string DataSource
		{
			get { return this.GetString("data source"); }
		}

		public int Port
		{
			get { return this.GetInt32("port number"); }
		}

		public string Database
		{
			get { return ExpandDataDirectory(this.GetString("initial catalog")); }
		}

		public short PacketSize
		{
			get { return this.GetInt16("packet size"); }
		}

		public string Role
		{
			get { return this.GetString("role name"); }
		}

		public byte Dialect
		{
			get { return this.GetByte("dialect"); }
		}

		public string Charset
		{
			get { return this.GetString("character set"); }
		}

		public int ConnectionTimeout
		{
			get { return this.GetInt32("connection timeout"); }
		}

		public bool Pooling
		{
			get { return this.GetBoolean("pooling"); }
		}

		public long ConnectionLifeTime
		{
			get { return this.GetInt64("connection lifetime"); }
		}

		public int MinPoolSize
		{
			get { return this.GetInt32("min pool size"); }
		}

		public int MaxPoolSize
		{
			get { return this.GetInt32("max pool size"); }
		}

		public int FetchSize
		{
			get { return this.GetInt32("fetch size"); }
		}

		public FbServerType ServerType
		{
			get { return (FbServerType)this.GetInt32("server type"); }
		}

		public IsolationLevel IsolationLevel
		{
			get { return this.GetIsolationLevel("isolation level"); }
		}

		public bool ReturnRecordsAffected
		{
			get { return this.GetBoolean("records affected"); }
		}

		public bool ContextConnection
		{
			get { return this.GetBoolean("context connection"); }
		}

		public bool Enlist
		{
			get { return this.GetBoolean("enlist"); }
		}

		public string ClientLibrary
		{
			get { return this.GetString("client library"); }
		}

		public int DbCachePages
		{
			get { return this.GetInt32("cache pages"); }
		}

		#endregion

		#region · Internal Properties ·
		internal bool FallIntoTrustedAuth
		{
			// on non-Win the UserID/Password is checked in Validate method
			get { return string.IsNullOrEmpty(this.UserID) && string.IsNullOrEmpty(this.Password); }
		}
		#endregion

		#region · Constructors ·

		public FbConnectionString()
		{
			this.SetDefaultOptions();
		}

		public FbConnectionString(string connectionString)
		{
			this.Load(connectionString);
		}

		internal FbConnectionString(bool isServiceConnectionString)
		{
			this.isServiceConnectionString = isServiceConnectionString;
			this.SetDefaultOptions();
		}

		#endregion

		#region · Methods ·

		public void Load(string connectionString)
		{
			this.SetDefaultOptions();

			if (connectionString != null && connectionString.Length > 0)
			{
				MatchCollection keyPairs = Regex.Matches(connectionString, @"([\w\s\d]*)\s*=\s*([^;]*)");

				foreach (Match keyPair in keyPairs)
				{
					if (keyPair.Groups.Count == 3)
					{
						string[] values = new string[] 
						{
							keyPair.Groups[1].Value.Trim(),
							keyPair.Groups[2].Value.Trim()
						};

						if (values.Length == 2 &&
							values[0] != null && values[0].Length > 0 &&
							values[1] != null && values[1].Length > 0)
						{
							values[0] = values[0].ToLower(CultureInfo.InvariantCulture);

							if (Synonyms.ContainsKey(values[0]))
							{
								string key = Synonyms[values[0]];
								if (key == "server type")
								{
									switch (this.UnquoteString(values[1].Trim()))
									{
										case "Default":
											this.options[key] = FbServerType.Default;
											break;

										case "Embedded":
											this.options[key] = FbServerType.Embedded;
											break;

										case "Context":
											this.options[key] = FbServerType.Context;
											break;

										default:
											this.options[key] = this.UnquoteString(values[1].Trim());
											break;
									}
								}
								else
								{
									this.options[key] = this.UnquoteString(values[1].Trim());
								}
							}
						}
					}
				}

				if (this.ContextConnection || this.ServerType == FbServerType.Context)
				{
					// When Context connection is true we should get the currently active connection
					// on the Firebird Server
					this.options["server type"] = FbServerType.Context;
					this.options["pooling"] = false;
					this.options["context connection"] = true;
				}
				else
				{
					if (this.Database != null && this.Database.Length > 0)
					{
						this.ParseConnectionInfo(this.Database);
					}
				}
			}
		}

		public void Validate()
		{
			if (!this.ContextConnection)
			{
				if (
#if (LINUX)  // on Linux Trusted Auth isn't available
                    (this.UserID == null || this.UserID.Length == 0) ||
                    (this.Password == null || this.Password.Length == 0) ||
#endif
					((this.Database == null || this.Database.Length == 0) && !this.isServiceConnectionString) ||
					((this.DataSource == null || this.DataSource.Length == 0) && this.ServerType != FbServerType.Embedded) ||
					(this.Charset == null || this.Charset.Length == 0) ||
					(this.Port == 0) ||
					(!Enum.IsDefined(typeof(FbServerType), this.ServerType)) ||
					(this.MinPoolSize > this.MaxPoolSize)
				   )
				{
					throw new ArgumentException("An invalid connection string argument has been supplied or a required connection string argument has not been supplied.");
				}
				else
				{
					if (this.Dialect < 1 || this.Dialect > 3)
					{
						throw new ArgumentException("Incorrect database dialect it should be 1, 2, or 3.");
					}
					if (this.PacketSize < 512 || this.PacketSize > 32767)
					{
#if (!NET_CF)
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "'Packet Size' value of {0} is not valid.{1}The value should be an integer >= 512 and <= 32767.", this.PacketSize, Environment.NewLine));
#else
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "'Packet Size' value of {0} is not valid.{1}The value should be an integer >= 512 and <= 32767.", this.PacketSize, "\r\n"));
#endif
					}
					if (this.DbCachePages < 0)
					{
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "'Db Cache Pages' value of {0} is not valid.{1}The value should be an integer >= 0.", this.DbCachePages, Environment.NewLine));
					}

					this.CheckIsolationLevel();
				}
			}
		}

		#endregion

		#region · Private Methods ·

		private void SetDefaultOptions()
		{
			if (this.options == null)
			{
				this.options = new Dictionary<string, object>();
			}

			this.options.Clear();

			// Add default key pairs values
			this.options.Add("data source", string.Empty);
			this.options.Add("port number", 3050);
			this.options.Add("user id", string.Empty);
			this.options.Add("password", string.Empty);
			this.options.Add("role name", string.Empty);
			this.options.Add("catalog", string.Empty);
			this.options.Add("character set", "NONE");
			this.options.Add("dialect", 3);
			this.options.Add("packet size", 8192);
			this.options.Add("pooling", true);
			this.options.Add("connection lifetime", 0);
			this.options.Add("min pool size", 0);
			this.options.Add("max pool size", 100);
			this.options.Add("connection timeout", 15);
			this.options.Add("fetch size", 200);
			this.options.Add("server type", FbServerType.Default);
			this.options.Add("isolation level", IsolationLevel.ReadCommitted.ToString());
			this.options.Add("records affected", true);
			this.options.Add("context connection", false);
			this.options.Add("enlist", false);
			this.options.Add("client library", "fbembed");
			this.options.Add("cache pages", 0);
		}

		private void ParseConnectionInfo(string connectInfo)
		{
			string database = null;
			string dataSource = null;
			int portNumber = -1;

			// allows standard syntax //host:port/....
			// and old fb syntax host/port:....
			connectInfo = connectInfo.Trim();
			char hostSepChar;
			char portSepChar;

			if (connectInfo.StartsWith("//"))
			{
				connectInfo = connectInfo.Substring(2);
				hostSepChar = '/';
				portSepChar = ':';
			}
			else
			{
				hostSepChar = ':';
				portSepChar = '/';
			}

			int sep = connectInfo.IndexOf(hostSepChar);
			if (sep == 0 || sep == connectInfo.Length - 1)
			{
				throw new ArgumentException("An invalid connection string argument has been supplied or a required connection string argument has not been supplied.");
			}
			else if (sep > 0)
			{
				dataSource = connectInfo.Substring(0, sep);
				database = connectInfo.Substring(sep + 1);
				int portSep = dataSource.IndexOf(portSepChar);

				if (portSep == 0 || portSep == dataSource.Length - 1)
				{
					throw new ArgumentException("An invalid connection string argument has been supplied or a required connection string argument has not been supplied.");
				}
				else if (portSep > 0)
				{
					portNumber = Int32.Parse(dataSource.Substring(portSep + 1), CultureInfo.InvariantCulture);
					dataSource = dataSource.Substring(0, portSep);
				}
				else if (portSep < 0 && dataSource.Length == 1)
				{
					if (this.DataSource == null || this.DataSource.Length == 0)
					{
						dataSource = "localhost";
					}
					else
					{
						dataSource = null;
					}

					database = connectInfo;
				}
			}
			else if (sep == -1)
			{
				database = connectInfo;
			}

			this.options["initial catalog"] = database;
			if (dataSource != null)
			{
				this.options["data source"] = dataSource;
			}
			if (portNumber != -1)
			{
				this.options["port number"] = portNumber;
			}
		}

		private string ExpandDataDirectory(string s)
		{
			const string dataDirectoryKeyword = "|DataDirectory|";

			if (s == null)
				return s;

#if (!NET_CF)
			string dataDirectoryLocation = (string)AppDomain.CurrentDomain.GetData("DataDirectory") ?? string.Empty;
			string pattern = string.Format("{0}{1}?", Regex.Escape(dataDirectoryKeyword), Regex.Escape(Path.DirectorySeparatorChar.ToString()));
			return Regex.Replace(s, pattern, dataDirectoryLocation + Path.DirectorySeparatorChar, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
#else
            if (s.ToUpper(CultureInfo.InvariantCulture).IndexOf(dataDirectoryKeyword.ToUpper(CultureInfo.InvariantCulture)) != -1 )
                throw new NotImplementedException();

            return s;
#endif
		}

		private string GetString(string key)
		{
			object value;
			if (this.options.TryGetValue(key, out value))
				return (string)value;
			else
				return null;
		}

		private bool GetBoolean(string key)
		{
			object value;
			if (this.options.TryGetValue(key, out value))
				return Boolean.Parse(value.ToString());
			else
				return false;
		}

		private byte GetByte(string key)
		{
			object value;
			if (this.options.TryGetValue(key, out value))
				return Convert.ToByte(value, CultureInfo.CurrentCulture);
			else
				return 0;
		}

		private short GetInt16(string key)
		{
			object value;
			if (this.options.TryGetValue(key, out value))
				return Convert.ToInt16(value, CultureInfo.InvariantCulture);
			else
				return 0;
		}

		private int GetInt32(string key)
		{
			object value;
			if (this.options.TryGetValue(key, out value))
				return Convert.ToInt32(value, CultureInfo.InvariantCulture);
			else
				return 0;
		}

		private long GetInt64(string key)
		{
			object value;
			if (this.options.TryGetValue(key, out value))
				return Convert.ToInt64(value, CultureInfo.InvariantCulture);
			else
				return 0;
		}

		private IsolationLevel GetIsolationLevel(string key)
		{
			object value;
			if (this.options.TryGetValue(key, out value))
			{
				string il = value.ToString().ToLower(CultureInfo.InvariantCulture);

				switch (il)
				{
					case "readcommitted":
						return IsolationLevel.ReadCommitted;

					case "readuncommitted":
						return IsolationLevel.ReadUncommitted;

					case "repeatableread":
						return IsolationLevel.RepeatableRead;

					case "serializable":
						return IsolationLevel.Serializable;

					case "chaos":
						return IsolationLevel.Chaos;

					case "snapshot":
						return IsolationLevel.Snapshot;

					case "unspecified":
						return IsolationLevel.Unspecified;
				}
			}

			return IsolationLevel.ReadCommitted;
		}

		private void CheckIsolationLevel()
		{
			string il = this.options["isolation level"].ToString().ToLower(CultureInfo.InvariantCulture);

			switch (il)
			{
				case "readcommitted":
				case "readuncommitted":
				case "repeatableread":
				case "serializable":
				case "chaos":
				case "unspecified":
				case "snapshot":
					break;

				default:
					throw new ArgumentException("Specified Isolation Level is not valid.");
			}
		}

		private string UnquoteString(string value)
		{
			string unquoted = value;

			if (unquoted.StartsWith("\""))
			{
				unquoted = unquoted.Remove(0, 1);
			}
			if (unquoted.EndsWith("\""))
			{
				unquoted = unquoted.Remove(unquoted.Length - 1, 1);
			}

			return unquoted;
		}

		#endregion
	}
}
