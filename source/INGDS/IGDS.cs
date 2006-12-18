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
 * 
 *  This file was originally ported from JayBird <http://firebird.sourceforge.net/>
 */

using System;
using System.Text;
using System.Collections;

namespace FirebirdSql.Data.INGDS
{
	/// <include file='xmldoc/igds.xml' path='doc/member[@name="T:GdsCodes"]/*'/>
	internal interface IGDS
	{
		//
		// Database functions
		//

		void isc_create_database(string file_name,
			isc_db_handle db_handle,
			IClumplet c);

		void isc_attach_database(string file_name,
			isc_db_handle db_handle,
			IClumplet c);

		void isc_database_info(isc_db_handle db_handle,
			int item_length,
			byte[] items,
			int buffer_length,
			byte[] buffer);

		void isc_detach_database(isc_db_handle db_handle);

		void isc_drop_database(isc_db_handle db_handle);

		byte[] isc_expand_dpb(byte[] dpb, int dpb_length,
			int param, object[] parameters);

		//
		// Transactions
		//

		void isc_start_transaction(isc_tr_handle tr_handle,
			isc_db_handle db_handle,
			ArrayList tpb);

		void isc_commit_transaction(isc_tr_handle tr_handle);

		void isc_commit_retaining(isc_tr_handle tr_handle);

		void isc_prepare_transaction(isc_tr_handle tr_handle);

		void isc_prepare_transaction2(isc_tr_handle tr_handle,
			byte[] bytes);

		void isc_rollback_transaction(isc_tr_handle tr_handle);
		void isc_rollback_retaining(isc_tr_handle tr_handle);


		//
		// Dynamic SQL
		//

		void isc_dsql_allocate_statement(isc_db_handle db_handle,
			isc_stmt_handle stmt_handle);

		void isc_dsql_alloc_statement2(isc_db_handle db_handle,
			isc_stmt_handle stmt_handle);

		XSQLDA isc_dsql_describe(isc_stmt_handle stmt_handle,
			int da_version);

		XSQLDA isc_dsql_describe_bind(isc_stmt_handle stmt_handle,
			int da_version);

		void isc_dsql_execute(isc_tr_handle tr_handle,
			isc_stmt_handle stmt_handle,
			int da_version,			
			XSQLDA xsqlda);

		void isc_dsql_execute2(isc_tr_handle tr_handle,
			isc_stmt_handle stmt_handle,
			int da_version,			
			XSQLDA in_xsqlda,
			XSQLDA out_xsqlda);

		void isc_dsql_execute_immediate(isc_db_handle db_handle,
			isc_tr_handle tr_handle,
			string statement,
			int dialect,
			XSQLDA xsqlda);
		void isc_dsql_execute_immediate(isc_db_handle db_handle,
			isc_tr_handle tr_handle,
			string statement,
			Encoding encoding,
			int dialect,
			XSQLDA xsqlda);

		void isc_dsql_exec_immed2(isc_db_handle db_handle,
			isc_tr_handle tr_handle,
			string statement,
			int dialect,
			XSQLDA in_xsqlda,
			XSQLDA out_xsqlda);
	                               
		void isc_dsql_exec_immed2(isc_db_handle db_handle,
			isc_tr_handle tr_handle,
			string statement,
			Encoding encoding,
			int dialect,
			XSQLDA in_xsqlda,
			XSQLDA out_xsqlda);

		object[] isc_dsql_fetch(isc_stmt_handle stmt_handle,
			int da_version,
			XSQLDA xsqlda);

		void isc_dsql_free_statement(isc_stmt_handle stmt_handle,
			int option);

		XSQLDA isc_dsql_prepare(isc_tr_handle tr_handle,
			isc_stmt_handle stmt_handle,
			string statement,
			int dialect);
	                           
		XSQLDA isc_dsql_prepare(isc_tr_handle tr_handle,
			isc_stmt_handle stmt_handle,
			string statement,
			Encoding encoding,
			int dialect);

		void isc_dsql_set_cursor_name(isc_stmt_handle stmt_handle,
			string cursor_name,
			int type);


		byte[] isc_dsql_sql_info(isc_stmt_handle stmt_handle,
			int item_length,
			byte[] items,
			int buffer_length);


		int isc_vax_integer(byte[] buffer, int pos, int length);


		//
		//Blob methods
		//

		void isc_create_blob2(isc_db_handle db,
			isc_tr_handle tr,
			isc_blob_handle blob,
			IClumplet bpb);

		void isc_open_blob2(isc_db_handle db,
			isc_tr_handle tr,
			isc_blob_handle blob,
			IClumplet bpb);

		byte[] isc_get_segment(isc_blob_handle blob,
			int maxread);

		void isc_put_segment(isc_blob_handle blob_handle,
			byte[] buffer);

		void isc_close_blob(isc_blob_handle blob);

		//
		// Handle declaration methods
		//
		isc_db_handle get_new_isc_db_handle();

		isc_tr_handle get_new_isc_tr_handle();

		isc_stmt_handle get_new_isc_stmt_handle();

		isc_blob_handle get_new_isc_blob_handle();
	}
}
