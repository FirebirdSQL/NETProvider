using System.Collections.Generic;
using FirebirdSql.Data.Client.Managed;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[TestFixture]
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
