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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

// This file was originally ported from Jaybird

using System;

namespace FirebirdSql.Data.Common
{
	internal static class IscCodes
	{
		#region General

		public const int SQLDA_VERSION1 = 1;
		public const int SQL_DIALECT_V5 = 1;
		public const int SQL_DIALECT_V6_TRANSITION = 2;
		public const int SQL_DIALECT_V6 = 3;
		public const int SQL_DIALECT_CURRENT = SQL_DIALECT_V6;
		public const int DSQL_close = 1;
		public const int DSQL_drop = 2;
		public const int ARRAY_DESC_COLUMN_MAJOR = 1;   /* Set for FORTRAN */
		public const int ISC_STATUS_LENGTH = 20;
		public const ushort INVALID_OBJECT = 0xFFFF;

		#endregion

		#region Buffer sizes

		public const int BUFFER_SIZE_128 = 128;
		public const int BUFFER_SIZE_256 = 256;
		public const int BUFFER_SIZE_32K = 32768;
		public const int DEFAULT_MAX_BUFFER_SIZE = 8192;
		public const int ROWS_AFFECTED_BUFFER_SIZE = 34;
		public const int STATEMENT_TYPE_BUFFER_SIZE = 8;
		public const int PREPARE_INFO_BUFFER_SIZE = 32768;

		#endregion

		#region Protocol Codes

		public const int GenericAchitectureClient = 1;

		public const int CONNECT_VERSION2 = 2;
		public const int CONNECT_VERSION3 = 3;
		public const int PROTOCOL_VERSION3 = 3;
		public const int PROTOCOL_VERSION4 = 4;
		public const int PROTOCOL_VERSION5 = 5;
		public const int PROTOCOL_VERSION6 = 6;
		public const int PROTOCOL_VERSION7 = 7;
		public const int PROTOCOL_VERSION8 = 8;
		public const int PROTOCOL_VERSION9 = 9;
		public const int PROTOCOL_VERSION10 = 10;

		public const int FB_PROTOCOL_FLAG = 0x8000;
		public const int FB_PROTOCOL_MASK = ~FB_PROTOCOL_FLAG;

		public const int PROTOCOL_VERSION11 = (FB_PROTOCOL_FLAG | 11);
		public const int PROTOCOL_VERSION12 = (FB_PROTOCOL_FLAG | 12);
		public const int PROTOCOL_VERSION13 = (FB_PROTOCOL_FLAG | 13);

		public const int ptype_rpc = 2;
		public const int ptype_batch_send = 3;
		public const int ptype_out_of_band = 4;
		public const int ptype_lazy_send = 5;

		public const int pflag_compress = 0x100;

		#endregion

		#region Statement Flags

		public const int STMT_DEFER_EXECUTE = 4;

		#endregion

		#region Server Class

		public const int isc_info_db_class_classic_access = 13;
		public const int isc_info_db_class_server_access = 14;

		#endregion

		#region Operation Codes

		// Operation (packet) types
		public const int op_void = 0;   // Packet has been voided
		public const int op_connect = 1;    // Connect to remote server
		public const int op_exit = 2;   // Remote end has exitted
		public const int op_accept = 3; // Server accepts connection
		public const int op_reject = 4; // Server rejects connection
		public const int op_protocol = 5;   // Protocol	selection
		public const int op_disconnect = 6; // Connect is going	away
		public const int op_credit = 7; // Grant (buffer) credits
		public const int op_continuation = 8;   // Continuation	packet
		public const int op_response = 9;   // Generic response	block

		// Page	server operations
		public const int op_open_file = 10; // Open	file for page service
		public const int op_create_file = 11;   // Create file for page	service
		public const int op_close_file = 12;    // Close file for page service
		public const int op_read_page = 13; // optionally lock and read	page
		public const int op_write_page = 14;    // write page and optionally release lock
		public const int op_lock = 15;  // sieze lock
		public const int op_convert_lock = 16;  // convert existing	lock
		public const int op_release_lock = 17;  // release existing	lock
		public const int op_blocking = 18;  // blocking	lock message

		// Full	context	server operations
		public const int op_attach = 19;    // Attach database
		public const int op_create = 20;    // Create database
		public const int op_detach = 21;    // Detach database
		public const int op_compile = 22;   // Request based operations
		public const int op_start = 23;
		public const int op_start_and_send = 24;
		public const int op_send = 25;
		public const int op_receive = 26;
		public const int op_unwind = 27;
		public const int op_release = 28;

		public const int op_transaction = 29;   // Transaction operations
		public const int op_commit = 30;
		public const int op_rollback = 31;
		public const int op_prepare = 32;
		public const int op_reconnect = 33;

		public const int op_create_blob = 34;   // Blob	operations //
		public const int op_open_blob = 35;
		public const int op_get_segment = 36;
		public const int op_put_segment = 37;
		public const int op_cancel_blob = 38;
		public const int op_close_blob = 39;

