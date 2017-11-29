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

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.TestsBase;

namespace EntityFramework.Firebird.Tests
{
	public abstract class EntityFrameworkTestsBase : FbTestsBase
	{
		public EntityFrameworkTestsBase()
			: base(FbServerType.Default, false, false)
		{ }

		public DbProviderServices GetProviderServices()
		{
			return FbProviderServices.Instance;
		}

		public TContext GetDbContext<TContext>() where TContext : FbTestDbContext
		{
			Database.SetInitializer<TContext>(null);
			Connection.Close();
			return (TContext)Activator.CreateInstance(typeof(TContext), Connection);
		}
	}

	public class FbTestDbContext : DbContext
	{
		public FbTestDbContext(FbConnection conn)
			: base(conn, false)
		{ }
	}
}
