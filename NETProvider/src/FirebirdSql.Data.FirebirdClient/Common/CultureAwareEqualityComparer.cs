/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2014 Jiri Cincura (jiri@cincura.net)
 *	All Rights Reserved.
 *	
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirebirdSql.Data.Common
{
	class CultureAwareEqualityComparer : IEqualityComparer<string>
	{
		public static readonly CultureAwareEqualityComparer Instance = new CultureAwareEqualityComparer();

		const CompareOptions CultureAwareCompareOptions = CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.IgnoreCase;

		public bool Equals(string x, string y)
		{
			return CurrentCompareInfo.Compare(x, y, CultureAwareCompareOptions) == 0;
		}

		public int GetHashCode(string obj)
		{
			return CurrentCompareInfo.GetSortKey(obj, CultureAwareCompareOptions).GetHashCode();
		}

		CompareInfo CurrentCompareInfo
		{
			get { return CultureInfo.CurrentCulture.CompareInfo; }
		}
	}
}
