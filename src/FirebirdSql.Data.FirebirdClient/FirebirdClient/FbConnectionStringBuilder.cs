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
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text;
using System.ComponentModel;

namespace FirebirdSql.Data.FirebirdClient;

public class FbConnectionStringBuilder : DbConnectionStringBuilder
{
	#region Properties

	[Category("Security")]
	[DisplayName("User ID")]
	[Description("Indicates the User ID to be used when connecting to the data source.")]
	[DefaultValue(Common.ConnectionString.DefaultValueUserId)]
	public string UserID
	{
		get { return Common.ConnectionString.GetString(GetKey(Common.ConnectionString.DefaultKeyUserId), base.TryGetValue, Common.ConnectionString.DefaultValueUserId); }
		set { SetValue(Common.ConnectionString.DefaultKeyUserId, value); }
	}

	[Category("Security")]
	[DisplayName("Password")]
	[Description("Indicates the password to be used when connecting to the data source.")]
	[PasswordPropertyText(true)]
	[DefaultValue(Common.ConnectionString.DefaultValuePassword)]
	public string Password
	{
		get { return Common.ConnectionString.GetString(GetKey(Common.ConnectionString.DefaultKeyPassword), base.TryGetValue, Common.ConnectionString.DefaultValuePassword); }
		set { SetValue(Common.ConnectionString.DefaultKeyPassword, value); }
	}

	[Category("Source")]
	[DisplayName("DataSource")]
	[Description("The name of the Firebird server to which to connect.")]
	[DefaultValue(Common.ConnectionString.DefaultValueDataSource)]
	public string DataSource
	{
		get { return Common.ConnectionString.GetString(GetKey(Common.ConnectionString.DefaultKeyDataSource), base.TryGetValue, Common.ConnectionString.DefaultValueDataSource); }
		set { SetValue(Common.ConnectionString.DefaultKeyDataSource, value); }
	}

	[Category("Source")]
	[DisplayName("Database")]
	[Description("The name of the actual database or the database to be used when a connection is open. It is normally the path to an .FDB file or an alias.")]
	[DefaultValue(Common.ConnectionString.DefaultValueCatalog)]
	public string Database
	{
		get { return Common.ConnectionString.GetString(GetKey(Common.ConnectionString.DefaultKeyCatalog), base.TryGetValue, Common.ConnectionString.DefaultValueCatalog); }
		set { SetValue(Common.ConnectionString.DefaultKeyCatalog, value); }
	}

	[Category("Source")]
	[DisplayName("Port")]
	[Description("Port to use for TCP/IP connections")]
	[DefaultValue(Common.ConnectionString.DefaultValuePortNumber)]
	public int Port
	{
		get { return Common.ConnectionString.GetInt32(GetKey(Common.ConnectionString.DefaultKeyPortNumber), base.TryGetValue, Common.ConnectionString.DefaultValuePortNumber); }
		set { SetValue(Common.ConnectionString.DefaultKeyPortNumber, value); }
	}

	[Category("Advanced")]
	[DisplayName("PacketSize")]
	[Description("The size (in bytes) of network packets. PacketSize may be in the range 512-32767 bytes.")]
	[DefaultValue(Common.ConnectionString.DefaultValuePacketSize)]
	public int PacketSize
	{
		get { return Common.ConnectionString.GetInt32(GetKey(Common.ConnectionString.DefaultKeyPacketSize), base.TryGetValue, Common.ConnectionString.DefaultValuePacketSize); }
		set { SetValue(Common.ConnectionString.DefaultKeyPacketSize, value); }
	}

	[Category("Security")]
	[DisplayName("Role")]
	[Description("The user role.")]
	[DefaultValue(Common.ConnectionString.DefaultValueRoleName)]
	public string Role
	{
		get { return Common.ConnectionString.GetString(GetKey(Common.ConnectionString.DefaultKeyRoleName), base.TryGetValue, Common.ConnectionString.DefaultValueRoleName); }
		set { SetValue(Common.ConnectionString.DefaultKeyRoleName, value); }
	}

	[Category("Advanced")]
	[DisplayName("Dialect")]
	[Description("The database SQL dialect.")]
	[DefaultValue(Common.ConnectionString.DefaultValueDialect)]
	public int Dialect
	{
		get { return Common.ConnectionString.GetInt32(GetKey(Common.ConnectionString.DefaultKeyDialect), base.TryGetValue, Common.ConnectionString.DefaultValueDialect); }
		set { SetValue(Common.ConnectionString.DefaultKeyDialect, value); }
	}

