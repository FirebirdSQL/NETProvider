# Batching

Batching is supported for Firebird 4 (and above). The work is handled by `FbBatchCommand` class. It has similar API surface as `FbCommand`. The usage should feel familiar.

### Specifics

Calling the `ExecuteNonQuery`/`ExecuteNonQueryAsync` does not throw an exception, should the exception happen on server while processing the data. Instead the returned `FbBatchNonQueryResult` object should be used to check the status. The `EnsureSuccess` method or `AllSuccess` property can be used for global check. Further enumeration gives detailed information.

Properties `MultiError`, `ReturnRecordsAffected` and `BatchBufferSize` allow for behavior fine-tuning (these represent `TAG_MULTIERROR`, `TAG_RECORD_COUNTS` and `TAG_BUFFER_BYTES_SIZE` in BPB).

When dealing with huge batches of possible unlimited size, it's good to use `ComputeCurrentBatchSize`/`ComputeCurrentBatchSizeAsync` to make sure the batch is not over `BatchBufferSize`. However calling `ComputeCurrentBatchSize`/`ComputeCurrentBatchSizeAsync` is not cheap and should be handled accordingly.    

### Limitations

At the moment batching is not supported for Firebird Embedded. The progress is tracked [here](https://github.com/FirebirdSQL/NETProvider/issues/1022).

Using (real) blobs as values is not supported. Regular `byte[]`, `string`, etc. values and the implicit conversions work just fine. The progress is tracked [here](https://github.com/FirebirdSQL/NETProvider/issues/1038). 

### Examples

Examples can be found in [`FbBatchCommandTests`](../src/FirebirdSql.Data.FirebirdClient.Tests/FbBatchCommandTests.cs).
