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

using FirebirdSql.Data.INGDS;
using FirebirdSql.Data.NGDS;

namespace FirebirdSql.Data.Firebird
{	
	/// <include file='xmldoc/fbiscconnection.xml' path='doc/member[@name="T:FbIscConnection"]/*'/>
	internal class FbIscConnection
	{
		#region FIELDS
				
		internal isc_db_handle_impl			db;
		internal FbConnectionRequestInfo	cri;
		
		private FbDatabaseInfo dbInfo;
		private IGDS	gds;
		private string	connectionString;		
		private string	dataSource;
		private string	user;
		private string	password;
		private string	database;
		private string	role;
		private string	charset;
		private string	port;
		private byte	dialect;
		private int		connectionTimeout;
		private long	lifetime;
		private long	created;
		private bool	pooling;
		private int		packetSize;
		private bool	pooled;		

		#endregion

		#region PROPERTIES

		public IGDS GDS
		{
			get { return gds; }
		}

		public string ConnectionString
		{
			get { return connectionString; }
		}

		public string DataSource
		{
			get { return dataSource; }
		}

		public int ConnectionTimeout
		{ 
			get { return connectionTimeout; } 
		}

		public string Database
		{ 
			get { return database; } 
		}

		public string User
		{ 
			get { return user; } 
		}

		public string Password
		{ 
			get { return password; } 
		}

		public byte	Dialect
		{ 
			get { return dialect; } 
		}

		public string Role
		{ 
			get { return role; } 
		}

		public string Charset
		{ 
			get { return charset; } 
		}

		public FbDatabaseInfo DatabaseInfo
		{
			get { return dbInfo; }
		}

		public long Lifetime
		{
			get { return lifetime; }
		}

		public int PacketSize
		{
			get { return packetSize; }
		}

		public long Created
		{
			get { return created; }
			set { created = value; }
		}
		
		public bool Pooling
		{
			get { return pooling; }
		}

		public bool Pooled
		{
			get { return pooled; }
			set { pooled = value; }
		}

		#endregion

		#region CONSTRUCTORS

		private FbIscConnection()
		{
			gds					= GDSFactory.NewGDS();
			connectionString	= String.Empty;		
			dataSource			= String.Empty;		
			user				= String.Empty;
			password			= String.Empty;
			database			= String.Empty;
			role				= String.Empty;		
			charset				= String.Empty;
			port				= "3050";
			dialect				= 3;
			connectionTimeout	= 15;
			lifetime			= 0;
			created				= 0;
			pooling				= true;
			packetSize			= 8192;
			pooled				= true;
		}

		public FbIscConnection(string connectionString) : this()
		{
			this.connectionString = connectionString;

			ParseConnectionString(connectionString);
		}

		#endregion

		#region METHODS

		/// <include file='xmldoc/fbiscconnection.xml' path='doc/member[@name="M:Open"]/*'/>
		public void Open()
		{				
			// New instance of RequestConnectionInfo class
			cri = new FbConnectionRequestInfo();
			// New instance for Database handler
			db	= (isc_db_handle_impl)gds.get_new_isc_db_handle();
			
			// DPB configuration
			cri.SetProperty(GdsCodes.isc_dpb_dummy_packet_interval, 
							new byte[] {120, 10, 0, 0});
			cri.SetProperty(GdsCodes.isc_dpb_sql_dialect, 
							new byte[] {dialect, 0, 0, 0});
			cri.SetProperty(GdsCodes.isc_dpb_lc_ctype, charset);
			if (role != null)
			{
				if (role.Length > 0)
				{
					cri.SetProperty(GdsCodes.isc_dpb_sql_role_name, role);
				}
			}
			cri.SetProperty(GdsCodes.isc_dpb_connect_timeout, connectionTimeout);			

			cri.SetUser(user);
			cri.SetPassword(password);

			// Set packet size
			db.PacketSize = packetSize;

			// Connect to database
			gds.isc_attach_database(GetDatabaseUrl(), db, cri.Dpb);

			this.dialect			= dialect;
			this.charset			= charset;
			this.role				= role;
			this.user				= user;
			this.password			= password;
			this.database			= database;
			this.connectionTimeout	= connectionTimeout;

			this.dbInfo	  = new FbDatabaseInfo(this);
		}

