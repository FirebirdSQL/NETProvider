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
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Services
{
	public sealed class FbValidation2 : FbService
	{

		public string TablesInclude { get; set; }
		public string TablesExclude { get; set; }
		public string IndicesInclude { get; set; }
		public string IndicesExclude { get; set; }
		public int? LockTimeout { get; set; }

		public FbValidation2(string connectionString = null)
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
				startSpb.Append(IscCodes.isc_action_svc_validate);
				startSpb.Append(IscCodes.isc_spb_dbname, Database, SpbFilenameEncoding);
				if (!string.IsNullOrEmpty(TablesInclude))
					startSpb.Append(IscCodes.isc_spb_val_tab_incl, TablesInclude);
				if (!string.IsNullOrEmpty(TablesExclude))
					startSpb.Append(IscCodes.isc_spb_val_tab_excl, TablesExclude);
				if (!string.IsNullOrEmpty(IndicesInclude))
					startSpb.Append(IscCodes.isc_spb_val_idx_incl, IndicesInclude);
				if (!string.IsNullOrEmpty(IndicesExclude))
					startSpb.Append(IscCodes.isc_spb_val_idx_excl, IndicesExclude);
				if (LockTimeout.HasValue)
					startSpb.Append(IscCodes.isc_spb_val_lock_timeout, (int)LockTimeout);
				await StartTask(startSpb, async).ConfigureAwait(false);
				await ProcessServiceOutput(EmptySpb, async).ConfigureAwait(false);
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
	}
}
