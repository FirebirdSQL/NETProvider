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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query
{
	public class NorthwindQueryFbFixture<TModelCustomizer> : NorthwindQueryRelationalFixture<TModelCustomizer>
		where TModelCustomizer : IModelCustomizer, new()
	{
		protected override ITestStoreFactory TestStoreFactory => FbTestStoreFactory.Instance;

		protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
		{
			base.OnModelCreating(modelBuilder, context);

			// quick and dirty
			foreach (var entityType in modelBuilder.Model.GetEntityTypes())
			{
				var pk = entityType.FindPrimaryKey();
				if (pk != null)
				{
					pk.Relational().Name = $"PK_{DateTime.UtcNow.Ticks}";
				}

				foreach (var fk in entityType.GetForeignKeys())
				{
					fk.Relational().Name = $"FK_{DateTime.UtcNow.Ticks}";
				}

				foreach (var c in entityType.GetProperties())
				{
					if (c.ClrType == typeof(string) && c.GetMaxLength() == null)
					{
						c.SetMaxLength(1000);
					}
				}
			}
		}
	}
}
