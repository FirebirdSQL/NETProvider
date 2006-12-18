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
using System.Text;
using System.Collections;

using FirebirdSql.Data.Firebird.Gds;

namespace FirebirdSql.Data.Firebird
{	
	internal class FbDbConnection : MarshalByRefObject
	{
		#region FIELDS

		private GdsDbAttachment	db;
		private GdsAttachParams	parameters;
		private long			created;
		private long			lifetime;
		private bool			pooled;
		private string			connectionString;
		
		#endregion

		#region PROPERTIES

		public GdsDbAttachment DB
		{
			get { return this.db; }
		}

		public long Lifetime
		{
			get { return this.lifetime; }
		}
		public long Created
		{
			get { return this.created; }
			set { this.created = value; }
		}
		
		public bool Pooled
		{
			get { return this.pooled; }
			set { this.pooled = value; }
		}

		public GdsAttachParams Parameters
		{
			get { return this.parameters; }
		}

		public string ConnectionString
		{
			get { return this.connectionString; }
		}

		#endregion

		#region CONSTRUCTORS

		public FbDbConnection(string connectionString)
		{
			this.connectionString	= connectionString;
			this.parameters			= new GdsAttachParams(connectionString);
			this.lifetime			= this.parameters.LifeTime;
		}

		public FbDbConnection(string connectionString, GdsAttachParams parameters)
		{
			this.connectionString	= connectionString;
			this.parameters			= parameters;
			this.lifetime			= parameters.LifeTime;
		}

		#endregion

		#region METHODS

		public void Connect()
		{							
			try
			{
				this.db = new GdsDbAttachment(this.parameters);
				this.db.Attach();
			}
			catch (GdsException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}
		
		public void Disconnect()
		{	
			try
			{
				this.db.Detach();
			}
			catch (GdsException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		public void CancelEvents()
		{
			try
			{
				if (this.db.EventsAtt != null)
				{
					this.db.EventsAtt.Detach();
					this.db.EventsAtt = null;
				}
			}
			catch (GdsException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		public bool Verify()
		{
			int INFO_SIZE = 16;
			
			byte[] buffer = new byte[INFO_SIZE];
			
			// Do not actually ask for any information
			byte[] databaseInfo  = new byte[]
			{
				GdsCodes.isc_info_end
			};

			try 
			{
				this.db.GetDatabaseInfo(databaseInfo, INFO_SIZE, buffer);

				return true;
			}
			catch
			{
				return false;
			}
		}

		#endregion
	}
}
