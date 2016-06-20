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
 *  Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *  Copyright (c) 2016 Jiri Cincura (jiri@cincura.net)
 *  All Rights Reserved.
 */

using System;
using FirebirdSql.Data.Client.Managed;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.FirebirdClient
{
	internal static class ClientFactory
	{
		public static IDatabase CreateDatabase(FbConnectionString options)
		{
			switch (options.ServerType)
			{
				case FbServerType.Default:
					return CreateManagedDatabase(options);

				case FbServerType.Embedded:
					return new Client.Native.FesDatabase(options.ClientLibrary, Charset.GetCharset(options.Charset));

				default:
					throw IncorrectServerTypeException();
			}
		}

		public static IServiceManager CreateServiceManager(FbConnectionString options)
		{
			switch (options.ServerType)
			{
				case FbServerType.Default:
					return CreateManagedServiceManager(options);

				case FbServerType.Embedded:
					return new Client.Native.FesServiceManager(options.ClientLibrary, Charset.GetCharset(options.Charset));

				default:
					throw IncorrectServerTypeException();
			}
		}

		private static IDatabase CreateManagedDatabase(FbConnectionString options)
		{
			var connection = new GdsConnection(options.UserID, options.Password, options.DataSource, options.Port, options.PacketSize, Charset.GetCharset(options.Charset), options.Compression);
			connection.Connect();
			connection.Identify(options.Database);

			switch (connection.ProtocolVersion)
			{
				case IscCodes.PROTOCOL_VERSION13:
					return new Client.Managed.Version13.GdsDatabase(connection);
				case IscCodes.PROTOCOL_VERSION12:
					return new Client.Managed.Version12.GdsDatabase(connection);
				case IscCodes.PROTOCOL_VERSION11:
					return new Client.Managed.Version11.GdsDatabase(connection);
				case IscCodes.PROTOCOL_VERSION10:
					return new Client.Managed.Version10.GdsDatabase(connection);
				default:
					throw UnsupportedProtocolException();
			}
		}

		private static IServiceManager CreateManagedServiceManager(FbConnectionString options)
		{
			var connection = new GdsConnection(options.UserID, options.Password, options.DataSource, options.Port, options.PacketSize, Charset.GetCharset(options.Charset), options.Compression);
			connection.Connect();
			connection.Identify(!string.IsNullOrEmpty(options.Database) ? options.Database : string.Empty);

			switch (connection.ProtocolVersion)
			{
				case IscCodes.PROTOCOL_VERSION13:
				case IscCodes.PROTOCOL_VERSION12:
				case IscCodes.PROTOCOL_VERSION11:
				case IscCodes.PROTOCOL_VERSION10:
					return new Client.Managed.Version10.GdsServiceManager(connection);
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
	}
}
