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
using FirebirdSql.Data.Firebird.Gds;

namespace FirebirdSql.Data.Firebird.Services
{
	#region ENUMS

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

	#region STRUCTS

	/// <include file='Doc/en_EN/FbBackup.xml' path='doc/struct[@name="FbBackupFile"]/overview/*'/>
	public struct FbBackupFile
	{
		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/struct[@name="FbBackupFile"]/property[@name="BackupFile"]/*'/>
		public string	BackupFile;
		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/struct[@name="FbBackupFile"]/property[@name="BackupLength"]/*'/>
		public int		BackupLength;

		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/struct[@name="FbBackupFile"]/constructor[@name="ctor(system.String,System.Int32)"]/*'/>
		public FbBackupFile(string fileName, int fileLength)
		{
			BackupFile		= fileName;
			BackupLength	= fileLength;
		}
	}

	#endregion

	/// <include file='Doc/en_EN/FbBackup.xml' path='doc/class[@name="FbBackup"]/overview/*'/>
	public sealed class FbBackup : FbService
	{
		#region FIELDS
		
		private string			database;
		private bool			verbose;
		
		private ArrayList		backupFiles;
		
		private int				factor;
		
		private FbBackupFlags	options;
		
		#endregion

		#region PROPERTIES

		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/class[@name="FbBackup"]/property[@name="Database"]/*'/>
		public string Database
		{
			get { return database; }
			set { database = value; }
		}

		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/class[@name="FbBackup"]/property[@name="BackupFiles"]/*'/>
		public ArrayList BackupFiles
		{
			get { return backupFiles; }
			set { backupFiles = value; }
		}

		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/class[@name="FbBackup"]/property[@name="Verbose"]/*'/>
		public bool	Verbose
		{
			get { return verbose; }
			set { verbose = value; }
		}

		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/class[@name="FbBackup"]/property[@name="Factor"]/*'/>
		public int Factor
		{
			get { return factor; }
			set { factor = value; }
		}

		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/class[@name="FbBackup"]/property[@name="Options"]/*'/>
		public FbBackupFlags Options
		{
			get { return options; }
			set { options = value; }
		}

		#endregion

		#region CONSTRUCTORS

		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/class[@name="FbBackup"]/constructor[@name="ctor"]/*'/>
		public FbBackup() : base()
		{
			database 		= String.Empty;
			verbose			= false;
			backupFiles		= new ArrayList();
			factor			= 0;
		}

		#endregion

		#region METHODS

		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/class[@name="FbBackup"]/method[@name="Start"]/*'/>
		public void Start()
		{
			// Configure Spb
			startSpb = new GdsSpbBuffer();
			startSpb.Append(GdsCodes.isc_action_svc_backup);
			startSpb.Append(GdsCodes.isc_spb_dbname, database);
			foreach(FbBackupFile bkpFile in backupFiles)
			{
				startSpb.Append(GdsCodes.isc_spb_bkp_file, bkpFile.BackupFile);
				startSpb.Append(GdsCodes.isc_spb_bkp_length, bkpFile.BackupLength);
			}
			if (verbose)
			{
				startSpb.Append(GdsCodes.isc_spb_verbose);
			}
			startSpb.Append(GdsCodes.isc_spb_options, (int)options);

			// Start execution
			startTask();
		}

		#endregion
	}
}
