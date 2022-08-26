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
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Services;

public sealed class FbValidation : FbService
{
	public FbValidationFlags Options { get; set; }

	public FbValidation(string connectionString = null)
		: base(connectionString)
	{ }

	public void Execute()
	{
		EnsureDatabase();

		try
		{
			try
			{
				Open();
				var startSpb = new ServiceParameterBuffer2(Service.ParameterBufferEncoding);
				startSpb.Append(IscCodes.isc_action_svc_repair);
				startSpb.Append2(IscCodes.isc_spb_dbname, ConnectionStringOptions.Database);
				startSpb.Append(IscCodes.isc_spb_options, (int)Options);
				if (ConnectionStringOptions.ParallelWorkers > 0)
					startSpb.Append(IscCodes.isc_spb_rpr_par_workers, ConnectionStringOptions.ParallelWorkers);
				StartTask(startSpb);
				ProcessServiceOutput(new ServiceParameterBuffer2(Service.ParameterBufferEncoding));
			}
			finally
			{
				Close();
			}
		}
		catch (Exception ex)
		{
			throw FbException.Create(ex);
		}
	}
	public async Task ExecuteAsync(CancellationToken cancellationToken = default)
	{
		EnsureDatabase();

		try
		{
			try
			{
				await OpenAsync(cancellationToken).ConfigureAwait(false);
				var startSpb = new ServiceParameterBuffer2(Service.ParameterBufferEncoding);
				startSpb.Append(IscCodes.isc_action_svc_repair);
				startSpb.Append2(IscCodes.isc_spb_dbname, ConnectionStringOptions.Database);
				startSpb.Append(IscCodes.isc_spb_options, (int)Options);
				if (ConnectionStringOptions.ParallelWorkers > 0)
					startSpb.Append(IscCodes.isc_spb_rpr_par_workers, ConnectionStringOptions.ParallelWorkers);
				await StartTaskAsync(startSpb, cancellationToken).ConfigureAwait(false);
				await ProcessServiceOutputAsync(new ServiceParameterBuffer2(Service.ParameterBufferEncoding), cancellationToken).ConfigureAwait(false);
			}
			finally
			{
				await CloseAsync(cancellationToken).ConfigureAwait(false);
			}
		}
		catch (Exception ex)
		{
			throw FbException.Create(ex);
		}
	}
}
