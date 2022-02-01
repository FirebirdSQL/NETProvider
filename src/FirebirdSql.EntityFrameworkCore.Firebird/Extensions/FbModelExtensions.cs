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
}
