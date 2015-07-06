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
 *	Copyright (c) 2014-2015 Jiri Cincura (jiri@cincura.net)
 *	All Rights Reserved.
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.FirebirdClient
{
	internal sealed class FbConnectionString
	{
		#region Constants
		internal const string DefaultDataSource = "";
		internal const int DefaultPortNumber = 3050;
		internal const string DefaultUserId = "";
		internal const string DefaultPassword = "";
		internal const string DefaultRoleName = "";
		internal const string DefaultCatalog = "";
		internal const string DefaultCharacterSet = "NONE";
		internal const int DefaultDialect = 3;
		internal const int DefaultPacketSize = 8192;
		internal const bool DefaultPooling = true;
		internal const int DefaultConnectionLifetime = 0;
		internal const int DefaultMinPoolSize = 0;
		internal const int DefaultMaxPoolSize = 100;
		internal const int DefaultConnectionTimeout = 15;
		internal const int DefaultFetchSize = 200;
		internal const FbServerType DefaultServerType = FbServerType.Default;
		internal const IsolationLevel DefaultIsolationLevel = IsolationLevel.ReadCommitted;
		internal const bool DefaultRecordsAffected = true;
		internal const bool DefaultContextConnection = false;
		internal const bool DefaultEnlist = false;
		internal const string DefaultClientLibrary = "fbembed";
		internal const int DefaultCachePages = 0;
		internal const bool DefaultNoDbTriggers = false;
		internal const bool DefaultNoGarbageCollect = false;
		#endregion

		#region Static Fields

		public static readonly IDictionary<string, string> Synonyms = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
		{
			{ "data source", "data source" },
			{ "datasource", "data source" },
			{ "server", "data source" },
			{ "host", "data source" },
			{ "port", "port number" },
			{ "port number", "port number" },
			{ "database", "initial catalog" },
			{ "initial catalog", "initial catalog" },
			{ "user id", "user id" },
			{ "userid", "user id" },
			{ "uid", "user id" },
			{ "user", "user id" },
			{ "user name", "user id" },
			{ "username", "user id" },
			{ "password", "password" },
			{ "user password", "password" },
			{ "userpassword", "password" },
			{ "dialect", "dialect" },
			{ "pooling", "pooling" },
			{ "max pool size", "max pool size" },
			{ "maxpoolsize", "max pool size" },
			{ "min pool size", "min pool size" },
			{ "minpoolsize", "min pool size" },
			{ "character set", "character set" },
			{ "charset", "character set" },
			{ "connection lifetime", "connection lifetime" },
			{ "connectionlifetime", "connection lifetime" },
			{ "timeout", "connection timeout" },
			{ "connection timeout", "connection timeout" },
			{ "connectiontimeout", "connection timeout" },
			{ "packet size", "packet size" },
			{ "packetsize", "packet size" },
			{ "role", "role name" },
			{ "role name", "role name" },
			{ "fetch size", "fetch size" },
			{ "fetchsize", "fetch size" },
			{ "server type", "server type" },
			{ "servertype", "server type" },
			{ "isolation level", "isolation level" },
			{ "isolationlevel", "isolation level" },
			{ "records affected", "records affected" },
			{ "context connection", "context connection" },
			{ "enlist", "enlist" },
			{ "clientlibrary", "client library" },
			{ "client library", "client library" },
			{ "cache pages", "cache pages" },
			{ "cachepages", "cache pages" },
			{ "pagebuffers", "cache pages" },
			{ "page buffers", "cache pages" },
			{ "no db triggers", "no db triggers" },
			{ "nodbtriggers", "no db triggers" },
			{ "no dbtriggers", "no db triggers" },
			{ "no database triggers", "no db triggers" },
			{ "nodatabasetriggers", "no db triggers" },
			{ "no garbage collect", "no garbage collect"},
			{ "nogarbagecollect", "no garbage collect"}
		};

		#endregion

		#region Fields

		private Dictionary<string, object> options;
		private bool isServiceConnectionString;

		#endregion

		#region Properties

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

		public bool NoDatabaseTriggers
		{
			get { return this.GetBoolean("no db triggers"); }
		}

		public bool NoGarbageCollect
		{
			get { return this.GetBoolean("no garbage collect"); }
		}

		#endregion

		#region Internal Properties
		internal bool FallIntoTrustedAuth
		{
			// on non-Win the UserID/Password is checked in Validate method
			get { return string.IsNullOrEmpty(this.UserID) && string.IsNullOrEmpty(this.Password); }
		}

		internal string NormalizedConnectionString
		{
			get { return string.Join(";", this.options.Keys.OrderBy(x => x, StringComparer.InvariantCulture).Select(key => string.Format("{0}={1}", key, WrapValueIfNeeded(this.options[key].ToString())))); }
		}
		#endregion

		#region Constructors

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

		#region Methods

		public void Load(string connectionString)
		{
			const string KeyPairsRegex = "(([\\w\\s\\d]*)\\s*?=\\s*?\"([^\"]*)\"|([\\w\\s\\d]*)\\s*?=\\s*?'([^']*)'|([\\w\\s\\d]*)\\s*?=\\s*?([^\"';][^;]*))";

			this.SetDefaultOptions();

			if (connectionString != null && connectionString.Length > 0)
			{
				MatchCollection keyPairs = Regex.Matches(connectionString, KeyPairsRegex);

				foreach (Match keyPair in keyPairs)
				{
					if (keyPair.Groups.Count == 8)
					{
						string[] values = new string[]
						{
							(keyPair.Groups[2].Success ? keyPair.Groups[2].Value
								: keyPair.Groups[4].Success ? keyPair.Groups[4].Value
									: keyPair.Groups[6].Success ? keyPair.Groups[6].Value
										: string.Empty)
							.Trim().ToLowerInvariant(),
							(keyPair.Groups[3].Success ? keyPair.Groups[3].Value
								: keyPair.Groups[5].Success ? keyPair.Groups[5].Value
									: keyPair.Groups[7].Success ? keyPair.Groups[7].Value
										: string.Empty)
							.Trim()
						};

						if (values.Length == 2 && !string.IsNullOrEmpty(values[0]) && !string.IsNullOrEmpty(values[1]))
						{
							string key;
							if (Synonyms.TryGetValue(values[0], out key))
							{
								if (key == "server type")
								{
									FbServerType serverType = default(FbServerType);
									try
									{
										serverType = (FbServerType)Enum.Parse(typeof(FbServerType), values[1], true);
									}
									catch
									{
										throw new NotSupportedException("Not supported 'server type'.");
									}
									this.options[key] = serverType;
								}
								else
								{
									this.options[key] = values[1];
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
					(string.IsNullOrEmpty(this.UserID)) ||
					(string.IsNullOrEmpty(this.Password)) ||
#endif
(string.IsNullOrEmpty(this.Database) && !this.isServiceConnectionString) ||
					(string.IsNullOrEmpty(this.DataSource) && this.ServerType != FbServerType.Embedded) ||
					(string.IsNullOrEmpty(this.Charset)) ||
					(this.Port == 0) ||
					(!Enum.IsDefined(typeof(FbServerType), this.ServerType)) ||
					(this.MinPoolSize > this.MaxPoolSize)
				   )
				{
					throw new ArgumentException("An invalid connection string argument has been supplied or a required connection string argument has not been supplied.");
				}
				if (this.Dialect < 1 || this.Dialect > 3)
				{
					throw new ArgumentException("Incorrect database dialect it should be 1, 2, or 3.");
				}
				if (this.PacketSize < 512 || this.PacketSize > 32767)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "'Packet Size' value of {0} is not valid.{1}The value should be an integer >= 512 and <= 32767.", this.PacketSize, Environment.NewLine));
				}
				if (this.DbCachePages < 0)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "'Db Cache Pages' value of {0} is not valid.{1}The value should be an integer >= 0.", this.DbCachePages, Environment.NewLine));
				}
				if (this.Pooling && this.NoDatabaseTriggers)
				{
					throw new ArgumentException("Cannot use Pooling and NoDBTriggers together.");
				}

				this.CheckIsolationLevel();
			}
		}

		#endregion

		#region Private Methods

		private void SetDefaultOptions()
		{
			if (this.options == null)
			{
				this.options = new Dictionary<string, object>();
			}

			this.options.Clear();

			this.options.Add("data source", DefaultDataSource);
			this.options.Add("port number", DefaultPortNumber);
			this.options.Add("user id", DefaultUserId);
			this.options.Add("password", DefaultPassword);
			this.options.Add("role name", DefaultRoleName);
			this.options.Add("initial catalog", DefaultCatalog);
			this.options.Add("character set", DefaultCharacterSet);
			this.options.Add("dialect", DefaultDialect);
			this.options.Add("packet size", DefaultPacketSize);
			this.options.Add("pooling", DefaultPooling);
			this.options.Add("connection lifetime", DefaultConnectionLifetime);
			this.options.Add("min pool size", DefaultMinPoolSize);
			this.options.Add("max pool size", DefaultMaxPoolSize);
			this.options.Add("connection timeout", DefaultConnectionTimeout);
			this.options.Add("fetch size", DefaultFetchSize);
			this.options.Add("server type", DefaultServerType);
			this.options.Add("isolation level", DefaultIsolationLevel);
			this.options.Add("records affected", DefaultRecordsAffected);
			this.options.Add("context connection", DefaultContextConnection);
			this.options.Add("enlist", DefaultEnlist);
			this.options.Add("client library", DefaultClientLibrary);
			this.options.Add("cache pages", DefaultCachePages);
			this.options.Add("no db triggers", DefaultNoDbTriggers);
			this.options.Add("no garbage collect", DefaultNoGarbageCollect);
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
					if (string.IsNullOrEmpty(this.DataSource))
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

			string dataDirectoryLocation = (string)AppDomain.CurrentDomain.GetData("DataDirectory") ?? string.Empty;
			string pattern = string.Format("{0}{1}?", Regex.Escape(dataDirectoryKeyword), Regex.Escape(Path.DirectorySeparatorChar.ToString()));
			return Regex.Replace(s, pattern, dataDirectoryLocation + Path.DirectorySeparatorChar, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
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

		private string WrapValueIfNeeded(string value)
		{
			if (value != null && value.Contains(";"))
				return "'" + value + "'";
			return value;
		}

		#endregion
	}
}