	[Category("Advanced")]
	[DisplayName("Character Set")]
	[Description("The connection character set encoding.")]
	[DefaultValue(Common.ConnectionString.DefaultValueCharacterSet)]
	public string Charset
	{
		get { return Common.ConnectionString.GetString(GetKey(Common.ConnectionString.DefaultKeyCharacterSet), base.TryGetValue, Common.ConnectionString.DefaultValueCharacterSet); }
		set { SetValue(Common.ConnectionString.DefaultKeyCharacterSet, value); }
	}

	[Category("Connection")]
	[DisplayName("Connection Timeout")]
	[Description("The time (in seconds) to wait for a connection to open.")]
	[DefaultValue(Common.ConnectionString.DefaultValueConnectionTimeout)]
	public int ConnectionTimeout
	{
		get { return Common.ConnectionString.GetInt32(GetKey(Common.ConnectionString.DefaultKeyConnectionTimeout), base.TryGetValue, Common.ConnectionString.DefaultValueConnectionTimeout); }
		set { SetValue(Common.ConnectionString.DefaultKeyConnectionTimeout, value); }
	}

	[Category("Pooling")]
	[DisplayName("Pooling")]
	[Description("When true the connection is grabbed from a pool or, if necessary, created and added to the appropriate pool.")]
	[DefaultValue(Common.ConnectionString.DefaultValuePooling)]
	public bool Pooling
	{
		get { return Common.ConnectionString.GetBoolean(GetKey(Common.ConnectionString.DefaultKeyPooling), base.TryGetValue, Common.ConnectionString.DefaultValuePooling); }
		set { SetValue(Common.ConnectionString.DefaultKeyPooling, value); }
	}

	[Category("Connection")]
	[DisplayName("Connection LifeTime")]
	[Description("When a connection is returned to the pool, its creation time is compared with the current time, and the connection is destroyed if that time span (in seconds) exceeds the value specified by connection lifetime.")]
	[DefaultValue(Common.ConnectionString.DefaultValueConnectionLifetime)]
	public int ConnectionLifeTime
	{
		get { return Common.ConnectionString.GetInt32(GetKey(Common.ConnectionString.DefaultKeyConnectionLifetime), base.TryGetValue, Common.ConnectionString.DefaultValueConnectionLifetime); }
		set { SetValue(Common.ConnectionString.DefaultKeyConnectionLifetime, value); }
	}

	[Category("Pooling")]
	[DisplayName("MinPoolSize")]
	[Description("The minimun number of connections allowed in the pool.")]
	[DefaultValue(Common.ConnectionString.DefaultValueMinPoolSize)]
	public int MinPoolSize
	{
		get { return Common.ConnectionString.GetInt32(GetKey(Common.ConnectionString.DefaultKeyMinPoolSize), base.TryGetValue, Common.ConnectionString.DefaultValueMinPoolSize); }
		set { SetValue(Common.ConnectionString.DefaultKeyMinPoolSize, value); }
	}

	[Category("Pooling")]
	[DisplayName("MaxPoolSize")]
	[Description("The maximum number of connections allowed in the pool.")]
	[DefaultValue(Common.ConnectionString.DefaultValueMaxPoolSize)]
	public int MaxPoolSize
	{
		get { return Common.ConnectionString.GetInt32(GetKey(Common.ConnectionString.DefaultKeyMaxPoolSize), base.TryGetValue, Common.ConnectionString.DefaultValueMaxPoolSize); }
		set { SetValue(Common.ConnectionString.DefaultKeyMaxPoolSize, value); }
	}

	[Category("Advanced")]
	[DisplayName("FetchSize")]
	[Description("The maximum number of rows to be fetched in a single call to read into the internal row buffer.")]
	[DefaultValue(Common.ConnectionString.DefaultValueFetchSize)]
	public int FetchSize
	{
		get { return Common.ConnectionString.GetInt32(GetKey(Common.ConnectionString.DefaultKeyFetchSize), base.TryGetValue, Common.ConnectionString.DefaultValueFetchSize); }
		set { SetValue(Common.ConnectionString.DefaultKeyFetchSize, value); }
	}

	[Category("Source")]
	[DisplayName("ServerType")]
	[Description("The type of server used.")]
	[DefaultValue(Common.ConnectionString.DefaultValueServerType)]
	public FbServerType ServerType
	{
		get { return GetServerType(Common.ConnectionString.DefaultKeyServerType, Common.ConnectionString.DefaultValueServerType); }
		set { SetValue(Common.ConnectionString.DefaultKeyServerType, value); }
	}

	[Category("Advanced")]
	[DisplayName("IsolationLevel")]
	[Description("The default Isolation Level for implicit transactions.")]
	[DefaultValue(Common.ConnectionString.DefaultValueIsolationLevel)]
	public IsolationLevel IsolationLevel
	{
		get { return GetIsolationLevel(Common.ConnectionString.DefaultKeyIsolationLevel, Common.ConnectionString.DefaultValueIsolationLevel); }
		set { SetValue(Common.ConnectionString.DefaultKeyIsolationLevel, value); }
	}

