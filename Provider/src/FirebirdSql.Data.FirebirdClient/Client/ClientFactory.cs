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
using System.Threading.Tasks;
using FirebirdSql.Data.Client.Managed;
using FirebirdSql.Data.Common;
using FirebirdSql.Data.FirebirdClient;
using WireCryptOption = FirebirdSql.Data.Client.Managed.Version13.WireCryptOption;

namespace FirebirdSql.Data.Client
{
	internal static class ClientFactory
	{
		public static async ValueTask<DatabaseBase> CreateDatabaseAsync(ConnectionString options, AsyncWrappingCommonArgs async)
		{
			return options.ServerType switch
			{
				FbServerType.Default => await CreateManagedDatabaseAsync(options, async).ConfigureAwait(false),
				FbServerType.Embedded => new Native.FesDatabase(options.ClientLibrary, Charset.GetCharset(options.Charset)),
				_ => throw IncorrectServerTypeException(),
			};
		}

		public static async ValueTask<ServiceManagerBase> CreateServiceManagerAsync(ConnectionString options, AsyncWrappingCommonArgs async)
		{
			return options.ServerType switch
			{
				FbServerType.Default => await CreateManagedServiceManagerAsync(options, async).ConfigureAwait(false),
				FbServerType.Embedded => new Native.FesServiceManager(options.ClientLibrary, Charset.GetCharset(options.Charset)),
				_ => throw IncorrectServerTypeException(),
			};
		}

		private static async ValueTask<DatabaseBase> CreateManagedDatabaseAsync(ConnectionString options, AsyncWrappingCommonArgs async)
		{
			var connection = new GdsConnection(options.UserID, options.Password, options.DataSource, options.Port, options.ConnectionTimeout, options.PacketSize, Charset.GetCharset(options.Charset), options.Compression, FbWireCryptToWireCryptOption(options.WireCrypt));
			await connection.ConnectAsync(async).ConfigureAwait(false);
			try
			{
				await connection.IdentifyAsync(options.Database, async).ConfigureAwait(false);
			}
			catch
			{
				await connection.DisconnectAsync(async).ConfigureAwait(false);
				throw;
			}
			return connection.ProtocolVersion switch
			{
				IscCodes.PROTOCOL_VERSION13 => new Managed.Version13.GdsDatabase(connection),
				IscCodes.PROTOCOL_VERSION12 => new Managed.Version12.GdsDatabase(connection),
				IscCodes.PROTOCOL_VERSION11 => new Managed.Version11.GdsDatabase(connection),
				IscCodes.PROTOCOL_VERSION10 => new Managed.Version10.GdsDatabase(connection),
				_ => throw UnsupportedProtocolException(),
			};
		}

		private static async ValueTask<ServiceManagerBase> CreateManagedServiceManagerAsync(ConnectionString options, AsyncWrappingCommonArgs async)
		{
			var connection = new GdsConnection(options.UserID, options.Password, options.DataSource, options.Port, options.ConnectionTimeout, options.PacketSize, Charset.GetCharset(options.Charset), options.Compression, FbWireCryptToWireCryptOption(options.WireCrypt));
			await connection.ConnectAsync(async).ConfigureAwait(false);
			try
			{
				await connection.IdentifyAsync(!string.IsNullOrEmpty(options.Database) ? options.Database : string.Empty, async).ConfigureAwait(false);
			}
			catch
			{
				await connection.DisconnectAsync(async).ConfigureAwait(false);
				throw;
			}
			return connection.ProtocolVersion switch
			{
				IscCodes.PROTOCOL_VERSION13 => new Managed.Version13.GdsServiceManager(connection),
				IscCodes.PROTOCOL_VERSION12 => new Managed.Version12.GdsServiceManager(connection),
				IscCodes.PROTOCOL_VERSION11 => new Managed.Version11.GdsServiceManager(connection),
				IscCodes.PROTOCOL_VERSION10 => new Managed.Version10.GdsServiceManager(connection),
				_ => throw UnsupportedProtocolException(),
			};
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
			return wireCrypt switch
			{
				FbWireCrypt.Disabled => WireCryptOption.Disabled,
				FbWireCrypt.Enabled => WireCryptOption.Enabled,
				FbWireCrypt.Required => WireCryptOption.Required,
				_ => throw new ArgumentOutOfRangeException(nameof(wireCrypt), $"{nameof(wireCrypt)}={wireCrypt}"),
			};
		}
	}
}
