# Time zones

Time zones from Firebird 4 are handled by `FbZonedDateTime` and `FbZonedTime` types respectively. Given the lack of proper support for time zones in .NET (especially cross platform), these types provide the building blocks for developer to work with time zones using some library (i.e. _NodaTime_). Both `FbZonedDateTime` and `FbZonedTime` can be used as parameter value for `FbParameter`.

### Examples

Examples can be found in [`FbTimeZonesSupportTests`](../src/FirebirdSql.Data.FirebirdClient.Tests/FbTimeZonesSupportTests.cs).
