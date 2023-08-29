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
using System.Reflection;
using FirebirdSql.EntityFrameworkCore.Firebird.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Extensions;

/// <summary>
///		FirebirdSQL specific extension methods for <see cref="DatabaseFacade"/>.
/// </summary>
public static class FbDatabaseFacadeExtensions
{
	/// <summary>
	///		<para>
	///			Returns true if the database provider currently in use is the FirebirdSQL provider.
	///		</para>
	///		<para>
	///			This method can only be used after the <see cref="DbContext" /> has been configured because
	///			it is only then that the provider is known. This means that this method cannot be used
	///			in <see cref="DbContext.OnConfiguring" /> because this is where application code sets the
	///			provider to use as part of configuring the context.
	///		</para>
	/// </summary>
	/// <param name="database">
	///		The facade from <see cref="DbContext.Database" />.
	/// </param>
	/// <returns>
	///		True if FirebirdSQL is being used; false otherwise.
	/// </returns>
	public static bool IsFirebird(this DatabaseFacade database)
		=> database.ProviderName.Equals(typeof(FbOptionsExtension).GetTypeInfo().Assembly.GetName().Name, StringComparison.Ordinal);
}
