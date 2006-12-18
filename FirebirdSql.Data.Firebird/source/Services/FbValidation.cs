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
using FirebirdSql.Data.Firebird.Gds;

namespace FirebirdSql.Data.Firebird.Services
{
	#region ENUMS

	/// <include file='Doc/en_EN/FbValidation.xml' path='doc/struct[@name="FbValidationFlags"]/overview/*'/>
    [Flags]
    public enum FbValidationFlags
	{
		/// <include file='Doc/en_EN/FbValidation.xml' path='doc/struct[@name="FbValidationFlags"]/field[@name="ValidateDatabase"]/*'/>
		ValidateDatabase	= 0x01,
		/// <include file='Doc/en_EN/FbValidation.xml' path='doc/struct[@name="FbValidationFlags"]/field[@name="SweepDatabase"]/*'/>
		SweepDatabase		= 0x02,
		/// <include file='Doc/en_EN/FbValidation.xml' path='doc/struct[@name="FbValidationFlags"]/field[@name="MendDatabase"]/*'/>
		MendDatabase		= 0x04,
		/// <include file='Doc/en_EN/FbValidation.xml' path='doc/struct[@name="FbValidationFlags"]/field[@name="CheckDatabase"]/*'/>
		CheckDatabase		= 0x10,
		/// <include file='Doc/en_EN/FbValidation.xml' path='doc/struct[@name="FbValidationFlags"]/field[@name="IgnoreChecksum"]/*'/>
		IgnoreChecksum		= 0x20,
		/// <include file='Doc/en_EN/FbValidation.xml' path='doc/struct[@name="FbValidationFlags"]/field[@name="KillShadows"]/*'/>
		KillShadows		 	= 0x40,
		/// <include file='Doc/en_EN/FbValidation.xml' path='doc/struct[@name="FbValidationFlags"]/field[@name="Full"]/*'/>
		Full				= 0x80
	}

	#endregion

	/// <include file='Doc/en_EN/FbValidation.xml' path='doc/class[@name="FbValidation"]/overview/*'/>
	public sealed class FbValidation : FbService
	{
		#region FIELDS
		
		private	string				database;
		private FbValidationFlags	options;
		
		#endregion
		
		#region PROPERTIES

		/// <include file='Doc/en_EN/FbValidation.xml' path='doc/class[@name="FbValidation"]/property[@name="Database"]/*'/>
		public string Database
		{
			get { return database; }
			set { database = value; }
		}

		/// <include file='Doc/en_EN/FbValidation.xml' path='doc/class[@name="FbValidation"]/property[@name="Options"]/*'/>
		public FbValidationFlags Options
		{
			get { return options; }
			set { options = value; }
		}		
		
		#endregion
				
		#region CONSTRUCTORS

		/// <include file='Doc/en_EN/FbValidation.xml' path='doc/class[@name="FbValidation"]/constructor[@name="ctor"]/*'/>
		public FbValidation() : base()
		{
			database = String.Empty;
		}
		
		#endregion
		
		#region METHODS

		/// <include file='Doc/en_EN/FbValidation.xml' path='doc/class[@name="FbValidation"]/method[@name="Start"]/*'/>
		public void Start()
		{			
			// Configure Spb
			startSpb = new GdsSpbBuffer();
			startSpb.Append(GdsCodes.isc_action_svc_repair);
			startSpb.Append(GdsCodes.isc_spb_dbname, database);
			startSpb.Append(GdsCodes.isc_spb_options, (int)options);
			
			// Start execution
			startTask();
		}
		
		#endregion
	}
}
