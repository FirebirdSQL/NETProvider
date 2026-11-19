# Remote event requeue overlapping the initial registration

This reproducer accompanies the fix for an internally initiated overlap in the managed provider. It is related to [issue #963](https://github.com/FirebirdSQL/NETProvider/issues/963), which reports `ArgumentOutOfRangeException: operation=<number>`. It does **not** establish that every report in that issue has this cause. In particular, this reproducer uses a separate connection for publishing events; it does not issue parallel application commands on one `FbConnection`.

## What overlaps

`FbRemoteEvent.QueueEvents` sends `op_que_events` on the event database's main connection and reads its response synchronously. After the first event arrives on the auxiliary event socket, `GdsEventManager.StartWaitingForEvents` calls `GdsDatabase.QueueEventsAsync` to register the next notification on **that same main protocol stream**. If the synchronous operation has not finished consuming its response, the asynchronous read can consume bytes belonging to the synchronous response. `GdsConnection.ProcessOperationAsync` then receives an invalid operation code and throws `ArgumentOutOfRangeException: operation=0` (the number depends on which bytes were consumed).

The two operations originate inside the provider even when the application uses `FbRemoteEvent` correctly. `CancelEvents` and `Detach` use the same stream and must not enter while an automatic requeue is in flight. The fix serializes those complete request/response exchanges per `GdsDatabase`, across sync and async variants. It does not change the general rule that application code must not perform concurrent commands on a single `FbConnection`.

## Controlled reproduction on the unfixed revision

The race is timing dependent. `operation-probe.patch` is a **test-only scheduling probe**, not part of the production fix. Apply it to a separate checkout of the unfixed revision. It pauses the synchronous response reader after obtaining the first opcode and lets the automatic asynchronous requeue read the following bytes. The probe is enabled only with `FB_EVENT_OPERATION_PROBE=1`. Never include this patch in a provider build intended for users.

Requirements: .NET 10 SDK, Firebird server with remote events enabled, and an isolated disposable database. We used Firebird 3.0.12 on `127.0.0.1:3051`. Set `FB_EVENT_REPRO_CONNECTION` to a connection string for that database. The runner disables pooling and uses separate connections for event registration and publishing. It posts only a named event and creates no tables.

```powershell
git clone https://github.com/FirebirdSQL/NETProvider.git NETProvider-unfixed
cd NETProvider-unfixed
git checkout 4385c0fdd6641877cf10bf9d3c81305428a743ef
git apply repros\RemoteEventRequeue\operation-probe.patch
$env:FB_EVENT_OPERATION_PROBE = '1'
$env:FB_EVENT_REPRO_CONNECTION = '<isolated Firebird connection string>'
dotnet build repros\RemoteEventRequeue\RemoteEventRequeueRepro.csproj -p:TargetFrameworks=net10.0 -p:GeneratePackageOnBuild=false
dotnet repros\RemoteEventRequeue\bin\Debug\net10.0\RemoteEventRequeueRepro.dll 1 10
```

Copy this repro directory into the `repros` folder of the unfixed checkout first, so its project reference points to that checkout. The target-framework override is needed if the local SDK cannot build the provider's other target frameworks.

In our run against revision `4385c0fd`, the first event was delivered and `RemoteEventError` reported:

```text
System.ArgumentOutOfRangeException: operation=0
  at GdsConnection.ProcessOperationAsync(...)
  at GdsDatabase.ReadSingleResponseAsync(...)
  at GdsDatabase.QueueEventsAsync(...)
  at GdsEventManager.StartWaitingForEvents(...)
```

The runner exited with an event error. This controlled probe establishes that overlapping response reads can produce the reported exception signature. It does not prove that an uninstrumented production occurrence had this exact timing.

## Validation of the fix

Without the probe, run the same program from this branch for a longer event/registration/cancellation stress run:

```powershell
$env:FB_EVENT_REPRO_CONNECTION = '<isolated Firebird connection string>'
dotnet build repros\RemoteEventRequeue\RemoteEventRequeueRepro.csproj -p:TargetFrameworks=net10.0 -p:GeneratePackageOnBuild=false
dotnet repros\RemoteEventRequeue\bin\Debug\net10.0\RemoteEventRequeueRepro.dll 1000 10
```

On the same Firebird 3.0.12 test database, the fixed provider completed 1,000 iterations with 2,029 events published, 957 received, and zero queue, event, or cleanup errors. The provider and test project compiled for .NET 10. The automated `FbRemoteEventTests.AutoRequeueDuringInitialRegistrationAndCancellationTest` exercises the same pattern in the normal integration suite. The local machine had no .NET 11 SDK and did not run the full suite or test against Firebird 2.5.
