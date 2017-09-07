using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Metadata.Internal
{
	public static class FbInternalMetadataBuilderExtensions
	{
		public static RelationalModelBuilderAnnotations Firebird(this InternalModelBuilder builder, ConfigurationSource configurationSource)
			   => new RelationalModelBuilderAnnotations(builder, configurationSource);

		public static RelationalPropertyBuilderAnnotations Firebird(this InternalPropertyBuilder builder, ConfigurationSource configurationSource)
			=> new RelationalPropertyBuilderAnnotations(builder, configurationSource);

		public static RelationalEntityTypeBuilderAnnotations Firebird(this InternalEntityTypeBuilder builder, ConfigurationSource configurationSource)
			=> new RelationalEntityTypeBuilderAnnotations(builder, configurationSource);

		public static RelationalKeyBuilderAnnotations Firebird(this InternalKeyBuilder builder, ConfigurationSource configurationSource)
			=> new RelationalKeyBuilderAnnotations(builder, configurationSource);

		public static RelationalIndexBuilderAnnotations Firebird(this InternalIndexBuilder builder, ConfigurationSource configurationSource)
			=> new RelationalIndexBuilderAnnotations(builder, configurationSource);

		public static RelationalForeignKeyBuilderAnnotations Firebird(this InternalRelationshipBuilder builder, ConfigurationSource configurationSource)
			=> new RelationalForeignKeyBuilderAnnotations(builder, configurationSource);
	}
}
