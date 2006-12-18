/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.ibphoenix.com/main.nfs?a=ibphoenix&l=;PAGES;NAME='ibp_idpl'
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Collections;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Firebird.Services
{
	#region Enumerations

	/// <include file='Doc/en_EN/FbBackup.xml' path='doc/enum[@name="FbBackupFlags"]/overview/*'/>
	[Flags]
	public enum FbBackupFlags
	{
		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/enum[@name="FbBackupFlags"]/field[@name="IgnoreChecksums"]/*'/>
		IgnoreChecksums		= 0x01,
		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/enum[@name="FbBackupFlags"]/field[@name="IgnoreLimbo"]/*'/>
		IgnoreLimbo			= 0x02,
		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/enum[@name="FbBackupFlags"]/field[@name="MetaDataOnly"]/*'/>
		MetaDataOnly		= 0x04,
		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/enum[@name="FbBackupFlags"]/field[@name="NoGarbageCollect"]/*'/>
		NoGarbageCollect	= 0x08,
		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/enum[@name="FbBackupFlags"]/field[@name="OldDescriptions"]/*'/>
		OldDescriptions		= 0x10,
		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/enum[@name="FbBackupFlags"]/field[@name="NonTransportable"]/*'/>
		NonTransportable	= 0x20,
		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/enum[@name="FbBackupFlags"]/field[@name="Convert"]/*'/>
		Convert				= 0x40,
		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/enum[@name="FbBackupFlags"]/field[@name="Expand"]/*'/>
		Expand				= 0x80
	}

	#endregion

	#region Structs

	/// <include file='Doc/en_EN/FbBackup.xml' path='doc/struct[@name="FbBackupFile"]/overview/*'/>
	public struct FbBackupFile
	{
		#region Fields

		private string	backupFile;
		private int		backupLength;

		#endregion

		#region Properties

		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/struct[@name="FbBackupFile"]/property[@name="BackupFile"]/*'/>
		public string BackupFile
		{
			get { return this.backupFile; }
			set { this.backupFile = value; }
		}
		
		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/struct[@name="FbBackupFile"]/property[@name="BackupLength"]/*'/>
		public int BackupLength
		{
			get { return this.backupLength; }
			set { this.backupLength = value; }
		}

		#endregion

		#region Constructors

		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/struct[@name="FbBackupFile"]/constructor[@name="ctor(system.String,System.Int32)"]/*'/>
		public FbBackupFile(string fileName, int fileLength)
		{
			this.backupFile		= fileName;
			this.backupLength	= fileLength;
		}

		#endregion
	}

	#endregion

	/// <include file='Doc/en_EN/FbBackup.xml' path='doc/class[@name="FbBackup"]/overview/*'/>
	public sealed class FbBackup : FbService
	{
		#region Fields
		
		private bool			verbose;
		private ArrayList		backupFiles;
		private int				factor;		
		private FbBackupFlags	options;
		
		#endregion

		#region Properties

		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/class[@name="FbBackup"]/property[@name="BackupFiles"]/*'/>
		public ArrayList BackupFiles
		{
			get { return this.backupFiles; }
		}

		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/class[@name="FbBackup"]/property[@name="Verbose"]/*'/>
		public bool	Verbose
		{
			get { return this.verbose; }
			set { this.verbose = value; }
		}

		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/class[@name="FbBackup"]/property[@name="Factor"]/*'/>
		public int Factor
		{
			get { return this.factor; }
			set { this.factor = value; }
		}

		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/class[@name="FbBackup"]/property[@name="Options"]/*'/>
		public FbBackupFlags Options
		{
			get { return this.options; }
			set { this.options = value; }
		}

		#endregion

		#region Constructors

		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/class[@name="FbBackup"]/constructor[@name="ctor"]/*'/>
		public FbBackup() : base()
		{
			this.verbose	 = false;
			this.backupFiles = new ArrayList();
			this.factor		 = 0;
		}

		#endregion

		#region Methods

		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/class[@name="FbBackup"]/method[@name="Start"]/*'/>
		public void Start()
		{
			// Configure Spb
			this.StartSpb = new SpbBuffer();
			this.StartSpb.Append(IscCodes.isc_action_svc_backup);
			this.StartSpb.Append(
				IscCodes.isc_spb_dbname, 
				this.Parameters.Database);
			foreach(FbBackupFile bkpFile in backupFiles)
			{
				this.StartSpb.Append(
					IscCodes.isc_spb_bkp_file, 
					bkpFile.BackupFile);
				
				this.StartSpb.Append(
					IscCodes.isc_spb_bkp_length, 
					bkpFile.BackupLength);
			}
			if (verbose)
			{
				this.StartSpb.Append(IscCodes.isc_spb_verbose);
			}
			this.StartSpb.Append(IscCodes.isc_spb_options, (int)this.options);

			// Start execution
			this.StartTask();
		}

		#endregion
	}
}
