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

using System;
using System.Linq;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Helpers;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.TestUtilities;
using FirebirdSql.EntityFrameworkCore.Firebird.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests;

public class UpdatesFbTest : UpdatesRelationalTestBase<UpdatesFbTest.UpdatesFbFixture>
{
	public UpdatesFbTest(UpdatesFbFixture fixture)
		: base(fixture)
	{ }

	[Fact]
	public override void Identifiers_are_generated_correctly()
	{
		using (var context = CreateContext())
		{
			var entityType = context.Model.FindEntityType(typeof(LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly));
			Assert.Equal(
				"LoginEntityTypeWithAnExtremely~",
				entityType.GetTableName());
			Assert.Equal(
				"PK_LoginEntityTypeWithAnExtrem~",
				entityType.GetKeys().Single().GetName());
			Assert.Equal(
				"FK_LoginEntityTypeWithAnExtrem~",
				entityType.GetForeignKeys().Single().GetConstraintName());
			Assert.Equal(
				"IX_LoginEntityTypeWithAnExtrem~",
				entityType.GetIndexes().Single().GetDatabaseName());
		}
	}

	[Fact(Skip = "Uses type of filtered index that is not supported on Firebird.")]
	public override void Swap_filtered_unique_index_values() => base.Swap_filtered_unique_index_values();

	public class UpdatesFbFixture : UpdatesRelationalFixture
	{
		protected override ITestStoreFactory TestStoreFactory => FbTestStoreFactory.Instance;

		protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
		{
			base.OnModelCreating(modelBuilder, context);
			ModelHelpers.SetStringLengths(modelBuilder);
			ModelHelpers.SetPrimaryKeyGeneration(modelBuilder, FbValueGenerationStrategy.IdentityColumn, x => x.ClrType == typeof(Person));
			modelBuilder.Entity<ProductBase>();
		}
	}
}
