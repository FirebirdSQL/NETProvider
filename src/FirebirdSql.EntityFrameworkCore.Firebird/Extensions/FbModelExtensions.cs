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

using FirebirdSql.EntityFrameworkCore.Firebird.Metadata;
using FirebirdSql.EntityFrameworkCore.Firebird.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore;

public static class FbModelExtensions
{
	public const string DefaultHiLoSequenceName = "EntityFrameworkHiLoSequence";
	public const string DefaultSequenceNameSuffix = "Sequence";

	public static void SetValueGenerationStrategy(this IMutableModel model, FbValueGenerationStrategy? value)
		=> model.SetOrRemoveAnnotation(FbAnnotationNames.ValueGenerationStrategy, value);

	public static void SetValueGenerationStrategy(this IConventionModel model, FbValueGenerationStrategy? value, bool fromDataAnnotation = false)
		=> model.SetOrRemoveAnnotation(FbAnnotationNames.ValueGenerationStrategy, value, fromDataAnnotation);

	public static FbValueGenerationStrategy? GetValueGenerationStrategy(this IModel model)
		=> (FbValueGenerationStrategy?)model[FbAnnotationNames.ValueGenerationStrategy];

	public static FbValueGenerationStrategy? GetValueGenerationStrategy(this IMutableModel model)
		=> (FbValueGenerationStrategy?)model[FbAnnotationNames.ValueGenerationStrategy];

	public static FbValueGenerationStrategy? GetValueGenerationStrategy(this IConventionModel model)
		=> (FbValueGenerationStrategy?)model[FbAnnotationNames.ValueGenerationStrategy];

	public static string GetHiLoSequenceName(this IReadOnlyModel model)
		=> (string)model[FbAnnotationNames.HiLoSequenceName] ?? DefaultHiLoSequenceName;

	public static void SetHiLoSequenceName(this IMutableModel model, string name)
		=> model.SetOrRemoveAnnotation(FbAnnotationNames.HiLoSequenceName, name);

	public static string SetHiLoSequenceName(this IConventionModel model, string name, bool fromDataAnnotation = false)
		=> (string)model.SetOrRemoveAnnotation(FbAnnotationNames.HiLoSequenceName, name, fromDataAnnotation)?.Value;

	public static ConfigurationSource? GetHiLoSequenceNameConfigurationSource(this IConventionModel model)
		=> model.FindAnnotation(FbAnnotationNames.HiLoSequenceName)?.GetConfigurationSource();

	public static string GetHiLoSequenceSchema(this IReadOnlyModel model)
		=> (string)model[FbAnnotationNames.HiLoSequenceSchema];

	public static void SetHiLoSequenceSchema(this IMutableModel model, string value)
		=> model.SetOrRemoveAnnotation(FbAnnotationNames.HiLoSequenceSchema, value);

	public static string SetHiLoSequenceSchema(this IConventionModel model, string value, bool fromDataAnnotation = false)
		=> (string)model.SetOrRemoveAnnotation(FbAnnotationNames.HiLoSequenceSchema, value, fromDataAnnotation)?.Value;

	public static ConfigurationSource? GetHiLoSequenceSchemaConfigurationSource(this IConventionModel model)
		=> model.FindAnnotation(FbAnnotationNames.HiLoSequenceSchema)?.GetConfigurationSource();

	public static string GetSequenceNameSuffix(this IReadOnlyModel model)
		=> (string)model[FbAnnotationNames.SequenceNameSuffix] ?? DefaultSequenceNameSuffix;

	public static void SetSequenceNameSuffix(this IMutableModel model, string name)
		=> model.SetOrRemoveAnnotation(FbAnnotationNames.SequenceNameSuffix, name);

	public static string SetSequenceNameSuffix(this IConventionModel model, string name, bool fromDataAnnotation = false)
		=> (string)model.SetOrRemoveAnnotation(FbAnnotationNames.SequenceNameSuffix, name, fromDataAnnotation)?.Value;

	public static ConfigurationSource? GetSequenceNameSuffixConfigurationSource(this IConventionModel model)
		=> model.FindAnnotation(FbAnnotationNames.SequenceNameSuffix)?.GetConfigurationSource();

	public static string GetSequenceSchema(this IReadOnlyModel model)
		=> (string)model[FbAnnotationNames.SequenceSchema];

	public static void SetSequenceSchema(this IMutableModel model, string value)
		=> model.SetOrRemoveAnnotation(FbAnnotationNames.SequenceSchema, value);

	public static string SetSequenceSchema(this IConventionModel model, string value, bool fromDataAnnotation = false)
		=> (string)model.SetOrRemoveAnnotation(FbAnnotationNames.SequenceSchema, value, fromDataAnnotation)?.Value;

	public static ConfigurationSource? GetSequenceSchemaConfigurationSource(this IConventionModel model)
		=> model.FindAnnotation(FbAnnotationNames.SequenceSchema)?.GetConfigurationSource();
}
