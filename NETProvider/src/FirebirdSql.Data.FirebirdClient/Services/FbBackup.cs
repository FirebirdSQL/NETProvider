/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *	All Rights Reserved.
 *  
 *  Contributors:
 *   Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Collections;

using FirebirdSql.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Services
{
	public sealed class FbBackup : FbService
	{
		#region Properties

		private FbBackupFileCollection backupFiles;
		public FbBackupFileCollection BackupFiles
		{
			get { return this.backupFiles; }
		}

		public bool Verbose { get; set; }
		public int Factor { get; set; }
		public FbBackupFlags Options { get; set; }

		#endregion

		#region Constructors

		public FbBackup(string connectionString = null)
			: base(connectionString)
		{
			this.backupFiles = new FbBackupFileCollection();
		}

		#endregion

		#region Methods

		public void Execute()
		{
			try
			{
				// Configure Spb
				this.StartSpb = new ServiceParameterBuffer();

				this.StartSpb.Append(IscCodes.isc_action_svc_backup);
				this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);

				foreach (FbBackupFile file in backupFiles)
				{
					this.StartSpb.Append(IscCodes.isc_spb_bkp_file, file.BackupFile);
					if (file.BackupLength.HasValue)
						this.StartSpb.Append(IscCodes.isc_spb_bkp_length, (int)file.BackupLength);
				}

				if (this.Verbose)
				{
					this.StartSpb.Append(IscCodes.isc_spb_verbose);
				}

				this.StartSpb.Append(IscCodes.isc_spb_options, (int)this.Options);

				this.Open();

				// Start execution
				this.StartTask();

				if (this.Verbose)
				{
					this.ProcessServiceOutput();
				}
			}
			catch (Exception ex)
			{
				throw new FbException(ex.Message, ex);
			}
			finally
			{
				// Close
				this.Close();
			}
		}

		#endregion
	}
}
