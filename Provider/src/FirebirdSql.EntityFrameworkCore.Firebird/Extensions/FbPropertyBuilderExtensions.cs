using FirebirdSql.EntityFrameworkCore.Firebird.Metadata;
using FirebirdSql.EntityFrameworkCore.Firebird.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Extensions
{
	public static class FbPropertyBuilderExtensions
    {
		public static PropertyBuilder UseFirebirdIdentityColumn(this PropertyBuilder propertyBuilder)
		{
			GetFbInternalBuilder(propertyBuilder).ValueGenerationStrategy(FbValueGenerationStrategy.IdentityColumn);
			return propertyBuilder;
		}

		public static PropertyBuilder<TProperty> UseFirebirdIdentityColumn<TProperty>(this PropertyBuilder<TProperty> propertyBuilder)
			=> (PropertyBuilder<TProperty>)UseFirebirdIdentityColumn((PropertyBuilder)propertyBuilder);

		public static PropertyBuilder UseFirebirdSequenceTrigger(this PropertyBuilder propertyBuilder)
		{
			GetFbInternalBuilder(propertyBuilder).ValueGenerationStrategy(FbValueGenerationStrategy.SequenceTrigger);
			return propertyBuilder;
		}

		public static PropertyBuilder<TProperty> UseFirebirdSequenceTrigger<TProperty>(this PropertyBuilder<TProperty> propertyBuilder)
			=> (PropertyBuilder<TProperty>)UseFirebirdSequenceTrigger((PropertyBuilder)propertyBuilder);

		static FbPropertyBuilderAnnotations GetFbInternalBuilder(PropertyBuilder propertyBuilder)
			=> propertyBuilder.GetInfrastructure<InternalPropertyBuilder>().Firebird(ConfigurationSource.Explicit);
	}
}
