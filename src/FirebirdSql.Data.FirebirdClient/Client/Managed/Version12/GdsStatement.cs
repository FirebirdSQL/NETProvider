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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version12;

internal class GdsStatement : Version11.GdsStatement
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

	public override void Execute(int timeout, IDescriptorFiller descriptorFiller)
	{
		EnsureNotDeallocated();

		Clear();

		try
		{
			RecordsAffected = -1;

			SendExecuteToBuffer(timeout, descriptorFiller);

			_database.Xdr.Flush();

			var numberOfResponses = (StatementType == DbStatementType.StoredProcedure ? 1 : 0) + 1;
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
				ProcessExecuteResponse(executeResponse);
			}
			finally
			{
				(Database as GdsDatabase).SafeFinishFetching(numberOfResponses);
			}

			// we need to split this in two, to allow server handle op_cancel properly

			if (DoRecordsAffected)
			{
				SendInfoSqlToBuffer(RowsAffectedInfoItems, IscCodes.ROWS_AFFECTED_BUFFER_SIZE);

				_database.Xdr.Flush();

				numberOfResponses = 1;
				try
				{
					numberOfResponses--;
					var rowsAffectedResponse = (GenericResponse)_database.ReadResponse();
					RecordsAffected = ProcessRecordsAffectedBuffer(ProcessInfoSqlResponse(rowsAffectedResponse));
				}
				finally
				{
					(Database as GdsDatabase).SafeFinishFetching(numberOfResponses);
				}
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

			await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			var numberOfResponses = (StatementType == DbStatementType.StoredProcedure ? 1 : 0) + 1;
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
				await ProcessExecuteResponseAsync(executeResponse, cancellationToken).ConfigureAwait(false);
			}
			finally
			{
				await (Database as GdsDatabase).SafeFinishFetchingAsync(numberOfResponses, cancellationToken).ConfigureAwait(false);
			}

			// we need to split this in two, to allow server handle op_cancel properly

			if (DoRecordsAffected)
			{
				await SendInfoSqlToBufferAsync(RowsAffectedInfoItems, IscCodes.ROWS_AFFECTED_BUFFER_SIZE, cancellationToken).ConfigureAwait(false);

				await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

				numberOfResponses = 1;
				try
				{
					numberOfResponses--;
					var rowsAffectedResponse = (GenericResponse)await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false);
					RecordsAffected = ProcessRecordsAffectedBuffer(await ProcessInfoSqlResponseAsync(rowsAffectedResponse, cancellationToken).ConfigureAwait(false));
				}
				finally
				{
					await (Database as GdsDatabase).SafeFinishFetchingAsync(numberOfResponses, cancellationToken).ConfigureAwait(false);
				}
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
}
