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
using FirebirdSql.Data.FirebirdClient;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	public class FbParameterCollectionTests
	{
		#region Unit Tests

		[Test]
		public void AddTest()
		{
			var command = new FbCommand();

			command.Parameters.Add(new FbParameter("@p292", 10000));
			command.Parameters.Add("@p01", FbDbType.Integer);
			command.Parameters.Add("@p02", 289273);
			command.Parameters.Add("#p3", FbDbType.SmallInt, 2, "sourceColumn");
		}

		[Test]
		public void DNET532_CheckCultureAwareIndexOf()
		{
			var curCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            try
			{
				System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("tr-TR");
				var command = new FbCommand();

				// \u0131 is turkish symbol "i without dot" that uppercases to "I" symbol.
				// see https://msdn.microsoft.com/en-us/library/ms973919.aspx#stringsinnet20_topic5 for more information
				var parameterName = "Turkish\u0131Parameter";
				command.Parameters.Add(parameterName, FbDbType.Char);
				Assert.AreNotEqual(-1, command.Parameters.IndexOf("turkishIParameter"));
			}
			finally
			{
				System.Threading.Thread.CurrentThread.CurrentCulture = curCulture;
            }
		}

		[Test]
		public void DNET532_CheckFlagForUsingOrdinalIgnoreCase()
		{
			var command = new FbCommand();
			command.Parameters.IndexOf("SomeField");

			for (var i = 0; i < 100; ++i)
			{
				command.Parameters.Add("FIELD" + i.ToString(), FbDbType.Integer);
			}

			const string probeParameterName = "FIELD0";
			const int noMatterValue = 12345;
			const int deleteIndex = 12;
			command.Parameters[probeParameterName].Value = noMatterValue;
			Assert.IsFalse(command.Parameters.HasParameterWithNonAsciiName);

			command.Parameters.Remove(command.Parameters[deleteIndex]);
			command.Parameters[probeParameterName].Value = noMatterValue;

			command.Parameters.RemoveAt(deleteIndex);
			command.Parameters[probeParameterName].Value = noMatterValue;

			command.Parameters.Insert(deleteIndex, new FbParameter("FIELD101", FbDbType.Integer));
			command.Parameters[probeParameterName].Value = noMatterValue;

			command.Parameters.Clear();
		}

		[Test]
		public void DNET532_CheckFlagForUsingOrdinalIgnoreCaseWithOuterChanges()
		{
			var collection = new FbParameterCollection();
			var parameter = new FbParameter() { ParameterName = "test" };
			collection.Add(parameter);
			var dummy1 = collection.IndexOf("dummy");
			Assert.IsFalse(collection.HasParameterWithNonAsciiName);
			parameter.ParameterName = "řčšřčšřčš";
			var dummy2 = collection.IndexOf("dummy");
			Assert.IsTrue(parameter.IsUnicodeParameterName);
			Assert.IsTrue(collection.HasParameterWithNonAsciiName);
		}

		[Test]
		public void CheckFbParameterParentPropertyInvariant()
		{
			var collection = new FbParameterCollection();
			var parameter = collection.Add("Name", FbDbType.Array);
			Assert.AreEqual(collection, parameter.Parent);
			Assert.Throws<ArgumentException>(() => collection.Add(parameter));
			Assert.Throws<ArgumentException>(() => collection.AddRange(new FbParameter[] { parameter }));

			collection.Remove(parameter);
			Assert.IsNull(parameter.Parent);

			Assert.Throws<ArgumentException>(() => collection.Remove(parameter));

			collection.Insert(0, parameter);
			Assert.AreEqual(collection, parameter.Parent);
			Assert.Throws<ArgumentException>(() => collection.Insert(0, parameter));
		}

		[Test]
		public void DNET635_ResetsParentOnClear()
		{
			var collection = new FbParameterCollection();
			var parameter = collection.Add("test", 0);
			Assert.IsNotNull(parameter.Parent);
			collection.Clear();
			Assert.IsNull(parameter.Parent);
		}

		#endregion
	}
}
