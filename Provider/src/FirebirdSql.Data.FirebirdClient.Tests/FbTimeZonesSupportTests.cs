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
using FirebirdSql.Data.TestsBase;
using FirebirdSql.Data.Types;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[TestFixtureSource(typeof(FbDefaultServerTypeTestFixtureSource))]
	[TestFixtureSource(typeof(FbEmbeddedServerTypeTestFixtureSource))]
	public class FbTimeZonesSupportTests : FbTestsBase
	{
		public FbTimeZonesSupportTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
			: base(serverType, compression, wireCrypt)
		{ }

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();

			if (!EnsureVersion(new Version(4, 0, 0, 0)))
				return;
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ReadsZonedDateTimeCorrectly(bool isExtended)
		{
			if (isExtended)
			{
				SetExtended();
			}
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast('2020-08-27 10:00 Europe/Prague' as timestamp with time zone) from rdb$database";
				var result = (FbZonedDateTime)cmd.ExecuteScalar();
				Assert.AreEqual(new DateTime(2020, 08, 27, 08, 00, 00, DateTimeKind.Utc), result.DateTime);
				Assert.AreEqual("Europe/Prague", result.TimeZone);
				Assert.AreEqual(isExtended ? TimeSpan.FromMinutes(120) : (TimeSpan?)null, result.Offset);
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ReadsZonedDateTimeNullCorrectly(bool isExtended)
		{
			if (isExtended)
			{
				SetExtended();
			}
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast(null as timestamp with time zone) from rdb$database";
				var result = (DBNull)cmd.ExecuteScalar();
				Assert.AreEqual(DBNull.Value, result);
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public void PassesZonedDateTimeCorrectly(bool isExtended)
		{
			if (isExtended)
			{
				SetExtended();
			}
			var value = new FbZonedDateTime(new DateTime(2020, 08, 27, 08, 00, 00, DateTimeKind.Utc), "Europe/Prague");
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast(@value as timestamp with time zone) from rdb$database";
				cmd.Parameters.AddWithValue("value", value);
				var result = (FbZonedDateTime)cmd.ExecuteScalar();
				Assert.AreEqual(value, result);
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public void PassesZonedDateTimeNullCorrectly(bool isExtended)
		{
			if (isExtended)
			{
				SetExtended();
			}
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast(@value as timestamp with time zone) from rdb$database";
				cmd.Parameters.AddWithValue("value", DBNull.Value);
				var result = (DBNull)cmd.ExecuteScalar();
				Assert.AreEqual(DBNull.Value, result);
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ReadsZonedTimeCorrectly(bool isExtended)
		{
			if (isExtended)
			{
				SetExtended();
			}
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast('15:00 Europe/Prague' as time with time zone) from rdb$database";
				var result = (FbZonedTime)cmd.ExecuteScalar();
				Assert.AreEqual(new TimeSpan(14, 00, 00), result.Time);
				Assert.AreEqual("Europe/Prague", result.TimeZone);
				Assert.AreEqual(isExtended ? TimeSpan.FromMinutes(60) : (TimeSpan?)null, result.Offset);
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ReadsZonedTimeNullCorrectly(bool isExtended)
		{
			if (isExtended)
			{
				SetExtended();
			}
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast(null as time with time zone) from rdb$database";
				var result = (DBNull)cmd.ExecuteScalar();
				Assert.AreEqual(DBNull.Value, result);
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public void PassesZonedTimeCorrectly(bool isExtended)
		{
			if (isExtended)
			{
				SetExtended();
			}
			var value = new FbZonedTime(new TimeSpan(14, 00, 00), "Europe/Prague");
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast(@value as time with time zone) from rdb$database";
				cmd.Parameters.AddWithValue("value", value);
				var result = (FbZonedTime)cmd.ExecuteScalar();
				Assert.AreEqual(value, result);
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public void PassesZonedTimeNullCorrectly(bool isExtended)
		{
			if (isExtended)
			{
				SetExtended();
			}
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast(@value as time with time zone) from rdb$database";
				cmd.Parameters.AddWithValue("value", DBNull.Value);
				var result = (DBNull)cmd.ExecuteScalar();
				Assert.AreEqual(DBNull.Value, result);
			}
		}

		[Test]
		public void SimpleSelectSchemaTableTest()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select current_timestamp, current_time from rdb$database";
				using (var reader = cmd.ExecuteReader())
				{
					var schema = reader.GetSchemaTable();
					Assert.AreEqual(typeof(FbZonedDateTime), schema.Rows[0].ItemArray[5]);
					Assert.AreEqual(typeof(FbZonedTime), schema.Rows[1].ItemArray[5]);
				}
			}
		}

		void SetExtended()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "set bind of time zone to extended";
				cmd.ExecuteNonQuery();
			}
		}
	}
}
