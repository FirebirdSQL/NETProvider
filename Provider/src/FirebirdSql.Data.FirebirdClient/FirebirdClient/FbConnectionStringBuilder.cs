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

using System;
using System.Collections;
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
			get { return GetString(FbConnectionString.DefaultKeyUserId, FbConnectionString.DefaultValueUserId); }
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
			get { return GetString(FbConnectionString.DefaultKeyPassword, FbConnectionString.DefaultValuePassword); }
			set { SetValue(FbConnectionString.DefaultKeyPassword, value); }
		}

		[Category("Source")]
		[DisplayName("DataSource")]
		[Description("The name of the Firebird server to which to connect.")]
		[DefaultValue(FbConnectionString.DefaultValueDataSource)]
		public string DataSource
		{
			get { return GetString(FbConnectionString.DefaultKeyDataSource, FbConnectionString.DefaultValueDataSource); }
			set { SetValue(FbConnectionString.DefaultKeyDataSource, value); }
		}

		[Category("Source")]
		[DisplayName("Database")]
		[Description("The name of the actual database or the database to be used when a connection is open. It is normally the path to an .FDB file or an alias.")]
		[DefaultValue(FbConnectionString.DefaultValueCatalog)]
		public string Database
		{
			get { return GetString(FbConnectionString.DefaultKeyCatalog, FbConnectionString.DefaultValueCatalog); }
			set { SetValue(FbConnectionString.DefaultKeyCatalog, value); }
		}

		[Category("Source")]
		[DisplayName("Port")]
		[Description("Port to use for TCP/IP connections")]
		[DefaultValue(FbConnectionString.DefaultValuePortNumber)]
		public int Port
		{
			get { return GetInt32(FbConnectionString.DefaultKeyPortNumber, FbConnectionString.DefaultValuePortNumber); }
			set { SetValue(FbConnectionString.DefaultKeyPortNumber, value); }
		}

		[Category("Advanced")]
		[DisplayName("PacketSize")]
		[Description("The size (in bytes) of network packets. PacketSize may be in the range 512-32767 bytes.")]
		[DefaultValue(FbConnectionString.DefaultValuePacketSize)]
		public int PacketSize
		{
			get { return GetInt32(FbConnectionString.DefaultKeyPacketSize, FbConnectionString.DefaultValuePacketSize); }
			set { SetValue(FbConnectionString.DefaultKeyPacketSize, value); }
		}

		[Category("Security")]
		[DisplayName("Role")]
		[Description("The user role.")]
		[DefaultValue(FbConnectionString.DefaultValueRoleName)]
		public string Role
		{
			get { return GetString(FbConnectionString.DefaultKeyRoleName, FbConnectionString.DefaultValueRoleName); }
			set { SetValue(FbConnectionString.DefaultKeyRoleName, value); }
		}

		[Category("Advanced")]
		[DisplayName("Dialect")]
		[Description("The database SQL dialect.")]
		[DefaultValue(FbConnectionString.DefaultValueDialect)]
		public int Dialect
		{
			get { return GetInt32(FbConnectionString.DefaultKeyDialect, FbConnectionString.DefaultValueDialect); }
			set { SetValue(FbConnectionString.DefaultKeyDialect, value); }
		}

		[Category("Advanced")]
		[DisplayName("Character Set")]
		[Description("The connection character set encoding.")]
		[DefaultValue(FbConnectionString.DefaultValueCharacterSet)]
		public string Charset
		{
			get { return GetString(FbConnectionString.DefaultKeyCharacterSet, FbConnectionString.DefaultValueCharacterSet); }
			set { SetValue(FbConnectionString.DefaultKeyCharacterSet, value); }
		}

		[Category("Connection")]
		[DisplayName("Connection Timeout")]
		[Description("The time (in seconds) to wait for a connection to open.")]
		[DefaultValue(FbConnectionString.DefaultValueConnectionTimeout)]
		public int ConnectionTimeout
		{
			get { return GetInt32(FbConnectionString.DefaultKeyConnectionTimeout, FbConnectionString.DefaultValueConnectionTimeout); }
			set { SetValue(FbConnectionString.DefaultKeyConnectionTimeout, value); }
		}

		[Category("Pooling")]
		[DisplayName("Pooling")]
		[Description("When true the connection is grabbed from a pool or, if necessary, created and added to the appropriate pool.")]
		[DefaultValue(FbConnectionString.DefaultValuePooling)]
		public bool Pooling
		{
			get { return GetBoolean(FbConnectionString.DefaultKeyPooling, FbConnectionString.DefaultValuePooling); }
			set { SetValue(FbConnectionString.DefaultKeyPooling, value); }
		}

		[Category("Connection")]
		[DisplayName("Connection LifeTime")]
		[Description("When a connection is returned to the pool, its creation time is compared with the current time, and the connection is destroyed if that time span (in seconds) exceeds the value specified by connection lifetime.")]
		[DefaultValue(FbConnectionString.DefaultValueConnectionLifetime)]
		public int ConnectionLifeTime
		{
			get { return GetInt32(FbConnectionString.DefaultKeyConnectionLifetime, FbConnectionString.DefaultValueConnectionLifetime); }
			set { SetValue(FbConnectionString.DefaultKeyConnectionLifetime, value); }
		}

		[Category("Pooling")]
		[DisplayName("MinPoolSize")]
		[Description("The minimun number of connections allowed in the pool.")]
		[DefaultValue(FbConnectionString.DefaultValueMinPoolSize)]
		public int MinPoolSize
		{
			get { return GetInt32(FbConnectionString.DefaultKeyMinPoolSize, FbConnectionString.DefaultValueMinPoolSize); }
			set { SetValue(FbConnectionString.DefaultKeyMinPoolSize, value); }
		}

		[Category("Pooling")]
		[DisplayName("MaxPoolSize")]
		[Description("The maximum number of connections allowed in the pool.")]
		[DefaultValue(FbConnectionString.DefaultValueMaxPoolSize)]
		public int MaxPoolSize
		{
			get { return GetInt32(FbConnectionString.DefaultKeyMaxPoolSize, FbConnectionString.DefaultValueMaxPoolSize); }
			set { SetValue(FbConnectionString.DefaultKeyMaxPoolSize, value); }
		}

		[Category("Advanced")]
		[DisplayName("FetchSize")]
		[Description("The maximum number of rows to be fetched in a single call to read into the internal row buffer.")]
		[DefaultValue(FbConnectionString.DefaultValueFetchSize)]
		public int FetchSize
		{
			get { return GetInt32(FbConnectionString.DefaultKeyFetchSize, FbConnectionString.DefaultValueFetchSize); }
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
			get { return GetBoolean(FbConnectionString.DefaultKeyRecordsAffected, FbConnectionString.DefaultValueRecordsAffected); }
			set { SetValue(FbConnectionString.DefaultKeyRecordsAffected, value); }
		}

		[Category("Pooling")]
		[DisplayName("Enlist")]
		[Description("If true, enlists the connections in the current transaction.")]
		[DefaultValue(FbConnectionString.DefaultValuePooling)]
		public bool Enlist
		{
			get { return GetBoolean(FbConnectionString.DefaultKeyEnlist, FbConnectionString.DefaultValueEnlist); }
			set { SetValue(FbConnectionString.DefaultKeyEnlist, value); }
		}

		[Category("Advanced")]
		[DisplayName("Client Library")]
		[Description("Client library for Firebird Embedded.")]
		[DefaultValue(FbConnectionString.DefaultValueClientLibrary)]
		public string ClientLibrary
		{
			get { return GetString(FbConnectionString.DefaultKeyClientLibrary, FbConnectionString.DefaultValueClientLibrary); }
			set { SetValue(FbConnectionString.DefaultKeyClientLibrary, value); }
		}

		[Category("Advanced")]
		[DisplayName("DB Cache Pages")]
		[Description("How many cache buffers to use for this session.")]
		[DefaultValue(FbConnectionString.DefaultValueDbCachePages)]
		public int DbCachePages
		{
			get { return GetInt32(FbConnectionString.DefaultKeyDbCachePages, FbConnectionString.DefaultValueDbCachePages); }
			set { SetValue(FbConnectionString.DefaultKeyDbCachePages, value); }
		}

		[Category("Advanced")]
		[DisplayName("No Triggers")]
		[Description("Disables database triggers for this connection.")]
		[DefaultValue(FbConnectionString.DefaultValueNoDbTriggers)]
		public bool NoDatabaseTriggers
		{
			get { return GetBoolean(FbConnectionString.DefaultKeyNoDbTriggers, FbConnectionString.DefaultValueNoDbTriggers); }
			set { SetValue(FbConnectionString.DefaultKeyNoDbTriggers, value); }
		}

		[Category("Advanced")]
		[DisplayName("No Garbage Collect")]
		[Description("If true, disables sweeping the database upon attachment.")]
		[DefaultValue(FbConnectionString.DefaultValueNoGarbageCollect)]
		public bool NoGarbageCollect
		{
			get { return GetBoolean(FbConnectionString.DefaultKeyNoGarbageCollect, FbConnectionString.DefaultValueNoGarbageCollect); }
			set { SetValue(FbConnectionString.DefaultKeyNoGarbageCollect, value); }
		}

		[Category("Advanced")]
		[DisplayName("Compression")]
		[Description("Enables or disables wire compression.")]
		[DefaultValue(FbConnectionString.DefaultValueCompression)]
		public bool Compression
		{
			get { return GetBoolean(FbConnectionString.DefaultKeyCompression, FbConnectionString.DefaultValueCompression); }
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

		#endregion

		#region Constructors

		public FbConnectionStringBuilder()
		{
		}

		public FbConnectionStringBuilder(string connectionString)
			: this()
		{
			ConnectionString = connectionString;
		}

		#endregion

		#region Private methods

		private int GetInt32(string keyword, int defaultValue)
		{
			return TryGetValue(GetKey(keyword), out var value)
				? Convert.ToInt32(value)
				: defaultValue;
		}

		private FbServerType GetServerType(string keyword, FbServerType defaultValue)
		{
			if (!TryGetValue(GetKey(keyword), out var value))
				return defaultValue;

			switch (value)
			{
				case FbServerType fbServerType:
					return fbServerType;
				case string s when s == "Default":
					return FbServerType.Default;
				case string s when s == "Embedded":
					return FbServerType.Embedded;
				default:
					return (FbServerType)GetInt32(keyword, (int)defaultValue);
			}
		}

		private IsolationLevel GetIsolationLevel(string keyword, IsolationLevel defaultValue)
		{
			if (!TryGetValue(GetKey(keyword), out var value))
				return defaultValue;

			return (IsolationLevel)GetInt32(keyword, (int)defaultValue);
		}

		private string GetString(string keyword, string defaultValue)
		{
			return TryGetValue(GetKey(keyword), out var value)
				? Convert.ToString(value)
				: defaultValue;
		}

		private bool GetBoolean(string keyword, bool defaultValue)
		{
			return TryGetValue(GetKey(keyword), out var value)
				? Convert.ToBoolean(value)
				: defaultValue;
		}

		private byte[] GetBytes(string keyword, byte[] defaultValue)
		{
			return TryGetValue(GetKey(keyword), out var value)
				? Convert.FromBase64String(value as string)
				: defaultValue;
		}

		private void SetValue<T>(string keyword, T value)
		{
			var index = GetKey(keyword);
			if (typeof(T) == typeof(byte[]))
			{
				this[index] = Convert.ToBase64String(value as byte[]);
			}
			else
			{
				this[index] = value;
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