		public const int op_info_database = 40; // Information services
		public const int op_info_request = 41;
		public const int op_info_transaction = 42;
		public const int op_info_blob = 43;

		public const int op_batch_segments = 44;    // Put a bunch of blob segments

		public const int op_mgr_set_affinity = 45;  // Establish server	affinity
		public const int op_mgr_clear_affinity = 46;    // Break server	affinity
		public const int op_mgr_report = 47;    // Report on server

		public const int op_que_events = 48;    // Que event notification request
		public const int op_cancel_events = 49; // Cancel event	notification request
		public const int op_commit_retaining = 50;  // Commit retaining	(what else)
		public const int op_prepare2 = 51;  // Message form	of prepare
		public const int op_event = 52; // Completed event request (asynchronous)
		public const int op_connect_request = 53;   // Request to establish	connection
		public const int op_aux_connect = 54;   // Establish auxiliary connection
		public const int op_ddl = 55;   // DDL call
		public const int op_open_blob2 = 56;
		public const int op_create_blob2 = 57;
		public const int op_get_slice = 58;
		public const int op_put_slice = 59;
		public const int op_slice = 60; // Successful response to public const int op_get_slice
		public const int op_seek_blob = 61; // Blob	seek operation

		// DSQL	operations //
		public const int op_allocate_statement = 62;    // allocate	a statment handle
		public const int op_execute = 63;   // execute a prepared statement
		public const int op_exec_immediate = 64;    // execute a statement
		public const int op_fetch = 65; // fetch a record
		public const int op_fetch_response = 66;    // response	for	record fetch
		public const int op_free_statement = 67;    // free	a statement
		public const int op_prepare_statement = 68; // prepare a statement
		public const int op_set_cursor = 69;    // set a cursor	name
		public const int op_info_sql = 70;
		public const int op_dummy = 71; // dummy packet	to detect loss of client
		public const int op_response_piggyback = 72;    // response	block for piggybacked messages
		public const int op_start_and_receive = 73;
		public const int op_start_send_and_receive = 74;

		public const int op_exec_immediate2 = 75;   // execute an immediate	statement with msgs
		public const int op_execute2 = 76;  // execute a statement with	msgs
		public const int op_insert = 77;
		public const int op_sql_response = 78;  // response	from execute; exec immed; insert
		public const int op_transact = 79;
		public const int op_transact_response = 80;
		public const int op_drop_database = 81;
		public const int op_service_attach = 82;
		public const int op_service_detach = 83;
		public const int op_service_info = 84;
		public const int op_service_start = 85;
		public const int op_rollback_retaining = 86;

		// Two following opcode are used in vulcan.
		// No plans to implement them completely for a while, but to
		// support protocol 11, where they are used, have them here.
		public const int op_update_account_info = 87;
		public const int op_authenticate_user = 88;

		public const int op_partial = 89;   // packet is not complete - delay processing
		public const int op_trusted_auth = 90;
		public const int op_cancel = 91;
		public const int op_cont_auth = 92;
		public const int op_ping = 93;
		public const int op_accept_data = 94;
		public const int op_abort_aux_connection = 95;
		public const int op_crypt = 96;
		public const int op_crypt_key_callback = 97;
		public const int op_cond_accept = 98;

		#endregion

		#region Database Parameter Block

