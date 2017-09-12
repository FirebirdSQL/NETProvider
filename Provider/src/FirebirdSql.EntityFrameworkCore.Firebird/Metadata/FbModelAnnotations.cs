using FirebirdSql.EntityFrameworkCore.Firebird.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Metadata
{
	public class FbModelAnnotations : RelationalModelAnnotations, IFbModelAnnotations
	{
		public FbModelAnnotations(IModel model)
			: base(model)
		{ }

		protected FbModelAnnotations(RelationalAnnotations annotations)
			: base(annotations)
		{ }

		public virtual FbValueGenerationStrategy? ValueGenerationStrategy
		{
			get => (FbValueGenerationStrategy?)Annotations.Metadata[FbAnnotationNames.ValueGenerationStrategy];
			set => Annotations.SetAnnotation(FbAnnotationNames.ValueGenerationStrategy, value);
		}
	}
}
