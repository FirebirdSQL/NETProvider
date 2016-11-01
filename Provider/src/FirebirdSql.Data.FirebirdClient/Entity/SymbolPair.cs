/*
 *  Firebird ADO.NET Data provider for .NET and Mono
 *
 *     The contents of this file are subject to the Initial
 *     Developer's Public License Version 1.0 (the "License");
 *     you may not use this file except in compliance with the
 *     License. You may obtain a copy of the License at
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *     express or implied.  See the License for the specific
 *     language governing rights and limitations under the License.
 *
 *  Copyright (c) 2008-2014 Jiri Cincura (jiri@cincura.net)
 *  All Rights Reserved.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
#if !EF6
using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees;
#else
#endif

using FirebirdSql.Data.FirebirdClient;

#if !EF6
namespace FirebirdSql.Data.Entity
#else
namespace FirebirdSql.Data.EntityFramework6.SqlGen
#endif
{
	internal class SymbolPair : ISqlFragment
	{
		#region Fields

		private Symbol _source;
		private Symbol _column;

		#endregion

		#region Properties

		public Symbol Source
		{
			get { return _source; }
			set { _source = value; }
		}

		public Symbol Column
		{
			get { return _column; }
			set { _column = value; }
		}

		#endregion

		#region Constructors

		public SymbolPair(Symbol source, Symbol column)
		{
			Source = source;
			Column = column;
		}

		#endregion

		#region ISqlFragment Members

		public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
		{
			// Symbol pair should never be part of a SqlBuilder.
			Debug.Assert(false);
		}

		#endregion
	}
}
