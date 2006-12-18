/*
 *  .NET External Procedure Engine for Firebird
 *
 *     The contents of this file are subject to the Initial
 *     Developer's Public License Version 1.0 (the "License");
 *     you may not use this file except in compliance with the
 *     License. You may obtain a copy of the License at
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *     express or implied.  See the License for the specific
 *     language governing rights and limitations under the License.
 *
 *  Copyright (c) 2005 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

#include "stdafx.h"
#include "fb_api_proto.h"
#include "fb_external_engine.h"
#include "CLRExternalEngine.h"
#include "RuntimeHostException.h"
#include "Convert.h"

static const FirebirdApiPointer* FirebirdAPI = NULL;
static ExternalEngine* Engine = NULL;

extern "C" __declspec(dllexport) void register_plugin()
{
}

extern "C" __declspec(dllexport) ExternalEngine* get_external_engine(ErrorObject* error, const void* ptr)
{
    if (Engine == NULL)
    {
		try
		{
			Engine = new CLRExternalEngine();
		}
		catch (RuntimeHostException* e)
		{
			error->addString(e->GetMessage().data(), fb_string_ascii);
		}
    }

	if (FirebirdAPI == NULL)
	{
		FirebirdAPI = (FirebirdApiPointer*)ptr;
	}

	return Engine;
}

extern "C" __declspec(dllexport) ISC_STATUS ext_get_current_database(ISC_STATUS *status, isc_db_handle *dbHandle)
{
	isc_tr_handle trHandle = 0;

	return FirebirdAPI->isc_get_curret_attachment_and_transactional(status, dbHandle, &trHandle);
}

extern "C" __declspec(dllexport) ISC_STATUS ext_get_current_transaction(ISC_STATUS *status, isc_tr_handle *trHandle)
{
	isc_db_handle dbHandle = 0;

	return FirebirdAPI->isc_get_curret_attachment_and_transactional(status, &dbHandle, trHandle);
}

//
// misc entrypoints
//
extern "C" __declspec(dllexport) ISC_LONG ext_vax_integer(char *buffer, short length)
{
	return FirebirdAPI->isc_vax_integer(buffer, length);
}


//
// database entrypoints
//
extern "C" __declspec(dllexport) ISC_STATUS ext_database_info(ISC_STATUS *status, 
															  isc_db_handle *dbHandle, 
															  short itemListBufferLength,
															  char * itemListBuffer,
															  short resultBufferLength,
															  char * resultBuffer)
{
	return FirebirdAPI->isc_database_info(status, dbHandle, itemListBufferLength, itemListBuffer, resultBufferLength, resultBuffer);
}

//
// statement entrypoints
//
extern "C" __declspec(dllexport) ISC_STATUS ext_dsql_allocate_statement(ISC_STATUS *status, 
																		isc_db_handle *dbHandle, 
																		isc_stmt_handle * trHandle)
{
	return FirebirdAPI->isc_dsql_allocate_statement(status, dbHandle, trHandle);
}

extern "C" __declspec(dllexport) ISC_STATUS ext_dsql_alloc_statement2(ISC_STATUS *status, 
																	  isc_db_handle *dbHandle, 
																	  isc_stmt_handle * trHandle)
{
	return FirebirdAPI->isc_dsql_alloc_statement2(status, dbHandle, trHandle);
}

extern "C" __declspec(dllexport) ISC_STATUS ext_dsql_describe(ISC_STATUS *status, 
															  isc_stmt_handle * stmtHandle, 
															  unsigned short length, 
															  XSQLDA * sqlda)
{
	return FirebirdAPI->isc_dsql_describe(status, stmtHandle, length, sqlda);
}

extern "C" __declspec(dllexport) ISC_STATUS ext_dsql_describe_bind(ISC_STATUS *status, 
																   isc_stmt_handle *stmtHandle, 
																   unsigned short length, 
																   XSQLDA *sqlda)
{
	return FirebirdAPI->isc_dsql_describe_bind(status, stmtHandle, length, sqlda);
}

extern "C" __declspec(dllexport) ISC_STATUS ext_dsql_execute(ISC_STATUS *status, 
															 isc_tr_handle *trHandle, 
															 isc_stmt_handle *stmtHandle, 
															 unsigned short length, 
															 XSQLDA *sqlda)
{
	return FirebirdAPI->isc_dsql_execute(status, trHandle, stmtHandle, length, sqlda);
}

extern "C" __declspec(dllexport) ISC_STATUS ext_dsql_execute2(ISC_STATUS *status, 
															  isc_tr_handle *trHandle, 
															  isc_stmt_handle *stmtHandle, 
															  unsigned short length, 
															  XSQLDA *inSqlda, 
															  XSQLDA *outSqlda)
{
	return FirebirdAPI->isc_dsql_execute2(status, trHandle, stmtHandle, length, inSqlda, outSqlda);
}

extern "C" __declspec(dllexport) ISC_STATUS ext_dsql_fetch(ISC_STATUS *status, 
														   isc_stmt_handle *stmHandle, 
														   unsigned short length, 
														   XSQLDA *sqlda)
{
	return FirebirdAPI->isc_dsql_fetch(status, stmHandle, length, sqlda);
}

extern "C" __declspec(dllexport) ISC_STATUS ext_dsql_free_statement(ISC_STATUS *status, 
																	isc_stmt_handle *stmtHandle, 
																	unsigned short action)
{
	return FirebirdAPI->isc_dsql_free_statement(status, stmtHandle, action);
}

extern "C" __declspec(dllexport) ISC_STATUS ext_dsql_prepare(ISC_STATUS *status,
															 isc_tr_handle *trHandle,
															 isc_stmt_handle *stmtHandle,
															 unsigned short length,
															 char *statement,
															 unsigned short dialect,
															 XSQLDA *sqlda)
{
	return FirebirdAPI->isc_dsql_prepare(status, trHandle, stmtHandle, length, statement, dialect, sqlda);
}

extern "C" __declspec(dllexport) ISC_STATUS ext_dsql_sql_info(ISC_STATUS *status, 
															  isc_stmt_handle *stmtHandle, 
															  short itemsLength, 
															  const char *items, 
															  short bufferLength, 
															  char *buffer)
{
	return FirebirdAPI->isc_dsql_sql_info(status, stmtHandle, itemsLength, items, bufferLength, buffer);
}

//
// transaction entrypoints
//
extern "C" __declspec(dllexport) ISC_STATUS ext_commit_retaining(ISC_STATUS *status, isc_tr_handle *trHandle)
{
	return FirebirdAPI->isc_commit_retaining(status, trHandle);
}

extern "C" __declspec(dllexport) ISC_STATUS ext_commit_transaction(ISC_STATUS *status, isc_tr_handle *trHandle)
{
	return FirebirdAPI->isc_commit_transaction(status, trHandle);
}

extern "C" __declspec(dllexport) ISC_STATUS ext_rollback_retaining(ISC_STATUS *status, isc_tr_handle *trHandle)
{
	return FirebirdAPI->isc_rollback_retaining(status, trHandle);
}

extern "C" __declspec(dllexport) ISC_STATUS ext_rollback_transaction(ISC_STATUS *status, isc_tr_handle *trHandle)
{
	return FirebirdAPI->isc_rollback_transaction(status, trHandle);
}

//
// blob entrypoints
//
extern "C" __declspec(dllexport) ISC_STATUS ext_create_blob2(ISC_STATUS *status, 
															 isc_db_handle *dbHandle, 
															 isc_tr_handle *trHandle, 
															 isc_blob_handle *blobHandle, 
															 ISC_QUAD *blobId, 
															 short bpbLength, 
															 char *bpbAddress)
{
	return FirebirdAPI->isc_create_blob2(status, dbHandle, trHandle, blobHandle, blobId, bpbLength, bpbAddress);
}

extern "C" __declspec(dllexport) ISC_STATUS ext_open_blob2(ISC_STATUS *status, 
														   isc_db_handle *dbHandle, 
														   isc_tr_handle *trHandle, 
														   isc_blob_handle *blobHandle, 
														   ISC_QUAD *blobId, 
														   ISC_USHORT bpbLength, 
														   ISC_UCHAR *bpbAddress)
{
	return FirebirdAPI->isc_open_blob2(status, dbHandle, trHandle, blobHandle, blobId, bpbLength, bpbAddress);
}

extern "C" __declspec(dllexport) ISC_STATUS ext_get_segment(ISC_STATUS *status, 
															isc_blob_handle *blobHandle, 
															unsigned short *actualSegLength, 
															unsigned short segBufferLength, 
															char *segBuffer)
{
	return FirebirdAPI->isc_get_segment(status, blobHandle, actualSegLength, segBufferLength, segBuffer);
}

extern "C" __declspec(dllexport) ISC_STATUS ext_put_segment(ISC_STATUS *status, isc_blob_handle *blobHandle, unsigned short segBufferLength, char *segBuffer)
{
	return FirebirdAPI->isc_put_segment(status, blobHandle, segBufferLength, segBuffer);
}

extern "C" __declspec(dllexport) ISC_STATUS ext_cancel_blob(ISC_STATUS *status, isc_blob_handle *blobHandle)
{
	return FirebirdAPI->isc_cancel_blob(status, blobHandle);
}

extern "C" __declspec(dllexport) ISC_STATUS ext_close_blob(ISC_STATUS *status, isc_blob_handle *blobHandle)
{
	return FirebirdAPI->isc_close_blob(status, blobHandle);
}

//
// array entrypoints
//
extern "C" __declspec(dllexport) ISC_STATUS ext_array_get_slice(ISC_STATUS *status, 
																isc_db_handle *dbHandle,
																isc_tr_handle *trHandle,
																ISC_QUAD *arrayId,
																ISC_ARRAY_DESC *desc,
																void *slice,
																ISC_LONG *sliceLength)
{
	return FirebirdAPI->isc_array_get_slice(status, dbHandle, trHandle, arrayId, desc, slice, sliceLength);
}

extern "C" __declspec(dllexport) ISC_STATUS ext_array_put_slice(ISC_STATUS *status,
																isc_db_handle *dbHandle,
																isc_tr_handle *trHandle,
																ISC_QUAD *arrayId,
																ISC_ARRAY_DESC *desc,
																void *slice,
																ISC_LONG *sliceLength)
{
	return FirebirdAPI->isc_array_put_slice(status, dbHandle, trHandle, arrayId, desc, slice, sliceLength);
}

extern "C" __declspec(dllexport) ISC_STATUS ext_free(char *objectHandle)
{
	return FirebirdAPI->isc_free(objectHandle);
}

extern "C" __declspec(dllexport) int ext_get_trigger_table_name(ISC_STATUS *status, char* tableName, int length)
{
	bool result = FirebirdAPI->isc_get_trigger_table_name(status, tableName, length);
	std::string strTableName (tableName);

	return ((result) ? (int)strTableName.size() : 0);
}

extern "C" __declspec(dllexport) int ext_get_trigger_action(ISC_STATUS *status)
{
	return FirebirdAPI->isc_get_trigger_action(status);
}

extern "C" __declspec(dllexport) bool ext_get_trigger_field(ISC_STATUS *status, int isOldNew, char* fieldName, void* value)
{
	return FirebirdAPI->isc_get_trigger_field(status, isOldNew, fieldName, (PARAMDSC*)value);
}

extern "C" __declspec(dllexport) bool ext_set_trigger_field(ISC_STATUS *status, int isOldNew, char* fieldName, void* value)
{
	PARAMDSC out;

	bool fieldFound = FirebirdAPI->isc_get_trigger_field(status, isOldNew, fieldName, &out);

	if (fieldFound)
	{
		Convert::Copy((PARAMDSC*)value, &out);
	}

	return fieldFound;
}