		/// <include file='xmldoc/fbiscconnection.xml' path='doc/member[@name="M:ClearWarnings"]/*'/>
		public void ClearWarnings()
		{
			db.ClearWarnings();
		}

		/// <include file='xmldoc/fbiscconnection.xml' path='doc/member[@name="M:Close"]/*'/>
		public void Close()
		{	
			ClearWarnings();
			gds.isc_detach_database(db);
				
			cri			= null;
			db			= null;
		}

		/// <include file='xmldoc/fbiscconnection.xml' path='doc/member[@name="M:VerifyAttachedDB"]/*'/>
		public bool VerifyAttachedDB()
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
				gds.isc_database_info(db, databaseInfo.Length, 
										databaseInfo, INFO_SIZE,buffer);

				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <include file='xmldoc/fbiscconnection.xml' path='doc/member[@name="M:ParseConnectionString"]/*'/>
		internal void ParseConnectionString(string connStr)
		{
			string	db			= null;
			string	user		= null;
			string	password	= null;
			string	dataSource	= "localhost";
			string	port		= "3050";
			string	charset		= "NONE";
			string	role		= null;
			byte	dialect		= 3;
			int		lifetime	= 0;
			bool	pooling		= true;
			int		connectionTimeout = 15;
			int		packetSize	= 8192;

			string[] elements = connStr.Split(';');			

			for (int i = 0; i < elements.Length; i++)
			{
				string[] values = elements[i].Split('=');

				if (values.Length == 2)
				{
					if (values[0] != null && values[1] != null)
					{
						switch (values[0].Trim().ToUpper())
						{
							case "DATABASE":
								db = values[1];
								break;

							case "USER":
								user = values[1];
								break;

							case "PASSWORD":
								password = values[1];
								break;

							case "SERVER":
							case "DATASOURCE":
								dataSource = values[1];
								break;

							case "PORT":
								port = values[1];
								break;

							case "DIALECT":
								dialect = byte.Parse(values[1]);
								break;

							case "CHARSET":
								charset = values[1];
								break;

							case "ROLE":
								role 	= values[1];
								break;

							case "LIFETIME":
							case "CONNECTION LIFETIME":
								lifetime = Int32.Parse(values[1]);
								break;

							case "POOLING":						
								pooling = bool.Parse(values[1]);
								break;

							case "TIMEOUT":
							case "CONNECTION TIMEOUT":
								connectionTimeout = Int32.Parse(values[1]);
								break;

							case "PACKET SIZE":
								packetSize = Int32.Parse(values[1]);
								break;

							default:
								break;
						}
					}
				}
			}

			if (db == null || user == null || password == null || dataSource == null)
			{
				throw new ArgumentException("An invalid connection string argument has been supplied or a required connection string argument has not been supplied.");
			}
			else
			{
				if (packetSize < 512 || packetSize > 32767)
				{
					// Valor 'Packet Size' no válido de '100'. El valor debe ser un entero >= 512 y <= 32767.
					StringBuilder msg = new StringBuilder();

					msg.AppendFormat("'Packet Size' value of {0} is not valid.", packetSize);
					msg.AppendFormat("The value should be an integer >= 512 and <= 32767.");

					throw new ArgumentException(msg.ToString());
				}
				else
				{
					this.dataSource			= dataSource;
					this.port				= port;
					this.database			= db;
					this.user				= user;
					this.password			= password;
					this.charset			= charset;
					this.dialect			= dialect;
					this.role				= role;
					this.pooling			= pooling;
					this.lifetime			= lifetime * TimeSpan.TicksPerSecond;
					this.connectionTimeout	= connectionTimeout;
					this.packetSize			= packetSize;
				}
			}
		}

		private string GetDatabaseUrl()
		{
			return dataSource + "/" + port + ":" + database;
		}

		#endregion
	}
}