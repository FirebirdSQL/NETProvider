using FirebirdSql.EntityFrameworkCore.Firebird.Extensions;
using Microsoft.EntityFrameworkCore.Scaffolding;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Scaffolding.Internal
{
	public class FbScaffoldingCodeGenerator: IScaffoldingProviderCodeGenerator
	{
		public virtual string GenerateUseProvider(string connectionString, string language)
		   => language == "CSharp"
			   ? $".{nameof(FbDbContextOptionsExtensions.UseFirebird)}({GenerateVerbatimStringLiteral(connectionString)})"
			   : null;

		static string GenerateVerbatimStringLiteral(string value) => "@\"" + value.Replace("\"", "\"\"") + "\"";
	}
}
