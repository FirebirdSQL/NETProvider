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
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Tests2
{
	public class MigrationsFbTest : MigrationsTestBase<MigrationsFbFixture>
	{
		public MigrationsFbTest(MigrationsFbFixture fixture)
			: base(fixture)
		{ }

		protected override void GiveMeSomeTime(DbContext db)
		{ }

		protected override Task GiveMeSomeTimeAsync(DbContext db)
			=> Task.CompletedTask;

		public override void Can_generate_idempotent_up_scripts()
		{
			Assert.Throws<NotSupportedException>(base.Can_generate_idempotent_up_scripts);
		}

		public override void Can_generate_idempotent_down_scripts()
		{
			Assert.Throws<NotSupportedException>(base.Can_generate_idempotent_down_scripts);
		}

		[WaitingForFixMerge]
		public override void Can_apply_all_migrations()
			=> base.Can_apply_all_migrations();

		[WaitingForFixMerge]
		public override Task Can_apply_all_migrations_async()
			=> base.Can_apply_all_migrations_async();

		[WaitingForFixMerge]
		public override void Can_revert_one_migrations()
			=> base.Can_revert_one_migrations();

		[WaitingForFixMerge]
		public override void Can_revert_all_migrations()
			=> base.Can_revert_all_migrations();

		[WaitingForFixMerge]
		public override void Can_generate_up_scripts()
			=> base.Can_generate_up_scripts();

		[WaitingForFixMerge]
		public override void Can_generate_up_script_using_names()
			=> base.Can_generate_up_script_using_names();

		[WaitingForFixMerge]
		public override void Can_generate_one_up_script()
			=> base.Can_generate_one_up_script();

		[WaitingForFixMerge]
		public override void Can_generate_one_down_script()
			=> base.Can_generate_one_down_script();

		[WaitingForFixMerge]
		public override void Can_generate_down_scripts()
			=> base.Can_generate_down_scripts();

		[WaitingForFixMerge]
		public override void Can_generate_down_script_using_names()
			=> base.Can_generate_down_script_using_names();

		public class WaitingForFixMergeAttribute : FactAttribute
		{
			public WaitingForFixMergeAttribute()
			{
				Skip = "Waiting for #14811 on EntityFrameworkCore to be merged.";
			}
		}
	}
}
