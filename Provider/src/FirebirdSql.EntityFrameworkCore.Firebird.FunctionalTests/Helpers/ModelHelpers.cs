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

using Microsoft.EntityFrameworkCore;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Helpers
{
	public static class ModelHelpers
	{
		public static void ConfigureModel(ModelBuilder modelBuilder, DbContext context)
		{
			// quick and dirty
			foreach (var entityType in modelBuilder.Model.GetEntityTypes())
			{
				foreach (var property in entityType.GetProperties())
				{
					if (property.ClrType == typeof(string) && property.GetMaxLength() == null)
					{
						property.SetMaxLength(500);
					}
				}
			}
		}
	}
}
