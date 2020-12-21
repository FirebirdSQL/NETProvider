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
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Services
{
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

		public void Execute() => ExecuteImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task ExecuteAsync(CancellationToken cancellationToken = default) => ExecuteImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task ExecuteImpl(AsyncWrappingCommonArgs async)
		{
			EnsureDatabase();

			try
			{
				await Open(async).ConfigureAwait(false);
				var startSpb = new ServiceParameterBuffer();
				startSpb.Append(IscCodes.isc_action_svc_backup);
				startSpb.Append(IscCodes.isc_spb_dbname, Database, SpbFilenameEncoding);
				foreach (var file in BackupFiles)
				{
					startSpb.Append(IscCodes.isc_spb_bkp_file, file.BackupFile, SpbFilenameEncoding);
					if (file.BackupLength.HasValue)
						startSpb.Append(IscCodes.isc_spb_bkp_length, (int)file.BackupLength);
				}
				if (Verbose)
					startSpb.Append(IscCodes.isc_spb_verbose);
				if (Factor > 0)
					startSpb.Append(IscCodes.isc_spb_bkp_factor, Factor);
				if (!string.IsNullOrEmpty(SkipData))
					startSpb.Append(IscCodes.isc_spb_bkp_skip_data, SkipData);
				startSpb.Append(IscCodes.isc_spb_options, (int)Options);
				startSpb.Append(IscCodes.isc_spb_bkp_stat, Statistics.BuildConfiguration());
				await StartTask(startSpb, async).ConfigureAwait(false);
				if (Verbose)
				{
					await ProcessServiceOutput(EmptySpb, async).ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				throw new FbException(ex.Message, ex);
			}
			finally
			{
				await Close(async).ConfigureAwait(false);
			}
		}
	}
}
