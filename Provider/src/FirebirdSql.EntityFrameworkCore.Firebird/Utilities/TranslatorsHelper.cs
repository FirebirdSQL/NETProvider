using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Utilities
{
	public class TranslatorsHelper
	{
		public static IEnumerable<Type> GetTranslators<TInterface>()
		{
			return Assembly.GetExecutingAssembly().GetTypes()
				.Where(t => t.GetInterfaces().Any(i => i == typeof(TInterface)) && t.GetConstructors().Any(c => c.GetParameters().Length == 0));
		}
	}
}
