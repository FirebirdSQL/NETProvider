using FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Metadata.Conventions
{
	public class FbConventionSetBuilder : RelationalConventionSetBuilder
	{
		public FbConventionSetBuilder(RelationalConventionSetBuilderDependencies dependencies)
			: base(dependencies)
		{ }

		public static ConventionSet Build()
		{
			var typeMapper = new FbTypeMapper(new RelationalTypeMapperDependencies());
			var dependencies = new RelationalConventionSetBuilderDependencies(typeMapper, null, null);
			return new FbConventionSetBuilder(dependencies)
				.AddConventions(new CoreConventionSetBuilder(new CoreConventionSetBuilderDependencies(typeMapper)).CreateConventionSet());
		}
	}
}
