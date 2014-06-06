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
 *	Copyright (c) 2013-2014 Jiri Cincura (jiri@cincura.net)
 *	All Rights Reserved.
 *	
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.EntityFramework6
{
	public class FbConnectionFactory : IDbConnectionFactory
	{
		public DbConnection CreateConnection(string nameOrConnectionString)
		{
			if (nameOrConnectionString == null)
				throw new ArgumentNullException("nameOrConnectionString cannot be null.");

			if (nameOrConnectionString.Contains('='))
			{
				return new FbConnection(nameOrConnectionString);
			}
			else
			{
				var configuration = ConfigurationManager.ConnectionStrings[nameOrConnectionString];
				if (configuration == null)
					throw new ArgumentException("Specified connection string name cannot be found.");
				return new FbConnection(configuration.ConnectionString);
			}
		}
	}
}