/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.Sql.Internal
{
	public class FbQueryCompilationContextFactory : RelationalQueryCompilationContextFactory
	{
		public FbQueryCompilationContextFactory(QueryCompilationContextDependencies dependencies, RelationalQueryCompilationContextDependencies relationalDependencies)
			: base(dependencies, relationalDependencies)
		{ }

		public override QueryCompilationContext Create(bool async)
			=> async
				? new FbQueryCompilationContext(
					Dependencies,
					new AsyncLinqOperatorProvider(),
					new AsyncQueryMethodProvider(),
					TrackQueryResults)
				: new FbQueryCompilationContext(
					Dependencies,
					new LinqOperatorProvider(),
					new QueryMethodProvider(),
					TrackQueryResults);
	}
}
