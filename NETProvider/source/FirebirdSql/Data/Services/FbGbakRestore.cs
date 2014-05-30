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
 *	Copyright (c) 2014 Jiri Cincura (jiri@cincura.net)
 *	All Rights Reserved.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FirebirdSql.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Services
{
	public class FbGbakRestore : FbService
	{
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

		public Stream InputStream { get; set; }
		public bool Verbose { get; set; }
		public int? PageBuffers { get; set; }
		public FbRestoreFlags Options { get; set; }


		public void Execute()
		{
			try
			{
				// Configure Spb
				this.StartSpb = new ServiceParameterBuffer();

				this.StartSpb.Append(IscCodes.isc_action_svc_restore);

				this.StartSpb.Append(IscCodes.isc_spb_bkp_file, "stdin");

				this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);

				if (this.Verbose)
				{
					this.StartSpb.Append(IscCodes.isc_spb_verbose);
				}

				if (this.PageBuffers.HasValue)
					this.StartSpb.Append(IscCodes.isc_spb_res_buffers, (int)this.PageBuffers);
				if (this.pageSize.HasValue)
					this.StartSpb.Append(IscCodes.isc_spb_res_page_size, (int)this.pageSize);
				this.StartSpb.Append(IscCodes.isc_spb_options, (int)this.Options);

				this.Open();

				// Start execution
				this.StartTask();

				//if (this.Verbose)
				//{
				//	this.ProcessServiceOutput();
				//}

				Do();
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

		void Do()
		{
			this.Query(new byte[] { IscCodes.isc_info_svc_stdin }, (_, x) =>
			{
				System.Diagnostics.Debugger.Break();
			});
		}
	}
}
