using System;
using System.Collections.Generic;
using System.Linq;
using FirebirdSql.EntityFrameworkCore.Firebird.Utilities;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal
{
	public class FbCompositeMemberTranslator : RelationalCompositeMemberTranslator
	{
		static readonly List<Type> Translators = TranslatorsHelper.GetTranslators<IMemberTranslator>().ToList();

		public FbCompositeMemberTranslator(RelationalCompositeMemberTranslatorDependencies dependencies)
			: base(dependencies)
		{
			AddTranslators(Translators.Select(t => (IMemberTranslator)Activator.CreateInstance(t)));
		}
	}
}
