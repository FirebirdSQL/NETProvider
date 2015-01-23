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
 *	Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *	All Rights Reserved.
 * 
 *  Contributors:
 *   Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Collections.Generic;

namespace FirebirdSql.Data.Services
{
	public struct FbDatabasesInfo
	{
		#region Fields

		private int connectionCount;
		private List<string> databases;

		#endregion

		#region Properties

		public int ConnectionCount
		{
			get { return this.connectionCount; }
			set { this.connectionCount = value; }
		}

		public List<string> Databases
		{
			get
			{
				if (this.databases == null)
				{
					this.databases = new List<string>();
				}
				return this.databases;
			}
		}

		#endregion
	}
}
