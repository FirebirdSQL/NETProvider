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

using System.Collections;
using FirebirdSql.Data.FirebirdClient;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace FirebirdSql.Data.TestsBase
{
	class FbServerTypeTestFixtureSource
	{
		public static IEnumerable Default()
		{
			yield return CreateTestFixtureData(nameof(FbServerType.Default), FbServerType.Default, false, FbWireCrypt.Disabled);
			yield return CreateTestFixtureData(nameof(FbServerType.Default), FbServerType.Default, false, FbWireCrypt.Required);
			yield return CreateTestFixtureData(nameof(FbServerType.Default), FbServerType.Default, true, FbWireCrypt.Disabled);
			yield return CreateTestFixtureData(nameof(FbServerType.Default), FbServerType.Default, true, FbWireCrypt.Required);

		}

		public static IEnumerable Embedded()
		{
			yield return CreateTestFixtureData(nameof(FbServerType.Embedded), FbServerType.Embedded, default(bool), default(FbWireCrypt));
		}

		static TestFixtureData CreateTestFixtureData(string category, params object[] args)
		{
			var result = new TestFixtureData(args);
			result.Properties.Set(PropertyNames.Category, $"Server{category}");
			return result;
		}
	}
}
