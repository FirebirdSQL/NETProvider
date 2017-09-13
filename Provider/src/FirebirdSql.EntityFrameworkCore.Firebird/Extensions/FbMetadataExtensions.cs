using FirebirdSql.EntityFrameworkCore.Firebird.Metadata;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Extensions
{
	public static class FbMetadataExtensions
	{
		public static FbPropertyAnnotations Firebird(this IProperty property)
			=> new FbPropertyAnnotations(property);

		public static RelationalEntityTypeAnnotations Firebird(this IEntityType entityType)
			=> new RelationalEntityTypeAnnotations(entityType);

		public static RelationalKeyAnnotations Firebird(this IKey key)
			=> new RelationalKeyAnnotations(key);

		public static RelationalForeignKeyAnnotations Firebird(this IForeignKey foreignKey)
			=> new RelationalForeignKeyAnnotations(foreignKey);

		public static RelationalIndexAnnotations Firebird(this IIndex index)
			=> new RelationalIndexAnnotations(index);

		public static FbModelAnnotations Firebird(this IModel model)
			=> new FbModelAnnotations(model);
	}
}
