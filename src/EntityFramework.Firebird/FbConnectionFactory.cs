/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Linq;

using FirebirdSql.Data.FirebirdClient;

namespace EntityFramework.Firebird;

public class FbConnectionFactory : IDbConnectionFactory
{
	public DbConnection CreateConnection(string nameOrConnectionString)
	{
		if (nameOrConnectionString == null)
			throw new ArgumentNullException(nameof(nameOrConnectionString));

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
