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
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Common;

internal sealed class ConnectionString
{
	#region Constants
	internal const string DefaultValueDataSource = "";
	internal const int DefaultValuePortNumber = 3050;
	internal const string DefaultValueUserId = "";
	internal const string DefaultValuePassword = "";
	internal const string DefaultValueRoleName = "";
	internal const string DefaultValueCatalog = "";
	internal const string DefaultValueCharacterSet = "UTF8";
	internal const int DefaultValueDialect = 3;
	internal const int DefaultValuePacketSize = 8192;
	internal const bool DefaultValuePooling = true;
	internal const int DefaultValueConnectionLifetime = 0;
	internal const int DefaultValueMinPoolSize = 0;
	internal const int DefaultValueMaxPoolSize = 100;
	internal const int DefaultValueConnectionTimeout = 15;
	internal const int DefaultValueFetchSize = 200;
	internal const FbServerType DefaultValueServerType = FbServerType.Default;
	internal const IsolationLevel DefaultValueIsolationLevel = IsolationLevel.ReadCommitted;
	internal const bool DefaultValueRecordsAffected = true;
	internal const bool DefaultValueEnlist = true;
	internal const string DefaultValueClientLibrary = "fbembed";
	internal const int DefaultValueDbCachePages = 0;
	internal const bool DefaultValueNoDbTriggers = false;
	internal const bool DefaultValueNoGarbageCollect = false;
	internal const bool DefaultValueCompression = false;
	internal const byte[] DefaultValueCryptKey = null;
	internal const FbWireCrypt DefaultValueWireCrypt = FbWireCrypt.Enabled;
	internal const string DefaultValueApplicationName = "";
	internal const int DefaultValueCommandTimeout = 0;
	internal const int DefaultValueParallelWorkers = 0;

	internal const string DefaultKeyUserId = "user id";
	internal const string DefaultKeyPortNumber = "port number";
	internal const string DefaultKeyDataSource = "data source";
	internal const string DefaultKeyPassword = "password";
	internal const string DefaultKeyRoleName = "role name";
	internal const string DefaultKeyCatalog = "initial catalog";
	internal const string DefaultKeyCharacterSet = "character set";
	internal const string DefaultKeyDialect = "dialect";
	internal const string DefaultKeyPacketSize = "packet size";
	internal const string DefaultKeyPooling = "pooling";
	internal const string DefaultKeyConnectionLifetime = "connection lifetime";
	internal const string DefaultKeyMinPoolSize = "min pool size";
	internal const string DefaultKeyMaxPoolSize = "max pool size";
	internal const string DefaultKeyConnectionTimeout = "connection timeout";
	internal const string DefaultKeyFetchSize = "fetch size";
	internal const string DefaultKeyServerType = "server type";
	internal const string DefaultKeyIsolationLevel = "isolation level";
	internal const string DefaultKeyRecordsAffected = "records affected";
	internal const string DefaultKeyEnlist = "enlist";
	internal const string DefaultKeyClientLibrary = "client library";
	internal const string DefaultKeyDbCachePages = "cache pages";
	internal const string DefaultKeyNoDbTriggers = "no db triggers";
	internal const string DefaultKeyNoGarbageCollect = "no garbage collect";
	internal const string DefaultKeyCompression = "compression";
	internal const string DefaultKeyCryptKey = "crypt key";
	internal const string DefaultKeyWireCrypt = "wire crypt";
	internal const string DefaultKeyApplicationName = "application name";
	internal const string DefaultKeyCommandTimeout = "command timeout";
	internal const string DefaultKeyParallelWorkers = "parallel workers";
	#endregion

	#region Static Fields

