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
 *	Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *	All Rights Reserved.
 *
 *  Contributors:
 *    Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Text;
using System.Data;
using FirebirdSql.Data.Client.Managed;

namespace FirebirdSql.Data.Common
{
	internal abstract class StatementBase : IDisposable
	{
		#region Protected Static Fields

		// Plan	information	items
		protected static readonly byte[] DescribePlanInfoItems = new byte[]
		{
			IscCodes.isc_info_sql_get_plan
		};

		// Records affected	items
		protected static readonly byte[] RowsAffectedInfoItems = new byte[]
		{
			IscCodes.isc_info_sql_records
		};

		// Describe	information	items
		protected static readonly byte[] DescribeInfoAndBindInfoItems = new byte[]
		{
			IscCodes.isc_info_sql_select,
			IscCodes.isc_info_sql_describe_vars,
			IscCodes.isc_info_sql_sqlda_seq,
			IscCodes.isc_info_sql_type,
			IscCodes.isc_info_sql_sub_type,
			IscCodes.isc_info_sql_length,
			IscCodes.isc_info_sql_scale,
			IscCodes.isc_info_sql_field,
			IscCodes.isc_info_sql_relation,
			// IscCodes.isc_info_sql_owner,
			IscCodes.isc_info_sql_alias,
			IscCodes.isc_info_sql_describe_end,

			IscCodes.isc_info_sql_bind,
			IscCodes.isc_info_sql_describe_vars,
			IscCodes.isc_info_sql_sqlda_seq,
			IscCodes.isc_info_sql_type,
			IscCodes.isc_info_sql_sub_type,
			IscCodes.isc_info_sql_length,
			IscCodes.isc_info_sql_scale,
			IscCodes.isc_info_sql_field,
			IscCodes.isc_info_sql_relation,
			// IscCodes.isc_info_sql_owner,
			IscCodes.isc_info_sql_alias,
			IscCodes.isc_info_sql_describe_end
		};

		protected static readonly byte[] StatementTypeInfoItems = new byte[]
		{
			IscCodes.isc_info_sql_stmt_type
		};

		#endregion

		#region Protected Fields

		protected EventHandler TransactionUpdate;

		#endregion

		#region Abstract Properties

		public abstract IDatabase Database { get; }
		public abstract TransactionBase Transaction { get; set; }
		public abstract Descriptor Parameters { get; set; }
		public abstract Descriptor Fields { get; }
		public abstract int RecordsAffected { get; protected set; }
		public abstract bool IsPrepared { get; }
		public abstract DbStatementType StatementType { get; protected set; }
		public abstract StatementState State { get; protected set; }
		public abstract int FetchSize { get; set; }
		public abstract bool ReturnRecordsAffected { get; set; }

		#endregion

		#region IDisposable methods

		public virtual void Dispose()
		{ }

		#endregion

		#region Methods

		public string GetExecutionPlan()
		{
			int count = 0;
			int bufferSize = IscCodes.DEFAULT_MAX_BUFFER_SIZE;
			byte[] buffer = GetSqlInfo(DescribePlanInfoItems, bufferSize);

			if (buffer[0] == IscCodes.isc_info_end)
			{
				return string.Empty;
			}

			while (buffer[0] == IscCodes.isc_info_truncated && count < 4)
			{
				bufferSize *= 2;
				buffer = GetSqlInfo(DescribePlanInfoItems, bufferSize);
				count++;
			}
			if (count > 3)
			{
				return null;
			}

			int len = buffer[1];
			len += buffer[2] << 8;
			if (len > 0)
			{
				return Database.Charset.GetString(buffer, 4, --len);
			}
			else
			{
				return string.Empty;
			}
		}

		public virtual void Close()
		{
			if (State == StatementState.Executed ||
				State == StatementState.Error)
			{
				if (StatementType == DbStatementType.Select ||
					StatementType == DbStatementType.SelectForUpdate ||
					StatementType == DbStatementType.StoredProcedure)
				{
					if (State == StatementState.Allocated ||
						State == StatementState.Prepared ||
						State == StatementState.Executed)
					{
						try
						{
							Free(IscCodes.DSQL_close);
						}
						catch
						{ }
					}
					ClearArrayHandles();
					State = StatementState.Closed;
				}
			}
		}

