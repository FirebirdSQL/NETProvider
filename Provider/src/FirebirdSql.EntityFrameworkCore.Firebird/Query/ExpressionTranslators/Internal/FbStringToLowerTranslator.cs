using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal
{
	public class FbStringToLowerTranslator : ParameterlessInstanceMethodCallTranslator
	{
		public FbStringToLowerTranslator()
			: base(typeof(string), nameof(string.ToLower), "LOWER")
		{ }
	}
}
