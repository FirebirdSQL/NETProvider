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

namespace FirebirdSql.Data.Firebird.Gds
{
	internal class GdsCharset
	{
		#region FIELDS

		private int			id;
		private string		name;
		private Encoding	encoding;
		private int			bytesPerCharacter;

		#endregion

		#region PROPERTIES

		public int ID
		{
			get { return id; }
		}

		public string Name
		{
			get { return name; }
		}

		public Encoding Encoding
		{
			get { return encoding; }
		}

		public int BytesPerCharacter
		{
			get { return bytesPerCharacter; }
		}

		#endregion

		#region CONSTRUCTORS

		public GdsCharset()
		{
		}

		public GdsCharset(
			int id, 
			string name, 
			int bytesPerCharacter, 
			string systemCharset)
		{
			this.id					= id;
			this.name				= name;
			this.bytesPerCharacter	= bytesPerCharacter;
			this.encoding			= Encoding.GetEncoding(systemCharset);
		}

		public GdsCharset(
			int id, 
			string name,
			int bytesPerCharacter, 
			int cp)
		{
			this.id					= id;
			this.name				= name;
			this.bytesPerCharacter	= bytesPerCharacter;
			this.encoding			= Encoding.GetEncoding(cp);
		}

		public GdsCharset(
			int id, 
			string name, 
			int bytesPerCharacter,
			Encoding encoding)
		{
			this.id					= id;
			this.name				= name;
			this.bytesPerCharacter	= bytesPerCharacter;
			this.encoding			= encoding;
		}

		#endregion
	}
}
