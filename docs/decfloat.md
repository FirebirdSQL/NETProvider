# DECFLOAT datatype

`DECFLOAT(16)` and `DECFLOAT(34)` from Firebird 4 is handled by `FbDecFloat` type. `FbDecFloat` can be used as parameter value for `FbParameter`.

### Values are never rounded silently

`DECFLOAT(16)` holds 16 significant digits and `DECFLOAT(34)` holds 34. A value that cannot be represented exactly in the precision or exponent range of the target (for example a 17 digit value passed to a `DECFLOAT(16)` parameter) throws `OverflowException`. Values that only need trailing zeros removed or the exponent adjusted (e.g. `1E384` in `DECFLOAT(16)`) are accepted.

### Other .NET types

Parameters: besides `FbDecFloat`, a `DECFLOAT` parameter accepts `decimal`, `double`, `float`, the integral types and `BigInteger`:

* `decimal` is converted exactly, including its scale (`1.10m` stays `1.10`).
* `double` and `float` are converted to the shortest decimal that converts back to the same binary value (`0.1` is `0.1`, `0.1 + 0.2` is `0.30000000000000004`), which may have up to 17 significant digits.

Reading a `DECFLOAT` column:

* `GetDecimal` is exact; it throws `OverflowException` when the value cannot be represented exactly as `decimal` (too large, too many digits, NaN or infinity).
* `GetDouble` and `GetFloat` return the nearest value (NaN and infinities are mapped).
* `GetInt16`, `GetInt32`, `GetInt64` and `GetByte` round to the nearest integer, ties to even, and throw `OverflowException` when out of range.

`FbDecFloat` has explicit conversions from and to `decimal`, `double` and `float`, with the same rules.

### Examples

Examples can be found in [`FbDecFloat16SupportTests`](../src/FirebirdSql.Data.FirebirdClient.Tests/FbDecFloat16SupportTests.cs) and [`FbDecFloat34SupportTests`](../src/FirebirdSql.Data.FirebirdClient.Tests/FbDecFloat34SupportTests.cs).
