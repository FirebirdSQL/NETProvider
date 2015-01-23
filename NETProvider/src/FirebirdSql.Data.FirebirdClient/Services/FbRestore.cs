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
	public sealed class FbRestore : FbService
	{
		#region Properties

		private FbBackupFileCollection backupFiles;
		public FbBackupFileCollection BackupFiles
		{
			get { return this.backupFiles; }
		}

		private int? pageSize;
		public int? PageSize
		{
			get { return this.pageSize; }
			set
			{
				if (value.HasValue && !PageSizeHelper.IsValidPageSize((int)value))
					throw new InvalidOperationException("Invalid page size.");

				this.pageSize = value;
			}
		}

		public bool Verbose { get; set; }
		public int? PageBuffers { get; set; }
		public bool ReadOnly { get; set; }
		public FbRestoreFlags Options { get; set; }

		#endregion

		#region Constructors

		public FbRestore(string connectionString = null)
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
				this.StartSpb = new ServiceParameterBuffer();

				this.StartSpb.Append(IscCodes.isc_action_svc_restore);

				foreach (FbBackupFile bkpFile in this.backupFiles)
				{
					this.StartSpb.Append(IscCodes.isc_spb_bkp_file, bkpFile.BackupFile);
				}

				this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);

				if (this.Verbose)
				{
					this.StartSpb.Append(IscCodes.isc_spb_verbose);
				}

				if (this.PageBuffers.HasValue)
					this.StartSpb.Append(IscCodes.isc_spb_res_buffers, (int)this.PageBuffers);
				if (this.pageSize.HasValue)
					this.StartSpb.Append(IscCodes.isc_spb_res_page_size, (int)this.pageSize);
				this.StartSpb.Append(IscCodes.isc_spb_res_access_mode, (byte)(this.ReadOnly ? IscCodes.isc_spb_res_am_readonly : IscCodes.isc_spb_res_am_readwrite));
				this.StartSpb.Append(IscCodes.isc_spb_options, (int)this.Options);

				this.Open();

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
				this.Close();
			}
		}

		#endregion
	}
}
