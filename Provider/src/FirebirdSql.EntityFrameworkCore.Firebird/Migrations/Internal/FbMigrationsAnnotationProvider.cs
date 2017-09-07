using System.Collections.Generic;
using FirebirdSql.EntityFrameworkCore.Firebird.Extensions;
using FirebirdSql.EntityFrameworkCore.Firebird.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Migrations.Internal
{
	public class FbMigrationsAnnotationProvider : MigrationsAnnotationProvider
	{
		public FbMigrationsAnnotationProvider(MigrationsAnnotationProviderDependencies dependencies)
			: base(dependencies)
		{ }

		public override IEnumerable<IAnnotation> For(IProperty property)
		{
			if (property.Firebird().ValueGenerationStrategy != null)
			{
				yield return new Annotation(FbAnnotationNames.ValueGenerationStrategy, property.Firebird().ValueGenerationStrategy);
			}
		}
	}
}
