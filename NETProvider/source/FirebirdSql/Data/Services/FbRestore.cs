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
		#region · Fields ·

		private FbBackupFileCollection backupFiles;
		private FbRestoreFlags options;
		private bool verbose;
		private int? pageBuffers;
		private int? pageSize;

		#endregion

		#region · Properties ·

		public FbBackupFileCollection BackupFiles
		{
			get { return this.backupFiles; }
		}

		public bool Verbose
		{
			get { return this.verbose; }
			set { this.verbose = value; }
		}

		public int? PageBuffers
		{
			get { return this.pageBuffers; }
			set { this.pageBuffers = value; }
		}

		public int? PageSize
		{
			get { return this.pageSize; }
			set
			{
				if (value != 1024 && value != 2048 &&
					value != 4096 && value != 8192 &&
					value != 16384)
				{
					throw new InvalidOperationException("Invalid page size.");
				}
				this.pageSize = value;
			}
		}

		public FbRestoreFlags Options
		{
			get { return this.options; }
			set { this.options = value; }
		}

		#endregion

		#region · Constructors ·

		public FbRestore()
			: base()
		{
			this.backupFiles = new FbBackupFileCollection();
		}

		#endregion

		#region · Methods ·

		public void Execute()
		{
			try
			{
				// Configure Spb
				this.StartSpb = new ServiceParameterBuffer();

				this.StartSpb.Append(IscCodes.isc_action_svc_restore);

				foreach (FbBackupFile bkpFile in backupFiles)
				{
					this.StartSpb.Append(IscCodes.isc_spb_bkp_file, bkpFile.BackupFile);
				}

				this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);

				if (this.verbose)
				{
					this.StartSpb.Append(IscCodes.isc_spb_verbose);
				}

				if (this.pageBuffers.HasValue)
					this.StartSpb.Append(IscCodes.isc_spb_res_buffers, (int)this.pageBuffers);
				if (this.pageSize.HasValue)
					this.StartSpb.Append(IscCodes.isc_spb_res_page_size, (int)this.pageSize);
				this.StartSpb.Append(IscCodes.isc_spb_options, (int)this.options);

				// Start execution
				this.StartTask();

				if (this.verbose)
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
