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

//$Authors = Mark Rotteveel

using System.Collections.Generic;
using FirebirdSql.Data.Client.Managed;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	public class GdsConnectionTests
	{
		static IEnumerable<TestCaseData> NormalizeLoginTestSource()
		{
			yield return new TestCaseData("sysdba").Returns("SYSDBA");
			yield return new TestCaseData("s").Returns("S");
			yield return new TestCaseData("\"CaseSensitive\"").Returns("CaseSensitive");
			yield return new TestCaseData("\"s\"").Returns("s");
			yield return new TestCaseData("\"With\"\"EscapedQuote\"").Returns("With\"EscapedQuote");
			yield return new TestCaseData("\"Invalid\"Escape\"").Returns("Invalid");
			yield return new TestCaseData("\"DanglingInvalidEscape\"\"").Returns("DanglingInvalidEscape");
			yield return new TestCaseData("\"EscapedQuoteAtEnd\"\"\"").Returns("EscapedQuoteAtEnd\"");
			yield return new TestCaseData("\"StartNoEndQuote").Returns("\"STARTNOENDQUOTE");
			yield return new TestCaseData("\"\"").Returns("\"\"");
			yield return new TestCaseData("").Returns("");
			yield return new TestCaseData(null).Returns(null);
		}
		[TestCaseSource(nameof(NormalizeLoginTestSource))]
		public string NormalizeLoginTest(string login)
		{
			return GdsConnection.NormalizeLogin(login);
		}
	}
}
