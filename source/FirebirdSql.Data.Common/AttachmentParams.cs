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
using System.IO;
using System.Text;
using System.Globalization;

namespace FirebirdSql.Data.Common
{
#if (SINGLE_DLL)
	internal
#else
	public
#endif
	sealed class AttachmentParams
	{
		#region Fields

		private string	connectionString;
		private string	userName;
		private string	userPassword;
		private string	dataSource;
		private int		port;
		private string	database;
		private int		packetSize;
		private	byte	dialect;
		private string	role;
		private int		timeout;
		private bool	pooling;
		private long	lifetime;
		private int		minPoolSize;
		private int		maxPoolSize;
		private int		serverType;
		private Charset	charset;

		#endregion

		#region Properties

		public string ConnectionString
		{
			get { return this.connectionString; }
			set 
			{ 
				this.connectionString = value; 
				this.parseConnectionString();
			}
		}

		public string UserName
		{
			get { return this.userName; }
			set { this.userName = value; }
		}

		public string UserPassword
		{
			get { return this.userPassword; }
			set { this.userPassword = value; }
		}

		public string DataSource
		{
			get { return this.dataSource; }
			set { this.dataSource = value; }
		}

		public int Port
		{
			get { return this.port; }
			set { this.port = value; }
		}

		public string Database
		{
			get { return this.database; }
			set { this.database = value; }
		}

		public int PacketSize
		{
			get { return this.packetSize; }
			set { this.packetSize = value; }
		}

		public string Role
		{
			get { return this.role; }
			set { this.role = value; }
		}

		public byte Dialect
		{
			get { return this.dialect; }
			set { this.dialect = value; }
		}

		public Charset Charset
		{
			get { return this.charset; }
			set { this.charset = value; }
		}

		public int Timeout
		{
			get { return this.timeout; }
			set { this.timeout = value; }
		}

		public bool Pooling
		{
			get { return this.pooling; }
			set { this.pooling = value; }
		}

		public long LifeTime
		{
			get { return this.lifetime; }
			set { this.lifetime = value; }
		}

		public int MinPoolSize
		{
			get { return this.minPoolSize; }
			set { this.minPoolSize = value; }
		}

		public int MaxPoolSize
		{
			get { return this.maxPoolSize; }
			set { this.maxPoolSize = value; }
		}

		public int ServerType
		{
			get { return this.serverType; }
			set { this.serverType = value; }
		}

		#endregion

		#region Constructors

		public AttachmentParams()
		{
			if (Charset.SupportedCharsets == null)
			{
				Charset.InitializeSupportedCharsets();
			}

			this.userName		= String.Empty;
			this.userPassword	= String.Empty;
			this.database		= String.Empty;
			this.role			= String.Empty;		
			this.charset		= Charset.DefaultCharset;
			this.dataSource		= "localhost";
			this.port			= 3050;
			this.dialect		= 3;
			this.packetSize		= 8192;
			this.timeout		= 15;
            this.pooling        = true;
            this.lifetime       = 0;
            this.minPoolSize	= 0;
			this.maxPoolSize	= 100;
        }

		public AttachmentParams(string connectionString) : this()
		{
			this.ConnectionString = connectionString;
		}
		
		#endregion

		#region Methods

		public DpbBuffer BuildDpb(bool isLittleEndian)
		{
			DpbBuffer dpb = new DpbBuffer(isLittleEndian);

			dpb.Append(IscCodes.isc_dpb_version1);
			dpb.Append(IscCodes.isc_dpb_dummy_packet_interval, 
				new byte[] {120, 10, 0, 0});
			dpb.Append(IscCodes.isc_dpb_sql_dialect, 
				new byte[] {this.dialect, 0, 0, 0});
			dpb.Append(IscCodes.isc_dpb_lc_ctype, this.charset.Name);
			if (this.role != null)
			{
				if (this.role.Length > 0)
				{
					dpb.Append(IscCodes.isc_dpb_sql_role_name, this.role);
				}
			}
			dpb.Append(IscCodes.isc_dpb_connect_timeout, this.timeout);
			dpb.Append(IscCodes.isc_dpb_user_name, this.userName);
			dpb.Append(IscCodes.isc_dpb_password, this.userPassword);

			return dpb;
		}

		#endregion

		#region Private Methods

		private void parseConnectionString()
		{
			bool dataSourceSet = false;
			
			string[] keyPairs = this.connectionString.Split(';');

			foreach (string keyPair in keyPairs)
			{
				string[] values = keyPair.Split('=');

				if (values.Length == 2 &&
					values[0] != null && values[0].Length > 0 &&
					values[1] != null && values[1].Length > 0)
				{
					values[0] = values[0].Trim().ToLower(CultureInfo.CurrentCulture);
					values[1] = values[1].Trim();

					switch (values[0])
					{
						case "database":
							this.database = values[1];
							break;

						case "datasource":
						case "data source":
						case "server":
						case "host":
							this.dataSource	= values[1];
							if (this.dataSource.Length > 0)
							{
								dataSourceSet = true;
							}
							break;

						case "user name":
						case "user":
						case "userid":
						case "user id":
							this.userName = values[1];
							break;

						case "user password":
						case "password":
							this.userPassword = values[1];
							break;

						case "port":
							this.port = Int32.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat);
							break;

						case "connection lifetime":
							this.lifetime = Int32.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat);
							break;

						case "min pool size":
							this.minPoolSize = Int32.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat);
							break;

						case "max pool size":
							this.maxPoolSize = Int32.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat);
							break;

