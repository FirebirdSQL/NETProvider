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
		if (modelStrategy == FbValueGenerationStrategy.IdentityColumn)
		{
			if (property.DeclaringEntityType.GetMappingStrategy() == RelationalAnnotationNames.TpcMappingStrategy)
			{
				return FbValueGenerationStrategy.SequenceTrigger;
			}
			else if (IsCompatibleIdentityColumn(property))
			{
				return FbValueGenerationStrategy.IdentityColumn;
			}
		}
		if (modelStrategy == FbValueGenerationStrategy.HiLo && IsCompatibleHiLoColumn(property))
		{
			return FbValueGenerationStrategy.HiLo;
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
		if (modelStrategy == FbValueGenerationStrategy.IdentityColumn)
		{
			if (property.DeclaringEntityType.GetMappingStrategy() == RelationalAnnotationNames.TpcMappingStrategy)
			{
				return FbValueGenerationStrategy.SequenceTrigger;
			}
			else if (IsCompatibleIdentityColumn(property))
			{
				return FbValueGenerationStrategy.IdentityColumn;
			}
		}
		if (modelStrategy == FbValueGenerationStrategy.HiLo && IsCompatibleHiLoColumn(property))
		{
			return FbValueGenerationStrategy.HiLo;
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
		if (modelStrategy == FbValueGenerationStrategy.IdentityColumn)
		{
			if (property.DeclaringEntityType.GetMappingStrategy() == RelationalAnnotationNames.TpcMappingStrategy)
			{
				return FbValueGenerationStrategy.SequenceTrigger;
			}
			else if (IsCompatibleIdentityColumn(property))
			{
				return FbValueGenerationStrategy.IdentityColumn;
			}
		}
		if (modelStrategy == FbValueGenerationStrategy.HiLo && IsCompatibleHiLoColumn(property))
		{
			return FbValueGenerationStrategy.HiLo;
		}

		return FbValueGenerationStrategy.None;
	}

	public static ConfigurationSource? GetValueGenerationStrategyConfigurationSource(this IConventionProperty property)
	{
		return property.FindAnnotation(FbAnnotationNames.ValueGenerationStrategy)?.GetConfigurationSource();
	}

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

	public static string GetHiLoSequenceName(this IReadOnlyProperty property)
	{
		return (string)property[FbAnnotationNames.HiLoSequenceName];
	}

	public static string GetHiLoSequenceName(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
	{
		var annotation = property.FindAnnotation(FbAnnotationNames.HiLoSequenceName);
		if (annotation != null)
		{
			return (string)annotation.Value;
		}

		return property.FindSharedStoreObjectRootProperty(storeObject)?.GetHiLoSequenceName(storeObject);
	}

	public static void SetHiLoSequenceName(this IMutableProperty property, string name)
	{
		property.SetOrRemoveAnnotation(FbAnnotationNames.HiLoSequenceName, name);
	}

	public static string SetHiLoSequenceName(this IConventionProperty property, string name, bool fromDataAnnotation = false)
	{
		return (string)property.SetOrRemoveAnnotation(FbAnnotationNames.HiLoSequenceName, name, fromDataAnnotation)?.Value;
	}

	public static ConfigurationSource? GetHiLoSequenceNameConfigurationSource(this IConventionProperty property)
	{
		return property.FindAnnotation(FbAnnotationNames.HiLoSequenceName)?.GetConfigurationSource();
	}

	public static string GetHiLoSequenceSchema(this IReadOnlyProperty property)
	{
		return (string)property[FbAnnotationNames.HiLoSequenceSchema];
	}

	public static string GetHiLoSequenceSchema(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
	{
		var annotation = property.FindAnnotation(FbAnnotationNames.HiLoSequenceSchema);
		if (annotation != null)
		{
			return (string)annotation.Value;
		}

		return property.FindSharedStoreObjectRootProperty(storeObject)?.GetHiLoSequenceSchema(storeObject);
	}

	public static void SetHiLoSequenceSchema(this IMutableProperty property, string schema)
	{
		property.SetOrRemoveAnnotation(FbAnnotationNames.HiLoSequenceSchema, schema);
	}

	public static string SetHiLoSequenceSchema(this IConventionProperty property, string schema, bool fromDataAnnotation = false)
	{
		return (string)property.SetOrRemoveAnnotation(FbAnnotationNames.HiLoSequenceSchema, schema, fromDataAnnotation)?.Value;
	}

	public static ConfigurationSource? GetHiLoSequenceSchemaConfigurationSource(this IConventionProperty property)
	{
		return property.FindAnnotation(FbAnnotationNames.HiLoSequenceSchema)?.GetConfigurationSource();
	}

	public static IReadOnlySequence FindHiLoSequence(this IReadOnlyProperty property)
	{
		var model = property.DeclaringEntityType.Model;

		var sequenceName = property.GetHiLoSequenceName()
			?? model.GetHiLoSequenceName();

		var sequenceSchema = property.GetHiLoSequenceSchema()
			?? model.GetHiLoSequenceSchema();

		return model.FindSequence(sequenceName, sequenceSchema);
	}

	public static IReadOnlySequence FindHiLoSequence(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
	{
		var model = property.DeclaringEntityType.Model;

		var sequenceName = property.GetHiLoSequenceName(storeObject)
			?? model.GetHiLoSequenceName();

		var sequenceSchema = property.GetHiLoSequenceSchema(storeObject)
			?? model.GetHiLoSequenceSchema();

		return model.FindSequence(sequenceName, sequenceSchema);
	}

	public static ISequence FindHiLoSequence(this IProperty property)
	{
		return (ISequence)((IReadOnlyProperty)property).FindHiLoSequence();
	}

	public static ISequence FindHiLoSequence(this IProperty property, in StoreObjectIdentifier storeObject)
	{
		return (ISequence)((IReadOnlyProperty)property).FindHiLoSequence(storeObject);
	}

	public static string GetSequenceName(this IReadOnlyProperty property)
	{
		return (string)property[FbAnnotationNames.SequenceName];
	}

	public static string GetSequenceName(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
	{
		var annotation = property.FindAnnotation(FbAnnotationNames.SequenceName);
		if (annotation != null)
		{
			return (string)annotation.Value;
		}

		return property.FindSharedStoreObjectRootProperty(storeObject)?.GetSequenceName(storeObject);
	}

	public static void SetSequenceName(this IMutableProperty property, string name)
	{
		property.SetOrRemoveAnnotation(FbAnnotationNames.SequenceName, name);
	}

	public static string SetSequenceName(this IConventionProperty property, string name, bool fromDataAnnotation = false)
	{
		return (string)property.SetOrRemoveAnnotation(FbAnnotationNames.SequenceName, name, fromDataAnnotation)?.Value;
	}

	public static ConfigurationSource? GetSequenceNameConfigurationSource(this IConventionProperty property)
	{
		return property.FindAnnotation(FbAnnotationNames.SequenceName)?.GetConfigurationSource();
	}

	public static string GetSequenceSchema(this IReadOnlyProperty property)
	{
		return (string)property[FbAnnotationNames.SequenceSchema];
	}

	public static string GetSequenceSchema(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
	{
		var annotation = property.FindAnnotation(FbAnnotationNames.SequenceSchema);
		if (annotation != null)
		{
			return (string)annotation.Value;
		}

		return property.FindSharedStoreObjectRootProperty(storeObject)?.GetSequenceSchema(storeObject);
	}

	public static void SetSequenceSchema(this IMutableProperty property, string schema)
	{
		property.SetOrRemoveAnnotation(FbAnnotationNames.SequenceSchema, schema);
	}

	public static string SetSequenceSchema(this IConventionProperty property, string schema, bool fromDataAnnotation = false)
	{
		return (string)property.SetOrRemoveAnnotation(FbAnnotationNames.SequenceSchema, schema, fromDataAnnotation)?.Value;
	}

	public static ConfigurationSource? GetSequenceSchemaConfigurationSource(this IConventionProperty property)
	{
		return property.FindAnnotation(FbAnnotationNames.SequenceSchema)?.GetConfigurationSource();
	}

	public static IReadOnlySequence FindSequence(this IReadOnlyProperty property)
	{
		var model = property.DeclaringEntityType.Model;

		var sequenceName = property.GetSequenceName()
			?? model.GetSequenceNameSuffix();

		var sequenceSchema = property.GetSequenceSchema()
			?? model.GetSequenceSchema();

		return model.FindSequence(sequenceName, sequenceSchema);
	}

	public static IReadOnlySequence FindSequence(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
	{
		var model = property.DeclaringEntityType.Model;

		var sequenceName = property.GetSequenceName(storeObject)
			?? model.GetSequenceNameSuffix();

		var sequenceSchema = property.GetSequenceSchema(storeObject)
			?? model.GetSequenceSchema();

		return model.FindSequence(sequenceName, sequenceSchema);
	}

	public static ISequence FindSequence(this IProperty property)
	{
		return (ISequence)((IReadOnlyProperty)property).FindSequence();
	}

	public static ISequence FindSequence(this IProperty property, in StoreObjectIdentifier storeObject)
	{
		return (ISequence)((IReadOnlyProperty)property).FindSequence(storeObject);
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
			if (value == FbValueGenerationStrategy.HiLo && !IsCompatibleHiLoColumn(property))
			{
				throw new ArgumentException($"Incompatible data type for {nameof(FbValueGenerationStrategy.HiLo)} for '{property.Name}'.");
			}
		}
	}

	static bool IsCompatibleIdentityColumn(IReadOnlyPropertyBase property)
	{
		return property.ClrType.IsInteger() || property.ClrType == typeof(decimal);
	}

	static bool IsCompatibleSequenceTrigger(IReadOnlyPropertyBase property)
	{
		return true;
	}

	static bool IsCompatibleHiLoColumn(IReadOnlyPropertyBase property)
	{
		return property.ClrType.IsInteger() || property.ClrType == typeof(decimal);
	}
}
