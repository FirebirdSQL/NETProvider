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

using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Helpers;

public class HasDataInTheSameTransactionAsDDLFactAttribute : FactAttribute
{
	public HasDataInTheSameTransactionAsDDLFactAttribute()
	{
		Skip = "HasData is called in the same transaction as DDL commands.";
	}
}
public class HasDataInTheSameTransactionAsDDLTheoryAttribute : TheoryAttribute
{
	public HasDataInTheSameTransactionAsDDLTheoryAttribute()
	{
		Skip = "HasData is called in the same transaction as DDL commands.";
	}
}

public class GeneratedNameTooLongFactAttribute : FactAttribute
{
	public GeneratedNameTooLongFactAttribute()
	{
		Skip = "Generated name in the query is too long.";
	}
}
public class GeneratedNameTooLongTheoryAttribute : TheoryAttribute
{
	public GeneratedNameTooLongTheoryAttribute()
	{
		Skip = "Generated name in the query is too long.";
	}
}

public class NotSupportedOnFirebirdFactAttribute : FactAttribute
{
	public NotSupportedOnFirebirdFactAttribute()
	{
		Skip = "Not supported on Firebird.";
	}
}
public class NotSupportedOnFirebirdTheoryAttribute : TheoryAttribute
{
	public NotSupportedOnFirebirdTheoryAttribute()
	{
		Skip = "Not supported on Firebird.";
	}
}

public class DoesNotHaveTheDataFactAttribute : FactAttribute
{
	public DoesNotHaveTheDataFactAttribute()
	{
		Skip = "Does not have the data.";
	}
}
public class DoesNotHaveTheDataTheoryAttribute : TheoryAttribute
{
	public DoesNotHaveTheDataTheoryAttribute()
	{
		Skip = "Does not have the data.";
	}
}

public class LongExecutionFactAttribute : FactAttribute
{
	public LongExecutionFactAttribute()
	{
		Skip = "Long execution.";
	}
}
public class LongExecutionTheoryAttribute : TheoryAttribute
{
	public LongExecutionTheoryAttribute()
	{
		Skip = "Long execution.";
	}
}

public class NotSupportedByProviderFactAttribute : FactAttribute
{
	public NotSupportedByProviderFactAttribute()
	{
		Skip = "Not supported by provider.";
	}
}
public class NotSupportedByProviderTheoryAttribute : TheoryAttribute
{
	public NotSupportedByProviderTheoryAttribute()
	{
		Skip = "Not supported by provider.";
	}
}