						case "timeout":
						case "connection timeout":
							this.timeout = Int32.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat);
							break;

						case "packet size":
							this.packetSize = Int32.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat);
							break;

						case "pooling":
							this.pooling = Boolean.Parse(values[1]);
							break;

						case "dialect":
							this.dialect = byte.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat);
							break;

						case "charset":
						{
							int	index = Charset.SupportedCharsets.IndexOf(values[1]);
							if (index == -1)
							{
								charset = null;
							}
							else
							{
								charset = Charset.SupportedCharsets[index];
							}
						}
						break;

						case "role":
						case "rolename":
						case "role name":
							this.role = values[1];
							break;

						case "servertype":
						case "server type":
							this.serverType = Int32.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat);
							break;
					}
				}
			}

			if (!dataSourceSet)
			{
				this.parseConnectionInfo(
					this.database, 
					ref this.database, 
					ref this.dataSource, 
					ref this.port);
			}

			if ((this.userName == null || this.userName.Length == 0) ||
				(this.userPassword == null || this.userPassword.Length == 0) ||
				(this.database == null || this.database.Length == 0) ||
				(this.dataSource == null || this.dataSource.Length == 0) ||
				this.port == 0 ||
				this.charset == null ||
				(this.serverType != 0 && this.serverType  != 1)	||
				(this.minPoolSize > this.maxPoolSize))
			{
				throw new ArgumentException("An invalid connection string argument has been supplied or a required connection string argument has not been supplied.");
			}
			else
			{
				if (this.dialect < 1 || this.dialect > 3)
				{
					throw new ArgumentException("Incorrect database dialect it should be 1, 2, or 3.");
				}

				if (this.packetSize < 512 || this.packetSize > 32767)
				{
					StringBuilder msg = new StringBuilder();

					msg.AppendFormat("'Packet Size' value of {0} is not valid.\r\nThe value should be an integer >= 512 and <= 32767.", this.PacketSize);

					throw new ArgumentException(msg.ToString());
				}
			}
		}

		private void parseConnectionInfo(
			string		connectInfo, 
			ref string	database, 
			ref string	dataSource, 
			ref int		port)
		{
			// allows standard syntax //host:port/....
			// and old fb syntax host/port:....
			connectInfo = connectInfo.Trim();
			char hostSepChar;
			char portSepChar;
			if (connectInfo.StartsWith("//"))
			{
				connectInfo = connectInfo.Substring(2);
				hostSepChar = '/';
				portSepChar = ':';
			}
			else 
			{
				hostSepChar = ':';
				portSepChar = '/';
			}

			int sep = connectInfo.IndexOf(hostSepChar);
			if (sep == 0 || sep == connectInfo.Length - 1) 
			{
				throw new ArgumentException("An invalid connection string argument has been supplied or a required connection string argument has not been supplied.");
			}
			else if (sep > 0) 
			{
				dataSource	= connectInfo.Substring(0, sep);
				database	= connectInfo.Substring(sep + 1);
				int portSep = dataSource.IndexOf(portSepChar);
				if (portSep == 0 || portSep == dataSource.Length - 1) 
				{
					throw new ArgumentException("An invalid connection string argument has been supplied or a required connection string argument has not been supplied.");
				}
				else if (portSep > 0) 
				{
					port = int.Parse(dataSource.Substring(portSep + 1), CultureInfo.InvariantCulture.NumberFormat);
					dataSource = dataSource.Substring(0, portSep);
				}
				else if (portSep < 0 && dataSource.Length == 1)
				{
					dataSource	= "localhost";
					database	= connectInfo;
				}
			}
			else if (sep == -1) 
			{
				database = connectInfo;
			}
		}

		#endregion

		#region Overriden methods

		public override string ToString()
		{
			return String.Format(
				"Data Source={0};Database={1};User Id={2};Password={3};" +
				"Dialect={4};Charset={5};Pooling={6};Connection Lifetime={7};" +
				"Role={8};PacketSize={9};Connection Timeout={10};" +
				"Pooling={11};Min pool size={12};Max pool size={13}" +
				"ServerType={14}",
				this.DataSource,
				this.Database,
				this.UserName,
				this.UserPassword,
				this.Dialect,
				this.Charset,
				this.Pooling,
				this.LifeTime,
				this.Role,
				this.PacketSize,
				this.Timeout,
				this.Pooling,
				this.MinPoolSize,
				this.MaxPoolSize,
				this.ServerType);
		}

		#endregion
	}
}
