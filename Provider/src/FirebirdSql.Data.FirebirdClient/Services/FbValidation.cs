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
 *
 *  Contributors:
 *   Jiri Cincura (jiri@cincura.net)
 */

using System;

using FirebirdSql.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Services
{
	public sealed class FbValidation : FbService
	{
		public FbValidationFlags Options { get; set; }

		public FbValidation(string connectionString = null)
			: base(connectionString)
		{ }

		public void Execute()
		{
			if (string.IsNullOrEmpty(Database))
			{
				throw new FbException("Validation should be used against a specific database");
			}

			try
			{
				StartSpb = new ServiceParameterBuffer();
				StartSpb.Append(IscCodes.isc_action_svc_repair);
				StartSpb.Append(IscCodes.isc_spb_dbname, Database);
				StartSpb.Append(IscCodes.isc_spb_options, (int)Options);

				Open();
				StartTask();
				ProcessServiceOutput();
			}
			catch (Exception ex)
			{
				throw new FbException(ex.Message, ex);
			}
			finally
			{
				Close();
			}
		}
	}
}
