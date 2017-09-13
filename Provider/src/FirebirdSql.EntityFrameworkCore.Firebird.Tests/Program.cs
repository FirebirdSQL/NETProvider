using System.Linq;
using System.Reflection;
using NUnitLite;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Tests
{
	static class Program
	{
		static int Main(string[] args)
		{
			args = args?.Any() ?? false
				? args
				: new[] { "--noresult" };
			return new AutoRun(Assembly.GetExecutingAssembly()).Execute(args);
		}
	}
}
