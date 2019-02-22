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
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Tests2
{
	public class MigrationsFbTest : MigrationsTestBase<MigrationsFbFixture>
	{
		public MigrationsFbTest(MigrationsFbFixture fixture)
			: base(fixture)
		{ }

		public override void Can_generate_idempotent_up_scripts()
		{
			Assert.Throws<NotSupportedException>(base.Can_generate_idempotent_up_scripts);
		}

		public override void Can_generate_idempotent_down_scripts()
		{
			Assert.Throws<NotSupportedException>(base.Can_generate_idempotent_down_scripts);
		}
	}
}
