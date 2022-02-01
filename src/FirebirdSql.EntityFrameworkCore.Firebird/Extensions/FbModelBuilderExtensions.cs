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
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore;

public static class FbModelBuilderExtensions
{
	public static ModelBuilder UseIdentityColumns(this ModelBuilder modelBuilder)
	{
		var model = modelBuilder.Model;
		model.SetValueGenerationStrategy(FbValueGenerationStrategy.IdentityColumn);
		return modelBuilder;
	}

	public static ModelBuilder UseSequenceTriggers(this ModelBuilder modelBuilder)
	{
		var model = modelBuilder.Model;
		model.SetValueGenerationStrategy(FbValueGenerationStrategy.SequenceTrigger);
		return modelBuilder;
	}

	public static IConventionModelBuilder HasValueGenerationStrategy(this IConventionModelBuilder modelBuilder, FbValueGenerationStrategy? valueGenerationStrategy, bool fromDataAnnotation = false)
	{
		if (modelBuilder.CanSetAnnotation(FbAnnotationNames.ValueGenerationStrategy, valueGenerationStrategy, fromDataAnnotation))
		{
			modelBuilder.Metadata.SetValueGenerationStrategy(valueGenerationStrategy, fromDataAnnotation);
			if (valueGenerationStrategy != FbValueGenerationStrategy.IdentityColumn)
			{
			}
			if (valueGenerationStrategy != FbValueGenerationStrategy.SequenceTrigger)
			{
			}
			return modelBuilder;
		}
		return null;
	}
}
