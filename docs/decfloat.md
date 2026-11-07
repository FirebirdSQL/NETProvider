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

### .NET 11 `Decimal64` and `Decimal128`

On .NET 11 or later, the IEEE 754 decimal types `System.Numerics.Decimal64` and `System.Numerics.Decimal128` match `DECFLOAT(16)` and `DECFLOAT(34)`. Conversions are exact, including the exponent (`1.10` stays `1.10`), negative zero, infinities and NaNs.

* Parameters: a `Decimal128` value is a `FbDbType.Dec34` parameter, a `Decimal64` or `Decimal32` value a `FbDbType.Dec16` parameter. They can also be passed to other numeric parameters. A `Decimal128` value passed to a `DECFLOAT(16)` parameter works when it fits exactly, otherwise it throws `OverflowException`.
* Reader: `GetFieldValue<Decimal128>()` and `GetFieldValue<Decimal64>()` read `DECFLOAT` and the other numeric types; they throw `OverflowException` when the value does not fit exactly (e.g. a 34 digit `DECFLOAT(34)` value read as `Decimal64`).
* `FbDecFloat` has explicit conversions from `Decimal32`, `Decimal64` and `Decimal128`, and to `Decimal64` and `Decimal128`.

```csharp
command.CommandText = "insert into t (d34) values (@d34)";
command.Parameters.AddWithValue("d34", Decimal128.Parse("1234567890123456789012345678.901234", CultureInfo.InvariantCulture));
...
var value = reader.GetFieldValue<Decimal128>(0);
```

### Examples

Examples can be found in [`FbDecFloat16SupportTests`](../src/FirebirdSql.Data.FirebirdClient.Tests/FbDecFloat16SupportTests.cs), [`FbDecFloat34SupportTests`](../src/FirebirdSql.Data.FirebirdClient.Tests/FbDecFloat34SupportTests.cs) and [`FbDecimalIeee754SupportTests`](../src/FirebirdSql.Data.FirebirdClient.Tests/FbDecimalIeee754SupportTests.cs).
