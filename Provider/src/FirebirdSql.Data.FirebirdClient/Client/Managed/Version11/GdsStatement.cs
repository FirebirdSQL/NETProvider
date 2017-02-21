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
 *	Copyright (c) 2002 - 2007 Carlos Guzman Alvarez
 *	Copyright (c) 2007 - 2008 Jiri Cincura (jiri@cincura.net)
 *	All Rights Reserved.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

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

		public override void Prepare(string commandText)
		{
			ClearAll();

			try
			{
				int numberOfResponses = 0;
				if (_state == StatementState.Deallocated)
				{
					SendAllocateToBuffer();
					numberOfResponses++;
				}

				SendPrepareToBuffer(commandText);
				numberOfResponses++;

				SendInfoSqlToBuffer(StatementTypeInfoItems, IscCodes.STATEMENT_TYPE_BUFFER_SIZE);
				numberOfResponses++;

				_database.XdrStream.Flush();

				try
				{
					GenericResponse allocateResponse = null;
					if (_state == StatementState.Deallocated)
					{
						numberOfResponses--;
						allocateResponse = _database.ReadGenericResponse();
					}

					numberOfResponses--;
					GenericResponse prepareResponse = _database.ReadGenericResponse();
					bool deferredExecute = ((prepareResponse.ObjectHandle & IscCodes.STMT_DEFER_EXECUTE) == IscCodes.STMT_DEFER_EXECUTE);

					numberOfResponses--;
					GenericResponse statementTypeResponse = _database.ReadGenericResponse();

					if (allocateResponse != null)
					{
						ProcessAllocateResponce(allocateResponse);
					}
					ProcessPrepareResponse(prepareResponse);
					_statementType = ProcessStatementTypeInfoBuffer(ProcessInfoSqlResponse(statementTypeResponse));
				}
				finally
				{
					SafeFinishFetching(ref numberOfResponses);
				}

				_state = StatementState.Prepared;
			}
			catch (IOException ex)
			{
				if (_state == StatementState.Allocated)
					_state = StatementState.Error;
				throw IscException.ForErrorCode(IscCodes.isc_net_read_err, ex);
			}
		}

		public override void Execute()
		{
			if (_state == StatementState.Deallocated)
			{
				throw new InvalidOperationException("Statement is not correctly created.");
			}

			Clear();

			try
			{
				RecordsAffected = -1;

				SendExecuteToBuffer();

				bool readRowsAffectedResponse = false;
				if (ReturnRecordsAffected &&
					(StatementType == DbStatementType.Insert ||
					StatementType == DbStatementType.Delete ||
					StatementType == DbStatementType.Update ||
					StatementType == DbStatementType.StoredProcedure ||
					StatementType == DbStatementType.Select))
				{
					SendInfoSqlToBuffer(RowsAffectedInfoItems, IscCodes.ROWS_AFFECTED_BUFFER_SIZE);

					readRowsAffectedResponse = true;
				}

				_database.XdrStream.Flush();

				int numberOfResponses =
					(StatementType == DbStatementType.StoredProcedure ? 1 : 0) + 1 + (readRowsAffectedResponse ? 1 : 0);
				try
				{
					SqlResponse sqlStoredProcedureResponse = null;
					if (StatementType == DbStatementType.StoredProcedure)
					{
						numberOfResponses--;
						sqlStoredProcedureResponse = _database.ReadSqlResponse();
						ProcessStoredProcedureExecuteResponse(sqlStoredProcedureResponse);
					}

					numberOfResponses--;
					GenericResponse executeResponse = _database.ReadGenericResponse();

					GenericResponse rowsAffectedResponse = null;
					if (readRowsAffectedResponse)
					{
						numberOfResponses--;
						rowsAffectedResponse = _database.ReadGenericResponse();
					}

					ProcessExecuteResponse(executeResponse);
					if (readRowsAffectedResponse)
						RecordsAffected = ProcessRecordsAffectedBuffer(ProcessInfoSqlResponse(rowsAffectedResponse));
				}
				finally
				{
					SafeFinishFetching(ref numberOfResponses);
				}

				_state = StatementState.Executed;
			}
			catch (IOException ex)
			{
				_state = StatementState.Error;
				throw IscException.ForErrorCode(IscCodes.isc_net_read_err, ex);
			}
		}

		#endregion

		#region Protected methods
		protected void SafeFinishFetching(ref int numberOfResponses)
		{
			while (numberOfResponses > 0)
			{
				numberOfResponses--;
				try
				{
					_database.ReadResponse();
				}
				catch (IscException)
				{ }
			}
		}

		protected override void Free(int option)
		{
			if (FreeNotNeeded(option))
				return;

			DoFreePacket(option);
			(Database as GdsDatabase).DeferredPackets.Enqueue(ProcessFreeResponse);
		}
		#endregion
	}
}