		public const int isc_dpb_version1 = 1;
		public const int isc_dpb_cdd_pathname = 1;
		public const int isc_dpb_allocation = 2;
		public const int isc_dpb_journal = 3;
		public const int isc_dpb_page_size = 4;
		public const int isc_dpb_num_buffers = 5;
		public const int isc_dpb_buffer_length = 6;
		public const int isc_dpb_debug = 7;
		public const int isc_dpb_garbage_collect = 8;
		public const int isc_dpb_verify = 9;
		public const int isc_dpb_sweep = 10;
		public const int isc_dpb_enable_journal = 11;
		public const int isc_dpb_disable_journal = 12;
		public const int isc_dpb_dbkey_scope = 13;
		public const int isc_dpb_number_of_users = 14;
		public const int isc_dpb_trace = 15;
		public const int isc_dpb_no_garbage_collect = 16;
		public const int isc_dpb_damaged = 17;
		public const int isc_dpb_license = 18;
		public const int isc_dpb_sys_user_name = 19;
		public const int isc_dpb_encrypt_key = 20;
		public const int isc_dpb_activate_shadow = 21;
		public const int isc_dpb_sweep_interval = 22;
		public const int isc_dpb_delete_shadow = 23;
		public const int isc_dpb_force_write = 24;
		public const int isc_dpb_begin_log = 25;
		public const int isc_dpb_quit_log = 26;
		public const int isc_dpb_no_reserve = 27;
		public const int isc_dpb_user_name = 28;
		public const int isc_dpb_password = 29;
		public const int isc_dpb_password_enc = 30;
		public const int isc_dpb_sys_user_name_enc = 31;
		public const int isc_dpb_interp = 32;
		public const int isc_dpb_online_dump = 33;
		public const int isc_dpb_old_file_size = 34;
		public const int isc_dpb_old_num_files = 35;
		public const int isc_dpb_old_file = 36;
		public const int isc_dpb_old_start_page = 37;
		public const int isc_dpb_old_start_seqno = 38;
		public const int isc_dpb_old_start_file = 39;
		public const int isc_dpb_drop_walfile = 40;
		public const int isc_dpb_old_dump_id = 41;
		public const int isc_dpb_lc_messages = 47;
		public const int isc_dpb_lc_ctype = 48;
		public const int isc_dpb_cache_manager = 49;
		public const int isc_dpb_shutdown = 50;
		public const int isc_dpb_online = 51;
		public const int isc_dpb_shutdown_delay = 52;
		public const int isc_dpb_reserved = 53;
		public const int isc_dpb_overwrite = 54;
		public const int isc_dpb_sec_attach = 55;
		public const int isc_dpb_connect_timeout = 57;
		public const int isc_dpb_dummy_packet_interval = 58;
		public const int isc_dpb_gbak_attach = 59;
		public const int isc_dpb_sql_role_name = 60;
		public const int isc_dpb_set_page_buffers = 61;
		public const int isc_dpb_working_directory = 62;
		public const int isc_dpb_sql_dialect = 63;
		public const int isc_dpb_set_db_readonly = 64;
		public const int isc_dpb_set_db_sql_dialect = 65;
		public const int isc_dpb_gfix_attach = 66;
		public const int isc_dpb_gstat_attach = 67;
		public const int isc_dpb_set_db_charset = 68;
		public const int isc_dpb_process_id = 71;
		public const int isc_dpb_no_db_triggers = 72;
		public const int isc_dpb_trusted_auth = 73;
		public const int isc_dpb_process_name = 74;
		public const int isc_dpb_utf8_filename = 77;
		public const int isc_dpb_client_version = 80;
		public const int isc_dpb_specific_auth_data = 84;

		#endregion

		#region Transaction Parameter Block

		public const int isc_tpb_version1 = 1;
		public const int isc_tpb_version3 = 3;
		public const int isc_tpb_consistency = 1;
		public const int isc_tpb_concurrency = 2;
		public const int isc_tpb_shared = 3;
		public const int isc_tpb_protected = 4;
		public const int isc_tpb_exclusive = 5;
		public const int isc_tpb_wait = 6;
		public const int isc_tpb_nowait = 7;
		public const int isc_tpb_read = 8;
		public const int isc_tpb_write = 9;
		public const int isc_tpb_lock_read = 10;
		public const int isc_tpb_lock_write = 11;
		public const int isc_tpb_verb_time = 12;
		public const int isc_tpb_commit_time = 13;
		public const int isc_tpb_ignore_limbo = 14;
		public const int isc_tpb_read_committed = 15;
		public const int isc_tpb_autocommit = 16;
		public const int isc_tpb_rec_version = 17;
		public const int isc_tpb_no_rec_version = 18;
		public const int isc_tpb_restart_requests = 19;
		public const int isc_tpb_no_auto_undo = 20;
		public const int isc_tpb_lock_timeout = 21;

		#endregion

		#region Services Parameter Block

		public const int isc_spb_version1 = 1;
		public const int isc_spb_current_version = 2;
		public const int isc_spb_version = isc_spb_current_version;
		public const int isc_spb_user_name = isc_dpb_user_name;
		public const int isc_spb_sys_user_name = isc_dpb_sys_user_name;
		public const int isc_spb_sys_user_name_enc = isc_dpb_sys_user_name_enc;
		public const int isc_spb_password = isc_dpb_password;
		public const int isc_spb_password_enc = isc_dpb_password_enc;
		public const int isc_spb_command_line = 105;
		public const int isc_spb_dbname = 106;
		public const int isc_spb_verbose = 107;
		public const int isc_spb_options = 108;
		public const int isc_spb_trusted_auth = 111;

		public const int isc_spb_connect_timeout = isc_dpb_connect_timeout;
		public const int isc_spb_dummy_packet_interval = isc_dpb_dummy_packet_interval;
		public const int isc_spb_sql_role_name = isc_dpb_sql_role_name;

		public const int isc_spb_specific_auth_data = isc_spb_trusted_auth;

		public const int isc_spb_num_att = 5;
		public const int isc_spb_num_db = 6;

		#endregion

		#region Services Actions

