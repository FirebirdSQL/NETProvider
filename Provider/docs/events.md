# Events

### Steps

* Install `FirebirdSql.Data.FirebirdClient` from NuGet.
* Add `using FirebirdSql.Data.FirebirdClient;`.

### Code

```csharp
using (var events = new FbRemoteEvent("database=localhost:demo.fdb;user=sysdba;password=masterkey"))
{
	events.RemoteEventCounts += (sender, e) => Console.WriteLine($"Event: {e.Name} | Counts: {e.Counts}");
	events.RemoteEventError += (sender, e) => Console.WriteLine($"ERROR: {e.Error}");
	events.QueueEvents("EVENT1", "EVENT2", "EVENT3", "EVENT4");
	Console.WriteLine("Listening...");
	Console.ReadLine();
}
```
