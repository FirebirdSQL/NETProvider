using FirebirdSql.EntityFrameworkCore.Firebird.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Infrastructure
{
	public class FbDbContextOptionsBuilder : RelationalDbContextOptionsBuilder<FbDbContextOptionsBuilder, FbOptionsExtension>
	{
		public FbDbContextOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
			: base(optionsBuilder)
		{ }
	}
}
