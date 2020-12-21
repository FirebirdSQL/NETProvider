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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Services
{
	public sealed class FbServerProperties : FbService
	{
		public FbServerProperties(string connectionString = null)
			: base(connectionString)
		{ }

		public int GetVersion() => GetVersionImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<int> GetVersionAsync(CancellationToken cancellationToken = default) => GetVersionImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<int> GetVersionImpl(AsyncWrappingCommonArgs async)
		{
			return GetInt32(IscCodes.isc_info_svc_version, async);
		}

		public string GetServerVersion() => GetServerVersionImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<string> GetServerVersionAsync(CancellationToken cancellationToken = default) => GetServerVersionImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<string> GetServerVersionImpl(AsyncWrappingCommonArgs async)
		{
			return GetString(IscCodes.isc_info_svc_server_version, async);
		}

		public string GetImplementation() => GetImplementationImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<string> GetImplementationAsync(CancellationToken cancellationToken = default) => GetImplementationImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<string> GetImplementationImpl(AsyncWrappingCommonArgs async)
		{
			return GetString(IscCodes.isc_info_svc_implementation, async);
		}

		public string GetRootDirectory() => GetRootDirectoryImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<string> GetRootDirectoryAsync(CancellationToken cancellationToken = default) => GetRootDirectoryImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<string> GetRootDirectoryImpl(AsyncWrappingCommonArgs async)
		{
			return GetString(IscCodes.isc_info_svc_get_env, async);
		}

		public string GetLockManager() => GetLockManagerImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<string> GetLockManagerAsync(CancellationToken cancellationToken = default) => GetLockManagerImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<string> GetLockManagerImpl(AsyncWrappingCommonArgs async)
		{
			return GetString(IscCodes.isc_info_svc_get_env_lock, async);
		}

		public string GetMessageFile() => GetMessageFileImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<string> GetMessageFileAsync(CancellationToken cancellationToken = default) => GetMessageFileImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<string> GetMessageFileImpl(AsyncWrappingCommonArgs async)
		{
			return GetString(IscCodes.isc_info_svc_get_env_msg, async);
		}

		public FbDatabasesInfo GetDatabasesInfo() => GetDatabasesInfoImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<FbDatabasesInfo> GetDatabasesInfoAsync(CancellationToken cancellationToken = default) => GetDatabasesInfoImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task<FbDatabasesInfo> GetDatabasesInfoImpl(AsyncWrappingCommonArgs async)
		{
			return (FbDatabasesInfo)(await GetInfo(IscCodes.isc_info_svc_svr_db_info, async).ConfigureAwait(false)).FirstOrDefault() ?? new FbDatabasesInfo();
		}

		public FbServerConfig GetServerConfig() => GetServerConfigImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<FbServerConfig> GetServerConfigAsync(CancellationToken cancellationToken = default) => GetServerConfigImpl(new AsyncWrappingCommonArgs(false, cancellationToken));
		private async Task<FbServerConfig> GetServerConfigImpl(AsyncWrappingCommonArgs async)
		{
			return (FbServerConfig)(await GetInfo(IscCodes.isc_info_svc_get_config, async).ConfigureAwait(false)).FirstOrDefault() ?? new FbServerConfig();
		}

		private async Task<string> GetString(int item, AsyncWrappingCommonArgs async)
		{
			return (string)(await GetInfo(item, async).ConfigureAwait(false)).FirstOrDefault();
		}

		private async Task<int> GetInt32(int item, AsyncWrappingCommonArgs async)
		{
			return (int)(await GetInfo(item, async).ConfigureAwait(false)).FirstOrDefault();
		}

		private Task<List<object>> GetInfo(int item, AsyncWrappingCommonArgs async)
		{
			return GetInfo(new byte[] { (byte)item }, async);
		}

		private Task<List<object>> GetInfo(byte[] items, AsyncWrappingCommonArgs async)
		{
			return Query(items, EmptySpb, async);
		}

		public static Version ParseServerVersion(string version)
		{
			var m = Regex.Match(version, @"\w{2}-\w(\d+\.\d+\.\d+\.\d+)");
			if (!m.Success)
				return null;
			return new Version(m.Groups[1].Value);
		}
	}
}
