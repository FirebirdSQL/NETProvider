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
 *	Copyright (c) 2013 Jiri Cincura (jiri@cincura.net)
 *	All Rights Reserved.
 */

using System;
using System.Collections.Generic;

using FirebirdSql.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Services
{
	public sealed class FbNRestore : FbService
	{
		#region Properties
		public IEnumerable<string> BackupFiles { get; set; }
		public bool DirectIO { get; set; }
		#endregion

		#region Constructors
		public FbNRestore(string connectionString = null)
			: base(connectionString)
		{ }
		#endregion

		#region Methods
		public void Execute()
		{
			try
			{
				// Configure Spb
				this.StartSpb = new ServiceParameterBuffer();

				this.StartSpb.Append(IscCodes.isc_action_svc_nrest);
				this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);

				foreach (var file in this.BackupFiles)
				{
					this.StartSpb.Append(IscCodes.isc_spb_nbk_file, file);
				}

				this.StartSpb.Append(IscCodes.isc_spb_nbk_direct, this.DirectIO ? "ON" : "OFF");

				this.Open();

				// Start execution
				this.StartTask();

				this.ProcessServiceOutput();
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
