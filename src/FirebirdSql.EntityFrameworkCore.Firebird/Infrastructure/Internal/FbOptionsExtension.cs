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
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Infrastructure.Internal;

public class FbOptionsExtension : RelationalOptionsExtension
{
	DbContextOptionsExtensionInfo _info;
	bool? _explicitParameterTypes;
	bool? _explicitStringLiteralTypes;

	public FbOptionsExtension()
	{ }

	public FbOptionsExtension(FbOptionsExtension copyFrom)
		: base(copyFrom)
	{
		_explicitParameterTypes = copyFrom._explicitParameterTypes;
		_explicitStringLiteralTypes = copyFrom._explicitStringLiteralTypes;
	}

	protected override RelationalOptionsExtension Clone()
		=> new FbOptionsExtension(this);

	public override void ApplyServices(IServiceCollection services)
		=> services.AddEntityFrameworkFirebird();

	public override DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);
	public virtual bool? ExplicitParameterTypes => _explicitParameterTypes;
	public virtual bool? ExplicitStringLiteralTypes => _explicitStringLiteralTypes;

	public virtual FbOptionsExtension WithExplicitParameterTypes(bool explicitParameterTypes)
	{
		var clone = (FbOptionsExtension)Clone();
		clone._explicitParameterTypes = explicitParameterTypes;
		return clone;
	}

	public virtual FbOptionsExtension WithExplicitStringLiteralTypes(bool explicitStringLiteralTypes)
	{
		var clone = (FbOptionsExtension)Clone();
		clone._explicitStringLiteralTypes = explicitStringLiteralTypes;
		return clone;
	}

	sealed class ExtensionInfo : RelationalExtensionInfo
	{
		int? _serviceProviderHash;

		public ExtensionInfo(IDbContextOptionsExtension extension)
			: base(extension)
		{ }

		new FbOptionsExtension Extension => (FbOptionsExtension)base.Extension;

		public override int GetServiceProviderHashCode()
		{
			return _serviceProviderHash ??= HashCode.Combine(base.GetServiceProviderHashCode(), Extension._explicitParameterTypes, Extension._explicitStringLiteralTypes);
		}

		public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
		{ }
	}
}