		public const int isc_action_svc_backup = 1; /* Starts database backup process on the server	*/
		public const int isc_action_svc_restore = 2;    /* Starts database restore process on the server */
		public const int isc_action_svc_repair = 3; /* Starts database repair process on the server	*/
		public const int isc_action_svc_add_user = 4;   /* Adds	a new user to the security database	*/
		public const int isc_action_svc_delete_user = 5;    /* Deletes a user record from the security database	*/
		public const int isc_action_svc_modify_user = 6;    /* Modifies	a user record in the security database */
		public const int isc_action_svc_display_user = 7;   /* Displays	a user record from the security	database */
		public const int isc_action_svc_properties = 8; /* Sets	database properties	*/
		public const int isc_action_svc_add_license = 9;    /* Adds	a license to the license file */
		public const int isc_action_svc_remove_license = 10;    /* Removes a license from the license file */
		public const int isc_action_svc_db_stats = 11;  /* Retrieves database statistics */
		public const int isc_action_svc_get_ib_log = 12;    /* Retrieves the InterBase log file	from the server	*/
		public const int isc_action_svc_nbak = 20;  /* Incremental nbackup */
		public const int isc_action_svc_nrest = 21; /* Incremental database restore */
		public const int isc_action_svc_trace_start = 22;   // Start trace session
		public const int isc_action_svc_trace_stop = 23;    // Stop trace session
		public const int isc_action_svc_trace_suspend = 24; // Suspend trace session
		public const int isc_action_svc_trace_resume = 25;  // Resume trace session
		public const int isc_action_svc_trace_list = 26;    // List existing sessions
		public const int isc_action_svc_set_mapping = 27;   // Set auto admins mapping in security database
		public const int isc_action_svc_drop_mapping = 28;  // Drop auto admins mapping in security database
		public const int isc_action_svc_display_user_adm = 29;  // Displays user(s) from security database with admin info

		#endregion

		#region Services Information

		public const int isc_info_svc_svr_db_info = 50; /* Retrieves the number	of attachments and databases */
		public const int isc_info_svc_get_license = 51; /* Retrieves all license keys and IDs from the license file	*/
		public const int isc_info_svc_get_license_mask = 52;    /* Retrieves a bitmask representing	licensed options on	the	server */
		public const int isc_info_svc_get_config = 53;  /* Retrieves the parameters	and	values for IB_CONFIG */
		public const int isc_info_svc_version = 54; /* Retrieves the version of	the	services manager */
		public const int isc_info_svc_server_version = 55;  /* Retrieves the version of	the	InterBase server */
		public const int isc_info_svc_implementation = 56;  /* Retrieves the implementation	of the InterBase server	*/
		public const int isc_info_svc_capabilities = 57;    /* Retrieves a bitmask representing	the	server's capabilities */
		public const int isc_info_svc_user_dbpath = 58; /* Retrieves the path to the security database in use by the server	*/
		public const int isc_info_svc_get_env = 59; /* Retrieves the setting of	$INTERBASE */
		public const int isc_info_svc_get_env_lock = 60;    /* Retrieves the setting of	$INTERBASE_LCK */
		public const int isc_info_svc_get_env_msg = 61; /* Retrieves the setting of	$INTERBASE_MSG */
		public const int isc_info_svc_line = 62;    /* Retrieves 1 line	of service output per call */
		public const int isc_info_svc_to_eof = 63;  /* Retrieves as much of	the	server output as will fit in the supplied buffer */
		public const int isc_info_svc_timeout = 64; /* Sets	/ signifies	a timeout value	for	reading	service	information	*/
		public const int isc_info_svc_get_licensed_users = 65;  /* Retrieves the number	of users licensed for accessing	the	server */
		public const int isc_info_svc_limbo_trans = 66; /* Retrieve	the	limbo transactions */
		public const int isc_info_svc_running = 67; /* Checks to see if	a service is running on	an attachment */
		public const int isc_info_svc_get_users = 68;   /* Returns the user	information	from isc_action_svc_display_users */
		public const int isc_info_svc_stdin = 78;   /* Returns size of data, needed as stdin for service */

		#endregion

		#region Services Properties

		public const int isc_spb_prp_page_buffers = 5;
		public const int isc_spb_prp_sweep_interval = 6;
		public const int isc_spb_prp_shutdown_db = 7;
		public const int isc_spb_prp_deny_new_attachments = 9;
		public const int isc_spb_prp_deny_new_transactions = 10;
		public const int isc_spb_prp_reserve_space = 11;
		public const int isc_spb_prp_write_mode = 12;
		public const int isc_spb_prp_access_mode = 13;
		public const int isc_spb_prp_set_sql_dialect = 14;

		public const int isc_spb_prp_force_shutdown = 41;
		public const int isc_spb_prp_attachments_shutdown = 42;
		public const int isc_spb_prp_transactions_shutdown = 43;
		public const int isc_spb_prp_shutdown_mode = 44;
		public const int isc_spb_prp_online_mode = 45;

		public const int isc_spb_prp_sm_normal = 0;
		public const int isc_spb_prp_sm_multi = 1;
		public const int isc_spb_prp_sm_single = 2;
		public const int isc_spb_prp_sm_full = 3;

		// RESERVE_SPACE_PARAMETERS
		public const int isc_spb_prp_res_use_full = 35;
		public const int isc_spb_prp_res = 36;

		// WRITE_MODE_PARAMETERS
		public const int isc_spb_prp_wm_async = 37;
		public const int isc_spb_prp_wm_sync = 38;

