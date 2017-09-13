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

using FirebirdSql.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Services
{
	public sealed class FbNBackup : FbService
	{
		private int _level;
		public int Level
		{
			get { return _level; }
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException();
				_level = value;
			}
		}
		public string BackupFile { get; set; }
		public bool DirectIO { get; set; }
		public FbNBackupFlags Options { get; set; }

		public FbNBackup(string connectionString = null)
			: base(connectionString)
		{ }

		public void Execute()
		{
			try
			{
				StartSpb = new ServiceParameterBuffer();
				StartSpb.Append(IscCodes.isc_action_svc_nbak);
				StartSpb.Append(IscCodes.isc_spb_dbname, Database);
				StartSpb.Append(IscCodes.isc_spb_nbk_level, _level);
				StartSpb.Append(IscCodes.isc_spb_nbk_file, BackupFile);
				StartSpb.Append(IscCodes.isc_spb_nbk_direct, DirectIO ? "ON" : "OFF");
				StartSpb.Append(IscCodes.isc_spb_options, (int)Options);

				Open();
				StartTask();
				ProcessServiceOutput();
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
