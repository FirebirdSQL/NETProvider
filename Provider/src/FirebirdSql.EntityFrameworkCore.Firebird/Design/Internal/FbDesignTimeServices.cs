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
