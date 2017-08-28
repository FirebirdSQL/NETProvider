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
 *   Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Data.Common;
#if EF6
using System.Data.Entity.Core.Common;
#endif

namespace FirebirdSql.Data.FirebirdClient
{
	public class FirebirdClientFactory : DbProviderFactory, IServiceProvider
	{
		#region Static Properties

		public static readonly FirebirdClientFactory Instance = new FirebirdClientFactory();

		#endregion

		#region Properties

#if !NETSTANDARD1_6
		public override bool CanCreateDataSourceEnumerator
		{
			get { return false; }
		}
#endif

		#endregion

		#region Constructors

		private FirebirdClientFactory()
			: base()
		{ }

		#endregion

		#region Methods

		public override DbCommand CreateCommand()
		{
			return new FbCommand();
		}

#if !NETSTANDARD1_6
		public override DbCommandBuilder CreateCommandBuilder()
		{
			return new FbCommandBuilder();
		}
#endif

		public override DbConnection CreateConnection()
		{
			return new FbConnection();
		}

		public override DbConnectionStringBuilder CreateConnectionStringBuilder()
		{
			return new FbConnectionStringBuilder();
		}

#if !NETSTANDARD1_6
		public override DbDataAdapter CreateDataAdapter()
		{
			return new FbDataAdapter();
		}
#endif

		public override DbParameter CreateParameter()
		{
			return new FbParameter();
		}

		#endregion

		#region IServiceProvider Members

		object IServiceProvider.GetService(Type serviceType)
		{
#if NETSTANDARD1_6 || NETSTANDARD2_0
			return null;
#else
			if (serviceType == typeof(DbProviderServices))
			{
				return FbProviderServices.Instance;
			}
			else
			{
				return null;
			}
#endif
		}

		#endregion
	}
}
