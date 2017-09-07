using FirebirdSql.EntityFrameworkCore.Firebird.Extensions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Infrastructure.Internal
{
	public class FbOptionsExtension : RelationalOptionsExtension
	{
		public FbOptionsExtension()
		{ }

		public FbOptionsExtension(FbOptionsExtension copyFrom)
			: base(copyFrom)
		{ }

		public override bool ApplyServices(IServiceCollection services)
		{
			services.AddEntityFrameworkFirebird();
			return true;
		}

		protected override RelationalOptionsExtension Clone()
			=> new FbOptionsExtension(this);
	}
}
