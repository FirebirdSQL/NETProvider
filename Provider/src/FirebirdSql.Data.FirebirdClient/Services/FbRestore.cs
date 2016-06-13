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
using FirebirdSql.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Services
{
	public sealed class FbRestore : FbService
	{
		private int? _pageSize;
		public int? PageSize
		{
			get { return _pageSize; }
			set
			{
				if (value.HasValue && !PageSizeHelper.IsValidPageSize((int)value))
					throw new InvalidOperationException("Invalid page size.");

				_pageSize = value;
			}
		}

		public FbBackupFileCollection BackupFiles { get; }
		public bool Verbose { get; set; }
		public int? PageBuffers { get; set; }
		public bool ReadOnly { get; set; }
		public string SkipData { get; set; }
		public FbRestoreFlags Options { get; set; }

		public FbRestore(string connectionString = null)
			: base(connectionString)
		{
			BackupFiles = new FbBackupFileCollection();
		}

		public void Execute()
		{
			try
			{
				StartSpb = new ServiceParameterBuffer();
				StartSpb.Append(IscCodes.isc_action_svc_restore);
				foreach (var bkpFile in BackupFiles)
				{
					StartSpb.Append(IscCodes.isc_spb_bkp_file, bkpFile.BackupFile);
				}
				StartSpb.Append(IscCodes.isc_spb_dbname, Database);
				if (Verbose)
					StartSpb.Append(IscCodes.isc_spb_verbose);
				if (PageBuffers.HasValue)
					StartSpb.Append(IscCodes.isc_spb_res_buffers, (int)PageBuffers);
				if (_pageSize.HasValue)
					StartSpb.Append(IscCodes.isc_spb_res_page_size, (int)_pageSize);
				StartSpb.Append(IscCodes.isc_spb_res_access_mode, (byte)(ReadOnly ? IscCodes.isc_spb_res_am_readonly : IscCodes.isc_spb_res_am_readwrite));
				if (!string.IsNullOrEmpty(SkipData))
					StartSpb.Append(IscCodes.isc_spb_res_skip_data, SkipData);
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
