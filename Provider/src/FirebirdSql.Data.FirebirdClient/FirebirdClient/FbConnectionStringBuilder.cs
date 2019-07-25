/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
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

namespace FirebirdSql.Data.FirebirdClient
{
	public class FbConnectionStringBuilder : DbConnectionStringBuilder
	{
		#region Properties

		[Category("Security")]
		[DisplayName("User ID")]
		[Description("Indicates the User ID to be used when connecting to the data source.")]
		[DefaultValue(FbConnectionString.DefaultValueUserId)]
		public string UserID
		{
			get { return FbConnectionString.GetString(GetKey(FbConnectionString.DefaultKeyUserId), TryGetValue, FbConnectionString.DefaultValueUserId); }
			set { SetValue(FbConnectionString.DefaultKeyUserId, value); }
		}

		[Category("Security")]
		[DisplayName("Password")]
		[Description("Indicates the password to be used when connecting to the data source.")]
#if !NETSTANDARD1_6
		[PasswordPropertyText(true)]
#endif
		[DefaultValue(FbConnectionString.DefaultValuePassword)]
		public string Password
		{
			get { return FbConnectionString.GetString(GetKey(FbConnectionString.DefaultKeyPassword), TryGetValue, FbConnectionString.DefaultValuePassword); }
			set { SetValue(FbConnectionString.DefaultKeyPassword, value); }
		}

		[Category("Source")]
		[DisplayName("DataSource")]
		[Description("The name of the Firebird server to which to connect.")]
		[DefaultValue(FbConnectionString.DefaultValueDataSource)]
		public string DataSource
		{
			get { return FbConnectionString.GetString(GetKey(FbConnectionString.DefaultKeyDataSource), TryGetValue, FbConnectionString.DefaultValueDataSource); }
			set { SetValue(FbConnectionString.DefaultKeyDataSource, value); }
		}

		[Category("Source")]
		[DisplayName("Database")]
		[Description("The name of the actual database or the database to be used when a connection is open. It is normally the path to an .FDB file or an alias.")]
		[DefaultValue(FbConnectionString.DefaultValueCatalog)]
		public string Database
		{
			get { return FbConnectionString.GetString(GetKey(FbConnectionString.DefaultKeyCatalog), TryGetValue, FbConnectionString.DefaultValueCatalog); }
			set { SetValue(FbConnectionString.DefaultKeyCatalog, value); }
		}

		[Category("Source")]
		[DisplayName("Port")]
		[Description("Port to use for TCP/IP connections")]
		[DefaultValue(FbConnectionString.DefaultValuePortNumber)]
		public int Port
		{
			get { return FbConnectionString.GetInt32(GetKey(FbConnectionString.DefaultKeyPortNumber), TryGetValue, FbConnectionString.DefaultValuePortNumber); }
			set { SetValue(FbConnectionString.DefaultKeyPortNumber, value); }
		}

		[Category("Advanced")]
		[DisplayName("PacketSize")]
		[Description("The size (in bytes) of network packets. PacketSize may be in the range 512-32767 bytes.")]
		[DefaultValue(FbConnectionString.DefaultValuePacketSize)]
		public int PacketSize
		{
			get { return FbConnectionString.GetInt32(GetKey(FbConnectionString.DefaultKeyPacketSize), TryGetValue, FbConnectionString.DefaultValuePacketSize); }
			set { SetValue(FbConnectionString.DefaultKeyPacketSize, value); }
		}

		[Category("Security")]
		[DisplayName("Role")]
		[Description("The user role.")]
		[DefaultValue(FbConnectionString.DefaultValueRoleName)]
		public string Role
		{
			get { return FbConnectionString.GetString(GetKey(FbConnectionString.DefaultKeyRoleName), TryGetValue, FbConnectionString.DefaultValueRoleName); }
			set { SetValue(FbConnectionString.DefaultKeyRoleName, value); }
		}

		[Category("Advanced")]
		[DisplayName("Dialect")]
		[Description("The database SQL dialect.")]
		[DefaultValue(FbConnectionString.DefaultValueDialect)]
		public int Dialect
		{
			get { return FbConnectionString.GetInt32(GetKey(FbConnectionString.DefaultKeyDialect), TryGetValue, FbConnectionString.DefaultValueDialect); }
			set { SetValue(FbConnectionString.DefaultKeyDialect, value); }
		}

		[Category("Advanced")]
		[DisplayName("Character Set")]
		[Description("The connection character set encoding.")]
		[DefaultValue(FbConnectionString.DefaultValueCharacterSet)]
		public string Charset
		{
			get { return FbConnectionString.GetString(GetKey(FbConnectionString.DefaultKeyCharacterSet), TryGetValue, FbConnectionString.DefaultValueCharacterSet); }
			set { SetValue(FbConnectionString.DefaultKeyCharacterSet, value); }
		}

