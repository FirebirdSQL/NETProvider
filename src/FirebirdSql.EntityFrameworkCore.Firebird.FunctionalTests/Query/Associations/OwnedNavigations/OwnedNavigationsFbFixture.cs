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

//$Authors = Niek Schoemaker (@niekschoemaker)

using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Associations;
using Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query.Associations.OwnedNavigations;

public class OwnedNavigationsFbFixture : OwnedNavigationsRelationalFixtureBase
{
	protected override ITestStoreFactory TestStoreFactory
		=> FbTestStoreFactory.Instance;

	protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
	{
		base.OnModelCreating(modelBuilder, context);

		// This directly overrides the Table names from OwnedNavigationsRelationalFixtureBase
		// This is needed as it otherwise exceeds the column length of Firebird 3 & 4.
		modelBuilder.Entity<RootEntity>(b =>
		{
			b.OwnsOne(
				e => e.RequiredAssociate, rrb =>
				{
					rrb.OwnsMany(
						r => r.NestedCollection, rcb =>
						{
							rcb.ToTable("RR_NC"); // RequiredRelated_NestedCollection
						});
				});

			b.OwnsOne(
				e => e.OptionalAssociate, orb =>
				{
					orb.OwnsMany(
						r => r.NestedCollection, rcb =>
						{
							rcb.ToTable("OR_NC"); // OptionalRelated_NestedCollection
						});
				});

			b.OwnsMany(
				e => e.AssociateCollection, rcb =>
				{
					rcb.OwnsMany(
						r => r.NestedCollection, rcb =>
						{
							rcb.ToTable("RC_NC"); // RelatedCollection_NestedCollection
						});
				});


			b.OwnsOne(
				e => e.RequiredAssociate, rrb =>
				{
					rrb.OwnsOne(r => r.RequiredNestedAssociate, rnb => rnb.ToTable("RR_RN")); // RequiredRelated_RequiredNested
					rrb.OwnsOne(r => r.OptionalNestedAssociate, rnb => rnb.ToTable("RR_ON")); // RequiredRelated_OptionalNested
				});

			b.OwnsOne(
				e => e.OptionalAssociate, rrb =>
				{
					rrb.OwnsOne(r => r.RequiredNestedAssociate, rnb => rnb.ToTable("OR_RN")); // OptionalRelated_RequiredNested
					rrb.OwnsOne(r => r.OptionalNestedAssociate, rnb => rnb.ToTable("OR_ON")); // OptionalRelated_OptionalNested
				});

			b.OwnsMany(
				e => e.AssociateCollection, rcb =>
				{
					rcb.OwnsOne(r => r.RequiredNestedAssociate, rnb => rnb.ToTable("RC_RN")); // RelatedCollection_RequiredNested
					rcb.OwnsOne(r => r.OptionalNestedAssociate, rnb => rnb.ToTable("RC_ON")); // RelatedCollection_OptionalNested
				});
		});
	}
}
