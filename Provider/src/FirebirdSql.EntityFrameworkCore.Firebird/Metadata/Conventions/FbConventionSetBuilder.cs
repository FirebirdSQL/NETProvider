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

using FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Metadata.Conventions
{
	public class FbConventionSetBuilder : RelationalConventionSetBuilder
	{
		public FbConventionSetBuilder(RelationalConventionSetBuilderDependencies dependencies)
			: base(dependencies)
		{ }

		public static ConventionSet Build()
		{
			var typeMapper = new FbTypeMapper(new RelationalTypeMapperDependencies());
			var dependencies = new RelationalConventionSetBuilderDependencies(typeMapper, null, null);
			return new FbConventionSetBuilder(dependencies)
				.AddConventions(new CoreConventionSetBuilder(new CoreConventionSetBuilderDependencies(typeMapper)).CreateConventionSet());
		}
	}
}
