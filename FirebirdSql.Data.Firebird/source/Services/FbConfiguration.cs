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

	/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/enum[@name="FbShutdownMode"]/overview/*'/>
	public enum FbShutdownMode
	{
		/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/enum[@name="FbShutdownMode"]/field[@name="Forced"]/*'/>
		Forced,
		/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/enum[@name="FbShutdownMode"]/field[@name="DenyTransaction"]/*'/>
		DenyTransaction,
		/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/enum[@name="FbShutdownMode"]/field[@name="DenyConnection"]/*'/>
		DenyConnection
	}

	#endregion

	/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/class[@name="FbConfiguration"]/overview/*'/>
	public sealed class FbConfiguration : FbService
	{
		#region FIELDS
		
		private string 					database;
		
		#endregion
		
		#region PROPERTIES

		/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/class[@name="FbConfiguration"]/property[@name="Database"]/*'/>
		public string Database
		{
			get { return database; }
			set { database = value; }
		}

		#endregion

		#region CONSTRUCTORS

		/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/class[@name="FbConfiguration"]/constructor[@name="ctor"]/*'/>
		public FbConfiguration() : base()
		{
			database = String.Empty;
		}

		#endregion

		#region METHODS

		/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/class[@name="FbConfiguration"]/method[@name="SetSqlDialect(System.Int32)"]/*'/>
		public void SetSqlDialect(int sqlDialect)
		{
			// Configure Spb
			startSpb = new GdsSpbBuffer();
			startSpb.Append(GdsCodes.isc_action_svc_properties);
			startSpb.Append(GdsCodes.isc_spb_dbname, database);
			startSpb.Append(GdsCodes.isc_spb_prp_set_sql_dialect, sqlDialect);

			// Start execution
			startTask();

			Close();
		}

		/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/class[@name="FbConfiguration"]/method[@name="SetSweepInterval(System.Int32)"]/*'/>
		public void SetSweepInterval(int sweepInterval)
		{
			// Configure Spb
			startSpb = new GdsSpbBuffer();
			startSpb.Append(GdsCodes.isc_action_svc_properties);
			startSpb.Append(GdsCodes.isc_spb_dbname, database);
			startSpb.Append(GdsCodes.isc_spb_prp_sweep_interval, sweepInterval);

			// Start execution
			startTask();

			Close();
		}

		/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/class[@name="FbConfiguration"]/method[@name="SetPageBuffers(System.Int32)"]/*'/>
		public void SetPageBuffers(int pageBuffers)
		{
			// Configure Spb
			startSpb = new GdsSpbBuffer();
			startSpb.Append(GdsCodes.isc_action_svc_properties);
			startSpb.Append(GdsCodes.isc_spb_dbname, database);
			startSpb.Append(GdsCodes.isc_spb_prp_page_buffers, pageBuffers);

			// Start execution
			startTask();

			Close();
		}

		/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/class[@name="FbConfiguration"]/method[@name="DatabaseShutdown(FbShutdownMode,System.Int32)"]/*'/>
		public void DatabaseShutdown(FbShutdownMode mode, int seconds)
		{
			// Configure Spb
			startSpb = new GdsSpbBuffer();
			startSpb.Append(GdsCodes.isc_action_svc_properties);
			startSpb.Append(GdsCodes.isc_spb_dbname, database);

			switch (mode)
			{
				case FbShutdownMode.Forced:
					startSpb.Append(GdsCodes.isc_spb_prp_shutdown_db, seconds);
					break;
				
				case FbShutdownMode.DenyTransaction:
					startSpb.Append(GdsCodes.isc_spb_prp_deny_new_transactions, seconds);	
					break;

				case FbShutdownMode.DenyConnection:
					startSpb.Append(GdsCodes.isc_spb_prp_deny_new_attachments, seconds);
					break;
			}

			// Start execution
			startTask();
			
			Close();
		}

		/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/class[@name="FbConfiguration"]/method[@name="DatabaseOnline"]/*'/>
		public void DatabaseOnline()
		{
			// Configure Spb
			startSpb = new GdsSpbBuffer();
			startSpb.Append(GdsCodes.isc_action_svc_properties);
			startSpb.Append(GdsCodes.isc_spb_dbname, database);
			startSpb.Append(GdsCodes.isc_spb_options, GdsCodes.isc_spb_prp_db_online);

			// Start execution
			startTask();
			
			Close();
		}

		/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/class[@name="FbConfiguration"]/method[@name="ActivateShadows"]/*'/>
		public void ActivateShadows()
		{
			// Configure Spb
			startSpb = new GdsSpbBuffer();
			startSpb.Append(GdsCodes.isc_action_svc_properties);
			startSpb.Append(GdsCodes.isc_spb_dbname, database);
			startSpb.Append(GdsCodes.isc_spb_options, GdsCodes.isc_spb_prp_activate);

			// Start execution
			startTask();

			Close();
		}

		/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/class[@name="FbConfiguration"]/method[@name="SetForcedWrites(System.Boolean)"]/*'/>
		public void SetForcedWrites(bool forcedWrites)
		{
			// Configure Spb
			startSpb = new GdsSpbBuffer();
			startSpb.Append(GdsCodes.isc_action_svc_properties);
			startSpb.Append(GdsCodes.isc_spb_dbname, database);

			// WriteMode
			if (forcedWrites)
			{
				startSpb.Append(GdsCodes.isc_spb_prp_write_mode, 
				                     (byte)GdsCodes.isc_spb_prp_wm_sync);
			}
			else
			{
				startSpb.Append(GdsCodes.isc_spb_prp_write_mode, 
				                     (byte)GdsCodes.isc_spb_prp_wm_async);
			}			

			// Start execution
			startTask();
			
			Close();
		}

		/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/class[@name="FbConfiguration"]/method[@name="SetReserveSpace(System.Boolean)"]/*'/>
		public void SetReserveSpace(bool reserveSpace)
		{
			startSpb = new GdsSpbBuffer();
			startSpb.Append(GdsCodes.isc_action_svc_properties);
			startSpb.Append(GdsCodes.isc_spb_dbname, database);

			// Reserve Space
			if (reserveSpace)
			{
				startSpb.Append(GdsCodes.isc_spb_prp_reserve_space, 
				                     (byte)GdsCodes.isc_spb_prp_res);
			}
			else
			{
				startSpb.Append(GdsCodes.isc_spb_prp_reserve_space, 
				                     (byte)GdsCodes.isc_spb_prp_res_use_full);
			}

			// Start execution
			startTask();			
			
			Close();
		}

		/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/class[@name="FbConfiguration"]/method[@name="SetAccessMode(System.Boolean)"]/*'/>
		public void SetAccessMode(bool readOnly)
		{
			// Configure Spb
			startSpb = new GdsSpbBuffer();

			startSpb.Append(GdsCodes.isc_action_svc_properties);
			startSpb.Append(GdsCodes.isc_spb_dbname, database);
			
			if (readOnly)
			{
				startSpb.Append(GdsCodes.isc_spb_prp_access_mode, 
				                     (byte)GdsCodes.isc_spb_prp_am_readonly);
			}
			else
			{
				startSpb.Append(GdsCodes.isc_spb_prp_access_mode, 
				                     (byte)GdsCodes.isc_spb_prp_am_readwrite);
			}
			
			// Start execution
			startTask();
			
			Close();
		}
		
		#endregion
	}
}
