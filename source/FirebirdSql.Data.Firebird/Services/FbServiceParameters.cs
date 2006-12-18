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

namespace FirebirdSql.Data.Firebird.Services
{
	/// <include file='Doc/en_EN/FbServiceParameters.xml' path='doc/class[@name="FbServiceParameters"]/overview/*'/>
	public sealed class FbServiceParameters
	{
		#region Fields

		private string	userName;
		private string	userPassword;
		private string	dataSource;
		private int		port;
		private byte	dialect;
		private string	database;
		private string	role;
		private int		packetSize;
		private int		serverType;

		#endregion

		#region Properties

		/// <include file='Doc/en_EN/FbServiceParameters.xml' path='doc/property[@name="UserName"]/*'/>
		public string UserName
		{
			get { return this.userName; }
			set { this.userName = value; }
		}

		/// <include file='Doc/en_EN/FbServiceParameters.xml' path='doc/property[@name="UserPassword"]/*'/>
		public string UserPassword
		{
			get { return this.userPassword; }
			set { this.userPassword = value; }
		}

		/// <include file='Doc/en_EN/FbServiceParameters.xml' path='doc/property[@name="DataSource"]/*'/>
		public string DataSource
		{
			get { return this.dataSource; }
			set { this.dataSource = value; }
		}

		/// <include file='Doc/en_EN/FbServiceParameters.xml' path='doc/property[@name="Port"]/*'/>
		public int Port
		{
			get { return this.port; }
			set { this.port = value; }
		}

		/// <include file='Doc/en_EN/FbServiceParameters.xml' path='doc/property[@name="Dialect"]/*'/>
		public byte	Dialect
		{
			get { return this.dialect; }
			set { this.dialect = value; }
		}

		/// <include file='Doc/en_EN/FbServiceParameters.xml' path='doc/property[@name="Database"]/*'/>
		public string Database
		{
			get { return this.database; }
			set { this.database = value; }
		}

		/// <include file='Doc/en_EN/FbServiceParameters.xml' path='doc/property[@name="PacketSize"]/*'/>
		public int PacketSize
		{
			get { return this.packetSize; }
			set { this.packetSize = value; }
		}

		/// <include file='Doc/en_EN/FbServiceParameters.xml' path='doc/property[@name="Role"]/*'/>
		public string Role
		{
			get { return this.role; }
			set { this.role = value; }
		}

		/// <include file='Doc/en_EN/FbServiceParameters.xml' path='doc/property[@name="ServerType"]/*'/>
		public int ServerType
		{
			get { return this.serverType; }
			set { this.serverType = value; }
		}

		#endregion

		#region Constructors
		
		/// <include file='Doc/en_EN/FbServiceParameters.xml' path='doc/constructor[@name="ctor"]/*'/>
		public FbServiceParameters()
		{
			this.userName		= String.Empty;
			this.userPassword	= String.Empty;
			this.database		= String.Empty;
			this.role			= String.Empty;		
			this.dataSource		= "localhost";
			this.port			= 3050;
			this.dialect		= 3;
			this.packetSize		= 8192;
		}
		
		#endregion
	}
}
