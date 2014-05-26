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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using FirebirdSql.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Services
{
	public sealed class FbGbakBackup : FbService
	{
		public FbBackupFlags Options { get; set; }
		public Stream OutputStream { get; set; }
		
		public void Execute()
		{
			try
			{
				// Configure Spb
				this.StartSpb = new ServiceParameterBuffer();

				this.StartSpb.Append(IscCodes.isc_action_svc_backup);
				this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);

				this.StartSpb.Append(IscCodes.isc_spb_bkp_file, "stdout");

				this.StartSpb.Append(IscCodes.isc_spb_options, (int)this.Options);

				this.Open();

				// Start execution
				this.StartTask();

				this.ReadOutput();
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

		void ReadOutput()
		{
			ArrayList info;
			while (true)
			{
				info = this.GetNext(new byte[] { IscCodes.isc_info_svc_to_eof });
				if (info.Count == 0)
					break;
				foreach (var item in info)
				{
					var buffer = item as byte[];
					OutputStream.Write(buffer, 0, buffer.Length);
				}
			}
		}
	}
}
