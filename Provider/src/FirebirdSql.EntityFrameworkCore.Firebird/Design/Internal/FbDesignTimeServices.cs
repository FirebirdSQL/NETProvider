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

using FirebirdSql.EntityFrameworkCore.Firebird.Scaffolding.Internal;
using FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Design.Internal
{
	public class FbDesignTimeServices : IDesignTimeServices
	{
		public void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
			=> serviceCollection
				.AddSingleton<IRelationalTypeMapper, FbTypeMapper>()
				.AddSingleton<IDatabaseModelFactory, FbDatabaseModelFactory>()
				.AddSingleton<IScaffoldingProviderCodeGenerator, FbScaffoldingCodeGenerator>()
				.AddSingleton<IAnnotationCodeGenerator, AnnotationCodeGenerator>();
	}
}
