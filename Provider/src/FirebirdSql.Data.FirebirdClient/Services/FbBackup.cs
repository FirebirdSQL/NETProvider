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

		public FbBackup(string connectionString = null)
			: base(connectionString)
		{
			BackupFiles = new FbBackupFileCollection();
		}

		public void Execute()
		{
			try
			{
				StartSpb = new ServiceParameterBuffer();
				StartSpb.Append(IscCodes.isc_action_svc_backup);
				StartSpb.Append(IscCodes.isc_spb_dbname, Database);
				foreach (var file in BackupFiles)
				{
					StartSpb.Append(IscCodes.isc_spb_bkp_file, file.BackupFile);
					if (file.BackupLength.HasValue)
						StartSpb.Append(IscCodes.isc_spb_bkp_length, (int)file.BackupLength);
				}
				if (Verbose)
					StartSpb.Append(IscCodes.isc_spb_verbose);
				if (Factor > 0)
					StartSpb.Append(IscCodes.isc_spb_bkp_factor, Factor);
				if (!string.IsNullOrEmpty(SkipData))
					StartSpb.Append(IscCodes.isc_spb_bkp_skip_data, SkipData);
				StartSpb.Append(IscCodes.isc_spb_options, (int)Options);

				Open();
				StartTask();
				if (Verbose)
				{
					ProcessServiceOutput();
				}
			}
			catch (Exception ex)
			{
				throw new FbException(ex.Message, ex);
			}
			finally
			{
				Close();
			}
		}
	}
}
