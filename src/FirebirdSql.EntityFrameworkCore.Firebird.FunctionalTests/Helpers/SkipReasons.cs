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

//$Authors = Jiri Cincura (jiri@cincura.net)

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Helpers;

// xUnit v3 reads [Fact]/[Theory] attributes including the inherited ones, hence overrides
// have to use the same attribute type (to replace the base one) with these reasons.
public static class SkipReasons
{
	public const string HasDataInTheSameTransactionAsDDL = "HasData is called in the same transaction as DDL commands.";
	public const string GeneratedNameTooLong = "Generated name in the query is too long.";
	public const string NotSupportedOnFirebird = "Not supported on Firebird.";
	public const string DoesNotHaveTheData = "Does not have the data.";
	public const string LongExecution = "Long execution.";
	public const string NotSupportedByProvider = "Not supported by provider.";
}
