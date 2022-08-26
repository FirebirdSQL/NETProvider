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

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Services;

public sealed class FbStreamingBackup : FbService
{
	public string SkipData { get; set; }
	public FbBackupFlags Options { get; set; }
	public Stream OutputStream { get; set; }

	public FbStreamingBackup(string connectionString = null)
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
				startSpb.Append(IscCodes.isc_action_svc_backup);
				startSpb.Append2(IscCodes.isc_spb_dbname, ConnectionStringOptions.Database);
				startSpb.Append2(IscCodes.isc_spb_bkp_file, "stdout");
				if (!string.IsNullOrEmpty(SkipData))
					startSpb.Append2(IscCodes.isc_spb_bkp_skip_data, SkipData);
				startSpb.Append(IscCodes.isc_spb_options, (int)Options);
				if (ConnectionStringOptions.ParallelWorkers > 0)
					startSpb.Append(IscCodes.isc_spb_bkp_parallel_workers, ConnectionStringOptions.ParallelWorkers);
				StartTask(startSpb);
				ReadOutput();
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
				startSpb.Append(IscCodes.isc_action_svc_backup);
				startSpb.Append2(IscCodes.isc_spb_dbname, ConnectionStringOptions.Database);
				startSpb.Append2(IscCodes.isc_spb_bkp_file, "stdout");
				if (!string.IsNullOrEmpty(SkipData))
					startSpb.Append2(IscCodes.isc_spb_bkp_skip_data, SkipData);
				startSpb.Append(IscCodes.isc_spb_options, (int)Options);
				if (ConnectionStringOptions.ParallelWorkers > 0)
					startSpb.Append(IscCodes.isc_spb_bkp_parallel_workers, ConnectionStringOptions.ParallelWorkers);
				await StartTaskAsync(startSpb, cancellationToken).ConfigureAwait(false);
				await ReadOutputAsync(cancellationToken).ConfigureAwait(false);
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

	void ReadOutput()
	{
		Query(new byte[] { IscCodes.isc_info_svc_to_eof }, new ServiceParameterBuffer2(Service.ParameterBufferEncoding), (_, x) =>
		{
			var buffer = x as byte[];
			OutputStream.Write(buffer, 0, buffer.Length);
		});
	}
	Task ReadOutputAsync(CancellationToken cancellationToken = default)
	{
		return QueryAsync(new byte[] { IscCodes.isc_info_svc_to_eof }, new ServiceParameterBuffer2(Service.ParameterBufferEncoding), async (_, x) =>
		{
			var buffer = x as byte[];
			await OutputStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
		}, cancellationToken);
	}
}
