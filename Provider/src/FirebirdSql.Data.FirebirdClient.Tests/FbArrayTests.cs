/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Default))]
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Embedded))]
	public class FbArrayTests : FbTestsBase
	{
		public FbArrayTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
			: base(serverType, compression, wireCrypt)
		{ }

		[Test]
		public async Task IntegerArrayTest()
		{
			var id_value = GetId();
			var insert_values = new int[] { 10, 20, 30, 40 };

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var insert = new FbCommand("INSERT INTO TEST (int_field, iarray_field) values(@int_field, @array_field)", Connection, transaction))
				{
					insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
					insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
					await insert.ExecuteNonQueryAsync();
				}
				await transaction.CommitAsync();
			}

			await using (var select = new FbCommand($"SELECT iarray_field FROM TEST WHERE int_field = {id_value}", Connection))
			{
				await using (var reader = await select.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						if (!await reader.IsDBNullAsync(0))
						{
							var select_values = new int[insert_values.Length];
							Array.Copy((Array)reader.GetValue(0), select_values, select_values.Length);
							CollectionAssert.AreEqual(insert_values, select_values);
						}
					}
				}
			}
		}

		[Test]
		public async Task ShortArrayTest()
		{
			var id_value = GetId();
			var insert_values = new short[] { 50, 60, 70, 80 };

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var insert = new FbCommand("INSERT INTO TEST (int_field, sarray_field) values(@int_field, @array_field)", Connection, transaction))
				{
					insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
					insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
					await insert.ExecuteNonQueryAsync();
				}

				await transaction.CommitAsync();
			}

			await using (var select = new FbCommand($"SELECT sarray_field FROM TEST WHERE int_field = {id_value}", Connection))
			{
				await using (var reader = await select.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						if (!await reader.IsDBNullAsync(0))
						{
							var select_values = new short[insert_values.Length];
							Array.Copy((Array)reader.GetValue(0), select_values, select_values.Length);
							CollectionAssert.AreEqual(insert_values, select_values);
						}
					}
				}
			}
		}

		[Test]
		public async Task BigIntArrayTest()
		{
			var id_value = GetId();
			var insert_values = new long[] { 50, 60, 70, 80 };

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var insert = new FbCommand("INSERT INTO TEST (int_field, larray_field) values(@int_field, @array_field)", Connection, transaction))
				{
					insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
					insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
					await insert.ExecuteNonQueryAsync();
				}
				await transaction.CommitAsync();
			}

			await using (var select = new FbCommand($"SELECT larray_field FROM TEST WHERE int_field = {id_value}", Connection))
			{
				await using (var reader = await select.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						if (!await reader.IsDBNullAsync(0))
						{
							var select_values = new long[insert_values.Length];
							Array.Copy((Array)reader.GetValue(0), select_values, select_values.Length);
							CollectionAssert.AreEqual(insert_values, select_values);
						}
					}
				}
			}
		}

		[Test]
		public async Task FloatArrayTest()
		{
			var id_value = GetId();
			var insert_values = new float[] { 130.10F, 140.20F, 150.30F, 160.40F };

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var insert = new FbCommand("INSERT INTO TEST (int_field, farray_field) values(@int_field, @array_field)", Connection, transaction))
				{
					insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
					insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
					await insert.ExecuteNonQueryAsync();
				}
				await transaction.CommitAsync();
			}

			await using (var select = new FbCommand($"SELECT farray_field FROM TEST WHERE int_field = {id_value}", Connection))
			{
				await using (var reader = await select.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						if (!await reader.IsDBNullAsync(0))
						{
							var select_values = new float[insert_values.Length];
							Array.Copy((Array)reader.GetValue(0), select_values, select_values.Length);
							CollectionAssert.AreEqual(insert_values, select_values);
						}
					}
				}
			}
		}

		[Test]
		public async Task DoubleArrayTest()
		{
			var id_value = GetId();
			var insert_values = new double[] { 170.10, 180.20, 190.30, 200.40 };

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var insert = new FbCommand("INSERT INTO TEST (int_field, barray_field) values(@int_field, @array_field)", Connection, transaction))
				{
					insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
					insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
					await insert.ExecuteNonQueryAsync();
				}
				await transaction.CommitAsync();
			}

			await using (var select = new FbCommand($"SELECT barray_field FROM TEST WHERE int_field = {id_value}", Connection))
			{
				await using (var reader = await select.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						if (!await reader.IsDBNullAsync(0))
						{
							var select_values = new double[insert_values.Length];
							Array.Copy((Array)reader.GetValue(0), select_values, select_values.Length);
							CollectionAssert.AreEqual(insert_values, select_values);
						}
					}
				}
			}
		}

		[Test]
		public async Task NumericArrayTest()
		{
			var id_value = GetId();
			var insert_values = new decimal[] { 210.10M, 220.20M, 230.30M, 240.40M };

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var insert = new FbCommand("INSERT INTO TEST (int_field, narray_field) values(@int_field, @array_field)", Connection, transaction))
				{
					insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
					insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
					await insert.ExecuteNonQueryAsync();
				}
				await transaction.CommitAsync();
			}

			await using (var select = new FbCommand($"SELECT narray_field FROM TEST WHERE int_field = {id_value}", Connection))
			{
				await using (var reader = await select.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						if (!await reader.IsDBNullAsync(0))
						{
							var select_values = new decimal[insert_values.Length];
							Array.Copy((Array)reader.GetValue(0), select_values, select_values.Length);
							CollectionAssert.AreEqual(insert_values, select_values);
						}
					}
				}
			}
		}

		[Test]
		public async Task DateArrayTest()
		{
			var id_value = GetId();
			var insert_values = new DateTime[] { DateTime.Today.AddDays(10), DateTime.Today.AddDays(20), DateTime.Today.AddDays(30), DateTime.Today.AddDays(40) };

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var insert = new FbCommand("INSERT INTO TEST (int_field, darray_field) values(@int_field, @array_field)", Connection, transaction))
				{
					insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
					insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
					await insert.ExecuteNonQueryAsync();
				}
				await transaction.CommitAsync();
			}

			await using (var select = new FbCommand($"SELECT darray_field FROM TEST WHERE int_field = {id_value}", Connection))
			{
				await using (var reader = await select.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						if (!await reader.IsDBNullAsync(0))
						{
							var select_values = new DateTime[insert_values.Length];
							Array.Copy((Array)reader.GetValue(0), select_values, select_values.Length);
							CollectionAssert.AreEqual(insert_values, select_values);
						}
					}
				}
			}
		}

		[Test]
		public async Task TimeArrayTest()
		{
			var id_value = GetId();
			var insert_values = new TimeSpan[] { new TimeSpan(3, 9, 10), new TimeSpan(4, 11, 12), new TimeSpan(6, 13, 14), new TimeSpan(8, 15, 16) };

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var insert = new FbCommand("INSERT INTO TEST (int_field, tarray_field) values(@int_field, @array_field)", Connection, transaction))
				{
					insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
					insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
					await insert.ExecuteNonQueryAsync();
				}
				await transaction.CommitAsync();
			}

			await using (var select = new FbCommand($"SELECT tarray_field FROM TEST WHERE int_field = {id_value}", Connection))
			{
				await using (var reader = await select.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						if (!await reader.IsDBNullAsync(0))
						{
							var select_values = new TimeSpan[insert_values.Length];
							Array.Copy((Array)reader.GetValue(0), select_values, select_values.Length);
							CollectionAssert.AreEqual(insert_values, select_values);
						}
					}
				}
			}
		}

		[Test]
		public async Task TimeStampArrayTest()
		{
			var id_value = GetId();
			var insert_values = new DateTime[] { DateTime.Now.AddSeconds(10), DateTime.Now.AddSeconds(20), DateTime.Now.AddSeconds(30), DateTime.Now.AddSeconds(40) };

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var insert = new FbCommand("INSERT INTO TEST (int_field, tsarray_field) values(@int_field, @array_field)", Connection, transaction))
				{
					insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
					insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
					await insert.ExecuteNonQueryAsync();
				}
				await transaction.CommitAsync();
			}

			await using (var select = new FbCommand($"SELECT tsarray_field FROM TEST WHERE int_field = {id_value}", Connection))
			{
				await using (var reader = await select.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						if (!await reader.IsDBNullAsync(0))
						{
							var select_values = new DateTime[insert_values.Length];
							Array.Copy((Array)reader.GetValue(0), select_values, select_values.Length);
							insert_values = insert_values.Select(x => new DateTime(x.Ticks / 1000 * 1000)).ToArray();
							CollectionAssert.AreEqual(insert_values, select_values);
						}
					}
				}
			}
		}

		[Test]
		public async Task CharArrayTest()
		{
			var id_value = GetId();
			var insert_values = new string[] { "abc", "abcdef", "abcdefghi", "abcdefghijkl" };

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var insert = new FbCommand("INSERT INTO TEST (int_field, carray_field) values(@int_field, @array_field)", Connection, transaction))
				{
					insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
					insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
					await insert.ExecuteNonQueryAsync();
				}
				await transaction.CommitAsync();
			}

			await using (var select = new FbCommand($"SELECT carray_field FROM TEST WHERE int_field = {id_value}", Connection))
			{
				await using (var reader = await select.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						if (!await reader.IsDBNullAsync(0))
						{
							var select_values = new string[insert_values.Length];
							Array.Copy((Array)reader.GetValue(0), select_values, select_values.Length);
							select_values = select_values.Select(x => x.TrimEnd(' ')).ToArray();
							CollectionAssert.AreEqual(insert_values, select_values);
						}
					}
				}
			}
		}

		[Test]
		public async Task VarCharArrayTest()
		{
			var id_value = GetId();
			var insert_values = new string[] { "abc", "abcdef", "abcdefghi", "abcdefghijkl" };

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var insert = new FbCommand("INSERT INTO TEST (int_field, varray_field) values(@int_field, @array_field)", Connection, transaction))
				{
					insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
					insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
					await insert.ExecuteNonQueryAsync();
				}
				await transaction.CommitAsync();
			}

			await using (var select = new FbCommand($"SELECT varray_field FROM TEST WHERE int_field = {id_value}", Connection))
			{
				await using (var reader = await select.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						if (!await reader.IsDBNullAsync(0))
						{
							var select_values = new string[insert_values.Length];
							Array.Copy((Array)reader.GetValue(0), select_values, select_values.Length);
							CollectionAssert.AreEqual(insert_values, select_values);
						}
					}
				}
			}
		}

		[Test]
		public async Task IntegerArrayPartialUpdateTest()
		{
			var new_values = new int[] { 100, 200 };

			await using (var update = new FbCommand("update TEST set iarray_field = @array_field WHERE int_field = 1", Connection))
			{
				update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
				await update.ExecuteNonQueryAsync();
			}
		}

		[Test]
		public async Task ShortArrayPartialUpdateTest()
		{
			var new_values = new short[] { 500, 600 };

			await using (var update = new FbCommand("update TEST set sarray_field = @array_field WHERE int_field = 1", Connection))
			{
				update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
				await update.ExecuteNonQueryAsync();
			}
		}

		[Test]
		public async Task BigIntArrayPartialUpdateTest()
		{
			var new_values = new long[] { 900, 1000, 1100, 1200 };

			await using (var update = new FbCommand("update TEST set larray_field = @array_field WHERE int_field = 1", Connection))
			{
				update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
				await update.ExecuteNonQueryAsync();
			}
		}

		[Test]
		public async Task FloatArrayPartialUpdateTest()
		{
			var new_values = new float[] { 1300.10F, 1400.20F };

			await using (var update = new FbCommand("update TEST set farray_field = @array_field WHERE int_field = 1", Connection))
			{
				update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
				await update.ExecuteNonQueryAsync();
			}
		}

		[Test]
		public async Task DoubleArrayPartialUpdateTest()
		{
			var new_values = new double[] { 1700.10, 1800.20 };

			await using (var update = new FbCommand("update TEST set barray_field = @array_field WHERE int_field = 1", Connection))
			{
				update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
				await update.ExecuteNonQueryAsync();
			}
		}

		[Test]
		public async Task NumericArrayPartialUpdateTest()
		{
			var new_values = new decimal[] { 2100.10M, 2200.20M };

			await using (var update = new FbCommand("update TEST set narray_field = @array_field WHERE int_field = 1", Connection))
			{
				update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
				await update.ExecuteNonQueryAsync();
			}
		}

		[Test]
		public async Task DateArrayPartialUpdateTest()
		{
			var new_values = new DateTime[] { DateTime.Now.AddDays(100), DateTime.Now.AddDays(200) };

			await using (var update = new FbCommand("update TEST set darray_field = @array_field WHERE int_field = 1", Connection))
			{
				update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
				await update.ExecuteNonQueryAsync();
			}
		}

		[Test]
		public async Task TimeArrayPartialUpdateTest()
		{
			var new_values = new TimeSpan[] { new TimeSpan(11, 13, 14), new TimeSpan(12, 15, 16) };

			await using (var update = new FbCommand("update TEST set tarray_field = @array_field WHERE int_field = 1", Connection))
			{
				update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
				await update.ExecuteNonQueryAsync();
			}
		}

		[Test]
		public async Task TimeStampArrayPartialUpdateTest()
		{
			var new_values = new DateTime[] { DateTime.Now.AddSeconds(100), DateTime.Now.AddSeconds(200) };

			await using (var update = new FbCommand("update TEST set tsarray_field = @array_field WHERE int_field = 1", Connection))
			{
				update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
				await update.ExecuteNonQueryAsync();
			}
		}

		[Test]
		public async Task CharArrayPartialUpdateTest()
		{
			var new_values = new string[] { "abc", "abcdef" };

			await using (var update = new FbCommand("update TEST set carray_field = @array_field WHERE int_field = 1", Connection))
			{
				update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
				await update.ExecuteNonQueryAsync();
			}
		}

		[Test]
		public async Task VarCharArrayPartialUpdateTest()
		{
			var new_values = new string[] { "abc", "abcdef" };

			await using (var update = new FbCommand("update TEST set varray_field = @array_field WHERE int_field = 1", Connection))
			{
				update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
				await update.ExecuteNonQueryAsync();
			}
		}

		[Test]
		public async Task BigArrayTest()
		{
			var id_value = GetId();
			int elements = short.MaxValue;
			var bytes = new byte[elements * 4];
			using (var rng = new RNGCryptoServiceProvider())
			{
				rng.GetBytes(bytes);
			}
			var insert_values = new int[elements];
			Buffer.BlockCopy(bytes, 0, insert_values, 0, bytes.Length);

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var insert = new FbCommand("INSERT INTO TEST (int_field, big_array) values(@int_field, @array_field)", Connection, transaction))
				{
					insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
					insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
					await insert.ExecuteNonQueryAsync();
				}
				await transaction.CommitAsync();
			}

			await using (var select = new FbCommand($"SELECT big_array FROM TEST WHERE int_field = {id_value}", Connection))
			{
				await using (var reader = await select.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						if (!await reader.IsDBNullAsync(0))
						{
							var select_values = new int[insert_values.Length];
							Array.Copy((Array)reader.GetValue(0), select_values, select_values.Length);
							CollectionAssert.AreEqual(insert_values, select_values);
						}
					}
				}
			}
		}

		[Test]
		public async Task PartialUpdatesTest()
		{
			var id_value = GetId();
			var elements = 16384;
			var bytes = new byte[elements * 4];
			using (var rng = new RNGCryptoServiceProvider())
			{
				rng.GetBytes(bytes);
			}
			var insert_values = new int[elements];
			Buffer.BlockCopy(bytes, 0, insert_values, 0, bytes.Length);

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var insert = new FbCommand("INSERT INTO TEST (int_field, big_array) values(@int_field, @array_field)", Connection, transaction))
				{
					insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
					insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
					await insert.ExecuteNonQueryAsync();
				}
				await transaction.CommitAsync();
			}

			await using (var select = new FbCommand($"SELECT big_array FROM TEST WHERE int_field = {id_value}", Connection))
			{
				await using (var reader = await select.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						if (!await reader.IsDBNullAsync(0))
						{
							var select_values = new int[insert_values.Length];
							Array.Copy((Array)reader.GetValue(0), select_values, select_values.Length);
							CollectionAssert.AreEqual(insert_values, select_values);
						}
					}
				}
			}
		}
	}
}
