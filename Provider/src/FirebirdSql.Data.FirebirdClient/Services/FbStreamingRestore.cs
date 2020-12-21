/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
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

namespace FirebirdSql.Data.Services
{
	public class FbStreamingRestore : FbService
	{
		private int? _pageSize;
		public int? PageSize
		{
			get { return _pageSize; }
			set
			{
				if (value is int v && !PageSizeHelper.IsValidPageSize(v))
					throw new InvalidOperationException("Invalid page size.");

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

		public void Execute() => ExecuteImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task ExecuteAsync(CancellationToken cancellationToken = default) => ExecuteImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task ExecuteImpl(AsyncWrappingCommonArgs async)
		{
			EnsureDatabase();

			try
			{
				await Open(async).ConfigureAwait(false);
				var startSpb = new ServiceParameterBuffer();
				startSpb.Append(IscCodes.isc_action_svc_restore);
				startSpb.Append(IscCodes.isc_spb_bkp_file, "stdin", SpbFilenameEncoding);
				startSpb.Append(IscCodes.isc_spb_dbname, Database, SpbFilenameEncoding);
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
					startSpb.Append(IscCodes.isc_spb_res_skip_data, SkipData);
				startSpb.Append(IscCodes.isc_spb_options, (int)Options);
				await StartTask(startSpb, async).ConfigureAwait(false);
				await ReadInput(async).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new FbException(ex.Message, ex);
			}
			finally
			{
				await Close(async).ConfigureAwait(false);
			}
		}

		async Task ReadInput(AsyncWrappingCommonArgs async)
		{
			var items = new byte[] { IscCodes.isc_info_svc_stdin, IscCodes.isc_info_svc_line };
			var spb = EmptySpb;
			var response = await Query(items, spb, async).ConfigureAwait(false);
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
						var dataSpb = new ServiceParameterBuffer();
						dataSpb.Append(IscCodes.isc_info_svc_line, data);
						spb = dataSpb;
					}
				}
				response = await Query(items, spb, async).ConfigureAwait(false);
				if (response.Count == 1)
				{
					break;
				}
				if (response[1] is string message)
				{
					OnServiceOutput(message);
				}
				requestedLength = GetLength(response);
				spb = EmptySpb;
			}
		}

		static int GetLength(IList<object> items)
		{
			// minus the size of isc code
			const int MaxLength = IscCodes.BUFFER_SIZE_32K - 4;
			return Math.Min((int)items[0], MaxLength);
		}
	}
}
