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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.ComponentModel;
using System.Data.Common;
using System.Runtime.Serialization;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.FirebirdClient;

#if !NET8_0_OR_GREATER
[Serializable]
#endif
public sealed class FbException : DbException
{
	#region Fields

	private FbErrorCollection _errors;

	#endregion

	#region Properties

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public FbErrorCollection Errors => _errors ??= new FbErrorCollection();

	public override int ErrorCode => (InnerException as IscException)?.ErrorCode ?? 0;

	public string SQLSTATE => (InnerException as IscException)?.SQLSTATE;

	#endregion

	#region Constructors

	private FbException(string message, Exception innerException)
		: base(message, innerException)
	{ }

#if !NET8_0_OR_GREATER
	private FbException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		_errors = (FbErrorCollection)info.GetValue("errors", typeof(FbErrorCollection));
	}
#endif

	#endregion

	#region Methods

#if !NET8_0_OR_GREATER
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);

		info.AddValue("errors", _errors);
	}
#endif

	#endregion

	#region Private Methods

	private void ProcessIscExceptionErrors(IscException innerException)
	{
		foreach (var error in innerException.Errors)
		{
			Errors.Add(error.Message, error.ErrorCode);
		}
	}

	#endregion

	internal static Exception Create(string message) => Create(message, null);
	internal static Exception Create(Exception innerException) => Create(null, innerException);
	internal static Exception Create(string message, Exception innerException)
	{
		message ??= innerException?.Message;
		if (innerException is IscException iscException)
		{
			if (iscException.ErrorCode == IscCodes.isc_cancelled)
			{
				return new OperationCanceledException(message, innerException);
			}
			else
			{
				var result = new FbException(message, innerException);
				result.ProcessIscExceptionErrors(iscException);
				return result;
			}
		}
		else
		{
			return new FbException(message, innerException);
		}
	}
}
