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

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Threading.Tasks;
using FirebirdSql.Data.TestsBase;
using FirebirdSql.Data.Types;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Default))]
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Embedded))]
	public class FbTimeZonesSupportTests : FbTestsBase
	{
		public FbTimeZonesSupportTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
			: base(serverType, compression, wireCrypt)
		{ }

		[SetUp]
		public override async Task SetUp()
		{
			await base.SetUp();

			if (!EnsureServerVersion(new Version(4, 0, 0, 0)))
				return;
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task ReadsZonedDateTimeCorrectly(bool isExtended)
		{
			if (isExtended)
			{
				await SetExtended();
			}
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast('2020-08-27 10:00 Europe/Prague' as timestamp with time zone) from rdb$database";
				var result = (FbZonedDateTime)await cmd.ExecuteScalarAsync();
				Assert.AreEqual(new DateTime(2020, 08, 27, 08, 00, 00, DateTimeKind.Utc), result.DateTime);
				Assert.AreEqual("Europe/Prague", result.TimeZone);
				Assert.AreEqual(isExtended ? TimeSpan.FromMinutes(120) : (TimeSpan?)null, result.Offset);
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task ReadsZonedDateTimeNullCorrectly(bool isExtended)
		{
			if (isExtended)
			{
				await SetExtended();
			}
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast(null as timestamp with time zone) from rdb$database";
				var result = (DBNull)await cmd.ExecuteScalarAsync();
				Assert.AreEqual(DBNull.Value, result);
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task PassesZonedDateTimeCorrectly(bool isExtended)
		{
			if (isExtended)
			{
				await SetExtended();
			}
			var value = new FbZonedDateTime(new DateTime(2020, 08, 27, 08, 00, 00, DateTimeKind.Utc), "Europe/Prague");
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast(@value as timestamp with time zone) from rdb$database";
				cmd.Parameters.AddWithValue("value", value);
				var result = (FbZonedDateTime)await cmd.ExecuteScalarAsync();
				Assert.AreEqual(value, result);
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task PassesZonedDateTimeNullCorrectly(bool isExtended)
		{
			if (isExtended)
			{
				await SetExtended();
			}
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast(@value as timestamp with time zone) from rdb$database";
				cmd.Parameters.AddWithValue("value", DBNull.Value);
				var result = (DBNull)await cmd.ExecuteScalarAsync();
				Assert.AreEqual(DBNull.Value, result);
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task ReadsZonedTimeCorrectly(bool isExtended)
		{
			if (isExtended)
			{
				await SetExtended();
			}
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast('15:00 Europe/Prague' as time with time zone) from rdb$database";
				var result = (FbZonedTime)await cmd.ExecuteScalarAsync();
				Assert.AreEqual(new TimeSpan(14, 00, 00), result.Time);
				Assert.AreEqual("Europe/Prague", result.TimeZone);
				Assert.AreEqual(isExtended ? TimeSpan.FromMinutes(60) : (TimeSpan?)null, result.Offset);
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task ReadsZonedTimeNullCorrectly(bool isExtended)
		{
			if (isExtended)
			{
				await SetExtended();
			}
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast(null as time with time zone) from rdb$database";
				var result = (DBNull)await cmd.ExecuteScalarAsync();
				Assert.AreEqual(DBNull.Value, result);
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task PassesZonedTimeCorrectly(bool isExtended)
		{
			if (isExtended)
			{
				await SetExtended();
			}
			var value = new FbZonedTime(new TimeSpan(14, 00, 00), "Europe/Prague");
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast(@value as time with time zone) from rdb$database";
				cmd.Parameters.AddWithValue("value", value);
				var result = (FbZonedTime)await cmd.ExecuteScalarAsync();
				Assert.AreEqual(value, result);
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task PassesZonedTimeNullCorrectly(bool isExtended)
		{
			if (isExtended)
			{
				await SetExtended();
			}
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast(@value as time with time zone) from rdb$database";
				cmd.Parameters.AddWithValue("value", DBNull.Value);
				var result = (DBNull)await cmd.ExecuteScalarAsync();
				Assert.AreEqual(DBNull.Value, result);
			}
		}

		[Test]
		public async Task SimpleSelectSchemaTableTest()
		{
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select current_timestamp, current_time from rdb$database";
				await using (var reader = await cmd.ExecuteReaderAsync())
				{
					var schema = await reader.GetSchemaTableAsync();
					Assert.AreEqual(typeof(FbZonedDateTime), schema.Rows[0].ItemArray[5]);
					Assert.AreEqual(typeof(FbZonedTime), schema.Rows[1].ItemArray[5]);
				}
			}
		}

		async Task SetExtended()
		{
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "set bind of time zone to extended";
				await cmd.ExecuteNonQueryAsync();
			}
		}
	}
}
