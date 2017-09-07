using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using FirebirdClientConnection = FirebirdSql.Data.FirebirdClient.FbConnection;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal
{
	public class FbConnection : RelationalConnection, IFbConnection
	{
		public FbConnection(RelationalConnectionDependencies dependencies)
			: base(dependencies)
		{ }

		public override bool IsMultipleActiveResultSetsEnabled => true;

		protected override DbConnection CreateDbConnection()
			=> new FirebirdClientConnection(ConnectionString);
	}
}
