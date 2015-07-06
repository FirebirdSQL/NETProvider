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
#if (!EF_6)
using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees;
#else
using System.Data.Entity.Core.Metadata.Edm;
#endif

#if (!EF_6)
namespace FirebirdSql.Data.Entity
#else
namespace FirebirdSql.Data.EntityFramework6.SqlGen
#endif
{
	internal class Symbol : ISqlFragment
	{
		#region Fields

		private Dictionary<string, Symbol> columns = new Dictionary<string, Symbol>(StringComparer.CurrentCultureIgnoreCase);
		private bool needsRenaming = false;
		private bool isUnnest = false;
		private string name;
		private string newName;
		private TypeUsage type;

		#endregion

		#region Public Properties

		public string Name
		{
			get { return this.name; }
		}

		public string NewName
		{
			get { return this.newName; }
			set { this.newName = value; }
		}

		#endregion

		#region Internal Properties

		internal Dictionary<string, Symbol> Columns
		{
			get { return this.columns; }
		}

		internal bool NeedsRenaming
		{
			get { return this.needsRenaming; }
			set { this.needsRenaming = value; }
		}

		internal bool IsUnnest
		{
			get { return this.isUnnest; }
			set { this.isUnnest = value; }
		}

		internal TypeUsage Type
		{
			get { return this.type; }
			set { this.type = value; }
		}

		#endregion

		#region Constructors

		public Symbol(string name, TypeUsage type)
		{
			this.name = name;
			this.newName = name;
			this.Type = type;
		}

		#endregion

		#region ISqlFragment Members

		/// <summary>
		/// Write this symbol out as a string for sql.  This is just
		/// the new name of the symbol (which could be the same as the old name).
		///
		/// We rename columns here if necessary.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="sqlGenerator"></param>
		public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
		{
			if (this.NeedsRenaming)
			{
				string newName;
				int i = sqlGenerator.AllColumnNames[this.NewName];
				do
				{
					++i;
					newName = this.Name + i.ToString(System.Globalization.CultureInfo.InvariantCulture);
				} while (sqlGenerator.AllColumnNames.ContainsKey(newName));
				sqlGenerator.AllColumnNames[this.NewName] = i;

				// Prevent it from being renamed repeatedly.
				this.NeedsRenaming = false;
				this.NewName = newName;

				// Add this column name to list of known names so that there are no subsequent
				// collisions
				sqlGenerator.AllColumnNames[newName] = 0;
			}

			writer.Write(SqlGenerator.QuoteIdentifier(this.NewName));
		}

		#endregion
	}
}
