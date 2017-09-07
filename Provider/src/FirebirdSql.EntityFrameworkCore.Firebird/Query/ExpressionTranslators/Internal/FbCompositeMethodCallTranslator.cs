using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal
{
	public class FbCompositeMethodCallTranslator : RelationalCompositeMethodCallTranslator
	{
		public FbCompositeMethodCallTranslator(RelationalCompositeMethodCallTranslatorDependencies dependencies)
			: base(dependencies)
		{
#warning Reflection?
			AddTranslators(new IMethodCallTranslator[]
			{
				new FbContainsOptimizedTranslator(),
				new FbConvertTranslator(),
				new FbDateAddTranslator(),
				new FbEndsWithOptimizedTranslator(),
				new FbMathTranslator(),
				new FbNewGuidTranslator(),
				new FbObjectToStringTranslator(),
				new FbStartsWithOptimizedTranslator(),
				new FbStringIsNullOrWhiteSpaceTranslator(),
				new FbStringReplaceTranslator(),
				new FbStringSubstringTranslator(),
				new FbStringToLowerTranslator(),
				new FbStringToUpperTranslator(),
				new FbStringTrimTranslator(),
			});
		}
	}
}
