using System.Reflection;
using FirebirdSql.EntityFrameworkCore.Firebird.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Extensions;

using System;

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
