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
 *	Copyright (c) 2002-2007 Carlos Guzman Alvarez
 *	All Rights Reserved.
 * 
 * Contributors:
 *      2007 - Dean Harding
 *      Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Runtime.InteropServices;

namespace FirebirdSql.Data.Client.Native
{
	/// <summary>
	/// This is the interface that the dynamically-generated class uses to call the native library.
	/// </summary>
	public interface IFbClient
	{
		#region Array Functions

		IntPtr isc_array_get_slice(
			[In, Out] IntPtr[] statusVector,
			ref	int dbHandle,
			ref	int trHandle,
			ref	long arrayId,
			IntPtr desc,
			byte[] destArray,
			ref	int sliceLength);

		IntPtr isc_array_put_slice(
			[In, Out] IntPtr[] statusVector,
			ref	int dbHandle,
			ref	int trHandle,
			ref	long arrayId,
			IntPtr desc,
			byte[] sourceArray,
			ref	int sliceLength);

		#endregion

		#region Blob Functions

		IntPtr isc_create_blob2(
			[In, Out] IntPtr[] statusVector,
			ref	int dbHandle,
			ref	int trHandle,
			ref	int blobHandle,
			ref	long blobId,
			short bpbLength,
			byte[] bpbAddress);

		IntPtr isc_open_blob2(
			[In, Out] IntPtr[] statusVector,
			ref	int dbHandle,
			ref	int trHandle,
			ref	int blobHandle,
			ref	long blobId,
			short bpbLength,
			byte[] bpbAddress);

		IntPtr isc_get_segment(
			[In, Out] IntPtr[] statusVector,
			ref	int blobHandle,
			ref	short actualSegLength,
			short segBufferLength,
			byte[] segBuffer);

		IntPtr isc_put_segment(
			[In, Out] IntPtr[] statusVector,
			ref	int blobHandle,
			short segBufferLength,
			byte[] segBuffer);

		IntPtr isc_cancel_blob(
			[In, Out] IntPtr[] statusVector,
			ref	int blobHandle);

		IntPtr isc_close_blob(
			[In, Out] IntPtr[] statusVector,
			ref	int blobHandle);

		#endregion

		#region Database Functions

		IntPtr isc_attach_database(
			[In, Out] IntPtr[] statusVector,
			short dbNameLength,
			byte[] dbName,
			ref	int dbHandle,
			short parmBufferLength,
			byte[] parmBuffer);

		IntPtr isc_detach_database(
			[In, Out] IntPtr[] statusVector,
			ref	int dbHandle);

		IntPtr isc_database_info(
			[In, Out] IntPtr[] statusVector,
			ref	int dbHandle,
			short itemListBufferLength,
			byte[] itemListBuffer,
			short resultBufferLength,
			byte[] resultBuffer);

		IntPtr isc_create_database(
			[In, Out] IntPtr[] statusVector,
			short dbNameLength,
			byte[] dbName,
			ref	int dbHandle,
			short parmBufferLength,
			byte[] parmBuffer,
			short dbType);

		IntPtr isc_drop_database(
			[In, Out] IntPtr[] statusVector,
			ref	int dbHandle);

		#endregion

		#region Transaction Functions

		IntPtr isc_start_multiple(
			[In, Out]	IntPtr[] statusVector,
			ref	int trHandle,
			short dbHandleCount,
			IntPtr tebVectorAddress);

		IntPtr isc_commit_transaction(
			[In, Out] IntPtr[] statusVector,
			ref	int trHandle);

		IntPtr isc_commit_retaining(
			[In, Out] IntPtr[] statusVector,
			ref	int trHandle);

		IntPtr isc_rollback_transaction(
			[In, Out] IntPtr[] statusVector,
			ref	int trHandle);

		IntPtr isc_rollback_retaining(
			[In, Out] IntPtr[] statusVector,
			ref	int trHandle);

		#endregion

		#region Cancel Functions

		IntPtr fb_cancel_operation(
			[In, Out] IntPtr[] statusVector,
			ref int dbHandle,
			int option);

		#endregion

		#region DSQL Functions

		IntPtr isc_dsql_allocate_statement(
			[In, Out] IntPtr[] statusVector,
			ref	int dbHandle,
			ref	int stmtHandle);

		IntPtr isc_dsql_describe(
			[In, Out] IntPtr[] statusVector,
			ref	int stmtHandle,
			short daVersion,
			IntPtr xsqlda);

		IntPtr isc_dsql_describe_bind(
			[In, Out] IntPtr[] statusVector,
			ref	int stmtHandle,
			short daVersion,
			IntPtr xsqlda);

		IntPtr isc_dsql_prepare(
			[In, Out] IntPtr[] statusVector,
			ref	int trHandle,
			ref	int stmtHandle,
			short length,
			byte[] statement,
			short dialect,
			IntPtr xsqlda);

		IntPtr isc_dsql_execute(
			[In, Out] IntPtr[] statusVector,
			ref	int trHandle,
			ref	int stmtHandle,
			short daVersion,
			IntPtr xsqlda);

		IntPtr isc_dsql_execute2(
			[In, Out] IntPtr[] statusVector,
			ref	int trHandle,
			ref	int stmtHandle,
			short da_version,
			IntPtr inXsqlda,
			IntPtr outXsqlda);

		IntPtr isc_dsql_fetch(
			[In, Out] IntPtr[] statusVector,
			ref	int stmtHandle,
			short daVersion,
			IntPtr xsqlda);

		IntPtr isc_dsql_free_statement(
			[In, Out] IntPtr[] statusVector,
			ref	int stmtHandle,
			short option);

		IntPtr isc_dsql_sql_info(
			[In, Out] IntPtr[] statusVector,
			ref	int stmtHandle,
			short itemsLength,
			byte[] items,
			short bufferLength,
			byte[] buffer);

		IntPtr isc_vax_integer(
			byte[] buffer,
			short length);

		#endregion

		#region Services Functions

		IntPtr isc_service_attach(
			[In, Out] IntPtr[] statusVector,
			short serviceLength,
			string service,
			ref	int svcHandle,
			short spbLength,
			byte[] spb);

		IntPtr isc_service_start(
			[In, Out] IntPtr[] statusVector,
			ref	int svcHandle,
			ref	int reserved,
			short spbLength,
			byte[] spb);

		IntPtr isc_service_detach(
			[In, Out] IntPtr[] statusVector,
			ref	int svcHandle);

		IntPtr isc_service_query(
			[In, Out] IntPtr[] statusVector,
			ref	int svcHandle,
			ref	int reserved,
			short sendSpbLength,
			byte[] sendSpb,
			short requestSpbLength,
			byte[] requestSpb,
			short bufferLength,
			byte[] buffer);

		#endregion
	}
}
