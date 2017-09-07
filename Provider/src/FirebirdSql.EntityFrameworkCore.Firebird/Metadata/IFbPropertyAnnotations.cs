using Microsoft.EntityFrameworkCore.Metadata;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Metadata
{
	public interface IFbPropertyAnnotations : IRelationalPropertyAnnotations
	{
		FbValueGenerationStrategy? ValueGenerationStrategy { get; }
	}
}