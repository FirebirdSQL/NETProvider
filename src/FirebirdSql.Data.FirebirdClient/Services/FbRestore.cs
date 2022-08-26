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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Services;

public sealed class FbRestore : FbService
{
	private int? _pageSize;
	public int? PageSize
	{
		get { return _pageSize; }
		set
		{
			if (value is int v && !SizeHelper.IsValidPageSize(v))
				throw SizeHelper.InvalidSizeException("page size");

			_pageSize = value;
		}
	}

	public FbBackupFileCollection BackupFiles { get; }
	public bool Verbose { get; set; }
	public int? PageBuffers { get; set; }
	public bool ReadOnly { get; set; }
	public string SkipData { get; set; }
	public FbRestoreFlags Options { get; set; }
	public FbBackupRestoreStatistics Statistics { get; set; }

	public FbRestore(string connectionString = null)
		: base(connectionString)
	{
		BackupFiles = new FbBackupFileCollection();
	}

	public void Execute()
	{
		EnsureDatabase();

		try
		{
			try
			{
				Open();
				var startSpb = new ServiceParameterBuffer2(Service.ParameterBufferEncoding);
				startSpb.Append(IscCodes.isc_action_svc_restore);
				foreach (var bkpFile in BackupFiles)
				{
					startSpb.Append2(IscCodes.isc_spb_bkp_file, bkpFile.BackupFile);
				}
				startSpb.Append2(IscCodes.isc_spb_dbname, ConnectionStringOptions.Database);
				if (Verbose)
					startSpb.Append(IscCodes.isc_spb_verbose);
				if (PageBuffers.HasValue)
					startSpb.Append(IscCodes.isc_spb_res_buffers, (int)PageBuffers);
				if (_pageSize.HasValue)
					startSpb.Append(IscCodes.isc_spb_res_page_size, (int)_pageSize);
				startSpb.Append(IscCodes.isc_spb_res_access_mode, (byte)(ReadOnly ? IscCodes.isc_spb_res_am_readonly : IscCodes.isc_spb_res_am_readwrite));
				if (!string.IsNullOrEmpty(SkipData))
					startSpb.Append2(IscCodes.isc_spb_res_skip_data, SkipData);
				startSpb.Append(IscCodes.isc_spb_options, (int)Options);
				startSpb.Append2(IscCodes.isc_spb_res_stat, Statistics.BuildConfiguration());
				if (ConnectionStringOptions.ParallelWorkers > 0)
					startSpb.Append(IscCodes.isc_spb_res_parallel_workers, ConnectionStringOptions.ParallelWorkers);
				StartTask(startSpb);
				if (Verbose)
				{
					ProcessServiceOutput(new ServiceParameterBuffer2(Service.ParameterBufferEncoding));
				}
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
	public async Task ExecuteAsync(CancellationToken cancellationToken = default)
	{
		EnsureDatabase();

		try
		{
			try
			{
				await OpenAsync(cancellationToken).ConfigureAwait(false);
				var startSpb = new ServiceParameterBuffer2(Service.ParameterBufferEncoding);
				startSpb.Append(IscCodes.isc_action_svc_restore);
				foreach (var bkpFile in BackupFiles)
				{
					startSpb.Append2(IscCodes.isc_spb_bkp_file, bkpFile.BackupFile);
				}
				startSpb.Append2(IscCodes.isc_spb_dbname, ConnectionStringOptions.Database);
				if (Verbose)
					startSpb.Append(IscCodes.isc_spb_verbose);
				if (PageBuffers.HasValue)
					startSpb.Append(IscCodes.isc_spb_res_buffers, (int)PageBuffers);
				if (_pageSize.HasValue)
					startSpb.Append(IscCodes.isc_spb_res_page_size, (int)_pageSize);
				startSpb.Append(IscCodes.isc_spb_res_access_mode, (byte)(ReadOnly ? IscCodes.isc_spb_res_am_readonly : IscCodes.isc_spb_res_am_readwrite));
				if (!string.IsNullOrEmpty(SkipData))
					startSpb.Append2(IscCodes.isc_spb_res_skip_data, SkipData);
				startSpb.Append(IscCodes.isc_spb_options, (int)Options);
				startSpb.Append2(IscCodes.isc_spb_res_stat, Statistics.BuildConfiguration());
				if (ConnectionStringOptions.ParallelWorkers > 0)
					startSpb.Append(IscCodes.isc_spb_res_parallel_workers, ConnectionStringOptions.ParallelWorkers);
				await StartTaskAsync(startSpb, cancellationToken).ConfigureAwait(false);
				if (Verbose)
				{
					await ProcessServiceOutputAsync(new ServiceParameterBuffer2(Service.ParameterBufferEncoding), cancellationToken).ConfigureAwait(false);
				}
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
}
