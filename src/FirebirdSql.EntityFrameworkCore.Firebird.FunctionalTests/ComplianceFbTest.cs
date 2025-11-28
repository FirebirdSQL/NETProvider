/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.Reflection;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests;

public class ComplianceFbTest : Microsoft.EntityFrameworkCore.RelationalComplianceTestBase
{
	protected override ICollection<Type> IgnoredTestBases { get; } =
	[
		typeof(Microsoft.EntityFrameworkCore.ApiConsistencyTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.BadDataJsonDeserializationTestBase),
		typeof(Microsoft.EntityFrameworkCore.BuiltInDataTypesTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.ComplexTypesTrackingRelationalTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.ComplexTypesTrackingTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.CompositeKeyEndToEndTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.ConcurrencyDetectorDisabledTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.ConcurrencyDetectorEnabledTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.ConcurrencyDetectorTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.ConferencePlannerTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.ConvertToProviderTypesTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.CustomConvertersTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.DataAnnotationTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.DataBindingTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.EntityFrameworkServiceCollectionExtensionsTestBase),
		typeof(Microsoft.EntityFrameworkCore.FieldMappingTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.FieldsOnlyLoadTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.FindTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.GraphUpdatesTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.ProxyGraphUpdatesTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.InterceptionTestBase),
		typeof(Microsoft.EntityFrameworkCore.JsonTypesTestBase),
		typeof(Microsoft.EntityFrameworkCore.KeysWithConvertersTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.LazyLoadProxyRelationalTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.LazyLoadProxyTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.LoadTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.LoggingTestBase),
		typeof(Microsoft.EntityFrameworkCore.ManyToManyFieldsLoadTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.ManyToManyLoadTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.ManyToManyTrackingTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.MaterializationInterceptionTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.ModelBuilding101TestBase),
		typeof(Microsoft.EntityFrameworkCore.MonsterFixupTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.MusicStoreTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.NotificationEntitiesTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.OptimisticConcurrencyTestBase<,>),
		typeof(Microsoft.EntityFrameworkCore.OverzealousInitializationTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.PropertyValuesRelationalTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.PropertyValuesTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.QueryExpressionInterceptionTestBase),
		typeof(Microsoft.EntityFrameworkCore.AdHocManyToManyQueryTestBase),
		typeof(Microsoft.EntityFrameworkCore.SaveChangesInterceptionTestBase),
		typeof(Microsoft.EntityFrameworkCore.SeedingTestBase),
		typeof(Microsoft.EntityFrameworkCore.SerializationTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.SingletonInterceptorsTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.SpatialTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.StoreGeneratedFixupTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.StoreGeneratedTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.ValueConvertersEndToEndTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.WithConstructorsTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.CommandInterceptionTestBase),
		typeof(Microsoft.EntityFrameworkCore.ConcurrencyDetectorDisabledRelationalTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.ConcurrencyDetectorEnabledRelationalTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.ConnectionInterceptionTestBase),
		typeof(Microsoft.EntityFrameworkCore.DataAnnotationRelationalTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.DesignTimeTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.EntitySplittingTestBase),
		typeof(Microsoft.EntityFrameworkCore.JsonTypesRelationalTestBase),
		typeof(Microsoft.EntityFrameworkCore.LoggingRelationalTestBase<,>),
		typeof(Microsoft.EntityFrameworkCore.ManyToManyTrackingRelationalTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.ModelBuilding101RelationalTestBase),
		typeof(Microsoft.EntityFrameworkCore.OptimisticConcurrencyRelationalTestBase<,>),
		typeof(Microsoft.EntityFrameworkCore.RelationalServiceCollectionExtensionsTestBase),
		typeof(Microsoft.EntityFrameworkCore.StoreGeneratedFixupRelationalTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.TableSplittingTestBase),
		typeof(Microsoft.EntityFrameworkCore.TPTTableSplittingTestBase),
		typeof(Microsoft.EntityFrameworkCore.TransactionInterceptionTestBase),
		typeof(Microsoft.EntityFrameworkCore.TransactionTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.TwoDatabasesTestBase),

		typeof(Microsoft.EntityFrameworkCore.ModelBuilding.ModelBuilderTest.ComplexCollectionTestBase),
		typeof(Microsoft.EntityFrameworkCore.ModelBuilding.ModelBuilderTest.ComplexTypeTestBase),
		typeof(Microsoft.EntityFrameworkCore.ModelBuilding.ModelBuilderTest.ModelBuilderTestBase),
		typeof(Microsoft.EntityFrameworkCore.ModelBuilding.ModelBuilderTest.InheritanceTestBase),
		typeof(Microsoft.EntityFrameworkCore.ModelBuilding.ModelBuilderTest.ManyToManyTestBase),
		typeof(Microsoft.EntityFrameworkCore.ModelBuilding.ModelBuilderTest.ManyToOneTestBase),
		typeof(Microsoft.EntityFrameworkCore.ModelBuilding.ModelBuilderTest.NonRelationshipTestBase),
		typeof(Microsoft.EntityFrameworkCore.ModelBuilding.ModelBuilderTest.OneToManyTestBase),
		typeof(Microsoft.EntityFrameworkCore.ModelBuilding.ModelBuilderTest.OneToOneTestBase),
		typeof(Microsoft.EntityFrameworkCore.ModelBuilding.ModelBuilderTest.OwnedTypesTestBase),
		typeof(Microsoft.EntityFrameworkCore.ModelBuilding.RelationalModelBuilderTest.RelationalNonRelationshipTestBase),
		typeof(Microsoft.EntityFrameworkCore.ModelBuilding.RelationalModelBuilderTest.RelationalComplexCollectionTestBase),
		typeof(Microsoft.EntityFrameworkCore.ModelBuilding.RelationalModelBuilderTest.RelationalComplexTypeTestBase),
		typeof(Microsoft.EntityFrameworkCore.ModelBuilding.RelationalModelBuilderTest.RelationalInheritanceTestBase),
		typeof(Microsoft.EntityFrameworkCore.ModelBuilding.RelationalModelBuilderTest.RelationalOneToManyTestBase),
		typeof(Microsoft.EntityFrameworkCore.ModelBuilding.RelationalModelBuilderTest.RelationalManyToOneTestBase),
		typeof(Microsoft.EntityFrameworkCore.ModelBuilding.RelationalModelBuilderTest.RelationalOneToOneTestBase),
		typeof(Microsoft.EntityFrameworkCore.ModelBuilding.RelationalModelBuilderTest.RelationalManyToManyTestBase),
		typeof(Microsoft.EntityFrameworkCore.ModelBuilding.RelationalModelBuilderTest.RelationalOwnedTypesTestBase),

		typeof(Microsoft.EntityFrameworkCore.BulkUpdates.BulkUpdatesTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.BulkUpdates.FiltersInheritanceBulkUpdatesTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.BulkUpdates.InheritanceBulkUpdatesTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.BulkUpdates.NonSharedModelBulkUpdatesTestBase),
		typeof(Microsoft.EntityFrameworkCore.BulkUpdates.NorthwindBulkUpdatesTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.BulkUpdates.FiltersInheritanceBulkUpdatesRelationalTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.BulkUpdates.InheritanceBulkUpdatesRelationalTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.BulkUpdates.NonSharedModelBulkUpdatesRelationalTestBase),
		typeof(Microsoft.EntityFrameworkCore.BulkUpdates.NorthwindBulkUpdatesRelationalTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.BulkUpdates.TPCFiltersInheritanceBulkUpdatesTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.BulkUpdates.TPCInheritanceBulkUpdatesTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.BulkUpdates.TPHInheritanceBulkUpdatesTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.BulkUpdates.TPTFiltersInheritanceBulkUpdatesTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.BulkUpdates.TPTInheritanceBulkUpdatesTestBase<>),

		typeof(Microsoft.EntityFrameworkCore.Update.ComplexCollectionJsonUpdateTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.Update.JsonUpdateTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.Update.NonSharedModelUpdatesTestBase),
		typeof(Microsoft.EntityFrameworkCore.Update.StoredProcedureUpdateTestBase),
		typeof(Microsoft.EntityFrameworkCore.Update.StoreValueGenerationTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.Update.UpdateSqlGeneratorTestBase),

		typeof(Microsoft.EntityFrameworkCore.Scaffolding.CompiledModelTestBase),
		typeof(Microsoft.EntityFrameworkCore.Scaffolding.CompiledModelRelationalTestBase),

		typeof(Microsoft.EntityFrameworkCore.Migrations.MigrationsInfrastructureTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.Migrations.MigrationsSqlGeneratorTestBase),

		// JSON not supported on FB
		typeof(Microsoft.EntityFrameworkCore.Query.JsonQueryTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.Query.JsonQueryRelationalTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.Query.AdHocJsonQueryRelationalTestBase),
		typeof(Microsoft.EntityFrameworkCore.Query.AdHocJsonQueryTestBase),
		typeof(Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson.ComplexJsonBulkUpdateRelationalTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson.ComplexJsonCollectionRelationalTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson.ComplexJsonMiscellaneousRelationalTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson.ComplexJsonPrimitiveCollectionRelationalTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson.ComplexJsonProjectionRelationalTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson.ComplexJsonSetOperationsRelationalTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson.ComplexJsonStructuralEqualityRelationalTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.Query.Associations.OwnedJson.OwnedJsonBulkUpdateRelationalTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.Query.Associations.OwnedJson.OwnedJsonCollectionRelationalTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.Query.Associations.OwnedJson.OwnedJsonMiscellaneousRelationalTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.Query.Associations.OwnedJson.OwnedJsonPrimitiveCollectionRelationalTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.Query.Associations.OwnedJson.OwnedJsonProjectionRelationalTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.Query.Associations.OwnedJson.OwnedJsonStructuralEqualityRelationalTestBase<>),

		// Spatial not supported on FB
		typeof(Microsoft.EntityFrameworkCore.Query.SpatialQueryTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.Query.SpatialQueryRelationalTestBase<>),

		// Uses some JSON
		typeof(Microsoft.EntityFrameworkCore.Query.AdHocPrecompiledQueryRelationalTestBase),
		typeof(Microsoft.EntityFrameworkCore.Query.PrecompiledQueryRelationalTestBase),
		typeof(Microsoft.EntityFrameworkCore.Query.PrecompiledSqlPregenerationQueryRelationalTestBase),

		// Tests for JSON Types in queries
		typeof(Microsoft.EntityFrameworkCore.Types.RelationalTypeTestBase<,>),
		typeof(Microsoft.EntityFrameworkCore.Types.TypeTestBase<,>),

		// Uses Complex Types with collections (JSON Arrays)
		typeof(Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties.ComplexPropertiesPrimitiveCollectionTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties.ComplexPropertiesCollectionTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties.ComplexPropertiesSetOperationsTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.Query.Associations.ComplexTableSplitting.ComplexTableSplittingPrimitiveCollectionRelationalTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations.OwnedNavigationsPrimitiveCollectionTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations.OwnedNavigationsPrimitiveCollectionRelationalTestBase<>),
		typeof(Microsoft.EntityFrameworkCore.Query.Associations.OwnedTableSplitting.OwnedTableSplittingPrimitiveCollectionRelationalTestBase<>),
	];

	protected override Assembly TargetAssembly { get; } = typeof(ComplianceFbTest).Assembly;
}
