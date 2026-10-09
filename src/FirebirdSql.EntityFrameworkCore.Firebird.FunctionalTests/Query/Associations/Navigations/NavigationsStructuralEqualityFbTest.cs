using Microsoft.EntityFrameworkCore.Query.Associations.Navigations;
using Xunit.Abstractions;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query.Associations.Navigations;

public class NavigationsStructuralEqualityFbTest : NavigationsStructuralEqualityRelationalTestBase<NavigationsFbFixture>
{
	public NavigationsStructuralEqualityFbTest(NavigationsFbFixture fixture, ITestOutputHelper testOutputHelper) : base(fixture, testOutputHelper)
	{
		Fixture.TestSqlLoggerFactory.Clear();
		Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
	}
}
