using FirebirdSql.EntityFrameworkCore.Firebird.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Internal
{
	public class FbOptions : IFbOptions
	{
		public void Initialize(IDbContextOptions options)
		{
			var fbOptions = GetOptions(options);
		}

		public void Validate(IDbContextOptions options)
		{
			var fbOptions = GetOptions(options);
		}

		static FbOptionsExtension GetOptions(IDbContextOptions options)
			=> options.FindExtension<FbOptionsExtension>() ?? new FbOptionsExtension();
	}
}
