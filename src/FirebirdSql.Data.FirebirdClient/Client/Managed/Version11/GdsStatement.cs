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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Client.Managed.Version10;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version11;

internal class GdsStatement : Version10.GdsStatement
{
	#region Constructors

	public GdsStatement(GdsDatabase database)
		: base(database)
	{ }

	public GdsStatement(GdsDatabase database, Version10.GdsTransaction transaction)
		: base(database, transaction)
	{ }

	#endregion

	#region Overriden Methods

	public override void Prepare(string commandText)
	{
		ClearAll();

		try
		{
			var numberOfResponses = 0;
			if (State == StatementState.Deallocated)
			{
				SendAllocateToBuffer();
				numberOfResponses++;
			}

			SendPrepareToBuffer(commandText);
			numberOfResponses++;

			SendInfoSqlToBuffer(StatementTypeInfoItems, IscCodes.STATEMENT_TYPE_BUFFER_SIZE);
			numberOfResponses++;

			_database.Xdr.Flush();

			try
			{
				GenericResponse allocateResponse = null;
				if (State == StatementState.Deallocated)
				{
					numberOfResponses--;
					allocateResponse = (GenericResponse)_database.ReadResponse();
				}

				numberOfResponses--;
				var prepareResponse = (GenericResponse)_database.ReadResponse();
				var deferredExecute = ((prepareResponse.ObjectHandle & IscCodes.STMT_DEFER_EXECUTE) == IscCodes.STMT_DEFER_EXECUTE);

				numberOfResponses--;
				var statementTypeResponse = (GenericResponse)_database.ReadResponse();

				if (allocateResponse != null)
				{
					ProcessAllocateResponse(allocateResponse);
				}
				ProcessPrepareResponse(prepareResponse);
				StatementType = ProcessStatementTypeInfoBuffer(ProcessInfoSqlResponse(statementTypeResponse));
			}
			finally
			{
				(Database as GdsDatabase).SafeFinishFetching(numberOfResponses);
			}

			State = StatementState.Prepared;
		}
		catch (IOException ex)
		{
			State = State == StatementState.Allocated ? StatementState.Error : State;
			throw IscException.ForIOException(ex);
		}
	}
	public override async ValueTask PrepareAsync(string commandText, CancellationToken cancellationToken = default)
	{
		ClearAll();

		try
		{
			var numberOfResponses = 0;
			if (State == StatementState.Deallocated)
			{
				await SendAllocateToBufferAsync(cancellationToken).ConfigureAwait(false);
				numberOfResponses++;
			}

			await SendPrepareToBufferAsync(commandText, cancellationToken).ConfigureAwait(false);
			numberOfResponses++;

			await SendInfoSqlToBufferAsync(StatementTypeInfoItems, IscCodes.STATEMENT_TYPE_BUFFER_SIZE, cancellationToken).ConfigureAwait(false);
			numberOfResponses++;

			await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			try
			{
				GenericResponse allocateResponse = null;
				if (State == StatementState.Deallocated)
				{
					numberOfResponses--;
					allocateResponse = (GenericResponse)await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false);
				}

				numberOfResponses--;
				var prepareResponse = (GenericResponse)await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false);
				var deferredExecute = ((prepareResponse.ObjectHandle & IscCodes.STMT_DEFER_EXECUTE) == IscCodes.STMT_DEFER_EXECUTE);

				numberOfResponses--;
				var statementTypeResponse = (GenericResponse)await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false);

