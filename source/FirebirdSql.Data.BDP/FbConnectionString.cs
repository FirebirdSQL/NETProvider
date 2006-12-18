/*
 *  Firebird BDP - Borland Data provider Firebird
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.ComponentModel;
using Borland.Data.Common;

namespace FirebirdSql.Data.Bdp
{
	// References:
	//	http://www.distribucon.com/bdp/BDP-DE.html
	public class FbConnectionString : BdpConnectionString
	{
		#region Protected Fields

		// Property Names
		// protected const string SRoleName	= "RoleName";
		protected const string SServerType	= "ServerType";
		protected const string SPacketSize	= "PacketSize";
		protected const string SDialect		= "Dialect";
		protected const string SCharset		= "Charset";
		
		// Property Values
		protected new const string SProvider	= "Firebird";
		protected const int DefServerType		= 0;
		protected const int DefDialect			= 3;
		protected const int DefPacketSize		= 8192;
		protected const string DefRoleName		= "";
		protected const string DefCharset		= "NONE";
		
		#endregion

		#region Properties

		[Category("Options")]
		[DefaultValue(DefRoleName)]
		public string RoleName
		{
			get { return base.OptionsDictionary[SRoleName]; } 
			set { base.OptionsDictionary[SRoleName] = value; }
		}

		[Category("Options")]
		[DefaultValue(DefDialect)]
		public int Dialect
		{
			get { return Convert.ToInt32(base.OptionsDictionary[SDialect]); } 
			set { base.OptionsDictionary[SDialect] = value.ToString(); }
		}

		[Category("Options")]
		[DefaultValue(DefCharset)]
		public string Charset
		{
			get { return base.OptionsDictionary[SCharset]; } 
			set { base.OptionsDictionary[SCharset] = value; }
		}

		[Category("Options")]
		[DefaultValue(DefServerType)]
		public int ServerType
		{
			get { return Convert.ToInt32(base.OptionsDictionary[SServerType]); } 
			set { base.OptionsDictionary[SServerType] = value.ToString(); }
		}

		[Category("Options")]
		[DefaultValue(DefPacketSize)]
		public int PacketSize
		{
			get { return Convert.ToInt32(base.OptionsDictionary[SPacketSize]); } 
			set { base.OptionsDictionary[SPacketSize] = value.ToString(); }
		}

		#endregion

		#region Constructors

		public FbConnectionString() : base()
		{
			this.SetProvider(SProvider);

			this.RoleName	= DefRoleName;
			this.ServerType = DefServerType;
			this.PacketSize	= DefPacketSize;
			this.Dialect	= DefDialect;
			this.Charset	= DefCharset;
		}

		#endregion
	}
}
