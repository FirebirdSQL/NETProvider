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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Services;

public sealed class FbServerProperties : FbService
{
	public FbServerProperties(string connectionString = null)
		: base(connectionString)
	{ }

	public int GetVersion()
	{
		return GetInt32(IscCodes.isc_info_svc_version);
	}
	public Task<int> GetVersionAsync(CancellationToken cancellationToken = default)
	{
		return GetInt32Async(IscCodes.isc_info_svc_version, cancellationToken);
	}

	public string GetServerVersion()
	{
		return GetString(IscCodes.isc_info_svc_server_version);
	}
	public Task<string> GetServerVersionAsync(CancellationToken cancellationToken = default)
	{
		return GetStringAsync(IscCodes.isc_info_svc_server_version, cancellationToken);
	}

	public string GetImplementation()
	{
		return GetString(IscCodes.isc_info_svc_implementation);
	}
	public Task<string> GetImplementationAsync(CancellationToken cancellationToken = default)
	{
		return GetStringAsync(IscCodes.isc_info_svc_implementation, cancellationToken);
	}

	public string GetRootDirectory()
	{
		return GetString(IscCodes.isc_info_svc_get_env);
	}
	public Task<string> GetRootDirectoryAsync(CancellationToken cancellationToken = default)
	{
		return GetStringAsync(IscCodes.isc_info_svc_get_env, cancellationToken);
	}

	public string GetLockManager()
	{
		return GetString(IscCodes.isc_info_svc_get_env_lock);
	}
	public Task<string> GetLockManagerAsync(CancellationToken cancellationToken = default)
	{
		return GetStringAsync(IscCodes.isc_info_svc_get_env_lock, cancellationToken);
	}

	public string GetMessageFile()
	{
		return GetString(IscCodes.isc_info_svc_get_env_msg);
	}
	public Task<string> GetMessageFileAsync(CancellationToken cancellationToken = default)
	{
		return GetStringAsync(IscCodes.isc_info_svc_get_env_msg, cancellationToken);
	}

	public FbDatabasesInfo GetDatabasesInfo()
	{
		return (FbDatabasesInfo)(GetInfo(IscCodes.isc_info_svc_svr_db_info)).FirstOrDefault() ?? new FbDatabasesInfo();
	}
	public async Task<FbDatabasesInfo> GetDatabasesInfoAsync(CancellationToken cancellationToken = default)
	{
		return (FbDatabasesInfo)(await GetInfoAsync(IscCodes.isc_info_svc_svr_db_info, cancellationToken).ConfigureAwait(false)).FirstOrDefault() ?? new FbDatabasesInfo();
	}

	public FbServerConfig GetServerConfig()
	{
		return (FbServerConfig)(GetInfo(IscCodes.isc_info_svc_get_config)).FirstOrDefault() ?? new FbServerConfig();
	}
	public async Task<FbServerConfig> GetServerConfigAsync(CancellationToken cancellationToken = default)
	{
		return (FbServerConfig)(await GetInfoAsync(IscCodes.isc_info_svc_get_config, cancellationToken).ConfigureAwait(false)).FirstOrDefault() ?? new FbServerConfig();
	}

	private string GetString(int item)
	{
		return (string)(GetInfo(item)).FirstOrDefault();
	}
	private async Task<string> GetStringAsync(int item, CancellationToken cancellationToken = default)
	{
		return (string)(await GetInfoAsync(item, cancellationToken).ConfigureAwait(false)).FirstOrDefault();
	}

	private int GetInt32(int item)
	{
		return (int)(GetInfo(item)).FirstOrDefault();
	}
	private async Task<int> GetInt32Async(int item, CancellationToken cancellationToken = default)
	{
		return (int)(await GetInfoAsync(item, cancellationToken).ConfigureAwait(false)).FirstOrDefault();
	}

	private List<object> GetInfo(int item)
	{
		return GetInfo(new byte[] { (byte)item });
	}
	private Task<List<object>> GetInfoAsync(int item, CancellationToken cancellationToken = default)
	{
		return GetInfoAsync(new byte[] { (byte)item }, cancellationToken);
	}

	private List<object> GetInfo(byte[] items)
	{
		try
		{
			try
			{
				Open();
				return Query(items, new ServiceParameterBuffer2(Service.ParameterBufferEncoding));
			}
			finally
			{
				Close();
			}
		}
		catch (Exception ex)
		{
			throw FbException.Create(ex);
		}
	}
	private async Task<List<object>> GetInfoAsync(byte[] items, CancellationToken cancellationToken = default)
	{
		try
		{
			try
			{
				await OpenAsync(cancellationToken).ConfigureAwait(false);
				return await QueryAsync(items, new ServiceParameterBuffer2(Service.ParameterBufferEncoding), cancellationToken).ConfigureAwait(false);
			}
			finally
			{
				await CloseAsync(cancellationToken).ConfigureAwait(false);
			}
		}
		catch (Exception ex)
		{
			throw FbException.Create(ex);
		}
	}

	public static Version ParseServerVersion(string version)
	{
		var m = Regex.Match(version, @"\w{2}-\w(\d+\.\d+\.\d+\.\d+)");
		if (!m.Success)
			return null;
		return new Version(m.Groups[1].Value);
	}
}
