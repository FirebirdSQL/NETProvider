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
	public class FbParameterCollectionTest : BaseTest 
	{
		public FbParameterCollectionTest() : base()
		{		
		}
		
		[Test]
		public void AddTest()
		{
			FbParameterCollection parameters = new FbParameterCollection();
			
			parameters.Add(new FbParameter("@p292", 10000));			
			parameters.Add("@p01", FbType.Integer);			
			parameters.Add("@p02", 289273);
			parameters.Add("#p3", FbType.NChar, "sourceColumn");
			parameters.Add("#p3", FbType.SmallInt, 2, "sourceColumn");
		}
	}
}
