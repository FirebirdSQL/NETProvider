using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal
{
	public class FbStringToUpperTranslator : ParameterlessInstanceMethodCallTranslator
	{
		public FbStringToUpperTranslator()
			: base(typeof(string), nameof(string.ToUpper), "UPPER")
		{ }
	}
}