		// ACCESS_MODE_PARAMETERS
		public const int isc_spb_prp_am_readonly = 39;
		public const int isc_spb_prp_am_readwrite = 40;

		// Option Flags
		public const int isc_spb_prp_activate = 0x0100;
		public const int isc_spb_prp_db_online = 0x0200;
		public const int isc_spb_prp_nolinger = 0x0400;

		#endregion

		#region Backup Service

		public const int isc_spb_bkp_file = 5;
		public const int isc_spb_bkp_factor = 6;
		public const int isc_spb_bkp_length = 7;
		public const int isc_spb_bkp_skip_data = 8;

		public const int isc_spb_bkp_ignore_checksums = 0x01;
		public const int isc_spb_bkp_ignore_limbo = 0x02;
		public const int isc_spb_bkp_metadata_only = 0x04;
		public const int isc_spb_bkp_no_garbage_collect = 0x08;
		public const int isc_spb_bkp_old_descriptions = 0x10;
		public const int isc_spb_bkp_non_transportable = 0x20;
		public const int isc_spb_bkp_convert = 0x40;
		public const int isc_spb_bkp_expand = 0x80;
		public const int isc_spb_bkp_no_triggers = 0x8000;

		#endregion

		#region Restore Service

		public const int isc_spb_res_skip_data = isc_spb_bkp_skip_data;
		public const int isc_spb_res_buffers = 9;
		public const int isc_spb_res_page_size = 10;
		public const int isc_spb_res_length = 11;
		public const int isc_spb_res_access_mode = 12;

		public const int isc_spb_res_metadata_only = isc_spb_bkp_metadata_only;
		public const int isc_spb_res_deactivate_idx = 0x0100;
		public const int isc_spb_res_no_shadow = 0x0200;
		public const int isc_spb_res_no_validity = 0x0400;
		public const int isc_spb_res_one_at_a_time = 0x0800;
		public const int isc_spb_res_replace = 0x1000;
		public const int isc_spb_res_create = 0x2000;
		public const int isc_spb_res_use_all_space = 0x4000;

		public const int isc_spb_res_am_readonly = isc_spb_prp_am_readonly;
		public const int isc_spb_res_am_readwrite = isc_spb_prp_am_readwrite;

		#endregion

		#region Repair Service

		public const int isc_spb_rpr_commit_trans = 15;
		public const int isc_spb_rpr_rollback_trans = 34;
		public const int isc_spb_rpr_recover_two_phase = 17;
		public const int isc_spb_tra_id = 18;
		public const int isc_spb_single_tra_id = 19;
		public const int isc_spb_multi_tra_id = 20;
		public const int isc_spb_tra_state = 21;
		public const int isc_spb_tra_state_limbo = 22;
		public const int isc_spb_tra_state_commit = 23;
		public const int isc_spb_tra_state_rollback = 24;
		public const int isc_spb_tra_state_unknown = 25;
		public const int isc_spb_tra_host_site = 26;
		public const int isc_spb_tra_remote_site = 27;
		public const int isc_spb_tra_db_path = 28;
		public const int isc_spb_tra_advise = 29;
		public const int isc_spb_tra_advise_commit = 30;
		public const int isc_spb_tra_advise_rollback = 31;
		public const int isc_spb_tra_advise_unknown = 33;

		#endregion

		#region Security Service

		public const int isc_spb_sec_userid = 5;
		public const int isc_spb_sec_groupid = 6;
		public const int isc_spb_sec_username = 7;
		public const int isc_spb_sec_password = 8;
		public const int isc_spb_sec_groupname = 9;
		public const int isc_spb_sec_firstname = 10;
		public const int isc_spb_sec_middlename = 11;
		public const int isc_spb_sec_lastname = 12;

		#endregion

		#region NBackup Service
		public const int isc_spb_nbk_level = 5;
		public const int isc_spb_nbk_file = 6;
		public const int isc_spb_nbk_direct = 7;
		public const int isc_spb_nbk_no_triggers = 0x01;
		#endregion

		#region Trace Service

		public const int isc_spb_trc_id = 1;
		public const int isc_spb_trc_name = 2;
		public const int isc_spb_trc_cfg = 3;

		#endregion

		#region Configuration Keys

		public const int ISCCFG_LOCKMEM_KEY = 0;
		public const int ISCCFG_LOCKSEM_KEY = 1;
		public const int ISCCFG_LOCKSIG_KEY = 2;
		public const int ISCCFG_EVNTMEM_KEY = 3;
		public const int ISCCFG_DBCACHE_KEY = 4;
		public const int ISCCFG_PRIORITY_KEY = 5;
		public const int ISCCFG_IPCMAP_KEY = 6;
		public const int ISCCFG_MEMMIN_KEY = 7;
		public const int ISCCFG_MEMMAX_KEY = 8;
		public const int ISCCFG_LOCKORDER_KEY = 9;
		public const int ISCCFG_ANYLOCKMEM_KEY = 10;
		public const int ISCCFG_ANYLOCKSEM_KEY = 11;
		public const int ISCCFG_ANYLOCKSIG_KEY = 12;
		public const int ISCCFG_ANYEVNTMEM_KEY = 13;
		public const int ISCCFG_LOCKHASH_KEY = 14;
		public const int ISCCFG_DEADLOCK_KEY = 15;
		public const int ISCCFG_LOCKSPIN_KEY = 16;
		public const int ISCCFG_CONN_TIMEOUT_KEY = 17;
		public const int ISCCFG_DUMMY_INTRVL_KEY = 18;
		public const int ISCCFG_TRACE_POOLS_KEY = 19; /* Internal Use only	*/
		public const int ISCCFG_REMOTE_BUFFER_KEY = 20;

