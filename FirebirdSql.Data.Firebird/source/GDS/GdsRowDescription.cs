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

namespace FirebirdSql.Data.Firebird.Gds
{
	internal class GdsRowDescription
	{
		#region FIELDS

		private int			version;
		private int			sqln;
		private int			sqld;
		private GdsField[]	sqlvar;
		
		#endregion

		#region PROPERTIES

		public int Version
		{
			get { return version; }
			set { version = value; }
		}

		public int SqlN
		{
			get { return sqln; }
			set { sqln = value; }
		}

		public int SqlD
		{
			get { return sqld; }
			set { sqld = value; }
		}

		public GdsField[] SqlVar
		{
			get { return sqlvar; }
			set { sqlvar = value; }
		}

		#endregion

		#region CONSTRUCTORS

		public GdsRowDescription() 
		{
			version = GdsCodes.SQLDA_VERSION1;
		}

		public GdsRowDescription(int n) : this()
		{
			sqln	= n;
			sqld	= n;
			sqlvar	= new GdsField[n];
		}

		#endregion
	}
}
