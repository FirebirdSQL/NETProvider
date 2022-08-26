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

public sealed class FbBackup : FbService
{
	public FbBackupFileCollection BackupFiles { get; }
	public bool Verbose { get; set; }
	public int Factor { get; set; }
	public string SkipData { get; set; }
	public FbBackupFlags Options { get; set; }
	public FbBackupRestoreStatistics Statistics { get; set; }

	public FbBackup(string connectionString = null)
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
				startSpb.Append(IscCodes.isc_action_svc_backup);
				startSpb.Append2(IscCodes.isc_spb_dbname, ConnectionStringOptions.Database);
				foreach (var file in BackupFiles)
				{
					startSpb.Append2(IscCodes.isc_spb_bkp_file, file.BackupFile);
					if (file.BackupLength.HasValue)
						startSpb.Append(IscCodes.isc_spb_bkp_length, (int)file.BackupLength);
				}
				if (Verbose)
					startSpb.Append(IscCodes.isc_spb_verbose);
				if (Factor > 0)
					startSpb.Append(IscCodes.isc_spb_bkp_factor, Factor);
				if (!string.IsNullOrEmpty(SkipData))
					startSpb.Append2(IscCodes.isc_spb_bkp_skip_data, SkipData);
				startSpb.Append(IscCodes.isc_spb_options, (int)Options);
				startSpb.Append2(IscCodes.isc_spb_bkp_stat, Statistics.BuildConfiguration());
				if (ConnectionStringOptions.ParallelWorkers > 0)
					startSpb.Append(IscCodes.isc_spb_bkp_parallel_workers, ConnectionStringOptions.ParallelWorkers);
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
				startSpb.Append(IscCodes.isc_action_svc_backup);
				startSpb.Append2(IscCodes.isc_spb_dbname, ConnectionStringOptions.Database);
				foreach (var file in BackupFiles)
				{
					startSpb.Append2(IscCodes.isc_spb_bkp_file, file.BackupFile);
					if (file.BackupLength.HasValue)
						startSpb.Append(IscCodes.isc_spb_bkp_length, (int)file.BackupLength);
				}
				if (Verbose)
					startSpb.Append(IscCodes.isc_spb_verbose);
				if (Factor > 0)
					startSpb.Append(IscCodes.isc_spb_bkp_factor, Factor);
				if (!string.IsNullOrEmpty(SkipData))
					startSpb.Append2(IscCodes.isc_spb_bkp_skip_data, SkipData);
				startSpb.Append(IscCodes.isc_spb_options, (int)Options);
				startSpb.Append2(IscCodes.isc_spb_bkp_stat, Statistics.BuildConfiguration());
				if (ConnectionStringOptions.ParallelWorkers > 0)
					startSpb.Append(IscCodes.isc_spb_bkp_parallel_workers, ConnectionStringOptions.ParallelWorkers);
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
