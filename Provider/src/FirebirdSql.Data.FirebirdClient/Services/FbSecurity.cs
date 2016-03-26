/*
 *	Firebird ADO.NET Data provider for .NET and Mono
 *
 *	   The contents of this file are subject to the Initial
 *	   Developer's Public License Version 1.0 (the "License");
 *	   you may not use this file except in compliance with the
 *	   License. You may obtain a copy of the License at
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *	   express or implied. See the License for the specific
 *	   language governing rights and limitations under the License.
 *
 *	Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.Collections;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Services
{
	public sealed class FbSecurity : FbService
	{
		#region Constructors

		public FbSecurity(string connectionString = null)
			: base(connectionString)
		{ }

		#endregion

		#region Methods

		public void AddUser(FbUserData user)
		{
			if (user.UserName != null && user.UserName.Length == 0)
			{
				throw new InvalidOperationException("Invalid user name.");
			}
			if (user.UserPassword != null && user.UserPassword.Length == 0)
			{
				throw new InvalidOperationException("Invalid user password.");
			}

			// Configure Spb
			StartSpb = new ServiceParameterBuffer();

			StartSpb.Append(IscCodes.isc_action_svc_add_user);

			StartSpb.Append(IscCodes.isc_spb_sec_username, user.UserName);
			StartSpb.Append(IscCodes.isc_spb_sec_password, user.UserPassword);

			if (user.FirstName != null && user.FirstName.Length > 0)
			{
				StartSpb.Append(IscCodes.isc_spb_sec_firstname, user.FirstName);
			}

			if (user.MiddleName != null && user.MiddleName.Length > 0)
			{
				StartSpb.Append(IscCodes.isc_spb_sec_middlename, user.MiddleName);
			}

			if (user.LastName != null && user.LastName.Length > 0)
			{
				StartSpb.Append(IscCodes.isc_spb_sec_lastname, user.LastName);
			}

			if (user.UserID != 0)
			{
				StartSpb.Append(IscCodes.isc_spb_sec_userid, user.UserID);
			}

			if (user.GroupID != 0)
			{
				StartSpb.Append(IscCodes.isc_spb_sec_groupid, user.GroupID);
			}

			if (user.GroupName != null && user.GroupName.Length > 0)
			{
				StartSpb.Append(IscCodes.isc_spb_sec_groupname, user.GroupName);
			}

			if (user.RoleName != null && user.RoleName.Length > 0)
			{
				StartSpb.Append(IscCodes.isc_spb_sql_role_name, user.RoleName);
			}

			Open();

			// Start execution
			StartTask();

			Close();
		}

		public void DeleteUser(FbUserData user)
		{
			if (user.UserName != null && user.UserName.Length == 0)
			{
				throw new InvalidOperationException("Invalid user name.");
			}

			// Configure Spb
			StartSpb = new ServiceParameterBuffer();

			StartSpb.Append(IscCodes.isc_action_svc_delete_user);

			StartSpb.Append(IscCodes.isc_spb_sec_username, user.UserName);

			if (user.RoleName != null && user.RoleName.Length > 0)
			{
				StartSpb.Append(IscCodes.isc_spb_sql_role_name, user.RoleName);
			}

			Open();

			// Start execution
			StartTask();

			Close();
		}

		public void ModifyUser(FbUserData user)
		{
			if (user.UserName != null && user.UserName.Length == 0)
			{
				throw new InvalidOperationException("Invalid user name.");
			}
			if (user.UserPassword != null && user.UserPassword.Length == 0)
			{
				throw new InvalidOperationException("Invalid user password.");
			}

			// Configure Spb
			StartSpb = new ServiceParameterBuffer();

			StartSpb.Append(IscCodes.isc_action_svc_modify_user);
			StartSpb.Append(IscCodes.isc_spb_sec_username, user.UserName);

			if (user.UserPassword != null && user.UserPassword.Length > 0)
			{
				StartSpb.Append(IscCodes.isc_spb_sec_password, user.UserPassword);
			}

			if (user.FirstName != null && user.FirstName.Length > 0)
			{
				StartSpb.Append(IscCodes.isc_spb_sec_firstname, user.FirstName);
			}

			if (user.MiddleName != null && user.MiddleName.Length > 0)
			{
				StartSpb.Append(IscCodes.isc_spb_sec_middlename, user.MiddleName);
			}

			if (user.LastName != null && user.LastName.Length > 0)
			{
				StartSpb.Append(IscCodes.isc_spb_sec_lastname, user.LastName);
			}

			StartSpb.Append(IscCodes.isc_spb_sec_userid, user.UserID);
			StartSpb.Append(IscCodes.isc_spb_sec_groupid, user.GroupID);

			if (user.GroupName != null && user.GroupName.Length > 0)
			{
				StartSpb.Append(IscCodes.isc_spb_sec_groupname, user.GroupName);
			}

			if (user.RoleName != null && user.RoleName.Length > 0)
			{
				StartSpb.Append(IscCodes.isc_spb_sql_role_name, user.RoleName);
			}

			Open();

			// Start execution
			StartTask();

			Close();
		}

		public FbUserData DisplayUser(string userName)
		{
			// Configure Spb
			StartSpb = new ServiceParameterBuffer();

			StartSpb.Append(IscCodes.isc_action_svc_display_user);
			StartSpb.Append(IscCodes.isc_spb_sec_username, userName);

			Open();

			// Start execution
			StartTask();

			ArrayList info = Query(new byte[] { IscCodes.isc_info_svc_get_users });

			Close();

			if (info.Count == 0)
			{
				return null;
			}

			FbUserData[] users = (FbUserData[])info[0];

			return (users != null && users.Length > 0) ? users[0] : null;
		}

		public FbUserData[] DisplayUsers()
		{
			// Configure Spb
			StartSpb = new ServiceParameterBuffer();

			StartSpb.Append(IscCodes.isc_action_svc_display_user);

			Open();

			// Start execution
			StartTask();

			ArrayList info = Query(new byte[] { IscCodes.isc_info_svc_get_users });

			Close();

			if (info.Count == 0)
			{
				return null;
			}

			return (FbUserData[])info[0];
		}

		public string GetUsersDbPath()
		{
			ArrayList info = Query(new byte[] { IscCodes.isc_info_svc_user_dbpath });
			return info.Count != 0 ? (string)info[0] : null;
		}

		#endregion
	}
}
