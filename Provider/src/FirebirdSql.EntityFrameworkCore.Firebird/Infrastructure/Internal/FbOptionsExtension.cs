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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Infrastructure.Internal
{
	public class FbOptionsExtension : RelationalOptionsExtension
	{
		private long? _serviceProviderHash;
		private bool? _explicitParameterTypes;

		public FbOptionsExtension()
		{ }

		public FbOptionsExtension(FbOptionsExtension copyFrom)
			: base(copyFrom)
		{
			_explicitParameterTypes = copyFrom._explicitParameterTypes;
		}

		protected override RelationalOptionsExtension Clone()
			=> new FbOptionsExtension(this);

		public virtual bool? ExplicitParameterTypes => _explicitParameterTypes;

		public virtual FbOptionsExtension WithExplicitParameterTypes(bool explicitParameterTypes)
		{
			var clone = (FbOptionsExtension)Clone();
			clone._explicitParameterTypes = explicitParameterTypes;
			return clone;
		}

		public override long GetServiceProviderHashCode()
		{
			if (_serviceProviderHash == null)
			{
				_serviceProviderHash = (base.GetServiceProviderHashCode() * 397) ^ (_explicitParameterTypes?.GetHashCode() ?? 0L);
			}
			return _serviceProviderHash.Value;
		}

		public override bool ApplyServices(IServiceCollection services)
		{
			services.AddEntityFrameworkFirebird();
			return true;
		}
	}
}
