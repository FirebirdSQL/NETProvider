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

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Firebird.Services
{
	#region Enumerations

	/// <include file='Doc/en_EN/FbStatistical.xml' path='doc/enum[@name="FbStatisticalFlags"]/overview/*'/>
	[Flags]
	public enum FbStatisticalFlags
	{
		/// <include file='Doc/en_EN/FbStatistical.xml' path='doc/enum[@name="FbStatisticalFlags"]/field[@name="DataPages"]/*'/>
		DataPages				= 0x01,
		/// <include file='Doc/en_EN/FbStatistical.xml' path='doc/enum[@name="FbStatisticalFlags"]/field[@name="DatabaseLog"]/*'/>
		DatabaseLog				= 0x02,
		/// <include file='Doc/en_EN/FbStatistical.xml' path='doc/enum[@name="FbStatisticalFlags"]/field[@name="HeaderPages"]/*'/>
		HeaderPages				= 0x04,
		/// <include file='Doc/en_EN/FbStatistical.xml' path='doc/enum[@name="FbStatisticalFlags"]/field[@name="IndexPages"]/*'/>
		IndexPages				= 0x08,
		/// <include file='Doc/en_EN/FbStatistical.xml' path='doc/enum[@name="FbStatisticalFlags"]/field[@name="SystemTablesRelations"]/*'/>
		SystemTablesRelations	= 0x10,
	}

	#endregion

	/// <include file='Doc/en_EN/FbStatistical.xml' path='doc/class[@name="FbStatistical"]/overview/*'/>
	public sealed class FbStatistical : FbService
	{
		#region Fields
		
		private FbStatisticalFlags 	options;
		
		#endregion

		#region Properties

		/// <include file='Doc/en_EN/FbStatistical.xml' path='doc/class[@name="FbStatistical"]/property[@name="Options"]/*'/>
		public FbStatisticalFlags Options
		{
			get { return this.options; }
			set { this.options = value; }
		}
				
		#endregion

		#region Constructors

		/// <include file='Doc/en_EN/FbStatistical.xml' path='doc/class[@name="FbStatistical"]/constructor[@name="FbStatistical"]/*'/>
		public FbStatistical() : base()
		{
		}
		
		#endregion

		#region Methods

		/// <include file='Doc/en_EN/FbStatistical.xml' path='doc/class[@name="FbStatistical"]/constructor[@name="Start"]/*'/>
		public void Start()
		{		
			// Configure Spb
			this.StartSpb = new SpbBuffer();
			this.StartSpb.Append(IscCodes.isc_action_svc_db_stats);
			this.StartSpb.Append(
				IscCodes.isc_spb_dbname, 
				this.Parameters.Database);
			this.StartSpb.Append(
				IscCodes.isc_spb_options, 
				(int)this.options);
			
			// Start execution
			this.StartTask();
		}
		
		#endregion		
	}
}
