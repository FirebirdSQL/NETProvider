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
using System.Data.Common;

using NUnit.Framework;
using FirebirdSql.Data.Firebird;

namespace FirebirdSql.Data.Firebird.Tests
{
	[TestFixture]
	public class FbParameterTest : BaseTest 
	{
		public FbParameterTest() : base()
		{		
		}
		
		[Test]
		public void ConstructorsTest()
		{
			FbParameter ctor01 = new FbParameter();
			FbParameter ctor02 = new FbParameter("ctor2", 10);
			FbParameter ctor03 = new FbParameter("ctor3", FbDbType.Char);
			FbParameter ctor04 = new FbParameter("ctor4", FbDbType.Integer, 4);
			FbParameter ctor05 = new FbParameter("ctor5", FbDbType.Integer, 4, "int_field");
			FbParameter ctor06 = new FbParameter(
				"ctor6", 
				FbDbType.Integer, 
				4, 
				ParameterDirection.Input, 
				false, 
				0, 
				0, 
				"int_field", 
				DataRowVersion.Original, 
				100);
		}
	}
}
