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

using FirebirdSql.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Services
{
	public sealed class FbNBackup : FbService
	{
		#region Properties
		private int level;
		public int Level
		{
			get { return this.level; }
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException();
				this.level = value;
			}
		}
		public string BackupFile { get; set; }
		public bool DirectIO { get; set; }
		public FbNBackupFlags Options { get; set; }
		#endregion

		#region Constructors
		public FbNBackup(string connectionString = null)
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

				this.StartSpb.Append(IscCodes.isc_action_svc_nbak);
				this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);

				this.StartSpb.Append(IscCodes.isc_spb_nbk_level, this.level);
				this.StartSpb.Append(IscCodes.isc_spb_nbk_file, this.BackupFile);

				this.StartSpb.Append(IscCodes.isc_spb_nbk_direct, this.DirectIO ? "ON" : "OFF");

				this.StartSpb.Append(IscCodes.isc_spb_options, (int)this.Options);

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
