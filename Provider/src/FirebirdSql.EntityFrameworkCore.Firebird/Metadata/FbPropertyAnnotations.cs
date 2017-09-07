using Microsoft.EntityFrameworkCore.Metadata;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Metadata
{
	public class FbPropertyAnnotations : RelationalPropertyAnnotations, IFbPropertyAnnotations
	{
		public FbPropertyAnnotations(IProperty property)
			: base(property)
		{ }

		protected FbPropertyAnnotations(RelationalAnnotations annotations)
			: base(annotations)
		{ }

		public virtual FbValueGenerationStrategy? ValueGenerationStrategy
		{
			get => GetValueGenerationStrategy(fallbackToModel: true);
			set => SetValueGenerationStrategy(value);
		}

		public virtual FbValueGenerationStrategy? GetValueGenerationStrategy(bool fallbackToModel)
		{
#warning Finish
			return default(FbValueGenerationStrategy?);
		}

		protected virtual bool SetValueGenerationStrategy(FbValueGenerationStrategy? value)
		{
#warning Finish
			return default(bool);
		}
	}
}
