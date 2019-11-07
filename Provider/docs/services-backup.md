# Services - Backup

### Steps

* Install `FirebirdSql.Data.FirebirdClient` from NuGet.
* Add `using FirebirdSql.Data.Services;`.

### Code

```csharp
var backup = new FbBackup("database=localhost:demo.fdb;user=sysdba;password=masterkey");
backup.BackupFiles.Add(new FbBackupFile(@"C:\backup.fbk"));
//backup.Options = ...
backup.Verbose = true;
backup.ServiceOutput += (sender, e) => Console.WriteLine(e.Message);
backup.Execute();
```

### More
* `FbRestore`
* `FbStreamingBackup`
* `FbStreamingRestore`