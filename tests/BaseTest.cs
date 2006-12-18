//
// Firebird .NET Data Provider - Firebird managed data provider for .NET and Mono
// Copyright (C) 2002-2003  Carlos Guzman Alvarez
//
// Distributable under LGPL license.
// You may obtain a copy of the License at http://www.gnu.org/copyleft/lesser.html
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//

using System;
using System.Configuration;


namespace FirebirdSql.Data.Firebird.Tests
{
	public class BaseTest
	{
		private string 	server;
		private string 	port;
		private string 	database;
		private string 	user;
		private string 	password;
		private string 	charset;
		private string 	role;
		private string 	dialect;
		private string	pooling;
		private string	lifetime;
		private string	packetSize;

		public string Server
		{
			get { return server; }
		}

		public string Port
		{
			get { return port; }
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
		
		public string Charset
		{
			get { return charset; }
		}

		public string Role
		{
			get { return role; }
		}

		public string Dialect
		{
			get { return dialect; }
		}

		public string Lifetime
		{
			get { return lifetime; }
		}

		public string Pooling
		{
			get { return pooling; }
		}
		
		public string PacketSize
		{
			get { return packetSize; }
		}

		public BaseTest()
		{
			server		= ConfigurationSettings.AppSettings["Server"];
			port 		= ConfigurationSettings.AppSettings["Port"];
			database	= ConfigurationSettings.AppSettings["Database"];			
			user 		= ConfigurationSettings.AppSettings["User"];
			password	= ConfigurationSettings.AppSettings["Password"];
			charset		= ConfigurationSettings.AppSettings["Charset"];
			role		= ConfigurationSettings.AppSettings["Role"];
			dialect		= ConfigurationSettings.AppSettings["Dialect"];
			lifetime	= ConfigurationSettings.AppSettings["Connection lifetime"];
			pooling		= ConfigurationSettings.AppSettings["Pooling"];
			packetSize	= ConfigurationSettings.AppSettings["Packet Size"];
		}

		public string GetConnectionString()
		{			
			return "Database="				+ Database	+ ";" + 
					"User="					+ User		+ ";" + 
					"Password="				+ Password	+ ";" + 
					"Server="				+ Server	+ ";" + 
					"Port="					+ Port		+ ";" + 
					"Dialect="				+ Dialect	+ ";" + 
					"Charset="				+ Charset	+ ";" +
					"Role="					+ Role		+ ";" +
					"Connection Lifetime="	+ Lifetime	+ ";" +
					"Pooling="				+ Pooling	+ ";" +
					"Packet Size="			+ PacketSize;
		}
	}
}
