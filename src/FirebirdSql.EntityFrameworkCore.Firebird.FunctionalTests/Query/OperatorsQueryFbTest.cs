﻿/*
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
using System.Threading.Tasks;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Helpers;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class OperatorsQueryFbTest : OperatorsQueryTestBase
{
	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Concat_and_json_scalar(bool async)
	{
		return base.Concat_and_json_scalar(async);
	}

	protected override Task<ContextFactory<TContext>> InitializeAsync<TContext>(Action<ModelBuilder> onModelCreating = null, Action<DbContextOptionsBuilder> onConfiguring = null, Func<IServiceCollection, IServiceCollection> addServices = null, Action<ModelConfigurationBuilder> configureConventions = null, Func<TContext, Task> seed = null, Func<string, bool> shouldLogCategory = null, Func<Task<TestStore>> createTestStore = null, bool usePooling = true, bool useServiceProvider = true)
	{
		return base.InitializeAsync(
			modelBuilder =>
			{
				ModelHelpers.SetStringLengths(modelBuilder);
				onModelCreating?.Invoke(modelBuilder);
			},
			onConfiguring, addServices, configureConventions, seed, shouldLogCategory, createTestStore, usePooling, useServiceProvider);
	}

	protected override ITestStoreFactory TestStoreFactory => FbTestStoreFactory.Instance;
}
