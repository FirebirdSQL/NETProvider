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
 *	Copyright (c) 2014 Jiri Cincura (jiri@cincura.net)
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
		private int? pageSize;
		public int? PageSize
		{
			get { return pageSize; }
			set
			{
				if (value.HasValue && !PageSizeHelper.IsValidPageSize((int)value))
					throw new InvalidOperationException("Invalid page size.");

				pageSize = value;
			}
		}

		public Stream InputStream { get; set; }
		public bool Verbose { get; set; }
		public int? PageBuffers { get; set; }
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
				if (pageSize.HasValue)
					StartSpb.Append(IscCodes.isc_spb_res_page_size, (int)pageSize);
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
			var items = Verbose
				? new byte[] { IscCodes.isc_info_svc_stdin, IscCodes.isc_info_svc_line }
				: new byte[] { IscCodes.isc_info_svc_stdin };
			var response = Query(items);
			var length = GetLength(response);
			while (true)
			{
				if (length > 0)
				{
					var buffer = new byte[length];
					var read = InputStream.Read(buffer, 0, length);
					if (read != 0)
					{
						Array.Resize(ref buffer, read);
						var spb = new ServiceParameterBuffer();
						spb.Append(IscCodes.isc_info_svc_line, buffer);
						QuerySpb = spb;
					}
				}
				response = Query(items);
				QuerySpb = null;
				length = GetLength(response);
				if (length == 0 && !ProcessMessages(response))
				{
					break;
				}
			}
		}

		bool ProcessMessages(ArrayList items)
		{
			var message = GetMessage(items);
			if (message == null)
				return false;
			WriteServiceOutputChecked(message);
			return true;
		}

		int GetLength(ArrayList items)
		{
			const int maxLength = (32 * 1024) - 4;
			return Math.Min(items[0] is int ? (int)items[0] : 0, maxLength);
		}

		string GetMessage(ArrayList items)
		{
			if (items[0] is string)
				return (string)items[0];
			if (items.Count > 1)
				return (string)items[1];
			return null;
		}
	}
}