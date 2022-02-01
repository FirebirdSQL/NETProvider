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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Metadata.Conventions;

public class FbValueGenerationStrategyConvention : IModelInitializedConvention, IModelFinalizingConvention
{
	public FbValueGenerationStrategyConvention(ProviderConventionSetBuilderDependencies dependencies, RelationalConventionSetBuilderDependencies relationalDependencies)
	{
		Dependencies = dependencies;
	}

	protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

	public virtual void ProcessModelInitialized(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
	{
		modelBuilder.HasValueGenerationStrategy(FbValueGenerationStrategy.IdentityColumn);
	}

	public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
	{
		foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
		{
			foreach (var property in entityType.GetDeclaredProperties())
			{
				// Needed for the annotation to show up in the model snapshot
				var strategy = property.GetValueGenerationStrategy();
				if (strategy != FbValueGenerationStrategy.None)
				{
					property.Builder.HasValueGenerationStrategy(strategy);
				}
			}
		}
	}
}
