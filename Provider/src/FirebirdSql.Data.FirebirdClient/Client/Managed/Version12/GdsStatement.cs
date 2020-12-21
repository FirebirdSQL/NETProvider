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
using System.IO;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version12
{
	internal class GdsStatement : Version11.GdsStatement
	{
		#region Constructors

		public GdsStatement(IDatabase db)
			: base(db)
		{ }

		public GdsStatement(IDatabase db, TransactionBase transaction)
			: base(db, transaction)
		{ }

		#endregion

		#region Overriden Methods

		public override async Task Execute(AsyncWrappingCommonArgs async)
		{
			EnsureNotDeallocated();

			Clear();

			try
			{
				RecordsAffected = -1;

				await SendExecuteToBuffer(async).ConfigureAwait(false);

				await _database.Xdr.Flush(async).ConfigureAwait(false);

				var numberOfResponses = (StatementType == DbStatementType.StoredProcedure ? 1 : 0) + 1;
				try
				{
					SqlResponse sqlStoredProcedureResponse = null;
					if (StatementType == DbStatementType.StoredProcedure)
					{
						numberOfResponses--;
						sqlStoredProcedureResponse = (SqlResponse)await _database.ReadResponse(async).ConfigureAwait(false);
						await ProcessStoredProcedureExecuteResponse(sqlStoredProcedureResponse, async).ConfigureAwait(false);
					}

					numberOfResponses--;
					var executeResponse = (GenericResponse)await _database.ReadResponse(async).ConfigureAwait(false);
					await ProcessExecuteResponse(executeResponse, async).ConfigureAwait(false);
				}
				finally
				{
					numberOfResponses = await SafeFinishFetching(numberOfResponses, async).ConfigureAwait(false);
				}

				// we need to split this in two, to allow server handle op_cancel properly

				if (DoRecordsAffected)
				{
					await SendInfoSqlToBuffer(RowsAffectedInfoItems, IscCodes.ROWS_AFFECTED_BUFFER_SIZE, async).ConfigureAwait(false);

					await _database.Xdr.Flush(async).ConfigureAwait(false);

					numberOfResponses = 1;
					try
					{
						numberOfResponses--;
						var rowsAffectedResponse = (GenericResponse)await _database.ReadResponse(async).ConfigureAwait(false);
						RecordsAffected = ProcessRecordsAffectedBuffer(await ProcessInfoSqlResponse(rowsAffectedResponse, async).ConfigureAwait(false));
					}
					finally
					{
						numberOfResponses = await SafeFinishFetching(numberOfResponses, async).ConfigureAwait(false);
					}
				}

				State = StatementState.Executed;
			}
			catch (IOException ex)
			{
				State = StatementState.Error;
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		#endregion
	}
}
