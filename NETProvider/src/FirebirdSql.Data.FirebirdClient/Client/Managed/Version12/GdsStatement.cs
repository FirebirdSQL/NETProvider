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
 *	Copyright (c) 2010 Jiri Cincura (jiri@cincura.net)
 *	All Rights Reserved.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version12
{
	internal class GdsStatement : Version11.GdsStatement
	{
		#region Constructors

		public GdsStatement(IDatabase db)
			: base(db)
		{ }

		public GdsStatement(IDatabase db, ITransaction transaction)
			: base(db, transaction)
		{ }

		#endregion

		#region Overriden Methods

		public override void Execute()
		{
			if (this.state == StatementState.Deallocated)
			{
				throw new InvalidOperationException("Statement is not correctly created.");
			}

			// Clear data
			this.Clear();

			lock (this.database.SyncObject)
			{
				try
				{
					this.RecordsAffected = -1;

					this.SendExecuteToBuffer();

					this.database.Flush();

					// sql?, execute
					int numberOfResponses =
						(this.StatementType == DbStatementType.StoredProcedure ? 1 : 0) + 1;
					try
					{
						SqlResponse sqlStoredProcedureResponse = null;
						if (this.StatementType == DbStatementType.StoredProcedure)
						{
							numberOfResponses--;
							sqlStoredProcedureResponse = this.database.ReadSqlResponse();
							this.ProcessStoredProcedureExecuteResponse(sqlStoredProcedureResponse);
						}

						numberOfResponses--;
						GenericResponse executeResponse = this.database.ReadGenericResponse();
						this.ProcessExecuteResponse(executeResponse);
					}
					finally
					{
						SafeFinishFetching(ref numberOfResponses);
					}

					//we need to split this in two, to alloow server handle op_cancel properly

					// Obtain records affected by query execution
					if (this.ReturnRecordsAffected &&
						(this.StatementType == DbStatementType.Insert ||
						this.StatementType == DbStatementType.Delete ||
						this.StatementType == DbStatementType.Update ||
						this.StatementType == DbStatementType.StoredProcedure ||
						this.StatementType == DbStatementType.Select))
					{
						// Grab rows affected
						this.SendInfoSqlToBuffer(RowsAffectedInfoItems, IscCodes.ROWS_AFFECTED_BUFFER_SIZE);
						
						this.database.Flush();

						//rows affected
						numberOfResponses = 1;
						try
						{
							numberOfResponses--;
							GenericResponse rowsAffectedResponse = this.database.ReadGenericResponse();
							this.RecordsAffected = this.ProcessRecordsAffectedBuffer(this.ProcessInfoSqlResponse(rowsAffectedResponse));
						}
						finally
						{
							SafeFinishFetching(ref numberOfResponses);
						}
					}

					this.state = StatementState.Executed;
				}
				catch (IOException)
				{
					this.state = StatementState.Error;
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		#endregion
	}
}
