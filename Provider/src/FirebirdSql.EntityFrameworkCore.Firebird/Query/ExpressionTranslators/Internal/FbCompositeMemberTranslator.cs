using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal
{
	public class FbCompositeMemberTranslator : RelationalCompositeMemberTranslator
	{
		public FbCompositeMemberTranslator(RelationalCompositeMemberTranslatorDependencies dependencies)
			: base(dependencies)
		{
#warning Reflection?
			AddTranslators(new IMemberTranslator[]
			{
				new FbStringLengthTranslator(),
				new FbDateTimeNowTranslator(),
				new FbDateTimeDateComponentTranslator(),
				new FbDateTimeDatePartComponentTranslator(),
			});
		}
	}
}
