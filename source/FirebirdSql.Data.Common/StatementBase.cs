/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.ibphoenix.com/main.nfs?a=ibphoenix&l=;PAGES;NAME='ibp_idpl'
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Text;
using System.Data;

namespace FirebirdSql.Data.Common
{
#if (SINGLE_DLL)
	internal
#else
	public
#endif
	abstract class StatementBase
	{
		#region Protected Static Fields

		// Plan information items
		protected static byte[] DescribePlanInfoItems = new byte[] 
		{ 
			IscCodes.isc_info_sql_get_plan
		};

		// Records affected items
		protected static byte[] RecordsAffectedInfoItems = new byte[]
		{
			IscCodes.isc_info_sql_records
		};

		// Describe information items
		protected static byte[] DescribeInfoItems = new byte[] 
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
			IscCodes.isc_info_sql_owner,
			IscCodes.isc_info_sql_alias,
			IscCodes.isc_info_sql_describe_end
		};

		protected static byte[] DescribeBindInfoItems = new byte[] 
		{ 
			IscCodes.isc_info_sql_bind,
			IscCodes.isc_info_sql_describe_vars,
			IscCodes.isc_info_sql_sqlda_seq,
			IscCodes.isc_info_sql_type,
			IscCodes.isc_info_sql_sub_type,
			IscCodes.isc_info_sql_length,
			IscCodes.isc_info_sql_scale,
			IscCodes.isc_info_sql_field,
			IscCodes.isc_info_sql_relation,
			IscCodes.isc_info_sql_owner,
			IscCodes.isc_info_sql_alias,
			IscCodes.isc_info_sql_describe_end 
		};

		protected static byte[] StatementTypeInfoItems = new byte[]
		{
			IscCodes.isc_info_sql_stmt_type
		};

		#endregion

		#region Properties

		public abstract IDbAttachment DB
		{
			get;
			set;
		}

		public abstract ITransaction Transaction
		{
			get;
			set;
		}
		
		public abstract RowDescriptor Parameters
		{
			get;
			set;
		}

		public abstract RowDescriptor Fields
		{
			get;
		}

		public abstract int RecordsAffected
		{
			get;
		}

		public abstract bool IsPrepared
		{
			get;
		}

		public abstract object[] Rows
		{
			get;
		}

		public abstract DbStatementType StatementType
		{
			get;
			set;
		}

		public abstract StatementState State
		{
			get;
			set;
		}

		#endregion
		
		#region Protected Fields

		protected TransactionUpdateEventHandler TransactionUpdate;

		#endregion

		#region Abstract Methods
		
		public abstract void Describe();
		public abstract void DescribeParameters();
		public abstract void Prepare(string commandText);
		public abstract void Execute();
		public abstract void ExecuteImmediate(string commandText);
		public abstract DbValue[] Fetch();
		public abstract byte[] GetSqlInfo(byte[] items, int bufferLength);
		// public abstract void SetCursorName(string cursorName);

		public abstract BlobBase CreateBlob();
		public abstract BlobBase CreateBlob(long handle);

		public abstract ArrayBase CreateArray(string tableName, string fieldName);

		public abstract ArrayBase CreateArray(
			long handle,
			string tableName,
			string fieldName);

		#endregion

		#region Protected Abstract Methods
		
		protected abstract void TransactionUpdated(object sender, EventArgs e);
		protected abstract void Free(int option);
		protected abstract DbStatementType GetStatementType();

		#endregion

		#region Methods

        public string GetExecutionPlan()
        {
            string plan = String.Empty;
            int count = 0;
            int bufferSize = IscCodes.MAX_BUFFER_SIZE;
            byte[] buffer = buffer = this.GetSqlInfo(DescribePlanInfoItems, bufferSize);

            while (buffer[0] == IscCodes.isc_info_truncated && count < 4)
            {
                bufferSize *= 2;
                buffer = this.GetSqlInfo(DescribePlanInfoItems, bufferSize);
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
                plan = Encoding.Default.GetString(buffer, 4, --len);
            }

            return plan;
        }

        public virtual void Close()
		{
			if (this.State == StatementState.Executed || 
				this.State == StatementState.Error)
			{
				if (this.StatementType == DbStatementType.Select			||
					this.StatementType == DbStatementType.SelectForUpdate	||
					this.StatementType == DbStatementType.StoredProcedure)
				{				
					this.Free(IscCodes.DSQL_close);
					this.State = StatementState.Closed;
				}
			}
		}

		public virtual void Release()
		{
			if (this.TransactionUpdate != null)
			{
				this.Transaction.Update -= this.TransactionUpdate;
				this.TransactionUpdate = null;
			}

			this.Free(IscCodes.DSQL_drop);
			this.State			= StatementState.Deallocated;
			this.StatementType	= DbStatementType.None;			
		}

		#endregion

		#region Protected Methods

		protected int GetRecordsAffected()
		{
			int insertCount		= 0;
			int updateCount		= 0;
			int deleteCount		= 0;
			int selectCount		= 0;
			int pos				= 0;
			int length			= 0;
			int type			= 0;

			byte[] buffer = this.GetSqlInfo(
				RecordsAffectedInfoItems, 
				IscCodes.MAX_BUFFER_SIZE);

			while ((type = buffer[pos++]) != IscCodes.isc_info_end) 
			{
				length = this.DB.VaxInteger(buffer, pos, 2);
				pos += 2;
				switch (type) 
				{
					case IscCodes.isc_info_sql_records:
						int l;
						int t;
						while ((t = buffer[pos++]) != IscCodes.isc_info_end) 
						{
							l = this.DB.VaxInteger(buffer, pos, 2);
							pos += 2;
							switch (t) 
							{
								case IscCodes.isc_info_req_insert_count:
									insertCount = this.DB.VaxInteger(
										buffer, pos, l);
									break;
								
								case IscCodes.isc_info_req_update_count:
									updateCount = this.DB.VaxInteger(
										buffer, pos, l);
									break;
								
								case IscCodes.isc_info_req_delete_count:
									deleteCount = this.DB.VaxInteger(
										buffer, pos, l);
									break;
								
								case IscCodes.isc_info_req_select_count:
									selectCount = this.DB.VaxInteger(
										buffer, pos, l);
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

		#endregion
	}
}
