/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using System.Collections.Generic;

namespace FirebirdSql.Data.Common;

internal static class IscErrorMessages
{
	static Dictionary<int, string> _messages = new Dictionary<int, string>()
		{
	{335544320, ""},
	{335544321, "arithmetic exception, numeric overflow, or string truncation"},		/* arith_except */
	{335544322, "invalid database key"},		/* bad_dbkey */
	{335544323, "file {0} is not a valid database"},		/* bad_db_format */
	{335544324, "invalid database handle (no active connection)"},		/* bad_db_handle */
	{335544325, "bad parameters on attach or create database"},		/* bad_dpb_content */
	{335544326, "unrecognized database parameter block"},		/* bad_dpb_form */
	{335544327, "invalid request handle"},		/* bad_req_handle */
	{335544328, "invalid BLOB handle"},		/* bad_segstr_handle */
	{335544329, "invalid BLOB ID"},		/* bad_segstr_id */
	{335544330, "invalid parameter in transaction parameter block"},		/* bad_tpb_content */
	{335544331, "invalid format for transaction parameter block"},		/* bad_tpb_form */
	{335544332, "invalid transaction handle (expecting explicit transaction start)"},		/* bad_trans_handle */
	{335544333, "internal Firebird consistency check ({0})"},		/* bug_check */
	{335544334, "conversion error from string \"{0}\""},		/* convert_error */
	{335544335, "database file appears corrupt ({0})"},		/* db_corrupt */
	{335544336, "deadlock"},		/* deadlock */
	{335544337, "attempt to start more than {0} transactions"},		/* excess_trans */
	{335544338, "no match for first value expression"},		/* from_no_match */
	{335544339, "information type inappropriate for object specified"},		/* infinap */
	{335544340, "no information of this type available for object specified"},		/* infona */
	{335544341, "unknown information item"},		/* infunk */
	{335544342, "action cancelled by trigger ({0}) to preserve data integrity"},		/* integ_fail */
	{335544343, "invalid request BLR at offset {0}"},		/* invalid_blr */
	{335544344, "I/O error during \"{0}\" operation for file \"{1}\""},		/* io_error */
	{335544345, "lock conflict on no wait transaction"},		/* lock_conflict */
	{335544346, "corrupt system table"},		/* metadata_corrupt */
	{335544347, "validation error for column {0}, value \"{1}\""},		/* not_valid */
	{335544348, "no current record for fetch operation"},		/* no_cur_rec */
	{335544349, "attempt to store duplicate value (visible to active transactions) in unique index \"{0}\""},		/* no_dup */
	{335544350, "program attempted to exit without finishing database"},		/* no_finish */
	{335544351, "unsuccessful metadata update"},		/* no_meta_update */
	{335544352, "no permission for {0} access to {1} {2}"},		/* no_priv */
	{335544353, "transaction is not in limbo"},		/* no_recon */
	{335544354, "invalid database key"},		/* no_record */
	{335544355, "BLOB was not closed"},		/* no_segstr_close */
	{335544356, "metadata is obsolete"},		/* obsolete_metadata */
	{335544357, "cannot disconnect database with open transactions ({0} active)"},		/* open_trans */
	{335544358, "message length error (encountered {0}, expected {1})"},		/* port_len */
	{335544359, "attempted update of read-only column {0}"},		/* read_only_field */
	{335544360, "attempted update of read-only table"},		/* read_only_rel */
	{335544361, "attempted update during read-only transaction"},		/* read_only_trans */
	{335544362, "cannot update read-only view {0}"},		/* read_only_view */
	{335544363, "no transaction for request"},		/* req_no_trans */
	{335544364, "request synchronization error"},		/* req_sync */
	{335544365, "request referenced an unavailable database"},		/* req_wrong_db */
	{335544366, "segment buffer length shorter than expected"},		/* segment */
	{335544367, "attempted retrieval of more segments than exist"},		/* segstr_eof */
	{335544368, "attempted invalid operation on a BLOB"},		/* segstr_no_op */
	{335544369, "attempted read of a new, open BLOB"},		/* segstr_no_read */
	{335544370, "attempted action on BLOB outside transaction"},		/* segstr_no_trans */
	{335544371, "attempted write to read-only BLOB"},		/* segstr_no_write */
	{335544372, "attempted reference to BLOB in unavailable database"},		/* segstr_wrong_db */
	{335544373, "operating system directive {0} failed"},		/* sys_request */
	{335544374, "attempt to fetch past the last record in a record stream"},		/* stream_eof */
	{335544375, "unavailable database"},		/* unavailable */
	{335544376, "table {0} was omitted from the transaction reserving list"},		/* unres_rel */
	{335544377, "request includes a DSRI extension not supported in this implementation"},		/* uns_ext */
	{335544378, "feature is not supported"},		/* wish_list */
	{335544379, "unsupported on-disk structure for file {0}; found {1}.{2}, support {3}.{4}"},		/* wrong_ods */
	{335544380, "wrong number of arguments on call"},		/* wronumarg */
	{335544381, "Implementation limit exceeded"},		/* imp_exc */
	{335544382, "{0}"},		/* random */
	{335544383, "unrecoverable conflict with limbo transaction {0}"},		/* fatal_conflict */
	{335544384, "internal error"},		/* badblk */
	{335544385, "internal error"},		/* invpoolcl */
	{335544386, "too many requests"},		/* nopoolids */
	{335544387, "internal error"},		/* relbadblk */
	{335544388, "block size exceeds implementation restriction"},		/* blktoobig */
	{335544389, "buffer exhausted"},		/* bufexh */
	{335544390, "BLR syntax error: expected {0} at offset {1}, encountered {2}"},		/* syntaxerr */
	{335544391, "buffer in use"},		/* bufinuse */
	{335544392, "internal error"},		/* bdbincon */
	{335544393, "request in use"},		/* reqinuse */
	{335544394, "incompatible version of on-disk structure"},		/* badodsver */
	{335544395, "table {0} is not defined"},		/* relnotdef */
	{335544396, "column {0} is not defined in table {1}"},		/* fldnotdef */
	{335544397, "internal error"},		/* dirtypage */
	{335544398, "internal error"},		/* waifortra */
	{335544399, "internal error"},		/* doubleloc */
	{335544400, "internal error"},		/* nodnotfnd */
	{335544401, "internal error"},		/* dupnodfnd */
	{335544402, "internal error"},		/* locnotmar */
	{335544403, "page {0} is of wrong type (expected {1}, found {2})"},		/* badpagtyp */
	{335544404, "database corrupted"},		/* corrupt */
	{335544405, "checksum error on database page {0}"},		/* badpage */
	{335544406, "index is broken"},		/* badindex */
	{335544407, "database handle not zero"},		/* dbbnotzer */
	{335544408, "transaction handle not zero"},		/* tranotzer */
	{335544409, "transaction--request mismatch (synchronization error)"},		/* trareqmis */
	{335544410, "bad handle count"},		/* badhndcnt */
	{335544411, "wrong version of transaction parameter block"},		/* wrotpbver */
	{335544412, "unsupported BLR version (expected {0}, encountered {1})"},		/* wroblrver */
	{335544413, "wrong version of database parameter block"},		/* wrodpbver */
	{335544414, "BLOB and array data types are not supported for {0} operation"},		/* blobnotsup */
	{335544415, "database corrupted"},		/* badrelation */
	{335544416, "internal error"},		/* nodetach */
	{335544417, "internal error"},		/* notremote */
	{335544418, "transaction in limbo"},		/* trainlim */
	{335544419, "transaction not in limbo"},		/* notinlim */
	{335544420, "transaction outstanding"},		/* traoutsta */
	{335544421, "connection rejected by remote interface"},		/* connect_reject */
	{335544422, "internal error"},		/* dbfile */
	{335544423, "internal error"},		/* orphan */
	{335544424, "no lock manager available"},		/* no_lock_mgr */
	{335544425, "context already in use (BLR error)"},		/* ctxinuse */
	{335544426, "context not defined (BLR error)"},		/* ctxnotdef */
	{335544427, "data operation not supported"},		/* datnotsup */
	{335544428, "undefined message number"},		/* badmsgnum */
	{335544429, "undefined parameter number"},		/* badparnum */
	{335544430, "unable to allocate memory from operating system"},		/* virmemexh */
	{335544431, "blocking signal has been received"},		/* blocking_signal */
	{335544432, "lock manager error"},		/* lockmanerr */
	{335544433, "communication error with journal \"{0}\""},		/* journerr */
	{335544434, "key size exceeds implementation restriction for index \"{0}\""},		/* keytoobig */
	{335544435, "null segment of UNIQUE KEY"},		/* nullsegkey */
	{335544436, "SQL error code = {0}"},		/* sqlerr */
	{335544437, "wrong DYN version"},		/* wrodynver */
	{335544438, "function {0} is not defined"},		/* funnotdef */
	{335544439, "function {0} could not be matched"},		/* funmismat */
	{335544440, ""},		/* bad_msg_vec */
	{335544441, "database detach completed with errors"},		/* bad_detach */
	{335544442, "database system cannot read argument {0}"},		/* noargacc_read */
	{335544443, "database system cannot write argument {0}"},		/* noargacc_write */
	{335544444, "operation not supported"},		/* read_only */
	{335544445, "{0} extension error"},		/* ext_err */
	{335544446, "not updatable"},		/* non_updatable */
	{335544447, "no rollback performed"},		/* no_rollback */
	{335544448, ""},		/* bad_sec_info */
	{335544449, ""},		/* invalid_sec_info */
	{335544450, "{0}"},		/* misc_interpreted */
	{335544451, "update conflicts with concurrent update"},		/* update_conflict */
	{335544452, "product {0} is not licensed"},		/* unlicensed */
	{335544453, "object {0} is in use"},		/* obj_in_use */
	{335544454, "filter not found to convert type {0} to type {1}"},		/* nofilter */
	{335544455, "cannot attach active shadow file"},		/* shadow_accessed */
	{335544456, "invalid slice description language at offset {0}"},		/* invalid_sdl */
	{335544457, "subscript out of bounds"},		/* out_of_bounds */
	{335544458, "column not array or invalid dimensions (expected {0}, encountered {1})"},		/* invalid_dimension */
	{335544459, "record from transaction {0} is stuck in limbo"},		/* rec_in_limbo */
	{335544460, "a file in manual shadow {0} is unavailable"},		/* shadow_missing */
	{335544461, "secondary server attachments cannot validate databases"},		/* cant_validate */
	{335544462, "secondary server attachments cannot start journaling"},		/* cant_start_journal */
	{335544463, "generator {0} is not defined"},		/* gennotdef */
	{335544464, "secondary server attachments cannot start logging"},		/* cant_start_logging */
	{335544465, "invalid BLOB type for operation"},		/* bad_segstr_type */
	{335544466, "violation of FOREIGN KEY constraint \"{0}\" on table \"{1}\""},		/* foreign_key */
	{335544467, "minor version too high found {0} expected {1}"},		/* high_minor */
	{335544468, "transaction {0} is {1}"},		/* tra_state */
	{335544469, "transaction marked invalid and cannot be committed"},		/* trans_invalid */
	{335544470, "cache buffer for page {0} invalid"},		/* buf_invalid */
	{335544471, "there is no index in table {0} with id {1}"},		/* indexnotdefined */
	{335544472, "Your user name and password are not defined. Ask your database administrator to set up a Firebird login."},		/* login */
	{335544473, "invalid bookmark handle"},		/* invalid_bookmark */
	{335544474, "invalid lock level {0}"},		/* bad_lock_level */
	{335544475, "lock on table {0} conflicts with existing lock"},		/* relation_lock */
	{335544476, "requested record lock conflicts with existing lock"},		/* record_lock */
	{335544477, "maximum indexes per table ({0}) exceeded"},		/* max_idx */
	{335544478, "enable journal for database before starting online dump"},		/* jrn_enable */
	{335544479, "online dump failure. Retry dump"},		/* old_failure */
	{335544480, "an online dump is already in progress"},		/* old_in_progress */
	{335544481, "no more disk/tape space.  Cannot continue online dump"},		/* old_no_space */
	{335544482, "journaling allowed only if database has Write-ahead Log"},		/* no_wal_no_jrn */
	{335544483, "maximum number of online dump files that can be specified is 16"},		/* num_old_files */
	{335544484, "error in opening Write-ahead Log file during recovery"},		/* wal_file_open */
	{335544485, "invalid statement handle"},		/* bad_stmt_handle */
	{335544486, "Write-ahead log subsystem failure"},		/* wal_failure */
	{335544487, "WAL Writer error"},		/* walw_err */
	{335544488, "Log file header of {0} too small"},		/* logh_small */
	{335544489, "Invalid version of log file {0}"},		/* logh_inv_version */
	{335544490, "Log file {0} not latest in the chain but open flag still set"},		/* logh_open_flag */
	{335544491, "Log file {0} not closed properly; database recovery may be required"},		/* logh_open_flag2 */
	{335544492, "Database name in the log file {0} is different"},		/* logh_diff_dbname */
	{335544493, "Unexpected end of log file {0} at offset {1}"},		/* logf_unexpected_eof */
	{335544494, "Incomplete log record at offset {0} in log file {1}"},		/* logr_incomplete */
	{335544495, "Log record header too small at offset {0} in log file {1}"},		/* logr_header_small */
	{335544496, "Log block too small at offset {0} in log file {1}"},		/* logb_small */
	{335544497, "Illegal attempt to attach to an uninitialized WAL segment for {0}"},		/* wal_illegal_attach */
	{335544498, "Invalid WAL parameter block option {0}"},		/* wal_invalid_wpb */
	{335544499, "Cannot roll over to the next log file {0}"},		/* wal_err_rollover */
	{335544500, "database does not use Write-ahead Log"},		/* no_wal */
	{335544501, "cannot drop log file when journaling is enabled"},		/* drop_wal */
	{335544502, "reference to invalid stream number"},		/* stream_not_defined */
	{335544503, "WAL subsystem encountered error"},		/* wal_subsys_error */
	{335544504, "WAL subsystem corrupted"},		/* wal_subsys_corrupt */
	{335544505, "must specify archive file when enabling long term journal for databases with round-robin log files"},		/* no_archive */
	{335544506, "database {0} shutdown in progress"},		/* shutinprog */
	{335544507, "refresh range number {0} already in use"},		/* range_in_use */
	{335544508, "refresh range number {0} not found"},		/* range_not_found */
	{335544509, "CHARACTER SET {0} is not defined"},		/* charset_not_found */
	{335544510, "lock time-out on wait transaction"},		/* lock_timeout */
	{335544511, "procedure {0} is not defined"},		/* prcnotdef */
	{335544512, "Input parameter mismatch for procedure {0}"},		/* prcmismat */
	{335544513, "Database {0}: WAL subsystem bug for pid {1}\n{2}"},		/* wal_bugcheck */
	{335544514, "Could not expand the WAL segment for database {0}"},		/* wal_cant_expand */
	{335544515, "status code {0} unknown"},		/* codnotdef */
	{335544516, "exception {0} not defined"},		/* xcpnotdef */
	{335544517, "exception {0}"},		/* except */
	{335544518, "restart shared cache manager"},		/* cache_restart */
	{335544519, "invalid lock handle"},		/* bad_lock_handle */
	{335544520, "long-term journaling already enabled"},		/* jrn_present */
	{335544521, "Unable to roll over please see Firebird log."},		/* wal_err_rollover2 */
	{335544522, "WAL I/O error.  Please see Firebird log."},		/* wal_err_logwrite */
	{335544523, "WAL writer - Journal server communication error.  Please see Firebird log."},		/* wal_err_jrn_comm */
	{335544524, "WAL buffers cannot be increased.  Please see Firebird log."},		/* wal_err_expansion */
	{335544525, "WAL setup error.  Please see Firebird log."},		/* wal_err_setup */
	{335544526, "obsolete"},		/* wal_err_ww_sync */
	{335544527, "Cannot start WAL writer for the database {0}"},		/* wal_err_ww_start */
	{335544528, "database {0} shutdown"},		/* shutdown */
	{335544529, "cannot modify an existing user privilege"},		/* existing_priv_mod */
	{335544530, "Cannot delete PRIMARY KEY being used in FOREIGN KEY definition."},		/* primary_key_ref */
	{335544531, "Column used in a PRIMARY constraint must be NOT NULL."},		/* primary_key_notnull */
	{335544532, "Name of Referential Constraint not defined in constraints table."},		/* ref_cnstrnt_notfound */
	{335544533, "Non-existent PRIMARY or UNIQUE KEY specified for FOREIGN KEY."},		/* foreign_key_notfound */
	{335544534, "Cannot update constraints (RDB$REF_CONSTRAINTS)."},		/* ref_cnstrnt_update */
	{335544535, "Cannot update constraints (RDB$CHECK_CONSTRAINTS)."},		/* check_cnstrnt_update */
	{335544536, "Cannot delete CHECK constraint entry (RDB$CHECK_CONSTRAINTS)"},		/* check_cnstrnt_del */
	{335544537, "Cannot delete index segment used by an Integrity Constraint"},		/* integ_index_seg_del */
	{335544538, "Cannot update index segment used by an Integrity Constraint"},		/* integ_index_seg_mod */
	{335544539, "Cannot delete index used by an Integrity Constraint"},		/* integ_index_del */
	{335544540, "Cannot modify index used by an Integrity Constraint"},		/* integ_index_mod */
	{335544541, "Cannot delete trigger used by a CHECK Constraint"},		/* check_trig_del */
	{335544542, "Cannot update trigger used by a CHECK Constraint"},		/* check_trig_update */
	{335544543, "Cannot delete column being used in an Integrity Constraint."},		/* cnstrnt_fld_del */
	{335544544, "Cannot rename column being used in an Integrity Constraint."},		/* cnstrnt_fld_rename */
	{335544545, "Cannot update constraints (RDB$RELATION_CONSTRAINTS)."},		/* rel_cnstrnt_update */
	{335544546, "Cannot define constraints on views"},		/* constaint_on_view */
	{335544547, "internal Firebird consistency check (invalid RDB$CONSTRAINT_TYPE)"},		/* invld_cnstrnt_type */
	{335544548, "Attempt to define a second PRIMARY KEY for the same table"},		/* primary_key_exists */
	{335544549, "cannot modify or erase a system trigger"},		/* systrig_update */
	{335544550, "only the owner of a table may reassign ownership"},		/* not_rel_owner */
	{335544551, "could not find object for GRANT"},		/* grant_obj_notfound */
	{335544552, "could not find column for GRANT"},		/* grant_fld_notfound */
	{335544553, "user does not have GRANT privileges for operation"},		/* grant_nopriv */
	{335544554, "object has non-SQL security class defined"},		/* nonsql_security_rel */
	{335544555, "column has non-SQL security class defined"},		/* nonsql_security_fld */
	{335544556, "Write-ahead Log without shared cache configuration not allowed"},		/* wal_cache_err */
	{335544557, "database shutdown unsuccessful"},		/* shutfail */
	{335544558, "Operation violates CHECK constraint {0} on view or table {1}"},		/* check_constraint */
	{335544559, "invalid service handle"},		/* bad_svc_handle */
	{335544560, "database {0} shutdown in {1} seconds"},		/* shutwarn */
	{335544561, "wrong version of service parameter block"},		/* wrospbver */
	{335544562, "unrecognized service parameter block"},		/* bad_spb_form */
	{335544563, "service {0} is not defined"},		/* svcnotdef */
	{335544564, "long-term journaling not enabled"},		/* no_jrn */
	{335544565, "Cannot transliterate character between character sets"},		/* transliteration_failed */
	{335544566, "WAL defined; Cache Manager must be started first"},		/* start_cm_for_wal */
	{335544567, "Overflow log specification required for round-robin log"},		/* wal_ovflow_log_required */
	{335544568, "Implementation of text subtype {0} not located."},		/* text_subtype */
	{335544569, "Dynamic SQL Error"},		/* dsql_error */
	{335544570, "Invalid command"},		/* dsql_command_err */
	{335544571, "Data type for constant unknown"},		/* dsql_constant_err */
	{335544572, "Invalid cursor reference"},		/* dsql_cursor_err */
	{335544573, "Data type unknown"},		/* dsql_datatype_err */
	{335544574, "Invalid cursor declaration"},		/* dsql_decl_err */
	{335544575, "Cursor {0} is not updatable"},		/* dsql_cursor_update_err */
	{335544576, "Attempt to reopen an open cursor"},		/* dsql_cursor_open_err */
	{335544577, "Attempt to reclose a closed cursor"},		/* dsql_cursor_close_err */
	{335544578, "Column unknown"},		/* dsql_field_err */
	{335544579, "Internal error"},		/* dsql_internal_err */
	{335544580, "Table unknown"},		/* dsql_relation_err */
	{335544581, "Procedure unknown"},		/* dsql_procedure_err */
	{335544582, "Request unknown"},		/* dsql_request_err */
	{335544583, "SQLDA error"},		/* dsql_sqlda_err */
	{335544584, "Count of read-write columns does not equal count of values"},		/* dsql_var_count_err */
	{335544585, "Invalid statement handle"},		/* dsql_stmt_handle */
	{335544586, "Function unknown"},		/* dsql_function_err */
	{335544587, "Column is not a BLOB"},		/* dsql_blob_err */
	{335544588, "COLLATION {0} for CHARACTER SET {1} is not defined"},		/* collation_not_found */
	{335544589, "COLLATION {0} is not valid for specified CHARACTER SET"},		/* collation_not_for_charset */
	{335544590, "Option specified more than once"},		/* dsql_dup_option */
	{335544591, "Unknown transaction option"},		/* dsql_tran_err */
	{335544592, "Invalid array reference"},		/* dsql_invalid_array */
	{335544593, "Array declared with too many dimensions"},		/* dsql_max_arr_dim_exceeded */
	{335544594, "Illegal array dimension range"},		/* dsql_arr_range_error */
	{335544595, "Trigger unknown"},		/* dsql_trigger_err */
	{335544596, "Subselect illegal in this context"},		/* dsql_subselect_err */
	{335544597, "Cannot prepare a CREATE DATABASE/SCHEMA statement"},		/* dsql_crdb_prepare_err */
	{335544598, "must specify column name for view select expression"},		/* specify_field_err */
	{335544599, "number of columns does not match select list"},		/* num_field_err */
	{335544600, "Only simple column names permitted for VIEW WITH CHECK OPTION"},		/* col_name_err */
	{335544601, "No WHERE clause for VIEW WITH CHECK OPTION"},		/* where_err */
	{335544602, "Only one table allowed for VIEW WITH CHECK OPTION"},		/* table_view_err */
	{335544603, "DISTINCT, GROUP or HAVING not permitted for VIEW WITH CHECK OPTION"},		/* distinct_err */
	{335544604, "FOREIGN KEY column count does not match PRIMARY KEY"},		/* key_field_count_err */
	{335544605, "No subqueries permitted for VIEW WITH CHECK OPTION"},		/* subquery_err */
	{335544606, "expression evaluation not supported"},		/* expression_eval_err */
	{335544607, "gen.c: node not supported"},		/* node_err */
	{335544608, "Unexpected end of command"},		/* command_end_err */
	{335544609, "INDEX {0}"},		/* index_name */
	{335544610, "EXCEPTION {0}"},		/* exception_name */
	{335544611, "COLUMN {0}"},		/* field_name */
	{335544612, "Token unknown"},		/* token_err */
	{335544613, "union not supported"},		/* union_err */
	{335544614, "Unsupported DSQL construct"},		/* dsql_construct_err */
	{335544615, "column used with aggregate"},		/* field_aggregate_err */
	{335544616, "invalid column reference"},		/* field_ref_err */
	{335544617, "invalid ORDER BY clause"},		/* order_by_err */
	{335544618, "Return mode by value not allowed for this data type"},		/* return_mode_err */
	{335544619, "External functions cannot have more than 10 parameters"},		/* extern_func_err */
	{335544620, "alias {0} conflicts with an alias in the same statement"},		/* alias_conflict_err */
	{335544621, "alias {0} conflicts with a procedure in the same statement"},		/* procedure_conflict_error */
	{335544622, "alias {0} conflicts with a table in the same statement"},		/* relation_conflict_err */
	{335544623, "Illegal use of keyword VALUE"},		/* dsql_domain_err */
	{335544624, "segment count of 0 defined for index {0}"},		/* idx_seg_err */
	{335544625, "A node name is not permitted in a secondary, shadow, cache or log file name"},		/* node_name_err */
	{335544626, "TABLE {0}"},		/* table_name */
	{335544627, "PROCEDURE {0}"},		/* proc_name */
	{335544628, "cannot create index {0}"},		/* idx_create_err */
	{335544629, "Write-ahead Log with shadowing configuration not allowed"},		/* wal_shadow_err */
	{335544630, "there are {0} dependencies"},		/* dependency */
	{335544631, "too many keys defined for index {0}"},		/* idx_key_err */
	{335544632, "Preceding file did not specify length, so {0} must include starting page number"},		/* dsql_file_length_err */
	{335544633, "Shadow number must be a positive integer"},		/* dsql_shadow_number_err */
	{335544634, "Token unknown - line {0}, column {1}"},		/* dsql_token_unk_err */
	{335544635, "there is no alias or table named {0} at this scope level"},		/* dsql_no_relation_alias */
	{335544636, "there is no index {0} for table {1}"},		/* indexname */
	{335544637, "table or procedure {0} is not referenced in plan"},		/* no_stream_plan */
	{335544638, "table or procedure {0} is referenced more than once in plan; use aliases to distinguish"},		/* stream_twice */
	{335544639, "table or procedure {0} is referenced in the plan but not the from list"},		/* stream_not_found */
	{335544640, "Invalid use of CHARACTER SET or COLLATE"},		/* collation_requires_text */
	{335544641, "Specified domain or source column {0} does not exist"},		/* dsql_domain_not_found */
	{335544642, "index {0} cannot be used in the specified plan"},		/* index_unused */
	{335544643, "the table {0} is referenced twice; use aliases to differentiate"},		/* dsql_self_join */
	{335544644, "attempt to fetch before the first record in a record stream"},		/* stream_bof */
	{335544645, "the current position is on a crack"},		/* stream_crack */
	{335544646, "database or file exists"},		/* db_or_file_exists */
	{335544647, "invalid comparison operator for find operation"},		/* invalid_operator */
	{335544648, "Connection lost to pipe server"},		/* conn_lost */
	{335544649, "bad checksum"},		/* bad_checksum */
	{335544650, "wrong page type"},		/* page_type_err */
	{335544651, "Cannot insert because the file is readonly or is on a read only medium."},		/* ext_readonly_err */
	{335544652, "multiple rows in singleton select"},		/* sing_select_err */
	{335544653, "cannot attach to password database"},		/* psw_attach */
	{335544654, "cannot start transaction for password database"},		/* psw_start_trans */
	{335544655, "invalid direction for find operation"},		/* invalid_direction */
	{335544656, "variable {0} conflicts with parameter in same procedure"},		/* dsql_var_conflict */
	{335544657, "Array/BLOB/DATE data types not allowed in arithmetic"},		/* dsql_no_blob_array */
	{335544658, "{0} is not a valid base table of the specified view"},		/* dsql_base_table */
	{335544659, "table or procedure {0} is referenced twice in view; use an alias to distinguish"},		/* duplicate_base_table */
	{335544660, "view {0} has more than one base table; use aliases to distinguish"},		/* view_alias */
	{335544661, "cannot add index, index root page is full."},		/* index_root_page_full */
	{335544662, "BLOB SUB_TYPE {0} is not defined"},		/* dsql_blob_type_unknown */
	{335544663, "Too many concurrent executions of the same request"},		/* req_max_clones_exceeded */
	{335544664, "duplicate specification of {0} - not supported"},		/* dsql_duplicate_spec */
	{335544665, "violation of PRIMARY or UNIQUE KEY constraint \"{0}\" on table \"{1}\""},		/* unique_key_violation */
	{335544666, "server version too old to support all CREATE DATABASE options"},		/* srvr_version_too_old */
	{335544667, "drop database completed with errors"},		/* drdb_completed_with_errs */
	{335544668, "procedure {0} does not return any values"},		/* dsql_procedure_use_err */
	{335544669, "count of column list and variable list do not match"},		/* dsql_count_mismatch */
	{335544670, "attempt to index BLOB column in index {0}"},		/* blob_idx_err */
	{335544671, "attempt to index array column in index {0}"},		/* array_idx_err */
	{335544672, "too few key columns found for index {0} (incorrect column name?)"},		/* key_field_err */
	{335544673, "cannot delete"},		/* no_delete */
	{335544674, "last column in a table cannot be deleted"},		/* del_last_field */
	{335544675, "sort error"},		/* sort_err */
	{335544676, "sort error: not enough memory"},		/* sort_mem_err */
	{335544677, "too many versions"},		/* version_err */
	{335544678, "invalid key position"},		/* inval_key_posn */
	{335544679, "segments not allowed in expression index {0}"},		/* no_segments_err */
	{335544680, "sort error: corruption in data structure"},		/* crrp_data_err */
	{335544681, "new record size of {0} bytes is too big"},		/* rec_size_err */
	{335544682, "Inappropriate self-reference of column"},		/* dsql_field_ref */
	{335544683, "request depth exceeded. (Recursive definition?)"},		/* req_depth_exceeded */
	{335544684, "cannot access column {0} in view {1}"},		/* no_field_access */
	{335544685, "dbkey not available for multi-table views"},		/* no_dbkey */
	{335544686, "journal file wrong format"},		/* jrn_format_err */
	{335544687, "intermediate journal file full"},		/* jrn_file_full */
	{335544688, "The prepare statement identifies a prepare statement with an open cursor"},		/* dsql_open_cursor_request */
	{335544689, "Firebird error"},		/* ib_error */
	{335544690, "Cache redefined"},		/* cache_redef */
	{335544691, "Insufficient memory to allocate page buffer cache"},		/* cache_too_small */
	{335544692, "Log redefined"},		/* log_redef */
	{335544693, "Log size too small"},		/* log_too_small */
	{335544694, "Log partition size too small"},		/* partition_too_small */
	{335544695, "Partitions not supported in series of log file specification"},		/* partition_not_supp */
	{335544696, "Total length of a partitioned log must be specified"},		/* log_length_spec */
	{335544697, "Precision must be from 1 to 18"},		/* precision_err */
	{335544698, "Scale must be between zero and precision"},		/* scale_nogt */
	{335544699, "Short integer expected"},		/* expec_short */
	{335544700, "Long integer expected"},		/* expec_long */
	{335544701, "Unsigned short integer expected"},		/* expec_ushort */
	{335544702, "Invalid ESCAPE sequence"},		/* escape_invalid */
	{335544703, "service {0} does not have an associated executable"},		/* svcnoexe */
	{335544704, "Failed to locate host machine."},		/* net_lookup_err */
	{335544705, "Undefined service {0}/{1}."},		/* service_unknown */
	{335544706, "The specified name was not found in the hosts file or Domain Name Services."},		/* host_unknown */
	{335544707, "user does not have GRANT privileges on base table/view for operation"},		/* grant_nopriv_on_base */
	{335544708, "Ambiguous column reference."},		/* dyn_fld_ambiguous */
	{335544709, "Invalid aggregate reference"},		/* dsql_agg_ref_err */
	{335544710, "navigational stream {0} references a view with more than one base table"},		/* complex_view */
	{335544711, "Attempt to execute an unprepared dynamic SQL statement."},		/* unprepared_stmt */
	{335544712, "Positive value expected"},		/* expec_positive */
	{335544713, "Incorrect values within SQLDA structure"},		/* dsql_sqlda_value_err */
	{335544714, "invalid blob id"},		/* invalid_array_id */
	{335544715, "Operation not supported for EXTERNAL FILE table {0}"},		/* extfile_uns_op */
	{335544716, "Service is currently busy: {0}"},		/* svc_in_use */
	{335544717, "stack size insufficent to execute current request"},		/* err_stack_limit */
	{335544718, "Invalid key for find operation"},		/* invalid_key */
	{335544719, "Error initializing the network software."},		/* net_init_error */
	{335544720, "Unable to load required library {0}."},		/* loadlib_failure */
	{335544721, "Unable to complete network request to host \"{0}\"."},		/* network_error */
	{335544722, "Failed to establish a connection."},		/* net_connect_err */
	{335544723, "Error while listening for an incoming connection."},		/* net_connect_listen_err */
	{335544724, "Failed to establish a secondary connection for event processing."},		/* net_event_connect_err */
	{335544725, "Error while listening for an incoming event connection request."},		/* net_event_listen_err */
	{335544726, "Error reading data from the connection."},		/* net_read_err */
	{335544727, "Error writing data to the connection."},		/* net_write_err */
	{335544728, "Cannot deactivate index used by an integrity constraint"},		/* integ_index_deactivate */
	{335544729, "Cannot deactivate index used by a PRIMARY/UNIQUE constraint"},		/* integ_deactivate_primary */
	{335544730, "Client/Server Express not supported in this release"},		/* cse_not_supported */
	{335544731, ""},		/* tra_must_sweep */
	{335544732, "Access to databases on file servers is not supported."},		/* unsupported_network_drive */
	{335544733, "Error while trying to create file"},		/* io_create_err */
	{335544734, "Error while trying to open file"},		/* io_open_err */
	{335544735, "Error while trying to close file"},		/* io_close_err */
	{335544736, "Error while trying to read from file"},		/* io_read_err */
	{335544737, "Error while trying to write to file"},		/* io_write_err */
	{335544738, "Error while trying to delete file"},		/* io_delete_err */
	{335544739, "Error while trying to access file"},		/* io_access_err */
	{335544740, "A fatal exception occurred during the execution of a user defined function."},		/* udf_exception */
	{335544741, "connection lost to database"},		/* lost_db_connection */
	{335544742, "User cannot write to RDB$USER_PRIVILEGES"},		/* no_write_user_priv */
	{335544743, "token size exceeds limit"},		/* token_too_long */
	{335544744, "Maximum user count exceeded.  Contact your database administrator."},		/* max_att_exceeded */
	{335544745, "Your login {0} is same as one of the SQL role name. Ask your database administrator to set up a valid Firebird login."},		/* login_same_as_role_name */
	{335544746, "\"REFERENCES table\" without \"(column)\" requires PRIMARY KEY on referenced table"},		/* reftable_requires_pk */
	{335544747, "The username entered is too long.  Maximum length is 31 bytes."},		/* usrname_too_long */
	{335544748, "The password specified is too long.  Maximum length is 8 bytes."},		/* password_too_long */
	{335544749, "A username is required for this operation."},		/* usrname_required */
	{335544750, "A password is required for this operation"},		/* password_required */
	{335544751, "The network protocol specified is invalid"},		/* bad_protocol */
	{335544752, "A duplicate user name was found in the security database"},		/* dup_usrname_found */
	{335544753, "The user name specified was not found in the security database"},		/* usrname_not_found */
	{335544754, "An error occurred while attempting to add the user."},		/* error_adding_sec_record */
	{335544755, "An error occurred while attempting to modify the user record."},		/* error_modifying_sec_record */
	{335544756, "An error occurred while attempting to delete the user record."},		/* error_deleting_sec_record */
	{335544757, "An error occurred while updating the security database."},		/* error_updating_sec_db */
	{335544758, "sort record size of {0} bytes is too big"},		/* sort_rec_size_err */
	{335544759, "can not define a not null column with NULL as default value"},		/* bad_default_value */
	{335544760, "invalid clause --- '{0}'"},		/* invalid_clause */
	{335544761, "too many open handles to database"},		/* too_many_handles */
	{335544762, "size of optimizer block exceeded"},		/* optimizer_blk_exc */
	{335544763, "a string constant is delimited by double quotes"},		/* invalid_string_constant */
	{335544764, "DATE must be changed to TIMESTAMP"},		/* transitional_date */
	{335544765, "attempted update on read-only database"},		/* read_only_database */
	{335544766, "SQL dialect {0} is not supported in this database"},		/* must_be_dialect_2_and_up */
	{335544767, "A fatal exception occurred during the execution of a blob filter."},		/* blob_filter_exception */
	{335544768, "Access violation.  The code attempted to access a virtual address without privilege to do so."},		/* exception_access_violation */
	{335544769, "Datatype misalignment.  The attempted to read or write a value that was not stored on a memory boundary."},		/* exception_datatype_missalignment */
	{335544770, "Array bounds exceeded.  The code attempted to access an array element that is out of bounds."},		/* exception_array_bounds_exceeded */
	{335544771, "Float denormal operand.  One of the floating-point operands is too small to represent a standard float value."},		/* exception_float_denormal_operand */
	{335544772, "Floating-point divide by zero.  The code attempted to divide a floating-point value by zero."},		/* exception_float_divide_by_zero */
	{335544773, "Floating-point inexact result.  The result of a floating-point operation cannot be represented as a decimal fraction."},		/* exception_float_inexact_result */
	{335544774, "Floating-point invalid operand.  An indeterminant error occurred during a floating-point operation."},		/* exception_float_invalid_operand */
	{335544775, "Floating-point overflow.  The exponent of a floating-point operation is greater than the magnitude allowed."},		/* exception_float_overflow */
	{335544776, "Floating-point stack check.  The stack overflowed or underflowed as the result of a floating-point operation."},		/* exception_float_stack_check */
	{335544777, "Floating-point underflow.  The exponent of a floating-point operation is less than the magnitude allowed."},		/* exception_float_underflow */
	{335544778, "Integer divide by zero.  The code attempted to divide an integer value by an integer divisor of zero."},		/* exception_integer_divide_by_zero */
	{335544779, "Integer overflow.  The result of an integer operation caused the most significant bit of the result to carry."},		/* exception_integer_overflow */
	{335544780, "An exception occurred that does not have a description.  Exception number {0}."},		/* exception_unknown */
	{335544781, "Stack overflow.  The resource requirements of the runtime stack have exceeded the memory available to it."},		/* exception_stack_overflow */
	{335544782, "Segmentation Fault. The code attempted to access memory without privileges."},		/* exception_sigsegv */
	{335544783, "Illegal Instruction. The Code attempted to perform an illegal operation."},		/* exception_sigill */
	{335544784, "Bus Error. The Code caused a system bus error."},		/* exception_sigbus */
	{335544785, "Floating Point Error. The Code caused an Arithmetic Exception or a floating point exception."},		/* exception_sigfpe */
	{335544786, "Cannot delete rows from external files."},		/* ext_file_delete */
	{335544787, "Cannot update rows in external files."},		/* ext_file_modify */
	{335544788, "Unable to perform operation"},		/* adm_task_denied */
	{335544789, "Specified EXTRACT part does not exist in input datatype"},		/* extract_input_mismatch */
	{335544790, "Service {0} requires SYSDBA permissions.  Reattach to the Service Manager using the SYSDBA account."},		/* insufficient_svc_privileges */
	{335544791, "The file {0} is currently in use by another process.  Try again later."},		/* file_in_use */
	{335544792, "Cannot attach to services manager"},		/* service_att_err */
	{335544793, "Metadata update statement is not allowed by the current database SQL dialect {0}"},		/* ddl_not_allowed_by_db_sql_dial */
	{335544794, "operation was cancelled"},		/* cancelled */
	{335544795, "unexpected item in service parameter block, expected {0}"},		/* unexp_spb_form */
	{335544796, "Client SQL dialect {0} does not support reference to {1} datatype"},		/* sql_dialect_datatype_unsupport */
	{335544797, "user name and password are required while attaching to the services manager"},		/* svcnouser */
	{335544798, "You created an indirect dependency on uncommitted metadata. You must roll back the current transaction."},		/* depend_on_uncommitted_rel */
	{335544799, "The service name was not specified."},		/* svc_name_missing */
	{335544800, "Too many Contexts of Relation/Procedure/Views. Maximum allowed is 256"},		/* too_many_contexts */
	{335544801, "data type not supported for arithmetic"},		/* datype_notsup */
	{335544802, "Database dialect being changed from 3 to 1"},		/* dialect_reset_warning */
	{335544803, "Database dialect not changed."},		/* dialect_not_changed */
	{335544804, "Unable to create database {0}"},		/* database_create_failed */
	{335544805, "Database dialect {0} is not a valid dialect."},		/* inv_dialect_specified */
	{335544806, "Valid database dialects are {0}."},		/* valid_db_dialects */
	{335544807, "SQL warning code = {0}"},		/* sqlwarn */
	{335544808, "DATE data type is now called TIMESTAMP"},		/* dtype_renamed */
	{335544809, "Function {0} is in {1}, which is not in a permitted directory for external functions."},		/* extern_func_dir_error */
	{335544810, "value exceeds the range for valid dates"},		/* date_range_exceeded */
	{335544811, "passed client dialect {0} is not a valid dialect."},		/* inv_client_dialect_specified */
	{335544812, "Valid client dialects are {0}."},		/* valid_client_dialects */
	{335544813, "Unsupported field type specified in BETWEEN predicate."},		/* optimizer_between_err */
	{335544814, "Services functionality will be supported in a later version  of the product"},		/* service_not_supported */
	{335544815, "GENERATOR {0}"},		/* generator_name */
	{335544816, "Function {0}"},		/* udf_name */
	{335544817, "Invalid parameter to FETCH or FIRST. Only integers >= 0 are allowed."},		/* bad_limit_param */
	{335544818, "Invalid parameter to OFFSET or SKIP. Only integers >= 0 are allowed."},		/* bad_skip_param */
	{335544819, "File exceeded maximum size of 2GB.  Add another database file or use a 64 bit I/O version of Firebird."},		/* io_32bit_exceeded_err */
	{335544820, "Unable to find savepoint with name {0} in transaction context"},		/* invalid_savepoint */
	{335544821, "Invalid column position used in the {0} clause"},		/* dsql_column_pos_err */
	{335544822, "Cannot use an aggregate or window function in a WHERE clause, use HAVING (for aggregate only) instead"},		/* dsql_agg_where_err */
	{335544823, "Cannot use an aggregate or window function in a GROUP BY clause"},		/* dsql_agg_group_err */
	{335544824, "Invalid expression in the {0} (not contained in either an aggregate function or the GROUP BY clause)"},		/* dsql_agg_column_err */
	{335544825, "Invalid expression in the {0} (neither an aggregate function nor a part of the GROUP BY clause)"},		/* dsql_agg_having_err */
	{335544826, "Nested aggregate and window functions are not allowed"},		/* dsql_agg_nested_err */
	{335544827, "Invalid argument in EXECUTE STATEMENT - cannot convert to string"},		/* exec_sql_invalid_arg */
	{335544828, "Wrong request type in EXECUTE STATEMENT '{0}'"},		/* exec_sql_invalid_req */
	{335544829, "Variable type (position {0}) in EXECUTE STATEMENT '{1}' INTO does not match returned column type"},		/* exec_sql_invalid_var */
	{335544830, "Too many recursion levels of EXECUTE STATEMENT"},		/* exec_sql_max_call_exceeded */
	{335544831, "Use of {0} at location {1} is not allowed by server configuration"},		/* conf_access_denied */
	{335544832, "Cannot change difference file name while database is in backup mode"},		/* wrong_backup_state */
	{335544833, "Physical backup is not allowed while Write-Ahead Log is in use"},		/* wal_backup_err */
	{335544834, "Cursor is not open"},		/* cursor_not_open */
	{335544835, "Target shutdown mode is invalid for database \"{0}\""},		/* bad_shutdown_mode */
	{335544836, "Concatenation overflow. Resulting string cannot exceed 32765 bytes in length."},		/* concat_overflow */
	{335544837, "Invalid offset parameter {0} to SUBSTRING. Only positive integers are allowed."},		/* bad_substring_offset */
	{335544838, "Foreign key reference target does not exist"},		/* foreign_key_target_doesnt_exist */
	{335544839, "Foreign key references are present for the record"},		/* foreign_key_references_present */
	{335544840, "cannot update"},		/* no_update */
	{335544841, "Cursor is already open"},		/* cursor_already_open */
	{335544842, "{0}"},		/* stack_trace */
	{335544843, "Context variable '{0}' is not found in namespace '{1}'"},		/* ctx_var_not_found */
	{335544844, "Invalid namespace name '{0}' passed to {1}"},		/* ctx_namespace_invalid */
	{335544845, "Too many context variables"},		/* ctx_too_big */
	{335544846, "Invalid argument passed to {0}"},		/* ctx_bad_argument */
	{335544847, "BLR syntax error. Identifier {0}... is too long"},		/* identifier_too_long */
	{335544848, "exception {0}"},		/* except2 */
	{335544849, "Malformed string"},		/* malformed_string */
	{335544850, "Output parameter mismatch for procedure {0}"},		/* prc_out_param_mismatch */
	{335544851, "Unexpected end of command - line {0}, column {1}"},		/* command_end_err2 */
	{335544852, "partner index segment no {0} has incompatible data type"},		/* partner_idx_incompat_type */
	{335544853, "Invalid length parameter {0} to SUBSTRING. Negative integers are not allowed."},		/* bad_substring_length */
	{335544854, "CHARACTER SET {0} is not installed"},		/* charset_not_installed */
	{335544855, "COLLATION {0} for CHARACTER SET {1} is not installed"},		/* collation_not_installed */
	{335544856, "connection shutdown"},		/* att_shutdown */
	{335544857, "Maximum BLOB size exceeded"},		/* blobtoobig */
	{335544858, "Can't have relation with only computed fields or constraints"},		/* must_have_phys_field */
	{335544859, "Time precision exceeds allowed range (0-{0})"},		/* invalid_time_precision */
	{335544860, "Unsupported conversion to target type BLOB (subtype {0})"},		/* blob_convert_error */
	{335544861, "Unsupported conversion to target type ARRAY"},		/* array_convert_error */
	{335544862, "Stream does not support record locking"},		/* record_lock_not_supp */
	{335544863, "Cannot create foreign key constraint {0}. Partner index does not exist or is inactive."},		/* partner_idx_not_found */
	{335544864, "Transactions count exceeded. Perform backup and restore to make database operable again"},		/* tra_num_exc */
	{335544865, "Column has been unexpectedly deleted"},		/* field_disappeared */
	{335544866, "{0} cannot depend on {1}"},		/* met_wrong_gtt_scope */
	{335544867, "Blob sub_types bigger than 1 (text) are for internal use only"},		/* subtype_for_internal_use */
	{335544868, "Procedure {0} is not selectable (it does not contain a SUSPEND statement)"},		/* illegal_prc_type */
	{335544869, "Datatype {0} is not supported for sorting operation"},		/* invalid_sort_datatype */
	{335544870, "COLLATION {0}"},		/* collation_name */
	{335544871, "DOMAIN {0}"},		/* domain_name */
	{335544872, "domain {0} is not defined"},		/* domnotdef */
	{335544873, "Array data type can use up to {0} dimensions"},		/* array_max_dimensions */
	{335544874, "A multi database transaction cannot span more than {0} databases"},		/* max_db_per_trans_allowed */
	{335544875, "Bad debug info format"},		/* bad_debug_format */
	{335544876, "Error while parsing procedure {0}'s BLR"},		/* bad_proc_BLR */
	{335544877, "index key too big"},		/* key_too_big */
	{335544878, "concurrent transaction number is {0}"},		/* concurrent_transaction */
	{335544879, "validation error for variable {0}, value \"{1}\""},		/* not_valid_for_var */
	{335544880, "validation error for {0}, value \"{1}\""},		/* not_valid_for */
	{335544881, "Difference file name should be set explicitly for database on raw device"},		/* need_difference */
	{335544882, "Login name too long ({0} characters, maximum allowed {1})"},		/* long_login */
	{335544883, "column {0} is not defined in procedure {1}"},		/* fldnotdef2 */
	{335544884, "Invalid SIMILAR TO pattern"},		/* invalid_similar_pattern */
	{335544885, "Invalid TEB format"},		/* bad_teb_form */
	{335544886, "Found more than one transaction isolation in TPB"},		/* tpb_multiple_txn_isolation */
	{335544887, "Table reservation lock type {0} requires table name before in TPB"},		/* tpb_reserv_before_table */
	{335544888, "Found more than one {0} specification in TPB"},		/* tpb_multiple_spec */
	{335544889, "Option {0} requires READ COMMITTED isolation in TPB"},		/* tpb_option_without_rc */
	{335544890, "Option {0} is not valid if {1} was used previously in TPB"},		/* tpb_conflicting_options */
	{335544891, "Table name length missing after table reservation {0} in TPB"},		/* tpb_reserv_missing_tlen */
	{335544892, "Table name length {0} is too long after table reservation {1} in TPB"},		/* tpb_reserv_long_tlen */
	{335544893, "Table name length {0} without table name after table reservation {1} in TPB"},		/* tpb_reserv_missing_tname */
	{335544894, "Table name length {0} goes beyond the remaining TPB size after table reservation {1}"},		/* tpb_reserv_corrup_tlen */
	{335544895, "Table name length is zero after table reservation {0} in TPB"},		/* tpb_reserv_null_tlen */
	{335544896, "Table or view {0} not defined in system tables after table reservation {1} in TPB"},		/* tpb_reserv_relnotfound */
	{335544897, "Base table or view {0} for view {1} not defined in system tables after table reservation {2} in TPB"},		/* tpb_reserv_baserelnotfound */
	{335544898, "Option length missing after option {0} in TPB"},		/* tpb_missing_len */
	{335544899, "Option length {0} without value after option {1} in TPB"},		/* tpb_missing_value */
	{335544900, "Option length {0} goes beyond the remaining TPB size after option {1}"},		/* tpb_corrupt_len */
	{335544901, "Option length is zero after table reservation {0} in TPB"},		/* tpb_null_len */
	{335544902, "Option length {0} exceeds the range for option {1} in TPB"},		/* tpb_overflow_len */
	{335544903, "Option value {0} is invalid for the option {1} in TPB"},		/* tpb_invalid_value */
	{335544904, "Preserving previous table reservation {0} for table {1}, stronger than new {2} in TPB"},		/* tpb_reserv_stronger_wng */
	{335544905, "Table reservation {0} for table {1} already specified and is stronger than new {2} in TPB"},		/* tpb_reserv_stronger */
	{335544906, "Table reservation reached maximum recursion of {0} when expanding views in TPB"},		/* tpb_reserv_max_recursion */
	{335544907, "Table reservation in TPB cannot be applied to {0} because it's a virtual table"},		/* tpb_reserv_virtualtbl */
	{335544908, "Table reservation in TPB cannot be applied to {0} because it's a system table"},		/* tpb_reserv_systbl */
	{335544909, "Table reservation {0} or {1} in TPB cannot be applied to {2} because it's a temporary table"},		/* tpb_reserv_temptbl */
	{335544910, "Cannot set the transaction in read only mode after a table reservation isc_tpb_lock_write in TPB"},		/* tpb_readtxn_after_writelock */
	{335544911, "Cannot take a table reservation isc_tpb_lock_write in TPB because the transaction is in read only mode"},		/* tpb_writelock_after_readtxn */
	{335544912, "value exceeds the range for a valid time"},		/* time_range_exceeded */
	{335544913, "value exceeds the range for valid timestamps"},		/* datetime_range_exceeded */
	{335544914, "string right truncation"},		/* string_truncation */
	{335544915, "blob truncation when converting to a string: length limit exceeded"},		/* blob_truncation */
	{335544916, "numeric value is out of range"},		/* numeric_out_of_range */
	{335544917, "Firebird shutdown is still in progress after the specified timeout"},		/* shutdown_timeout */
	{335544918, "Attachment handle is busy"},		/* att_handle_busy */
	{335544919, "Bad written UDF detected: pointer returned in FREE_IT function was not allocated by ib_util_malloc"},		/* bad_udf_freeit */
	{335544920, "External Data Source provider '{0}' not found"},		/* eds_provider_not_found */
	{335544921, "Execute statement error at {0} :\n{1}Data source : {2}"},		/* eds_connection */
	{335544922, "Execute statement preprocess SQL error"},		/* eds_preprocess */
	{335544923, "Statement expected"},		/* eds_stmt_expected */
	{335544924, "Parameter name expected"},		/* eds_prm_name_expected */
	{335544925, "Unclosed comment found near '{0}'"},		/* eds_unclosed_comment */
	{335544926, "Execute statement error at {0} :\n{1}Statement : {2}\nData source : {3}"},		/* eds_statement */
	{335544927, "Input parameters mismatch"},		/* eds_input_prm_mismatch */
	{335544928, "Output parameters mismatch"},		/* eds_output_prm_mismatch */
	{335544929, "Input parameter '{0}' have no value set"},		/* eds_input_prm_not_set */
	{335544930, "BLR stream length {0} exceeds implementation limit {1}"},		/* too_big_blr */
	{335544931, "Monitoring table space exhausted"},		/* montabexh */
	{335544932, "module name or entrypoint could not be found"},		/* modnotfound */
	{335544933, "nothing to cancel"},		/* nothing_to_cancel */
	{335544934, "ib_util library has not been loaded to deallocate memory returned by FREE_IT function"},		/* ibutil_not_loaded */
	{335544935, "Cannot have circular dependencies with computed fields"},		/* circular_computed */
	{335544936, "Security database error"},		/* psw_db_error */
	{335544937, "Invalid data type in DATE/TIME/TIMESTAMP addition or subtraction in add_datettime()"},		/* invalid_type_datetime_op */
	{335544938, "Only a TIME value can be added to a DATE value"},		/* onlycan_add_timetodate */
	{335544939, "Only a DATE value can be added to a TIME value"},		/* onlycan_add_datetotime */
	{335544940, "TIMESTAMP values can be subtracted only from another TIMESTAMP value"},		/* onlycansub_tstampfromtstamp */
	{335544941, "Only one operand can be of type TIMESTAMP"},		/* onlyoneop_mustbe_tstamp */
	{335544942, "Only HOUR, MINUTE, SECOND and MILLISECOND can be extracted from TIME values"},		/* invalid_extractpart_time */
	{335544943, "HOUR, MINUTE, SECOND and MILLISECOND cannot be extracted from DATE values"},		/* invalid_extractpart_date */
	{335544944, "Invalid argument for EXTRACT() not being of DATE/TIME/TIMESTAMP type"},		/* invalidarg_extract */
	{335544945, "Arguments for {0} must be integral types or NUMERIC/DECIMAL without scale"},		/* sysf_argmustbe_exact */
	{335544946, "First argument for {0} must be integral type or floating point type"},		/* sysf_argmustbe_exact_or_fp */
	{335544947, "Human readable UUID argument for {0} must be of string type"},		/* sysf_argviolates_uuidtype */
	{335544948, "Human readable UUID argument for {1} must be of exact length {0}"},		/* sysf_argviolates_uuidlen */
	{335544949, "Human readable UUID argument for {2} must have \"-\" at position {1} instead of \"{0}\""},		/* sysf_argviolates_uuidfmt */
	{335544950, "Human readable UUID argument for {2} must have hex digit at position {1} instead of \"{0}\""},		/* sysf_argviolates_guidigits */
	{335544951, "Only HOUR, MINUTE, SECOND and MILLISECOND can be added to TIME values in {0}"},		/* sysf_invalid_addpart_time */
	{335544952, "Invalid data type in addition of part to DATE/TIME/TIMESTAMP in {0}"},		/* sysf_invalid_add_datetime */
	{335544953, "Invalid part {0} to be added to a DATE/TIME/TIMESTAMP value in {1}"},		/* sysf_invalid_addpart_dtime */
	{335544954, "Expected DATE/TIME/TIMESTAMP type in evlDateAdd() result"},		/* sysf_invalid_add_dtime_rc */
	{335544955, "Expected DATE/TIME/TIMESTAMP type as first and second argument to {0}"},		/* sysf_invalid_diff_dtime */
	{335544956, "The result of TIME-<value> in {0} cannot be expressed in YEAR, MONTH, DAY or WEEK"},		/* sysf_invalid_timediff */
	{335544957, "The result of TIME-TIMESTAMP or TIMESTAMP-TIME in {0} cannot be expressed in HOUR, MINUTE, SECOND or MILLISECOND"},		/* sysf_invalid_tstamptimediff */
	{335544958, "The result of DATE-TIME or TIME-DATE in {0} cannot be expressed in HOUR, MINUTE, SECOND and MILLISECOND"},		/* sysf_invalid_datetimediff */
	{335544959, "Invalid part {0} to express the difference between two DATE/TIME/TIMESTAMP values in {1}"},		/* sysf_invalid_diffpart */
	{335544960, "Argument for {0} must be positive"},		/* sysf_argmustbe_positive */
	{335544961, "Base for {0} must be positive"},		/* sysf_basemustbe_positive */
	{335544962, "Argument #{0} for {1} must be zero or positive"},		/* sysf_argnmustbe_nonneg */
	{335544963, "Argument #{0} for {1} must be positive"},		/* sysf_argnmustbe_positive */
	{335544964, "Base for {0} cannot be zero if exponent is negative"},		/* sysf_invalid_zeropowneg */
	{335544965, "Base for {0} cannot be negative if exponent is not an integral value"},		/* sysf_invalid_negpowfp */
	{335544966, "The numeric scale must be between -128 and 127 in {0}"},		/* sysf_invalid_scale */
	{335544967, "Argument for {0} must be zero or positive"},		/* sysf_argmustbe_nonneg */
	{335544968, "Binary UUID argument for {0} must be of string type"},		/* sysf_binuuid_mustbe_str */
	{335544969, "Binary UUID argument for {1} must use {0} bytes"},		/* sysf_binuuid_wrongsize */
	{335544970, "Missing required item {0} in service parameter block"},		/* missing_required_spb */
	{335544971, "{0} server is shutdown"},		/* net_server_shutdown */
	{335544972, "Invalid connection string"},		/* bad_conn_str */
	{335544973, "Unrecognized events block"},		/* bad_epb_form */
	{335544974, "Could not start first worker thread - shutdown server"},		/* no_threads */
	{335544975, "Timeout occurred while waiting for a secondary connection for event processing"},		/* net_event_connect_timeout */
	{335544976, "Argument for {0} must be different than zero"},		/* sysf_argmustbe_nonzero */
	{335544977, "Argument for {0} must be in the range [-1, 1]"},		/* sysf_argmustbe_range_inc1_1 */
	{335544978, "Argument for {0} must be greater or equal than one"},		/* sysf_argmustbe_gteq_one */
	{335544979, "Argument for {0} must be in the range ]-1, 1["},		/* sysf_argmustbe_range_exc1_1 */
	{335544980, "Incorrect parameters provided to internal function {0}"},		/* internal_rejected_params */
	{335544981, "Floating point overflow in built-in function {0}"},		/* sysf_fp_overflow */
	{335544982, "Floating point overflow in result from UDF {0}"},		/* udf_fp_overflow */
	{335544983, "Invalid floating point value returned by UDF {0}"},		/* udf_fp_nan */
	{335544984, "Shared memory area is probably already created by another engine instance in another Windows session"},		/* instance_conflict */
	{335544985, "No free space found in temporary directories"},		/* out_of_temp_space */
	{335544986, "Explicit transaction control is not allowed"},		/* eds_expl_tran_ctrl */
	{335544987, "Use of TRUSTED switches in spb_command_line is prohibited"},		/* no_trusted_spb */
	{335544988, "PACKAGE {0}"},		/* package_name */
	{335544989, "Cannot make field {0} of table {1} NOT NULL because there are NULLs present"},		/* cannot_make_not_null */
	{335544990, "Feature {0} is not supported anymore"},		/* feature_removed */
	{335544991, "VIEW {0}"},		/* view_name */
	{335544992, "Can not access lock files directory {0}"},		/* lock_dir_access */
	{335544993, "Fetch option {0} is invalid for a non-scrollable cursor"},		/* invalid_fetch_option */
	{335544994, "Error while parsing function {0}'s BLR"},		/* bad_fun_BLR */
	{335544995, "Cannot execute function {0} of the unimplemented package {1}"},		/* func_pack_not_implemented */
	{335544996, "Cannot execute procedure {0} of the unimplemented package {1}"},		/* proc_pack_not_implemented */
	{335544997, "External function {0} not returned by the external engine plugin {1}"},		/* eem_func_not_returned */
	{335544998, "External procedure {0} not returned by the external engine plugin {1}"},		/* eem_proc_not_returned */
	{335544999, "External trigger {0} not returned by the external engine plugin {1}"},		/* eem_trig_not_returned */
	{335545000, "Incompatible plugin version {0} for external engine {1}"},		/* eem_bad_plugin_ver */
	{335545001, "External engine {0} not found"},		/* eem_engine_notfound */
	{335545002, "Attachment is in use"},		/* attachment_in_use */
	{335545003, "Transaction is in use"},		/* transaction_in_use */
	{335545004, "Error loading plugin {0}"},		/* pman_cannot_load_plugin */
	{335545005, "Loadable module {0} not found"},		/* pman_module_notfound */
	{335545006, "Standard plugin entrypoint does not exist in module {0}"},		/* pman_entrypoint_notfound */
	{335545007, "Module {0} exists but can not be loaded"},		/* pman_module_bad */
	{335545008, "Module {0} does not contain plugin {1} type {2}"},		/* pman_plugin_notfound */
	{335545009, "Invalid usage of context namespace DDL_TRIGGER"},		/* sysf_invalid_trig_namespace */
	{335545010, "Value is NULL but isNull parameter was not informed"},		/* unexpected_null */
	{335545011, "Type {0} is incompatible with BLOB"},		/* type_notcompat_blob */
	{335545012, "Invalid date"},		/* invalid_date_val */
	{335545013, "Invalid time"},		/* invalid_time_val */
	{335545014, "Invalid timestamp"},		/* invalid_timestamp_val */
	{335545015, "Invalid index {0} in function {1}"},		/* invalid_index_val */
	{335545016, "{0}"},		/* formatted_exception */
	{335545017, "Asynchronous call is already running for this attachment"},		/* async_active */
	{335545018, "Function {0} is private to package {1}"},		/* private_function */
	{335545019, "Procedure {0} is private to package {1}"},		/* private_procedure */
	{335545020, "Request can't access new records in relation {0} and should be recompiled"},		/* request_outdated */
	{335545021, "invalid events id (handle)"},		/* bad_events_handle */
	{335545022, "Cannot copy statement {0}"},		/* cannot_copy_stmt */
	{335545023, "Invalid usage of boolean expression"},		/* invalid_boolean_usage */
	{335545024, "Arguments for {0} cannot both be zero"},		/* sysf_argscant_both_be_zero */
	{335545025, "missing service ID in spb"},		/* spb_no_id */
	{335545026, "External BLR message mismatch: invalid null descriptor at field {0}"},		/* ee_blr_mismatch_null */
	{335545027, "External BLR message mismatch: length = {0}, expected {1}"},		/* ee_blr_mismatch_length */
	{335545028, "Subscript {0} out of bounds [{1}, {2}]"},		/* ss_out_of_bounds */
	{335545029, "Install incomplete. To complete security database initialization please CREATE USER. For details read doc/README.security_database.txt."},		/* missing_data_structures */
	{335545030, "{0} operation is not allowed for system table {1}"},		/* protect_sys_tab */
	{335545031, "Libtommath error code {0} in function {1}"},		/* libtommath_generic */
	{335545032, "unsupported BLR version (expected between {0} and {1}, encountered {2})"},		/* wroblrver2 */
	{335545033, "expected length {0}, actual {1}"},		/* trunc_limits */
	{335545034, "Wrong info requested in isc_svc_query() for anonymous service"},		/* info_access */
	{335545035, "No isc_info_svc_stdin in user request, but service thread requested stdin data"},		/* svc_no_stdin */
	{335545036, "Start request for anonymous service is impossible"},		/* svc_start_failed */
	{335545037, "All services except for getting server log require switches"},		/* svc_no_switches */
	{335545038, "Size of stdin data is more than was requested from client"},		/* svc_bad_size */
	{335545039, "Crypt plugin {0} failed to load"},		/* no_crypt_plugin */
	{335545040, "Length of crypt plugin name should not exceed {0} bytes"},		/* cp_name_too_long */
	{335545041, "Crypt failed - already crypting database"},		/* cp_process_active */
	{335545042, "Crypt failed - database is already in requested state"},		/* cp_already_crypted */
	{335545043, "Missing crypt plugin, but page appears encrypted"},		/* decrypt_error */
	{335545044, "No providers loaded"},		/* no_providers */
	{335545045, "NULL data with non-zero SPB length"},		/* null_spb */
	{335545046, "Maximum ({0}) number of arguments exceeded for function {1}"},		/* max_args_exceeded */
	{335545047, "External BLR message mismatch: names count = {0}, blr count = {1}"},		/* ee_blr_mismatch_names_count */
	{335545048, "External BLR message mismatch: name {0} not found"},		/* ee_blr_mismatch_name_not_found */
	{335545049, "Invalid resultset interface"},		/* bad_result_set */
	{335545050, "Message length passed from user application does not match set of columns"},		/* wrong_message_length */
	{335545051, "Resultset is missing output format information"},		/* no_output_format */
	{335545052, "Message metadata not ready - item {0} is not finished"},		/* item_finish */
	{335545053, "Missing configuration file: {0}"},		/* miss_config */
	{335545054, "{0}: illegal line <{1}>"},		/* conf_line */
	{335545055, "Invalid include operator in {0} for <{1}>"},		/* conf_include */
	{335545056, "Include depth too big"},		/* include_depth */
	{335545057, "File to include not found"},		/* include_miss */
	{335545058, "Only the owner can change the ownership"},		/* protect_ownership */
	{335545059, "undefined variable number"},		/* badvarnum */
	{335545060, "Missing security context for {0}"},		/* sec_context */
	{335545061, "Missing segment {0} in multisegment connect block parameter"},		/* multi_segment */
	{335545062, "Different logins in connect and attach packets - client library error"},		/* login_changed */
	{335545063, "Exceeded exchange limit during authentication handshake"},		/* auth_handshake_limit */
	{335545064, "Incompatible wire encryption levels requested on client and server"},		/* wirecrypt_incompatible */
	{335545065, "Client attempted to attach unencrypted but wire encryption is required"},		/* miss_wirecrypt */
	{335545066, "Client attempted to start wire encryption using unknown key {0}"},		/* wirecrypt_key */
	{335545067, "Client attempted to start wire encryption using unsupported plugin {0}"},		/* wirecrypt_plugin */
	{335545068, "Error getting security database name from configuration file"},		/* secdb_name */
	{335545069, "Client authentication plugin is missing required data from server"},		/* auth_data */
	{335545070, "Client authentication plugin expected {1} bytes of {2} from server, got {0}"},		/* auth_datalength */
	{335545071, "Attempt to get information about an unprepared dynamic SQL statement."},		/* info_unprepared_stmt */
	{335545072, "Problematic key value is {0}"},		/* idx_key_value */
	{335545073, "Cannot select virtual table {0} for update WITH LOCK"},		/* forupdate_virtualtbl */
	{335545074, "Cannot select system table {0} for update WITH LOCK"},		/* forupdate_systbl */
	{335545075, "Cannot select temporary table {0} for update WITH LOCK"},		/* forupdate_temptbl */
	{335545076, "System {0} {1} cannot be modified"},		/* cant_modify_sysobj */
	{335545077, "Server misconfigured - contact administrator please"},		/* server_misconfigured */
	{335545078, "Deprecated backward compatibility ALTER ROLE ... SET/DROP AUTO ADMIN mapping may be used only for RDB$ADMIN role"},		/* alter_role */
	{335545079, "Mapping {0} already exists"},		/* map_already_exists */
	{335545080, "Mapping {0} does not exist"},		/* map_not_exists */
	{335545081, "{0} failed when loading mapping cache"},		/* map_load */
	{335545082, "Invalid name <*> in authentication block"},		/* map_aster */
	{335545083, "Multiple maps found for {0}"},		/* map_multi */
	{335545084, "Undefined mapping result - more than one different results found"},		/* map_undefined */
	{335545085, "Incompatible mode of attachment to damaged database"},		/* baddpb_damaged_mode */
	{335545086, "Attempt to set in database number of buffers which is out of acceptable range [{0}:{1}]"},		/* baddpb_buffers_range */
	{335545087, "Attempt to temporarily set number of buffers less than {0}"},		/* baddpb_temp_buffers */
	{335545088, "Global mapping is not available when database {0} is not present"},		/* map_nodb */
	{335545089, "Global mapping is not available when table RDB$MAP is not present in database {0}"},		/* map_notable */
	{335545090, "Your attachment has no trusted role"},		/* miss_trusted_role */
	{335545091, "Role {0} is invalid or unavailable"},		/* set_invalid_role */
	{335545092, "Cursor {0} is not positioned in a valid record"},		/* cursor_not_positioned */
	{335545093, "Duplicated user attribute {0}"},		/* dup_attribute */
	{335545094, "There is no privilege for this operation"},		/* dyn_no_priv */
	{335545095, "Using GRANT OPTION on {0} not allowed"},		/* dsql_cant_grant_option */
	{335545096, "read conflicts with concurrent update"},		/* read_conflict */
	{335545097, "{0} failed when working with CREATE DATABASE grants"},		/* crdb_load */
	{335545098, "CREATE DATABASE grants check is not possible when database {0} is not present"},		/* crdb_nodb */
	{335545099, "CREATE DATABASE grants check is not possible when table RDB$DB_CREATORS is not present in database {0}"},		/* crdb_notable */
	{335545100, "Interface {2} version too old: expected {0}, found {1}"},		/* interface_version_too_old */
	{335545101, "Input parameter mismatch for function {0}"},		/* fun_param_mismatch */
	{335545102, "Error during savepoint backout - transaction invalidated"},		/* savepoint_backout_err */
	{335545103, "Domain used in the PRIMARY KEY constraint of table {0} must be NOT NULL"},		/* domain_primary_key_notnull */
	{335545104, "CHARACTER SET {0} cannot be used as a attachment character set"},		/* invalid_attachment_charset */
	{335545105, "Some database(s) were shutdown when trying to read mapping data"},		/* map_down */
	{335545106, "Error occurred during login, please check server firebird.log for details"},		/* login_error */
	{335545107, "Database already opened with engine instance, incompatible with current"},		/* already_opened */
	{335545108, "Invalid crypt key {0}"},		/* bad_crypt_key */
	{335545109, "Page requires encryption but crypt plugin is missing"},		/* encrypt_error */
	{335545110, "Maximum index depth ({0} levels) is reached"},		/* max_idx_depth */
	{335545111, "System privilege {0} does not exist"},		/* wrong_prvlg */
	{335545112, "System privilege {0} is missing"},		/* miss_prvlg */
	{335545113, "Invalid or missing checksum of encrypted database"},		/* crypt_checksum */
	{335545114, "You must have SYSDBA rights at this server"},		/* not_dba */
	{335545115, "Cannot open cursor for non-SELECT statement"},		/* no_cursor */
	{335545116, "If <window frame bound 1> specifies {0}, then <window frame bound 2> shall not specify {1}"},		/* dsql_window_incompat_frames */
	{335545117, "RANGE based window with <expr> {PRECEDING | FOLLOWING} cannot have ORDER BY with more than one value"},		/* dsql_window_range_multi_key */
	{335545118, "RANGE based window with <offset> PRECEDING/FOLLOWING must have a single ORDER BY key of numerical, date, time or timestamp types"},		/* dsql_window_range_inv_key_type */
	{335545119, "Window RANGE/ROWS PRECEDING/FOLLOWING value must be of a numerical type"},		/* dsql_window_frame_value_inv_type */
	{335545120, "Invalid PRECEDING or FOLLOWING offset in window function: cannot be negative"},		/* window_frame_value_invalid */
	{335545121, "Window {0} not found"},		/* dsql_window_not_found */
	{335545122, "Cannot use PARTITION BY clause while overriding the window {0}"},		/* dsql_window_cant_overr_part */
	{335545123, "Cannot use ORDER BY clause while overriding the window {0} which already has an ORDER BY clause"},		/* dsql_window_cant_overr_order */
	{335545124, "Cannot override the window {0} because it has a frame clause. Tip: it can be used without parenthesis in OVER"},		/* dsql_window_cant_overr_frame */
	{335545125, "Duplicate window definition for {0}"},		/* dsql_window_duplicate */
	{335545126, "SQL statement is too long. Maximum size is {0} bytes."},		/* sql_too_long */
	{335545127, "Config level timeout expired."},		/* cfg_stmt_timeout */
	{335545128, "Attachment level timeout expired."},		/* att_stmt_timeout */
	{335545129, "Statement level timeout expired."},		/* req_stmt_timeout */
	{335545130, "Killed by database administrator."},		/* att_shut_killed */
	{335545131, "Idle timeout expired."},		/* att_shut_idle */
	{335545132, "Database is shutdown."},		/* att_shut_db_down */
	{335545133, "Engine is shutdown."},		/* att_shut_engine */
	{335545134, "OVERRIDING clause can be used only when an identity column is present in the INSERT's field list for table/view {0}"},		/* overriding_without_identity */
	{335545135, "OVERRIDING SYSTEM VALUE can be used only for identity column defined as 'GENERATED ALWAYS' in INSERT for table/view {0}"},		/* overriding_system_invalid */
	{335545136, "OVERRIDING USER VALUE can be used only for identity column defined as 'GENERATED BY DEFAULT' in INSERT for table/view {0}"},		/* overriding_user_invalid */
	{335545137, "OVERRIDING clause should be used when an identity column defined as 'GENERATED ALWAYS' is present in the INSERT's field list for table table/view {0}"},		/* overriding_missing */
	{335545138, "DecFloat precision must be 16 or 34"},		/* decprecision_err */
	{335545139, "Decimal float divide by zero.  The code attempted to divide a DECFLOAT value by zero."},		/* decfloat_divide_by_zero */
	{335545140, "Decimal float inexact result.  The result of an operation cannot be represented as a decimal fraction."},		/* decfloat_inexact_result */
	{335545141, "Decimal float invalid operation.  An indeterminant error occurred during an operation."},		/* decfloat_invalid_operation */
	{335545142, "Decimal float overflow.  The exponent of a result is greater than the magnitude allowed."},		/* decfloat_overflow */
	{335545143, "Decimal float underflow.  The exponent of a result is less than the magnitude allowed."},		/* decfloat_underflow */
	{335545144, "Sub-function {0} has not been defined"},		/* subfunc_notdef */
	{335545145, "Sub-procedure {0} has not been defined"},		/* subproc_notdef */
	{335545146, "Sub-function {0} has a signature mismatch with its forward declaration"},		/* subfunc_signat */
	{335545147, "Sub-procedure {0} has a signature mismatch with its forward declaration"},		/* subproc_signat */
	{335545148, "Default values for parameters are not allowed in definition of the previously declared sub-function {0}"},		/* subfunc_defvaldecl */
	{335545149, "Default values for parameters are not allowed in definition of the previously declared sub-procedure {0}"},		/* subproc_defvaldecl */
	{335545150, "Sub-function {0} was declared but not implemented"},		/* subfunc_not_impl */
	{335545151, "Sub-procedure {0} was declared but not implemented"},		/* subproc_not_impl */
	{335545152, "Invalid HASH algorithm {0}"},		/* sysf_invalid_hash_algorithm */
	{335545153, "Expression evaluation error for index \"{0}\" on table \"{1}\""},		/* expression_eval_index */
	{335545154, "Invalid decfloat trap state {0}"},		/* invalid_decfloat_trap */
	{335545155, "Invalid decfloat rounding mode {0}"},		/* invalid_decfloat_round */
	{335545156, "Invalid part {0} to calculate the {0} of a DATE/TIMESTAMP"},		/* sysf_invalid_first_last_part */
	{335545157, "Expected DATE/TIMESTAMP value in {0}"},		/* sysf_invalid_date_timestamp */
	{335545158, "Precision must be from {0} to {1}"},		/* precision_err2 */
	{335545159, "invalid batch handle"},		/* bad_batch_handle */
	{335545160, "Bad international character in tag {0}"},		/* intl_char */
	{335545161, "Null data in parameters block with non-zero length"},		/* null_block */
	{335545162, "Items working with running service and getting generic server information should not be mixed in single info block"},		/* mixed_info */
	{335545163, "Unknown information item, code {0}"},		/* unknown_info */
	{335545164, "Wrong version of blob parameters block {0}, should be {1}"},		/* bpb_version */
	{335545165, "User management plugin is missing or failed to load"},		/* user_manager */
	{335545166, "Missing entrypoint {0} in ICU library"},		/* icu_entrypoint */
	{335545167, "Could not find acceptable ICU library"},		/* icu_library */
	{335545168, "Name {0} not found in system MetadataBuilder"},		/* metadata_name */
	{335545169, "Parse to tokens error"},		/* tokens_parse */
	{335545170, "Error opening international conversion descriptor from {0} to {1}"},		/* iconv_open */
	{335545171, "Message {0} is out of range, only {1} messages in batch"},		/* batch_compl_range */
	{335545172, "Detailed error info for message {0} is missing in batch"},		/* batch_compl_detail */
	{335545173, "Compression stream init error {0}"},		/* deflate_init */
	{335545174, "Decompression stream init error {0}"},		/* inflate_init */
	{335545175, "Segment size ({0}) should not exceed 65535 (64K - 1) when using segmented blob"},		/* big_segment */
	{335545176, "Invalid blob policy in the batch for {0}() call"},		/* batch_policy */
	{335545177, "Can't change default BPB after adding any data to batch"},		/* batch_defbpb */
	{335545178, "Unexpected info buffer structure querying for server batch parameters"},		/* batch_align */
	{335545179, "Duplicated segment {0} in multisegment connect block parameter"},		/* multi_segment_dup */
	{335545180, "Plugin not supported by network protocol"},		/* non_plugin_protocol */
	{335545181, "Error parsing message format"},		/* message_format */
	{335545182, "Wrong version of batch parameters block {0}, should be {1}"},		/* batch_param_version */
	{335545183, "Message size ({0}) in batch exceeds internal buffer size ({1})"},		/* batch_msg_long */
	{335545184, "Batch already opened for this statement"},		/* batch_open */
	{335545185, "Invalid type of statement used in batch"},		/* batch_type */
	{335545186, "Statement used in batch must have parameters"},		/* batch_param */
	{335545187, "There are no blobs in associated with batch statement"},		/* batch_blobs */
	{335545188, "appendBlobData() is used to append data to last blob but no such blob was added to the batch"},		/* batch_blob_append */
	{335545189, "Portions of data, passed as blob stream, should have size multiple to the alignment required for blobs"},		/* batch_stream_align */
	{335545190, "Repeated blob id {0} in registerBlob()"},		/* batch_rpt_blob */
	{335545191, "Blob buffer format error"},		/* batch_blob_buf */
	{335545192, "Unusable (too small) data remained in {0} buffer"},		/* batch_small_data */
	{335545193, "Blob continuation should not contain BPB"},		/* batch_cont_bpb */
	{335545194, "Size of BPB ({0}) greater than remaining data ({1})"},		/* batch_big_bpb */
	{335545195, "Size of segment ({0}) greater than current BLOB data ({1})"},		/* batch_big_segment */
	{335545196, "Size of segment ({0}) greater than available data ({1})"},		/* batch_big_seg2 */
	{335545197, "Unknown blob ID {0} in the batch message"},		/* batch_blob_id */
	{335545198, "Internal buffer overflow - batch too big"},		/* batch_too_big */
	{335545199, "Numeric literal too long"},		/* num_literal */
	{335545200, "Error using events in mapping shared memory: {0}"},		/* map_event */
	{335545201, "Global mapping memory overflow"},		/* map_overflow */
	{335545202, "Header page overflow - too many clumplets on it"},		/* hdr_overflow */
	{335545203, "No matching client/server authentication plugins configured for execute statement in embedded datasource"},		/* vld_plugins */
	{335545204, "Missing database encryption key for your attachment"},		/* db_crypt_key */
	{335545205, "Key holder plugin {0} failed to load"},		/* no_keyholder_plugin */
	{335545206, "Cannot reset user session"},		/* ses_reset_err */
	{335545207, "There are open transactions ({0} active)"},		/* ses_reset_open_trans */
	{335545208, "Session was reset with warning(s)"},		/* ses_reset_warn */
	{335545209, "Transaction is rolled back due to session reset, all changes are lost"},		/* ses_reset_tran_rollback */
	{335545210, "Plugin {0}:"},		/* plugin_name */
	{335545211, "PARAMETER {0}"},		/* parameter_name */
	{335545212, "Starting page number for file {0} must be {1} or greater"},		/* file_starting_page_err */
	{335545213, "Invalid time zone offset: {0} - must use format +/-hours:minutes and be between -14:00 and +14:00"},		/* invalid_timezone_offset */
	{335545214, "Invalid time zone region: {0}"},		/* invalid_timezone_region */
	{335545215, "Invalid time zone ID: {0}"},		/* invalid_timezone_id */
	{335545216, "Wrong base64 text length {0}, should be multiple of 4"},		/* tom_decode64len */
	{335545217, "Invalid first parameter datatype - need string or blob"},		/* tom_strblob */
	{335545218, "Error registering {0} - probably bad tomcrypt library"},		/* tom_reg */
	{335545219, "Unknown crypt algorithm {0} in USING clause"},		/* tom_algorithm */
	{335545220, "Should specify mode parameter for symmetric cipher"},		/* tom_mode_miss */
	{335545221, "Unknown symmetric crypt mode specified"},		/* tom_mode_bad */
	{335545222, "Mode parameter makes no sense for chosen cipher"},		/* tom_no_mode */
	{335545223, "Should specify initialization vector (IV) for chosen cipher and/or mode"},		/* tom_iv_miss */
	{335545224, "Initialization vector (IV) makes no sense for chosen cipher and/or mode"},		/* tom_no_iv */
	{335545225, "Invalid counter endianess {0}"},		/* tom_ctrtype_bad */
	{335545226, "Counter endianess parameter is not used in mode {0}"},		/* tom_no_ctrtype */
	{335545227, "Too big counter value {0}, maximum {1} can be used"},		/* tom_ctr_big */
	{335545228, "Counter length/value parameter is not used with {0} {1}"},		/* tom_no_ctr */
	{335545229, "Invalid initialization vector (IV) length {0}, need {1}"},		/* tom_iv_length */
	{335545230, "TomCrypt library error: {0}"},		/* tom_error */
	{335545231, "Starting PRNG yarrow"},		/* tom_yarrow_start */
	{335545232, "Setting up PRNG yarrow"},		/* tom_yarrow_setup */
	{335545233, "Initializing {0} mode"},		/* tom_init_mode */
	{335545234, "Encrypting in {0} mode"},		/* tom_crypt_mode */
	{335545235, "Decrypting in {0} mode"},		/* tom_decrypt_mode */
	{335545236, "Initializing cipher {0}"},		/* tom_init_cip */
	{335545237, "Encrypting using cipher {0}"},		/* tom_crypt_cip */
	{335545238, "Decrypting using cipher {0}"},		/* tom_decrypt_cip */
	{335545239, "Setting initialization vector (IV) for {0}"},		/* tom_setup_cip */
	{335545240, "Invalid initialization vector (IV) length {0}, need  8 or 12"},		/* tom_setup_chacha */
	{335545241, "Encoding {0}"},		/* tom_encode */
	{335545242, "Decoding {0}"},		/* tom_decode */
	{335545243, "Importing RSA key"},		/* tom_rsa_import */
	{335545244, "Invalid OAEP packet"},		/* tom_oaep */
	{335545245, "Unknown hash algorithm {0}"},		/* tom_hash_bad */
	{335545246, "Making RSA key"},		/* tom_rsa_make */
	{335545247, "Exporting {0} RSA key"},		/* tom_rsa_export */
	{335545248, "RSA-signing data"},		/* tom_rsa_sign */
	{335545249, "Verifying RSA-signed data"},		/* tom_rsa_verify */
	{335545250, "Invalid key length {0}, need 16 or 32"},		/* tom_chacha_key */
	{335545251, "invalid replicator handle"},		/* bad_repl_handle */
	{335545252, "Transaction's base snapshot number does not exist"},		/* tra_snapshot_does_not_exist */
	{335545253, "Input parameter '{0}' is not used in SQL query text"},		/* eds_input_prm_not_used */
	{335545254, "Effective user is {0}"},		/* effective_user */
	{335545255, "Invalid time zone bind mode {0}"},		/* invalid_time_zone_bind */
	{335545256, "Invalid decfloat bind mode {0}"},		/* invalid_decfloat_bind */
	{335545257, "Invalid hex text length {0}, should be multiple of 2"},		/* odd_hex_len */
	{335545258, "Invalid hex digit {0} at position {1}"},		/* invalid_hex_digit */
	{335545259, "Error processing isc_dpb_set_bind clumplet \"{0}\""},		/* bind_err */
	{335545260, "The following statement failed: {0}"},		/* bind_statement */
	{335545261, "Can not convert {0} to {1}"},		/* bind_convert */
	{335545262, "cannot update old BLOB"},		/* cannot_update_old_blob */
	{335545263, "cannot read from new BLOB"},		/* cannot_read_new_blob */
	{335545264, "No permission for CREATE {0} operation"},		/* dyn_no_create_priv */
	{335545265, "SUSPEND could not be used without RETURNS clause in PROCEDURE or EXECUTE BLOCK"},		/* suspend_without_returns */
	{335545266, "String truncated warning due to the following reason"},		/* truncate_warn */
	{335545267, "Monitoring data does not fit into the field"},		/* truncate_monitor */
	{335545268, "Engine data does not fit into return value of system function"},		/* truncate_context */
	{335545269, "Multiple source records cannot match the same target during MERGE"},		/* merge_dup_update */
	{335545270, "RDB$PAGES written by non-system transaction, DB appears to be damaged"},		/* wrong_page */
	{335545271, "Replication error"},		/* repl_error */
	{335545272, "Reset of user session failed. Connection is shut down."},		/* ses_reset_failed */
	{335545273, "File size is less than expected"},		/* block_size */
	{335545274, "Invalid key length {0}, need >{1}"},		/* tom_key_length */
	{335545275, "Invalid information arguments"},		/* inf_invalid_args */
	{335545276, "Empty or NULL parameter {0} is not accepted"},		/* sysf_invalid_null_empty */
	{335545277, "Undefined local table number {0}"},		/* bad_loctab_num */
	{335545278, "Invalid text <{0}> after quoted string"},		/* quoted_str_bad */
	{335545279, "Missing terminating quote <{0}> in the end of quoted string"},		/* quoted_str_miss */
	{335545280, "{0}: inconsistent shared memory type/version; found {1}, expected {2}"},		/* wrong_shmem_ver */
	{335545281, "{0}-bit engine can't open database already opened by {1}-bit engine"},		/* wrong_shmem_bitness */
	{335545282, "Procedures cannot specify access type other than NATURAL in the plan"},		/* wrong_proc_plan */
	{335545283, "Invalid RDB$BLOB_UTIL handle"},		/* invalid_blob_util_handle */
	{335545284, "Invalid temporary BLOB ID"},		/* bad_temp_blob_id */
	{335545285, "ODS upgrade failed while adding new system {0}"},		/* ods_upgrade_err */
	{335545286, "Wrong parallel workers value {0}, valid range are from 1 to {1}"},		/* bad_par_workers */
	{335545287, "Definition of index expression is not found for index {0}"},		/* idx_expr_not_found */
	{335545288, "Definition of index condition is not found for index {0}"},		/* idx_cond_not_found */
	{335740929, "data base file name ({0}) already given"},		/* gfix_db_name */
	{335740930, "invalid switch {0}"},		/* gfix_invalid_sw */
	{335740931, "gfix version {0}"},		/* gfix_version */
	{335740932, "incompatible switch combination"},		/* gfix_incmp_sw */
	{335740933, "replay log pathname required"},		/* gfix_replay_req */
	{335740934, "number of page buffers for cache required"},		/* gfix_pgbuf_req */
	{335740935, "numeric value required"},		/* gfix_val_req */
	{335740936, "positive numeric value required"},		/* gfix_pval_req */
	{335740937, "number of transactions per sweep required"},		/* gfix_trn_req */
	{335740938, "transaction number or \"all\" required"},		/* gfix_trn_all_req */
	{335740939, "\"sync\" or \"async\" required"},		/* gfix_sync_req */
	{335740940, "\"full\" or \"reserve\" required"},		/* gfix_full_req */
	{335740941, "user name required"},		/* gfix_usrname_req */
	{335740942, "password required"},		/* gfix_pass_req */
	{335740943, "subsystem name"},		/* gfix_subs_name */
	{335740944, "\"wal\" required"},		/* gfix_wal_req */
	{335740945, "number of seconds required"},		/* gfix_sec_req */
	{335740946, "numeric value between 0 and 32767 inclusive required"},		/* gfix_nval_req */
	{335740947, "must specify type of shutdown"},		/* gfix_type_shut */
	{335740948, "please retry, specifying an option"},		/* gfix_retry */
	{335740949, "plausible options are:"},		/* gfix_opt */
	{335740950, "\\n    Options can be abbreviated to the unparenthesized characters"},		/* gfix_qualifiers */
	{335740951, "please retry, giving a database name"},		/* gfix_retry_db */
	{335740952, "Summary of validation errors"},		/* gfix_summary */
	{335740953, "   -ac(tivate_shadow)   activate shadow file for database usage"},		/* gfix_opt_active */
	{335740954, "   -at(tach)            shutdown new database attachments"},		/* gfix_opt_attach */
	{335740955, "\t-begin_log\tbegin logging for replay utility"},		/* gfix_opt_begin_log */
	{335740956, "   -b(uffers)           set page buffers <n>"},		/* gfix_opt_buffers */
	{335740957, "   -co(mmit)            commit transaction <tr / all>"},		/* gfix_opt_commit */
	{335740958, "   -ca(che)             shutdown cache manager"},		/* gfix_opt_cache */
	{335740959, "\t-disable\tdisable WAL"},		/* gfix_opt_disable */
	{335740960, "   -fu(ll)              validate record fragments (-v)"},		/* gfix_opt_full */
	{335740961, "   -fo(rce_shutdown)    force database shutdown"},		/* gfix_opt_force */
	{335740962, "   -h(ousekeeping)      set sweep interval <n>"},		/* gfix_opt_housekeep */
	{335740963, "   -i(gnore)            ignore checksum errors"},		/* gfix_opt_ignore */
	{335740964, "   -k(ill_shadow)       kill all unavailable shadow files"},		/* gfix_opt_kill */
	{335740965, "   -l(ist)              show limbo transactions"},		/* gfix_opt_list */
	{335740966, "   -me(nd)              prepare corrupt database for backup"},		/* gfix_opt_mend */
	{335740967, "   -n(o_update)         read-only validation (-v)"},		/* gfix_opt_no_update */
	{335740968, "   -o(nline)            database online <single / multi / normal>"},		/* gfix_opt_online */
	{335740969, "   -pr(ompt)            prompt for commit/rollback (-l)"},		/* gfix_opt_prompt */
	{335740970, "   -pa(ssword)          default password"},		/* gfix_opt_password */
	{335740971, "\t-quit_log\tquit logging for replay utility"},		/* gfix_opt_quit_log */
	{335740972, "   -r(ollback)          rollback transaction <tr / all>"},		/* gfix_opt_rollback */
	{335740973, "   -sw(eep)             force garbage collection"},		/* gfix_opt_sweep */
	{335740974, "   -sh(utdown)          shutdown <full / single / multi>"},		/* gfix_opt_shut */
	{335740975, "   -tw(o_phase)         perform automated two-phase recovery"},		/* gfix_opt_two_phase */
	{335740976, "   -tra(nsaction)       shutdown transaction startup"},		/* gfix_opt_tran */
	{335740977, "   -u(se)               use full or reserve space for versions"},		/* gfix_opt_use */
	{335740978, "   -user                default user name"},		/* gfix_opt_user */
	{335740979, "   -v(alidate)          validate database structure"},		/* gfix_opt_validate */
	{335740980, "   -w(rite)             write synchronously or asynchronously"},		/* gfix_opt_write */
	{335740981, "   -x                   set debug on"},		/* gfix_opt_x */
	{335740982, "   -z                   print software version number"},		/* gfix_opt_z */
	{335740983, "\\n\tNumber of record level errors\t: {0}"},		/* gfix_rec_err */
	{335740984, "\tNumber of Blob page errors\t: {0}"},		/* gfix_blob_err */
	{335740985, "\tNumber of data page errors\t: {0}"},		/* gfix_data_err */
	{335740986, "\tNumber of index page errors\t: {0}"},		/* gfix_index_err */
	{335740987, "\tNumber of pointer page errors\t: {0}"},		/* gfix_pointer_err */
	{335740988, "\tNumber of transaction page errors\t: {0}"},		/* gfix_trn_err */
	{335740989, "\tNumber of database page errors\t: {0}"},		/* gfix_db_err */
	{335740990, "bad block type"},		/* gfix_bad_block */
	{335740991, "internal block exceeds maximum size"},		/* gfix_exceed_max */
	{335740992, "corrupt pool"},		/* gfix_corrupt_pool */
	{335740993, "virtual memory exhausted"},		/* gfix_mem_exhausted */
	{335740994, "bad pool id"},		/* gfix_bad_pool */
	{335740995, "Transaction state {0} not in valid range."},		/* gfix_trn_not_valid */
	{335740996, "ATTACH_DATABASE: attempted attach of {0},"},		/* gfix_dbg_attach */
	{335740997, " failed"},		/* gfix_dbg_failed */
	{335740998, " succeeded"},		/* gfix_dbg_success */
	{335740999, "Transaction {0} is in limbo."},		/* gfix_trn_limbo */
	{335741000, "More limbo transactions than fit.  Try again"},		/* gfix_try_again */
	{335741001, "Unrecognized info item {0}"},		/* gfix_unrec_item */
	{335741002, "A commit of transaction {0} will violate two-phase commit."},		/* gfix_commit_violate */
	{335741003, "A rollback of transaction {0} is needed to preserve two-phase commit."},		/* gfix_preserve */
	{335741004, "Transaction {0} has already been partially committed."},		/* gfix_part_commit */
	{335741005, "A rollback of this transaction will violate two-phase commit."},		/* gfix_rback_violate */
	{335741006, "Transaction {0} has been partially committed."},		/* gfix_part_commit2 */
	{335741007, "A commit is necessary to preserve the two-phase commit."},		/* gfix_commit_pres */
	{335741008, "Insufficient information is available to determine"},		/* gfix_insuff_info */
	{335741009, "a proper action for transaction {0}."},		/* gfix_action */
	{335741010, "Transaction {0}: All subtransactions have been prepared."},		/* gfix_all_prep */
	{335741011, "Either commit or rollback is possible."},		/* gfix_comm_rback */
	{335741012, "unexpected end of input"},		/* gfix_unexp_eoi */
	{335741013, "Commit, rollback, or neither (c, r, or n)?"},		/* gfix_ask */
	{335741014, "Could not reattach to database for transaction {0}."},		/* gfix_reattach_failed */
	{335741015, "Original path: {0}"},		/* gfix_org_path */
	{335741016, "Enter a valid path:"},		/* gfix_enter_path */
	{335741017, "Attach unsuccessful."},		/* gfix_att_unsucc */
	{335741018, "failed to reconnect to a transaction in database {0}"},		/* gfix_recon_fail */
	{335741019, "Transaction {0}:"},		/* gfix_trn2 */
	{335741020, "  Multidatabase transaction:"},		/* gfix_mdb_trn */
	{335741021, "    Host Site: {0}"},		/* gfix_host_site */
	{335741022, "    Transaction {0}"},		/* gfix_trn */
	{335741023, "has been prepared."},		/* gfix_prepared */
	{335741024, "has been committed."},		/* gfix_committed */
	{335741025, "has been rolled back."},		/* gfix_rolled_back */
	{335741026, "is not available."},		/* gfix_not_available */
	{335741027, "is not found, assumed not prepared."},		/* gfix_not_prepared */
	{335741028, "is not found, assumed to be committed."},		/* gfix_be_committed */
	{335741029, "        Remote Site: {0}"},		/* gfix_rmt_site */
	{335741030, "        Database Path: {0}"},		/* gfix_db_path */
	{335741031, "  Automated recovery would commit this transaction."},		/* gfix_auto_comm */
	{335741032, "  Automated recovery would rollback this transaction."},		/* gfix_auto_rback */
	{335741033, "Warning: Multidatabase transaction is in inconsistent state for recovery."},		/* gfix_warning */
	{335741034, "Transaction {0} was committed, but prior ones were rolled back."},		/* gfix_trn_was_comm */
	{335741035, "Transaction {0} was rolled back, but prior ones were committed."},		/* gfix_trn_was_rback */
	{335741036, "Transaction description item unknown"},		/* gfix_trn_unknown */
	{335741037, "   -mo(de)              read_only or read_write database"},		/* gfix_opt_mode */
	{335741038, "\"read_only\" or \"read_write\" required"},		/* gfix_mode_req */
	{335741039, "   -sq(l_dialect)       set database dialect n"},		/* gfix_opt_SQL_dialect */
	{335741040, "database SQL dialect must be one of '{0}'"},		/* gfix_SQL_dialect */
	{335741041, "dialect number required"},		/* gfix_dialect_req */
	{335741042, "positive or zero numeric value required"},		/* gfix_pzval_req */
	{335741043, "   -tru(sted)           use trusted authentication"},		/* gfix_opt_trusted */
	{335741044, "could not open password file {0}, errno {1}"},
	{335741045, "could not read password file {0}, errno {1}"},
	{335741046, "empty password file {0}"},
	{335741047, "   -fe(tch_password)    fetch password from file"},
	{335741048, "usage: gfix [options] <database>"},
	{335741049, "   -nol(inger)          close database ignoring linger setting for it"},		/* gfix_opt_nolinger */
	{335741050, "\tNumber of inventory page errors\t: {0}"},		/* gfix_pip_err */
	{335741051, "\tNumber of record level warnings\t: {0}"},		/* gfix_rec_warn */
	{335741052, "\tNumber of blob page warnings\t: {0}"},		/* gfix_blob_warn */
	{335741053, "\tNumber of data page warnings\t: {0}"},		/* gfix_data_warn */
	{335741054, "\tNumber of index page warnings\t: {0}"},		/* gfix_index_warn */
	{335741055, "\tNumber of pointer page warnings\t: {0}"},		/* gfix_pointer_warn */
	{335741056, "\tNumber of transaction page warnings\t: {0}"},		/* gfix_trn_warn */
	{335741057, "\tNumber of database page warnings\t: {0}"},		/* gfix_db_warn */
	{335741058, "\tNumber of inventory page warnings\t: {0}"},		/* gfix_pip_warn */
	{335741059, "   -icu                 fix database to be usable with present ICU version"},		/* gfix_opt_icu */
	{335741060, "   -role                set SQL role name"},		/* gfix_opt_role */
	{335741061, "SQL role name required"},		/* gfix_role_req */
	{335741062, "   -repl(ica)           replica mode <none / read_only / read_write>"},		/* gfix_opt_repl */
	{335741063, "replica mode (none / read_only / read_write) required"},		/* gfix_repl_mode_req */
	{335741064, "   -par(allel)          parallel workers <n> (-sweep, -icu)"},		/* gfix_opt_parallel */
	{335741065, "   -up(grade)           upgrade database ODS"},		/* gfix_opt_upgrade */
	{336003074, "Cannot SELECT RDB$DB_KEY from a stored procedure."},		/* dsql_dbkey_from_non_table */
	{336003075, "Precision 10 to 18 changed from DOUBLE PRECISION in SQL dialect 1 to 64-bit scaled integer in SQL dialect 3"},		/* dsql_transitional_numeric */
	{336003076, "Use of {0} expression that returns different results in dialect 1 and dialect 3"},		/* dsql_dialect_warning_expr */
	{336003077, "Database SQL dialect {0} does not support reference to {1} datatype"},		/* sql_db_dialect_dtype_unsupport */
	{336003078, ""},
	{336003079, "DB dialect {0} and client dialect {1} conflict with respect to numeric precision {2}."},		/* sql_dialect_conflict_num */
	{336003080, "WARNING: Numeric literal {0} is interpreted as a floating-point"},		/* dsql_warning_number_ambiguous */
	{336003081, "value in SQL dialect 1, but as an exact numeric value in SQL dialect 3."},		/* dsql_warning_number_ambiguous1 */
	{336003082, "WARNING: NUMERIC and DECIMAL fields with precision 10 or greater are stored"},		/* dsql_warn_precision_ambiguous */
	{336003083, "as approximate floating-point values in SQL dialect 1, but as 64-bit"},		/* dsql_warn_precision_ambiguous1 */
	{336003084, "integers in SQL dialect 3."},		/* dsql_warn_precision_ambiguous2 */
	{336003085, "Ambiguous field name between {0} and {1}"},		/* dsql_ambiguous_field_name */
	{336003086, "External function should have return position between 1 and {0}"},		/* dsql_udf_return_pos_err */
	{336003087, "Label {0} {1} in the current scope"},		/* dsql_invalid_label */
	{336003088, "Datatypes {0}are not comparable in expression {1}"},		/* dsql_datatypes_not_comparable */
	{336003089, "Empty cursor name is not allowed"},		/* dsql_cursor_invalid */
	{336003090, "Statement already has a cursor {0} assigned"},		/* dsql_cursor_redefined */
	{336003091, "Cursor {0} is not found in the current context"},		/* dsql_cursor_not_found */
	{336003092, "Cursor {0} already exists in the current context"},		/* dsql_cursor_exists */
	{336003093, "Relation {0} is ambiguous in cursor {1}"},		/* dsql_cursor_rel_ambiguous */
	{336003094, "Relation {0} is not found in cursor {1}"},		/* dsql_cursor_rel_not_found */
	{336003095, "Cursor is not open"},		/* dsql_cursor_not_open */
	{336003096, "Data type {0} is not supported for EXTERNAL TABLES. Relation '{1}', field '{2}'"},		/* dsql_type_not_supp_ext_tab */
	{336003097, "Feature not supported on ODS version older than {0}.{1}"},		/* dsql_feature_not_supported_ods */
	{336003098, "Primary key required on table {0}"},		/* primary_key_required */
	{336003099, "UPDATE OR INSERT field list does not match primary key of table {0}"},		/* upd_ins_doesnt_match_pk */
	{336003100, "UPDATE OR INSERT field list does not match MATCHING clause"},		/* upd_ins_doesnt_match_matching */
	{336003101, "UPDATE OR INSERT without MATCHING could not be used with views based on more than one table"},		/* upd_ins_with_complex_view */
	{336003102, "Incompatible trigger type"},		/* dsql_incompatible_trigger_type */
	{336003103, "Database trigger type can't be changed"},		/* dsql_db_trigger_type_cant_change */
	{336003104, "To be used with RDB$RECORD_VERSION, {0} must be a table or a view of single table"},		/* dsql_record_version_table */
	{336003105, "SQLDA version expected between {0} and {1}, found {2}"},		/* dsql_invalid_sqlda_version */
	{336003106, "at SQLVAR index {0}"},		/* dsql_sqlvar_index */
	{336003107, "empty pointer to NULL indicator variable"},		/* dsql_no_sqlind */
	{336003108, "empty pointer to data"},		/* dsql_no_sqldata */
	{336003109, "No SQLDA for input values provided"},		/* dsql_no_input_sqlda */
	{336003110, "No SQLDA for output values provided"},		/* dsql_no_output_sqlda */
	{336003111, "Wrong number of parameters (expected {0}, got {1})"},		/* dsql_wrong_param_num */
	{336003112, "Invalid DROP SQL SECURITY clause"},		/* dsql_invalid_drop_ss_clause */
	{336003113, "UPDATE OR INSERT value for field {0}, part of the implicit or explicit MATCHING clause, cannot be DEFAULT"},		/* upd_ins_cannot_default */
	{336068609, "ODS version not supported by DYN"},
	{336068610, "unsupported DYN verb"},
	{336068611, "STORE RDB$FIELD_DIMENSIONS failed"},
	{336068612, "unsupported DYN verb"},
	{336068613, "{0}"},
	{336068614, "unsupported DYN verb"},
	{336068615, "DEFINE BLOB FILTER failed"},
	{336068616, "DEFINE GENERATOR failed"},
	{336068617, "DEFINE GENERATOR unexpected DYN verb"},
	{336068618, "DEFINE FUNCTION failed"},
	{336068619, "unsupported DYN verb"},
	{336068620, "DEFINE FUNCTION ARGUMENT failed"},
	{336068621, "STORE RDB$FIELDS failed"},
	{336068622, "No table specified for index"},
	{336068623, "STORE RDB$INDEX_SEGMENTS failed"},
	{336068624, "unsupported DYN verb"},
	{336068625, "PRIMARY KEY column lookup failed"},
	{336068626, "could not find UNIQUE or PRIMARY KEY constraint in table {0} with specified columns"},
	{336068627, "PRIMARY KEY lookup failed"},
	{336068628, "could not find PRIMARY KEY index in specified table {0}"},
	{336068629, "STORE RDB$INDICES failed"},
	{336068630, "STORE RDB$FIELDS failed"},
	{336068631, "STORE RDB$RELATION_FIELDS failed"},
	{336068632, "STORE RDB$RELATIONS failed"},
	{336068633, "STORE RDB$USER_PRIVILEGES failed defining a table"},
	{336068634, "unsupported DYN verb"},
	{336068635, "STORE RDB$RELATIONS failed"},
	{336068636, "STORE RDB$FIELDS failed"},
	{336068637, "STORE RDB$RELATION_FIELDS failed"},
	{336068638, "unsupported DYN verb"},
	{336068639, "DEFINE TRIGGER failed"},
	{336068640, "unsupported DYN verb"},
	{336068641, "DEFINE TRIGGER MESSAGE failed"},
	{336068642, "STORE RDB$VIEW_RELATIONS failed"},
	{336068643, "ERASE RDB$FIELDS failed"},
	{336068644, "ERASE BLOB FILTER failed"},
	{336068645, "BLOB Filter {0} not found"},		/* dyn_filter_not_found */
	{336068646, "unsupported DYN verb"},
	{336068647, "ERASE RDB$FUNCTION_ARGUMENTS failed"},
	{336068648, "ERASE RDB$FUNCTIONS failed"},
	{336068649, "Function {0} not found"},		/* dyn_func_not_found */
	{336068650, "unsupported DYN verb"},
	{336068651, "Domain {0} is used in table {1} (local name {2}) and cannot be dropped"},
	{336068652, "ERASE RDB$FIELDS failed"},
	{336068653, "ERASE RDB$FIELDS failed"},
	{336068654, "Column not found"},
	{336068655, "ERASE RDB$INDICES failed"},
	{336068656, "Index not found"},		/* dyn_index_not_found */
	{336068657, "ERASE RDB$INDEX_SEGMENTS failed"},
	{336068658, "No segments found for index"},
	{336068659, "No table specified in ERASE RFR"},
	{336068660, "Column {0} from table {1} is referenced in view {2}"},
	{336068661, "ERASE RDB$RELATION_FIELDS failed"},
	{336068662, "View {0} not found"},		/* dyn_view_not_found */
	{336068663, "Column not found for table"},
	{336068664, "ERASE RDB$INDEX_SEGMENTS failed"},
	{336068665, "ERASE RDB$INDICES failed"},
	{336068666, "ERASE RDB$RELATION_FIELDS failed"},
	{336068667, "ERASE RDB$VIEW_RELATIONS failed"},
	{336068668, "ERASE RDB$RELATIONS failed"},
	{336068669, "Table not found"},
	{336068670, "ERASE RDB$USER_PRIVILEGES failed"},
	{336068671, "ERASE RDB$FILES failed"},
	{336068672, "unsupported DYN verb"},
	{336068673, "ERASE RDB$TRIGGER_MESSAGES failed"},
	{336068674, "ERASE RDB$TRIGGERS failed"},
	{336068675, "Trigger not found"},
	{336068676, "MODIFY RDB$VIEW_RELATIONS failed"},
	{336068677, "unsupported DYN verb"},
	{336068678, "TRIGGER NAME expected"},
	{336068679, "ERASE TRIGGER MESSAGE failed"},
	{336068680, "Trigger Message not found"},
	{336068681, "unsupported DYN verb"},
	{336068682, "ERASE RDB$SECURITY_CLASSES failed"},
	{336068683, "Security class not found"},
	{336068684, "unsupported DYN verb"},
	{336068685, "SELECT RDB$USER_PRIVILEGES failed in grant"},
	{336068686, "SELECT RDB$USER_PRIVILEGES failed in grant"},
	{336068687, "STORE RDB$USER_PRIVILEGES failed in grant"},
	{336068688, "Specified domain or source column does not exist"},
	{336068689, "Generation of column name failed"},
	{336068690, "Generation of index name failed"},
	{336068691, "Generation of trigger name failed"},
	{336068692, "MODIFY DATABASE failed"},
	{336068693, "MODIFY RDB$CHARACTER_SETS failed"},
	{336068694, "MODIFY RDB$COLLATIONS failed"},
	{336068695, "MODIFY RDB$FIELDS failed"},
	{336068696, "MODIFY RDB$BLOB_FILTERS failed"},
	{336068697, "Domain not found"},		/* dyn_domain_not_found */
	{336068698, "unsupported DYN verb"},
	{336068699, "MODIFY RDB$INDICES failed"},
	{336068700, "MODIFY RDB$FUNCTIONS failed"},
	{336068701, "Index column not found"},
	{336068702, "MODIFY RDB$GENERATORS failed"},
	{336068703, "MODIFY RDB$RELATION_FIELDS failed"},
	{336068704, "Local column {0} not found"},
	{336068705, "add EXTERNAL FILE not allowed"},
	{336068706, "drop EXTERNAL FILE not allowed"},
	{336068707, "MODIFY RDB$RELATIONS failed"},
	{336068708, "MODIFY RDB$PROCEDURE_PARAMETERS failed"},
	{336068709, "Table column not found"},
	{336068710, "MODIFY TRIGGER failed"},
	{336068711, "TRIGGER NAME expected"},
	{336068712, "unsupported DYN verb"},
	{336068713, "MODIFY TRIGGER MESSAGE failed"},
	{336068714, "Create metadata BLOB failed"},
	{336068715, "Write metadata BLOB failed"},
	{336068716, "Close metadata BLOB failed"},
	{336068717, "Triggers created automatically cannot be modified"},		/* dyn_cant_modify_auto_trig */
	{336068718, "unsupported DYN verb"},
	{336068719, "ERASE RDB$USER_PRIVILEGES failed in revoke(1)"},
	{336068720, "Access to RDB$USER_PRIVILEGES failed in revoke(2)"},
	{336068721, "ERASE RDB$USER_PRIVILEGES failed in revoke (3)"},
	{336068722, "Access to RDB$USER_PRIVILEGES failed in revoke (4)"},
	{336068723, "CREATE VIEW failed"},
	{336068724, " attempt to index BLOB column in INDEX {0}"},
	{336068725, " attempt to index array column in index {0}"},
	{336068726, "key size too big for index {0}"},
	{336068727, "no keys for index {0}"},
	{336068728, "Unknown columns in index {0}"},
	{336068729, "STORE RDB$RELATION_CONSTRAINTS failed"},
	{336068730, "STORE RDB$CHECK_CONSTRAINTS failed"},
	{336068731, "Column: {0} not defined as NOT NULL - cannot be used in PRIMARY KEY constraint definition"},
	{336068732, "A column name is repeated in the definition of constraint: {0}"},
	{336068733, "Integrity Constraint lookup failed"},
	{336068734, "Same set of columns cannot be used in more than one PRIMARY KEY and/or UNIQUE constraint definition"},
	{336068735, "STORE RDB$REF_CONSTRAINTS failed"},
	{336068736, "No table specified in delete_constraint"},
	{336068737, "ERASE RDB$RELATION_CONSTRAINTS failed"},
	{336068738, "CONSTRAINT {0} does not exist."},
	{336068739, "Generation of constraint name failed"},
	{336068740, "Table {0} already exists"},		/* dyn_dup_table */
	{336068741, "Number of referencing columns do not equal number of referenced columns"},
	{336068742, "STORE RDB$PROCEDURES failed"},
	{336068743, "Procedure {0} already exists"},		/* dyn_dup_procedure */
	{336068744, "STORE RDB$PROCEDURE_PARAMETERS failed"},
	{336068745, "Store into system table {0} failed"},
	{336068746, "ERASE RDB$PROCEDURE_PARAMETERS failed"},
	{336068747, "ERASE RDB$PROCEDURES failed"},
	{336068748, "Procedure {0} not found"},		/* dyn_proc_not_found */
	{336068749, "MODIFY RDB$PROCEDURES failed"},
	{336068750, "DEFINE EXCEPTION failed"},
	{336068751, "ERASE EXCEPTION failed"},
	{336068752, "Exception not found"},		/* dyn_exception_not_found */
	{336068753, "MODIFY EXCEPTION failed"},
	{336068754, "Parameter {0} in procedure {1} not found"},		/* dyn_proc_param_not_found */
	{336068755, "Trigger {0} not found"},		/* dyn_trig_not_found */
	{336068756, "Only one data type change to the domain {0} allowed at a time"},
	{336068757, "Only one data type change to the field {0} allowed at a time"},
	{336068758, "STORE RDB$FILES failed"},
	{336068759, "Character set {0} not found"},		/* dyn_charset_not_found */
	{336068760, "Collation {0} not found"},		/* dyn_collation_not_found */
	{336068761, "ERASE RDB$LOG_FILES failed"},
	{336068762, "STORE RDB$LOG_FILES failed"},
	{336068763, "Role {0} not found"},		/* dyn_role_not_found */
	{336068764, "Difference file lookup failed"},
	{336068765, "DEFINE SHADOW failed"},
	{336068766, "MODIFY RDB$ROLES failed"},
	{336068767, "Name longer than database column size"},		/* dyn_name_longer */
	{336068768, "\"Only one constraint allowed for a domain\""},
	{336068770, "Looking up column position failed"},
	{336068771, "A node name is not permitted in a table with external file definition"},
	{336068772, "Shadow lookup failed"},
	{336068773, "Shadow {0} already exists"},
	{336068774, "Cannot add file with the same name as the database or added files"},
	{336068775, "no grant option for privilege {0} on column {1} of table/view {2}"},
	{336068776, "no grant option for privilege {0} on column {1} of base table/view {2}"},
	{336068777, "no grant option for privilege {0} on table/view {1} (for column {2})"},
	{336068778, "no grant option for privilege {0} on base table/view {1} (for column {2})"},
	{336068779, "no {0} privilege with grant option on table/view {1} (for column {2})"},
	{336068780, "no {0} privilege with grant option on base table/view {1} (for column {2})"},
	{336068781, "no grant option for privilege {0} on table/view {1}"},
	{336068782, "no {0} privilege with grant option on table/view {1}"},
	{336068783, "table/view {0} does not exist"},
	{336068784, "column {0} does not exist in table/view {1}"},		/* dyn_column_does_not_exist */
	{336068785, "Can not alter a view"},
	{336068786, "EXTERNAL FILE table not supported in this context"},
	{336068787, "attempt to index COMPUTED BY column in INDEX {0}"},
	{336068788, "Table Name lookup failed"},
	{336068789, "attempt to index a view"},
	{336068790, "SELECT RDB$RELATIONS failed in grant"},
	{336068791, "SELECT RDB$RELATION_FIELDS failed in grant"},
	{336068792, "SELECT RDB$RELATIONS/RDB$OWNER_NAME failed in grant"},
	{336068793, "SELECT RDB$USER_PRIVILEGES failed in grant"},
	{336068794, "SELECT RDB$VIEW_RELATIONS/RDB$RELATION_FIELDS/... failed in grant"},
	{336068795, "column {0} from table {1} is referenced in index {2}"},
	{336068796, "SQL role {0} does not exist"},		/* dyn_role_does_not_exist */
	{336068797, "user {0} has no grant admin option on SQL role {1}"},		/* dyn_no_grant_admin_opt */
	{336068798, "user {0} is not a member of SQL role {1}"},		/* dyn_user_not_role_member */
	{336068799, "{0} is not the owner of SQL role {1}"},		/* dyn_delete_role_failed */
	{336068800, "{0} is a SQL role and not a user"},		/* dyn_grant_role_to_user */
	{336068801, "user name {0} could not be used for SQL role"},		/* dyn_inv_sql_role_name */
	{336068802, "SQL role {0} already exists"},		/* dyn_dup_sql_role */
	{336068803, "keyword {0} can not be used as a SQL role name"},		/* dyn_kywd_spec_for_role */
	{336068804, "SQL roles are not supported in on older versions of the database.  A backup and restore of the database is required."},		/* dyn_roles_not_supported */
	{336068812, "Cannot rename domain {0} to {1}.  A domain with that name already exists."},		/* dyn_domain_name_exists */
	{336068813, "Cannot rename column {0} to {1}.  A column with that name already exists in table {2}."},		/* dyn_field_name_exists */
	{336068814, "Column {0} from table {1} is referenced in {2}"},		/* dyn_dependency_exists */
	{336068815, "Cannot change datatype for column {0}.  Changing datatype is not supported for BLOB or ARRAY columns."},		/* dyn_dtype_invalid */
	{336068816, "New size specified for column {0} must be at least {1} characters."},		/* dyn_char_fld_too_small */
	{336068817, "Cannot change datatype for {0}.  Conversion from base type {1} to {2} is not supported."},		/* dyn_invalid_dtype_conversion */
	{336068818, "Cannot change datatype for column {0} from a character type to a non-character type."},		/* dyn_dtype_conv_invalid */
	{336068819, "unable to allocate memory from the operating system"},		/* dyn_virmemexh */
	{336068820, "Zero length identifiers are not allowed"},		/* dyn_zero_len_id */
	{336068821, "ERASE RDB$GENERATORS failed"},		/* del_gen_fail */
	{336068822, "Sequence {0} not found"},		/* dyn_gen_not_found */
	{336068823, "Difference file is not defined"},
	{336068824, "Difference file is already defined"},
	{336068825, "Database is already in the physical backup mode"},
	{336068826, "Database is not in the physical backup mode"},
	{336068827, "DEFINE COLLATION failed"},
	{336068828, "CREATE COLLATION statement is not supported in older versions of the database.  A backup and restore is required."},
	{336068829, "Maximum number of collations per character set exceeded"},		/* max_coll_per_charset */
	{336068830, "Invalid collation attributes"},		/* invalid_coll_attr */
	{336068831, "Collation {0} not installed for character set {1}"},
	{336068832, "Cannot use the internal domain {0} as new type for field {1}"},
	{336068833, "Default value is not allowed for array type in field {0}"},
	{336068834, "Default value is not allowed for array type in domain {0}"},
	{336068835, "DYN_UTIL_is_array failed for domain {0}"},
	{336068836, "DYN_UTIL_copy_domain failed for domain {0}"},
	{336068837, "Local column {0} doesn't have a default"},
	{336068838, "Local column {0} default belongs to domain {1}"},
	{336068839, "File name is invalid"},
	{336068840, "{0} cannot reference {1}"},		/* dyn_wrong_gtt_scope */
	{336068841, "Local column {0} is computed, cannot set a default value"},
	{336068842, "ERASE RDB$COLLATIONS failed"},		/* del_coll_fail */
	{336068843, "Collation {0} is used in table {1} (field name {2}) and cannot be dropped"},		/* dyn_coll_used_table */
	{336068844, "Collation {0} is used in domain {1} and cannot be dropped"},		/* dyn_coll_used_domain */
	{336068845, "Cannot delete system collation"},		/* dyn_cannot_del_syscoll */
	{336068846, "Cannot delete default collation of CHARACTER SET {0}"},		/* dyn_cannot_del_def_coll */
	{336068847, "Domain {0} is used in procedure {1} (parameter name {2}) and cannot be dropped"},
	{336068848, "Field {0} cannot be used twice in index {1}"},
	{336068849, "Table {0} not found"},		/* dyn_table_not_found */
	{336068850, "attempt to reference a view ({0}) in a foreign key"},
	{336068851, "Collation {0} is used in procedure {1} (parameter name {2}) and cannot be dropped"},		/* dyn_coll_used_procedure */
	{336068852, "New scale specified for column {0} must be at most {1}."},		/* dyn_scale_too_big */
	{336068853, "New precision specified for column {0} must be at least {1}."},		/* dyn_precision_too_small */
	{336068854, "{0} is not grantor of {1} on {2} to {3}."},
	{336068855, "Warning: {0} on {1} is not granted to {2}."},		/* dyn_miss_priv_warning */
	{336068856, "Feature '{0}' is not supported in ODS {1}.{2}"},		/* dyn_ods_not_supp_feature */
	{336068857, "Cannot add or remove COMPUTED from column {0}"},		/* dyn_cannot_addrem_computed */
	{336068858, "Password should not be empty string"},		/* dyn_no_empty_pw */
	{336068859, "Index {0} already exists"},		/* dyn_dup_index */
	{336068860, "Only {0} or user with privilege USE_GRANTED_BY_CLAUSE can use GRANTED BY clause"},		/* dyn_locksmith_use_granted */
	{336068861, "Exception {0} already exists"},		/* dyn_dup_exception */
	{336068862, "Sequence {0} already exists"},		/* dyn_dup_generator */
	{336068863, "ERASE RDB$USER_PRIVILEGES failed in REVOKE ALL ON ALL"},
	{336068864, "Package {0} not found"},		/* dyn_package_not_found */
	{336068865, "Schema {0} not found"},		/* dyn_schema_not_found */
	{336068866, "Cannot ALTER or DROP system procedure {0}"},		/* dyn_cannot_mod_sysproc */
	{336068867, "Cannot ALTER or DROP system trigger {0}"},		/* dyn_cannot_mod_systrig */
	{336068868, "Cannot ALTER or DROP system function {0}"},		/* dyn_cannot_mod_sysfunc */
	{336068869, "Invalid DDL statement for procedure {0}"},		/* dyn_invalid_ddl_proc */
	{336068870, "Invalid DDL statement for trigger {0}"},		/* dyn_invalid_ddl_trig */
	{336068871, "Function {0} has not been defined on the package body {1}"},		/* dyn_funcnotdef_package */
	{336068872, "Procedure {0} has not been defined on the package body {1}"},		/* dyn_procnotdef_package */
	{336068873, "Function {0} has a signature mismatch on package body {1}"},		/* dyn_funcsignat_package */
	{336068874, "Procedure {0} has a signature mismatch on package body {1}"},		/* dyn_procsignat_package */
	{336068875, "Default values for parameters are not allowed in the definition of a previously declared packaged procedure {0}.{1}"},		/* dyn_defvaldecl_package_proc */
	{336068876, "Function {0} already exists"},		/* dyn_dup_function */
	{336068877, "Package body {0} already exists"},		/* dyn_package_body_exists */
	{336068878, "Invalid DDL statement for function {0}"},		/* dyn_invalid_ddl_func */
	{336068879, "Cannot alter new style function {0} with ALTER EXTERNAL FUNCTION. Use ALTER FUNCTION instead."},		/* dyn_newfc_oldsyntax */
	{336068880, "Cannot delete system generator {0}"},
	{336068881, "Identity column {0} of table {1} must be of exact number type with zero scale"},
	{336068882, "Identity column {0} of table {1} cannot be changed to NULLable"},
	{336068883, "Identity column {0} of table {1} cannot have default value"},
	{336068884, "Domain {0} must be of exact number type with zero scale because it's used in an identity column"},
	{336068885, "Generation of generator name failed"},
	{336068886, "Parameter {0} in function {1} not found"},		/* dyn_func_param_not_found */
	{336068887, "Parameter {0} of routine {1} not found"},		/* dyn_routine_param_not_found */
	{336068888, "Parameter {0} of routine {1} is ambiguous (found in both procedures and functions). Use a specifier keyword."},		/* dyn_routine_param_ambiguous */
	{336068889, "Collation {0} is used in function {1} (parameter name {2}) and cannot be dropped"},		/* dyn_coll_used_function */
	{336068890, "Domain {0} is used in function {1} (parameter name {2}) and cannot be dropped"},		/* dyn_domain_used_function */
	{336068891, "ALTER USER requires at least one clause to be specified"},		/* dyn_alter_user_no_clause */
	{336068892, "Cannot delete system SQL role {0}"},
	{336068893, "Column {0} is not an identity column"},
	{336068894, "Duplicate {0} {1}"},		/* dyn_duplicate_package_item */
	{336068895, "System {0} {1} cannot be modified"},		/* dyn_cant_modify_sysobj */
	{336068896, "INCREMENT BY 0 is an illegal option for sequence {0}"},		/* dyn_cant_use_zero_increment */
	{336068897, "Can't use {0} in FOREIGN KEY constraint"},		/* dyn_cant_use_in_foreignkey */
	{336068898, "Default values for parameters are not allowed in the definition of a previously declared packaged function {0}.{1}"},		/* dyn_defvaldecl_package_func */
	{336068899, "Password must be specified when creating user"},		/* dyn_create_user_no_password */
	{336068900, "role {0} can not be granted to role {1}"},		/* dyn_cyclic_role */
	{336068901, "DROP SYSTEM PRIVILEGES should not be used in CREATE ROLE operator"},
	{336068902, "Access to SYSTEM PRIVILEGES in ROLES denied to {0}"},
	{336068903, "Only {0}, DB owner {1} or user with privilege USE_GRANTED_BY_CLAUSE can use GRANTED BY clause"},
	{336068904, "INCREMENT BY 0 is an illegal option for identity column {0} of table {1}"},		/* dyn_cant_use_zero_inc_ident */
	{336068905, "Concurrent ALTER DATABASE is not supported"},		/* dyn_concur_alter_database */
	{336068906, "Incompatible ALTER DATABASE clauses: '{0}' and '{1}'"},		/* dyn_incompat_alter_database */
	{336068907, "no {0} privilege with grant option on DDL {1}"},		/* dyn_no_ddl_grant_opt_priv */
	{336068908, "no {0} privilege with grant option on object {1}"},		/* dyn_no_grant_opt_priv */
	{336068909, "Function {0} does not exist"},		/* dyn_func_not_exist */
	{336068910, "Procedure {0} does not exist"},		/* dyn_proc_not_exist */
	{336068911, "Package {0} does not exist"},		/* dyn_pack_not_exist */
	{336068912, "Trigger {0} does not exist"},		/* dyn_trig_not_exist */
	{336068913, "View {0} does not exist"},		/* dyn_view_not_exist */
	{336068914, "Table {0} does not exist"},		/* dyn_rel_not_exist */
	{336068915, "Exception {0} does not exist"},		/* dyn_exc_not_exist */
	{336068916, "Generator/Sequence {0} does not exist"},		/* dyn_gen_not_exist */
	{336068917, "Field {0} of table {1} does not exist"},		/* dyn_fld_not_exist */
	{336330752, "could not locate appropriate error message"},
	{336330753, "found unknown switch"},		/* gbak_unknown_switch */
	{336330754, "page size parameter missing"},		/* gbak_page_size_missing */
	{336330755, "Page size specified ({0}) greater than limit (32768 bytes)"},		/* gbak_page_size_toobig */
	{336330756, "redirect location for output is not specified"},		/* gbak_redir_ouput_missing */
	{336330757, "conflicting switches for backup/restore"},		/* gbak_switches_conflict */
	{336330758, "device type {0} not known"},		/* gbak_unknown_device */
	{336330759, "protection is not there yet"},		/* gbak_no_protection */
	{336330760, "page size is allowed only on restore or create"},		/* gbak_page_size_not_allowed */
	{336330761, "multiple sources or destinations specified"},		/* gbak_multi_source_dest */
	{336330762, "requires both input and output filenames"},		/* gbak_filename_missing */
	{336330763, "input and output have the same name.  Disallowed."},		/* gbak_dup_inout_names */
	{336330764, "expected page size, encountered \"{0}\""},		/* gbak_inv_page_size */
	{336330765, "REPLACE specified, but the first file {0} is a database"},		/* gbak_db_specified */
	{336330766, "database {0} already exists.  To replace it, use the -REP switch"},		/* gbak_db_exists */
	{336330767, "device type not specified"},		/* gbak_unk_device */
	{336330768, "cannot create APOLLO tape descriptor file {0}"},
	{336330769, "cannot set APOLLO tape descriptor attribute for {0}"},
	{336330770, "cannot create APOLLO cartridge descriptor file {0}"},
	{336330771, "cannot close APOLLO tape descriptor file {0}"},
	{336330772, "gds_$blob_info failed"},		/* gbak_blob_info_failed */
	{336330773, "do not understand BLOB INFO item {0}"},		/* gbak_unk_blob_item */
	{336330774, "gds_$get_segment failed"},		/* gbak_get_seg_failed */
	{336330775, "gds_$close_blob failed"},		/* gbak_close_blob_failed */
	{336330776, "gds_$open_blob failed"},		/* gbak_open_blob_failed */
	{336330777, "Failed in put_blr_gen_id"},		/* gbak_put_blr_gen_id_failed */
	{336330778, "data type {0} not understood"},		/* gbak_unk_type */
	{336330779, "gds_$compile_request failed"},		/* gbak_comp_req_failed */
	{336330780, "gds_$start_request failed"},		/* gbak_start_req_failed */
	{336330781, "gds_$receive failed"},		/* gbak_rec_failed */
	{336330782, "gds_$release_request failed"},		/* gbak_rel_req_failed */
	{336330783, "gds_$database_info failed"},		/* gbak_db_info_failed */
	{336330784, "Expected database description record"},		/* gbak_no_db_desc */
	{336330785, "failed to create database {0}"},		/* gbak_db_create_failed */
	{336330786, "RESTORE: decompression length error"},		/* gbak_decomp_len_error */
	{336330787, "cannot find table {0}"},		/* gbak_tbl_missing */
	{336330788, "Cannot find column for BLOB"},		/* gbak_blob_col_missing */
	{336330789, "gds_$create_blob failed"},		/* gbak_create_blob_failed */
	{336330790, "gds_$put_segment failed"},		/* gbak_put_seg_failed */
	{336330791, "expected record length"},		/* gbak_rec_len_exp */
	{336330792, "wrong length record, expected {0} encountered {1}"},		/* gbak_inv_rec_len */
	{336330793, "expected data attribute"},		/* gbak_exp_data_type */
	{336330794, "Failed in store_blr_gen_id"},		/* gbak_gen_id_failed */
	{336330795, "do not recognize record type {0}"},		/* gbak_unk_rec_type */
	{336330796, "Expected backup version 1..10.  Found {0}"},		/* gbak_inv_bkup_ver */
	{336330797, "expected backup description record"},		/* gbak_missing_bkup_desc */
	{336330798, "string truncated"},		/* gbak_string_trunc */
	{336330799, "warning -- record could not be restored"},		/* gbak_cant_rest_record */
	{336330800, "gds_$send failed"},		/* gbak_send_failed */
	{336330801, "no table name for data"},		/* gbak_no_tbl_name */
	{336330802, "unexpected end of file on backup file"},		/* gbak_unexp_eof */
	{336330803, "database format {0} is too old to restore to"},		/* gbak_db_format_too_old */
	{336330804, "array dimension for column {0} is invalid"},		/* gbak_inv_array_dim */
	{336330805, "expected array version number {0} but instead found {1}"},
	{336330806, "expected array dimension {0} but instead found {1}"},
	{336330807, "Expected XDR record length"},		/* gbak_xdr_len_expected */
	{336330808, "Unexpected I/O error while {0} backup file"},
	{336330809, "adding file {0}, starting at page {1}"},
	{336330810, "array"},
	{336330811, "backup"},
	{336330812, "    {0}B(ACKUP_DATABASE)    backup database to file"},
	{336330813, "\t\tbackup file is compressed"},
	{336330814, "    {0}D(EVICE)             backup file device type on APOLLO (CT or MT)"},
	{336330815, "    {0}M(ETA_DATA)          backup or restore metadata only"},
	{336330816, "blob"},
	{336330817, "cannot open backup file {0}"},		/* gbak_open_bkup_error */
	{336330818, "cannot open status and error output file {0}"},		/* gbak_open_error */
	{336330819, "closing file, committing, and finishing"},
	{336330820, "committing metadata"},
	{336330821, "commit failed on table {0}"},
	{336330822, "committing secondary files"},
	{336330823, "creating index {0}"},
	{336330824, "committing data for table {0}"},
	{336330825, "    {0}C(REATE_DATABASE)    create database from backup file (restore)"},
	{336330826, "created database {0}, page_size {1} bytes"},
	{336330827, "creating file {0}"},
	{336330828, "creating indexes"},
	{336330829, "database {0} has a page size of {1} bytes."},
	{336330830, "    {0}I(NACTIVE)           deactivate indexes during restore"},
	{336330831, "do not understand BLOB INFO item {0}"},
	{336330832, "do not recognize {0} attribute {1} -- continuing"},
	{336330833, "error accessing BLOB column {0} -- continuing"},
	{336330834, "Exiting before completion due to errors"},
	{336330835, "Exiting before completion due to errors"},
	{336330836, "column"},
	{336330837, "file"},
	{336330838, "file length"},
	{336330839, "filter"},
	{336330840, "finishing, closing, and going home"},
	{336330841, "function"},
	{336330842, "function argument"},
	{336330843, "gbak version {0}"},
	{336330844, "domain"},
	{336330845, "index"},
	{336330846, "trigger {0} is invalid"},
	{336330847, "legal switches are:"},
	{336330848, "length given for initial file ({0}) is less than minimum ({1})"},
	{336330849, "    {0}E(XPAND)             no data compression"},
	{336330850, "    {0}L(IMBO)              ignore transactions in limbo"},
	{336330851, "    {0}O(NE_AT_A_TIME)      restore one table at a time"},
	{336330852, "opened file {0}"},
	{336330853, "    {0}P(AGE_SIZE)          override default page size"},
	{336330854, "page size"},
	{336330855, "page size specified ({0} bytes) rounded up to {1} bytes"},
	{336330856, "    {0}Z                    print version number"},
	{336330857, "privilege"},
	{336330858, "     {0} records ignored"},
	{336330859, "   {0} records restored"},
	{336330860, "{0} records written"},
	{336330861, "    {0}Y  <path>            redirect/suppress status message output"},
	{336330862, "Reducing the database page size from {0} bytes to {1} bytes"},
	{336330863, "table"},
	{336330864, "    {0}REP(LACE_DATABASE)   replace database from backup file (restore)"},
	{336330865, "    {0}V(ERIFY)             report each action taken"},
	{336330866, "restore failed for record in table {0}"},
	{336330867, "    restoring column {0}"},
	{336330868, "    restoring file {0}"},
	{336330869, "    restoring filter {0}"},
	{336330870, "restoring function {0}"},
	{336330871, "    restoring argument for function {0}"},
	{336330872, "     restoring gen id value of: {0}"},
	{336330873, "restoring domain {0}"},
	{336330874, "    restoring index {0}"},
	{336330875, "    restoring privilege for user {0}"},
	{336330876, "restoring data for table {0}"},
	{336330877, "restoring security class {0}"},
	{336330878, "    restoring trigger {0}"},
	{336330879, "    restoring trigger message for {0}"},
	{336330880, "    restoring type {0} for column {1}"},
	{336330881, "started transaction"},
	{336330882, "starting transaction"},
	{336330883, "security class"},
	{336330884, "switches can be abbreviated to the unparenthesized characters"},
	{336330885, "transportable backup -- data in XDR format"},
	{336330886, "trigger"},
	{336330887, "trigger message"},
	{336330888, "trigger type"},
	{336330889, "unknown switch \"{0}\""},
	{336330890, "validation error on column in table {0}"},
	{336330891, "    Version(s) for database \"{0}\""},
	{336330892, "view"},
	{336330893, "    writing argument for function {0}"},
	{336330894, "    writing data for table {0}"},
	{336330895, "     writing gen id of: {0}"},
	{336330896, "         writing column {0}"},
	{336330897, "    writing filter {0}"},
	{336330898, "writing filters"},
	{336330899, "    writing function {0}"},
	{336330900, "writing functions"},
	{336330901, "    writing domain {0}"},
	{336330902, "writing domains"},
	{336330903, "    writing index {0}"},
	{336330904, "    writing privilege for user {0}"},
	{336330905, "    writing table {0}"},
	{336330906, "writing tables"},
	{336330907, "    writing security class {0}"},
	{336330908, "    writing trigger {0}"},
	{336330909, "    writing trigger message for {0}"},
	{336330910, "writing trigger messages"},
	{336330911, "writing triggers"},
	{336330912, "    writing type {0} for column {1}"},
	{336330913, "writing types"},
	{336330914, "writing shadow files"},
	{336330915, "    writing shadow file {0}"},
	{336330916, "writing id generators"},
	{336330917, "    writing generator {0} value {1}"},
	{336330918, "readied database {0} for backup"},
	{336330919, "restoring table {0}"},
	{336330920, "type"},
	{336330921, "gbak:"},
	{336330922, "committing metadata for table {0}"},
	{336330923, "error committing metadata for table {0}"},
	{336330924, "    {0}K(ILL)               restore without creating shadows"},
	{336330925, "cannot commit index {0}"},
	{336330926, "cannot commit files"},
	{336330927, "    {0}T(RANSPORTABLE)      transportable backup -- data in XDR format"},
	{336330928, "closing file, committing, and finishing. {0} bytes written"},
	{336330929, "    {0}G(ARBAGE_COLLECT)    inhibit garbage collection"},
	{336330930, "    {0}IG(NORE)             ignore bad checksums"},
	{336330931, "\tcolumn {0} used in index {1} seems to have vanished"},
	{336330932, "index {0} omitted because {1} of the expected {2} keys were found"},
	{336330933, "    {0}FA(CTOR)             blocking factor"},
	{336330934, "blocking factor parameter missing"},		/* gbak_missing_block_fac */
	{336330935, "expected blocking factor, encountered \"{0}\""},		/* gbak_inv_block_fac */
	{336330936, "a blocking factor may not be used in conjunction with device CT"},		/* gbak_block_fac_specified */
	{336330937, "restoring generator {0} value: {1}"},
	{336330938, "    {0}OL(D_DESCRIPTIONS)   save old style metadata descriptions"},
	{336330939, "    {0}N(O_VALIDITY)        do not restore database validity conditions"},
	{336330940, "user name parameter missing"},		/* gbak_missing_username */
	{336330941, "password parameter missing"},		/* gbak_missing_password */
	{336330942, "    {0}PAS(SWORD)           Firebird password"},
	{336330943, "    {0}USER                 Firebird user name"},
	{336330944, "writing stored procedures"},
	{336330945, "writing stored procedure {0}"},
	{336330946, "writing parameter {0} for stored procedure"},
	{336330947, "restoring stored procedure {0}"},
	{336330948, "    restoring parameter {0} for stored procedure"},
	{336330949, "writing exceptions"},
	{336330950, "writing exception {0}"},
	{336330951, "restoring exception {0}"},
	{336330952, " missing parameter for the number of bytes to be skipped"},		/* gbak_missing_skipped_bytes */
	{336330953, "expected number of bytes to be skipped, encountered \"{0}\""},		/* gbak_inv_skipped_bytes */
	{336330954, "adjusting an invalid decompression length from {0} to {1}"},
	{336330955, "skipped {0} bytes after reading a bad attribute {1}"},
	{336330956, "    {0}S(KIP_BAD_DATA)      skip number of bytes after reading bad data"},
	{336330957, "skipped {0} bytes looking for next valid attribute, encountered attribute {1}"},
	{336330958, "writing table constraints"},
	{336330959, "writing constraint {0}"},
	{336330960, "table constraint"},
	{336330961, "writing referential constraints"},
	{336330962, "writing check constraints"},
	{336330963, "writing character sets"},		/* msgVerbose_write_charsets */
	{336330964, "writing collations"},		/* msgVerbose_write_collations */
	{336330965, "character set"},		/* gbak_err_restore_charset */
	{336330966, "writing character set {0}"},		/* msgVerbose_restore_charset */
	{336330967, "collation"},		/* gbak_err_restore_collation */
	{336330968, "writing collation {0}"},		/* msgVerbose_restore_collation */
	{336330972, "Unexpected I/O error while reading from backup file"},		/* gbak_read_error */
	{336330973, "Unexpected I/O error while writing to backup file"},		/* gbak_write_error */
	{336330974, "\n\nCould not open file name \"{0}\""},
	{336330975, "\n\nCould not write to file \"{0}\""},
	{336330976, "\n\nCould not read from file \"{0}\""},
	{336330977, "Done with volume #{0}, \"{1}\""},
	{336330978, "\tPress return to reopen that file, or type a new\n\tname followed by return to open a different file."},
	{336330979, "Type a file name to open and hit return"},
	{336330980, "  Name: "},
	{336330981, "\n\nERROR: Backup incomplete"},
	{336330982, "Expected backup start time {0}, found {1}"},
	{336330983, "Expected backup database {0}, found {1}"},
	{336330984, "Expected volume number {0}, found volume {1}"},
	{336330985, "could not drop database {0} (no privilege or database might be in use)"},		/* gbak_db_in_use */
	{336330986, "Skipped bad security class entry: {0}"},
	{336330987, "Unknown V3 SUB_TYPE: {0} in FIELD: {1}."},
	{336330988, "Converted V3 sub_type: {0} to character_set_id: {1} and collate_id: {2}."},
	{336330989, "Converted V3 scale: {0} to character_set_id: {1} and callate_id: {2}."},
	{336330990, "System memory exhausted"},		/* gbak_sysmemex */
	{336330991, "    {0}NT                   Non-Transportable backup file format"},
	{336330992, "Index \"{0}\" failed to activate because:"},
	{336330993, "  The unique index has duplicate values or NULLs."},
	{336330994, "  Delete or Update duplicate values or NULLs, and activate index with"},
	{336330995, "  ALTER INDEX \"{0}\" ACTIVE;"},
	{336330996, "  Not enough disk space to create the sort file for an index."},
	{336330997, "  Set the TMP environment variable to a directory on a filesystem that does have enough space, and activate index with"},
	{336330998, "Database is not online due to failure to activate one or more indices."},
	{336330999, "Run gfix -online to bring database online without active indices."},
	{336331000, "writing SQL roles"},		/* write_role_1 */
	{336331001, "    writing SQL role: {0}"},		/* write_role_2 */
	{336331002, "SQL role"},		/* gbak_restore_role_failed */
	{336331003, "    restoring SQL role: {0}"},		/* restore_role */
	{336331004, "    {0}RO(LE)               Firebird SQL role"},		/* gbak_role_op */
	{336331005, "SQL role parameter missing"},		/* gbak_role_op_missing */
	{336331006, "    {0}CO(NVERT)            backup external files as tables"},		/* gbak_convert_ext_tables */
	{336331007, "gbak: WARNING:"},		/* gbak_warning */
	{336331008, "gbak: ERROR:"},		/* gbak_error */
	{336331009, "    {0}BU(FFERS)            override page buffers default"},		/* gbak_page_buffers */
	{336331010, "page buffers parameter missing"},		/* gbak_page_buffers_missing */
	{336331011, "expected page buffers, encountered \"{0}\""},		/* gbak_page_buffers_wrong_param */
	{336331012, "page buffers is allowed only on restore or create"},		/* gbak_page_buffers_restore */
	{336331013, "Starting with volume #{0}, \"{1}\""},
	{336331014, "size specification either missing or incorrect for file {0}"},		/* gbak_inv_size */
	{336331015, "file {0} out of sequence"},		/* gbak_file_outof_sequence */
	{336331016, "can't join -- one of the files missing"},		/* gbak_join_file_missing */
	{336331017, " standard input is not supported when using join operation"},		/* gbak_stdin_not_supptd */
	{336331018, "standard output is not supported when using split operation or in verbose mode"},		/* gbak_stdout_not_supptd */
	{336331019, "backup file {0} might be corrupt"},		/* gbak_bkup_corrupt */
	{336331020, "database file specification missing"},		/* gbak_unk_db_file_spec */
	{336331021, "can't write a header record to file {0}"},		/* gbak_hdr_write_failed */
	{336331022, "free disk space exhausted"},		/* gbak_disk_space_ex */
	{336331023, "file size given ({0}) is less than minimum allowed ({1})"},		/* gbak_size_lt_min */
	{336331024, "Warning -- free disk space exhausted for file {0}, the rest of the bytes ({1}) will be written to file {2}"},
	{336331025, "service name parameter missing"},		/* gbak_svc_name_missing */
	{336331026, "Cannot restore over current database, must be SYSDBA or owner of the existing database."},		/* gbak_not_ownr */
	{336331027, ""},
	{336331028, "    {0}USE_(ALL_SPACE)      do not reserve space for record versions"},
	{336331029, "    {0}SE(RVICE)            use services manager"},
	{336331030, "    {0}MO(DE) <access>      \"read_only\" or \"read_write\" access"},		/* gbak_opt_mode */
	{336331031, "\"read_only\" or \"read_write\" required"},		/* gbak_mode_req */
	{336331032, "setting database to read-only access"},
	{336331033, "just data ignore all constraints etc."},		/* gbak_just_data */
	{336331034, "restoring data only ignoring foreign key, unique, not null & other constraints"},		/* gbak_data_only */
	{336331035, "closing file, committing, and finishing. {0} bytes written"},
	{336331036, "    {0}R(ECREATE_DATABASE) [O(VERWRITE)] create (or replace if OVERWRITE used)\\n\t\t\t\tdatabase from backup file (restore)"},
	{336331037, "    activating and creating deferred index {0}"},		/* gbak_activating_idx */
	{336331038, "check constraint"},
	{336331039, "exception"},
	{336331040, "array dimensions"},
	{336331041, "generator"},
	{336331042, "procedure"},
	{336331043, "procedure parameter"},
	{336331044, "referential constraint"},
	{336331045, "type (in RDB$TYPES)"},
	{336331046, "    {0}NOD(BTRIGGERS)       do not run database triggers"},
	{336331047, "    {0}TRU(STED)            use trusted authentication"},
	{336331048, "writing names mapping"},		/* write_map_1 */
	{336331049, "    writing map for {0}"},		/* write_map_2 */
	{336331050, "    restoring map for {0}"},		/* get_map_1 */
	{336331051, "name mapping"},		/* get_map_2 */
	{336331052, "cannot restore arbitrary mapping"},		/* get_map_3 */
	{336331053, "restoring names mapping"},		/* get_map_4 */
	{336331054, "    {0}FIX_FSS_D(ATA)       fix malformed UNICODE_FSS data"},
	{336331055, "    {0}FIX_FSS_M(ETADATA)   fix malformed UNICODE_FSS metadata"},
	{336331056, "Character set parameter missing"},
	{336331057, "Character set {0} not found"},
	{336331058, "    {0}FE(TCH_PASSWORD)     fetch password from file"},
	{336331059, "too many passwords provided"},
	{336331060, "could not open password file {0}, errno {1}"},
	{336331061, "could not read password file {0}, errno {1}"},
	{336331062, "empty password file {0}"},
	{336331063, "Attribute {0} was already processed for exception {1}"},
	{336331064, "Skipping attribute {0} because the message already exists for exception {1}"},
	{336331065, "Trying to recover from unexpected attribute {0} due to wrong message length for exception {1}"},
	{336331066, "Attribute not specified for storing text bigger than 255 bytes"},
	{336331067, "Unable to store text bigger than 65536 bytes"},
	{336331068, "Failed while adjusting the security class name"},
	{336331069, "Usage:"},
	{336331070, "     gbak -b <db set> <backup set> [backup options] [general options]"},
	{336331071, "     gbak -c <backup set> <db set> [restore options] [general options]"},
	{336331072, "     <db set> = <database> | <db1 size1>...<dbN> (size in db pages)"},
	{336331073, "     <backup set> = <backup> | <bk1 size1>...<bkN> (size in bytes = n[K|M|G])"},
	{336331074, "     -recreate overwrite and -replace can be used instead of -c"},
	{336331075, "backup options are:"},
	{336331076, "restore options are:"},
	{336331077, "general options are:"},
	{336331078, "verbose interval value parameter missing"},		/* gbak_missing_interval */
	{336331079, "verbose interval value cannot be smaller than {0}"},		/* gbak_wrong_interval */
	{336331080, "    {0}VERBI(NT) <n>        verbose information with explicit interval"},
	{336331081, "verify (verbose) and verbint options are mutually exclusive"},		/* gbak_verify_verbint */
	{336331082, "option -{0} is allowed only on restore or create"},		/* gbak_option_only_restore */
	{336331083, "option -{0} is allowed only on backup"},		/* gbak_option_only_backup */
	{336331084, "options -{0} and -{1} are mutually exclusive"},		/* gbak_option_conflict */
	{336331085, "parameter for option -{0} was already specified with value \"{1}\""},		/* gbak_param_conflict */
	{336331086, "option -{0} was already specified"},		/* gbak_option_repeated */
	{336331087, "writing package {0}"},
	{336331088, "writing packages"},
	{336331089, "restoring package {0}"},
	{336331090, "package"},
	{336331091, "dependency depth greater than {0} for view {1}"},		/* gbak_max_dbkey_recursion */
	{336331092, "value greater than {0} when calculating length of rdb$db_key for view {1}"},		/* gbak_max_dbkey_length */
	{336331093, "Invalid metadata detected. Use -FIX_FSS_METADATA option."},		/* gbak_invalid_metadata */
	{336331094, "Invalid data detected. Use -FIX_FSS_DATA option."},		/* gbak_invalid_data */
	{336331095, "text for attribute {0} is too large in {1}, truncating to {2} bytes"},
	{336331096, "Expected backup version {1}..{2}.  Found {0}"},		/* gbak_inv_bkup_ver2 */
	{336331097, "    writing view {0}"},
	{336331098, "    table {0} is a view"},
	{336331099, "writing security classes"},
	{336331100, "database format {0} is too old to backup"},		/* gbak_db_format_too_old2 */
	{336331101, "backup version is {0}"},
	{336331102, "adjusting system generators"},
	{336331103, "Error closing database, but backup file is OK"},
	{336331104, "database"},
	{336331105, "required mapping attributes are missing in backup file"},
	{336331106, "missing regular expression to skip tables"},
	{336331107, "    {0}SKIP_D(ATA)          skip data for table"},
	{336331108, "regular expression to skip tables was already set"},
	{336331109, "adjusting views dbkey length"},
	{336331110, "updating ownership of packages, procedures and tables"},
	{336331111, "adding missing privileges"},
	{336331112, "adjusting the ONLINE and FORCED WRITES flags"},
	{336331113, "    {0}ST(ATISTICS) TDRW    show statistics:"},
	{336331114, "        T                 time from start"},
	{336331115, "        D                 delta time"},
	{336331116, "        R                 page reads"},
	{336331117, "        W                 page writes"},
	{336331118, "statistics parameter missing"},		/* gbak_missing_perf */
	{336331119, "wrong char \"{0}\" at statistics parameter"},		/* gbak_wrong_perf */
	{336331120, "too many chars at statistics parameter"},		/* gbak_too_long_perf */
	{336331121, "total statistics"},
	{336331122, "could not append BLOB data to batch"},
	{336331123, "could not start batch when restoring table {0}, trying old way"},
	{336331124, "    {0}KEYNAME              name of a key to be used for encryption"},
	{336331125, "    {0}CRYPT                crypt plugin name"},
	{336331126, "    {0}ZIP                  backup file is in zip compressed format"},
	{336331127, "Keyname parameter missing"},
	{336331128, "Key holder parameter missing but backup file is encrypted"},
	{336331129, "CryptPlugin parameter missing"},
	{336331130, "Unknown crypt plugin name - use -CRYPT switch"},
	{336331131, "Inflate error {0}"},
	{336331132, "Deflate error {0}"},
	{336331133, "Key holder parameter missing"},
	{336331134, "    {0}KEYHOLDER            name of a key holder plugin"},
	{336331135, "Decompression stream init error {0}"},
	{336331136, "Compression stream init error {0}"},
	{336331137, "Invalid reply from getInfo() when waiting for DB encryption"},
	{336331138, "Problems with just created database encryption"},
	{336331139, "Skipped trigger {0} on system table {1}"},
	{336331140, "    {0}INCLUDE(_DATA)       backup data of table(s)"},
	{336331141, "missing regular expression to include tables"},
	{336331142, "regular expression to include tables was already set"},
	{336331143, "writing database create grants"},
	{336331144, "    database create grant for {0}"},
	{336331145, "    restoring database create grant for {0}"},
	{336331146, "restoring database create grants"},
	{336331147, "database create grant"},
	{336331148, "writing publications"},
	{336331149, "    writing publication {0}"},
	{336331150, "    writing publication for table {0}"},
	{336331151, "restoring publication {0}"},
	{336331152, "publication"},
	{336331153, "restoring publication for table {0}"},
	{336331154, "publication for table"},
	{336331155, "    {0}REPLICA <mode>      \"none\", \"read_only\" or \"read_write\" replica mode"},		/* gbak_opt_replica */
	{336331156, "\"none\", \"read_only\" or \"read_write\" required"},		/* gbak_replica_req */
	{336331157, "could not access batch parameters"},
	{336331158, "    {0}PAR(ALLEL)           parallel workers"},
	{336331159, "parallel workers parameter missing"},		/* gbak_missing_prl_wrks */
	{336331160, "expected parallel workers, encountered \"{0}\""},		/* gbak_inv_prl_wrks */
	{336331161, "    {0}D(IRECT_IO)          direct IO for backup file(s)"},
	{336331162, "use up to {0} parallel workers"},
	{336396289, "Firebird error"},
	{336396362, "Rollback not performed"},
	{336396364, "Connection error"},
	{336396365, "Connection not established"},
	{336396366, "Connection authorization failure."},
	{336396375, "deadlock"},
	{336396376, "Unsuccessful execution caused by deadlock."},
	{336396377, "record from transaction {0} is stuck in limbo"},
	{336396379, "operation completed with errors"},
	{336396382, "the SQL statement cannot be executed"},
	{336396384, "Unsuccessful execution caused by an unavailable resource."},
	{336396386, "Unsuccessful execution caused by a system error that precludes successful execution of subsequent statements"},
	{336396387, "Unsuccessful execution caused by system error that does not preclude successful execution of subsequent statements"},
	{336396446, "Wrong numeric type"},
	{336396447, "too many versions"},
	{336396448, "intermediate journal file full"},
	{336396449, "journal file wrong format"},
	{336396450, "database {0} shutdown in {1} seconds"},
	{336396451, "restart shared cache manager"},
	{336396452, "exception {0}"},
	{336396453, "bad checksum"},
	{336396454, "refresh range number {0} not found"},
	{336396455, "expression evaluation not supported"},
	{336396456, "FOREIGN KEY column count does not match PRIMARY KEY"},
	{336396457, "Attempt to define a second PRIMARY KEY for the same table"},
	{336396458, "column used with aggregate"},
	{336396459, "invalid column reference"},
	{336396460, "invalid key position"},
	{336396461, "invalid direction for find operation"},
	{336396462, "Invalid statement handle"},
	{336396463, "invalid lock handle"},
	{336396464, "invalid lock level {0}"},
	{336396465, "invalid bookmark handle"},
	{336396468, "wrong or obsolete version"},
	{336396471, "The INSERT, UPDATE, DELETE, DDL or authorization statement cannot be executed because the transaction is inquiry only"},
	{336396472, "external file could not be opened for output"},
	{336396477, "multiple rows in singleton select"},
	{336396478, "No subqueries permitted for VIEW WITH CHECK OPTION"},
	{336396479, "DISTINCT, GROUP or HAVING not permitted for VIEW WITH CHECK OPTION"},
	{336396480, "Only one table allowed for VIEW WITH CHECK OPTION"},
	{336396481, "No WHERE clause for VIEW WITH CHECK OPTION"},
	{336396482, "Only simple column names permitted for VIEW WITH CHECK OPTION"},
	{336396484, "An error was found in the application program input parameters for the SQL statement."},
	{336396485, "Invalid insert or update value(s): object columns are constrained - no 2 table rows can have duplicate column values"},
	{336396486, "Arithmetic overflow or division by zero has occurred."},
	{336396594, "cannot access column {0} in view {1}"},
	{336396595, "Too many concurrent executions of the same request"},
	{336396596, "maximum indexes per table ({0}) exceeded"},
	{336396597, "new record size of {0} bytes is too big"},
	{336396598, "segments not allowed in expression index {0}"},
	{336396599, "wrong page type"},
	{336396603, "invalid ARRAY or BLOB operation"},
	{336396611, "{0} extension error"},
	{336396624, "key size exceeds implementation restriction for index \"{0}\""},
	{336396625, "definition error for index {0}"},
	{336396628, "cannot create index"},
	{336396651, "duplicate specification of {0} - not supported"},
	{336396663, "The insert failed because a column definition includes validation constraints."},
	{336396670, "Cannot delete object referenced by another object"},
	{336396671, "Cannot modify object referenced by another object"},
	{336396672, "Object is referenced by another object"},
	{336396673, "lock on conflicts with existing lock"},
	{336396681, "This operation is not defined for system tables."},
	{336396683, "Inappropriate self-reference of column"},
	{336396684, "Illegal array dimension range"},
	{336396687, "database or file exists"},
	{336396688, "sort error: corruption in data structure"},
	{336396689, "node not supported"},
	{336396690, "Shadow number must be a positive integer"},
	{336396691, "Preceding file did not specify length, so {0} must include starting page number"},
	{336396692, "illegal operation when at beginning of stream"},
	{336396693, "the current position is on a crack"},
	{336396735, "cannot modify an existing user privilege"},
	{336396736, "user does not have the privilege to perform operation"},
	{336396737, "This user does not have privilege to perform this operation on this object."},
	{336396756, "transaction marked invalid by I/O error"},
	{336396757, "Cannot prepare a CREATE DATABASE/SCHEMA statement"},
	{336396758, "violation of FOREIGN KEY constraint \"{0}\""},
	{336396769, "The prepare statement identifies a prepare statement with an open cursor"},
	{336396770, "Unknown statement or request"},
	{336396778, "Attempt to update non-updatable cursor"},
	{336396780, "The cursor identified in the UPDATE or DELETE statement is not positioned on a row."},
	{336396784, "Unknown cursor"},
	{336396786, "The cursor identified in an OPEN statement is already open."},
	{336396787, "The cursor identified in a FETCH or CLOSE statement is not open."},
	{336396875, "Overflow occurred during data type conversion."},
	{336396881, "null segment of UNIQUE KEY"},
	{336396882, "subscript out of bounds"},
	{336396886, "data operation not supported"},
	{336396887, "invalid comparison operator for find operation"},
	{336396974, "Cannot transliterate character between character sets"},
	{336396975, "count of column list and variable list do not match"},
	{336396985, "Incompatible column/host variable data type"},
	{336396991, "Operation violates CHECK constraint {0} on view or table"},
	{336396992, "internal Firebird consistency check (invalid RDB$CONSTRAINT_TYPE)"},
	{336396993, "Cannot update constraints (RDB$RELATION_CONSTRAINTS)."},
	{336396994, "Cannot delete CHECK constraint entry (RDB$CHECK_CONSTRAINTS)"},
	{336396995, "Cannot update constraints (RDB$CHECK_CONSTRAINTS)."},
	{336396996, "Cannot update constraints (RDB$REF_CONSTRAINTS)."},
	{336396997, "Column used in a PRIMARY constraint must be NOT NULL."},
	{336397004, "index {0} cannot be used in the specified plan"},
	{336397005, "table {0} is referenced in the plan but not the from list"},
	{336397006, "the table {0} is referenced twice; use aliases to differentiate"},
	{336397007, "table {0} is not referenced in plan"},
	{336397027, "Log file specification partition error"},
	{336397028, "Cache or Log redefined"},
	{336397029, "Write-ahead Log with shadowing configuration not allowed"},
	{336397030, "Overflow log specification required for round-robin log"},
	{336397031, "WAL defined; Cache Manager must be started first"},
	{336397033, "Write-ahead Log without shared cache configuration not allowed"},
	{336397034, "Cannot start WAL writer for the database {0}"},
	{336397035, "WAL writer synchronization error for the database {0}"},
	{336397036, "WAL setup error.  Please see Firebird log."},
	{336397037, "WAL buffers cannot be increased.  Please see Firebird log."},
	{336397038, "WAL writer - Journal server communication error.  Please see Firebird log."},
	{336397039, "WAL I/O error.  Please see Firebird log."},
	{336397040, "Unable to roll over; please see Firebird log."},
	{336397041, "obsolete"},
	{336397042, "obsolete"},
	{336397043, "obsolete"},
	{336397044, "obsolete"},
	{336397045, "database does not use Write-ahead Log"},
	{336397046, "Cannot roll over to the next log file {0}"},
	{336397047, "obsolete"},
	{336397048, "obsolete"},
	{336397049, "Cache or Log size too small"},
	{336397050, "Log record header too small at offset {0} in log file {1}"},
	{336397051, "Incomplete log record at offset {0} in log file {1}"},
	{336397052, "Unexpected end of log file {0} at offset {1}"},
	{336397053, "Database name in the log file {0} is different"},
	{336397054, "Log file {0} not closed properly; database recovery may be required"},
	{336397055, "Log file {0} not latest in the chain but open flag still set"},
	{336397056, "Invalid version of log file {0}"},
	{336397057, "Log file header of {0} too small"},
	{336397058, "obsolete"},
	{336397069, "table {0} is not defined"},
	{336397080, "invalid ORDER BY clause"},
	{336397082, "Column does not belong to referenced table"},
	{336397083, "column {0} is not defined in table {1}"},
	{336397084, "Undefined name"},
	{336397085, "Ambiguous column reference."},
	{336397116, "function {0} is not defined"},
	{336397117, "Invalid data type, length, or value"},
	{336397118, "Invalid number of arguments"},
	{336397126, "dbkey not available for multi-table views"},
	{336397130, "number of columns does not match select list"},
	{336397131, "must specify column name for view select expression"},
	{336397133, "{0} is not a valid base table of the specified view"},
	{336397137, "This column cannot be updated because it is derived from an SQL function or expression."},
	{336397138, "The object of the INSERT, DELETE or UPDATE statement is a view for which the requested operation is not permitted."},
	{336397183, "Invalid String"},
	{336397184, "Invalid token"},
	{336397185, "Invalid numeric literal"},
	{336397203, "An error occurred while trying to update the security database"},
	{336397204, "non-SQL security class defined"},
	{336397205, "ODS versions before ODS{0} are not supported"},		/* dsql_too_old_ods */
	{336397206, "Table {0} does not exist"},		/* dsql_table_not_found */
	{336397207, "View {0} does not exist"},		/* dsql_view_not_found */
	{336397208, "At line {0}, column {1}"},		/* dsql_line_col_error */
	{336397209, "At unknown line and column"},		/* dsql_unknown_pos */
	{336397210, "Column {0} cannot be repeated in {1} statement"},		/* dsql_no_dup_name */
	{336397211, "Too many values (more than {0}) in member list to match against"},		/* dsql_too_many_values */
	{336397212, "Array and BLOB data types not allowed in computed field"},		/* dsql_no_array_computed */
	{336397213, "Implicit domain name {0} not allowed in user created domain"},		/* dsql_implicit_domain_name */
	{336397214, "scalar operator used on field {0} which is not an array"},		/* dsql_only_can_subscript_array */
	{336397215, "cannot sort on more than 255 items"},		/* dsql_max_sort_items */
	{336397216, "cannot group on more than 255 items"},		/* dsql_max_group_items */
	{336397217, "Cannot include the same field ({0}.{1}) twice in the ORDER BY clause with conflicting sorting options"},		/* dsql_conflicting_sort_field */
	{336397218, "column list from derived table {0} has more columns than the number of items in its SELECT statement"},		/* dsql_derived_table_more_columns */
	{336397219, "column list from derived table {0} has less columns than the number of items in its SELECT statement"},		/* dsql_derived_table_less_columns */
	{336397220, "no column name specified for column number {0} in derived table {1}"},		/* dsql_derived_field_unnamed */
	{336397221, "column {0} was specified multiple times for derived table {1}"},		/* dsql_derived_field_dup_name */
	{336397222, "Internal dsql error: alias type expected by pass1_expand_select_node"},		/* dsql_derived_alias_select */
	{336397223, "Internal dsql error: alias type expected by pass1_field"},		/* dsql_derived_alias_field */
	{336397224, "Internal dsql error: column position out of range in pass1_union_auto_cast"},		/* dsql_auto_field_bad_pos */
	{336397225, "Recursive CTE member ({0}) can refer itself only in FROM clause"},		/* dsql_cte_wrong_reference */
	{336397226, "CTE '{0}' has cyclic dependencies"},		/* dsql_cte_cycle */
	{336397227, "Recursive member of CTE can't be member of an outer join"},		/* dsql_cte_outer_join */
	{336397228, "Recursive member of CTE can't reference itself more than once"},		/* dsql_cte_mult_references */
	{336397229, "Recursive CTE ({0}) must be an UNION"},		/* dsql_cte_not_a_union */
	{336397230, "CTE '{0}' defined non-recursive member after recursive"},		/* dsql_cte_nonrecurs_after_recurs */
	{336397231, "Recursive member of CTE '{0}' has {1} clause"},		/* dsql_cte_wrong_clause */
	{336397232, "Recursive members of CTE ({0}) must be linked with another members via UNION ALL"},		/* dsql_cte_union_all */
	{336397233, "Non-recursive member is missing in CTE '{0}'"},		/* dsql_cte_miss_nonrecursive */
	{336397234, "WITH clause can't be nested"},		/* dsql_cte_nested_with */
	{336397235, "column {0} appears more than once in USING clause"},		/* dsql_col_more_than_once_using */
	{336397236, "feature is not supported in dialect {0}"},		/* dsql_unsupp_feature_dialect */
	{336397237, "CTE \"{0}\" is not used in query"},		/* dsql_cte_not_used */
	{336397238, "column {0} appears more than once in ALTER VIEW"},		/* dsql_col_more_than_once_view */
	{336397239, "{0} is not supported inside IN AUTONOMOUS TRANSACTION block"},		/* dsql_unsupported_in_auto_trans */
	{336397240, "Unknown node type {0} in dsql/GEN_expr"},		/* dsql_eval_unknode */
	{336397241, "Argument for {0} in dialect 1 must be string or numeric"},		/* dsql_agg_wrongarg */
	{336397242, "Argument for {0} in dialect 3 must be numeric"},		/* dsql_agg2_wrongarg */
	{336397243, "Strings cannot be added to or subtracted from DATE or TIME types"},		/* dsql_nodateortime_pm_string */
	{336397244, "Invalid data type for subtraction involving DATE, TIME or TIMESTAMP types"},		/* dsql_invalid_datetime_subtract */
	{336397245, "Adding two DATE values or two TIME values is not allowed"},		/* dsql_invalid_dateortime_add */
	{336397246, "DATE value cannot be subtracted from the provided data type"},		/* dsql_invalid_type_minus_date */
	{336397247, "Strings cannot be added or subtracted in dialect 3"},		/* dsql_nostring_addsub_dial3 */
	{336397248, "Invalid data type for addition or subtraction in dialect 3"},		/* dsql_invalid_type_addsub_dial3 */
	{336397249, "Invalid data type for multiplication in dialect 1"},		/* dsql_invalid_type_multip_dial1 */
	{336397250, "Strings cannot be multiplied in dialect 3"},		/* dsql_nostring_multip_dial3 */
	{336397251, "Invalid data type for multiplication in dialect 3"},		/* dsql_invalid_type_multip_dial3 */
	{336397252, "Division in dialect 1 must be between numeric data types"},		/* dsql_mustuse_numeric_div_dial1 */
	{336397253, "Strings cannot be divided in dialect 3"},		/* dsql_nostring_div_dial3 */
	{336397254, "Invalid data type for division in dialect 3"},		/* dsql_invalid_type_div_dial3 */
	{336397255, "Strings cannot be negated (applied the minus operator) in dialect 3"},		/* dsql_nostring_neg_dial3 */
	{336397256, "Invalid data type for negation (minus operator)"},		/* dsql_invalid_type_neg */
	{336397257, "Cannot have more than 255 items in DISTINCT / UNION DISTINCT list"},		/* dsql_max_distinct_items */
	{336397258, "ALTER CHARACTER SET {0} failed"},		/* dsql_alter_charset_failed */
	{336397259, "COMMENT ON {0} failed"},		/* dsql_comment_on_failed */
	{336397260, "CREATE FUNCTION {0} failed"},		/* dsql_create_func_failed */
	{336397261, "ALTER FUNCTION {0} failed"},		/* dsql_alter_func_failed */
	{336397262, "CREATE OR ALTER FUNCTION {0} failed"},		/* dsql_create_alter_func_failed */
	{336397263, "DROP FUNCTION {0} failed"},		/* dsql_drop_func_failed */
	{336397264, "RECREATE FUNCTION {0} failed"},		/* dsql_recreate_func_failed */
	{336397265, "CREATE PROCEDURE {0} failed"},		/* dsql_create_proc_failed */
	{336397266, "ALTER PROCEDURE {0} failed"},		/* dsql_alter_proc_failed */
	{336397267, "CREATE OR ALTER PROCEDURE {0} failed"},		/* dsql_create_alter_proc_failed */
	{336397268, "DROP PROCEDURE {0} failed"},		/* dsql_drop_proc_failed */
	{336397269, "RECREATE PROCEDURE {0} failed"},		/* dsql_recreate_proc_failed */
	{336397270, "CREATE TRIGGER {0} failed"},		/* dsql_create_trigger_failed */
	{336397271, "ALTER TRIGGER {0} failed"},		/* dsql_alter_trigger_failed */
	{336397272, "CREATE OR ALTER TRIGGER {0} failed"},		/* dsql_create_alter_trigger_failed */
	{336397273, "DROP TRIGGER {0} failed"},		/* dsql_drop_trigger_failed */
	{336397274, "RECREATE TRIGGER {0} failed"},		/* dsql_recreate_trigger_failed */
	{336397275, "CREATE COLLATION {0} failed"},		/* dsql_create_collation_failed */
	{336397276, "DROP COLLATION {0} failed"},		/* dsql_drop_collation_failed */
	{336397277, "CREATE DOMAIN {0} failed"},		/* dsql_create_domain_failed */
	{336397278, "ALTER DOMAIN {0} failed"},		/* dsql_alter_domain_failed */
	{336397279, "DROP DOMAIN {0} failed"},		/* dsql_drop_domain_failed */
	{336397280, "CREATE EXCEPTION {0} failed"},		/* dsql_create_except_failed */
	{336397281, "ALTER EXCEPTION {0} failed"},		/* dsql_alter_except_failed */
	{336397282, "CREATE OR ALTER EXCEPTION {0} failed"},		/* dsql_create_alter_except_failed */
	{336397283, "RECREATE EXCEPTION {0} failed"},		/* dsql_recreate_except_failed */
	{336397284, "DROP EXCEPTION {0} failed"},		/* dsql_drop_except_failed */
	{336397285, "CREATE SEQUENCE {0} failed"},		/* dsql_create_sequence_failed */
	{336397286, "CREATE TABLE {0} failed"},		/* dsql_create_table_failed */
	{336397287, "ALTER TABLE {0} failed"},		/* dsql_alter_table_failed */
	{336397288, "DROP TABLE {0} failed"},		/* dsql_drop_table_failed */
	{336397289, "RECREATE TABLE {0} failed"},		/* dsql_recreate_table_failed */
	{336397290, "CREATE PACKAGE {0} failed"},		/* dsql_create_pack_failed */
	{336397291, "ALTER PACKAGE {0} failed"},		/* dsql_alter_pack_failed */
	{336397292, "CREATE OR ALTER PACKAGE {0} failed"},		/* dsql_create_alter_pack_failed */
	{336397293, "DROP PACKAGE {0} failed"},		/* dsql_drop_pack_failed */
	{336397294, "RECREATE PACKAGE {0} failed"},		/* dsql_recreate_pack_failed */
	{336397295, "CREATE PACKAGE BODY {0} failed"},		/* dsql_create_pack_body_failed */
	{336397296, "DROP PACKAGE BODY {0} failed"},		/* dsql_drop_pack_body_failed */
	{336397297, "RECREATE PACKAGE BODY {0} failed"},		/* dsql_recreate_pack_body_failed */
	{336397298, "CREATE VIEW {0} failed"},		/* dsql_create_view_failed */
	{336397299, "ALTER VIEW {0} failed"},		/* dsql_alter_view_failed */
	{336397300, "CREATE OR ALTER VIEW {0} failed"},		/* dsql_create_alter_view_failed */
	{336397301, "RECREATE VIEW {0} failed"},		/* dsql_recreate_view_failed */
	{336397302, "DROP VIEW {0} failed"},		/* dsql_drop_view_failed */
	{336397303, "DROP SEQUENCE {0} failed"},		/* dsql_drop_sequence_failed */
	{336397304, "RECREATE SEQUENCE {0} failed"},		/* dsql_recreate_sequence_failed */
	{336397305, "DROP INDEX {0} failed"},		/* dsql_drop_index_failed */
	{336397306, "DROP FILTER {0} failed"},		/* dsql_drop_filter_failed */
	{336397307, "DROP SHADOW {0} failed"},		/* dsql_drop_shadow_failed */
	{336397308, "DROP ROLE {0} failed"},		/* dsql_drop_role_failed */
	{336397309, "DROP USER {0} failed"},		/* dsql_drop_user_failed */
	{336397310, "CREATE ROLE {0} failed"},		/* dsql_create_role_failed */
	{336397311, "ALTER ROLE {0} failed"},		/* dsql_alter_role_failed */
	{336397312, "ALTER INDEX {0} failed"},		/* dsql_alter_index_failed */
	{336397313, "ALTER DATABASE failed"},		/* dsql_alter_database_failed */
	{336397314, "CREATE SHADOW {0} failed"},		/* dsql_create_shadow_failed */
	{336397315, "DECLARE FILTER {0} failed"},		/* dsql_create_filter_failed */
	{336397316, "CREATE INDEX {0} failed"},		/* dsql_create_index_failed */
	{336397317, "CREATE USER {0} failed"},		/* dsql_create_user_failed */
	{336397318, "ALTER USER {0} failed"},		/* dsql_alter_user_failed */
	{336397319, "GRANT failed"},		/* dsql_grant_failed */
	{336397320, "REVOKE failed"},		/* dsql_revoke_failed */
	{336397321, "Recursive member of CTE cannot use aggregate or window function"},		/* dsql_cte_recursive_aggregate */
	{336397322, "{1} MAPPING {0} failed"},		/* dsql_mapping_failed */
	{336397323, "ALTER SEQUENCE {0} failed"},		/* dsql_alter_sequence_failed */
	{336397324, "CREATE GENERATOR {0} failed"},		/* dsql_create_generator_failed */
	{336397325, "SET GENERATOR {0} failed"},		/* dsql_set_generator_failed */
	{336397326, "WITH LOCK can be used only with a single physical table"},		/* dsql_wlock_simple */
	{336397327, "FIRST/SKIP cannot be used with OFFSET/FETCH or ROWS"},		/* dsql_firstskip_rows */
	{336397328, "WITH LOCK cannot be used with aggregates"},		/* dsql_wlock_aggregates */
	{336397329, "WITH LOCK cannot be used with {0}"},		/* dsql_wlock_conflict */
	{336397330, "Number of arguments ({0}) exceeds the maximum ({1}) number of EXCEPTION USING arguments"},		/* dsql_max_exception_arguments */
	{336397331, "String literal with {0} bytes exceeds the maximum length of {1} bytes"},		/* dsql_string_byte_length */
	{336397332, "String literal with {0} characters exceeds the maximum length of {1} characters for the {2} character set"},		/* dsql_string_char_length */
	{336397333, "Too many BEGIN...END nesting. Maximum level is {0}"},		/* dsql_max_nesting */
	{336397334, "RECREATE USER {0} failed"},		/* dsql_recreate_user_failed */
	{336461924, "Row not found for fetch, update or delete, or the result of a query is an empty table."},
	{336461925, "segment buffer length shorter than expected"},
	{336462125, "Datatype needs modification"},
	{336462436, "Duplicate column or domain name found."},
	{336527507, "invalid block type encountered"},
	{336527508, "wrong packet type"},
	{336527509, "cannot map page"},
	{336527510, "request to allocate invalid block type"},
	{336527511, "request to allocate block type larger than maximum size"},
	{336527512, "memory pool free list is invalid"},
	{336527513, "invalid pool id encountered"},
	{336527514, "attempt to release free block"},
	{336527515, "attempt to release block overlapping following free block"},
	{336527516, "attempt to release block overlapping prior free block"},
	{336527517, "cannot sort on a field that does not exist"},
	{336527518, "database file not available"},
	{336527519, "cannot assert logical lock"},
	{336527520, "wrong ACL version"},
	{336527521, "shadow block not found"},
	{336527522, "shadow lock not synchronized properly"},
	{336527523, "root file name not listed for shadow"},
	{336527524, "failed to remove symbol from hash table"},
	{336527525, "cannot find tip page"},
	{336527526, "invalid rsb type"},
	{336527527, "invalid SEND request"},
	{336527528, "looper: action not yet implemented"},
	{336527529, "return data type not supported"},
	{336527530, "unexpected reply from journal server"},
	{336527531, "journal server is incompatible version"},
	{336527532, "journal server refused connection"},
	{336527533, "referenced index description not found"},
	{336527534, "index key too big"},
	{336527535, "partner index description not found"},
	{336527536, "bad difference record"},
	{336527537, "applied differences will not fit in record"},
	{336527538, "record length inconsistent"},
	{336527539, "decompression overran buffer"},
	{336527540, "cannot reposition for update after sort for RMS"},
	{336527541, "external access type not implemented"},
	{336527542, "differences record too long"},
	{336527543, "wrong record length"},
	{336527544, "limbo impossible"},
	{336527545, "wrong record version"},
	{336527546, "record disappeared"},
	{336527547, "cannot delete system tables"},
	{336527548, "cannot update erased record"},
	{336527549, "comparison not supported for specified data types"},
	{336527550, "conversion not supported for specified data types"},
	{336527551, "conversion error"},
	{336527552, "overflow during conversion"},
	{336527553, "null or invalid array"},
	{336527554, "BLOB not found"},
	{336527555, "cannot update old BLOB"},
	{336527556, "relation for array not known"},
	{336527557, "field for array not known"},
	{336527558, "array subscript computation error"},
	{336527559, "expected field node"},
	{336527560, "invalid BLOB ID"},
	{336527561, "cannot find BLOB page"},
	{336527562, "unknown data type"},
	{336527563, "shadow block not found for extend file"},
	{336527564, "index inconsistent"},
	{336527565, "index bucket overfilled"},
	{336527566, "exceeded index level"},
	{336527567, "page already in use"},
	{336527568, "page not accessed for write"},
	{336527569, "attempt to release page not acquired"},
	{336527570, "page in use during flush"},
	{336527571, "attempt to remove page from dirty page list when not there"},
	{336527572, "CCH_precedence: block marked"},
	{336527573, "insufficient cache size"},
	{336527574, "no cache buffers available for reuse"},
	{336527575, "page {0}, page type {1} lock conversion denied"},
	{336527576, "page {0}, page type {1} lock denied"},
	{336527577, "buffer marked for update"},
	{336527578, "CCH: {0}, status = {1} (218)"},
	{336527579, "request of unknown resource"},
	{336527580, "release of unknown resource"},
	{336527581, "(CMP) copy: cannot remap"},
	{336527582, "bad BLR -- invalid stream"},
	{336527583, "argument of scalar operation must be an array"},
	{336527584, "quad word arithmetic not supported"},
	{336527585, "data type not supported for arithmetic"},
	{336527586, "request size limit exceeded"},
	{336527587, "cannot access field {0} in view {1}"},
	{336527588, "cannot access field in view {0}"},
	{336527589, "EVL_assign_to: invalid operation"},
	{336527590, "EVL_bitmap: invalid operation"},
	{336527591, "EVL_boolean: invalid operation"},
	{336527592, "EVL_expr: invalid operation"},
	{336527593, "eval_statistical: invalid operation"},
	{336527594, "Unimplemented conversion, FAO directive O,Z,S"},
	{336527595, "Unimplemented conversion, FAO directive X,U"},
	{336527596, "Error parsing RDB FAO msg string"},
	{336527597, "Error parsing RDB FAO msg str"},
	{336527598, "unknown parameter in RdB status vector"},
	{336527599, "Firebird status vector inconsistent"},
	{336527600, "Firebird/RdB message parameter inconsistency"},
	{336527601, "error parsing RDB FAO message string"},
	{336527602, "unimplemented FAO directive"},
	{336527603, "missing pointer page in DPM_data_pages"},
	{336527604, "Fragment does not exist"},
	{336527605, "pointer page disappeared in DPM_delete"},
	{336527606, "pointer page lost from DPM_delete_relation"},
	{336527607, "missing pointer page in DPM_dump"},
	{336527608, "cannot find record fragment"},
	{336527609, "pointer page vanished from DPM_next"},
	{336527610, "temporary page buffer too small"},
	{336527611, "damaged data page"},
	{336527612, "header fragment length changed"},
	{336527613, "pointer page vanished from extend_relation"},
	{336527614, "pointer page vanished from relation list in locate_space"},
	{336527615, "cannot find free space"},
	{336527616, "pointer page vanished from mark_full"},
	{336527617, "bad record in RDB$PAGES"},
	{336527618, "page slot not empty"},
	{336527619, "bad pointer page"},
	{336527620, "index unexpectedly deleted"},
	{336527621, "scalar operator used on field which is not an array"},
	{336527622, "active"},
	{336527623, "committed"},
	{336527624, "rolled back"},
	{336527625, "in an ill-defined state"},
	{336527626, "next transaction older than oldest active transaction"},
	{336527627, "next transaction older than oldest transaction"},
	{336527628, "buffer marked during cache unwind"},
	{336527629, "error in recovery! database corrupted"},
	{336527630, "error in recovery! wrong data page record"},
	{336527631, "error in recovery! no space on data page"},
	{336527632, "error in recovery! wrong header page record"},
	{336527633, "error in recovery! wrong generator page record"},
	{336527634, "error in recovery! wrong b-tree page record"},
	{336527635, "error in recovery! wrong page inventory page record"},
	{336527636, "error in recovery! wrong pointer page record"},
	{336527637, "error in recovery! wrong index root page record"},
	{336527638, "error in recovery! wrong transaction page record"},
	{336527639, "error in recovery! out of sequence log record encountered"},
	{336527640, "error in recovery! unknown page type"},
	{336527641, "error in recovery! unknown record type"},
	{336527642, "journal server cannot archive to specified archive directory"},
	{336527643, "checksum error in log record when reading from log file"},
	{336527644, "cannot restore singleton select data"},
	{336527645, "lock not found in internal lock manager"},
	{336527646, "size of opt block exceeded"},
	{336527647, "Too many savepoints"},
	{336527648, "garbage collect record disappeared"},
	{336527649, "Unknown BLOB FILTER ACTION_"},
	{336527650, "error during savepoint backout"},		/* savepoint_error */
	{336527651, "cannot find record back version"},
	{336527652, "Illegal user_type."},
	{336527653, "bad ACL"},
	{336527654, "inconsistent LATCH_mark release"},
	{336527655, "inconsistent LATCH_mark call"},
	{336527656, "inconsistent latch downgrade call"},
	{336527657, "bdb is unexpectedly marked"},
	{336527658, "missing exclusive latch"},
	{336527659, "exceeded maximum number of shared latches on a bdb"},
	{336527660, "can't find shared latch"},
	{336527661, "Non-zero use_count of a buffer in the empty que"},		/* cache_non_zero_use_count */
	{336527662, "Unexpected page change from latching"},		/* unexpected_page_change */
	{336527663, "Invalid expression for evaluation"},
	{336527664, "RDB$FLAGS for trigger {0} in RDB$TRIGGERS is corrupted"},		/* rdb$triggers_rdb$flags_corrupt */
	{336527665, "Blobs accounting is inconsistent"},
	{336527666, "Found array data type with more than 16 dimensions"},
	{336658432, "Statement failed, SQLSTATE = {0}"},		/* GEN_ERR */
	{336658433, "usage:    isql [options] [<database>]"},		/* USAGE */
	{336658434, "Unknown switch: {0}"},		/* SWITCH */
	{336658435, "Use CONNECT or CREATE DATABASE to specify a database"},		/* NO_DB */
	{336658436, "Unable to open {0}"},		/* FILE_OPEN_ERR */
	{336658437, "Commit current transaction (y/n)?"},		/* COMMIT_PROMPT */
	{336658438, "Committing."},		/* COMMIT_MSG */
	{336658439, "Rolling back work."},		/* ROLLBACK_MSG */
	{336658440, "Command error: {0}"},		/* CMD_ERR */
	{336658441, "Enter data or NULL for each column.  RETURN to end."},		/* ADD_PROMPT */
	{336658442, "ISQL Version: {0}"},		/* VERSION */
	{336658443, "\t-a(ll)                  extract metadata incl. legacy non-SQL tables"},		/* USAGE_ALL */
	{336658444, "Number of DB pages allocated = {0}"},		/* NUMBER_PAGES */
	{336658445, "Sweep interval = {0}"},		/* SWEEP_INTERV */
	{336658446, "Number of wal buffers = {0}"},		/* NUM_WAL_BUFF */
	{336658447, "Wal buffer size = {0}"},		/* WAL_BUFF_SIZE */
	{336658448, "Check point length = {0}"},		/* CKPT_LENGTH */
	{336658449, "Check point interval = {0}"},		/* CKPT_INTERV */
	{336658450, "Wal group commit wait = {0}"},		/* WAL_GRPC_WAIT */
	{336658451, "Base level = {0}"},		/* BASE_LEVEL */
	{336658452, "Transaction in limbo = {0}"},		/* LIMBO */
	{336658453, "Frontend commands:"},		/* HLP_FRONTEND */
	{336658454, "BLOBVIEW <blobid>          -- view BLOB in text editor"},		/* HLP_BLOBED */
	{336658455, "BLOBDUMP <blobid> <file>   -- dump BLOB to a file"},		/* HLP_BLOBDMP */
	{336658456, "EDIT     [<filename>]      -- edit SQL script file and execute"},		/* HLP_EDIT */
	{336658457, "INput    <filename>        -- take input from the named SQL file"},		/* HLP_INPUT */
	{336658458, "OUTput   [<filename>]      -- write output to named file"},		/* HLP_OUTPUT */
	{336658459, "SHELL    <command>         -- execute Operating System command in sub-shell"},		/* HLP_SHELL */
	{336658460, "HELP                       -- display this menu"},		/* HLP_HELP */
	{336658461, "Set commands:"},		/* HLP_SETCOM */
	{336658462, "    SET                    -- display current SET options"},		/* HLP_SET */
	{336658463, "    SET AUTOddl            -- toggle autocommit of DDL statements"},		/* HLP_SETAUTO */
	{336658464, "    SET BLOB [ALL|<n>]     -- display BLOBS of subtype <n> or ALL"},		/* HLP_SETBLOB */
	{336658465, "    SET COUNT              -- toggle count of selected rows on/off"},		/* HLP_SETCOUNT */
	{336658466, "    SET ECHO               -- toggle command echo on/off"},		/* HLP_SETECHO */
	{336658467, "    SET STATs              -- toggle display of performance statistics"},		/* HLP_SETSTAT */
	{336658468, "    SET TERM <string>      -- change statement terminator string"},		/* HLP_SETTERM */
	{336658469, "SHOW     <object> [<name>] -- display system information"},		/* HLP_SHOW */
	{336658470, "    <object> = CHECK, COLLATION, DATABASE, DOMAIN, EXCEPTION, FILTER, FUNCTION,"},		/* HLP_OBJTYPE */
	{336658471, "EXIT                       -- exit and commit changes"},		/* HLP_EXIT */
	{336658472, "QUIT                       -- exit and roll back changes"},		/* HLP_QUIT */
	{336658473, "All commands may be abbreviated to letters in CAPitals"},		/* HLP_ALL */
	{336658474, "\tSET SCHema/DB <db name> -- changes current database"},		/* HLP_SETSCHEMA */
	{336658475, "Yes"},		/* YES_ANS */
	{336658476, "Current memory = !c\nDelta memory = !d\nMax memory = !x\nElapsed time = !e sec\n"},		/* REPORT1 */
	{336658477, "Cpu = !u sec\nBuffers = !b\nReads = !r\nWrites = !w\nFetches = !f"},		/* REPORT2 */
	{336658478, "BLOB display set to subtype {0}. This BLOB: subtype = {1}"},		/* BLOB_SUBTYPE */
	{336658479, "BLOB: {0}, type 'edit' or filename to load>"},		/* BLOB_PROMPT */
	{336658480, "Enter {0} as Y/M/D>"},		/* DATE_PROMPT */
	{336658481, "Enter {0}>"},		/* NAME_PROMPT */
	{336658482, "Bad date {0}"},		/* DATE_ERR */
	{336658483, "CON> "},		/* CON_PROMPT */
	{336658484, "    SET LIST               -- toggle column or table display format"},		/* HLP_SETLIST */
	{336658485, "{0} not found"},		/* NOT_FOUND */
	{336658486, "Errors occurred (possibly duplicate domains) in creating {0} in {1}"},		/* COPY_ERR */
	{336658487, "Server version too old to support the isql command"},		/* SERVER_TOO_OLD */
	{336658488, "Records affected: {0}"},		/* REC_COUNT */
	{336658489, "Unlicensed for database \"{0}\""},		/* UNLICENSED */
	{336658490, "    SET WIDTH <col> [<n>]  -- set/unset print width to <n> for column <col>"},		/* HLP_SETWIDTH */
	{336658491, "    SET PLAN               -- toggle display of query access plan"},		/* HLP_SETPLAN */
	{336658492, "    SET TIME               -- toggle display of timestamp with DATE values"},		/* HLP_SETTIME */
	{336658493, "EDIT                       -- edit current command buffer and execute"},		/* HLP_EDIT2 */
	{336658494, "OUTput                     -- return output to stdout"},		/* HLP_OUTPUT2 */
	{336658495, "    SET NAMES <csname>     -- set name of runtime character set"},		/* HLP_SETNAMES */
	{336658496, "               GENERATOR, GRANT, INDEX, PACKAGE, PROCEDURE, ROLE, SQL DIALECT,"},		/* HLP_OBJTYPE2 */
	{336658497, "    SET BLOB               -- turn off BLOB display"},		/* HLP_SETBLOB2 */
	{336658498, "SET      <option>          -- (Use HELP SET for complete list)"},		/* HLP_SET_ROOT */
	{336658499, "There are no tables in this database"},		/* NO_TABLES */
	{336658500, "There is no table {0} in this database"},		/* NO_TABLE */
	{336658501, "There are no views in this database"},		/* NO_VIEWS */
	{336658502, "There is no view {0} in this database"},		/* NO_VIEW */
	{336658503, "There are no indices on table {0} in this database"},		/* NO_INDICES_ON_REL */
	{336658504, "There is no table or index {0} in this database"},		/* NO_REL_OR_INDEX */
	{336658505, "There are no indices in this database"},		/* NO_INDICES */
	{336658506, "There is no domain {0} in this database"},		/* NO_DOMAIN */
	{336658507, "There are no domains in this database"},		/* NO_DOMAINS */
	{336658508, "There is no exception {0} in this database"},		/* NO_EXCEPTION */
	{336658509, "There are no exceptions in this database"},		/* NO_EXCEPTIONS */
	{336658510, "There is no filter {0} in this database"},		/* NO_FILTER */
	{336658511, "There are no filters in this database"},		/* NO_FILTERS */
	{336658512, "There is no user-defined function {0} in this database"},		/* NO_FUNCTION */
	{336658513, "There are no user-defined functions in this database"},		/* NO_FUNCTIONS */
	{336658514, "There is no generator {0} in this database"},		/* NO_GEN */
	{336658515, "There are no generators in this database"},		/* NO_GENS */
	{336658516, "There is no privilege granted on table {0} in this database"},		/* NO_GRANT_ON_REL */
	{336658517, "There is no privilege granted on stored procedure {0} in this database"},		/* NO_GRANT_ON_PROC */
	{336658518, "There is no table or stored procedure {0} in this database"},		/* NO_REL_OR_PROC */
	{336658519, "There is no stored procedure {0} in this database"},		/* NO_PROC */
	{336658520, "There are no stored procedures in this database"},		/* NO_PROCS */
	{336658521, "There are no triggers on table {0} in this database"},		/* NO_TRIGGERS_ON_REL */
	{336658522, "There is no table or trigger {0} in this database"},		/* NO_REL_OR_TRIGGER */
	{336658523, "There are no triggers in this database"},		/* NO_TRIGGERS */
	{336658524, "There are no check constraints on table {0} in this database"},		/* NO_CHECKS_ON_REL */
	{336658525, "Buffers = !b\nReads = !r\nWrites !w\nFetches = !f"},		/* REPORT2_WINDOWS_ONLY */
	{336658526, "Single isql command exceeded maximum buffer size"},		/* BUFFER_OVERFLOW */
	{336658527, "There are no roles in this database"},		/* NO_ROLES */
	{336658528, "There is no metadata object {0} in this database"},		/* NO_OBJECT */
	{336658529, "There is no membership privilege granted on {0} in this database"},		/* NO_GRANT_ON_ROL */
	{336658530, "Expected end of statement, encountered EOF"},		/* UNEXPECTED_EOF */
	{336658533, "Bad TIME: {0}"},		/* TIME_ERR */
	{336658534, "               SYSTEM, TABLE, TRIGGER, VERSION, USERS, VIEW, WIRE_STATISTICS"},		/* HLP_OBJTYPE3 */
	{336658535, "There is no role {0} in this database"},		/* NO_ROLE */
	{336658536, "\t-b(ail)                 bail on errors (set bail on)"},		/* USAGE_BAIL */
	{336658537, "Incomplete string in {0}"},
	{336658538, "    SET SQL DIALECT <n>    -- set sql dialect to <n>"},		/* HLP_SETSQLDIALECT */
	{336658539, "There is no privilege granted in this database"},		/* NO_GRANT_ON_ANY */
	{336658540, "    SET PLANONLY           -- toggle display of query plan without executing"},		/* HLP_SETPLANONLY */
	{336658541, "    SET HEADING            -- toggle display of query column titles"},		/* HLP_SETHEADING */
	{336658542, "    SET BAIL               -- toggle bailing out on errors in non-interactive mode"},		/* HLP_SETBAIL */
	{336658543, "\t-c(ache) <num>          number of cache buffers"},		/* USAGE_CACHE */
	{336658544, "Enter {0} as H:M:S>"},		/* TIME_PROMPT */
	{336658545, "Enter {0} as Y/MON/D H:MIN:S[.MSEC]>"},		/* TIMESTAMP_PROMPT */
	{336658546, "Bad TIMESTAMP: {0}"},		/* TIMESTAMP_ERR */
	{336658547, "There are no comments for objects in this database"},		/* NO_COMMENTS */
	{336658548, "Printing only the first {0} blobs."},		/* ONLY_FIRST_BLOBS */
	{336658549, "Tables:"},		/* MSG_TABLES */
	{336658550, "Functions:"},		/* MSG_FUNCTIONS */
	{336658551, "At line {0} in file {1}"},		/* EXACTLINE */
	{336658552, "After line {0} in file {1}"},		/* AFTERLINE */
	{336658553, "There is no trigger {0} in this database"},		/* NO_TRIGGER */
	{336658554, "\t-ch(arset) <charset>    connection charset (set names)"},		/* USAGE_CHARSET */
	{336658555, "\t-d(atabase) <database>  database name to put in script creation"},		/* USAGE_DATABASE */
	{336658556, "\t-e(cho)                 echo commands (set echo on)"},		/* USAGE_ECHO */
	{336658557, "\t-ex(tract)              extract metadata"},		/* USAGE_EXTRACT */
	{336658558, "\t-i(nput) <file>         input file (set input)"},		/* USAGE_INPUT */
	{336658559, "\t-m(erge)                merge standard error"},		/* USAGE_MERGE */
	{336658560, "\t-m2                     merge diagnostic"},		/* USAGE_MERGE2 */
	{336658561, "\t-n(oautocommit)         no autocommit DDL (set autoddl off)"},		/* USAGE_NOAUTOCOMMIT */
	{336658562, "\t-now(arnings)           do not show warnings"},		/* USAGE_NOWARN */
	{336658563, "\t-o(utput) <file>        output file (set output)"},		/* USAGE_OUTPUT */
	{336658564, "\t-pag(elength) <size>    page length"},		/* USAGE_PAGE */
	{336658565, "\t-p(assword) <password>  connection password"},		/* USAGE_PASSWORD */
	{336658566, "\t-q(uiet)                do not show the message \"Use CONNECT...\""},		/* USAGE_QUIET */
	{336658567, "\t-r(ole) <role>          role name"},		/* USAGE_ROLE */
	{336658568, "\t-r2 <role>              role (uses quoted identifier)"},		/* USAGE_ROLE2 */
	{336658569, "\t-s(qldialect) <dialect> SQL dialect (set sql dialect)"},		/* USAGE_SQLDIALECT */
	{336658570, "\t-t(erminator) <term>    command terminator (set term)"},		/* USAGE_TERM */
	{336658571, "\t-u(ser) <user>          user name"},		/* USAGE_USER */
	{336658572, "\t-x                      extract metadata"},		/* USAGE_XTRACT */
	{336658573, "\t-z                      show program and server version"},		/* USAGE_VERSION */
	{336658574, "missing argument for switch \"{0}\""},		/* USAGE_NOARG */
	{336658575, "argument \"{0}\" for switch \"{1}\" is not an integer"},		/* USAGE_NOTINT */
	{336658576, "value \"{0}\" for switch \"{1}\" is out of range"},		/* USAGE_RANGE */
	{336658577, "switch \"{0}\" or its equivalent used more than once"},		/* USAGE_DUPSW */
	{336658578, "more than one database name: \"{0}\", \"{1}\""},		/* USAGE_DUPDB */
	{336658579, "No dependencies for {0} were found"},		/* NO_DEPENDENCIES */
	{336658580, "There is no collation {0} in this database"},		/* NO_COLLATION */
	{336658581, "There are no user-defined collations in this database"},		/* NO_COLLATIONS */
	{336658582, "Collations:"},		/* MSG_COLLATIONS */
	{336658583, "There are no security classes for {0}"},		/* NO_SECCLASS */
	{336658584, "There is no database-wide security class"},		/* NO_DB_WIDE_SECCLASS */
	{336658585, "Cannot get server version without database connection"},		/* CANNOT_GET_SRV_VER */
	{336658586, "\t-nod(btriggers)         do not run database triggers"},		/* USAGE_NODBTRIGGERS */
	{336658587, "\t-tr(usted)              use trusted authentication"},		/* USAGE_TRUSTED */
	{336658588, "BULK> "},		/* BULK_PROMPT */
	{336658589, "There are no connected users"},		/* NO_CONNECTED_USERS */
	{336658590, "Users in the database"},		/* USERS_IN_DB */
	{336658591, "Output was truncated"},		/* OUTPUT_TRUNCATED */
	{336658592, "Valid options are:"},		/* VALID_OPTIONS */
	{336658593, "\t-f(etch_password)       fetch password from file"},		/* USAGE_FETCH */
	{336658594, "could not open password file {0}, errno {1}"},		/* PASS_FILE_OPEN */
	{336658595, "could not read password file {0}, errno {1}"},		/* PASS_FILE_READ */
	{336658596, "empty password file {0}"},		/* EMPTY_PASS */
	{336658597, "    SET MAXROWS [<n>]      -- limit select stmt to <n> rows, zero is no limit"},		/* HLP_SETMAXROWS */
	{336658598, "There is no package {0} in this database"},		/* NO_PACKAGE */
	{336658599, "There are no packages in this database"},		/* NO_PACKAGES */
	{336658600, "There is no schema {0} in this database"},		/* NO_SCHEMA */
	{336658601, "There are no schemas in this database"},		/* NO_SCHEMAS */
	{336658602, "Unable to convert {0} to a number for MAXROWS option"},		/* MAXROWS_INVALID */
	{336658603, "Value {0} for MAXROWS is out of range. Max value is {1}"},		/* MAXROWS_OUTOF_RANGE */
	{336658604, "The value ({0}) for MAXROWS must be zero or greater"},		/* MAXROWS_NEGATIVE */
	{336658605, "    SET EXPLAIN            -- toggle display of query access plan in the explained form"},		/* HLP_SETEXPLAIN */
	{336658606, "There is no privilege granted on generator {0} in this database"},		/* NO_GRANT_ON_GEN */
	{336658607, "There is no privilege granted on exception {0} in this database"},		/* NO_GRANT_ON_XCP */
	{336658608, "There is no privilege granted on domain {0} in this database"},		/* NO_GRANT_ON_FLD */
	{336658609, "There is no privilege granted on character set {0} in this database"},		/* NO_GRANT_ON_CS */
	{336658610, "There is no privilege granted on collation {0} in this database"},		/* NO_GRANT_ON_COLL */
	{336658611, "There is no privilege granted on package {0} in this database"},		/* NO_GRANT_ON_PKG */
	{336658612, "There is no privilege granted on function {0} in this database"},		/* NO_GRANT_ON_FUN */
	{336658613, "Current memory = !\nDelta memory = !\nMax memory = !\nElapsed time = ~ sec\n"},		/* REPORT_NEW1 */
	{336658614, "Cpu = ~ sec\n"},		/* REPORT_NEW2 */
	{336658615, "Buffers = !\nReads = !\nWrites = !\nFetches = !"},		/* REPORT_NEW3 */
	{336658616, "There is no mapping {0} in this database"},		/* NO_MAP */
	{336658617, "There are no mappings in this database"},		/* NO_MAPS */
	{336658618, "Invalid characters for SET TERMINATOR are {0}"},		/* INVALID_TERM_CHARS */
	{336658619, "Records displayed: {0}"},		/* REC_DISPLAYCOUNT */
	{336658620, "Full NULL columns hidden due to RecordBuff: {0}"},		/* COLUMNS_HIDDEN */
	{336658621, "    SET RECORDBuf          -- toggle limited buffering and trimming of columns"},		/* HLP_SETRECORDBUF */
	{336658622, "Number of DB pages used = {0}"},		/* NUMBER_USED_PAGES */
	{336658623, "Number of DB pages free = {0}"},		/* NUMBER_FREE_PAGES */
	{336658624, "Database encrypted"},		/* DATABASE_CRYPTED */
	{336658625, "Database not encrypted"},		/* DATABASE_NOT_CRYPTED */
	{336658626, "crypt thread not complete"},		/* DATABASE_CRYPT_PROCESS */
	{336658627, "Roles:"},		/* MSG_ROLES */
	{336658628, "Timeouts are not supported by server"},		/* NO_TIMEOUTS */
	{336658629, "    SET KEEP_TRAN_params   -- toggle to keep or not to keep text of following successful SET TRANSACTION statement"},		/* HLP_SETKEEPTRAN */
	{336658630, "    SET PER_TABle_stats    -- toggle display of detailed per-table statistics"},		/* HLP_SETPERTAB */
	{336658631, "Statement type is not recognized"},		/* BAD_STMT_TYPE */
	{336658632, "Packages:"},		/* MSG_PACKAGES */
	{336658633, "There is no publication {0} in this database"},		/* NO_PUBLICATION */
	{336658634, "There is no publications in this database"},		/* NO_PUBLICATIONS */
	{336658635, "Publications:"},		/* MSG_PUBLICATIONS */
	{336658636, "Procedures:"},		/* MSG_PROCEDURES */
	{336658641, "    SET WIRE_stats         -- toggle display of wire (network) statistics"},		/* HLP_SETWIRESTATS */
	{336723969, "GSEC>"},		/* GsecMsg1 */
	{336723970, "gsec"},		/* GsecMsg2 */
	{336723971, "ADD            add user"},		/* GsecMsg3 */
	{336723972, "DELETE         delete user"},		/* GsecMsg4 */
	{336723973, "DISPLAY        display user(s)"},		/* GsecMsg5 */
	{336723974, "MODIFY         modify user"},		/* GsecMsg6 */
	{336723975, "PW             user's password"},		/* GsecMsg7 */
	{336723976, "UID            user's ID"},		/* GsecMsg8 */
	{336723977, "GID            user's group ID"},		/* GsecMsg9 */
	{336723978, "PROJ           user's project name"},		/* GsecMsg10 */
	{336723979, "ORG            user's organization name"},		/* GsecMsg11 */
	{336723980, "FNAME          user's first name"},		/* GsecMsg12 */
	{336723981, "MNAME          user's middle name/initial"},		/* GsecMsg13 */
	{336723982, "LNAME          user's last name"},		/* GsecMsg14 */
	{336723983, "unable to open database"},		/* gsec_cant_open_db */
	{336723984, "error in switch specifications"},		/* gsec_switches_error */
	{336723985, "no operation specified"},		/* gsec_no_op_spec */
	{336723986, "no user name specified"},		/* gsec_no_usr_name */
	{336723987, "add record error"},		/* gsec_err_add */
	{336723988, "modify record error"},		/* gsec_err_modify */
	{336723989, "find/modify record error"},		/* gsec_err_find_mod */
	{336723990, "record not found for user: {0}"},		/* gsec_err_rec_not_found */
	{336723991, "delete record error"},		/* gsec_err_delete */
	{336723992, "find/delete record error"},		/* gsec_err_find_del */
	{336723993, "users defined for node"},		/* GsecMsg25 */
	{336723994, "     user name                    uid   gid admin     full name"},		/* GsecMsg26 */
	{336723995, "------------------------------------------------------------------------------------------------"},		/* GsecMsg27 */
	{336723996, "find/display record error"},		/* gsec_err_find_disp */
	{336723997, "invalid parameter, no switch defined"},		/* gsec_inv_param */
	{336723998, "operation already specified"},		/* gsec_op_specified */
	{336723999, "password already specified"},		/* gsec_pw_specified */
	{336724000, "uid already specified"},		/* gsec_uid_specified */
	{336724001, "gid already specified"},		/* gsec_gid_specified */
	{336724002, "project already specified"},		/* gsec_proj_specified */
	{336724003, "organization already specified"},		/* gsec_org_specified */
	{336724004, "first name already specified"},		/* gsec_fname_specified */
	{336724005, "middle name already specified"},		/* gsec_mname_specified */
	{336724006, "last name already specified"},		/* gsec_lname_specified */
	{336724007, "gsec version"},		/* GsecMsg39 */
	{336724008, "invalid switch specified"},		/* gsec_inv_switch */
	{336724009, "ambiguous switch specified"},		/* gsec_amb_switch */
	{336724010, "no operation specified for parameters"},		/* gsec_no_op_specified */
	{336724011, "no parameters allowed for this operation"},		/* gsec_params_not_allowed */
	{336724012, "incompatible switches specified"},		/* gsec_incompat_switch */
	{336724013, "gsec utility - maintains user password database"},		/* GsecMsg45 */
	{336724014, "command line usage:"},		/* GsecMsg46 */
	{336724015, "<command> [ <parameter> ... ]"},		/* GsecMsg47 */
	{336724016, "interactive usage:"},		/* GsecMsg48 */
	{336724017, "available commands:"},		/* GsecMsg49 */
	{336724018, "adding a new user:"},		/* GsecMsgs50 */
	{336724019, "add <name> [ <parameter> ... ]"},		/* GsecMsg51 */
	{336724020, "deleting a current user:"},		/* GsecMsg52 */
	{336724021, "delete <name>"},		/* GsecMsg53 */
	{336724022, "displaying all users:"},		/* GsecMsg54 */
	{336724023, "display"},		/* GsecMsg55 */
	{336724024, "displaying one user:"},		/* GsecMsg56 */
	{336724025, "display <name>"},		/* GsecMsg57 */
	{336724026, "modifying a user's parameters:"},		/* GsecMsg58 */
	{336724027, "modify <name> <parameter> [ <parameter> ... ]"},		/* GsecMsg59 */
	{336724028, "help:"},		/* GsecMsg60 */
	{336724029, "? (interactive only)"},		/* GsecMsg61 */
	{336724030, "help"},		/* GsecMsg62 */
	{336724031, "quit interactive session:"},		/* GsecMsg63 */
	{336724032, "quit (interactive only)"},		/* GsecMsg64 */
	{336724033, "available parameters:"},		/* GsecMsg65 */
	{336724034, "-pw <password>"},		/* GsecMsg66 */
	{336724035, "-uid <uid>"},		/* GsecMsg67 */
	{336724036, "-gid <uid>"},		/* GsecMsg68 */
	{336724037, "-proj <projectname>"},		/* GsecMsg69 */
	{336724038, "-org <organizationname>"},		/* GsecMsg70 */
	{336724039, "-fname <firstname>"},		/* GsecMsg71 */
	{336724040, "-mname <middlename>"},		/* GsecMsg72 */
	{336724041, "-lname <lastname>"},		/* GsecMsg73 */
	{336724042, "gsec - memory allocation error"},
	{336724043, "gsec error"},
	{336724044, "Invalid user name (maximum 31 bytes allowed)"},		/* gsec_inv_username */
	{336724045, "Warning - maximum 8 significant bytes of password used"},		/* gsec_inv_pw_length */
	{336724046, "database already specified"},		/* gsec_db_specified */
	{336724047, "database administrator name already specified"},		/* gsec_db_admin_specified */
	{336724048, "database administrator password already specified"},		/* gsec_db_admin_pw_specified */
	{336724049, "SQL role name already specified"},		/* gsec_sql_role_specified */
	{336724050, "[ <options> ... ]"},		/* GsecMsg82 */
	{336724051, "available options:"},		/* GsecMsg83 */
	{336724052, "-user <database administrator name>"},		/* GsecMsg84 */
	{336724053, "-password <database administrator password>"},		/* GsecMsg85 */
	{336724054, "-role <database administrator SQL role name>"},		/* GsecMsg86 */
	{336724055, "-database <database to manage>"},		/* GsecMsg87 */
	{336724056, "-z"},		/* GsecMsg88 */
	{336724057, "displaying version number:"},		/* GsecMsg89 */
	{336724058, "z (interactive only)"},		/* GsecMsg90 */
	{336724059, "-trusted (use trusted authentication)"},		/* GsecMsg91 */
	{336724060, "invalid switch specified in interactive mode"},		/* GsecMsg92 */
	{336724061, "error closing security database"},		/* GsecMsg93 */
	{336724062, "error releasing request in security database"},		/* GsecMsg94 */
	{336724063, "-fetch_password <file to fetch password from>"},		/* GsecMsg95 */
	{336724064, "error fetching password from file"},		/* GsecMsg96 */
	{336724065, "error changing AUTO ADMINS MAPPING in security database"},		/* GsecMsg97 */
	{336724066, "changing admins mapping to RDB$ADMIN role in security database:"},		/* GsecMsg98 */
	{336724067, "invalid parameter for -MAPPING, only SET or DROP is accepted"},		/* GsecMsg99 */
	{336724068, "mapping {set|drop}"},		/* GsecMsg100 */
	{336724069, "use gsec -? to get help"},		/* GsecMsg101 */
	{336724070, "-admin {yes|no}"},		/* GsecMsg102 */
	{336724071, "invalid parameter for -ADMIN, only YES or NO is accepted"},		/* GsecMsg103 */
	{336724072, "not enough privileges to complete operation"},		/* GsecMsg104 */
	{336920577, "found unknown switch"},		/* gstat_unknown_switch */
	{336920578, "please retry, giving a database name"},		/* gstat_retry */
	{336920579, "Wrong ODS version, expected {0}, encountered {1}"},		/* gstat_wrong_ods */
	{336920580, "Unexpected end of database file."},		/* gstat_unexpected_eof */
	{336920581, "gstat version {0}"},
	{336920582, "\nDatabase \"{0}\""},
	{336920583, "\n\nDatabase file sequence:"},
	{336920584, "File {0} continues as file {1}"},
	{336920585, "File {0} is the {1} file"},
	{336920586, "\nAnalyzing database pages ..."},
	{336920587, "    Primary pointer page: {0}, Index root page: {1}"},
	{336920588, "    Data pages: {0}, data page slots: {1}, average fill: {2}"},
	{336920589, "    Fill distribution:"},
	{336920590, "    Index {0} ({1})"},
	{336920591, "\tDepth: {0}, leaf buckets: {1}, nodes: {2}"},
	{336920592, "\tAverage data length: {0}, total dup: {1}, max dup: {2}"},
	{336920593, "\tFill distribution:"},
	{336920594, "    Expected data on page {0}"},
	{336920595, "    Expected b-tree bucket on page {0} from {1}"},
	{336920596, "unknown switch \"{0}\""},
	{336920597, "Available switches:"},
	{336920598, "    -a      analyze data and index pages"},
	{336920599, "    -d      analyze data pages"},
	{336920600, "    -h      analyze header page ONLY"},
	{336920601, "    -i      analyze index leaf pages"},
	{336920602, "    -l      analyze log page"},
	{336920603, "    -s      analyze system relations in addition to user tables"},
	{336920604, "    -z      display version number"},
	{336920605, "Can't open database file {0}"},		/* gstat_open_err */
	{336920606, "Can't read a database page"},		/* gstat_read_err */
	{336920607, "System memory exhausted"},		/* gstat_sysmemex */
	{336920608, "    -u      username"},		/* gstat_username */
	{336920609, "    -p      password"},		/* gstat_password */
	{336920610, "    -r      analyze average record and version length"},
	{336920611, "    -t      tablename <tablename2...> (case sensitive)"},
	{336920612, "    -tr     use trusted authentication"},
	{336920613, "    -fetch  fetch password from file"},
	{336920614, "option -h is incompatible with options -a, -d, -i, -r, -s and -t"},
	{336920615, "usage:   gstat [options] <database> or gstat <database> [options]"},
	{336920616, "database name was already specified"},
	{336920617, "option -t needs a table name"},
	{336920618, "option -t got a too long table name {0}"},
	{336920619, "option -t accepts several table names only if used after <database>"},
	{336920620, "table \"{0}\" not found"},
	{336920621, "use gstat -? to get help"},
	{336920622, "    Primary pages: {0}, secondary pages: {1}, swept pages: {2}"},
	{336920623, "    Big record pages: {0}"},
	{336920624, "    Blobs: {0}, total length: {1}, blob pages: {2}"},
	{336920625, "        Level 0: {0}, Level 1: {1}, Level 2: {2}"},
	{336920626, "option -e is incompatible with options -a, -d, -h, -i, -r, -s and -t"},
	{336920627, "    -e      analyze database encryption"},
	{336920628, "Data pages: total {0}, encrypted {1}, non-crypted {2}"},
	{336920629, "Index pages: total {0}, encrypted {1}, non-crypted {2}"},
	{336920630, "Blob pages: total {0}, encrypted {1}, non-crypted {2}"},
	{336920631, "no encrypted database support, only -e and -h can be used"},
	{336920632, "    Empty pages: {0}, full pages: {1}"},
	{336920633, "    -role   SQL role name"},
	{336920634, "Other pages: total {0}, ENCRYPTED {1} (DB problem!), non-crypted {2}"},
	{336920635, "Gstat execution time {0}"},
	{336920636, "Gstat completion time {0}"},
	{336920637, "    Expected page inventory page {0}"},
	{336920638, "Generator pages: total {0}, encrypted {1}, non-crypted {2}"},
	{336920639, "    Table size: {0} bytes"},
	{336920640, "        Level {0}: {1}, total length: {2}, blob pages: {3}"},
	{336986113, "Wrong value for access mode"},		/* fbsvcmgr_bad_am */
	{336986114, "Wrong value for write mode"},		/* fbsvcmgr_bad_wm */
	{336986115, "Wrong value for reserve space"},		/* fbsvcmgr_bad_rs */
	{336986116, "Unknown tag ({0}) in info_svr_db_info block after isc_svc_query()"},		/* fbsvcmgr_info_err */
	{336986117, "Unknown tag ({0}) in isc_svc_query() results"},		/* fbsvcmgr_query_err */
	{336986118, "Unknown switch \"{0}\""},		/* fbsvcmgr_switch_unknown */
	{336986119, "Service Manager Version"},
	{336986120, "Server version"},
	{336986121, "Server implementation"},
	{336986122, "Path to firebird.msg"},
	{336986123, "Server root"},
	{336986124, "Path to lock files"},
	{336986125, "Security database"},
	{336986126, "Databases"},
	{336986127, "   Database in use"},
	{336986128, "   Number of attachments"},
	{336986129, "   Number of databases"},
	{336986130, "Information truncated"},
	{336986131, "Usage: fbsvcmgr manager-name switches..."},
	{336986132, "Manager-name should be service_mgr, may be prefixed with host name"},
	{336986133, "according to common rules (host:service_mgr, \\\\host\\service_mgr)."},
	{336986134, "Switches exactly match SPB tags, used in abbreviated form."},
	{336986135, "Remove isc_, spb_ and svc_ parts of tag and you will get the switch."},
	{336986136, "For example: isc_action_svc_backup is specified as action_backup,"},
	{336986137, "             isc_spb_dbname => dbname,"},
	{336986138, "             isc_info_svc_implementation => info_implementation,"},
	{336986139, "             isc_spb_prp_db_online => prp_db_online and so on."},
	{336986140, "You may specify single action or multiple info items when calling fbsvcmgr once."},
	{336986141, "Full command line samples:"},
	{336986142, "fbsvcmgr service_mgr user sysdba password masterkey action_db_stats dbname employee sts_hdr_pages"},
	{336986143, "  (will list header info in database employee on local machine)"},
	{336986144, "fbsvcmgr yourserver:service_mgr user sysdba password masterkey info_server_version info_svr_db_info"},
	{336986145, "  (will show firebird version and databases usage on yourserver)"},
	{336986146, "Transaction in limbo"},
	{336986147, "Multidatabase transaction in limbo"},
	{336986148, "Host Site"},
	{336986149, "Transaction"},
	{336986150, "has been prepared"},
	{336986151, "has been committed"},
	{336986152, "has been rolled back"},
	{336986153, "is not available"},
	{336986154, "Remote Site"},
	{336986155, "Database Path"},
	{336986156, "Automated recovery would commit this transaction"},
	{336986157, "Automated recovery would rollback this transaction"},
	{336986158, "No idea should it be commited or rolled back"},
	{336986159, "Wrong value for shutdown mode"},		/* fbsvcmgr_bad_sm */
	{336986160, "could not open file {0}"},		/* fbsvcmgr_fp_open */
	{336986161, "could not read file {0}"},		/* fbsvcmgr_fp_read */
	{336986162, "empty file {0}"},		/* fbsvcmgr_fp_empty */
	{336986163, "Firebird Services Manager version {0}"},
	{336986164, "Invalid or missing parameter for switch {0}"},		/* fbsvcmgr_bad_arg */
	{336986165, "To get full list of known services run with -? switch"},
	{336986166, "Attaching to services manager:"},
	{336986167, "Information requests:"},
	{336986168, "Actions:"},
	{336986169, "Server capabilities:"},
	{336986170, "Unknown tag ({0}) in isc_info_svc_limbo_trans block after isc_svc_query()"},		/* fbsvcmgr_info_limbo */
	{336986171, "Unknown tag ({0}) in isc_spb_tra_state block after isc_svc_query()"},		/* fbsvcmgr_limbo_state */
	{336986172, "Unknown tag ({0}) in isc_spb_tra_advise block after isc_svc_query()"},		/* fbsvcmgr_limbo_advise */
	{336986173, "Wrong value for replica mode"},		/* fbsvcmgr_bad_rm */
	{337051649, "Switches trusted_user and trusted_role are not supported from command line"},		/* utl_trusted_switch */
	{337117185, "ERROR: "},
	{337117186, "Physical Backup Manager    Copyright (C) 2004 Firebird development team"},
	{337117187, "  Original idea is of Sean Leyne <sean@broadviewsoftware.com>"},
	{337117188, "  Designed and implemented by Nickolay Samofatov <skidder@bssys.com>"},
	{337117189, "  This work was funded through a grant from BroadView Software, Inc.\\n"},
	{337117190, "Usage: nbackup <options>"},
	{337117191, "exclusive options are:"},
	{337117192, "  -L(OCK) <database>                     Lock database for filesystem copy"},
	{337117193, "  -UN(LOCK) <database>                   Unlock previously locked database"},
	{337117194, "  -F(IXUP) <database>                    Fixup database after filesystem copy"},
	{337117195, "  -B(ACKUP) <level>|<GUID> <db> [<file>] Create incremental backup"},
	{337117196, "  -R(ESTORE) <db> [<file0> [<file1>...]] Restore incremental backup"},
	{337117197, "  -U(SER) <user>                         User name"},
	{337117198, "  -P(ASSWORD) <password>                 Password"},
	{337117199, "  -FETCH_PASSWORD <file>                 Fetch password from file"},
	{337117200, "  -NOD(BTRIGGERS)                        Do not run database triggers"},
	{337117201, "  -S(IZE)                                Print database size in pages after lock"},
	{337117202, "  -Z                                     Print program version"},
	{337117203, "Notes:"},
	{337117204, "  <database> may specify database alias."},
	{337117205, "  Incremental backups of multi-file databases are not supported yet."},
	{337117206, "  \"stdout\" may be used as a value of <filename> for -B option."},
	{337117207, "PROBLEM ON \"{0}\"."},
	{337117208, "general options are:"},
	{337117209, "switches can be abbreviated to the unparenthesized characters"},
	{337117210, "  Option -S(IZE) only is valid together with -L(OCK)."},
	{337117211, "  For historical reasons, -N is equivalent to -UN(LOCK)"},
	{337117212, "  and -T is equivalent to -NOD(BTRIGGERS)."},
	{337117213, "Missing parameter for switch {0}"},		/* nbackup_missing_param */
	{337117214, "Only one of -LOCK, -UNLOCK, -FIXUP, -BACKUP or -RESTORE should be specified"},		/* nbackup_allowed_switches */
	{337117215, "Unrecognized parameter {0}"},		/* nbackup_unknown_param */
	{337117216, "Unknown switch {0}"},		/* nbackup_unknown_switch */
	{337117217, "Fetch password can't be used in service mode"},		/* nbackup_nofetchpw_svc */
	{337117218, "Error working with password file \"{0}\""},		/* nbackup_pwfile_error */
	{337117219, "Switch -SIZE can be used only with -LOCK"},		/* nbackup_size_with_lock */
	{337117220, "None of -LOCK, -UNLOCK, -FIXUP, -BACKUP or -RESTORE specified"},		/* nbackup_no_switch */
	{337117221, "Failure: "},
	{337117222, "Enter name of the backup file of level {0} (\".\" - do not restore further):"},
	{337117223, "IO error reading file: {0}"},		/* nbackup_err_read */
	{337117224, "IO error writing file: {0}"},		/* nbackup_err_write */
	{337117225, "IO error seeking file: {0}"},		/* nbackup_err_seek */
	{337117226, "Error opening database file: {0}"},		/* nbackup_err_opendb */
	{337117227, "Error in posix_fadvise({0}) for database {1}"},		/* nbackup_err_fadvice */
	{337117228, "Error creating database file: {0}"},		/* nbackup_err_createdb */
	{337117229, "Error opening backup file: {0}"},		/* nbackup_err_openbk */
	{337117230, "Error creating backup file: {0}"},		/* nbackup_err_createbk */
	{337117231, "Unexpected end of database file {0}"},		/* nbackup_err_eofdb */
	{337117232, "Database {0} is not in state ({1}) to be safely fixed up"},		/* nbackup_fixup_wrongstate */
	{337117233, "Database error"},		/* nbackup_err_db */
	{337117234, "Username or password is too long"},		/* nbackup_userpw_toolong */
	{337117235, "Cannot find record for database \"{0}\" backup level {1} in the backup history"},		/* nbackup_lostrec_db */
	{337117236, "Internal error. History query returned null SCN or GUID"},		/* nbackup_lostguid_db */
	{337117237, "Unexpected end of file when reading header of database file \"{0}\" (stage {1})"},		/* nbackup_err_eofhdrdb */
	{337117238, "Internal error. Database file is not locked. Flags are {0}"},		/* nbackup_db_notlock */
	{337117239, "Internal error. Cannot get backup guid clumplet"},		/* nbackup_lostguid_bk */
	{337117240, "Internal error. Database page {0} had been changed during backup (page SCN={1}, backup SCN={2})"},		/* nbackup_page_changed */
	{337117241, "Database file size is not a multiple of page size"},		/* nbackup_dbsize_inconsistent */
	{337117242, "Level 0 backup is not restored"},		/* nbackup_failed_lzbk */
	{337117243, "Unexpected end of file when reading header of backup file: {0}"},		/* nbackup_err_eofhdrbk */
	{337117244, "Invalid incremental backup file: {0}"},		/* nbackup_invalid_incbk */
	{337117245, "Unsupported version {0} of incremental backup file: {1}"},		/* nbackup_unsupvers_incbk */
	{337117246, "Invalid level {0} of incremental backup file: {1}, expected {2}"},		/* nbackup_invlevel_incbk */
	{337117247, "Wrong order of backup files or invalid incremental backup file detected, file: {0}"},		/* nbackup_wrong_orderbk */
	{337117248, "Unexpected end of backup file: {0}"},		/* nbackup_err_eofbk */
	{337117249, "Error creating database file: {0} via copying from: {1}"},		/* nbackup_err_copy */
	{337117250, "Unexpected end of file when reading header of restored database file (stage {0})"},		/* nbackup_err_eofhdr_restdb */
	{337117251, "Cannot get backup guid clumplet from L0 backup"},		/* nbackup_lostguid_l0bk */
	{337117252, "Physical Backup Manager version {0}"},
	{337117253, "Enter name of the backup file of level {0} (\".\" - do not restore further):"},
	{337117254, "  -D(IRECT) <ON | OFF>                   Use or not direct I/O when backing up database"},
	{337117255, "Wrong parameter {0} for switch -D, need ON or OFF"},		/* nbackup_switchd_parameter */
	{337117256, "special options are:"},
	{337117257, "Terminated due to user request"},		/* nbackup_user_stop */
	{337117258, "  -DE(COMPRESS) <command>                Command to extract archives during restore"},
	{337117259, "Too complex decompress command (> {0} arguments)"},		/* nbackup_deco_parse */
	{337117260, "  -RO(LE) <role>                         SQL role name"},
	{337117261, "Cannot find record for database \"{0}\" backup GUID {1} in the backup history"},		/* nbackup_lostrec_guid_db */
	{337117262, "  -I(NPLACE)                             Restore incremental backup(s) to existing database"},
	{337117263, "  -INPLACE option could corrupt the database that has changed since previous restore."},
	{337117264, "  -SEQ(UENCE)                            Preserve original replication sequence"},
	{337117265, "Switch -SEQ(UENCE) can be used only with -FIXUP or -RESTORE"},		/* nbackup_seq_misuse */
	{337117266, "  -CLEAN_HIST(ORY)                       Clean old records from backup history"},
	{337117267, "  -K(EEP) <N> <(R)OWS | (D)AYS>          How many recent rows (or days back from today) should be kept in the history"},
	{337117268, "Wrong parameter value for switch {0}"},		/* nbackup_wrong_param */
	{337117269, "Switch -CLEAN_HISTORY can be used only with -BACKUP"},		/* nbackup_clean_hist_misuse */
	{337117270, "-KEEP can be used only with -CLEAN_HISTORY"},		/* nbackup_clean_hist_missed */
	{337117271, "-KEEP is required with -CLEAN_HISTORY"},		/* nbackup_keep_hist_missed */
	{337117272, "-KEEP can be used one time only"},		/* nbackup_second_keep_switch */
	{337182721, "Firebird Trace Manager version {0}"},
	{337182722, "ERROR: "},
	{337182723, "Firebird Trace Manager."},
	{337182724, "Usage: fbtracemgr <action> [<parameters>]"},
	{337182725, "Actions:"},
	{337182726, "  -STA[RT]                              Start trace session"},
	{337182727, "  -STO[P]                               Stop trace session"},
	{337182728, "  -SU[SPEND]                            Suspend trace session"},
	{337182729, "  -R[ESUME]                             Resume trace session"},
	{337182730, "  -L[IST]                               List existing trace sessions"},
	{337182731, "  -Z                                    Show program version"},
	{337182732, "Action parameters:"},
	{337182733, "  -N[AME]    <string>                   Session name"},
	{337182734, "  -I[D]      <number>                   Session ID"},
	{337182735, "  -C[ONFIG]  <string>                   Trace configuration file name"},
	{337182736, "Connection parameters:"},
	{337182737, "  -SE[RVICE]  <string>                  Service name"},
	{337182738, "  -U[SER]     <string>                  User name"},
	{337182739, "  -P[ASSWORD] <string>                  Password"},
	{337182740, "  -FE[TCH]    <string>                  Fetch password from file"},
	{337182741, "  -T[RUSTED]  <string>                  Force trusted authentication"},
	{337182742, "Examples:"},
	{337182743, "  fbtracemgr -SE remote_host:service_mgr -USER SYSDBA -PASS masterkey -LIST"},
	{337182744, "  fbtracemgr -SE service_mgr -START -NAME my_trace -CONFIG my_cfg.txt"},
	{337182745, "  fbtracemgr -SE service_mgr -SUSPEND -ID 2"},
	{337182746, "  fbtracemgr -SE service_mgr -RESUME -ID 2"},
	{337182747, "  fbtracemgr -SE service_mgr -STOP -ID 4"},
	{337182748, "Notes:"},
	{337182749, "  Press CTRL+C to stop interactive trace session"},
	{337182750, "conflicting actions \"{0}\" and \"{1}\" found"},		/* trace_conflict_acts */
	{337182751, "action switch not found"},		/* trace_act_notfound */
	{337182752, "switch \"{0}\" must be set only once"},		/* trace_switch_once */
	{337182753, "value for switch \"{0}\" is missing"},		/* trace_param_val_miss */
	{337182754, "invalid value (\"{0}\") for switch \"{1}\""},		/* trace_param_invalid */
	{337182755, "unknown switch \"{0}\" encountered"},		/* trace_switch_unknown */
	{337182756, "switch \"{0}\" can be used by service only"},		/* trace_switch_svc_only */
	{337182757, "switch \"{0}\" can be used by interactive user only"},		/* trace_switch_user_only */
	{337182758, "mandatory parameter \"{0}\" for switch \"{1}\" is missing"},		/* trace_switch_param_miss */
	{337182759, "parameter \"{0}\" is incompatible with action \"{1}\""},		/* trace_param_act_notcompat */
	{337182760, "mandatory switch \"{0}\" is missing"},		/* trace_mandatory_switch_miss */
		};

	public static bool TryGet(int key, out string value) => _messages.TryGetValue(key, out value);
}
