using System;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal
{
	public class FbNewGuidTranslator : SingleOverloadStaticMethodCallTranslator
	{
		public FbNewGuidTranslator()
			: base(typeof(Guid), nameof(Guid.NewGuid), "GEN_UUID")
		{ }
	}
}
