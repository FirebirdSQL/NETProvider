using FirebirdSql.EntityFrameworkCore.Firebird.Metadata;
using Microsoft.EntityFrameworkCore;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Extensions
{
	public static class FbModelBuilderExtensions
	{
		public static ModelBuilder ForFirebirdUseIdentityColumns(this ModelBuilder modelBuilder)
		{
			var property = modelBuilder.Model;
			property.Firebird().ValueGenerationStrategy = FbValueGenerationStrategy.IdentityColumn;
			return modelBuilder;
		}

		public static ModelBuilder ForFirebirdUseSequenceTriggers(this ModelBuilder modelBuilder)
		{
			var property = modelBuilder.Model;
			property.Firebird().ValueGenerationStrategy = FbValueGenerationStrategy.SequenceTrigger;
			return modelBuilder;
		}
	}
}
