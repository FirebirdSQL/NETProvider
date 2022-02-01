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
using FirebirdSql.EntityFrameworkCore.Firebird.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Metadata.Conventions;

public class FbStoreGenerationConvention : StoreGenerationConvention
{
	public FbStoreGenerationConvention(ProviderConventionSetBuilderDependencies dependencies, RelationalConventionSetBuilderDependencies relationalDependencies)
		: base(dependencies, relationalDependencies)
	{ }

	public override void ProcessPropertyAnnotationChanged(IConventionPropertyBuilder propertyBuilder, string name, IConventionAnnotation annotation, IConventionAnnotation oldAnnotation, IConventionContext<IConventionAnnotation> context)
	{
		if (annotation == null
			|| oldAnnotation?.Value != null)
		{
			return;
		}

		var configurationSource = annotation.GetConfigurationSource();
		var fromDataAnnotation = configurationSource != ConfigurationSource.Convention;
		switch (name)
		{
			case RelationalAnnotationNames.DefaultValue:
				if (propertyBuilder.HasValueGenerationStrategy(null, fromDataAnnotation) == null
					&& propertyBuilder.HasDefaultValue(null, fromDataAnnotation) != null)
				{
					context.StopProcessing();
					return;
				}

				break;
			case RelationalAnnotationNames.DefaultValueSql:
				if (propertyBuilder.HasValueGenerationStrategy(null, fromDataAnnotation) == null
					&& propertyBuilder.HasDefaultValueSql(null, fromDataAnnotation) != null)
				{
					context.StopProcessing();
					return;
				}

				break;
			case RelationalAnnotationNames.ComputedColumnSql:
				if (propertyBuilder.HasValueGenerationStrategy(null, fromDataAnnotation) == null
					&& propertyBuilder.HasComputedColumnSql(null, fromDataAnnotation) != null)
				{
					context.StopProcessing();
					return;
				}

				break;
			case FbAnnotationNames.ValueGenerationStrategy:
				if ((propertyBuilder.HasDefaultValue(null, fromDataAnnotation) == null
					 | propertyBuilder.HasDefaultValueSql(null, fromDataAnnotation) == null
					 | propertyBuilder.HasComputedColumnSql(null, fromDataAnnotation) == null)
					&& propertyBuilder.HasValueGenerationStrategy(null, fromDataAnnotation) != null)
				{
					context.StopProcessing();
					return;
				}

				break;
		}

		base.ProcessPropertyAnnotationChanged(propertyBuilder, name, annotation, oldAnnotation, context);
	}

	protected override void Validate(IConventionProperty property, in StoreObjectIdentifier storeObject)
	{
		if (property.GetValueGenerationStrategyConfigurationSource() != null
			   && property.GetValueGenerationStrategy() != FbValueGenerationStrategy.None)
		{
			if (property.GetDefaultValue() != null)
			{
				throw new InvalidOperationException(
					RelationalStrings.ConflictingColumnServerGeneration(
						nameof(FbValueGenerationStrategy), property.Name, "DefaultValue"));
			}

			if (property.GetDefaultValueSql() != null)
			{
				throw new InvalidOperationException(
					RelationalStrings.ConflictingColumnServerGeneration(
						nameof(FbValueGenerationStrategy), property.Name, "DefaultValueSql"));
			}

			if (property.GetComputedColumnSql() != null)
			{
				throw new InvalidOperationException(
					RelationalStrings.ConflictingColumnServerGeneration(
						nameof(FbValueGenerationStrategy), property.Name, "ComputedColumnSql"));
			}
		}
		base.Validate(property, storeObject);
	}
}
