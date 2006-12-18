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
		#region Fields

		private ArrayList			backupFiles;
		private bool				verbose;
		private int					pageBuffers;
		private int					pageSize;
		private FbRestoreFlags		options;
		
		#endregion

		#region Properties

		/// <include file='Doc/en_EN/FbRestore.xml' path='doc/class[@name="FbRestore"]/property[@name="BackupFiles"]/*'/>
		public ArrayList BackupFiles
		{
			get { return this.backupFiles; }
		}

		/// <include file='Doc/en_EN/FbRestore.xml' path='doc/class[@name="FbRestore"]/property[@name="Verbose"]/*'/>
		public bool	Verbose
		{
			get { return this.verbose; }
			set { this.verbose = value; }
		}

		/// <include file='Doc/en_EN/FbRestore.xml' path='doc/class[@name="FbRestore"]/property[@name="PageBuffers"]/*'/>
		public int PageBuffers
		{
			get { return this.pageBuffers; }
			set { this.pageBuffers = value; }
		}

		/// <include file='Doc/en_EN/FbRestore.xml' path='doc/class[@name="FbRestore"]/property[@name="PageSize"]/*'/>
		public int PageSize
		{
			get { return this.pageSize; }
			set
			{
				if (this.pageSize != 1024 && 
					this.pageSize != 2048 && 
					this.pageSize != 4096 &&
					this.pageSize != 8192 && 
					this.pageSize != 16384)
				{
					throw new InvalidOperationException("Invalid page size.");
				}
				this.pageSize = value;
			}
		}

		/// <include file='Doc/en_EN/FbRestore.xml' path='doc/class[@name="FbRestore"]/property[@name="Options"]/*'/>
		public FbRestoreFlags Options
		{
			get { return this.options; }
			set { this.options = value; }
		}

		#endregion

		#region Constructors

		/// <include file='Doc/en_EN/FbRestore.xml' path='doc/class[@name="FbRestore"]/constructor[@name="ctor"]/*'/>
		public FbRestore() : base()
		{
			this.backupFiles	= new ArrayList();
			this.verbose		= false;
			this.pageSize		= 4096;
			this.pageBuffers	= 2048;
		}

		#endregion

		#region Methods

		/// <include file='Doc/en_EN/FbRestore.xml' path='doc/class[@name="FbRestore"]/method[@name="Start"]/*'/>
		public void Start()
		{
			// Configure Spb
			this.StartSpb = new SpbBuffer();
			this.StartSpb.Append(IscCodes.isc_action_svc_restore);			
			foreach(FbBackupFile bkpFile in backupFiles)
			{
				this.StartSpb.Append(
					IscCodes.isc_spb_bkp_file, 
					bkpFile.BackupFile);
			}
			this.StartSpb.Append(
				IscCodes.isc_spb_dbname, 
				this.Parameters.Database);
			if (this.verbose)
			{
				this.StartSpb.Append(IscCodes.isc_spb_verbose);
			}
			this.StartSpb.Append(IscCodes.isc_spb_res_buffers, this.pageBuffers);
			this.StartSpb.Append(IscCodes.isc_spb_res_page_size, this.pageSize);
			this.StartSpb.Append(IscCodes.isc_spb_options, (int)this.options);
			
			// Start execution
			this.StartTask();			
		}
				
		#endregion		
	}
}