		#endregion

		#region Common Structural Codes

		public const int isc_info_end = 1;
		public const int isc_info_truncated = 2;
		public const int isc_info_error = 3;
		public const int isc_info_data_not_ready = 4;
		public const int isc_info_flag_end = 127;

		#endregion

		#region SQL Information

		public const int isc_info_sql_select = 4;
		public const int isc_info_sql_bind = 5;
		public const int isc_info_sql_num_variables = 6;
		public const int isc_info_sql_describe_vars = 7;
		public const int isc_info_sql_describe_end = 8;
		public const int isc_info_sql_sqlda_seq = 9;
		public const int isc_info_sql_message_seq = 10;
		public const int isc_info_sql_type = 11;
		public const int isc_info_sql_sub_type = 12;
		public const int isc_info_sql_scale = 13;
		public const int isc_info_sql_length = 14;
		public const int isc_info_sql_null_ind = 15;
		public const int isc_info_sql_field = 16;
		public const int isc_info_sql_relation = 17;
		public const int isc_info_sql_owner = 18;
		public const int isc_info_sql_alias = 19;
		public const int isc_info_sql_sqlda_start = 20;
		public const int isc_info_sql_stmt_type = 21;
		public const int isc_info_sql_get_plan = 22;
		public const int isc_info_sql_records = 23;
		public const int isc_info_sql_batch_fetch = 24;
		public const int isc_info_sql_relation_alias = 25;

		#endregion

		#region SQL Information Return Values

		public const int isc_info_sql_stmt_select = 1;
		public const int isc_info_sql_stmt_insert = 2;
		public const int isc_info_sql_stmt_update = 3;
		public const int isc_info_sql_stmt_delete = 4;
		public const int isc_info_sql_stmt_ddl = 5;
		public const int isc_info_sql_stmt_get_segment = 6;
		public const int isc_info_sql_stmt_put_segment = 7;
		public const int isc_info_sql_stmt_exec_procedure = 8;
		public const int isc_info_sql_stmt_start_trans = 9;
		public const int isc_info_sql_stmt_commit = 10;
		public const int isc_info_sql_stmt_rollback = 11;
		public const int isc_info_sql_stmt_select_for_upd = 12;
		public const int isc_info_sql_stmt_set_generator = 13;
		public const int isc_info_sql_stmt_savepoint = 14;

		#endregion

		#region Database Information

		public const int isc_info_db_id = 4;
		public const int isc_info_reads = 5;
		public const int isc_info_writes = 6;
		public const int isc_info_fetches = 7;
		public const int isc_info_marks = 8;

		public const int isc_info_implementation = 11;
		public const int isc_info_isc_version = 12;
		public const int isc_info_base_level = 13;
		public const int isc_info_page_size = 14;
		public const int isc_info_num_buffers = 15;
		public const int isc_info_limbo = 16;
		public const int isc_info_current_memory = 17;
		public const int isc_info_max_memory = 18;
		public const int isc_info_window_turns = 19;
		public const int isc_info_license = 20;

		public const int isc_info_allocation = 21;
		public const int isc_info_attachment_id = 22;
		public const int isc_info_read_seq_count = 23;
		public const int isc_info_read_idx_count = 24;
		public const int isc_info_insert_count = 25;
		public const int isc_info_update_count = 26;
		public const int isc_info_delete_count = 27;
		public const int isc_info_backout_count = 28;
		public const int isc_info_purge_count = 29;
		public const int isc_info_expunge_count = 30;

		public const int isc_info_sweep_interval = 31;
		public const int isc_info_ods_version = 32;
		public const int isc_info_ods_minor_version = 33;
		public const int isc_info_no_reserve = 34;
		public const int isc_info_logfile = 35;
		public const int isc_info_cur_logfile_name = 36;
		public const int isc_info_cur_log_part_offset = 37;
		public const int isc_info_num_wal_buffers = 38;
		public const int isc_info_wal_buffer_size = 39;
		public const int isc_info_wal_ckpt_length = 40;

		public const int isc_info_wal_cur_ckpt_interval = 41;
		public const int isc_info_wal_prv_ckpt_fname = 42;
		public const int isc_info_wal_prv_ckpt_poffset = 43;
		public const int isc_info_wal_recv_ckpt_fname = 44;
		public const int isc_info_wal_recv_ckpt_poffset = 45;
		public const int isc_info_wal_grpc_wait_usecs = 47;
		public const int isc_info_wal_num_io = 48;
		public const int isc_info_wal_avg_io_size = 49;
		public const int isc_info_wal_num_commits = 50;

