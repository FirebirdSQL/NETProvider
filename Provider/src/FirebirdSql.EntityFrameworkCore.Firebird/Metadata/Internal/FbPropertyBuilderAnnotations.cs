using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Metadata.Internal
{
	public class FbPropertyBuilderAnnotations : FbPropertyAnnotations
	{
		public FbPropertyBuilderAnnotations(InternalPropertyBuilder internalBuilder, ConfigurationSource configurationSource)
			: base(new RelationalAnnotationsBuilder(internalBuilder, configurationSource))
		{ }

		public new virtual bool ColumnName(string value)
			=> SetColumnName(value);

		public new virtual bool ColumnType(string value)
			=> SetColumnType(value);

		public new virtual bool DefaultValueSql(string value)
			=> SetDefaultValueSql(value);

		public new virtual bool ComputedColumnSql(string value)
			=> SetComputedColumnSql(value);

		public new virtual bool DefaultValue(object value)
			=> SetDefaultValue(value);

		public new virtual bool ValueGenerationStrategy(FbValueGenerationStrategy? value)
		{
			if (!SetValueGenerationStrategy(value))
			{
				return false;
			}

			return true;
		}
	}
}
