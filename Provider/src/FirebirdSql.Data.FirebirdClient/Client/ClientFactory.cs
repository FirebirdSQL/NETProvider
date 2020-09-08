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
using FirebirdSql.Data.Client.Managed;
using FirebirdSql.Data.Common;
using FirebirdSql.Data.FirebirdClient;
using WireCryptOption = FirebirdSql.Data.Client.Managed.Version13.WireCryptOption;

namespace FirebirdSql.Data.Client
{
	internal static class ClientFactory
	{
		public static IDatabase CreateDatabase(ConnectionString options)
		{
			switch (options.ServerType)
			{
				case FbServerType.Default:
					return CreateManagedDatabase(options);
				case FbServerType.Embedded:
					return new Native.FesDatabase(options.ClientLibrary, Charset.GetCharset(options.Charset));
				default:
					throw IncorrectServerTypeException();
			}
		}

		public static IServiceManager CreateServiceManager(ConnectionString options)
		{
			switch (options.ServerType)
			{
				case FbServerType.Default:
					return CreateManagedServiceManager(options);
				case FbServerType.Embedded:
					return new Native.FesServiceManager(options.ClientLibrary, Charset.GetCharset(options.Charset));
				default:
					throw IncorrectServerTypeException();
			}
		}

		private static IDatabase CreateManagedDatabase(ConnectionString options)
		{
			var connection = new GdsConnection(options.UserID, options.Password, options.DataSource, options.Port, options.PacketSize, Charset.GetCharset(options.Charset), options.Compression, FbWireCryptToWireCryptOption(options.WireCrypt));
			connection.Connect();
			connection.Identify(options.Database);
			switch (connection.ProtocolVersion)
			{
				case IscCodes.PROTOCOL_VERSION13:
					return new Managed.Version13.GdsDatabase(connection);
				case IscCodes.PROTOCOL_VERSION12:
					return new Managed.Version12.GdsDatabase(connection);
				case IscCodes.PROTOCOL_VERSION11:
					return new Managed.Version11.GdsDatabase(connection);
				case IscCodes.PROTOCOL_VERSION10:
					return new Managed.Version10.GdsDatabase(connection);
				default:
					throw UnsupportedProtocolException();
			}
		}

		private static IServiceManager CreateManagedServiceManager(ConnectionString options)
		{
			var connection = new GdsConnection(options.UserID, options.Password, options.DataSource, options.Port, options.PacketSize, Charset.GetCharset(options.Charset), options.Compression, FbWireCryptToWireCryptOption(options.WireCrypt));
			connection.Connect();
			connection.Identify(!string.IsNullOrEmpty(options.Database) ? options.Database : string.Empty);
			switch (connection.ProtocolVersion)
			{
				case IscCodes.PROTOCOL_VERSION13:
					return new Managed.Version13.GdsServiceManager(connection);
				case IscCodes.PROTOCOL_VERSION12:
					return new Managed.Version12.GdsServiceManager(connection);
				case IscCodes.PROTOCOL_VERSION11:
					return new Managed.Version11.GdsServiceManager(connection);
				case IscCodes.PROTOCOL_VERSION10:
					return new Managed.Version10.GdsServiceManager(connection);
				default:
					throw UnsupportedProtocolException();
			}
		}

		private static NotSupportedException UnsupportedProtocolException()
		{
			return new NotSupportedException("Protocol not supported.");
		}

		private static Exception IncorrectServerTypeException()
		{
			return new NotSupportedException("Specified server type is not correct.");
		}

		private static WireCryptOption FbWireCryptToWireCryptOption(FbWireCrypt wireCrypt)
		{
			switch (wireCrypt)
			{
				case FbWireCrypt.Disabled:
					return WireCryptOption.Disabled;
				case FbWireCrypt.Enabled:
					return WireCryptOption.Enabled;
				case FbWireCrypt.Required:
					return WireCryptOption.Required;
				default:
					throw new ArgumentOutOfRangeException(nameof(wireCrypt), $"{nameof(wireCrypt)}={wireCrypt}");
			}
		}
	}
}
