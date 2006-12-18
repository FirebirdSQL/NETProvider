/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.ibphoenix.com/main.nfs?a=ibphoenix&l=;PAGES;NAME='ibp_idpl'
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Data;
using System.Collections;
using FirebirdSql.Data.Firebird;
using FirebirdSql.Data.Firebird.Isql;
using NUnit.Framework;
using System.Configuration;

namespace FirebirdSql.Data.Firebird.Tests
{
	[TestFixture]
	public class FbScriptTest : BaseTest 
	{
		public FbScriptTest()
		{
		}

		[Test]
		public void IsqlScriptTest()
		{
			if (System.IO.File.Exists(ConfigurationSettings.AppSettings["IsqlScript"]))
			{
				FbScript isql = new FbScript();

				ArrayList commands = isql.Parse(
					ConfigurationSettings.AppSettings["IsqlScript"], 
					System.Text.Encoding.UTF8);

				foreach (string command in commands)
				{
					Console.WriteLine(command);
				}
			}
		}
	}
}