		[Category("Connection")]
		[DisplayName("Connection Timeout")]
		[Description("The time (in seconds) to wait for a connection to open.")]
		[DefaultValue(FbConnectionString.DefaultValueConnectionTimeout)]
		public int ConnectionTimeout
		{
			get { return FbConnectionString.GetInt32(GetKey(FbConnectionString.DefaultKeyConnectionTimeout), TryGetValue, FbConnectionString.DefaultValueConnectionTimeout); }
			set { SetValue(FbConnectionString.DefaultKeyConnectionTimeout, value); }
		}

		[Category("Pooling")]
		[DisplayName("Pooling")]
		[Description("When true the connection is grabbed from a pool or, if necessary, created and added to the appropriate pool.")]
		[DefaultValue(FbConnectionString.DefaultValuePooling)]
		public bool Pooling
		{
			get { return FbConnectionString.GetBoolean(GetKey(FbConnectionString.DefaultKeyPooling), TryGetValue, FbConnectionString.DefaultValuePooling); }
			set { SetValue(FbConnectionString.DefaultKeyPooling, value); }
		}

		[Category("Connection")]
		[DisplayName("Connection LifeTime")]
		[Description("When a connection is returned to the pool, its creation time is compared with the current time, and the connection is destroyed if that time span (in seconds) exceeds the value specified by connection lifetime.")]
		[DefaultValue(FbConnectionString.DefaultValueConnectionLifetime)]
		public int ConnectionLifeTime
		{
			get { return FbConnectionString.GetInt32(GetKey(FbConnectionString.DefaultKeyConnectionLifetime), TryGetValue, FbConnectionString.DefaultValueConnectionLifetime); }
			set { SetValue(FbConnectionString.DefaultKeyConnectionLifetime, value); }
		}

		[Category("Pooling")]
		[DisplayName("MinPoolSize")]
		[Description("The minimun number of connections allowed in the pool.")]
		[DefaultValue(FbConnectionString.DefaultValueMinPoolSize)]
		public int MinPoolSize
		{
			get { return FbConnectionString.GetInt32(GetKey(FbConnectionString.DefaultKeyMinPoolSize), TryGetValue, FbConnectionString.DefaultValueMinPoolSize); }
			set { SetValue(FbConnectionString.DefaultKeyMinPoolSize, value); }
		}

		[Category("Pooling")]
		[DisplayName("MaxPoolSize")]
		[Description("The maximum number of connections allowed in the pool.")]
		[DefaultValue(FbConnectionString.DefaultValueMaxPoolSize)]
		public int MaxPoolSize
		{
			get { return FbConnectionString.GetInt32(GetKey(FbConnectionString.DefaultKeyMaxPoolSize), TryGetValue, FbConnectionString.DefaultValueMaxPoolSize); }
			set { SetValue(FbConnectionString.DefaultKeyMaxPoolSize, value); }
		}

		[Category("Advanced")]
		[DisplayName("FetchSize")]
		[Description("The maximum number of rows to be fetched in a single call to read into the internal row buffer.")]
		[DefaultValue(FbConnectionString.DefaultValueFetchSize)]
		public int FetchSize
		{
			get { return FbConnectionString.GetInt32(GetKey(FbConnectionString.DefaultKeyFetchSize), TryGetValue, FbConnectionString.DefaultValueFetchSize); }
			set { SetValue(FbConnectionString.DefaultKeyFetchSize, value); }
		}

		[Category("Source")]
		[DisplayName("ServerType")]
		[Description("The type of server used.")]
		[DefaultValue(FbConnectionString.DefaultValueServerType)]
		public FbServerType ServerType
		{
			get { return GetServerType(FbConnectionString.DefaultKeyServerType, FbConnectionString.DefaultValueServerType); }
			set { SetValue(FbConnectionString.DefaultKeyServerType, value); }
		}

		[Category("Advanced")]
		[DisplayName("IsolationLevel")]
		[Description("The default Isolation Level for implicit transactions.")]
		[DefaultValue(FbConnectionString.DefaultValueIsolationLevel)]
		public IsolationLevel IsolationLevel
		{
			get { return GetIsolationLevel(FbConnectionString.DefaultKeyIsolationLevel, FbConnectionString.DefaultValueIsolationLevel); }
			set { SetValue(FbConnectionString.DefaultKeyIsolationLevel, value); }
		}

		[Category("Advanced")]
		[DisplayName("Records Affected")]
		[Description("Get the number of rows affected by a command when true.")]
		[DefaultValue(FbConnectionString.DefaultValueRecordsAffected)]
		public bool ReturnRecordsAffected
		{
			get { return FbConnectionString.GetBoolean(GetKey(FbConnectionString.DefaultKeyRecordsAffected), TryGetValue, FbConnectionString.DefaultValueRecordsAffected); }
			set { SetValue(FbConnectionString.DefaultKeyRecordsAffected, value); }
		}

