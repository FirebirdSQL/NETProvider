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

using System.Linq;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query
{
	public class FiltersInheritanceQueryFbTest : FiltersInheritanceQueryTestBase<FiltersInheritanceQueryFbFixture>
	{
		public FiltersInheritanceQueryFbTest(FiltersInheritanceQueryFbFixture fixture)
			: base(fixture)
		{ }

		[Fact(Skip = "Currently wrong test.")]
		public override void Can_use_is_kiwi_in_projection()
		{
			base.Can_use_is_kiwi_in_projection();
		}

		[Fact(Skip = "Currently wrong test.")]
		public override void Can_use_of_type_animal()
		{
			base.Can_use_of_type_animal();
		}

		[Fact(Skip = "Currently wrong test.")]
		public override void Can_use_of_type_bird()
		{
			base.Can_use_of_type_bird();
		}

		[Fact(Skip = "Currently wrong test.")]
		public override void Can_use_of_type_bird_with_projection()
		{
			base.Can_use_of_type_bird_with_projection();
		}

		[Fact(Skip = "Currently wrong test.")]
		public override void Can_use_derived_set()
		{
			base.Can_use_derived_set();
		}
	}
}
