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

namespace FirebirdSql.Data.Services
{
	public sealed class FbUserData
	{
		private string _userName;
		private string _firstName;
		private string _lastName;
		private string _middleName;
		private string _userPassword;
		private string _groupName;
		private string _roleName;
		private int _userID;
		private int _groupID;

		public string UserName
		{
			get { return _userName; }
			set
			{
				if (value == null)
				{
					throw new InvalidOperationException("The user name cannot be null.");
				}
				if (value.Length > 31)
				{
					throw new InvalidOperationException("The user name cannot have more than 31 characters.");
				}

				_userName = value;
			}
		}

		public string UserPassword
		{
			get { return _userPassword; }
			set
			{
				if (value == null)
				{
					throw new InvalidOperationException("The user password cannot be null.");
				}
				if (value.Length > 31)
				{
					throw new InvalidOperationException("The user password cannot have more than 31 characters.");
				}

				_userPassword = value;
			}
		}

		public string FirstName
		{
			get { return _firstName; }
			set { _firstName = value; }
		}

		public string LastName
		{
			get { return _lastName; }
			set { _lastName = value; }
		}

		public string MiddleName
		{
			get { return _middleName; }
			set { _middleName = value; }
		}

		public int UserID
		{
			get { return _userID; }
			set { _userID = value; }
		}

		public int GroupID
		{
			get { return _groupID; }
			set { _groupID = value; }
		}

		public string GroupName
		{
			get { return _groupName; }
			set { _groupName = value; }
		}

		public string RoleName
		{
			get { return _roleName; }
			set { _roleName = value; }
		}

		public FbUserData()
		{
			_userName = string.Empty;
			_firstName = string.Empty;
			_lastName = string.Empty;
			_middleName = string.Empty;
			_userPassword = string.Empty;
			_roleName = string.Empty;
		}
	}
}