	[Category("Advanced")]
	[DisplayName("Records Affected")]
	[Description("Get the number of rows affected by a command when true.")]
	[DefaultValue(Common.ConnectionString.DefaultValueRecordsAffected)]
	public bool ReturnRecordsAffected
	{
		get { return Common.ConnectionString.GetBoolean(GetKey(Common.ConnectionString.DefaultKeyRecordsAffected), base.TryGetValue, Common.ConnectionString.DefaultValueRecordsAffected); }
		set { SetValue(Common.ConnectionString.DefaultKeyRecordsAffected, value); }
	}

	[Category("Pooling")]
	[DisplayName("Enlist")]
	[Description("If true, enlists the connections in the current transaction.")]
	[DefaultValue(Common.ConnectionString.DefaultValuePooling)]
	public bool Enlist
	{
		get { return Common.ConnectionString.GetBoolean(GetKey(Common.ConnectionString.DefaultKeyEnlist), base.TryGetValue, Common.ConnectionString.DefaultValueEnlist); }
		set { SetValue(Common.ConnectionString.DefaultKeyEnlist, value); }
	}

	[Category("Advanced")]
	[DisplayName("Client Library")]
	[Description("Client library for Firebird Embedded.")]
	[DefaultValue(Common.ConnectionString.DefaultValueClientLibrary)]
	public string ClientLibrary
	{
		get { return Common.ConnectionString.GetString(GetKey(Common.ConnectionString.DefaultKeyClientLibrary), base.TryGetValue, Common.ConnectionString.DefaultValueClientLibrary); }
		set { SetValue(Common.ConnectionString.DefaultKeyClientLibrary, value); }
	}

	[Category("Advanced")]
	[DisplayName("DB Cache Pages")]
	[Description("How many cache buffers to use for this session.")]
	[DefaultValue(Common.ConnectionString.DefaultValueDbCachePages)]
	public int DbCachePages
	{
		get { return Common.ConnectionString.GetInt32(GetKey(Common.ConnectionString.DefaultKeyDbCachePages), base.TryGetValue, Common.ConnectionString.DefaultValueDbCachePages); }
		set { SetValue(Common.ConnectionString.DefaultKeyDbCachePages, value); }
	}

	[Category("Advanced")]
	[DisplayName("No Triggers")]
	[Description("Disables database triggers for this connection.")]
	[DefaultValue(Common.ConnectionString.DefaultValueNoDbTriggers)]
	public bool NoDatabaseTriggers
	{
		get { return Common.ConnectionString.GetBoolean(GetKey(Common.ConnectionString.DefaultKeyNoDbTriggers), base.TryGetValue, Common.ConnectionString.DefaultValueNoDbTriggers); }
		set { SetValue(Common.ConnectionString.DefaultKeyNoDbTriggers, value); }
	}

	[Category("Advanced")]
	[DisplayName("No Garbage Collect")]
	[Description("If true, disables sweeping the database upon attachment.")]
	[DefaultValue(Common.ConnectionString.DefaultValueNoGarbageCollect)]
	public bool NoGarbageCollect
	{
		get { return Common.ConnectionString.GetBoolean(GetKey(Common.ConnectionString.DefaultKeyNoGarbageCollect), base.TryGetValue, Common.ConnectionString.DefaultValueNoGarbageCollect); }
		set { SetValue(Common.ConnectionString.DefaultKeyNoGarbageCollect, value); }
	}

	[Category("Advanced")]
	[DisplayName("Compression")]
	[Description("Enables or disables wire compression.")]
	[DefaultValue(Common.ConnectionString.DefaultValueCompression)]
	public bool Compression
	{
		get { return Common.ConnectionString.GetBoolean(GetKey(Common.ConnectionString.DefaultKeyCompression), base.TryGetValue, Common.ConnectionString.DefaultValueCompression); }
		set { SetValue(Common.ConnectionString.DefaultKeyCompression, value); }
	}

	[Category("Advanced")]
	[DisplayName("Crypt Key")]
	[Description("Key used for database decryption.")]
	[DefaultValue(Common.ConnectionString.DefaultValueCryptKey)]
	public byte[] CryptKey
	{
		get { return GetBytes(Common.ConnectionString.DefaultKeyCryptKey, Common.ConnectionString.DefaultValueCryptKey); }
		set { SetValue(Common.ConnectionString.DefaultKeyCryptKey, value); }
	}

