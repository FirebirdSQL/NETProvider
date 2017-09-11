using System;
using System.Collections.Generic;
using System.Linq;
using FirebirdSql.EntityFrameworkCore.Firebird.Utilities;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal
{
	public class FbCompositeMethodCallTranslator : RelationalCompositeMethodCallTranslator
	{
		static readonly List<Type> Translators = TranslatorsHelper.GetTranslators<IMethodCallTranslator>().ToList();

		public FbCompositeMethodCallTranslator(RelationalCompositeMethodCallTranslatorDependencies dependencies)
			: base(dependencies)
		{
			AddTranslators(Translators.Select(t => (IMethodCallTranslator)Activator.CreateInstance(t)));
		}
	}
}
