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
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.FirebirdClient;

public sealed class FbInfoMessageEventArgs : EventArgs
{
	#region Fields

	private FbErrorCollection _errors;
	private string _message;

	#endregion

	#region Properties

	public FbErrorCollection Errors
	{
		get { return _errors; }
	}

	public string Message
	{
		get { return _message; }
	}

	#endregion

	#region Constructors

	internal FbInfoMessageEventArgs(IscException ex)
	{
		_message = ex.Message;
		_errors = new FbErrorCollection();
		foreach (var error in ex.Errors)
		{
			_errors.Add(error.Message, error.ErrorCode);
		}
	}

	#endregion
}
