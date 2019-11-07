# ADO.NET - Schema

### Steps

* Install `FirebirdSql.Data.FirebirdClient` from NuGet.
* Add `using FirebirdSql.Data.FirebirdClient;`.

### Code

```csharp
using (var connection = new FbConnection("database=localhost:demo.fdb;user=sysdba;password=masterkey"))
{
	connection.Open();

	var metadataCollections = connection.GetSchema();
	var dataTypes = connection.GetSchema(DbMetaDataCollectionNames.DataTypes);
	var dataSourceInformation = connection.GetSchema(DbMetaDataCollectionNames.DataSourceInformation);
	var reservedWords = connection.GetSchema(DbMetaDataCollectionNames.ReservedWords);
	var userTables = connection.GetSchema("Tables", new string[] { null, null, null, "TABLE" });
	var systemTables = connection.GetSchema("Tables", new string[] { null, null, null, "SYSTEM TABLE" });
	var tableColumns = connection.GetSchema("Columns", new string[] { null, null, "TableName" });
}
```
