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
 *	Copyright (c) 2014, 2016 Jiri Cincura (jiri@cincura.net)
 *	All Rights Reserved.
 */

using System;
using System.Collections;
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
				if (value.HasValue && !PageSizeHelper.IsValidPageSize((int)value))
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

		public void Execute()
		{
			try
			{
				StartSpb = new ServiceParameterBuffer();
				StartSpb.Append(IscCodes.isc_action_svc_restore);
				StartSpb.Append(IscCodes.isc_spb_bkp_file, "stdin");
				StartSpb.Append(IscCodes.isc_spb_dbname, Database);
				if (Verbose)
				{
					StartSpb.Append(IscCodes.isc_spb_verbose);
				}
				if (PageBuffers.HasValue)
					StartSpb.Append(IscCodes.isc_spb_res_buffers, (int)PageBuffers);
				if (_pageSize.HasValue)
					StartSpb.Append(IscCodes.isc_spb_res_page_size, (int)_pageSize);
				StartSpb.Append(IscCodes.isc_spb_res_access_mode, (byte)(ReadOnly ? IscCodes.isc_spb_res_am_readonly : IscCodes.isc_spb_res_am_readwrite));
				if (!string.IsNullOrEmpty(SkipData))
					StartSpb.Append(IscCodes.isc_spb_res_skip_data, SkipData);
				StartSpb.Append(IscCodes.isc_spb_options, (int)Options);

				Open();
				StartTask();
				ReadInput();
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

		void ReadInput()
		{
			var items = new byte[] { IscCodes.isc_info_svc_stdin, IscCodes.isc_info_svc_line };
			var response = Query(items);
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
						var spb = new ServiceParameterBuffer();
						spb.Append(IscCodes.isc_info_svc_line, data);
						QuerySpb = spb;
					}
				}
				response = Query(items);
				if (response.Count == 1)
				{
					break;
				}
				var message = response[1] as string;
				if (message != null)
				{
					OnServiceOutput(message);
				}
				requestedLength = GetLength(response);
				QuerySpb = null;
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
