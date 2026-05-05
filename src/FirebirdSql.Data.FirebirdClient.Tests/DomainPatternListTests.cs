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

//$Authors = Ebubekir Cagri Sen (ebubekircagrisen@gmail.com)

using FirebirdSql.Data.Common;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests;

[NoServerCategory]
public class DomainPatternListTests
{
	[Test]
	public void Empty_NullSpec()
	{
		Assert.IsFalse(DomainPatternList.Parse(null).HasAny);
	}

	[Test]
	public void Empty_BlankSpec()
	{
		Assert.IsFalse(DomainPatternList.Parse("   ").HasAny);
	}

	[Test]
	public void Percent_PrefixWildcard_Matches()
	{
		var list = DomainPatternList.Parse("D_BOOL%");
		Assert.IsTrue(list.HasAny);
		Assert.IsTrue(list.Matches("D_BOOL"));
		Assert.IsTrue(list.Matches("D_BOOL_NULLABLE"));
		Assert.IsFalse(list.Matches("BOOL"));
	}

	[Test]
	public void Percent_OnlyMatchesAnything()
	{
		var list = DomainPatternList.Parse("%");
		Assert.IsTrue(list.Matches("ANYTHING"));
		Assert.IsTrue(list.Matches("X"));
		Assert.IsTrue(list.Matches("A"));
	}

	[Test]
	public void Underscore_MatchesSingleChar()
	{
		var list = DomainPatternList.Parse("FLAG_");
		Assert.IsTrue(list.Matches("FLAGX"));
		Assert.IsTrue(list.Matches("FLAG1"));
		Assert.IsFalse(list.Matches("FLAG"));
		Assert.IsFalse(list.Matches("FLAGXX"));
	}

	[Test]
	public void CaseInsensitive()
	{
		var list = DomainPatternList.Parse("d_bool%");
		Assert.IsTrue(list.Matches("D_BOOL"));
		Assert.IsTrue(list.Matches("D_Bool_Nullable"));
	}

	[Test]
	public void MultiplePatterns_CommaSeparated()
	{
		var list = DomainPatternList.Parse("D_BOOL%,BOOL\\_%,FLAG");
		Assert.IsTrue(list.Matches("D_BOOL_X"));
		Assert.IsTrue(list.Matches("FLAG"));
		Assert.IsFalse(list.Matches("FLAGX"));
		Assert.IsFalse(list.Matches("OTHER"));
	}

	[Test]
	public void RdbSystemDomains_Skipped()
	{
		var list = DomainPatternList.Parse("%");
		Assert.IsFalse(list.Matches("RDB$1"));
		Assert.IsFalse(list.Matches("rdb$abc"));
	}

	[Test]
	public void EmptyOrWhitespaceTokens_Ignored()
	{
		var list = DomainPatternList.Parse(",,FOO,, ,BAR");
		Assert.IsTrue(list.Matches("FOO"));
		Assert.IsTrue(list.Matches("BAR"));
	}

	[Test]
	public void NullDomain_NoMatch()
	{
		var list = DomainPatternList.Parse("%");
		Assert.IsFalse(list.Matches(null));
		Assert.IsFalse(list.Matches(""));
		Assert.IsFalse(list.Matches("   "));
	}

	[Test]
	public void TrimsDomainName()
	{
		var list = DomainPatternList.Parse("D_BOOL");
		Assert.IsTrue(list.Matches("  D_BOOL  "));
	}

	[Test]
	public void Percent_InMiddle_Works()
	{
		var list = DomainPatternList.Parse("FOO%BAR");
		Assert.IsTrue(list.Matches("FOOBAR"));
		Assert.IsTrue(list.Matches("FOO_X_BAR"));
		Assert.IsFalse(list.Matches("FOOBAZ"));
	}

	[Test]
	public void LiteralStar_HasNoSpecialMeaning()
	{
		// '*' is no longer a wildcard; it's matched literally.
		var list = DomainPatternList.Parse("FOO*");
		Assert.IsFalse(list.Matches("FOO"));
		Assert.IsFalse(list.Matches("FOOBAR"));
		Assert.IsTrue(list.Matches("FOO*"));
	}
}
