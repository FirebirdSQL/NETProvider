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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Linq;
using FirebirdSql.Data.Isql;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	public class FbScriptTests
	{
		#region Unit Tests

		[Test]
		public void NullScript()
		{
			Assert.Throws<ArgumentNullException>(() => new FbScript(null));
		}

		[Test]
		public void EmptyScript()
		{
			FbScript script = new FbScript(string.Empty);
			script.Parse();
			Assert.AreEqual(0, script.Results.Count());
		}

		[Test]
		public void WhiteSpacesScript()
		{
			FbScript script = new FbScript("\t    \t ");
			script.Parse();
			Assert.AreEqual(0, script.Results.Count());
		}

		[Test]
		public void SimpleStatementNoSemicolonWithLiteral()
		{
			const string text =
@"select * from foo where x = 'foobar'";
			FbScript script = new FbScript(text);
			script.Parse();
			Assert.AreEqual(1, script.Results.Count());
			Assert.AreEqual(text, script.Results[0].Text);
		}

		[Test]
		public void SimpleStatementWithSemicolonWithLiteral()
		{
			const string text =
@"select * from foo where x = 'foobar';";
			FbScript script = new FbScript(text);
			script.Parse();
			Assert.AreEqual(1, script.Results.Count());
			Assert.AreEqual(text.Substring(0, text.Length - 1), script.Results[0].Text);
		}

		[Test]
		public void SimpleStatementNoSemicolonWithSemicolonInLiteral()
		{
			const string text =
@"select * from foo where x = 'foo;bar'";
			FbScript script = new FbScript(text);
			script.Parse();
			Assert.AreEqual(1, script.Results.Count());
			Assert.AreEqual(text, script.Results[0].Text);
		}

		[Test]
		public void SimpleStatementNoSemicolonWithEscapedSingleQuoteInLiteral()
		{
			const string text =
@"select * from foo where x = 'foo''bar'";
			FbScript script = new FbScript(text);
			script.Parse();
			Assert.AreEqual(1, script.Results.Count());
			Assert.AreEqual(text, script.Results[0].Text);
		}

		[Test]
		public void TwoStatements()
		{
			const string text =
@"select * from foo;select * from bar";
			FbScript script = new FbScript(text);
			script.Parse();
			Assert.AreEqual(2, script.Results.Count());
		}

		[Test]
		public void OneStatementNoSemicolonOneAfterSingleLineComment()
		{
			const string text =
@"select * from foo--;select * from bar";
			FbScript script = new FbScript(text);
			script.Parse();
			Assert.AreEqual(1, script.Results.Count());
			Assert.AreEqual(text, script.Results[0].Text);
		}

		[Test]
		public void OneStatementWithMultilineCommentNoSemicolon()
		{
			const string text =
@"select * from foo /* foo */";
			FbScript script = new FbScript(text);
			script.Parse();
			Assert.AreEqual(1, script.Results.Count());
			Assert.AreEqual(text, script.Results[0].Text);
		}

		[Test]
		public void OneStatementWithMultilineCommentSeparatedBySemicolon()
		{
			const string text =
@"select * from foo /* foo */;";
			FbScript script = new FbScript(text);
			script.Parse();
			Assert.AreEqual(1, script.Results.Count());
			Assert.AreEqual(text.Substring(0, text.Length - 1), script.Results[0].Text);
		}

		[Test]
		public void OneStatementWithMultilineCommentWithSemicolon()
		{
			const string text =
@"select * from foo /* ;foo */";
			FbScript script = new FbScript(text);
			script.Parse();
			Assert.AreEqual(1, script.Results.Count());
			Assert.AreEqual(text, script.Results[0].Text);
		}

		[Test]
		public void OneStatementWithMultilineCommentWithSemicolonWithSemicolonAtTheEnd()
		{
			const string text =
@"select * from foo /* ;foo */;";
			FbScript script = new FbScript(text);
			script.Parse();
			Assert.AreEqual(1, script.Results.Count());
			Assert.AreEqual(text.Substring(0, text.Length - 1), script.Results[0].Text);
		}

		[Test]
		public void SemicolonOnly()
		{
			const string text =
@";";
			FbScript script = new FbScript(text);
			script.Parse();
			Assert.AreEqual(0, script.Results.Count());
		}

		[Test]
		public void MultilineCommentSeparatedBySemicolon()
		{
			const string text =
@"/*
foo
*/;";
			FbScript script = new FbScript(text);
			script.UnknownStatement += (sender, e) =>
			{
				if (e.Statement.Text == text.Substring(0, text.Length - 1))
				{
					e.Ignore = true;
				}
			};
			script.Parse();
			Assert.AreEqual(0, script.Results.Count());
		}

		[Test]
		public void OneStatementWithSemicolonOneAfterSingleLineComment()
		{
			const string text =
@"select * from foo;--select * from bar";
			FbScript script = new FbScript(text);
			script.UnknownStatement += (sender, e) =>
			{
				if (e.Statement.Text == "--select * from bar")
				{
					e.Ignore = true;
				}
			};
			script.Parse();
			Assert.AreEqual(1, script.Results.Count());
			Assert.AreEqual("select * from foo", script.Results[0].Text);
		}

		[Test]
		public void ManuallySettingStatementType()
		{
			const string text =
@"create db 'foobar'";
			FbScript script = new FbScript(text);
			script.UnknownStatement += (sender, e) =>
			{
				if (e.Statement.Text == text)
				{
					e.NewStatementType = SqlStatementType.CreateDatabase;
					e.Handled = true;
				}
			};
			script.Parse();
			Assert.AreEqual(1, script.Results.Count());
			Assert.AreEqual(SqlStatementType.CreateDatabase, script.Results[0].StatementType);
		}

		[Test]
		public void CreatePackage()
		{
			const string text =
@"create package p as begin end";
			FbScript script = new FbScript(text);
			script.Parse();
			Assert.AreEqual(1, script.Results.Count());
			Assert.AreEqual(SqlStatementType.CreatePackage, script.Results[0].StatementType);
		}

		[Test]
		public void RecreatePackage()
		{
			const string text =
@"recreate package p as begin end";
			FbScript script = new FbScript(text);
			script.Parse();
			Assert.AreEqual(1, script.Results.Count());
			Assert.AreEqual(SqlStatementType.RecreatePackage, script.Results[0].StatementType);
		}

		[Test]
		public void CreatePackageBody()
		{
			const string text =
@"create package body p as begin end";
			FbScript script = new FbScript(text);
			script.Parse();
			Assert.AreEqual(1, script.Results.Count());
			Assert.AreEqual(SqlStatementType.CreatePackageBody, script.Results[0].StatementType);
		}

		[Test]
		public void RecreatePackageBody()
		{
			const string text =
@"recreate package body p as begin end";
			FbScript script = new FbScript(text);
			script.Parse();
			Assert.AreEqual(1, script.Results.Count());
			Assert.AreEqual(SqlStatementType.RecreatePackageBody, script.Results[0].StatementType);
		}

		[Test]
		public void DropPackage()
		{
			const string text =
@"drop package p as begin end";
			FbScript script = new FbScript(text);
			script.Parse();
			Assert.AreEqual(1, script.Results.Count());
			Assert.AreEqual(SqlStatementType.DropPackage, script.Results[0].StatementType);
		}

		[Test]
		public void DropPackageBody()
		{
			const string text =
@"drop package body p as begin end";
			FbScript script = new FbScript(text);
			script.Parse();
			Assert.AreEqual(1, script.Results.Count());
			Assert.AreEqual(SqlStatementType.DropPackageBody, script.Results[0].StatementType);
		}

		[Test]
		public void Merge()
		{
			const string text =
@"merge into table t using foo f on f.id = t.id when ";
			FbScript script = new FbScript(text);
			script.Parse();
			Assert.AreEqual(1, script.Results.Count());
			Assert.AreEqual(SqlStatementType.Merge, script.Results[0].StatementType);
		}

		#endregion
	}
}
