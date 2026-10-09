using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests;

[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Default))]
[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Embedded))]
public class BlobStreamTests : FbTestsBase
{
	public BlobStreamTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
		: base(serverType, compression, wireCrypt)
	{ }

	[Test]
	public async Task FbBlobStreamReadTest()
	{
		var id_value = RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);
		var insert_values = RandomNumberGenerator.GetBytes(100000 * 4);

		await using (var transaction = await Connection.BeginTransactionAsync())
		{
			await using (var insert = new FbCommand("INSERT INTO TEST (int_field, blob_field) values(@int_field, @blob_field)", Connection, transaction))
			{
				insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
				insert.Parameters.Add("@blob_field", FbDbType.Binary).Value = insert_values;
				await insert.ExecuteNonQueryAsync();
			}
			await transaction.CommitAsync();
		}

		await using (var select = new FbCommand($"SELECT blob_field FROM TEST WHERE int_field = {id_value}", Connection))
		{
			await using var reader = await select.ExecuteReaderAsync();
			while (await reader.ReadAsync())
			{
				await using var output = new MemoryStream();
				await using (var stream = reader.GetStream(0))
				{
					await stream.CopyToAsync(output);
				}

				var select_values = output.ToArray();
				CollectionAssert.AreEqual(insert_values, select_values);
			}
		}
	}

	[Test]
	public async Task FbBlobStreamWriteTest()
	{
		var id_value = RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);
		var insert_values = RandomNumberGenerator.GetBytes(100000 * 4);

		await using (var transaction = await Connection.BeginTransactionAsync())
		{
			await using (var insert = new FbCommand("INSERT INTO TEST (int_field, blob_field) values(@int_field, @blob_field)", Connection, transaction))
			{
				insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
				insert.Parameters.Add("@blob_field", FbDbType.Binary).Value = insert_values;
				await insert.ExecuteNonQueryAsync();
			}

			await using (var select = new FbCommand($"SELECT blob_field FROM TEST WHERE int_field = {id_value}", Connection, transaction))
			{
				await using var reader = await select.ExecuteReaderAsync();
				while (await reader.ReadAsync())
				{
					await using var stream = reader.GetStream(0);
					await stream.WriteAsync(insert_values);

					break;
				}
			}
			await transaction.CommitAsync();
		}

		await using (var select = new FbCommand($"SELECT blob_field FROM TEST WHERE int_field = {id_value}", Connection))
		{
			var select_values = (byte[])await select.ExecuteScalarAsync();
			CollectionAssert.AreEqual(insert_values, select_values);
		}
	}
}