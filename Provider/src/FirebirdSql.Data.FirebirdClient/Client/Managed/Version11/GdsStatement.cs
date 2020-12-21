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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System.IO;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version11
{
	internal class GdsStatement : Version10.GdsStatement
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

		public override async Task Prepare(string commandText, AsyncWrappingCommonArgs async)
		{
			ClearAll();

			try
			{
				var numberOfResponses = 0;
				if (State == StatementState.Deallocated)
				{
					await SendAllocateToBuffer(async).ConfigureAwait(false);
					numberOfResponses++;
				}

				await SendPrepareToBuffer(commandText, async).ConfigureAwait(false);
				numberOfResponses++;

				await SendInfoSqlToBuffer(StatementTypeInfoItems, IscCodes.STATEMENT_TYPE_BUFFER_SIZE, async).ConfigureAwait(false);
				numberOfResponses++;

				await _database.Xdr.Flush(async).ConfigureAwait(false);

				try
				{
					GenericResponse allocateResponse = null;
					if (State == StatementState.Deallocated)
					{
						numberOfResponses--;
						allocateResponse = (GenericResponse)await _database.ReadResponse(async).ConfigureAwait(false);
					}

					numberOfResponses--;
					var prepareResponse = (GenericResponse)await _database.ReadResponse(async).ConfigureAwait(false);
					var deferredExecute = ((prepareResponse.ObjectHandle & IscCodes.STMT_DEFER_EXECUTE) == IscCodes.STMT_DEFER_EXECUTE);

					numberOfResponses--;
					var statementTypeResponse = (GenericResponse)await _database.ReadResponse(async).ConfigureAwait(false);

					if (allocateResponse != null)
					{
						await ProcessAllocateResponse(allocateResponse, async).ConfigureAwait(false);
					}
					await ProcessPrepareResponse(prepareResponse, async).ConfigureAwait(false);
					StatementType = ProcessStatementTypeInfoBuffer(await ProcessInfoSqlResponse(statementTypeResponse, async).ConfigureAwait(false));
				}
				finally
				{
					numberOfResponses = await SafeFinishFetching(numberOfResponses, async).ConfigureAwait(false);
				}

				State = StatementState.Prepared;
			}
			catch (IOException ex)
			{
				State = State == StatementState.Allocated ? StatementState.Error : State;
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		public override async Task Execute(AsyncWrappingCommonArgs async)
		{
			EnsureNotDeallocated();

			Clear();

			try
			{
				RecordsAffected = -1;

				await SendExecuteToBuffer(async).ConfigureAwait(false);

				var readRowsAffectedResponse = false;
				if (DoRecordsAffected)
				{
					await SendInfoSqlToBuffer(RowsAffectedInfoItems, IscCodes.ROWS_AFFECTED_BUFFER_SIZE, async).ConfigureAwait(false);

					readRowsAffectedResponse = true;
				}

				await _database.Xdr.Flush(async).ConfigureAwait(false);

				var numberOfResponses = (StatementType == DbStatementType.StoredProcedure ? 1 : 0) + 1 + (readRowsAffectedResponse ? 1 : 0);
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

					GenericResponse rowsAffectedResponse = null;
					if (readRowsAffectedResponse)
					{
						numberOfResponses--;
						rowsAffectedResponse = (GenericResponse)await _database.ReadResponse(async).ConfigureAwait(false);
					}

					await ProcessExecuteResponse(executeResponse, async).ConfigureAwait(false);
					if (readRowsAffectedResponse)
					{
						RecordsAffected = ProcessRecordsAffectedBuffer(await ProcessInfoSqlResponse(rowsAffectedResponse, async).ConfigureAwait(false));
					}
				}
				finally
				{
					numberOfResponses = await SafeFinishFetching(numberOfResponses, async).ConfigureAwait(false);
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

		#region Protected methods
		protected async Task<int> SafeFinishFetching(int numberOfResponses, AsyncWrappingCommonArgs async)
		{
			while (numberOfResponses > 0)
			{
				numberOfResponses--;
				try
				{
					await _database.ReadResponse(async).ConfigureAwait(false);
				}
				catch (IscException)
				{ }
			}
			return numberOfResponses;
		}

		protected override async Task Free(int option, AsyncWrappingCommonArgs async)
		{
			if (FreeNotNeeded(option))
				return;

			await DoFreePacket(option, async).ConfigureAwait(false);
			(Database as GdsDatabase).DeferredPackets.Enqueue(ProcessFreeResponse);
		}
		#endregion
	}
}
