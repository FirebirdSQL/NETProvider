//
// Firebird .NET Data Provider - Firebird managed data provider for .NET and Mono
// Copyright (C) 2002-2003  Carlos Guzman Alvarez
//
// Distributable under LGPL license.
// You may obtain a copy of the License at http://www.gnu.org/copyleft/lesser.html
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//

using NUnit.Framework;

using System;
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
			FbParameter ctor02 = new FbParameter("ctor01", FbType.BigInt);
			FbParameter ctor03 = new FbParameter("ctor01", "10");
			FbParameter ctor04 = new FbParameter("ctor01", FbType.Char, "sourceColumn");
			FbParameter ctor05 = new FbParameter("ctor01", FbType.SmallInt, 2, "SourceColumn");
		}
	}
}
