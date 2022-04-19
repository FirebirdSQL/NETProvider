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
using FirebirdSql.EntityFrameworkCore.Firebird.Metadata;
using FirebirdSql.EntityFrameworkCore.Firebird.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore;

public static class FbPropertyExtensions
{
	public static FbValueGenerationStrategy GetValueGenerationStrategy(this IProperty property)
	{
		var annotation = property[FbAnnotationNames.ValueGenerationStrategy];
		if (annotation != null)
		{
			return (FbValueGenerationStrategy)annotation;
		}

		if (property.ValueGenerated != ValueGenerated.OnAdd
			|| property.IsForeignKey()
			|| property.TryGetDefaultValue(out _)
			|| property.GetDefaultValueSql() != null
			|| property.GetComputedColumnSql() != null)
		{
			return FbValueGenerationStrategy.None;
		}

		var modelStrategy = property.DeclaringEntityType.Model.GetValueGenerationStrategy();

		if (modelStrategy == FbValueGenerationStrategy.SequenceTrigger && IsCompatibleSequenceTrigger(property))
		{
			return FbValueGenerationStrategy.SequenceTrigger;
		}
		if (modelStrategy == FbValueGenerationStrategy.IdentityColumn && IsCompatibleIdentityColumn(property))
		{
			return FbValueGenerationStrategy.IdentityColumn;
		}

		return FbValueGenerationStrategy.None;
	}

	public static FbValueGenerationStrategy GetValueGenerationStrategy(this IMutableProperty property)
	{
		var annotation = property[FbAnnotationNames.ValueGenerationStrategy];
		if (annotation != null)
		{
			return (FbValueGenerationStrategy)annotation;
		}

		if (property.ValueGenerated != ValueGenerated.OnAdd
			|| property.IsForeignKey()
			|| property.TryGetDefaultValue(out _)
			|| property.GetDefaultValueSql() != null
			|| property.GetComputedColumnSql() != null)
		{
			return FbValueGenerationStrategy.None;
		}

		var modelStrategy = property.DeclaringEntityType.Model.GetValueGenerationStrategy();

		if (modelStrategy == FbValueGenerationStrategy.SequenceTrigger && IsCompatibleSequenceTrigger(property))
		{
			return FbValueGenerationStrategy.SequenceTrigger;
		}
		if (modelStrategy == FbValueGenerationStrategy.IdentityColumn && IsCompatibleIdentityColumn(property))
		{
			return FbValueGenerationStrategy.IdentityColumn;
		}

		return FbValueGenerationStrategy.None;
	}

	public static FbValueGenerationStrategy GetValueGenerationStrategy(this IConventionProperty property)
	{
		var annotation = property[FbAnnotationNames.ValueGenerationStrategy];
		if (annotation != null)
		{
			return (FbValueGenerationStrategy)annotation;
		}

		if (property.ValueGenerated != ValueGenerated.OnAdd
			|| property.IsForeignKey()
			|| property.TryGetDefaultValue(out _)
			|| property.GetDefaultValueSql() != null
			|| property.GetComputedColumnSql() != null)
		{
			return FbValueGenerationStrategy.None;
		}

		var modelStrategy = property.DeclaringEntityType.Model.GetValueGenerationStrategy();

		if (modelStrategy == FbValueGenerationStrategy.SequenceTrigger && IsCompatibleSequenceTrigger(property))
		{
			return FbValueGenerationStrategy.SequenceTrigger;
		}
		if (modelStrategy == FbValueGenerationStrategy.IdentityColumn && IsCompatibleIdentityColumn(property))
		{
			return FbValueGenerationStrategy.IdentityColumn;
		}

		return FbValueGenerationStrategy.None;
	}

	public static ConfigurationSource? GetValueGenerationStrategyConfigurationSource(this IConventionProperty property)
		=> property.FindAnnotation(FbAnnotationNames.ValueGenerationStrategy)?.GetConfigurationSource();

	public static void SetValueGenerationStrategy(this IMutableProperty property, FbValueGenerationStrategy? value)
	{
		CheckValueGenerationStrategy(property, value);
		property.SetOrRemoveAnnotation(FbAnnotationNames.ValueGenerationStrategy, value);
	}

	public static void SetValueGenerationStrategy(this IConventionProperty property, FbValueGenerationStrategy? value, bool fromDataAnnotation = false)
	{
		CheckValueGenerationStrategy(property, value);
		property.SetOrRemoveAnnotation(FbAnnotationNames.ValueGenerationStrategy, value, fromDataAnnotation);
	}

	static void CheckValueGenerationStrategy(IReadOnlyPropertyBase property, FbValueGenerationStrategy? value)
	{
		if (value != null)
		{
			if (value == FbValueGenerationStrategy.IdentityColumn && !IsCompatibleIdentityColumn(property))
			{
				throw new ArgumentException($"Incompatible data type for {nameof(FbValueGenerationStrategy.IdentityColumn)} for '{property.Name}'.");
			}
			if (value == FbValueGenerationStrategy.SequenceTrigger && !IsCompatibleSequenceTrigger(property))
			{
				throw new ArgumentException($"Incompatible data type for {nameof(FbValueGenerationStrategy.SequenceTrigger)} for '{property.Name}'.");
			}
		}
	}

	static bool IsCompatibleIdentityColumn(IReadOnlyPropertyBase property)
		=> property.ClrType.IsInteger() || property.ClrType == typeof(decimal);

	static bool IsCompatibleSequenceTrigger(IReadOnlyPropertyBase property)
		=> true;
}
