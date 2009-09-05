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
        #region · Fields ·

        private bool verbose;
        private int factor;
        private FbBackupFileCollection backupFiles;
        private FbBackupFlags options;

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

        public int Factor
        {
            get { return this.factor; }
            set { this.factor = value; }
        }

        public FbBackupFlags Options
        {
            get { return this.options; }
            set { this.options = value; }
        }

        #endregion

        #region · Constructors ·

        public FbBackup()
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

                this.StartSpb.Append(IscCodes.isc_action_svc_backup);
                this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);

                foreach (FbBackupFile file in backupFiles)
                {
                    this.StartSpb.Append(IscCodes.isc_spb_bkp_file, file.BackupFile);
					if (file.BackupLength.HasValue)
						this.StartSpb.Append(IscCodes.isc_spb_bkp_length, (int)file.BackupLength);
                }

                if (verbose)
                {
                    this.StartSpb.Append(IscCodes.isc_spb_verbose);
                }

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
                // Close
                this.Close();
            }
        }

        #endregion
    }
}
