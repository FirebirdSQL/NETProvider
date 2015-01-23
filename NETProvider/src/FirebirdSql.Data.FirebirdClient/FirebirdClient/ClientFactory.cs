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
 *  All Rights Reserved.
 * 
 *  Contributors:
 *    Jiri Cincura (jiri@cincura.net)
 */

using System;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.FirebirdClient
{
	internal sealed class ClientFactory
	{
		#region Static Methods

		public static IDatabase CreateDatabase(FbConnectionString options)
		{
			switch (options.ServerType)
			{
				case FbServerType.Default:
					// Managed Client
					return CreateManagedDatabase(options);

				case FbServerType.Embedded:
					// Native (PInvoke) Client
					return new FirebirdSql.Data.Client.Native.FesDatabase(options.ClientLibrary, Charset.GetCharset(options.Charset));

				case FbServerType.Context:
					// External Engine (PInvoke) Client
					return new FirebirdSql.Data.Client.ExternalEngine.ExtDatabase();

				default:
					throw new NotSupportedException("Specified server type is not correct.");
			}
		}

		public static IServiceManager CreateServiceManager(FbConnectionString options)
		{
			switch (options.ServerType)
			{
				case FbServerType.Default:
					return CreateManagedServiceManager(options);

				case FbServerType.Embedded:
					// PInvoke Client
					return new FirebirdSql.Data.Client.Native.FesServiceManager(options.ClientLibrary, Charset.GetCharset(options.Charset));

				default:
					throw new NotSupportedException("Specified server type is not correct.");
			}
		}

		private static IDatabase CreateManagedDatabase(FbConnectionString options)
		{
			FirebirdSql.Data.Client.Managed.Version10.GdsConnection connection = new FirebirdSql.Data.Client.Managed.Version10.GdsConnection(options.DataSource, options.Port, options.PacketSize, Charset.GetCharset(options.Charset));

			connection.Connect();
			connection.Identify(options.Database);

			switch (connection.ProtocolVersion)
			{
				case IscCodes.PROTOCOL_VERSION12:
					return new FirebirdSql.Data.Client.Managed.Version12.GdsDatabase(connection);
				case IscCodes.PROTOCOL_VERSION11:
					return new FirebirdSql.Data.Client.Managed.Version11.GdsDatabase(connection);
				case IscCodes.PROTOCOL_VERSION10:
					return new FirebirdSql.Data.Client.Managed.Version10.GdsDatabase(connection);
				default:
					throw new NotSupportedException("Protocol not supported.");
			}
		}

		private static IServiceManager CreateManagedServiceManager(FbConnectionString options)
		{
			FirebirdSql.Data.Client.Managed.Version10.GdsConnection connection = new FirebirdSql.Data.Client.Managed.Version10.GdsConnection(options.DataSource, options.Port, options.PacketSize, Charset.GetCharset(options.Charset));

			connection.Connect();
			connection.Identify(!string.IsNullOrEmpty(options.Database) ? options.Database : string.Empty);

			switch (connection.ProtocolVersion)
			{
				case IscCodes.PROTOCOL_VERSION12:
				case IscCodes.PROTOCOL_VERSION11:
				case IscCodes.PROTOCOL_VERSION10:
					return new FirebirdSql.Data.Client.Managed.Version10.GdsServiceManager(connection);
				default:
					throw new NotSupportedException("Protocol not supported.");
			}
		}
		#endregion

		#region Constructors

		private ClientFactory()
		{ }

		#endregion
	}
}
