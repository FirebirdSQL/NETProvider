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

using FirebirdSql.Data.INGDS;
using FirebirdSql.Data.NGDS;

namespace FirebirdSql.Data.Firebird
{
	/// <include file='xmldoc/fbconnectionrequestinfo.xml' path='doc/member[@name="T:FbConnectionRequestInfo"]/*'/>
	internal class FbConnectionRequestInfo
	{
		#region FIELDS

		private IClumplet c;

		#endregion

		#region PROPERTIES

		internal IClumplet Dpb
		{
			get { return c; }
		}

		#endregion

		#region CONSTRUCTORS
		
		/// <include file='xmldoc/fbconnectionrequestinfo.xml' path='doc/member[@name="M:#ctor"]/*'/>
		public FbConnectionRequestInfo() 
		{
			c = null;
		}

		/// <include file='xmldoc/fbconnectionrequestinfo.xml' path='doc/member[@name="M:#ctor(FirebirdSql.Data.Firebird.FbConnectionRequestInfo)"]/*'/>
		public FbConnectionRequestInfo(FbConnectionRequestInfo src) 
		{
			c = GDSFactory.CloneClumplet(src.c);
		}

		#endregion

		#region PROPERTIES

		public void SetProperty(int type, string content) 
		{
			Append(GDSFactory.NewClumplet(type, content));
		}

		public void SetProperty(int type) 
		{
			Append(GDSFactory.NewClumplet(type));
		}

		public void SetProperty(int type, int content) 
		{
			Append(GDSFactory.NewClumplet(type, content));
		}

		public void SetProperty(int type, short content) 
		{
			Append(GDSFactory.NewClumplet(type, content));
		}

		public void SetProperty(int type, byte[] content) 
		{
			Append(GDSFactory.NewClumplet(type, content));
		}

		public string GetStringProperty(int type)
		{
			if (c == null) 
			{
				return null;        
			}

			if (c.Find(type) == null) 
			{
				return null;        
			}

			return Encoding.Default.GetString(c.Find(type));
		}

		private void Append(IClumplet newc) 
		{
			if (c == null) 
			{
				c = newc;
			}
			else 
			{
				c.Append(newc);
			}
		}

		public void SetUser(string user) 
		{
			SetProperty(GdsCodes.isc_dpb_user_name, user);
		}

		public string GetUser()
		{
			return GetStringProperty(GdsCodes.isc_dpb_user_name);
		}

		public void SetPassword(string password) 
		{
			SetProperty(GdsCodes.isc_dpb_password, password);
		}

		public string GetPassword()
		{
			return GetStringProperty(GdsCodes.isc_dpb_password);
		}

		/// <include file='xmldoc/fbconnectionrequestinfo.xml' path='doc/member[@name="M:Equals(System.Object)"]/*'/>
		public override bool Equals(Object other) 
		{
			if ((other == null) || !(other is FbConnectionRequestInfo)) 
			{
			  return false;
			}
			IClumplet otherc = ((FbConnectionRequestInfo)other).c;
			if (c == null) 
			{
				return (otherc == null);
			}
			return c.Equals(otherc);
		}

		/// <include file='xmldoc/fbconnectionrequestinfo.xml' path='doc/member[@name="M:GetHashCode"]/*'/>
		public override int GetHashCode() 
		{
			if (c == null) 
			{
				return 0;
			}
			return c.GetHashCode();
		}

		#endregion
	}
}
