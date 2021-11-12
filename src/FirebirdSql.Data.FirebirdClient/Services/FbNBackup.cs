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
using System.Threading;
using System.Threading.Tasks;
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
			EnsureDatabase();

			try
			{
				Open();
				var startSpb = new ServiceParameterBuffer2();
				startSpb.Append(IscCodes.isc_action_svc_nbak);
				startSpb.Append2(IscCodes.isc_spb_dbname, Database, SpbFilenameEncoding);
				startSpb.Append(IscCodes.isc_spb_nbk_level, _level);
				startSpb.Append2(IscCodes.isc_spb_nbk_file, BackupFile, SpbFilenameEncoding);
				startSpb.Append2(IscCodes.isc_spb_nbk_direct, DirectIO ? "ON" : "OFF");
				startSpb.Append(IscCodes.isc_spb_options, (int)Options);
				StartTask(startSpb);
				ProcessServiceOutput(ServiceParameterBufferBase.Empty);
			}
			catch (Exception ex)
			{
				throw FbException.Create(ex);
			}
			finally
			{
				Close();
			}
		}
		public async Task ExecuteAsync(CancellationToken cancellationToken = default)
		{
			EnsureDatabase();

			try
			{
				await OpenAsync(cancellationToken).ConfigureAwait(false);
				var startSpb = new ServiceParameterBuffer2();
				startSpb.Append(IscCodes.isc_action_svc_nbak);
				startSpb.Append2(IscCodes.isc_spb_dbname, Database, SpbFilenameEncoding);
				startSpb.Append(IscCodes.isc_spb_nbk_level, _level);
				startSpb.Append2(IscCodes.isc_spb_nbk_file, BackupFile, SpbFilenameEncoding);
				startSpb.Append2(IscCodes.isc_spb_nbk_direct, DirectIO ? "ON" : "OFF");
				startSpb.Append(IscCodes.isc_spb_options, (int)Options);
				await StartTaskAsync(startSpb, cancellationToken).ConfigureAwait(false);
				await ProcessServiceOutputAsync(ServiceParameterBufferBase.Empty, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw FbException.Create(ex);
			}
			finally
			{
				await CloseAsync(cancellationToken).ConfigureAwait(false);
			}
		}
	}
}
