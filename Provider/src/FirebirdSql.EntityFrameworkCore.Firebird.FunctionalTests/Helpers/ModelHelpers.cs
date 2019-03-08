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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FirebirdSql.EntityFrameworkCore.Firebird.Metadata;
using Microsoft.EntityFrameworkCore;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Helpers
{
	public static class ModelHelpers
	{
		public static void SetStringLengths(ModelBuilder modelBuilder, DbContext context)
		{
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

		public static void SimpleTableNamesUnique(ModelBuilder modelBuilder, DbContext context)
		{
			var names = new HashSet<string>(StringComparer.InvariantCulture);
			foreach (var entityType in modelBuilder.Model.GetEntityTypes())
			{
				var name = new string(entityType.Relational().TableName.Where(char.IsUpper).ToArray());
				var cnt = 1;
				while (names.Contains(name))
				{
					name = name + cnt++;
				}
				names.Add(name);
				entityType.Relational().TableName = name;
			}
		}

		public static void SetPrimaryKeyGeneration(ModelBuilder modelBuilder, DbContext context, FbValueGenerationStrategy valueGenerationStrategy = FbValueGenerationStrategy.SequenceTrigger)
		{
			foreach (var entityType in modelBuilder.Model.GetEntityTypes())
			{
				var pk = entityType.FindPrimaryKey();
				if (pk == null)
					continue;
				var properties = pk.Properties;
				if (properties.Count() != 1)
					continue;
				var fbPropertyAnnotations = properties[0].Firebird();
				if (fbPropertyAnnotations.ValueGenerationStrategy == null)
				{
					properties[0].Firebird().ValueGenerationStrategy = valueGenerationStrategy;
				}
			}
		}
	}
}
