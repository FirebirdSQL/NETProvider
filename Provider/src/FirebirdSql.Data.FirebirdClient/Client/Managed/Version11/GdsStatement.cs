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

		public GdsStatement(DatabaseBase db)
			: base(db)
		{ }

		public GdsStatement(DatabaseBase db, TransactionBase transaction)
			: base(db, transaction)
		{ }

		#endregion

		#region Overriden Methods

		public override async ValueTask PrepareAsync(string commandText, AsyncWrappingCommonArgs async)
		{
			ClearAll();

			try
			{
				var numberOfResponses = 0;
				if (State == StatementState.Deallocated)
				{
					await SendAllocateToBufferAsync(async).ConfigureAwait(false);
					numberOfResponses++;
				}

				await SendPrepareToBufferAsync(commandText, async).ConfigureAwait(false);
				numberOfResponses++;

				await SendInfoSqlToBufferAsync(StatementTypeInfoItems, IscCodes.STATEMENT_TYPE_BUFFER_SIZE, async).ConfigureAwait(false);
				numberOfResponses++;

				await _database.Xdr.FlushAsync(async).ConfigureAwait(false);

				try
				{
					GenericResponse allocateResponse = null;
					if (State == StatementState.Deallocated)
					{
						numberOfResponses--;
						allocateResponse = (GenericResponse)await _database.ReadResponseAsync(async).ConfigureAwait(false);
					}

					numberOfResponses--;
					var prepareResponse = (GenericResponse)await _database.ReadResponseAsync(async).ConfigureAwait(false);
					var deferredExecute = ((prepareResponse.ObjectHandle & IscCodes.STMT_DEFER_EXECUTE) == IscCodes.STMT_DEFER_EXECUTE);

					numberOfResponses--;
					var statementTypeResponse = (GenericResponse)await _database.ReadResponseAsync(async).ConfigureAwait(false);

					if (allocateResponse != null)
					{
						await ProcessAllocateResponseAsync(allocateResponse, async).ConfigureAwait(false);
					}
					await ProcessPrepareResponseAsync(prepareResponse, async).ConfigureAwait(false);
					StatementType = ProcessStatementTypeInfoBuffer(await ProcessInfoSqlResponseAsync(statementTypeResponse, async).ConfigureAwait(false));
				}
				finally
				{
					numberOfResponses = await SafeFinishFetchingAsync(numberOfResponses, async).ConfigureAwait(false);
				}

				State = StatementState.Prepared;
			}
			catch (IOException ex)
			{
				State = State == StatementState.Allocated ? StatementState.Error : State;
				throw IscException.ForIOException(ex);
			}
		}

		public override async ValueTask ExecuteAsync(AsyncWrappingCommonArgs async)
		{
			EnsureNotDeallocated();

			Clear();

			try
			{
				RecordsAffected = -1;

				await SendExecuteToBufferAsync(async).ConfigureAwait(false);

				var readRowsAffectedResponse = false;
				if (DoRecordsAffected)
				{
					await SendInfoSqlToBufferAsync(RowsAffectedInfoItems, IscCodes.ROWS_AFFECTED_BUFFER_SIZE, async).ConfigureAwait(false);

					readRowsAffectedResponse = true;
				}

				await _database.Xdr.FlushAsync(async).ConfigureAwait(false);

				var numberOfResponses = (StatementType == DbStatementType.StoredProcedure ? 1 : 0) + 1 + (readRowsAffectedResponse ? 1 : 0);
				try
				{
					SqlResponse sqlStoredProcedureResponse = null;
					if (StatementType == DbStatementType.StoredProcedure)
					{
						numberOfResponses--;
						sqlStoredProcedureResponse = (SqlResponse)await _database.ReadResponseAsync(async).ConfigureAwait(false);
						await ProcessStoredProcedureExecuteResponseAsync(sqlStoredProcedureResponse, async).ConfigureAwait(false);
					}

					numberOfResponses--;
					var executeResponse = (GenericResponse)await _database.ReadResponseAsync(async).ConfigureAwait(false);

					GenericResponse rowsAffectedResponse = null;
					if (readRowsAffectedResponse)
					{
						numberOfResponses--;
						rowsAffectedResponse = (GenericResponse)await _database.ReadResponseAsync(async).ConfigureAwait(false);
					}

					await ProcessExecuteResponseAsync(executeResponse, async).ConfigureAwait(false);
					if (readRowsAffectedResponse)
					{
						RecordsAffected = ProcessRecordsAffectedBuffer(await ProcessInfoSqlResponseAsync(rowsAffectedResponse, async).ConfigureAwait(false));
					}
				}
				finally
				{
					numberOfResponses = await SafeFinishFetchingAsync(numberOfResponses, async).ConfigureAwait(false);
				}

				State = StatementState.Executed;
			}
			catch (IOException ex)
			{
				State = StatementState.Error;
				throw IscException.ForIOException(ex);
			}
		}

		#endregion

		#region Protected methods
		protected async ValueTask<int> SafeFinishFetchingAsync(int numberOfResponses, AsyncWrappingCommonArgs async)
		{
			while (numberOfResponses > 0)
			{
				numberOfResponses--;
				try
				{
					await _database.ReadResponseAsync(async).ConfigureAwait(false);
				}
				catch (IscException)
				{ }
			}
			return numberOfResponses;
		}

		protected override async ValueTask FreeAsync(int option, AsyncWrappingCommonArgs async)
		{
			if (FreeNotNeeded(option))
				return;

			await DoFreePacketAsync(option, async).ConfigureAwait(false);
			(Database as GdsDatabase).DeferredPackets.Enqueue(ProcessFreeResponseAsync);
		}
		#endregion
	}
}