				if (allocateResponse != null)
				{
					await ProcessAllocateResponseAsync(allocateResponse, cancellationToken).ConfigureAwait(false);
				}
				await ProcessPrepareResponseAsync(prepareResponse, cancellationToken).ConfigureAwait(false);
				StatementType = ProcessStatementTypeInfoBuffer(await ProcessInfoSqlResponseAsync(statementTypeResponse, cancellationToken).ConfigureAwait(false));
			}
			finally
			{
				await (Database as GdsDatabase).SafeFinishFetchingAsync(numberOfResponses, cancellationToken).ConfigureAwait(false);
			}

			State = StatementState.Prepared;
		}
		catch (IOException ex)
		{
			State = State == StatementState.Allocated ? StatementState.Error : State;
			throw IscException.ForIOException(ex);
		}
	}

	public override void Execute(int timeout, IDescriptorFiller descriptorFiller)
	{
		EnsureNotDeallocated();

		Clear();

		try
		{
			RecordsAffected = -1;

			SendExecuteToBuffer(timeout, descriptorFiller);

			var readRowsAffectedResponse = false;
			if (DoRecordsAffected)
			{
				SendInfoSqlToBuffer(RowsAffectedInfoItems, IscCodes.ROWS_AFFECTED_BUFFER_SIZE);

				readRowsAffectedResponse = true;
			}

			_database.Xdr.Flush();

			var numberOfResponses = (StatementType == DbStatementType.StoredProcedure ? 1 : 0) + 1 + (readRowsAffectedResponse ? 1 : 0);
			try
			{
				SqlResponse sqlStoredProcedureResponse = null;
				if (StatementType == DbStatementType.StoredProcedure)
				{
					numberOfResponses--;
					sqlStoredProcedureResponse = (SqlResponse)_database.ReadResponse();
					ProcessStoredProcedureExecuteResponse(sqlStoredProcedureResponse);
				}

				numberOfResponses--;
				var executeResponse = (GenericResponse)_database.ReadResponse();

				GenericResponse rowsAffectedResponse = null;
				if (readRowsAffectedResponse)
				{
					numberOfResponses--;
					rowsAffectedResponse = (GenericResponse)_database.ReadResponse();
				}

				ProcessExecuteResponse(executeResponse);
				if (readRowsAffectedResponse)
				{
					RecordsAffected = ProcessRecordsAffectedBuffer(ProcessInfoSqlResponse(rowsAffectedResponse));
				}
			}
			finally
			{
				(Database as GdsDatabase).SafeFinishFetching(numberOfResponses);
			}

			State = StatementState.Executed;
		}
		catch (IOException ex)
		{
			State = StatementState.Error;
			throw IscException.ForIOException(ex);
		}
	}
	public override async ValueTask ExecuteAsync(int timeout, IDescriptorFiller descriptorFiller, CancellationToken cancellationToken = default)
	{
		EnsureNotDeallocated();

		Clear();

		try
		{
			RecordsAffected = -1;

			await SendExecuteToBufferAsync(timeout, descriptorFiller, cancellationToken).ConfigureAwait(false);

			var readRowsAffectedResponse = false;
			if (DoRecordsAffected)
			{
				await SendInfoSqlToBufferAsync(RowsAffectedInfoItems, IscCodes.ROWS_AFFECTED_BUFFER_SIZE, cancellationToken).ConfigureAwait(false);

				readRowsAffectedResponse = true;
			}

			await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			var numberOfResponses = (StatementType == DbStatementType.StoredProcedure ? 1 : 0) + 1 + (readRowsAffectedResponse ? 1 : 0);
			try
			{
				SqlResponse sqlStoredProcedureResponse = null;
				if (StatementType == DbStatementType.StoredProcedure)
				{
					numberOfResponses--;
					sqlStoredProcedureResponse = (SqlResponse)await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false);
					await ProcessStoredProcedureExecuteResponseAsync(sqlStoredProcedureResponse, cancellationToken).ConfigureAwait(false);
				}

				numberOfResponses--;
				var executeResponse = (GenericResponse)await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false);

				GenericResponse rowsAffectedResponse = null;
				if (readRowsAffectedResponse)
				{
					numberOfResponses--;
					rowsAffectedResponse = (GenericResponse)await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false);
				}

				await ProcessExecuteResponseAsync(executeResponse, cancellationToken).ConfigureAwait(false);
				if (readRowsAffectedResponse)
				{
					RecordsAffected = ProcessRecordsAffectedBuffer(await ProcessInfoSqlResponseAsync(rowsAffectedResponse, cancellationToken).ConfigureAwait(false));
				}
			}
			finally
			{
				await (Database as GdsDatabase).SafeFinishFetchingAsync(numberOfResponses, cancellationToken).ConfigureAwait(false);
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
	protected override void Free(int option)
	{
		if (FreeNotNeeded(option))
			return;

		DoFreePacket(option);
		(Database as GdsDatabase).AppendDeferredPacket(ProcessFreeResponse);
	}
	protected override async ValueTask FreeAsync(int option, CancellationToken cancellationToken = default)
	{
		if (FreeNotNeeded(option))
			return;

		await DoFreePacketAsync(option, cancellationToken).ConfigureAwait(false);
		(Database as GdsDatabase).AppendDeferredPacket(ProcessFreeResponseAsync);
	}
	#endregion
}
