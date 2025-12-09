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
using System.Collections.Generic;
using System.Linq;
using FirebirdSql.EntityFrameworkCore.Firebird.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Helpers;

public static class ModelHelpers
{
	public static void SetStringLengths(ModelBuilder modelBuilder)
	{
		foreach (var entityType in modelBuilder.Model.GetEntityTypes())
		{
			HandleProperties(entityType.GetProperties());
			HandleComplexProperties(entityType.GetComplexProperties());
		}

		void HandleProperties(IEnumerable<IMutableProperty> properties)
		{
			foreach (var property in properties)
			{
				SetStringLength(property);
			}
		}
		void HandleComplexProperties(IEnumerable<IMutableComplexProperty> complexProperties)
		{
			foreach (var cp in complexProperties)
			{
				HandleProperties(cp.ComplexType.GetProperties());
				HandleComplexProperties(cp.ComplexType.GetComplexProperties());
			}
		}
		void SetStringLength(IMutableProperty property)
		{
			if (property.ClrType == typeof(string) && property.GetMaxLength() == null)
			{
				property.SetMaxLength(500);
			}
		}
	}

	public static void SimpleTableNames(ModelBuilder modelBuilder)
	{
		var names = new HashSet<string>(StringComparer.InvariantCulture);
		foreach (var entityType in modelBuilder.Model.GetEntityTypes())
		{
			if (entityType.BaseType != null)
				continue;
			entityType.SetTableName(Simplify(entityType.GetTableName()));
			foreach (var property in entityType.GetProperties())
			{
				property.SetColumnName(Simplify(property.Name));
			}
		}

		string Simplify(string name)
		{
			name = new string(name.Where(char.IsUpper).ToArray());
			var cnt = 1;
			while (names.Contains(name + cnt))
			{
				cnt++;
			}
			name += cnt;
			names.Add(name);
			return name;
		}
	}

	public static void SetPrimaryKeyGeneration(ModelBuilder modelBuilder, FbValueGenerationStrategy valueGenerationStrategy = FbValueGenerationStrategy.SequenceTrigger, Func<IMutableEntityType, bool> filter = null)
	{
		filter ??= _ => true;
		foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(filter))
		{
			var pk = entityType.FindPrimaryKey();
			if (pk == null)
				continue;
			var properties = pk.Properties;
			if (properties.Count() != 1)
				continue;
			var property = properties[0];
			if (property.GetValueGenerationStrategy() == FbValueGenerationStrategy.None)
			{
				property.SetValueGenerationStrategy(valueGenerationStrategy);
			}
		}
	}

	public static void ShortenMM(ModelBuilder modelBuilder)
	{
		foreach (var entityType in modelBuilder.Model.GetEntityTypes())
		{
			entityType.SetTableName(Shorten(entityType.ShortName()));
			foreach (var property in entityType.GetProperties())
			{
				property.SetColumnName(Shorten(property.Name));
			}
		}

		static string Shorten(string s)
		{
			return s
				.Replace("UnidirectionalEntity", "UE")
				.Replace("Unidirectional", "U")
				.Replace("JoinOneToThree", "J1_3")
				.Replace("EntityTableSharing", "ETS")
				.Replace("GeneratedKeys", "GK")
				.Replace("ImplicitManyToMany", "IMM");
		}
	}
}
