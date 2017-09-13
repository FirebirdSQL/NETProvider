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

//$Authors = Carlos Guzman Alvarez, Dean Harding, Jiri Cincura (jiri@cincura.net)

using FirebirdSql.Data.Client.Native.Handle;
using FirebirdSql.Data.Common;
using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace FirebirdSql.Data.Client.Native
{
	/// <summary>
	/// This is the interface that the dynamically-generated class uses to call the native library. 
	/// Each connection can specify different client library to use even on the same OS. 
	/// IFbClient and FbClientactory classes are implemented to support this feature.
	/// Public visibility added, because auto-generated assembly can't work with internal types
	/// </summary>
	public interface IFbClient
	{
		#region Array Functions

		IntPtr isc_array_get_slice(
			[In, Out] IntPtr[] statusVector,
			[MarshalAs(UnmanagedType.I4)] ref DatabaseHandle dbHandle,
			[MarshalAs(UnmanagedType.I4)] ref TransactionHandle trHandle,
			ref long arrayId,
			IntPtr desc,
			byte[] destArray,
			ref int sliceLength);

		IntPtr isc_array_put_slice(
			[In, Out] IntPtr[] statusVector,
			[MarshalAs(UnmanagedType.I4)] ref DatabaseHandle dbHandle,
			[MarshalAs(UnmanagedType.I4)] ref TransactionHandle trHandle,
			ref long arrayId,
			IntPtr desc,
			byte[] sourceArray,
			ref int sliceLength);

		#endregion

		#region Blob Functions

		IntPtr isc_create_blob2(
			[In, Out] IntPtr[] statusVector,
			[MarshalAs(UnmanagedType.I4)] ref DatabaseHandle dbHandle,
			[MarshalAs(UnmanagedType.I4)] ref TransactionHandle trHandle,
			[MarshalAs(UnmanagedType.I4)] ref BlobHandle blobHandle,
			ref long blobId,
			short bpbLength,
			byte[] bpbAddress);

		IntPtr isc_open_blob2(
			[In, Out] IntPtr[] statusVector,
			[MarshalAs(UnmanagedType.I4)] ref DatabaseHandle dbHandle,
			[MarshalAs(UnmanagedType.I4)] ref TransactionHandle trHandle,
			[MarshalAs(UnmanagedType.I4)] ref BlobHandle blobHandle,
			ref long blobId,
			short bpbLength,
			byte[] bpbAddress);

		IntPtr isc_get_segment(
			[In, Out] IntPtr[] statusVector,
			[MarshalAs(UnmanagedType.I4)] ref BlobHandle blobHandle,
			ref short actualSegLength,
			short segBufferLength,
			byte[] segBuffer);

		IntPtr isc_put_segment(
			[In, Out] IntPtr[] statusVector,
			[MarshalAs(UnmanagedType.I4)] ref BlobHandle blobHandle,
			short segBufferLength,
			byte[] segBuffer);

		IntPtr isc_cancel_blob(
			[In, Out] IntPtr[] statusVector,
			[MarshalAs(UnmanagedType.I4)] ref BlobHandle blobHandle);

		IntPtr isc_close_blob(
			[In, Out] IntPtr[] statusVector,
			[MarshalAs(UnmanagedType.I4)] ref BlobHandle blobHandle);

		#endregion

		#region Database Functions

		IntPtr isc_attach_database(
			[In, Out] IntPtr[] statusVector,
			short dbNameLength,
			byte[] dbName,
			[MarshalAs(UnmanagedType.I4)] ref DatabaseHandle dbHandle,
			short parmBufferLength,
			byte[] parmBuffer);

		IntPtr isc_detach_database(
			[In, Out] IntPtr[] statusVector,
			[MarshalAs(UnmanagedType.I4)] ref DatabaseHandle dbHandle);

		IntPtr isc_database_info(
			[In, Out] IntPtr[] statusVector,
			[MarshalAs(UnmanagedType.I4)] ref DatabaseHandle dbHandle,
			short itemListBufferLength,
			byte[] itemListBuffer,
			short resultBufferLength,
			byte[] resultBuffer);

		IntPtr isc_create_database(
			[In, Out] IntPtr[] statusVector,
			short dbNameLength,
			byte[] dbName,
			[MarshalAs(UnmanagedType.I4)] ref DatabaseHandle dbHandle,
			short parmBufferLength,
			byte[] parmBuffer,
			short dbType);
		IntPtr isc_drop_database(
			[In, Out] IntPtr[] statusVector,
			[MarshalAs(UnmanagedType.I4)] ref DatabaseHandle dbHandle);

		#endregion

		#region Transaction Functions

		IntPtr isc_start_multiple(
			[In, Out]   IntPtr[] statusVector,
			[MarshalAs(UnmanagedType.I4)] ref TransactionHandle trHandle,
			short dbHandleCount,
			IntPtr tebVectorAddress);

		IntPtr isc_commit_transaction(
			[In, Out] IntPtr[] statusVector,
			[MarshalAs(UnmanagedType.I4)] ref TransactionHandle trHandle);

		IntPtr isc_commit_retaining(
			[In, Out] IntPtr[] statusVector,
			[MarshalAs(UnmanagedType.I4)] ref TransactionHandle trHandle);

		IntPtr isc_rollback_transaction(
			[In, Out] IntPtr[] statusVector,
			[MarshalAs(UnmanagedType.I4)] ref TransactionHandle trHandle);

		IntPtr isc_rollback_retaining(
			[In, Out] IntPtr[] statusVector,
			[MarshalAs(UnmanagedType.I4)] ref TransactionHandle trHandle);

		#endregion

		#region Cancel Functions

		IntPtr fb_cancel_operation(
			[In, Out] IntPtr[] statusVector,
			[MarshalAs(UnmanagedType.I4)] ref DatabaseHandle dbHandle,
			int option);

		#endregion

		#region DSQL Functions

		IntPtr isc_dsql_allocate_statement(
			[In, Out] IntPtr[] statusVector,
			[MarshalAs(UnmanagedType.I4)] ref DatabaseHandle dbHandle,
			[MarshalAs(UnmanagedType.I4)] ref StatementHandle stmtHandle);

		IntPtr isc_dsql_describe(
			[In, Out] IntPtr[] statusVector,
			[MarshalAs(UnmanagedType.I4)] ref StatementHandle stmtHandle,
			short daVersion,
			IntPtr xsqlda);

		IntPtr isc_dsql_describe_bind(
			[In, Out] IntPtr[] statusVector,
			 [MarshalAs(UnmanagedType.I4)] ref StatementHandle stmtHandle,
			short daVersion,
			IntPtr xsqlda);

		IntPtr isc_dsql_prepare(
			[In, Out] IntPtr[] statusVector,
			[MarshalAs(UnmanagedType.I4)] ref TransactionHandle trHandle,
			[MarshalAs(UnmanagedType.I4)] ref StatementHandle stmtHandle,
			short length,
			byte[] statement,
			short dialect,
			IntPtr xsqlda);

		IntPtr isc_dsql_execute(
			[In, Out] IntPtr[] statusVector,
			[MarshalAs(UnmanagedType.I4)] ref TransactionHandle trHandle,
			[MarshalAs(UnmanagedType.I4)] ref StatementHandle stmtHandle,
			short daVersion,
			IntPtr xsqlda);

		IntPtr isc_dsql_execute2(
			[In, Out] IntPtr[] statusVector,
			[MarshalAs(UnmanagedType.I4)] ref TransactionHandle trHandle,
			[MarshalAs(UnmanagedType.I4)] ref StatementHandle stmtHandle,
			short da_version,
			IntPtr inXsqlda,
			IntPtr outXsqlda);

		IntPtr isc_dsql_fetch(
			[In, Out] IntPtr[] statusVector,
			[MarshalAs(UnmanagedType.I4)] ref StatementHandle stmtHandle,
			short daVersion,
			IntPtr xsqlda);

		IntPtr isc_dsql_free_statement(
			[In, Out] IntPtr[] statusVector,
			[MarshalAs(UnmanagedType.I4)] ref StatementHandle stmtHandle,
			short option);

		IntPtr isc_dsql_sql_info(
			[In, Out] IntPtr[] statusVector,
			[MarshalAs(UnmanagedType.I4)] ref StatementHandle stmtHandle,
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
			ref int svcHandle,
			short spbLength,
			byte[] spb);

		IntPtr isc_service_start(
			[In, Out] IntPtr[] statusVector,
			ref int svcHandle,
			ref int reserved,
			short spbLength,
			byte[] spb);

		IntPtr isc_service_detach(
			[In, Out] IntPtr[] statusVector,
			ref int svcHandle);

		IntPtr isc_service_query(
			[In, Out] IntPtr[] statusVector,
			ref int svcHandle,
			ref int reserved,
			short sendSpbLength,
			byte[] sendSpb,
			short requestSpbLength,
			byte[] requestSpb,
			short bufferLength,
			byte[] buffer);

		#endregion
	}
}