		public const int isc_info_wal_avg_grpc_size = 51;
		public const int isc_info_forced_writes = 52;
		public const int isc_info_user_names = 53;
		public const int isc_info_page_errors = 54;
		public const int isc_info_record_errors = 55;
		public const int isc_info_bpage_errors = 56;
		public const int isc_info_dpage_errors = 57;
		public const int isc_info_ipage_errors = 58;
		public const int isc_info_ppage_errors = 59;
		public const int isc_info_tpage_errors = 60;

		public const int isc_info_set_page_buffers = 61;
		public const int isc_info_db_sql_dialect = 62;
		public const int isc_info_db_read_only = 63;
		public const int isc_info_db_size_in_pages = 64;

		/* Values 65 -100 unused to	avoid conflict with	InterBase */

		public const int frb_info_att_charset = 101;
		public const int isc_info_db_class = 102;
		public const int isc_info_firebird_version = 103;
		public const int isc_info_oldest_transaction = 104;
		public const int isc_info_oldest_active = 105;
		public const int isc_info_oldest_snapshot = 106;
		public const int isc_info_next_transaction = 107;
		public const int isc_info_db_provider = 108;
		public const int isc_info_active_transactions = 109;

		#endregion

		#region Information Request

		public const int isc_info_number_messages = 4;
		public const int isc_info_max_message = 5;
		public const int isc_info_max_send = 6;
		public const int isc_info_max_receive = 7;
		public const int isc_info_state = 8;
		public const int isc_info_message_number = 9;
		public const int isc_info_message_size = 10;
		public const int isc_info_request_cost = 11;
		public const int isc_info_access_path = 12;
		public const int isc_info_req_select_count = 13;
		public const int isc_info_req_insert_count = 14;
		public const int isc_info_req_update_count = 15;
		public const int isc_info_req_delete_count = 16;

		#endregion

		#region Array Slice Description Language

		public const int isc_sdl_version1 = 1;
		public const int isc_sdl_eoc = 255;
		public const int isc_sdl_relation = 2;
		public const int isc_sdl_rid = 3;
		public const int isc_sdl_field = 4;
		public const int isc_sdl_fid = 5;
		public const int isc_sdl_struct = 6;
		public const int isc_sdl_variable = 7;
		public const int isc_sdl_scalar = 8;
		public const int isc_sdl_tiny_integer = 9;
		public const int isc_sdl_short_integer = 10;
		public const int isc_sdl_long_integer = 11;
		public const int isc_sdl_literal = 12;
		public const int isc_sdl_add = 13;
		public const int isc_sdl_subtract = 14;
		public const int isc_sdl_multiply = 15;
		public const int isc_sdl_divide = 16;
		public const int isc_sdl_negate = 17;
		public const int isc_sdl_eql = 18;
		public const int isc_sdl_neq = 19;
		public const int isc_sdl_gtr = 20;
		public const int isc_sdl_geq = 21;
		public const int isc_sdl_lss = 22;
		public const int isc_sdl_leq = 23;
		public const int isc_sdl_and = 24;
		public const int isc_sdl_or = 25;
		public const int isc_sdl_not = 26;
		public const int isc_sdl_while = 27;
		public const int isc_sdl_assignment = 28;
		public const int isc_sdl_label = 29;
		public const int isc_sdl_leave = 30;
		public const int isc_sdl_begin = 31;
		public const int isc_sdl_end = 32;
		public const int isc_sdl_do3 = 33;
		public const int isc_sdl_do2 = 34;
		public const int isc_sdl_do1 = 35;
		public const int isc_sdl_element = 36;

		#endregion

		#region Blob Parameter Block

		public const int isc_bpb_version1 = 1;
		public const int isc_bpb_source_type = 1;
		public const int isc_bpb_target_type = 2;
		public const int isc_bpb_type = 3;
		public const int isc_bpb_source_interp = 4;
		public const int isc_bpb_target_interp = 5;
		public const int isc_bpb_filter_parameter = 6;

		public const int isc_bpb_type_segmented = 0;
		public const int isc_bpb_type_stream = 1;

		public const int RBL_eof = 1;
		public const int RBL_segment = 2;
		public const int RBL_eof_pending = 4;
		public const int RBL_create = 8;

		#endregion

		#region Blob Information

		public const int isc_info_blob_num_segments = 4;
		public const int isc_info_blob_max_segment = 5;
		public const int isc_info_blob_total_length = 6;
		public const int isc_info_blob_type = 7;

		#endregion

		#region Event Codes

		public const int P_REQ_async = 1;   // Auxiliary asynchronous port
		public const int EPB_version1 = 1;

		#endregion

		#region Facilities

