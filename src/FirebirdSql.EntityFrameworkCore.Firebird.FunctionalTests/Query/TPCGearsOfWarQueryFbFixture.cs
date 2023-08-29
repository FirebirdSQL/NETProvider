/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Helpers;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class TPCGearsOfWarQueryFbFixture : TPCGearsOfWarQueryRelationalFixture
{
	protected override ITestStoreFactory TestStoreFactory => FbTestStoreFactory.Instance;

	protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
	{
		base.OnModelCreating(modelBuilder, context);
		ModelHelpers.SetStringLengths(modelBuilder);

		modelBuilder.Entity<City>().Property(g => g.Location).HasColumnType("varchar(100)");

		// No support yet for DateOnly/TimeOnly (#24507)
		modelBuilder.Entity<Mission>(
			b =>
			{
				b.Ignore(m => m.Date);
				b.Ignore(m => m.Time);
			});
	}
}
