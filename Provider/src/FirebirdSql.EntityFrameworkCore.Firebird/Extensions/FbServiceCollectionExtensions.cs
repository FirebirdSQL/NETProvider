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

using FirebirdSql.EntityFrameworkCore.Firebird.Infrastructure.Internal;
using FirebirdSql.EntityFrameworkCore.Firebird.Internal;
using FirebirdSql.EntityFrameworkCore.Firebird.Metadata.Conventions;
using FirebirdSql.EntityFrameworkCore.Firebird.Migrations;
using FirebirdSql.EntityFrameworkCore.Firebird.Migrations.Internal;
using FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal;
using FirebirdSql.EntityFrameworkCore.Firebird.Query.Sql.Internal;
using FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal;
using FirebirdSql.EntityFrameworkCore.Firebird.Update.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.DependencyInjection;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Extensions
{
	public static class FbServiceCollectionExtensions
	{
		public static IServiceCollection AddEntityFrameworkFirebird(this IServiceCollection serviceCollection)
		{
			var builder = new EntityFrameworkRelationalServicesBuilder(serviceCollection)
				.TryAdd<IDatabaseProvider, DatabaseProvider<FbOptionsExtension>>()
				.TryAdd<IRelationalDatabaseCreator, FbDatabaseCreator>()
				.TryAdd<IRelationalTypeMapper, FbTypeMapper>()
				.TryAdd<ISqlGenerationHelper, FbSqlGenerationHelper>()
				.TryAdd<IMigrationsAnnotationProvider, FbMigrationsAnnotationProvider>()
				.TryAdd<IConventionSetBuilder, FbConventionSetBuilder>()
				.TryAdd<IUpdateSqlGenerator>(p => p.GetService<IFbUpdateSqlGenerator>())
				.TryAdd<IModificationCommandBatchFactory, FbModificationCommandBatchFactory>()
				.TryAdd<IRelationalConnection>(p => p.GetService<IFbConnection>())
				.TryAdd<IMigrationsSqlGenerator, FbMigrationsSqlGenerator>()
				.TryAdd<IHistoryRepository, FbHistoryRepository>()
				.TryAdd<IMemberTranslator, FbCompositeMemberTranslator>()
				.TryAdd<ICompositeMethodCallTranslator, FbCompositeMethodCallTranslator>()
				.TryAdd<IQuerySqlGeneratorFactory, FbQuerySqlGeneratorFactory>()
				.TryAdd<ISingletonOptions, IFbOptions>(p => p.GetService<IFbOptions>())
				.TryAddProviderSpecificServices(b => b
					.TryAddSingleton<IFbOptions, FbOptions>()
					.TryAddScoped<IFbUpdateSqlGenerator, FbUpdateSqlGenerator>()
					.TryAddScoped<IFbMigrationSqlGeneratorBehavior, FbMigrationSqlGeneratorBehavior>()
					.TryAddScoped<IFbConnection, FbConnection>());

			builder.TryAddCoreServices();

			return serviceCollection;
		}
	}
}
