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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FirebirdSql.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Services;

public class FbStreamingRestore : FbService
{
	private int? _pageSize;
	public int? PageSize
	{
		get { return _pageSize; }
		set
		{
			if (value is int v && !SizeHelper.IsValidPageSize(v))
				throw SizeHelper.InvalidSizeException("page size");

			_pageSize = value;
		}
	}

	public Stream InputStream { get; set; }
	public bool Verbose { get; set; }
	public int? PageBuffers { get; set; }
	public bool ReadOnly { get; set; }
	public string SkipData { get; set; }
	public FbRestoreFlags Options { get; set; }

	public FbStreamingRestore(string connectionString = null)
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
				startSpb.Append(IscCodes.isc_action_svc_restore);
				startSpb.Append2(IscCodes.isc_spb_bkp_file, "stdin");
				startSpb.Append2(IscCodes.isc_spb_dbname, ConnectionStringOptions.Database);
				if (Verbose)
				{
					startSpb.Append(IscCodes.isc_spb_verbose);
				}
				if (PageBuffers.HasValue)
					startSpb.Append(IscCodes.isc_spb_res_buffers, (int)PageBuffers);
				if (_pageSize.HasValue)
					startSpb.Append(IscCodes.isc_spb_res_page_size, (int)_pageSize);
				startSpb.Append(IscCodes.isc_spb_res_access_mode, (byte)(ReadOnly ? IscCodes.isc_spb_res_am_readonly : IscCodes.isc_spb_res_am_readwrite));
				if (!string.IsNullOrEmpty(SkipData))
					startSpb.Append2(IscCodes.isc_spb_res_skip_data, SkipData);
				startSpb.Append(IscCodes.isc_spb_options, (int)Options);
				if (ConnectionStringOptions.ParallelWorkers > 0)
					startSpb.Append(IscCodes.isc_spb_res_parallel_workers, ConnectionStringOptions.ParallelWorkers);
				StartTask(startSpb);
				ReadInput();
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
				startSpb.Append(IscCodes.isc_action_svc_restore);
				startSpb.Append2(IscCodes.isc_spb_bkp_file, "stdin");
				startSpb.Append2(IscCodes.isc_spb_dbname, ConnectionStringOptions.Database);
				if (Verbose)
				{
					startSpb.Append(IscCodes.isc_spb_verbose);
				}
				if (PageBuffers.HasValue)
					startSpb.Append(IscCodes.isc_spb_res_buffers, (int)PageBuffers);
				if (_pageSize.HasValue)
					startSpb.Append(IscCodes.isc_spb_res_page_size, (int)_pageSize);
				startSpb.Append(IscCodes.isc_spb_res_access_mode, (byte)(ReadOnly ? IscCodes.isc_spb_res_am_readonly : IscCodes.isc_spb_res_am_readwrite));
				if (!string.IsNullOrEmpty(SkipData))
					startSpb.Append2(IscCodes.isc_spb_res_skip_data, SkipData);
				startSpb.Append(IscCodes.isc_spb_options, (int)Options);
				if (ConnectionStringOptions.ParallelWorkers > 0)
					startSpb.Append(IscCodes.isc_spb_res_parallel_workers, ConnectionStringOptions.ParallelWorkers);
				await StartTaskAsync(startSpb, cancellationToken).ConfigureAwait(false);
				await ReadInputAsync(cancellationToken).ConfigureAwait(false);
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

	void ReadInput()
	{
		var items = new byte[] { IscCodes.isc_info_svc_stdin, IscCodes.isc_info_svc_line };
		var spb = new ServiceParameterBuffer2(Service.ParameterBufferEncoding);
		var response = Query(items, spb);
		var requestedLength = GetLength(response);
		while (true)
		{
			if (requestedLength > 0)
			{
				var data = new byte[requestedLength];
				var read = InputStream.Read(data, 0, requestedLength);
				if (read > 0)
				{
					Array.Resize(ref data, read);
					spb = new ServiceParameterBuffer2(Service.ParameterBufferEncoding);
					spb.Append2(IscCodes.isc_info_svc_line, data);
				}
			}
			response = Query(items, spb);
			if (response.Count == 1)
			{
				break;
			}
			if (response[1] is string message)
			{
				OnServiceOutput(message);
			}
			requestedLength = GetLength(response);
			spb = new ServiceParameterBuffer2(Service.ParameterBufferEncoding);
		}
	}
	async Task ReadInputAsync(CancellationToken cancellationToken = default)
	{
		var items = new byte[] { IscCodes.isc_info_svc_stdin, IscCodes.isc_info_svc_line };
		var spb = new ServiceParameterBuffer2(Service.ParameterBufferEncoding);
		var response = await QueryAsync(items, spb, cancellationToken).ConfigureAwait(false);
		var requestedLength = GetLength(response);
		while (true)
		{
			if (requestedLength > 0)
			{
				var data = new byte[requestedLength];
				var read = InputStream.Read(data, 0, requestedLength);
				if (read > 0)
				{
					Array.Resize(ref data, read);
					spb = new ServiceParameterBuffer2(Service.ParameterBufferEncoding);
					spb.Append2(IscCodes.isc_info_svc_line, data);
				}
			}
			response = await QueryAsync(items, spb, cancellationToken).ConfigureAwait(false);
			if (response.Count == 1)
			{
				break;
			}
			if (response[1] is string message)
			{
				OnServiceOutput(message);
			}
			requestedLength = GetLength(response);
			spb = new ServiceParameterBuffer2(Service.ParameterBufferEncoding);
		}
	}

	static int GetLength(IList<object> items)
	{
		// minus the size of isc code
		const int MaxLength = IscCodes.BUFFER_SIZE_32K - 4;
		return Math.Min((int)items[0], MaxLength);
	}
}
