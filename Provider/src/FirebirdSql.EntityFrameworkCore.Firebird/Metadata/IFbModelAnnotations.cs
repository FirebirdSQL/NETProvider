using Microsoft.EntityFrameworkCore.Metadata;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Metadata
{
	public interface IFbModelAnnotations : IRelationalModelAnnotations
	{
		FbValueGenerationStrategy? ValueGenerationStrategy { get; }
	}
}