		public const int JRD = 0;
		public const int GFIX = 3;
		public const int DSQL = 7;
		public const int DYN = 8;
		public const int GBAK = 12;
		public const int GDEC = 18;
		public const int LICENSE = 19;
		public const int GSTAT = 21;

		#endregion

		#region Error code generation

		public const int ISC_MASK = 0x14000000; // Defines the code	as a valid ISC code

		#endregion

		#region ISC Error codes

		public const int isc_facility = 20;
		public const int isc_err_base = 335544320;
		public const int isc_err_factor = 1;
		public const int isc_arg_end = 0;    // end of argument list
		public const int isc_arg_gds = 1;    // generic DSRI	status value
		public const int isc_arg_string = 2;    // string argument
		public const int isc_arg_cstring = 3;   // count & string argument
		public const int isc_arg_number = 4;    // numeric argument	(long)
		public const int isc_arg_interpreted = 5;   // interpreted status code (string)
		public const int isc_arg_vms = 6;   // VAX/VMS status code (long)
		public const int isc_arg_unix = 7;  // UNIX	error code
		public const int isc_arg_domain = 8;    // Apollo/Domain error code
		public const int isc_arg_dos = 9;   // MSDOS/OS2 error code
		public const int isc_arg_mpexl = 10;    // HP MPE/XL error code
		public const int isc_arg_mpexl_ipc = 11;    // HP MPE/XL IPC error code
		public const int isc_arg_next_mach = 15;    // NeXT/Mach error code
		public const int isc_arg_netware = 16;  // NetWare error code
		public const int isc_arg_win32 = 17;    // Win32 error code
		public const int isc_arg_warning = 18;  // warning argument
		public const int isc_arg_sql_state = 19;    // SQLSTATE

		public const int isc_open_trans = 335544357;
		public const int isc_segment = 335544366;
		public const int isc_segstr_eof = 335544367;
		public const int isc_connect_reject = 335544421;
		public const int isc_invalid_dimension = 335544458;
		public const int isc_tra_state = 335544468;
		public const int isc_except = 335544517;
		public const int isc_dsql_sqlda_err = 335544583;
		public const int isc_network_error = 335544721;
		public const int isc_net_read_err = 335544726;
		public const int isc_net_write_err = 335544727;
		public const int isc_stack_trace = 335544842;
		public const int isc_except2 = 335544848;
		public const int isc_arith_except = 335544321;
		public const int isc_string_truncation = 335544914;
		public const int isc_formatted_exception = 335545016;

		#endregion

		#region BLR Codes

		public const int blr_version5 = 5;
		public const int blr_begin = 2;
		public const int blr_message = 4;
		public const int blr_eoc = 76;
		public const int blr_end = 255;

		public const int blr_text = 14;
		public const int blr_text2 = 15;
		public const int blr_short = 7;
		public const int blr_long = 8;
		public const int blr_quad = 9;
		public const int blr_int64 = 16;
		public const int blr_float = 10;
		public const int blr_double = 27;
		public const int blr_d_float = 11;
		public const int blr_timestamp = 35;
		public const int blr_varying = 37;
		public const int blr_varying2 = 38;
		public const int blr_blob = 261;
		public const int blr_cstring = 40;
		public const int blr_cstring2 = 41;
		public const int blr_blob_id = 45;
		public const int blr_sql_date = 12;
		public const int blr_sql_time = 13;
		public const int blr_bool = 23;

		public const int blr_null = 45;

		#endregion

		#region DataType Definitions

		public const int SQL_TEXT = 452;
		public const int SQL_VARYING = 448;
		public const int SQL_SHORT = 500;
		public const int SQL_LONG = 496;
		public const int SQL_FLOAT = 482;
		public const int SQL_DOUBLE = 480;
		public const int SQL_D_FLOAT = 530;
		public const int SQL_TIMESTAMP = 510;
		public const int SQL_BLOB = 520;
		public const int SQL_ARRAY = 540;
		public const int SQL_QUAD = 550;
		public const int SQL_TYPE_TIME = 560;
		public const int SQL_TYPE_DATE = 570;
		public const int SQL_INT64 = 580;
		public const int SQL_BOOLEAN = 32764;
		public const int SQL_NULL = 32766;

		// Historical alias	for	pre	V6 applications
		public const int SQL_DATE = SQL_TIMESTAMP;

		#endregion

		#region Cancel types
		public const int fb_cancel_disable = 1;
		public const int fb_cancel_enable = 2;
		public const int fb_cancel_raise = 3;
		public const int fb_cancel_abort = 4;
		#endregion

		#region User identification data
		public const int CNCT_user = 1;
		public const int CNCT_passwd = 2;
		public const int CNCT_host = 4;
		public const int CNCT_group = 5;
		public const int CNCT_user_verification = 6;
		public const int CNCT_specific_data = 7;
		public const int CNCT_plugin_name = 8;
		public const int CNCT_login = 9;
		public const int CNCT_plugin_list = 10;
		public const int CNCT_client_crypt = 11;
		#endregion
	}
}
