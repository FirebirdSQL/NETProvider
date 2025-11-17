/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Niek Schoemaker (@niekschoemaker)

using System;
using System.Threading.Tasks;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Translations;
using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query.Translations;

public class BasicTypesQueryFbFixture : BasicTypesQueryFixtureBase, ITestSqlLoggerFactory
{
	private BasicTypesData _expectedData;

	protected override ITestStoreFactory TestStoreFactory => FbTestStoreFactory.Instance;

	public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

	protected override Task SeedAsync(BasicTypesContext context)
	{
		_expectedData ??= CreateExpectedData();
		context.AddRange(_expectedData.BasicTypesEntities);
		context.AddRange(_expectedData.NullableBasicTypesEntities);
		return context.SaveChangesAsync();
	}

	public override ISetSource GetExpectedData()
		=> _expectedData ??= CreateExpectedData();

	private BasicTypesData CreateExpectedData()
	{
		var expectedData = (BasicTypesData)base.GetExpectedData();
		foreach (var entry in expectedData.BasicTypesEntities)
		{
			// For all Temporal types Firebird supports up to 1/10ths of a millisecond precision
			entry.DateTime = RoundToFirebirdPrecision(entry.DateTime);
			entry.TimeOnly = RoundToFirebirdPrecision(entry.TimeOnly);
			entry.TimeSpan = RoundToFirebirdPrecision(entry.TimeSpan);
			entry.DateTimeOffset = RoundToFirebirdPrecision(entry.DateTimeOffset);
		}

		// Do the same for the nullable counterparts
		foreach (var entry in expectedData.NullableBasicTypesEntities)
		{
			if (entry.DateTime.HasValue)
			{
				entry.DateTime = RoundToFirebirdPrecision(entry.DateTime.Value);
			}

			if (entry.TimeOnly.HasValue)
			{
				entry.TimeOnly = RoundToFirebirdPrecision(entry.TimeOnly.Value);
			}

			if (entry.TimeSpan.HasValue)
			{
				entry.TimeSpan = RoundToFirebirdPrecision(entry.TimeSpan.Value);
			}

			if (entry.DateTimeOffset.HasValue)
			{
				entry.DateTimeOffset = RoundToFirebirdPrecision(entry.DateTimeOffset.Value);
			}
		}

		return expectedData;
	}

	private static DateTime RoundToFirebirdPrecision(DateTime dateTime)
		=> new (StripSubDeciMillisecond(dateTime.Ticks), dateTime.Kind);

	private static DateTimeOffset RoundToFirebirdPrecision(DateTimeOffset dateTimeOffset)
		=> new (StripSubDeciMillisecond(dateTimeOffset.Ticks), TimeSpan.Zero);

	private static TimeOnly RoundToFirebirdPrecision(TimeOnly timeOnly)
		=> new (StripSubDeciMillisecond(timeOnly.Ticks));

	private static TimeSpan RoundToFirebirdPrecision(TimeSpan timeOnly)
		=> new (StripSubDeciMillisecond(timeOnly.Ticks));

	static long StripSubDeciMillisecond(long ticks) => ticks - (ticks % (TimeSpan.TicksPerMillisecond / 10));
}
