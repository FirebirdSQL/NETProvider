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
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.TestsBase;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Tests
{
	public abstract class EntityFrameworkCoreTestsBase : FbTestsBase
	{
		public EntityFrameworkCoreTestsBase()
			: base(FbServerType.Default, false, FbWireCrypt.Enabled, false)
		{ }

		public async Task<TContext> GetDbContext<TContext>() where TContext : FbTestDbContext
		{
			await Connection.CloseAsync();
			return (TContext)Activator.CreateInstance(typeof(TContext), Connection.ConnectionString);
		}
	}
}