	internal static readonly IDictionary<string, string> Synonyms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			{ DefaultKeyDataSource, DefaultKeyDataSource },
			{ "datasource", DefaultKeyDataSource },
			{ "server", DefaultKeyDataSource },
			{ "host", DefaultKeyDataSource },
			{ "port", DefaultKeyPortNumber },
			{ DefaultKeyPortNumber, DefaultKeyPortNumber },
			{ "database", DefaultKeyCatalog },
			{ DefaultKeyCatalog, DefaultKeyCatalog },
			{ DefaultKeyUserId, DefaultKeyUserId },
			{ "userid", DefaultKeyUserId },
			{ "uid", DefaultKeyUserId },
			{ "user", DefaultKeyUserId },
			{ "user name", DefaultKeyUserId },
			{ "username", DefaultKeyUserId },
			{ DefaultKeyPassword, DefaultKeyPassword },
			{ "user password", DefaultKeyPassword },
			{ "userpassword", DefaultKeyPassword },
			{ DefaultKeyDialect, DefaultKeyDialect },
			{ DefaultKeyPooling, DefaultKeyPooling },
			{ DefaultKeyMaxPoolSize, DefaultKeyMaxPoolSize },
			{ "maxpoolsize", DefaultKeyMaxPoolSize },
			{ DefaultKeyMinPoolSize, DefaultKeyMinPoolSize },
			{ "minpoolsize", DefaultKeyMinPoolSize },
			{ DefaultKeyCharacterSet, DefaultKeyCharacterSet },
			{ "charset", DefaultKeyCharacterSet },
			{ DefaultKeyConnectionLifetime, DefaultKeyConnectionLifetime },
			{ "connectionlifetime", DefaultKeyConnectionLifetime },
			{ "timeout", DefaultKeyConnectionTimeout },
			{ DefaultKeyConnectionTimeout, DefaultKeyConnectionTimeout },
			{ "connectiontimeout", DefaultKeyConnectionTimeout },
			{ DefaultKeyPacketSize, DefaultKeyPacketSize },
			{ "packetsize", DefaultKeyPacketSize },
			{ "role", DefaultKeyRoleName },
			{ DefaultKeyRoleName, DefaultKeyRoleName },
			{ DefaultKeyFetchSize, DefaultKeyFetchSize },
			{ "fetchsize", DefaultKeyFetchSize },
			{ DefaultKeyServerType, DefaultKeyServerType },
			{ "servertype", DefaultKeyServerType },
			{ DefaultKeyIsolationLevel, DefaultKeyIsolationLevel },
			{ "isolationlevel", DefaultKeyIsolationLevel },
			{ DefaultKeyRecordsAffected, DefaultKeyRecordsAffected },
			{ DefaultKeyEnlist, DefaultKeyEnlist },
			{ "clientlibrary", DefaultKeyClientLibrary },
			{ DefaultKeyClientLibrary, DefaultKeyClientLibrary },
			{ DefaultKeyDbCachePages, DefaultKeyDbCachePages },
			{ "cachepages", DefaultKeyDbCachePages },
			{ "pagebuffers", DefaultKeyDbCachePages },
			{ "page buffers", DefaultKeyDbCachePages },
			{ DefaultKeyNoDbTriggers, DefaultKeyNoDbTriggers },
			{ "nodbtriggers", DefaultKeyNoDbTriggers },
			{ "no dbtriggers", DefaultKeyNoDbTriggers },
			{ "no database triggers", DefaultKeyNoDbTriggers },
			{ "nodatabasetriggers", DefaultKeyNoDbTriggers },
			{ DefaultKeyNoGarbageCollect, DefaultKeyNoGarbageCollect },
			{ "nogarbagecollect", DefaultKeyNoGarbageCollect },
			{ DefaultKeyCompression, DefaultKeyCompression },
			{ "wire compression", DefaultKeyCompression },
			{ DefaultKeyCryptKey, DefaultKeyCryptKey },
			{ "cryptkey", DefaultKeyCryptKey },
			{ DefaultKeyWireCrypt, DefaultKeyWireCrypt },
			{ "wirecrypt", DefaultKeyWireCrypt },
			{ DefaultKeyApplicationName, DefaultKeyApplicationName },
			{ "applicationname", DefaultKeyApplicationName },
			{ "app", DefaultKeyApplicationName },
			{ DefaultKeyCommandTimeout, DefaultKeyCommandTimeout },
			{ "commandtimeout", DefaultKeyCommandTimeout },
			{ DefaultKeyParallelWorkers, DefaultKeyParallelWorkers },
			{ "parallelworkers", DefaultKeyParallelWorkers },
			{ "parallel", DefaultKeyParallelWorkers },
		};

	internal static readonly IDictionary<string, object> DefaultValues = new Dictionary<string, object>(StringComparer.Ordinal)
		{
			{ DefaultKeyDataSource, DefaultValueDataSource },
			{ DefaultKeyPortNumber, DefaultValuePortNumber },
			{ DefaultKeyUserId, DefaultValueUserId },
			{ DefaultKeyPassword, DefaultValuePassword },
			{ DefaultKeyRoleName, DefaultValueRoleName },
			{ DefaultKeyCatalog, DefaultValueCatalog },
			{ DefaultKeyCharacterSet, DefaultValueCharacterSet },
			{ DefaultKeyDialect, DefaultValueDialect },
			{ DefaultKeyPacketSize, DefaultValuePacketSize },
			{ DefaultKeyPooling, DefaultValuePooling },
			{ DefaultKeyConnectionLifetime, DefaultValueConnectionLifetime },
			{ DefaultKeyMinPoolSize, DefaultValueMinPoolSize },
			{ DefaultKeyMaxPoolSize, DefaultValueMaxPoolSize },
			{ DefaultKeyConnectionTimeout, DefaultValueConnectionTimeout },
			{ DefaultKeyFetchSize, DefaultValueFetchSize },
			{ DefaultKeyServerType, DefaultValueServerType },
			{ DefaultKeyIsolationLevel, DefaultValueIsolationLevel },
			{ DefaultKeyRecordsAffected, DefaultValueRecordsAffected },
			{ DefaultKeyEnlist, DefaultValueEnlist },
			{ DefaultKeyClientLibrary, DefaultValueClientLibrary },
			{ DefaultKeyDbCachePages, DefaultValueDbCachePages },
			{ DefaultKeyNoDbTriggers, DefaultValueNoDbTriggers },
			{ DefaultKeyNoGarbageCollect, DefaultValueNoGarbageCollect },
			{ DefaultKeyCompression, DefaultValueCompression },
			{ DefaultKeyCryptKey, DefaultValueCryptKey },
			{ DefaultKeyWireCrypt, DefaultValueWireCrypt },
			{ DefaultKeyApplicationName, DefaultValueApplicationName },
			{ DefaultKeyCommandTimeout, DefaultValueCommandTimeout },
			{ DefaultKeyParallelWorkers, DefaultValueParallelWorkers },
		};

	#endregion

	#region Fields

	private Dictionary<string, object> _options;

	#endregion

	#region Properties

	public string UserID => GetString(DefaultKeyUserId, _options.TryGetValue);
	public string Password => GetString(DefaultKeyPassword, _options.TryGetValue);
	public string DataSource => GetString(DefaultKeyDataSource, _options.TryGetValue);
	public int Port => GetInt32(DefaultKeyPortNumber, _options.TryGetValue);
	public string Database => ExpandDataDirectory(GetString(DefaultKeyCatalog, _options.TryGetValue));
	public int PacketSize => GetInt32(DefaultKeyPacketSize, _options.TryGetValue);
	public string Role => GetString(DefaultKeyRoleName, _options.TryGetValue);
	public short Dialect => GetInt16(DefaultKeyDialect, _options.TryGetValue);
	public string Charset => GetString(DefaultKeyCharacterSet, _options.TryGetValue);
	public int ConnectionTimeout => GetInt32(DefaultKeyConnectionTimeout, _options.TryGetValue);
	public bool Pooling => GetBoolean(DefaultKeyPooling, _options.TryGetValue);
	public long ConnectionLifetime => GetInt64(DefaultKeyConnectionLifetime, _options.TryGetValue);
	public int MinPoolSize => GetInt32(DefaultKeyMinPoolSize, _options.TryGetValue);
	public int MaxPoolSize => GetInt32(DefaultKeyMaxPoolSize, _options.TryGetValue);
	public int FetchSize => GetInt32(DefaultKeyFetchSize, _options.TryGetValue);
	public FbServerType ServerType => GetServerType(DefaultKeyServerType, _options.TryGetValue);
	public IsolationLevel IsolationLevel => GetIsolationLevel(DefaultKeyIsolationLevel, _options.TryGetValue);
	public bool ReturnRecordsAffected => GetBoolean(DefaultKeyRecordsAffected, _options.TryGetValue);
	public bool Enlist => GetBoolean(DefaultKeyEnlist, _options.TryGetValue);
	public string ClientLibrary => GetString(DefaultKeyClientLibrary, _options.TryGetValue);
	public int DbCachePages => GetInt32(DefaultKeyDbCachePages, _options.TryGetValue);
	public bool NoDatabaseTriggers => GetBoolean(DefaultKeyNoDbTriggers, _options.TryGetValue);
	public bool NoGarbageCollect => GetBoolean(DefaultKeyNoGarbageCollect, _options.TryGetValue);
	public bool Compression => GetBoolean(DefaultKeyCompression, _options.TryGetValue);
	public byte[] CryptKey => GetBytes(DefaultKeyCryptKey, _options.TryGetValue);
	public FbWireCrypt WireCrypt => GetWireCrypt(DefaultKeyWireCrypt, _options.TryGetValue);
	public string ApplicationName => GetString(DefaultKeyApplicationName, _options.TryGetValue);
	public int CommandTimeout => GetInt32(DefaultKeyCommandTimeout, _options.TryGetValue);
	public int ParallelWorkers => GetInt32(DefaultKeyParallelWorkers, _options.TryGetValue);

	#endregion

	#region Internal Properties
	internal string NormalizedConnectionString
	{
		get { return string.Join(";", _options.OrderBy(x => x.Key, StringComparer.Ordinal).Where(x => x.Value != null).Select(x => string.Format("{0}={1}", x.Key, WrapValueIfNeeded(x.Value.ToString())))); }
	}
	#endregion

	#region Constructors

	public ConnectionString()
	{
		SetDefaultOptions();
	}

	public ConnectionString(string connectionString)
		: this()
	{
		Load(connectionString);
	}

	#endregion

	#region Methods

	public void Validate()
	{
		if (
			(string.IsNullOrEmpty(Database)) ||
			(string.IsNullOrEmpty(DataSource) && ServerType != FbServerType.Embedded) ||
			(string.IsNullOrEmpty(Charset))
		   )
		{
			throw new ArgumentException("An invalid connection string argument has been supplied or a required connection string argument has not been supplied.");
		}
		if (Port <= 0 || Port > 65535)
		{
			throw new ArgumentException("Incorrect port.");
		}
		if (MinPoolSize > MaxPoolSize)
		{
			throw new ArgumentException("Incorrect pool size.");
		}
		if (Dialect < 1 || Dialect > 3)
		{
			throw new ArgumentException("Incorrect database dialect it should be 1, 2, or 3.");
		}
		if (PacketSize < 512 || PacketSize > 32767)
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "'Packet Size' value of {0} is not valid.{1}The value should be an integer >= 512 and <= 32767.", PacketSize, Environment.NewLine));
		}
		if (DbCachePages < 0)
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "'Cache Pages' value of {0} is not valid.{1}The value should be an integer >= 0.", DbCachePages, Environment.NewLine));
		}
		if (Pooling && NoDatabaseTriggers)
		{
			throw new ArgumentException("Cannot use Pooling and NoDatabaseTriggers together.");
		}
		if (ParallelWorkers < 0)
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "'Parallel Workers' value of {0} is not valid.{1}The value should be an integer >= 0.", ParallelWorkers, Environment.NewLine));
		}
	}

	#endregion

	#region Private Methods

	private void Load(string connectionString)
	{
		const string KeyPairsRegex = "(([\\w\\s\\d]*)\\s*?=\\s*?\"([^\"]*)\"|([\\w\\s\\d]*)\\s*?=\\s*?'([^']*)'|([\\w\\s\\d]*)\\s*?=\\s*?([^\"';][^;]*))";

		if (!string.IsNullOrEmpty(connectionString))
		{
			var keyPairs = Regex.Matches(connectionString, KeyPairsRegex);

			foreach (Match keyPair in keyPairs)
			{
				if (keyPair.Groups.Count == 8)
				{
					var values = new string[]
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
						if (Synonyms.TryGetValue(values[0], out var key))
						{
							switch (key)
							{
								case DefaultKeyServerType:
									_options[key] = ParseEnum<FbServerType>(values[1], DefaultKeyServerType);
									break;
								case DefaultKeyIsolationLevel:
									_options[key] = ParseEnum<IsolationLevel>(values[1], DefaultKeyIsolationLevel);
									break;
								case DefaultKeyCryptKey:
									var cryptKey = default(byte[]);
									try
									{
										cryptKey = Convert.FromBase64String(values[1]);
									}
									catch
									{
										throw NotSupported(DefaultKeyCryptKey);
									}
									_options[key] = cryptKey;
									break;
								case DefaultKeyWireCrypt:
									_options[key] = ParseEnum<FbWireCrypt>(values[1], DefaultKeyWireCrypt);
									break;
								default:
									_options[key] = values[1];
									break;
							}
						}
					}
				}
			}

			if (!string.IsNullOrEmpty(Database))
			{
				ParseConnectionInfo(Database);
			}
		}
	}

	private void SetDefaultOptions()
	{
		_options = new Dictionary<string, object>(DefaultValues);
	}

	// it is expected the hostname do be at least 2 characters to prevent possible ambiguity (DNET-892)
	private void ParseConnectionInfo(string connectionInfo)
	{
		connectionInfo = connectionInfo.Trim();

		{
			// URL style inet://[hostv6]:port/database
			var match = Regex.Match(connectionInfo, "^inet://\\[(?<host>[A-Za-z0-9:]{2,})\\]:(?<port>\\d+)/(?<database>.+)$");
			if (match.Success)
			{
				_options[DefaultKeyCatalog] = match.Groups["database"].Value;
				_options[DefaultKeyDataSource] = match.Groups["host"].Value;
				_options[DefaultKeyPortNumber] = int.Parse(match.Groups["port"].Value, CultureInfo.InvariantCulture);
				return;
			}
		}
		{
			// URL style inet://host:port/database
			var match = Regex.Match(connectionInfo, "^inet://(?<host>[A-Za-z0-9\\.-]{2,}):(?<port>\\d+)/(?<database>.+)$");
			if (match.Success)
			{
				_options[DefaultKeyCatalog] = match.Groups["database"].Value;
				_options[DefaultKeyDataSource] = match.Groups["host"].Value;
				_options[DefaultKeyPortNumber] = int.Parse(match.Groups["port"].Value, CultureInfo.InvariantCulture);
				return;
			}
		}
		{
			// URL style inet://host/database
			var match = Regex.Match(connectionInfo, "^inet://(?<host>[A-Za-z0-9\\.:-]{2,})/(?<database>.+)$");
			if (match.Success)
			{
				_options[DefaultKeyCatalog] = match.Groups["database"].Value;
				_options[DefaultKeyDataSource] = match.Groups["host"].Value;
				return;
			}
		}
		{
			// URL style inet:///database
			var match = Regex.Match(connectionInfo, "^inet:///(?<database>.+)$");
			if (match.Success)
			{
				_options[DefaultKeyCatalog] = match.Groups["database"].Value;
				_options[DefaultKeyDataSource] = "localhost";
				return;
			}
		}
		{
			// new style //[hostv6]:port/database
			var match = Regex.Match(connectionInfo, "^//\\[(?<host>[A-Za-z0-9:]{2,})\\]:(?<port>\\d+)/(?<database>.+)$");
			if (match.Success)
			{
				_options[DefaultKeyCatalog] = match.Groups["database"].Value;
				_options[DefaultKeyDataSource] = match.Groups["host"].Value;
				_options[DefaultKeyPortNumber] = int.Parse(match.Groups["port"].Value, CultureInfo.InvariantCulture);
				return;
			}
		}
		{
			// new style //host:port/database
			var match = Regex.Match(connectionInfo, "^//(?<host>[A-Za-z0-9\\.-]{2,}):(?<port>\\d+)/(?<database>.+)$");
			if (match.Success)
			{
				_options[DefaultKeyCatalog] = match.Groups["database"].Value;
				_options[DefaultKeyDataSource] = match.Groups["host"].Value;
				_options[DefaultKeyPortNumber] = int.Parse(match.Groups["port"].Value, CultureInfo.InvariantCulture);
				return;
			}
		}
		{
			// new style //host/database
			var match = Regex.Match(connectionInfo, "^//(?<host>[A-Za-z0-9\\.:-]{2,})/(?<database>.+)$");
			if (match.Success)
			{
				_options[DefaultKeyCatalog] = match.Groups["database"].Value;
				_options[DefaultKeyDataSource] = match.Groups["host"].Value;
				return;
			}
		}
		{
			// old style host:X:\database
			var match = Regex.Match(connectionInfo, "^(?<host>[A-Za-z0-9\\.:-]{2,}):(?<database>[A-Za-z]:\\\\.+)$");
			if (match.Success)
			{
				_options[DefaultKeyCatalog] = match.Groups["database"].Value;
				_options[DefaultKeyDataSource] = match.Groups["host"].Value;
				return;
			}
		}
		{
			// old style host/port:database
			var match = Regex.Match(connectionInfo, "^(?<host>[A-Za-z0-9\\.:-]{2,})/(?<port>\\d+):(?<database>.+)$");
			if (match.Success)
			{
				_options[DefaultKeyCatalog] = match.Groups["database"].Value;
				_options[DefaultKeyDataSource] = match.Groups["host"].Value;
				_options[DefaultKeyPortNumber] = int.Parse(match.Groups["port"].Value, CultureInfo.InvariantCulture);
				return;
			}
		}
		{
			// old style host:database
			var match = Regex.Match(connectionInfo, "^(?<host>[A-Za-z0-9\\.:-]{2,}):(?<database>.+)$");
			if (match.Success)
			{
				_options[DefaultKeyCatalog] = match.Groups["database"].Value;
				_options[DefaultKeyDataSource] = match.Groups["host"].Value;
				return;
			}
		}

		_options[DefaultKeyCatalog] = connectionInfo;
	}

	#endregion

	#region Internal Static Methods

	internal delegate bool TryGetValueDelegate(string key, out object value);

	internal static short GetInt16(string key, TryGetValueDelegate tryGetValue, short defaultValue = default)
	{
		return tryGetValue(key, out var value)
			? Convert.ToInt16(value, CultureInfo.InvariantCulture)
			: defaultValue;
	}

	internal static int GetInt32(string key, TryGetValueDelegate tryGetValue, int defaultValue = default)
	{
		return tryGetValue(key, out var value)
			? Convert.ToInt32(value, CultureInfo.InvariantCulture)
			: defaultValue;
	}

	internal static long GetInt64(string key, TryGetValueDelegate tryGetValue, long defaultValue = default)
	{
		return tryGetValue(key, out var value)
			? Convert.ToInt64(value, CultureInfo.InvariantCulture)
			: defaultValue;
	}

	internal static string GetString(string key, TryGetValueDelegate tryGetValue, string defaultValue = default)
	{
		return tryGetValue(key, out var value)
			? Convert.ToString(value, CultureInfo.InvariantCulture)
			: defaultValue;
	}

	internal static bool GetBoolean(string key, TryGetValueDelegate tryGetValue, bool defaultValue = default)
	{
		return tryGetValue(key, out var value)
			? Convert.ToBoolean(value, CultureInfo.InvariantCulture)
			: defaultValue;
	}

	internal static byte[] GetBytes(string key, TryGetValueDelegate tryGetValue, byte[] defaultValue = default)
	{
		return tryGetValue(key, out var value)
			? (byte[])value
			: defaultValue;
	}

	internal static FbServerType GetServerType(string key, TryGetValueDelegate tryGetValue, FbServerType defaultValue = default)
	{
		return tryGetValue(key, out var value)
			? (FbServerType)value
			: defaultValue;
	}

	internal static IsolationLevel GetIsolationLevel(string key, TryGetValueDelegate tryGetValue, IsolationLevel defaultValue = default)
	{
		return tryGetValue(key, out var value)
			? (IsolationLevel)value
			: defaultValue;
	}

	internal static FbWireCrypt GetWireCrypt(string key, TryGetValueDelegate tryGetValue, FbWireCrypt defaultValue = default)
	{
		return tryGetValue(key, out var value)
			? (FbWireCrypt)value
			: defaultValue;
	}

	#endregion

	#region Private Static Methods

	private static string ExpandDataDirectory(string s)
	{
		const string DataDirectoryKeyword = "|DataDirectory|";
		if (s == null)
			return s;

		var dataDirectoryLocation = (string)AppDomain.CurrentDomain.GetData("DataDirectory") ?? string.Empty;
		var pattern = string.Format("{0}{1}?", Regex.Escape(DataDirectoryKeyword), Regex.Escape(Path.DirectorySeparatorChar.ToString()));
		return Regex.Replace(s, pattern, dataDirectoryLocation + Path.DirectorySeparatorChar, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
	}

	private static T ParseEnum<T>(string value, string name) where T : struct
	{
		if (!Enum.TryParse<T>(value, true, out var result))
			throw NotSupported(name);
		return result;
	}

	private static Exception NotSupported(string name) => new NotSupportedException($"Not supported '{name}'.");

	private static string WrapValueIfNeeded(string value)
	{
		if (value != null && value.Contains(';'))
			return "'" + value + "'";
		return value;
	}

	#endregion
}
