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

//$Authors = Jiri Cincura (jiri@cincura.net), Jean Ressouche, Rafael Almeida (ralms@ralms.net)

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
