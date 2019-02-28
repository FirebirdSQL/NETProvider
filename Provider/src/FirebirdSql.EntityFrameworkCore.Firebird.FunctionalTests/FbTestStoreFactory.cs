/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
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
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests
{
	public class FbTestStoreFactory : RelationalTestStoreFactory
	{
		public static FbTestStoreFactory Instance { get; } = new FbTestStoreFactory();

		static FbTestStoreFactory()
		{
			// See #14847 on EntityFrameworkCore.
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
		}

		public override TestStore Create(string storeName)
			=> throw new NotImplementedException();

		public override TestStore GetOrCreate(string storeName)
			=> new FbTestStore(storeName);

		public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
			=> serviceCollection.AddEntityFrameworkFirebird();
	}
}
