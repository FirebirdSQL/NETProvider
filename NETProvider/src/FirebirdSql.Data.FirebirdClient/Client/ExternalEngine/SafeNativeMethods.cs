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
 *	Copyright (c) 2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace FirebirdSql.Data.Client.ExternalEngine
{
	[SuppressUnmanagedCodeSecurity]
	internal sealed class SafeNativeMethods
	{
		#region Conditional Constants

		public const string DllPath = "clrexternalengine";

		#endregion

		#region Constructors

		private SafeNativeMethods()
		{
		}

		#endregion

		#region External Engine Functions

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_get_current_database")]
		public static extern int isc_get_current_database([In, Out] int[] statusVector, ref int dbHandle);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_get_current_transaction")]
		public static extern int isc_get_current_transaction([In, Out] int[] statusVector, ref int trHandle);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_get_trigger_table_name")]
		public static extern int isc_get_trigger_table_name(
			[In, Out] int[] statusVector,
			byte[] tableName,
			int length);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_get_trigger_action")]
		public static extern int isc_get_trigger_action([In, Out] int[] statusVector);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_get_trigger_field")]
		public static extern bool isc_get_trigger_field(
			[In, Out] int[] statusVector,
			int isOldNew,
			byte[] fieldName,
			IntPtr paramdsc);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_set_trigger_field")]
		public static extern bool isc_set_trigger_field(
			[In, Out] int[] statusVector,
			int isOldNew,
			byte[] fieldName,
			IntPtr paramdsc);

		#endregion

		#region Array Functions

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_array_get_slice")]
		public static extern int isc_array_get_slice(
			[In, Out] int[] statusVector,
			ref	int dbHandle,
			ref	int trHandle,
			ref	long arrayId,
			IntPtr desc,
			byte[] destArray,
			ref	int sliceLength);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_array_put_slice")]
		public static extern int isc_array_put_slice(
			[In, Out] int[] statusVector,
			ref	int dbHandle,
			ref	int trHandle,
			ref	long arrayId,
			IntPtr desc,
			byte[] sourceArray,
			ref	int sliceLength);

		#endregion

		#region Blob Functions

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_create_blob2")]
		public static extern int isc_create_blob2(
			[In, Out] int[] statusVector,
			ref	int dbHandle,
			ref	int trHandle,
			ref	int blobHandle,
			ref	long blobId,
			short bpbLength,
			byte[] bpbAddress);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_open_blob2")]
		public static extern int isc_open_blob2(
			[In, Out] int[] statusVector,
			ref	int dbHandle,
			ref	int trHandle,
			ref	int blobHandle,
			ref	long blobId,
			short bpbLength,
			byte[] bpbAddress);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_get_segment")]
		public static extern int isc_get_segment(
			[In, Out] int[] statusVector,
			ref	int blobHandle,
			ref	short actualSegLength,
			short segBufferLength,
			byte[] segBuffer);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_put_segment")]
		public static extern int isc_put_segment(
			[In, Out] int[] statusVector,
			ref	int blobHandle,
			short segBufferLength,
			byte[] segBuffer);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_cancel_blob")]
		public static extern int isc_cancel_blob(
			[In, Out] int[] statusVector,
			ref	int blobHandle);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_close_blob")]
		public static extern int isc_close_blob(
			[In, Out] int[] statusVector,
			ref	int blobHandle);

		#endregion

		#region Database Functions

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_attach_database")]
		public static extern int isc_attach_database(
			[In, Out] int[] statusVector,
			short dbNameLength,
			string dbName,
			ref	int dbHandle,
			short parmBufferLength,
			byte[] parmBuffer);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_detach_database")]
		public static extern int isc_detach_database(
			[In, Out] int[] statusVector,
			ref	int dbHandle);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_database_info")]
		public static extern int isc_database_info(
			[In, Out] int[] statusVector,
			ref	int dbHandle,
			short itemListBufferLength,
			byte[] itemListBuffer,
			short resultBufferLength,
			byte[] resultBuffer);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_create_database")]
		public static extern int isc_create_database(
			[In, Out] int[] statusVector,
			short dbNameLength,
			string dbName,
			ref	int dbHandle,
			short parmBufferLength,
			byte[] parmBuffer,
			short dbType);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_drop_database")]
		public static extern int isc_drop_database(
			[In, Out] int[] statusVector,
			ref	int dbHandle);

		#endregion

		#region Transaction Functions

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_commit_transaction")]
		public static extern int isc_commit_transaction(
			[In, Out] int[] statusVector,
			ref	int trHandle);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_commit_retaining")]
		public static extern int isc_commit_retaining(
			[In, Out] int[] statusVector,
			ref	int trHandle);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_rollback_transaction")]
		public static extern int isc_rollback_transaction(
			[In, Out] int[] statusVector,
			ref	int trHandle);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_rollback_retaining")]
		public static extern int isc_rollback_retaining(
			[In, Out] int[] statusVector,
			ref	int trHandle);

		#endregion

		#region DSQL Functions

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_dsql_allocate_statement")]
		public static extern int isc_dsql_allocate_statement(
			[In, Out] int[] statusVector,
			ref	int dbHandle,
			ref	int stmtHandle);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_dsql_describe")]
		public static extern int isc_dsql_describe(
			[In, Out] int[] statusVector,
			ref	int stmtHandle,
			short daVersion,
			IntPtr xsqlda);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_dsql_describe_bind")]
		public static extern int isc_dsql_describe_bind(
			[In, Out] int[] statusVector,
			ref	int stmtHandle,
			short daVersion,
			IntPtr xsqlda);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_dsql_prepare")]
		public static extern int isc_dsql_prepare(
			[In, Out] int[] statusVector,
			ref	int trHandle,
			ref	int stmtHandle,
			short length,
			byte[] statement,
			short dialect,
			IntPtr xsqlda);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_dsql_execute")]
		public static extern int isc_dsql_execute(
			[In, Out] int[] statusVector,
			ref	int trHandle,
			ref	int stmtHandle,
			short daVersion,
			IntPtr xsqlda);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_dsql_execute2")]
		public static extern int isc_dsql_execute2(
			[In, Out] int[] statusVector,
			ref	int trHandle,
			ref	int stmtHandle,
			short da_version,
			IntPtr inXsqlda,
			IntPtr outXsqlda);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_dsql_fetch")]
		public static extern int isc_dsql_fetch(
			[In, Out] int[] statusVector,
			ref	int stmtHandle,
			short daVersion,
			IntPtr xsqlda);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_dsql_free_statement")]
		public static extern int isc_dsql_free_statement(
			[In, Out] int[] statusVector,
			ref	int stmtHandle,
			short option);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_dsql_sql_info")]
		public static extern int isc_dsql_sql_info(
			[In, Out] int[] statusVector,
			ref	int stmtHandle,
			short itemsLength,
			byte[] items,
			short bufferLength,
			byte[] buffer);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_vax_integer")]
		public static extern int isc_vax_integer(
			byte[] buffer,
			short length);

		#endregion

		#region Services Functions

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_service_attach")]
		public static extern int isc_service_attach(
			[In, Out] int[] statusVector,
			short serviceLength,
			string service,
			ref	int svcHandle,
			short spbLength,
			byte[] spb);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_service_start")]
		public static extern int isc_service_start(
			[In, Out] int[] statusVector,
			ref	int svcHandle,
			ref	int reserved,
			short spbLength,
			byte[] spb);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_service_detach")]
		public static extern int isc_service_detach(
			[In, Out] int[] statusVector,
			ref	int svcHandle);

		[DllImport(SafeNativeMethods.DllPath, EntryPoint = "ext_service_query")]
		public static extern int isc_service_query(
			[In, Out] int[] statusVector,
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
