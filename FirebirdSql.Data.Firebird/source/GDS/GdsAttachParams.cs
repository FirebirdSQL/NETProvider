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
using System.Text.RegularExpressions;

namespace FirebirdSql.Data.Firebird.Gds
{
	internal class GdsAttachParams
	{
		#region FIELDS

		private string		userName;
		private string		userPassword;
		private string		dataSource;
		private int			port;
		private string		database;
		private int			packetSize;
		private	byte		dialect;
		private string		role;
		private int			timeout;
		private bool		pooling;
		private long		lifetime;
		private GdsCharset	charset;

		#endregion

		#region PROPERTIES

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

		public GdsCharset Charset
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

		#endregion

		#region CONSTRUCTORS

		public GdsAttachParams()
		{
			if (GdsDbAttachment.CharSets == null)
			{
				GdsDbAttachment.InitializeCharSets();
			}

			this.userName		= String.Empty;
			this.userPassword	= String.Empty;
			this.database		= String.Empty;
			this.role			= String.Empty;		
			this.charset		= GdsDbAttachment.CharSets[0];
			this.dataSource		= "localhost";
			this.port			= 3050;
			this.dialect		= 3;			
			this.packetSize		= 8192;
			this.timeout		= 15;
		}

		public GdsAttachParams(string connectionString) : this()
		{
			this.ParseConnectionString(connectionString);
		}

		public GdsAttachParams(string dataSource, int port, string database, int packetSize) : this()
		{
			if (database == null || database.Equals(String.Empty)) 
			{
				throw new GdsException("null filename in DbAttachInfo");
			}
			if (dataSource != null) 
			{
				this.dataSource = dataSource;
			}
			this.database	= database;
			this.port		= port;
			this.packetSize	= packetSize;
		}
		
		#endregion

		#region METHODS

		public void ParseConnectionString(string connectionString)
		{
			string		userName		= String.Empty;
			string		userPassword	= String.Empty;
			string		dataSource		= "localhost";
			int			port			= 3050;
			string		database		= String.Empty;
			int			packetSize		= 8192;
			byte		dialect			= 3;
			string		role			= String.Empty;
			int			timeout			= 15;
			bool		pooling			= true;
			long		lifetime		= 0;
			bool		dataSourceSet	= false;
			GdsCharset	charset			= GdsDbAttachment.CharSets["NONE"];

			MatchCollection	elements = Regex.Matches(connectionString, @"([\w\s\d]*)\s*=\s*([^;]*)");

			foreach (Match element in elements)
			{
				if (element.Groups[2].Value.Trim().Length > 0)
				{
					switch (element.Groups[1].Value.Trim().ToLower())
					{
						case "database":
							database = element.Groups[2].Value.Trim();
							break;

						case "datasource":
						case "data source":
						case "server":
						case "host":
							dataSourceSet	= true;
							dataSource		= element.Groups[2].Value.Trim();
							break;

						case "user name":
						case "user":
						case "userid":
						case "user id":
							userName = element.Groups[2].Value.Trim();
							break;

						case "user password":
						case "password":
							userPassword = element.Groups[2].Value.Trim();
							break;

						case "port":
							port = Int32.Parse(element.Groups[2].Value.Trim());
							break;

						case "connection lifetime":
							lifetime = Int32.Parse(element.Groups[2].Value.Trim());
							break;

						case "timeout":
						case "connection timeout":
							timeout = Int32.Parse(element.Groups[2].Value.Trim());
							break;

						case "packet size":
							packetSize = Int32.Parse(element.Groups[2].Value.Trim());
							break;

						case "pooling":
							pooling = Boolean.Parse(element.Groups[2].Value.Trim());
							break;

						case "dialect":
							dialect = byte.Parse(element.Groups[2].Value.Trim());
							break;

						case "charset":
						{
							string name = element.Groups[2].Value.Trim();
							if (GdsDbAttachment.CharSets.IndexOf(name) == -1)
							{
								charset = null;
							}
							else
							{
								charset = GdsDbAttachment.CharSets[name];
							}
						}
						break;

						case "role":
							role = element.Groups[2].Value.Trim();
							break;
					}
				}
			}

			if (!dataSourceSet)
			{
				this.parseConnectionInfo(
					database, 
					ref database, 
					ref dataSource, 
					ref port);
			}

			if (userName == String.Empty		|| 
				userPassword == String.Empty	|| 
				database == String.Empty		|| 
				dataSource == String.Empty		|| 
				charset == null					||
				port == 0)
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

			// Update attachement parameters
			this.userName		= userName;
			this.userPassword	= userPassword;
			this.dataSource		= dataSource;
			this.port			= port;
			this.database		= database;
			this.packetSize		= packetSize;
			this.dialect		= dialect;
			this.role			= role;
			this.timeout		= timeout;
			this.pooling		= pooling;
			this.lifetime		= lifetime * TimeSpan.TicksPerSecond;
			this.charset		= charset;
		}

		private void parseConnectionInfo(
			string connectInfo, 
			ref string database, 
			ref string dataSource, 
			ref int port)
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
					port = int.Parse(dataSource.Substring(portSep + 1));							
					dataSource = dataSource.Substring(0, portSep);
				}
			}
			else if (sep == -1) 
			{
				database = connectInfo;
			}
		}

		#endregion
	}
}