	[Category("Advanced")]
	[DisplayName("Wire Crypt")]
	[Description("Selection for wire encryption.")]
	[DefaultValue(Common.ConnectionString.DefaultValueWireCrypt)]
	public FbWireCrypt WireCrypt
	{
		get { return GetWireCrypt(Common.ConnectionString.DefaultKeyWireCrypt, Common.ConnectionString.DefaultValueWireCrypt); }
		set { SetValue(Common.ConnectionString.DefaultKeyWireCrypt, value); }
	}

	[Category("Advanced")]
	[DisplayName("Application Name")]
	[Description("The name of the application making the connection.")]
	[DefaultValue(Common.ConnectionString.DefaultValueApplicationName)]
	public string ApplicationName
	{
		get { return Common.ConnectionString.GetString(GetKey(Common.ConnectionString.DefaultKeyApplicationName), base.TryGetValue, Common.ConnectionString.DefaultValueApplicationName); }
		set { SetValue(Common.ConnectionString.DefaultKeyApplicationName, value); }
	}

	[Category("Advanced")]
	[DisplayName("Command Timeout")]
	[Description("The time (in seconds) for command execution.")]
	[DefaultValue(Common.ConnectionString.DefaultValueCommandTimeout)]
	public int CommandTimeout
	{
		get { return Common.ConnectionString.GetInt32(GetKey(Common.ConnectionString.DefaultKeyCommandTimeout), base.TryGetValue, Common.ConnectionString.DefaultValueCommandTimeout); }
		set { SetValue(Common.ConnectionString.DefaultKeyCommandTimeout, value); }
	}

	[Category("Advanced")]
	[DisplayName("Parallel Workers")]
	[Description("Number of parallel workers to use for certain operations in Firebird.")]
	[DefaultValue(Common.ConnectionString.DefaultValueParallelWorkers)]
	public int ParallelWorkers
	{
		get { return Common.ConnectionString.GetInt32(GetKey(Common.ConnectionString.DefaultKeyParallelWorkers), base.TryGetValue, Common.ConnectionString.DefaultValueParallelWorkers); }
		set { SetValue(Common.ConnectionString.DefaultKeyParallelWorkers, value); }
	}

	#endregion

	#region Constructors

	public FbConnectionStringBuilder()
	{ }

	public FbConnectionStringBuilder(string connectionString)
		: this()
	{
		ConnectionString = connectionString;
	}

	#endregion

	#region Private methods

	private FbServerType GetServerType(string keyword, FbServerType defaultValue)
	{
		var key = GetKey(keyword);
		if (!TryGetValue(key, out var value))
			return defaultValue;
		switch (value)
		{
			case FbServerType fbServerType:
				return fbServerType;
			case string s when Enum.TryParse<FbServerType>(s, true, out var enumResult):
				return enumResult;
			default:
				return Common.ConnectionString.GetServerType(key, base.TryGetValue, defaultValue);
		}
	}

	private IsolationLevel GetIsolationLevel(string keyword, IsolationLevel defaultValue)
	{
		var key = GetKey(keyword);
		if (!TryGetValue(key, out var value))
			return defaultValue;
		switch (value)
		{
			case IsolationLevel isolationLevel:
				return isolationLevel;
			case string s when Enum.TryParse<IsolationLevel>(s, true, out var enumResult):
				return enumResult;
			default:
				return Common.ConnectionString.GetIsolationLevel(key, base.TryGetValue, defaultValue);
		}
	}

	private FbWireCrypt GetWireCrypt(string keyword, FbWireCrypt defaultValue)
	{
		var key = GetKey(keyword);
		if (!TryGetValue(key, out var value))
			return defaultValue;
		switch (value)
		{
			case FbWireCrypt fbWireCrypt:
				return fbWireCrypt;
			case string s when Enum.TryParse<FbWireCrypt>(s, true, out var enumResult):
				return enumResult;
			default:
				return Common.ConnectionString.GetWireCrypt(key, base.TryGetValue, defaultValue);
		}
	}

	private byte[] GetBytes(string keyword, byte[] defaultValue)
	{
		var key = GetKey(keyword);
		if (!TryGetValue(key, out var value))
			return defaultValue;
		switch (value)
		{
			case byte[] bytes:
				return bytes;
			case string s:
				return Convert.FromBase64String(s);
			default:
				return defaultValue;
		}
	}

	private void SetValue<T>(string keyword, T value)
	{
		var key = GetKey(keyword);
		if (value is byte[] bytes)
		{
			this[key] = Convert.ToBase64String(bytes);
		}
		else
		{
			this[key] = value;
		}
	}

	private string GetKey(string keyword)
	{
		var synonymKey = Common.ConnectionString.Synonyms[keyword];
		foreach (string key in Keys)
		{
			if (Common.ConnectionString.Synonyms.ContainsKey(key) && Common.ConnectionString.Synonyms[key] == synonymKey)
			{
				synonymKey = key;
				break;
			}
		}
		return synonymKey;
	}

	#endregion
}
