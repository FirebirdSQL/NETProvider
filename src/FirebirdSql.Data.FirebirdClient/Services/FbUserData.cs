/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Carlos Guzman Alvarez

using System;

namespace FirebirdSql.Data.Services;

public sealed class FbUserData
{
	private string _userName;
	public string UserName
	{
		get { return _userName; }
		set
		{
			if (value == null)
				throw new InvalidOperationException("The user name cannot be null.");
			if (value.Length > 31)
				throw new InvalidOperationException("The user name cannot have more than 31 characters.");
			_userName = value;
		}
	}

	private string _userPassword;
	public string UserPassword
	{
		get { return _userPassword; }
		set
		{
			if (value == null)
				throw new InvalidOperationException("The user password cannot be null.");
			if (value.Length > 31)
				throw new InvalidOperationException("The user password cannot have more than 31 characters.");
			_userPassword = value;
		}
	}

	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string MiddleName { get; set; }
	public int UserID { get; set; }
	public int GroupID { get; set; }
	public string GroupName { get; set; }
	public string RoleName { get; set; }

	public FbUserData()
	{
		UserName = string.Empty;
		UserPassword = string.Empty;
		FirstName = string.Empty;
		LastName = string.Empty;
		MiddleName = string.Empty;
		RoleName = string.Empty;
	}
}
