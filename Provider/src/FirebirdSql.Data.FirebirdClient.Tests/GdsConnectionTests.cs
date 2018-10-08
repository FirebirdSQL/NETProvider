using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[TestFixture]
	class GdsConnectionTests
	{
		[Test, TestCaseSource(typeof(NormalizeLoginTestData), "TestCases")]
		public string NormalizeLoginTest(string login)
		{
			return Client.Managed.GdsConnection.NormalizeLogin(login);
		}
	}

	class NormalizeLoginTestData
	{
		public static IEnumerable<TestCaseData> TestCases
		{
			get
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
		}
	}
}
