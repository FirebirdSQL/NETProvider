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

	/// <include file='Doc/en_EN/FbRestore.xml' path='doc/enum[@name="FbRestoreFlags"]/overview/*'/>
	[Flags]
	public enum FbRestoreFlags
	{
		/// <include file='Doc/en_EN/FbRestore.xml' path='doc/enum[@name="FbRestoreFlags"]/field[@name="DeactivateIndexes"]/*'/>
		DeactivateIndexes			= 0x0100,
		/// <include file='Doc/en_EN/FbRestore.xml' path='doc/enum[@name="FbRestoreFlags"]/field[@name="NoShadow"]/*'/>
		NoShadow					= 0x0200,
		/// <include file='Doc/en_EN/FbRestore.xml' path='doc/enum[@name="FbRestoreFlags"]/field[@name="NoValidity"]/*'/>
		NoValidity					= 0x0400,
		/// <include file='Doc/en_EN/FbRestore.xml' path='doc/enum[@name="FbRestoreFlags"]/field[@name="IndividualCommit"]/*'/>
		IndividualCommit			= 0x0800,
		/// <include file='Doc/en_EN/FbRestore.xml' path='doc/enum[@name="FbRestoreFlags"]/field[@name="Replace"]/*'/>
		Replace						= 0x1000,
		/// <include file='Doc/en_EN/FbRestore.xml' path='doc/enum[@name="FbRestoreFlags"]/field[@name="Create"]/*'/>
		Create						= 0x2000,
		/// <include file='Doc/en_EN/FbRestore.xml' path='doc/enum[@name="FbRestoreFlags"]/field[@name="UseAllSpace"]/*'/>
		UseAllSpace					= 0x4000
	}

	#endregion

	/// <include file='Doc/en_EN/FbRestore.xml' path='doc/class[@name="FbRestore"]/overview/*'/>
	public sealed class FbRestore : FbService
	{
		#region FIELDS

		private	string				database;
		private ArrayList			backupFiles;
		private bool				verbose;
		private int					pageBuffers;
		private int					pageSize;
		private FbRestoreFlags		options;
		
		#endregion

		#region PROPERTIES

		/// <include file='Doc/en_EN/FbRestore.xml' path='doc/class[@name="FbRestore"]/property[@name="Database"]/*'/>
		public string Database
		{
			get { return database; }
			set { database = value; }
		}

		/// <include file='Doc/en_EN/FbRestore.xml' path='doc/class[@name="FbRestore"]/property[@name="BackupFiles"]/*'/>
		public ArrayList BackupFiles
		{
			get { return backupFiles; }
			set { backupFiles = value; }
		}

		/// <include file='Doc/en_EN/FbRestore.xml' path='doc/class[@name="FbRestore"]/property[@name="Verbose"]/*'/>
		public bool	Verbose
		{
			get { return verbose; }
			set { verbose = value; }
		}

		/// <include file='Doc/en_EN/FbRestore.xml' path='doc/class[@name="FbRestore"]/property[@name="PageBuffers"]/*'/>
		public int PageBuffers
		{
			get { return pageBuffers; }
			set { pageBuffers = value; }
		}

		/// <include file='Doc/en_EN/FbRestore.xml' path='doc/class[@name="FbRestore"]/property[@name="PageSize"]/*'/>
		public int PageSize
		{
			get { return pageSize; }
			set
			{
				if (pageSize != 1024 && 
					pageSize != 2048 && 
					pageSize != 4096 &&
					pageSize != 8192 && 
					pageSize != 16384)
				{
					throw new InvalidOperationException("Invalid page size.");
				}
				pageSize = value;
			}
		}

		/// <include file='Doc/en_EN/FbRestore.xml' path='doc/class[@name="FbRestore"]/property[@name="Options"]/*'/>
		public FbRestoreFlags Options
		{
			get { return options; }
			set { options = value; }
		}

		#endregion

		#region CONSTRUCTORS

		/// <include file='Doc/en_EN/FbRestore.xml' path='doc/class[@name="FbRestore"]/constructor[@name="ctor"]/*'/>
		public FbRestore() : base()
		{
			database		= String.Empty;
			backupFiles		= new ArrayList();
			verbose			= false;
			pageSize		= 4096;
			pageBuffers		= 2048;
		}

		#endregion

		#region METHODS

		/// <include file='Doc/en_EN/FbRestore.xml' path='doc/class[@name="FbRestore"]/method[@name="Start"]/*'/>
		public void Start()
		{
			// Configure Spb
			startSpb = new GdsSpbBuffer();
			startSpb.Append(GdsCodes.isc_action_svc_restore);			
			foreach(FbBackupFile bkpFile in backupFiles)
			{
				startSpb.Append(GdsCodes.isc_spb_bkp_file, bkpFile.BackupFile);
			}
			startSpb.Append(GdsCodes.isc_spb_dbname, database);
			if (verbose)
			{
				startSpb.Append(GdsCodes.isc_spb_verbose);
			}
			startSpb.Append(GdsCodes.isc_spb_res_buffers, pageBuffers);
			startSpb.Append(GdsCodes.isc_spb_res_page_size, pageSize);
			startSpb.Append(GdsCodes.isc_spb_options, (int)options);
			
			// Start execution
			startTask();			
		}
				
		#endregion		
	}
}