		public virtual void Release()
		{
			if (Transaction != null && TransactionUpdate != null)
			{
				Transaction.Update -= TransactionUpdate;
				TransactionUpdate = null;
			}

			Free(IscCodes.DSQL_drop);

			ClearArrayHandles();
			State = StatementState.Deallocated;
			StatementType = DbStatementType.None;
		}

		#endregion

		#region Abstract Methods

		public abstract void Describe();
		public abstract void DescribeParameters();
		public abstract void Prepare(string commandText);
		public abstract void Execute();
		public abstract DbValue[] Fetch();
		public abstract DbValue[] GetOutputParameters();

		public abstract BlobBase CreateBlob();
		public abstract BlobBase CreateBlob(long handle);

		public abstract ArrayBase CreateArray(ArrayDesc descriptor);
		public abstract ArrayBase CreateArray(string tableName, string fieldName);
		public abstract ArrayBase CreateArray(long handle, string tableName, string fieldName);

		#endregion

		#region Protected Abstract Methods

		protected abstract void TransactionUpdated(object sender, EventArgs e);
		protected abstract byte[] GetSqlInfo(byte[] items, int bufferLength);
		protected abstract void Free(int option);

		#endregion

		#region Protected Methods

		protected byte[] GetSqlInfo(byte[] items)
		{
			return GetSqlInfo(items, IscCodes.DEFAULT_MAX_BUFFER_SIZE);
		}

		protected int GetRecordsAffected()
		{
			byte[] buffer = GetSqlInfo(RowsAffectedInfoItems, IscCodes.ROWS_AFFECTED_BUFFER_SIZE);

			return ProcessRecordsAffectedBuffer(buffer);
		}

		protected int ProcessRecordsAffectedBuffer(byte[] buffer)
		{
			int insertCount = 0;
			int updateCount = 0;
			int deleteCount = 0;
			int selectCount = 0;
			int pos = 0;
			int length = 0;
			int type = 0;

			while ((type = buffer[pos++]) != IscCodes.isc_info_end)
			{
				length = IscHelper.VaxInteger(buffer, pos, 2);
				pos += 2;

				switch (type)
				{
					case IscCodes.isc_info_sql_records:
						int l;
						int t;

						while ((t = buffer[pos++]) != IscCodes.isc_info_end)
						{
							l = IscHelper.VaxInteger(buffer, pos, 2);
							pos += 2;

							switch (t)
							{
								case IscCodes.isc_info_req_insert_count:
									insertCount = IscHelper.VaxInteger(buffer, pos, l);
									break;

								case IscCodes.isc_info_req_update_count:
									updateCount = IscHelper.VaxInteger(buffer, pos, l);
									break;

								case IscCodes.isc_info_req_delete_count:
									deleteCount = IscHelper.VaxInteger(buffer, pos, l);
									break;

								case IscCodes.isc_info_req_select_count:
									selectCount = IscHelper.VaxInteger(buffer, pos, l);
									break;
							}

							pos += l;
						}
						break;

					default:
						pos += length;
						break;
				}
			}

			return insertCount + updateCount + deleteCount;
		}

		protected DbStatementType GetStatementType()
		{
			byte[] buffer = GetSqlInfo(StatementTypeInfoItems, IscCodes.STATEMENT_TYPE_BUFFER_SIZE);

			return ProcessStatementTypeInfoBuffer(buffer);
		}

		protected DbStatementType ProcessStatementTypeInfoBuffer(byte[] buffer)
		{
			DbStatementType stmtType = DbStatementType.None;
			int pos = 0;
			int length = 0;
			int type = 0;

			while ((type = buffer[pos++]) != IscCodes.isc_info_end)
			{
				length = IscHelper.VaxInteger(buffer, pos, 2);
				pos += 2;
				switch (type)
				{
					case IscCodes.isc_info_sql_stmt_type:
						stmtType = (DbStatementType)IscHelper.VaxInteger(buffer, pos, length);
						pos += length;
						break;

					default:
						pos += length;
						break;
				}
			}

			return stmtType;
		}

		protected void ClearArrayHandles()
		{
			if (Fields != null && Fields.Count > 0)
			{
				for (int i = 0; i < Fields.Count; i++)
				{
					if (Fields[i].IsArray())
					{
						Fields[i].ArrayHandle = null;
					}
				}
			}
		}

		#endregion
	}
}