		[Category("Pooling")]
		[DisplayName("Enlist")]
		[Description("If true, enlists the connections in the current transaction.")]
		[DefaultValue(FbConnectionString.DefaultValuePooling)]
		public bool Enlist
		{
			get { return FbConnectionString.GetBoolean(GetKey(FbConnectionString.DefaultKeyEnlist), TryGetValue, FbConnectionString.DefaultValueEnlist); }
			set { SetValue(FbConnectionString.DefaultKeyEnlist, value); }
		}

		[Category("Advanced")]
		[DisplayName("Client Library")]
		[Description("Client library for Firebird Embedded.")]
		[DefaultValue(FbConnectionString.DefaultValueClientLibrary)]
		public string ClientLibrary
		{
			get { return FbConnectionString.GetString(GetKey(FbConnectionString.DefaultKeyClientLibrary), TryGetValue, FbConnectionString.DefaultValueClientLibrary); }
			set { SetValue(FbConnectionString.DefaultKeyClientLibrary, value); }
		}

		[Category("Advanced")]
		[DisplayName("DB Cache Pages")]
		[Description("How many cache buffers to use for this session.")]
		[DefaultValue(FbConnectionString.DefaultValueDbCachePages)]
		public int DbCachePages
		{
			get { return FbConnectionString.GetInt32(GetKey(FbConnectionString.DefaultKeyDbCachePages), TryGetValue, FbConnectionString.DefaultValueDbCachePages); }
			set { SetValue(FbConnectionString.DefaultKeyDbCachePages, value); }
		}

		[Category("Advanced")]
		[DisplayName("No Triggers")]
		[Description("Disables database triggers for this connection.")]
		[DefaultValue(FbConnectionString.DefaultValueNoDbTriggers)]
		public bool NoDatabaseTriggers
		{
			get { return FbConnectionString.GetBoolean(GetKey(FbConnectionString.DefaultKeyNoDbTriggers), TryGetValue, FbConnectionString.DefaultValueNoDbTriggers); }
			set { SetValue(FbConnectionString.DefaultKeyNoDbTriggers, value); }
		}

		[Category("Advanced")]
		[DisplayName("No Garbage Collect")]
		[Description("If true, disables sweeping the database upon attachment.")]
		[DefaultValue(FbConnectionString.DefaultValueNoGarbageCollect)]
		public bool NoGarbageCollect
		{
			get { return FbConnectionString.GetBoolean(GetKey(FbConnectionString.DefaultKeyNoGarbageCollect), TryGetValue, FbConnectionString.DefaultValueNoGarbageCollect); }
			set { SetValue(FbConnectionString.DefaultKeyNoGarbageCollect, value); }
		}

		[Category("Advanced")]
		[DisplayName("Compression")]
		[Description("Enables or disables wire compression.")]
		[DefaultValue(FbConnectionString.DefaultValueCompression)]
		public bool Compression
		{
			get { return FbConnectionString.GetBoolean(GetKey(FbConnectionString.DefaultKeyCompression), TryGetValue, FbConnectionString.DefaultValueCompression); }
			set { SetValue(FbConnectionString.DefaultKeyCompression, value); }
		}

		[Category("Advanced")]
		[DisplayName("CryptKey")]
		[Description("Key used for database decryption.")]
		[DefaultValue(FbConnectionString.DefaultValueCryptKey)]
		public byte[] CryptKey
		{
			get { return GetBytes(FbConnectionString.DefaultKeyCryptKey, FbConnectionString.DefaultValueCryptKey); }
			set { SetValue(FbConnectionString.DefaultKeyCryptKey, value); }
		}

		[Category("Advanced")]
		[DisplayName("WireCrypt")]
		[Description("Selection for wire encryption.")]
		[DefaultValue(FbConnectionString.DefaultValueWireCrypt)]
		public FbWireCrypt WireCrypt
		{
			get { return GetWireCrypt(FbConnectionString.DefaultKeyWireCrypt, FbConnectionString.DefaultValueWireCrypt); }
			set { SetValue(FbConnectionString.DefaultKeyWireCrypt, value); }
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
					return FbConnectionString.GetServerType(key, TryGetValue, defaultValue);
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
					return FbConnectionString.GetIsolationLevel(key, TryGetValue, defaultValue);
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
					return FbConnectionString.GetWireCrypt(key, TryGetValue, defaultValue);
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
			var synonymKey = FbConnectionString.Synonyms[keyword];
			foreach (string key in Keys)
			{
				if (FbConnectionString.Synonyms.ContainsKey(key) && FbConnectionString.Synonyms[key] == synonymKey)
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
