namespace FirebirdSql.Data.FirebirdClient;

/// <summary>
/// Error codes occuring in Firebird database management system.
/// 
/// Messages are taken from documentation at: https://ib-aid.com/download/docs/firebird-language-reference-2.5/fblangref25-appx02-sqlcodes.html
/// </summary>
public static partial class FbErrorCodes
{
	/// <summary>
	/// Segment buffer length shorter than expected.
	/// </summary>
	public const int SegmentBuffer = 335544366;
	/// <summary>
	/// No match for first value expression.
	/// </summary>
	public const int FromNoMatch = 335544338;
	/// <summary>
	/// Invalid database key.
	/// </summary>
	public const int NoRecord = 335544354;
	/// <summary>
	/// Attempted retrieval of more segments than exist.
	/// </summary>
	public const int SegstrEof = 335544367;
	/// <summary>
	/// Attempt to fetch past the last record in a record stream.
	/// </summary>
	public const int StreamEof = 335544374;
	/// <summary>
	/// Bad debug info format.
	/// </summary>
	public const int BadDebugFormat = 335544875;
	/// <summary>
	/// Table/procedure has non-SQL security class defined.
	/// </summary>
	public const int NonSqlSecurityRel = 335544554;
	/// <summary>
	/// Column has non-SQL security class defined.
	/// </summary>
	public const int NonSqlSecurityFld = 335544555;
	/// <summary>
	/// Procedure @1 does not return any values
	/// </summary>
	public const int DsqlProcedureUseError = 335544668;
	/// <summary>
	/// The username entered is too long. Maximum length is 31 bytes.
	/// </summary>
	public const int UsernameTooLong = 335544747;
	/// <summary>
	/// The password specified is too long. Maximum length is 8 bytes.
	/// </summary>
	public const int PasswordTooLong = 335544748;
	/// <summary>
	/// A username is required for this operation.
	/// </summary>
	public const int UsernameRequired = 335544749;
	/// <summary>
	/// A password is required for this operation.
	/// </summary>
	public const int PasswordRequired = 335544750;
	/// <summary>
	/// The network protocol specified is invalid.
	/// </summary>
	public const int BadProtocol = 335544751;
	/// <summary>
	/// A duplicate user name was found in the security database.
	/// </summary>
	public const int DupUsernameFound = 335544752;
	/// <summary>
	/// The user name specified was not found in the security database.
	/// </summary>
	public const int UsernameNotFound = 335544753;
	/// <summary>
	/// An error occurred while attempting to add the user.
	/// </summary>
	public const int ErrorAddingUserRecord = 335544754;
	/// <summary>
	/// An error occurred while attempting to modify the user record.
	/// </summary>
	public const int ErrorModifyingUserRecord = 335544755;
	/// <summary>
	/// An error occurred while attempting to delete the user record.
	/// </summary>
	public const int ErrorDeletingUserRecord = 335544756;
	/// <summary>
	///  An error occurred while updating the security database.
	/// </summary>
	public const int ErrorUpdatingSecurityDb = 335544757;
	/// <summary>
	/// Data type for constant unknown.
	/// </summary>
	public const int DsqlConstantError = 335544571;
	//336003075 dsql_transitional_numeric skipped (might be not needed in Firebird 3)
	//336003077 sql_db_dialect_dtype_unsupport skipped (might be not needed in Firebird 3)
	/// <summary>
	/// Invalid label @1 @2 in the current scope.
	/// </summary>
	public const int DsqlInvalidLabel = 336003087;
	/// <summary>
	/// Datatypes @1 are not comparable in expression @2.
	/// </summary>
	public const int DsqlDatatypesNotComparable = 336003088;
	/// <summary>
	/// Invalid request BLR at offset @1.
	/// </summary>
	public const int InvalidRequestBlr = 335544343;
	/// <summary>
	/// BLR syntax error: expected @1 at offset @2, encountered @3.
	/// </summary>
	public const int SyntaxError = 335544390;
	/// <summary>
	/// Context already in use (BLR error).
	/// </summary>
	public const int ContextInUse = 335544425;
	/// <summary>
	/// Context not defined (BLR error).
	/// </summary>
	public const int ContextNotDefined = 335544426;
	/// <summary>
	/// Bad parameter number.
	/// </summary>
	public const int BadParameterNum = 335544429;
	//335544440 bad_msg_vec skipped (no message applied)
	/// <summary>
	/// Invalid slice description language at offset @1.
	/// </summary>
	public const int InvalidSdl = 335544456;
	/// <summary>
	/// DSQL: Invalid command.
	/// </summary>
	public const int DsqlCommandError = 335544570;
	/// <summary>
	/// DSQL: Internal error.
	/// </summary>
	public const int DsqlInternalError = 335544579;
	/// <summary>
	/// DSQL: Option specified more than once.
	/// </summary>
	public const int DsqlDuplicateOption = 335544590;
	/// <summary>
	/// Unknown transaction option.
	/// </summary>
	public const int DsqlUnknownTransaction = 335544591;
	/// <summary>
	/// DSQL: Invalid array reference.
	/// </summary>
	public const int DsqlInvalidArray = 335544592;
	/// <summary>
	/// Unexpected end of command.
	/// </summary>
	public const int CommandEnd = 335544608;
	/// <summary>
	/// Unknown token.
	/// </summary>
	public const int UnknownToken = 335544612;
	/// <summary>
	/// DSQL: Token unknown - line @1, column @2.
	/// </summary>
	public const int DsqlUnknownToken = 335544634;
	/// <summary>
	/// DSQL: Invalid aggregate reference.
	/// </summary>
	public const int DsqlInvalidAggregate = 335544709;
	/// <summary>
	/// Invalid blob id.
	/// </summary>
	public const int InvalidBlobId = 335544714;
	/// <summary>
	/// Client/Server Express not supported in this release.
	/// </summary>
	public const int CseNotSupported = 335544730;
	/// <summary>
	/// Token size exceeds limit.
	/// </summary>
	public const int TokenTooLong = 335544743;
	/// <summary>
	/// A string constant is delimited by double quotes.
	/// </summary>
	public const int InvalidStringConstant = 335544763;
	/// <summary>
	/// DATE must be changed to TIMESTAMP.
	/// Its original symbol is transitional_date.
	/// </summary>
	public const int DateTypeMismatch = 335544764;
	/// <summary>
	/// Client SQL dialect @1 does not support reference to @2 datatype.
	/// </summary>
	public const int SqlDialectDatatypeNotSupported = 335544796;
	/// <summary>
	/// You created an indirect dependency on uncommitted metadata.
	/// You must roll back the current transaction.
	/// </summary>
	public const int DependOnUncommittedRel = 335544798;
	/// <summary>
	/// DSQL: Invalid column position used in the @1 clause.
	/// </summary>
	public const int DsqlInvalidColumnPosition = 335544821;
	/// <summary>
	/// DSQL: Cannot use an aggregate function in a WHERE clause, use HAVING instead.
	/// </summary>
	public const int DsqlWhereOnAggregate = 335544822;
	/// <summary>
	/// DSQL: Cannot use an aggregate function in a GROUP BY clause.
	/// </summary>
	public const int DsqlGroupOnAggregate = 335544823;
	/// <summary>
	/// DSQL: Invalid expression in the @1 (not contained in either an aggregate function or
	/// the GROUP BY clause).
	/// </summary>
	public const int DsqlAggregateColumnError = 335544824;
	/// <summary>
	/// DSQL: Invalid expression in the @1 (neither an aggregate function nor a part of the
	/// GROUP BY clause).
	/// </summary>
	public const int DsqlAggregateHavingError = 335544825;
	/// <summary>
	/// DSQL: Nested aggregate functions are not allowed.
	/// </summary>
	public const int DsqlNestedAggregateError = 335544826;
	/// <summary>
	/// Malformed string.
	/// </summary>
	public const int MalformedString = 335544849;
	/// <summary>
	/// Unexpected end of command - line @1, column @2.
	/// </summary>
	public const int RangedCommandEnd = 335544851;
	/// <summary>
	/// DSQL: Cannot sort on more than 255 items.
	/// </summary>
	public const int DsqlMaxSortItems = 336397215;
	/// <summary>
	/// DSQL: Cannot group on more than 255 items.
	/// </summary>
	public const int DsqlMaxGroupItems = 336397216;
	/// <summary>
	/// DSQL: Cannot include the same field (@1.@2) twice in the ORDER BY clause with
	/// conflicting sorting options.
	/// </summary>
	public const int DsqlConflictingSortField = 336397217;
	/// <summary>
	/// DSQL: Column list from derived table @1 has more columns than the number of items in
	/// its SELECT statement.
	/// </summary>
	public const int DsqlDerivedTableMoreColumns = 336397218;
	/// <summary>
	/// DSQL: Column list from derived table @1 has less columns than the number of items in
	/// its SELECT statement.
	/// </summary>
	public const int DsqlDerivedTableLessColumns = 336397219;
	/// <summary>
	/// DSQL: No column name specified for column number @1 in derived table @2.
	/// </summary>
	public const int DsqlDerivedFieldUnnamed = 336397220;
	/// <summary>
	/// DSQL: Column @1 was specified multiple times for derived table @2.
	/// </summary>
	public const int DsqlDerivedFieldDupName = 336397221;
	/// <summary>
	/// Internal dsql error: alias type expected by pass1_expand_select_node.
	/// </summary>
	public const int DsqlDerivedAliasSelect = 336397222;
	/// <summary>
	/// Internal dsql error: alias type expected by pass1_field
	/// </summary>
	public const int DsqlDerivedAliasField = 336397223;
	/// <summary>
	/// Internal dsql error: column position out of range in pass1_union_auto_cast.
	/// </summary>
	public const int DsqlAutoFieldBadPos = 336397224;
	/// <summary>
	/// DSQL: Recursive CTE member (@1) can refer itself only in FROM clause.
	/// </summary>
	public const int DsqlCteWrongReference = 336397225;
	/// <summary>
	/// DSQL: CTE '@1' has cyclic dependencies.
	/// </summary>
	public const int DsqlCteCycle = 336397226;
	/// <summary>
	/// DSQL: Recursive member of CTE can't be member of an outer join.
	/// </summary>
	public const int DsqlCteOuterJoin = 336397227;
	/// <summary>
	/// DSQL: Recursive member of CTE can't reference itself more than once.
	/// </summary>
	public const int DsqlCteMultipleReferences = 336397228;
	/// <summary>
	/// DSQL: Recursive CTE (@1) must be an UNION.
	/// </summary>
	public const int DsqlCteNotUnion = 336397229;
	/// <summary>
	/// DSQL: CTE '@1' defined non-recursive member after recursive.
	/// </summary>
	public const int DsqlCteNonrecursAfterRecurs = 336397230;
	/// <summary>
	/// DSQL: Recursive member of CTE '@1' has @2 clause.
	/// </summary>
	public const int DsqlCteWrongClause = 336397231;
	/// <summary>
	/// DSQL: Recursive members of CTE (@1) must be linked with another members via UNION ALL.
	/// </summary>
	public const int DsqlCteUnionAll = 336397232;
	/// <summary>
	/// DSQL: Non-recursive member is missing in CTE '@1'.
	/// </summary>
	public const int DsqlCteMissNonrecursive = 336397233;
	/// <summary>
	/// DSQL: WITH clause can't be nested.
	/// </summary>
	public const int DsqlCteNestedWith = 336397234;
	/// <summary>
	/// DSQL: Column @1 appears more than once in USING clause.
	/// </summary>
	public const int DsqlColumnMoreThanOnceUsing = 336397235;
	/// <summary>
	/// DSQL: CTE "@1" is not used in query.
	/// </summary>
	public const int DsqlCteNotUsed = 336397237;
	/// <summary>
	/// Invalid ESCAPE sequence.
	/// </summary>
	public const int LikeEscapeInvalid = 335544702;
	/// <summary>
	/// Specified EXTRACT part does not exist in input datatype.
	/// </summary>
	public const int ExtractInputMismatch = 335544789;
	/// <summary>
	/// Attempted update of read-only table.
	/// </summary>
	public const int ReadOnlyUpdate = 335544360;
	/// <summary>
	/// Cannot update read-only view @1.
	/// </summary>
	public const int ReadOnlyView = 335544362;
	/// <summary>
	/// Not updatable.
	/// </summary>
	public const int NonUpdatable = 335544446;
	/// <summary>
	/// Cannot define constraints on views.
	/// </summary>
	public const int ConstraintOnView = 335544546;
	/// <summary>
	/// Attempted update of read - only column.
	/// </summary>
	public const int ReadOnlyColumn = 335544359;
	/// <summary>
	/// DSQL: @1 is not a valid base table of the specified view.
	/// </summary>
	public const int DsqlBaseTable = 335544658;
	/// <summary>
	/// Must specify column name for view select expression.
	/// </summary>
	public const int SpecifyFieldError = 335544598;
	/// <summary>
	/// Number of columns does not match select list.
	/// </summary>
	public const int NumFieldError = 335544599;
	/// <summary>
	/// Database key not available for multi-table views.
	/// </summary>
	public const int NoDbKey = 335544685;
	/// <summary>
	/// Input parameter mismatch for procedure @1.
	/// </summary>
	public const int ProcMismatch = 335544512;
	/// <summary>
	/// External functions cannot have morethan 10 parametrs.
	/// </summary>
	public const int ExternalFunctionError = 335544619;
	/// <summary>
	/// Output parameter mismatch for procedure @1.
	/// </summary>
	public const int ProcOutParamMismatch = 335544850;
	/// <summary>
	/// Function @1 could not be matched.
	/// </summary>
	public const int FuncMismatch = 335544439;
	/// <summary>
	/// Column not array or invalid dimensions (expected @1, encountered @2).
	/// </summary>
	public const int InvalidDimension = 335544458;
	/// <summary>
	/// Return mode by value not allowed for this data type.
	/// </summary>
	public const int ReturnModeError = 335544618;
	/// <summary>
	/// Array data type can use up to @1 dimensions.
	/// </summary>
	public const int ArrayMaxDimensions = 335544873;
	/// <summary>
	/// Function @1 is not defined.
	/// </summary>
	public const int FuncNotDefined = 335544438;
	/// <summary>
	/// Ambiguous column reference.
	/// </summary>
	public const int DynamicFieldAmbiguous = 335544708;
	/// <summary>
	/// Ambiguous field name between @1 and @2.
	/// </summary>
	public const int DsqlAmbiguousFieldName = 336003085;
	/// <summary>
	/// Generator @1 is not defined.
	/// </summary>
	public const int GeneratorNotDefined = 335544463;
	/// <summary>
	/// Reference to invalid stream number.
	/// </summary>
	public const int StreamNotDefined = 335544502;
	/// <summary>
	/// CHARACTER SET @1 is not defined.
	/// </summary>
	public const int CharsetNotFound = 335544509;
	/// <summary>
	/// Procedure @1 is not defined.
	/// </summary>
	public const int ProcNotDefined = 335544511;
	/// <summary>
	/// Status code @1 unknown.
	/// </summary>
	public const int ErrorCodeNotDefined = 335544515;
	/// <summary>
	/// Exception @1 not defined.
	/// </summary>
	public const int ExceptionCodeNotDefined = 335544516;
	/// <summary>
	/// Name of Referential Constraint not defined in constraints table.
	/// </summary>
	public const int RefConstraintNotFound = 335544532;
	/// <summary>
	/// Could not find table/procedure for GRANT.
	/// </summary>
	public const int GrantObjNotFound = 335544551;
	/// <summary>
	/// Implementation of text subtype @1 not located.
	/// </summary>
	public const int TextSubtype = 335544568;
	/// <summary>
	/// DSQL: Unknown data type.
	/// </summary>
	public const int DsqlDatatypeError = 335544573;
	/// <summary>
	/// DSQL: Table unknown.
	/// </summary>
	public const int DsqlRelationError = 335544580;
	/// <summary>
	/// DSQL: Procedure unknown.
	/// </summary>
	public const int DsqlProcedureError = 335544581;
	/// <summary>
	/// Collation @1 for character set @2 is not defined.
	/// </summary>
	public const int CollationNotFound = 335544588;
	/// <summary>
	/// Collation @1 is not valid for specified character set.
	/// </summary>
	public const int CollationNotForCharset = 335544589;
	/// <summary>
	/// Unknown trigger.
	/// </summary>
	public const int DsqlTriggerError = 335544595;
	/// <summary>
	/// Alias @1 conflicts with an alias in the same statement.
	/// </summary>
	public const int AliasConflictError = 335544620;
	/// <summary>
	/// Alias @1 conflicts with a procedure in the same statement.
	/// </summary>
	public const int ProcedureConflictError = 335544621;
	/// <summary>
	/// Alias @1 conflicts with a table in the same statement.
	/// </summary>
	public const int RelationConflictError = 335544622;
	/// <summary>
	/// DSQL: There is no alias or table named @1 at this scope level.
	/// </summary>
	public const int DsqlNoRelationAlias = 335544635;
	/// <summary>
	/// There is no index @1 for table @2.
	/// </summary>
	public const int NoIndexName = 335544636;
	/// <summary>
	/// Invalid use of CHARACTER SET or COLLATE.
	/// </summary>
	public const int CollationRequiresText = 335544640;
	/// <summary>
	/// BLOB subtype @1 is not defined.
	/// </summary>
	public const int DsqlBlobTypeUnknown = 335544662;
	/// <summary>
	/// Can not define a not null column with NULL as default value.
	/// </summary>
	public const int BadDefaultValue = 335544759;
	/// <summary>
	/// Invalid clause - '@1'.
	/// </summary>
	public const int InvalidClause = 335544760;
	/// <summary>
	/// Too many Contexts of Relation/Procedure/Views. Maximum allowed is 255.
	/// </summary>
	public const int TooManyContexts = 335544800;
	/// <summary>
	/// Invalid parameter to FIRST. Only integers >= 0 are allowed.
	/// </summary>
	public const int BadLimitParam = 335544817;
	/// <summary>
	/// Invalid parameter to SKIP. Only integers >= 0 are allowed.
	/// </summary>
	public const int BadSkipParam = 335544818;
	/// <summary>
	/// Invalid offset parameter @1 to SUBSTRING. Only positive integers are allowed.
	/// </summary>
	public const int BadSubstringOffset = 335544837;
	/// <summary>
	/// Invalid length parameter @1 to SUBSTRING. Negative integers are not allowed.
	/// </summary>
	public const int BadSubstringLength = 335544853;
	/// <summary>
	/// CHARACTER SET @1 is not installed.
	/// </summary>
	public const int CharsetNotInstalled = 335544854;
	/// <summary>
	/// COLLATION @1 for CHARACTER SET @2 is not installed.
	/// </summary>
	public const int CollationNotInstalled = 335544855;
	/// <summary>
	/// Blob subtypes bigger than 1 (text) are for internal use only.
	/// </summary>
	public const int SubtypeForInternalUse = 335544867;
	/// <summary>
	/// Column @1 is not defined in table @2.
	/// </summary>
	public const int FieldNotDefined = 335544396;
	/// <summary>
	/// Could not find column for GRANT.
	/// </summary>
	public const int GrantFieldNotfound = 335544552;
	/// <summary>
	/// Column @1 is not defined in procedure @2.
	/// </summary>
	public const int FieldNotDefinedInProc = 335544883;
	/// <summary>
	/// DSQL: Unknown column.
	/// </summary>
	public const int DsqlFieldError = 335544578;
	/// <summary>
	/// DSQL: Column is not a BLOB.
	/// </summary>
	public const int DsqlBlobError = 335544587;
	/// <summary>
	/// DSQL: Subselect illegal in this context.
	/// </summary>
	public const int DsqlSubselectError = 335544596;
	/// <summary>
	/// DSQL: At line @1, column @2.
	/// </summary>
	public const int DsqlLineColError = 336397208;
	/// <summary>
	/// DSQL: At unknown line and column.
	/// </summary>
	public const int DsqlUnknownPos = 336397209;
	/// <summary>
	/// Column @1 cannot be repeated in @2 statement.
	/// </summary>
	public const int DsqlNoDupName = 336397210;
	/// <summary>
	/// Invalid ORDER BY clause.
	/// </summary>
	public const int OrderByError = 335544617;
	/// <summary>
	/// Table @1 is not defined.
	/// </summary>
	public const int RelationNotDefined = 335544395;
	/// <summary>
	/// Domain @1 is not defined.
	/// </summary>
	public const int DomainNotDefined = 335544872;
	/// <summary>
	/// WAL Writer error.
	/// </summary>
	public const int WalWriterError = 335544487;
	/// <summary>
	/// Invalid version of log file @1.
	/// </summary>
	public const int LogHeaderSmall = 335544488;
	/// <summary>
	/// Log file @1 not latest in the chain but open flag still set.
	/// </summary>
	public const int LogHeaderInvalidVersion = 335544489;
	/// <summary>
	/// Log file @1 not closed properly; database recovery may be required.
	/// </summary>
	public const int LogHeaderOpenFlag2 = 335544491;
	/// <summary>
	/// Database name in the log file @1 is different.
	/// </summary>
	public const int LogHeaderDiffDbname = 335544492;
	/// <summary>
	/// Unexpected end of log file @1 at offset @2.
	/// </summary>
	public const int LogFileUnexpectedEof = 335544493;
	/// <summary>
	/// Incomplete log record at offset @1 in log file @2.
	/// </summary>
	public const int LogRecordIncomplete = 335544494;
	/// <summary>
	/// Log record header too small at offset @1 in log file @.
	/// </summary>
	public const int LogRecordHeaderSmall2 = 335544495;
	/// <summary>
	/// Log block too small at offset @1 in log file @2.
	/// </summary>
	public const int LogBlockSmall = 335544496;
	/// <summary>
	/// Insufficient memory to allocate page buffer cache.
	/// </summary>
	public const int CacheTooSmall = 335544691;
	/// <summary>
	/// Log size too small.
	/// </summary>
	public const int LogSizeTooSmall = 335544693;
	/// <summary>
	/// Log partition size too small.
	/// </summary>
	public const int PartitionTooSmall = 335544694;
	/// <summary>
	/// Database does not use Write-ahead Log.
	/// </summary>
	public const int NoWriteAheadLog = 335544500;
	/// <summary>
	/// WAL defined; Cache Manager must be started first.
	/// </summary>
	public const int StartCacheManagerForWal = 335544566;
	/// <summary>
	/// Cache redefined.
	/// </summary>
	public const int CacheRedef = 335544690;
	/// <summary>
	/// Log redefined.
	/// </summary>
	public const int LogRedef = 335544692;
	/// <summary>
	/// Partitions not supported in series of log file specification.
	/// </summary>
	public const int PartitionNotSupported = 335544695;
	/// <summary>
	/// Total length of a partitioned log must be specified.
	/// </summary>
	public const int LogLengthSpec = 335544696;
	/// <summary>
	/// Table @1 is not referenced in plan.
	/// </summary>
	public const int NoStreamPlan = 335544637;
	/// <summary>
	/// Table @1 is referenced more than once in plan; use aliases to distinguish.
	/// </summary>
	public const int StreamTwice = 335544638;
	/// <summary>
	/// DSQL: The table @1 is referenced twice; use aliases to differentiate.
	/// </summary>
	public const int DsqlSelfJoinError = 335544643;
	/// <summary>
	/// Table @1 is referenced twice in view; use an alias to distinguish.
	/// </summary>
	public const int DuplicateBaseTable = 335544659;
	/// <summary>
	/// View @1 has more than one base table; use aliases to distinguish.
	/// </summary>
	public const int ViewAlias = 335544660;
	/// <summary>
	/// Navigational stream @1 references a view with more than one base table.
	/// </summary>
	public const int ComplexView = 335544710;
	/// <summary>
	/// Table @1 is referenced in the plan but not the from list.
	/// </summary>
	public const int StreamNotFound = 335544639;
	/// <summary>
	/// Index @1 cannot be used in the specified plan.
	/// </summary>
	public const int IndexUnused = 335544642;
	/// <summary>
	/// Column used in a PRIMARY constraint must be NOT NULL.
	/// </summary>
	public const int PrimaryKeyNotNull = 335544531;
	/// <summary>
	/// Cannot update constraints (RDB$REF_CONSTRAINTS).
	/// </summary>
	public const int RefConstraintUpdate = 335544534;
	/// <summary>
	/// Cannot update constraints (RDB$CHECK_CONSTRAINTS).
	/// </summary>
	public const int CheckConstraintUpdate = 335544535;
	/// <summary>
	/// Cannot delete CHECK constraint entry (RDB$CHECK_CONSTRAINTS).
	/// </summary>
	public const int CheckConstraintDelete = 335544536;
	/// <summary>
	/// Cannot update constraints (RDB$RELATION_CONSTRAINTS).
	/// </summary>
	public const int RelConstraintUpdate = 335544545;
	/// <summary>
	/// Internal gds software consistency check (invalid RDB$CONSTRAINT_TYPE).
	/// </summary>
	public const int InvalidConstraintType = 335544547;
	/// <summary>
	/// Operation violates check constraint @1 on view or table @2.
	/// </summary>
	public const int CheckConstraint = 335544558;
	/// <summary>
	/// UPDATE OR INSERT field list does not match primary key of table @1.
	/// </summary>
	public const int UpdateInsertDoesntMatchPrimaryKey = 336003099;
	/// <summary>
	/// UPDATE OR INSERT field list does not match MATCHING clause.
	/// </summary>
	public const int UpdateInsertDoesntMatchMatching = 336003100;
	/// <summary>
	/// DSQL: Count of column list and variable list do not match.
	/// </summary>
	public const int DsqlCountMismatch = 335544669;
	/// <summary>
	/// Cannot transliterate character between character sets.
	/// </summary>
	public const int TransliterationFailed = 335544565;
	/// <summary>
	/// Cannot change datatype for column @1.
	/// Changing datatype is not supported for BLOB or ARRAY columns.
	/// </summary>
	public const int DynamicDatatypeInvalid = 336068815;
	/// <summary>
	/// Column @1 from table @2 is referenced in @3.
	/// </summary>
	public const int DynamicDependencyExists = 336068814;
	/// <summary>
	/// Invalid comparison operator for find operation.
	/// </summary>
	public const int InvalidOperator = 335544647;
	/// <summary>
	/// Attempted invalid operation on a BLOB.
	/// </summary>
	public const int InvalidBlobOperation = 335544368;
	/// <summary>
	/// BLOB and array data types are not supported for @1 operation.
	/// </summary>
	public const int BlobNotSupported = 335544414;
	/// <summary>
	/// Data operation not supported.
	/// </summary>
	public const int DataOperationNotSupported = 335544427;
	/// <summary>
	/// Subscript out of bounds.
	/// </summary>
	public const int OutOfBounds = 335544457;
	/// <summary>
	/// Null segment of UNIQUE KEY.
	/// </summary>
	public const int NullSegmentKey = 335544435;
	/// <summary>
	/// Conversion error from string "@1".
	/// </summary>
	public const int StrConvertError = 335544334;
	/// <summary>
	/// Filter not found to convert type @1 to type @2.
	/// </summary>
	public const int NoFilter = 335544454;
	/// <summary>
	/// Unsupported conversion to target type BLOB (subtype @1).
	/// </summary>
	public const int BlobConvertError = 335544860;
	/// <summary>
	/// Unsupported conversion to target type ARRAY.
	/// </summary>
	public const int ArrayConvertError = 335544861;
	/// <summary>
	/// DSQL: Attempt to reclose a closed cursor.
	/// </summary>
	public const int DsqlCursorCloseError = 335544577;
	/// <summary>
	/// DSQL: Statement already has a cursor @1 assigned.
	/// </summary>
	public const int DsqlCursorRedefined = 336003090;
	/// <summary>
	/// DSQL: Cursor @1 is not found in the current context.
	/// </summary>
	public const int DsqlCursorNotFound = 336003091;
	/// <summary>
	/// DSQL: Cursor @1 already exists in the current context.
	/// </summary>
	public const int DsqlCursorExists = 336003092;
	/// <summary>
	/// DSQL: Relation @1 is ambiguous in cursor @2.
	/// </summary>
	public const int DsqlCursorRelAmbiguous = 336003093;
	/// <summary>
	/// DSQL: Relation @1 is not found in cursor @2.
	/// </summary>
	public const int DsqlCursorRelNotFound = 336003094;
	/// <summary>
	/// DSQL: Cursor is not open.
	/// </summary>
	public const int DsqlCursorNotOpen = 336003095;
	/// <summary>
	/// DSQL: Invalid cursor declaration.
	/// </summary>
	public const int DsqlDeclarationError = 335544574;
	/// <summary>
	/// DSQL: Attempt to reopen an open cursor.
	/// </summary>
	public const int DsqlCursorOpenError = 335544576;
	/// <summary>
	/// DSQL: Empty cursor name is not allowed.
	/// </summary>
	public const int DsqlCursorInvalid = 336003089;
	/// <summary>
	/// DSQL: Invalid cursor reference.
	/// </summary>
	public const int DsqlCursorError = 335544572;
	/// <summary>
	/// No current record for fetch operation.
	/// </summary>
	public const int NoCurrentRecord = 335544348;
	/// <summary>
	/// DSQL: Cursor @1 is not updatable.
	/// </summary>
	public const int DsqlCursorNotUpdatable = 335544575;
	/// <summary>
	/// DSQL: Request unknown.
	/// </summary>
	public const int DsqlRequestUnknown = 335544582;
	/// <summary>
	/// DSQL: The prepare statement identifies a prepare statement with an open cursor.
	/// </summary>
	public const int DsqlOpenCursorRequest = 335544688;
	/// <summary>
	/// Violation of FOREIGN KEY constraint "@1" on table "@2".
	/// </summary>
	public const int ForeignKeyConstraintViolation = 335544466;
	/// <summary>
	/// Foreign key reference target does not exist.
	/// </summary>
	public const int ForeignKeyTargetDoesntExist = 335544838;
	/// <summary>
	/// Foreign key references are present for the record.
	/// </summary>
	public const int ForeignKeyReferencesPresent = 335544839;
	/// <summary>
	/// DSQL: Cannot prepare a CREATE DATABASE/SCHEMA statement.
	/// </summary>
	public const int DsqlCreateDbPrepareError = 335544597;
	/// <summary>
	/// Transaction marked invalid by I/O error.
	/// </summary>
	public const int TransactionInvalid = 335544469;
	/// <summary>
	/// No permission for @1 access to @2 @3.
	/// </summary>
	public const int NoPriv = 335544352;
	/// <summary>
	/// Service @1 requires SYSDBA permissions.
	/// Reattach to the Service Manager using the SYSDBA account
	/// </summary>
	public const int InsufficientServicePrivileges = 335544790;
	/// <summary>
	/// Only the owner of a table may reassign ownership.
	/// </summary>
	public const int NotRelationOwner = 335544550;
	/// <summary>
	/// User does not have GRANT privileges for operation.
	/// </summary>
	public const int GrantNoPriv = 335544553;
	/// <summary>
	/// User does not have GRANT privileges on base table/view for operation.
	/// </summary>
	public const int GrantNoPrivOnBase = 335544707;
	/// <summary>
	/// Cannot modify an existing user privilege.
	/// </summary>
	public const int ExistingPrivMod = 335544529;
	/// <summary>
	/// The current position is on a crack.
	/// </summary>
	public const int StreamCrack = 335544645;
	/// <summary>
	/// Illegal operation when at beginning of stream.
	/// </summary>
	public const int StreamBof = 335544644;
	/// <summary>
	/// DSQL: Preceding file did not specify length, so @1 must include starting page number.
	/// </summary>
	public const int DsqlFileLengthError = 335544632;
	/// <summary>
	/// Shadow number must be a positive integer.
	/// </summary>
	public const int DsqlShadowNumberError = 335544633;
	/// <summary>
	/// Gen.c: node not supported.
	/// </summary>
	public const int NodeError = 335544607;
	/// <summary>
	/// A node name is not permitted in a secondary, shadow, cache or log file name.
	/// </summary>
	public const int NodeNameError = 335544625;
	/// <summary>
	/// Sort error: corruption in data structure.
	/// </summary>
	public const int CorruptDataError = 335544680;
	/// <summary>
	/// Database or file exists.
	/// </summary>
	public const int DbOrFileExists = 335544646;
	/// <summary>
	/// DSQL: Array declared with too many dimensions.
	/// </summary>
	public const int DsqlMaxArrDimensionExceeded = 335544593;
	/// <summary>
	/// Illegal array dimension range.
	/// </summary>
	public const int DsqlArrRangeError = 335544594;
	/// <summary>
	/// Inappropriate self-reference of column.
	/// </summary>
	public const int DsqlFieldRef = 335544682;
	/// <summary>
	/// Cannot SELECT RDB$DB_KEY from a stored procedure.
	/// </summary>
	public const int DsqlDbkeyFromNonTable = 336003074;
	/// <summary>
	/// External function should have return position between 1 and @1.
	/// </summary>
	public const int DsqlUdfReturnPosError = 336003086;
	/// <summary>
	/// Data type @1 is not supported for EXTERNAL TABLES. Relation '@2', field '@3'.
	/// </summary>
	public const int DsqlTypeNotSupportExtTab = 336003096;
	/// <summary>
	/// Unsuccessful metadata update.
	/// </summary>
	public const int NoMetaUpdate = 335544351;
	/// <summary>
	/// Cannot modify or erase a system trigger.
	/// </summary>
	public const int SysTriggerUpdate = 335544549;
	/// <summary>
	/// DSQL: Array/BLOB/DATE data types not allowed in arithmetic.
	/// </summary>
	public const int DsqlNoBlobArray = 335544657;
	/// <summary>
	/// "REFERENCES table" without "(column)" requires PRIMARY KEY on referenced table.
	/// </summary>
	public const int ReftableRequiresPrimaryKey = 335544746;
	/// <summary>
	/// GENERATOR @1.
	/// </summary>
	public const int GeneratorName = 335544815;
	/// <summary>
	/// UDF @1.
	/// </summary>
	public const int UdfName = 335544816;
	/// <summary>
	/// Can't have relation with only computed fields or constraints.
	/// </summary>
	public const int MustHavePhysField = 335544858;
	/// <summary>
	/// DSQL: Table @1 does not exist.
	/// </summary>
	public const int DsqlTableNotFound = 336397206;
	/// <summary>
	/// DSQL: View @1 does not exist.
	/// </summary>
	public const int DsqlViewNotFound = 336397207;
	/// <summary>
	/// DSQL: Array and BLOB data types not allowed in computed field.
	/// </summary>
	public const int DsqlNoArrayOnCompField = 336397212;
	/// <summary>
	/// DSQL: Scalar operator used on field @1 which is not an array.
	/// </summary>
	public const int DsqlOnlyCanSubscriptArray = 336397214;
	/// <summary>
	/// Cannot rename domain @1 to @2. A domain with that name already exists.
	/// </summary>
	public const int DynDomainNameExists = 336068812;
	/// <summary>
	/// Cannot rename column @1 to @2.
	/// A column with that name already exists in table @3.
	/// </summary>
	public const int DynFieldNameExists = 336068813;
	/// <summary>
	/// Lock on table @1 conflicts with existing lock.
	/// </summary>
	public const int RelationLock = 335544475;
	/// <summary>
	/// Requested record lock conflicts with existing lock.
	/// </summary>
	public const int RecordLock = 335544476;
	/// <summary>
	/// Refresh range number @1 already in use.
	/// </summary>
	public const int RangeInUse = 335544507;
	/// <summary>
	/// Cannot delete PRIMARY KEY being used in FOREIGN KEY definition.
	/// </summary>
	public const int PrimaryKeyOnForeignDel = 335544530;
	/// <summary>
	/// Cannot delete index used by an Integrity Constraint.
	/// </summary>
	public const int IntegrityIndexDel = 335544539;
	/// <summary>
	/// Cannot modify index used by an Integrity Constraint.
	/// </summary>
	public const int IntegrityIndexMod = 335544540;
	/// <summary>
	/// Cannot delete trigger used by a CHECK Constraint.
	/// </summary>
	public const int CheckTrigDel = 335544541;
	/// <summary>
	/// Cannot delete column being used in an Integrity Constraint.
	/// </summary>
	public const int ConstraintFieldDel = 335544543;
	/// <summary>
	/// There are @1 dependencies.
	/// </summary>
	public const int Dependency = 335544630;
	/// <summary>
	/// Last column in a table cannot be deleted.
	/// </summary>
	public const int DelLastField = 335544674;
	/// <summary>
	/// Cannot deactivate index used by an integrity constraint.
	/// </summary>
	public const int IntegrityIndexDeactivate = 335544728;
	/// <summary>
	/// Cannot deactivate index used by a PRIMARY/UNIQUE constraint.
	/// </summary>
	public const int IntegDeactivatePrimary = 335544729;
	/// <summary>
	/// Cannot update trigger used by a CHECK Constraint.
	/// </summary>
	public const int CheckTriggerUpdate = 335544542;
	/// <summary>
	/// Cannot rename column being used in an Integrity Constraint.
	/// </summary>
	public const int ConstraintFieldRename = 335544544;
	/// <summary>
	/// Cannot delete index segment used by an Integrity Constraint.
	/// </summary>
	public const int IntegrityIndexSegmentDel = 335544537;
	/// <summary>
	/// Cannot update index segment used by an Integrity Constraint.
	/// </summary>
	public const int IntegrityIndexSegmentMod = 335544538;
	/// <summary>
	/// Validation error for column @1, value "@2".
	/// </summary>
	public const int NotValid = 335544347;
	/// <summary>
	/// Validation error for variable @1, value "@2".
	/// </summary>
	public const int NotValidForVar = 335544879;
	/// <summary>
	/// Validation error for @1, value "@2".
	/// </summary>
	public const int NotValidFor = 335544880;
	/// <summary>
	/// Duplicate specification of @1- not supported.
	/// </summary>
	public const int DsqlDuplicateSpec = 335544664;
	/// <summary>
	/// Implicit domain name @1 not allowed in user created domain.
	/// </summary>
	public const int DsqlImplicitDomainName = 336397213;
	/// <summary>
	/// Primary key required on table @1.
	/// </summary>
	public const int PrimaryKeyRequired = 336003098;
	/// <summary>
	/// Non-existent PRIMARY or UNIQUE KEY specified for FOREIGN KEY.
	/// </summary>
	public const int ForeignKeyNotFound = 335544533;
	/// <summary>
	/// Cannot create index @1.
	/// </summary>
	public const int IndexCreateError = 335544628;
	/// <summary>
	/// Segment count of 0 defined for index @1.
	/// </summary>
	public const int IndexSegmentError = 335544624;
	/// <summary>
	/// Too many keys defined for index @1.
	/// </summary>
	public const int IndexKeyError = 335544631;
	/// <summary>
	/// Too few key columns found for index @1 (incorrect column name?).
	/// </summary>
	public const int KeyFieldError = 335544672;
	/// <summary>
	/// Key size exceeds implementation restriction for index "@1".
	/// </summary>
	public const int KeyTooBig = 335544434;
	/// <summary>
	/// @1 extension error.
	/// </summary>
	public const int ExtErr = 335544445;
	/// <summary>
	/// Invalid BLOB type for operation.
	/// </summary>
	public const int BadBlobType = 335544465;
	/// <summary>
	/// Attempt to index BLOB column in index @1.
	/// </summary>
	public const int BlobIndexError = 335544670;
	/// <summary>
	/// Attempt to index array column in index @1.
	/// </summary>
	public const int ArrayIndexError = 335544671;
	/// <summary>
	/// Page @1 is of wrong type (expected @2, found @3).
	/// </summary>
	public const int BadPageType = 335544403;
	/// <summary>
	/// Wrong page type.
	/// </summary>
	public const int PageTypeError = 335544650;
	/// <summary>
	/// Segments not allowed in expression index @1.
	/// </summary>
	public const int NoSegmentsError = 335544679;
	/// <summary>
	/// New record size of @1 bytes is too big.
	/// </summary>
	public const int RecordSizeError = 335544681;
	/// <summary>
	/// Maximum indexes per table (@1) exceeded.
	/// </summary>
	public const int MaxIndex = 335544477;
	/// <summary>
	/// Too many concurrent executions of the same request.
	/// </summary>
	public const int RequestMaxClonesExceeded = 335544663;
	/// <summary>
	/// Cannot access column @1 in view @2.
	/// </summary>
	public const int NoFieldAccess = 335544684;
	/// <summary>
	/// Arithmetic exception, numeric overflow, or string truncation.
	/// </summary>
	public const int FbArithmeticException = 335544321;
	/// <summary>
	/// Concatenation overflow. Resulting string cannot exceed 32K in length.
	/// </summary>
	public const int ConcatOverflow = 335544836;
	/// <summary>
	/// Attempt to store duplicate value (visible to active transactions)
	/// in unique index "@1".
	/// </summary>
	public const int NoDuplication = 335544349;
	/// <summary>
	/// Violation of PRIMARY or UNIQUE KEY constraint "@1" on table "@2".
	/// </summary>
	public const int UniqueKeyViolation = 335544665;
	/// <summary>
	/// DSQL: Feature not supported on ODS version older than @1.@2.
	/// </summary>
	public const int DsqlFeatureNotSupportedOds = 336003097;
	/// <summary>
	/// Wrong number of arguments on call.
	/// </summary>
	public const int WrongArgNum = 335544380;
	/// <summary>
	/// DSQL: SQLDA missing or incorrect version, or incorrect number/type of variables.
	/// </summary>
	public const int DsqlSqldaError = 335544583;
	/// <summary>
	/// DSQL: Count of read - write columns does not equal count of values.
	/// </summary>
	public const int DsqlVarCountError = 335544584;
	/// <summary>
	/// Unknown function.
	/// </summary>
	public const int DsqlFunctionError = 335544586;
	/// <summary>
	/// Incorrect values within SQLDA structure.
	/// </summary>
	public const int DsqlSqldaValueError = 335544713;
	/// <summary>
	/// ODS versions before ODS@1 are not supported.
	/// </summary>
	public const int DsqlTooOldOds = 336397205;
	/// <summary>
	/// Only simple column names permitted for VIEW WITH CHECK OPTION.
	/// </summary>
	public const int ColNameError = 335544600;
	/// <summary>
	/// No WHERE clause for VIEW WITH CHECK OPTION.
	/// </summary>
	public const int WhereError = 335544601;
	/// <summary>
	/// Only one table allowed for VIEW WITH CHECK OPTION.
	/// </summary>
	public const int TableViewError = 335544602;
	/// <summary>
	/// DISTINCT, GROUP or HAVING not permitted for VIEW WITH CHECK OPTION.
	/// </summary>
	public const int DistinctError = 335544603;
	/// <summary>
	/// No subqueries permitted for VIEW WITH CHECK OPTION.
	/// </summary>
	public const int SubqueryError = 335544605;
	/// <summary>
	/// Multiple rows in singleton select.
	/// </summary>
	public const int SingletonSelectError = 335544652;
	/// <summary>
	/// Cannot insert because the file is readonly or is on a read only medium.
	/// </summary>
	public const int ExtReadonlyError = 335544651;
	/// <summary>
	/// Operation not supported for EXTERNAL FILE table @1.
	/// </summary>
	public const int ExtFileUnsupportedOperation = 335544715;
	/// <summary>
	/// DB dialect @1 and client dialect @2 conflict with respect to numeric precision @3.
	/// </summary>
	public const int IscSqlDialectConflictNum = 336003079;
	/// <summary>
	/// UPDATE OR INSERT without MATCHING could not be used with views based on more than one table.
	/// </summary>
	public const int UpdateInsertWithComplexView = 336003101;
	/// <summary>
	/// DSQL: Incompatible trigger type.
	/// </summary>
	public const int DsqlIncompatibleTriggerType = 336003102;
	/// <summary>
	/// Database trigger type can't be changed.
	/// </summary>
	public const int DsqlDbTriggerTypeCannotChange = 336003103;
	/// <summary>
	/// Attempted update during read - only transaction.
	/// </summary>
	public const int ReadOnlyTransaction = 335544361;
	/// <summary>
	/// Attempted write to read-only BLOB.
	/// </summary>
	public const int BlobNoWrite = 335544371;
	/// <summary>
	/// Operation not supported.
	/// </summary>
	public const int ReadOnly = 335544444;
	/// <summary>
	/// Attempted update on read - only database.
	/// </summary>
	public const int ReadOnlyDatabase = 335544765;
	/// <summary>
	/// SQL dialect @1 is not supported in this database.
	/// </summary>
	public const int MustBeDialect2AndUp = 335544766;
	/// <summary>
	/// Metadata update statement is not allowed by the current database SQL dialect @1.
	/// </summary>
	public const int DdlNotAllowedByDbSqlDialect = 335544793;
	/// <summary>
	/// Metadata is obsolete.
	/// </summary>
	public const int ObsoleteMetadata = 335544356;
	/// <summary>
	/// Unsupported on - disk structure for file @1; found @2.@3, support @4.@5.
	/// </summary>
	public const int WrongOds = 335544379;
	/// <summary>
	/// Wrong DYN version.
	/// </summary>
	public const int WrongDynVersion = 335544437;
	/// <summary>
	/// Minor version too high found @1 expected @2.
	/// </summary>
	public const int HighMinor = 335544467;
	/// <summary>
	/// Difference file name should be set explicitly for database on raw device.
	/// </summary>
	public const int NeedDifference = 335544881;
	/// <summary>
	/// Invalid bookmark handle.
	/// </summary>
	public const int InvalidBookmark = 335544473;
	/// <summary>
	/// Invalid lock level @1.
	/// </summary>
	public const int BadLockLevel = 335544474;
	/// <summary>
	/// Invalid lock handle.
	/// </summary>
	public const int BadLockHandle = 335544519;
	/// <summary>
	/// DSQL: Invalid statement handle.
	/// </summary>
	public const int DsqlStatementHandle = 335544585;
	/// <summary>
	/// Invalid direction for find operation.
	/// </summary>
	public const int InvalidDirection = 335544655;
	/// <summary>
	/// Invalid key for find operation.
	/// </summary>
	public const int InvalidKey = 335544718;
	/// <summary>
	/// Invalid key position.
	/// </summary>
	public const int InvalidKeyPosition = 335544678;
	/// <summary>
	/// New size specified for column @1 must be at least @2 characters.
	/// </summary>
	public const int DynCharFieldTooSmall = 336068816;
	/// <summary>
	/// Cannot change datatype for @1.Conversion from base type @2 to @3 is not supported.
	/// </summary>
	public const int DynInvalidDataTypeConversion = 336068817;
	/// <summary>
	/// Cannot change datatype for column @1 from a character type to a non-character type.
	/// </summary>
	public const int DynDatatypeConversionError = 336068818;
	/// <summary>
	/// Maximum number of collations per character set exceeded.
	/// </summary>
	public const int MaxCollationsPerCharset = 336068829;
	/// <summary>
	/// Invalid collation attributes.
	/// </summary>
	public const int InvalidCollationAttr = 336068830;
	/// <summary>
	/// New scale specified for column @1 must be at most @2.
	/// </summary>
	public const int DynScaleTooBig = 336068852;
	/// <summary>
	/// New precision specified for column @1 must be at least @2.
	/// </summary>
	public const int DynPrecisionTooSmall = 336068853;
	/// <summary>
	/// Invalid column reference.
	/// </summary>
	public const int FieldRefError = 335544616;
	/// <summary>
	/// Column used with aggregate.
	/// </summary>
	public const int FieldAggregateError = 335544615;
	/// <summary>
	/// Attempt to define a second PRIMARY KEY for the same table.
	/// </summary>
	public const int PrimaryKeyExists = 335544548;
	/// <summary>
	/// FOREIGN KEY column count does not match PRIMARY KEY.
	/// </summary>
	public const int KeyFieldCountError = 335544604;
	/// <summary>
	/// Expression evaluation not supported.
	/// </summary>
	public const int ExpressionEvalError = 335544606;
	/// <summary>
	/// Value exceeds the range for valid dates.
	/// </summary>
	public const int DateRangeExceeded = 335544810;
	/// <summary>
	/// Refresh range number @1 not found.
	/// </summary>
	public const int RangeNotFound = 335544508;
	/// <summary>
	/// Bad checksum.
	/// </summary>
	public const int BadChecksum = 335544649;
	/// <summary>
	/// Restart shared cache manager.
	/// </summary>
	public const int CacheRestart = 335544518;
	/// <summary>
	/// Database @1 shutdown in @2 seconds.
	/// </summary>
	public const int ShutdownWarning = 335544560;
	/// <summary>
	/// Too many versions.
	/// </summary>
	public const int VersionError = 335544677;
	/// <summary>
	/// Precision must be from 1 to 18.
	/// </summary>
	public const int PrecisionError = 335544697;
	/// <summary>
	/// Scale must be between zero and precision.
	/// </summary>
	public const int ScaleBetweenZeroAndPrecision = 335544698;
	/// <summary>
	/// Short integer expected.
	/// </summary>
	public const int ShortIntExpected = 335544699;
	/// <summary>
	/// Long integer expected
	/// </summary>
	public const int LongIntExpected = 335544700;
	/// <summary>
	/// Unsigned short integer expected.
	/// </summary>
	public const int UShortIntExcepted = 335544701;
	/// <summary>
	/// Positive value expected.
	/// </summary>
	public const int PositiveNumExpected = 335544712;
	/// <summary>
	/// Database file name (@1) already given.
	/// </summary>
	public const int DbNameAlreadyGiven = 335740929;
	/// <summary>
	/// GBak: Unknown switch.
	/// </summary>
	public const int GbakUnknownSwitch = 336330753;
	/// <summary>
	/// GStat: Unknown switch.
	/// </summary>
	public const int GstatUnknownSwitch = 336920577;
	/// <summary>
	/// Wrong value for access mode.
	/// Symbol: fbsvcmgr_bad_am
	/// </summary>
	public const int WrongAccessModeVal = 336986113;
	/// <summary>
	/// Gfix: Invalid switch @1.
	/// </summary>
	public const int GfixInvalidSwitch = 335740930;
	/// <summary>
	/// Invalid database key.
	/// </summary>
	public const int BadDbKey = 335544322;
	/// <summary>
	/// Wrong value for write mode.
	/// Symbol: fbsvcmgr_bad_wm
	/// </summary>
	public const int WrongWriteModeVal = 336986114;
	/// <summary>
	/// GBak: Page size parameter missing.
	/// </summary>
	public const int GbakPageSizeMissing = 336330754;
	/// <summary>
	/// Gstat: Please retry, giving a database name.
	/// </summary>
	public const int GstatRetryDbName = 336920578;
	/// <summary>
	/// Wrong value for reserve space.
	/// </summary>
	public const int WrongReserveSpaceValue = 336986115;
	/// <summary>
	/// Gstat: Wrong ODS version, expected @1, encountered @2.
	/// </summary>
	public const int GstatWrongOds = 336920579;
	/// <summary>
	/// Page size specified (@1) greater than limit (16384 bytes).
	/// </summary>
	public const int GbakPageSizeTooBig = 336330755;
	/// <summary>
	/// Gfix: Incompatible switch combination.
	/// </summary>
	public const int GfixIncompatibleSwitch = 335740932;
	/// <summary>
	/// Gstat: Unexpected end of database file.
	/// </summary>
	public const int GstatUnexpectedEof = 336920580;
	/// <summary>
	/// Gbak: Redirect location for output is not specified.
	/// </summary>
	public const int GbakRedirectOutputMissing = 336330756;
	/// <summary>
	/// Unknown tag (@1) in info_svr_db_info block after isc_svc_query().
	/// Symbol: fbsvcmgr_info_err
	/// </summary>
	public const int FbsvcmgrUnknownTag = 336986116;
	/// <summary>
	/// Gfix: Replay log pathname required.
	/// </summary>
	public const int GfixReplayRequired = 335740933;
	/// <summary>
	/// Conflicting switches for backup/restore.
	/// </summary>
	public const int GbakSwitchesConflict = 336330757;
	/// <summary>
	/// Unknown tag (@1) in isc_svc_query() results.
	/// </summary>
	public const int FbsvcmgrQueryError = 336986117;
	/// <summary>
	/// Unrecognized database parameter block.
	/// </summary>
	public const int BadDbParamForm = 335544326;
	/// <summary>
	/// Gfix: Number of page buffers for cache required.
	/// </summary>
	public const int GfixPageBufferRequired = 335740934;
	/// <summary>
	/// Unknown switch "@1".
	/// Symbol: fbsvcmgr_switch_unknown
	/// </summary>
	public const int SwitchUnknown = 336986118;
	/// <summary>
	/// Gbak: Device type @1 not known.
	/// </summary>
	public const int GbakUnknownDevice = 336330758;
	/// <summary>
	/// Invalid request handle.
	/// </summary>
	public const int BadRequestHandle = 335544327;
	/// <summary>
	/// Numeric value required.
	/// </summary>
	public const int GfixNumericValueRequired = 335740935;
	/// <summary>
	/// Protection is not there yet.
	/// </summary>
	public const int GbakNoProtection = 336330759;
	/// <summary>
	/// Invalid BLOB handle.
	/// </summary>
	public const int BadBlobHandle = 335544328;
	/// <summary>
	/// Gfix: Positive numeric value required.
	/// </summary>
	public const int GfixPosiiveValRequired = 335740936;
	/// <summary>
	/// Page size is allowed only on restore or create.
	/// </summary>
	public const int GbakPageSizeNotAllowed = 336330760;
	/// <summary>
	/// Invalid BLOB ID.
	/// </summary>
	public const int BadBlobId = 335544329;
	/// <summary>
	/// Gfix: Number of transactions per sweep required.
	/// </summary>
	public const int GfixSweepTransactionRequired = 335740937;
	/// <summary>
	/// Gbak: Multiple sources or destinations specified.
	/// </summary>
	public const int GbakMultiSourceDest = 336330761;
	/// <summary>
	/// Invalid parameter in transaction parameter block.
	/// </summary>
	public const int BadTransactionParamContent = 335544330;
	/// <summary>
	/// Gbak: Requires both input and output filenames.
	/// </summary>
	public const int GbakFilenameMissing = 336330762;
	/// <summary>
	/// Invalid format for transaction parameter block.
	/// </summary>
	public const int BadTransactionParamForm = 335544331;
	/// <summary>
	/// Gbak: Input and output must not have the same name.
	/// </summary>
	public const int GbakDupInputOutputNames = 336330763;
	/// <summary>
	/// Gfix: "full" or "reserve" required.
	/// </summary>
	public const int GfixFullReserveRequired = 335740940;
	/// <summary>
	/// Invalid transaction handle (expecting explicit transaction start).
	/// </summary>
	public const int BadTransactionHandle = 335544332;
	/// <summary>
	/// Gbak: Expected page size, encountered "@1".
	/// </summary>
	public const int GbakInvalidPageSize = 336330764;
	/// <summary>
	/// Gfix: User name required.
	/// </summary>
	public const int GfixUsernameRequired = 335740941;
	/// <summary>
	/// Gbak: REPLACE specified, but the first file @1 is a database.
	/// </summary>
	public const int GbakDbSpecified = 336330765;
	/// <summary>
	/// Gfix: Password required.
	/// </summary>
	public const int GfixPasswordRequired = 335740942;
	/// <summary>
	/// Gbak: Database @1 already exists.To replace it, use the -REP switch.
	/// </summary>
	public const int GbakDbExists = 336330766;
	/// <summary>
	/// Gfix: Subsystem name.
	/// </summary>
	public const int GfixSubsystemName = 335740943;
	/// <summary>
	/// Gsec: Unable to open database.
	/// </summary>
	public const int GsecCantOpenDb = 336723983;
	/// <summary>
	/// Gbak: Device type not specified.
	/// </summary>
	public const int GbakDeviceNotSpecified = 336330767;
	/// <summary>
	/// Gsec: Error in switch specifications.
	/// </summary>
	public const int GsecSwitchesError = 336723984;
	/// <summary>
	/// Gfix: Number of seconds required.
	/// </summary>
	public const int GfixSecondsRequired = 335740945;
	/// <summary>
	/// Attempt to start more than @1 transactions.
	/// </summary>
	public const int ExcessTransactions = 335544337;
	/// <summary>
	/// Gsec: No operation specified.
	/// </summary>
	public const int GsecNoOperationSpecified = 336723985;
	/// <summary>
	/// Gfix: Numeric value between 0 and 32767 inclusive required.
	/// </summary>
	public const int GfixNum0To32767Required = 335740946;
	/// <summary>
	/// Gsec: No user name specified.
	/// </summary>
	public const int GsecNoUserName = 336723986;
	/// <summary>
	/// Gfix: Must specify type of shutdown.
	/// </summary>
	public const int GfixNoShutdownType = 335740947;
	/// <summary>
	/// Information type inappropriate for object specified.
	/// </summary>
	public const int InappriopriateInfo = 335544339;
	/// <summary>
	/// No information of this type available for object specified.
	/// </summary>
	public const int InfoNotAvailable = 335544340;
	/// <summary>
	/// Gsec: Add record error.
	/// </summary>
	public const int GsecAddRecError = 336723987;
	/// <summary>
	/// Gsec: Add record error.
	/// </summary>
	public const int GsecModifyRecError = 336723988;
	/// <summary>
	/// Gbak: Gds_$blob_info failed.
	/// </summary>
	public const int GbakBlobInfoFailed = 336330772;
	/// <summary>
	/// Please retry, specifying an option.
	/// </summary>
	public const int GfixRetry = 335740948;
	/// <summary>
	/// Unknown information item.
	/// </summary>
	public const int UnknownInfoItem = 335544341;
	/// <summary>
	/// Gsec: Find / modify record error.
	/// </summary>
	public const int GsecFindModError = 336723989;
	/// <summary>
	/// Gbak: Unknown BLOB INFO item @1.
	/// </summary>
	public const int GbakUnknownBlobItem = 336330773;
	/// <summary>
	/// Action cancelled by trigger (@1) to preserve data integrity.
	/// </summary>
	public const int IntegrationFail = 335544342;
	/// <summary>
	/// Gbak: Gds_$get_segment failed.
	/// </summary>
	public const int GbakGetSegFailed = 336330774;
	/// <summary>
	/// Gsec: Record not found for user: @1.
	/// </summary>
	public const int GsecRecNotFound = 336723990;
	/// <summary>
	/// Gsec: Delete record error.
	/// </summary>
	public const int GsecDeleteError = 336723991;
	/// <summary>
	/// Gbak: Gds_$close_blob failed.
	/// </summary>
	public const int GbakCloseBlobFailed = 336330775;
	/// <summary>
	/// Gfix: Please retry, giving a database name.
	/// </summary>
	public const int GfixRetryDb = 335740951;
	/// <summary>
	/// Gbak: Gds_$open_blob failed.
	/// </summary>
	public const int GbakOpenBlobFailed = 336330776;
	/// <summary>
	/// Gsec: Find / delete record error.
	/// </summary>
	public const int GsecFindDeleteError = 336723992;
	/// <summary>
	/// Lock conflict on no wait transaction.
	/// </summary>
	public const int LockConflict = 335544345;
	/// <summary>
	/// Gbak: Failed in put_blr_gen_id.
	/// </summary>
	public const int GbakPutBlrGenIdFailed = 336330777;
	/// <summary>
	/// Gbak: Data type @1 not understood.
	/// </summary>
	public const int GbakUnknownType = 336330778;
	/// <summary>
	/// Gbak: Gds_$compile_request failed.
	/// </summary>
	public const int GbakCompileRequestFailed = 336330779;
	/// <summary>
	/// Gbak: Gds_$start_request failed.
	/// </summary>
	public const int GbakStartRequestFailed = 336330780;
	/// <summary>
	/// Gsec: Find / display record error.
	/// </summary>
	public const int GsecFindDisplayError = 336723996;
	/// <summary>
	/// Gbak: gds_$receive failed.
	/// </summary>
	public const int GbakReceiveFailed = 336330781;
	/// <summary>
	/// Gsec: Can't open database file @1.
	/// </summary>
	public const int GstatDbFileOpenError = 336920605;
	/// <summary>
	/// Gsec: Invalid parameter, no switch defined.
	/// </summary>
	public const int GsecInvParam = 336723997;
	/// <summary>
	/// Program attempted to exit without finishing database.
	/// </summary>
	public const int NoFinish = 335544350;
	/// <summary>
	/// Gstat: Can't read a database page.
	/// </summary>
	public const int GstatReadError = 336920606;
	/// <summary>
	/// Gbak: Gds_$release_request failed.
	/// </summary>
	public const int GbakReleaseRequestFailed = 336330782;
	/// <summary>
	/// Gsec: Operation already specified.
	/// </summary>
	public const int GsecOperationSpecified = 336723998;
	/// <summary>
	/// Gstat: System memory exhausted.
	/// </summary>
	public const int GstatSysMemoryExhausted = 336920607;
	/// <summary>
	/// Gbak: gds_$database_info failed.
	/// </summary>
	public const int GbakDbInfoFailed = 336330783;
	/// <summary>
	/// Gsec: Password already specified.
	/// </summary>
	public const int GsecPasswordSpecified = 336723999;
	/// <summary>
	/// Gsec: Uid already specified.
	/// </summary>
	public const int GsecUidAlreadySpecified = 336724000;
	/// <summary>
	/// Gbak: Expected database description record.
	/// </summary>
	public const int GbakNoDbDescription = 336330784;
	/// <summary>
	/// Transaction is not in limbo.
	/// </summary>
	public const int TransactionNotInLimbo = 335544353;
	/// <summary>
	/// Gsec: Gid already specified.
	/// </summary>
	public const int GsecGidSpecified = 336724001;
	/// <summary>
	/// Gbak: Failed to create database @1.
	/// </summary>
	public const int GbakDbCreateFailed = 336330785;
	/// <summary>
	/// Gsec: Project already specified.
	/// </summary>
	public const int GsecProjectSpecified = 336724002;
	/// <summary>
	/// Gbak: RESTORE: decompression length error.
	/// </summary>
	public const int GbakDecompressionLengthError = 336330786;
	/// <summary>
	/// BLOB was not closed.
	/// </summary>
	public const int NoBlobClose = 335544355;
	/// <summary>
	/// Gbak: Cannot find table @1.
	/// </summary>
	public const int GbakTableMissing = 336330787;
	/// <summary>
	/// Gsec: Organization already specified.
	/// </summary>
	public const int GsecOrgSpecified = 336724003;
	/// <summary>
	/// Gbak: Cannot find column for BLOB.
	/// </summary>
	public const int GbakBlobColumnMissing = 336330788;
	/// <summary>
	/// Gsec: First name already specified.
	/// </summary>
	public const int GsecFirstNameSpecified = 336724004;
	/// <summary>
	/// Cannot disconnect database with open transactions (@1 active).
	/// </summary>
	public const int OpenTransaction = 335544357;
	/// <summary>
	/// Gbak: Gds_$create_blob failed.
	/// </summary>
	public const int GbakCreateBlobFailed = 336330789;
	/// <summary>
	/// Gsec: Middle name already specified.
	/// </summary>
	public const int GsecMiddleNameSpecified = 336724005;
	/// <summary>
	/// Message length error ( encountered @1, expected @2).
	/// </summary>
	public const int MessageLengthError = 335544358;
	/// <summary>
	/// Gbak: Gds_$put_segment failed.
	/// </summary>
	public const int GbakPutSegmentFailed = 336330790;
	/// <summary>
	/// Gsec: Last name already specified
	/// </summary>
	public const int GsecLastNameSpecified = 336724006;
	/// <summary>
	/// Expected record length
	/// </summary>
	public const int GbakRecordLengthExpexted = 336330791;
	/// <summary>
	/// Gsec: Invalid switch specified.
	/// </summary>
	public const int GsecInvalidSwitch = 336724008;
	/// <summary>
	/// Gbak: Wrong length record, expected @1 encountered @2.
	/// </summary>
	public const int GbakInvalidRecordLength = 336330792;
	/// <summary>
	/// Gbak: Expected data type.
	/// </summary>
	public const int GbakExpectedDataType = 336330793;
	/// <summary>
	/// Gsec: Ambiguous switch specified.
	/// </summary>
	public const int GsecAmbiguousSwitch = 336724009;
	/// <summary>
	/// Gbak: Failed in store_blr_gen_id.
	/// </summary>
	public const int GbakGenIdFailed = 336330794;
	/// <summary>
	/// Gsec: No operation specified for parameters.
	/// </summary>
	public const int GsecNoOpForParamsSpecified = 336724010;
	/// <summary>
	/// No transaction for request.
	/// </summary>
	public const int NoTransactionForRequest = 335544363;
	/// <summary>
	/// Gbak: Do not recognize record type @1.
	/// </summary>
	public const int GbakUnknownRecordType = 336330795;
	/// <summary>
	/// Gsec: No parameters allowed for this operation.
	/// </summary>
	public const int GsecParamsNotAllowed = 336724011;
	/// <summary>
	/// Request synchronization error.
	/// </summary>
	public const int RequestSyncError = 335544364;
	/// <summary>
	/// Gsec: Incompatible switches specified.
	/// </summary>
	public const int GsecIncompatibleSwitch = 336724012;
	/// <summary>
	/// Gbak: Expected backup version 1..8. Found @1.
	/// </summary>
	public const int GbakInvalidBackupVersion = 336330796;
	/// <summary>
	/// Request referenced an unavailable database.
	/// </summary>
	public const int WrongDbRequest = 335544365;
	/// <summary>
	/// Gbak: Expected backup description record.
	/// </summary>
	public const int GbakMissingBackupDesc = 336330797;
	/// <summary>
	/// String truncated.
	/// </summary>
	public const int GbakStringTruncated = 336330798;
	/// <summary>
	/// Warning - record could not be restored.
	/// </summary>
	public const int GbakCantRestoreRecord = 336330799;
	/// <summary>
	/// Gds_$send failed.
	/// </summary>
	public const int GbakSendFailed = 336330800;
	/// <summary>
	/// Attempted read of a new, open BLOB.
	/// </summary>
	public const int BlobNoRead = 335544369;
	/// <summary>
	/// No table name for data.
	/// </summary>
	public const int GbakNoTableName = 336330801;
	/// <summary>
	/// Attempted action on blob outside transaction.
	/// </summary>
	public const int BlobOutsideTransaction = 335544370;
	/// <summary>
	/// Gbak: Unexpected end of file on backup file.
	/// </summary>
	public const int GbakUnexpectedEof = 336330802;
	/// <summary>
	/// Database format @1 is too old to restore to.
	/// </summary>
	public const int GbakDbFormatTooOld = 336330803;
	/// <summary>
	/// Attempted reference to BLOB in unavailable database.
	/// </summary>
	public const int BlobWrongDb = 335544372;
	/// <summary>
	/// Array dimension for column @1 is invalid.
	/// </summary>
	public const int GbakInvalidArrayDimension = 336330804;
	/// <summary>
	/// Gbak: Expected XDR record length.
	/// </summary>
	public const int GbakXdrLengthExpected = 336330807;
	/// <summary>
	/// Table @1 was omitted from the transaction reserving list.
	/// </summary>
	public const int UnresolvedRelation = 335544376;
	/// <summary>
	/// Request includes a DSRI extension not supported in this implementation.
	/// </summary>
	public const int UnsupportedExtension = 335544377;
	/// <summary>
	/// Feature is not supported.
	/// </summary>
	public const int FeatureNotSupported = 335544378;
	/// <summary>
	/// @1.
	/// </summary>
	public const int RandomMessage = 335544382;
	/// <summary>
	/// Unrecoverable conflict with limbo transaction @1.
	/// </summary>
	public const int FatalConflict = 335544383;
	/// <summary>
	/// Gfix: Internal block exceeds maximum size.
	/// </summary>
	public const int GfixExceedMax = 335740991;
	/// <summary>
	/// Gfix: Corrupt pool.
	/// </summary>
	public const int GfixCorruptPool = 335740992;
	/// <summary>
	/// Gfix: Virtual memory exhausted.
	/// </summary>
	public const int GfixVirtualMemoryExhausted = 335740993;
	/// <summary>
	/// Gbak: Cannot open backup file @1.
	/// </summary>
	public const int GbakOpenBackupError = 336330817;
	/// <summary>
	/// Gfix: Bad pool id.
	/// </summary>
	public const int GfixBadPool = 335740994;
	/// <summary>
	/// Cannot open status and error output file @1.
	/// </summary>
	public const int GbakOpenError = 336330818;
	/// <summary>
	/// Gfix: Transaction state @1 not in valid range.
	/// </summary>
	public const int GfixTransactionNotValid = 335740995;
	/// <summary>
	/// Internal error.
	/// </summary>
	public const int InternalDbError = 335544392;
	/// <summary>
	/// Gsec: Invalid user name (maximum 31 bytes allowed).
	/// </summary>
	public const int GsecUsernameTooLong = 336724044;
	/// <summary>
	/// Warning - maximum 8 significant bytes of password used.
	/// </summary>
	public const int GsecPasswordTooLong = 336724045;
	/// <summary>
	/// Gsec: Database already specified.
	/// </summary>
	public const int GsecDbAlreadySpecified = 336724046;
	/// <summary>
	/// Gsec: Database administrator name already specified.
	/// </summary>
	public const int GsecDbAdminAlreadySpecified = 336724047;
	/// <summary>
	/// Gsec: Database administrator password already specified.
	/// </summary>
	public const int GsecDbAdminPasswordAlreadySpecified = 336724048;
	/// <summary>
	/// Gsec: SQL role name already specified.
	/// </summary>
	public const int GsecSqlRoleAlreadySpecified = 336724049;
	/// <summary>
	/// Gfix: Unexpected end of input.
	/// </summary>
	public const int GfixUnexpectedEoi = 335741012;
	/// <summary>
	/// Database handle not zero.
	/// </summary>
	public const int DbHandleNotZero = 335544407;
	/// <summary>
	/// Transaction handle not zero.
	/// </summary>
	public const int TransactionHandleNotZero = 335544408;
	/// <summary>
	/// Gfix: Failed to reconnect to a transaction in database @1.
	/// </summary>
	public const int GfixReconnectionFail = 335741018;
	/// <summary>
	/// Transaction in limbo.
	/// </summary>
	public const int TransactionInLimbo = 335544418;
	/// <summary>
	/// Transaction not in limbo.
	/// </summary>
	public const int TransactionNotInLimbo2 = 335544419;
	/// <summary>
	/// Transaction outstanding.
	/// </summary>
	public const int TransactionOutstanding = 335544420;
	/// <summary>
	/// Undefined message number.
	/// </summary>
	public const int BadMessageNumber = 335544428;
	/// <summary>
	/// Gfix: Transaction description item unknown.
	/// </summary>
	public const int GfixUnknownTransaction = 335741036;
	/// <summary>
	/// Gfix: "read_only" or "read_write" required.
	/// </summary>
	public const int GfixModeRequired = 335741038;
	/// <summary>
	/// Blocking signal has been received.
	/// </summary>
	public const int BlockingSignal = 335544431;
	/// <summary>
	/// Gfix: Positive or zero numeric value required.
	/// </summary>
	public const int GfixPositiveOrZeroNumRequired = 335741042;
	/// <summary>
	/// Database system cannot read argument @1.
	/// </summary>
	public const int NoArgRead = 335544442;
	/// <summary>
	/// Database system cannot write argument @1.
	/// </summary>
	public const int NoArgWrite = 335544443;
	/// <summary>
	/// @1.
	/// </summary>
	public const int MiscInterpreted = 335544450;
	/// <summary>
	/// Transaction @1 is @2.
	/// </summary>
	public const int TransactionState = 335544468;
	/// <summary>
	/// Invalid statement handle.
	/// </summary>
	public const int BadStatementHandle = 335544485;
	/// <summary>
	/// Gbak: Blocking factor parameter missing.
	/// </summary>
	public const int GbakMissingBlockFactorParam = 336330934;
	/// <summary>
	/// Gbak: Expected blocking factor, encountered "@1".
	/// </summary>
	public const int GbakInvalidBlockFactorParam = 336330935;
	/// <summary>
	/// Gbak: A blocking factor may not be used in conjunction with device CT.
	/// </summary>
	public const int GbakBlockFacSpecified = 336330936;
	/// <summary>
	/// SQL role @1 does not exist.
	/// </summary>
	public const int DynRoleDoesNotExist = 336068796;
	/// <summary>
	/// Gbak: User name parameter missing.
	/// </summary>
	public const int GbakMissingUsername = 336330940;
	/// <summary>
	/// Gbak: Password parameter missing.
	/// </summary>
	public const int GbakPasswordUsername = 336330941;
	/// <summary>
	/// User @1 has no grant admin option on SQL role @2.
	/// </summary>
	public const int DynNoGrantAdminOption = 336068797;
	/// <summary>
	/// Lock time-out on wait transaction.
	/// </summary>
	public const int LockTimeout = 335544510;
	/// <summary>
	/// User @1 is not a member of SQL role @2.
	/// </summary>
	public const int DynUserNotRoleMember = 336068798;
	/// <summary>
	/// @1 is not the owner of SQL role @2.
	/// </summary>
	public const int DynDeleteRoleFailed = 336068799;
	/// <summary>
	/// @1 is a SQL role and not a user.
	/// </summary>
	public const int DynGrantRoleToUser = 336068800;
	/// <summary>
	/// User name @1 could not be used for SQL role.
	/// </summary>
	public const int DynInvalidSqlRoleName = 336068801;
	/// <summary>
	/// SQL role @1 already exists.
	/// </summary>
	public const int DynDupSqlRole = 336068802;
	/// <summary>
	/// Keyword @1 can not be used as a SQL role name.
	/// </summary>
	public const int DynKeywordSpecForRole = 336068803;
	/// <summary>
	/// SQL roles are not supported in on older versions of the database.
	/// A backup and restore of the database is required.
	/// </summary>
	public const int DynRolesNotSupported = 336068804;
	/// <summary>
	/// Gbak: Missing parameter for the number of bytes to be skipped.
	/// </summary>
	public const int GbakMissingSkippedBytes = 336330952;
	/// <summary>
	/// Gbak: Expected number of bytes to be skipped, encountered "@1".
	/// </summary>
	public const int GbakInvalidSkippedBytes = 336330953;
	/// <summary>
	/// Zero length identifiers are not allowed.
	/// </summary>
	public const int DynZeroLengthId = 336068820;
	/// <summary>
	/// Gbak: Error on charset restore.
	/// </summary>
	public const int GbakRestoreCharsetError = 336330965;
	/// <summary>
	/// Gbak: Error on collation restore.
	/// </summary>
	public const int GbakRestoreCollationError = 336330967;
	/// <summary>
	/// Gbak: Unexpected I/O error while reading from backup file.
	/// </summary>
	public const int GbakReadError = 336330972;
	/// <summary>
	/// Gbak: Unexpected I/O error while writing to backup file.
	/// </summary>
	public const int GbakWriteError = 336330973;
	/// <summary>
	/// @1 cannot reference @2.
	/// </summary>
	public const int DynWrongGttScope = 336068840;
	/// <summary>
	/// Gbak: Could not drop database @1 (database might be in use).
	/// </summary>
	public const int GbakDbInUse = 336330985;
	/// <summary>
	/// Gbak: System memory exhausted.
	/// </summary>
	public const int GbakSysMemoryExhausted = 336330990;
	/// <summary>
	/// Invalid service handle.
	/// </summary>
	public const int BadServiceHandle = 335544559;
	/// <summary>
	/// Wrong version of service parameter block.
	/// </summary>
	public const int WrongServiceParamBlockVersion = 335544561;
	/// <summary>
	/// Unrecognized service parameter block.
	/// </summary>
	public const int UnrecognizedServiceParamBlockVersion = 335544562;
	/// <summary>
	/// Service @1 is not defined.
	/// </summary>
	public const int ServiceNotDef = 335544563;
	/// <summary>
	/// Feature '@1' is not supported in ODS @2.@3.
	/// </summary>
	public const int DynOdsNotSupportedFeature = 336068856;
	/// <summary>
	/// Gbak: SQL role restore failed.
	/// </summary>
	public const int GbakRestoreRoleFailed = 336331002;
	/// <summary>
	/// Gbak: SQL role parameter missing.
	/// </summary>
	public const int GbakRoleParamMissing = 336331005;
	/// <summary>
	/// Gbak: Page buffers parameter missing.
	/// </summary>
	public const int GbakPageBuffersMissing = 336331010;
	/// <summary>
	/// Gbak: Expected page buffers, encountered "@1".
	/// </summary>
	public const int GbakPageBuffersWrongParam = 336331011;
	/// <summary>
	/// Gbak: Page buffers is allowed only on restore or create.
	/// </summary>
	public const int GbakPageBuffersRestore = 336331012;
	/// <summary>
	/// Gbak: Size specification either missing or incorrect for file @1.
	/// </summary>
	public const int GbakInvalidSize = 336331014;
	/// <summary>
	/// Gbak: File @1 out of sequence.
	/// </summary>
	public const int GbakFileOutOfSequence = 336331015;
	/// <summary>
	/// Gbak: Can't join - one of the files missing.
	/// </summary>
	public const int GbakJoinFileMissing = 336331016;
	/// <summary>
	/// Gbak: Standard input is not supported when using join operation.
	/// </summary>
	public const int GbakStdinNotSupported = 336331017;
	/// <summary>
	/// Gbak: Standard output is not supported when using split operation.
	/// </summary>
	public const int GbakStdoutNotSupported = 336331018;
	/// <summary>
	/// Gbak: Backup file @1 might be corrupt.
	/// </summary>
	public const int GbakBackupCorrupt = 336331019;
	/// <summary>
	/// Gbak: Database file specification missing.
	/// </summary>
	public const int GbakMissingDbFileSpec = 336331020;
	/// <summary>
	/// Gbak: Can't write a header record to file @1.
	/// </summary>
	public const int GbakHeaderWriteFailed = 336331021;
	/// <summary>
	/// Gbak: Free disk space exhausted.
	/// </summary>
	public const int GbakDiskSpaceExhausted = 336331022;
	/// <summary>
	/// Gbak: File size given (@1) is less than minimum allowed (@2).
	/// </summary>
	public const int GbakSizeLessThanMinimum = 336331023;
	/// <summary>
	/// Gbak: Service name parameter missing.
	/// </summary>
	public const int GbakSvcNameMissing = 336331025;
	/// <summary>
	/// Gbak: Cannot restore over current database, must be SYSDBA or
	/// owner of the existing database.
	/// </summary>
	public const int GbakNotOwner = 336331026;
	/// <summary>
	/// Gbak: "read_only" or "read_write" required.
	/// </summary>
	public const int GbakModeRequired = 336331031;
	/// <summary>
	/// Gbak: Just data ignore all constraints etc.
	/// </summary>
	public const int GbakJustData = 336331033;
	/// <summary>
	/// Restoring data only ignoring foreign key, unique, not null & other constraints.
	/// </summary>
	public const int GbakDataOnly = 336331034;
	/// <summary>
	/// INDEX @1.
	/// </summary>
	public const int IndexName = 335544609;
	/// <summary>
	/// EXCEPTION @1.
	/// </summary>
	public const int ExceptionName = 335544610;
	/// <summary>
	/// COLUMN @1.
	/// </summary>
	public const int ColumnName = 335544611;
	/// <summary>
	/// Union not supported.
	/// </summary>
	public const int UnionNotSupported = 335544613;
	/// <summary>
	/// DSQL: Unsupported DSQL construct.
	/// </summary>
	public const int DsqlConstructNotSupported = 335544614;
	/// <summary>
	/// Illegal use of keyword VALUE.
	/// </summary>
	public const int DsqlIllegalValKeywordUsage = 335544623;
	/// <summary>
	/// TABLE @1.
	/// </summary>
	public const int TableName = 335544626;
	/// <summary>
	/// PROCEDURE @1.
	/// </summary>
	public const int ProcedureName = 335544627;
	/// <summary>
	/// Specified domain or source column @1 does not exist.
	/// </summary>
	public const int DsqlDomainNotFound = 335544641;
	/// <summary>
	/// Variable @1 conflicts with parameter in same procedure.
	/// </summary>
	public const int DsqlVarConflict = 335544656;
	/// <summary>
	/// Server version too old to support all CREATE DATABASE options.
	/// </summary>
	public const int ServerVersionTooOld = 335544666;
	/// <summary>
	/// Cannot delete.
	/// </summary>
	public const int CannotDelete = 335544673;
	/// <summary>
	/// Sort error.
	/// </summary>
	public const int SortError = 335544675;
	/// <summary>
	/// Service @1 does not have an associated executable.
	/// </summary>
	public const int ServiceNoExe = 335544703;
	/// <summary>
	/// Failed to locate host machine.
	/// </summary>
	public const int NetLookupError = 335544704;
	/// <summary>
	/// Undefined service @1/@2.
	/// </summary>
	public const int ServiceUnknown = 335544705;
	/// <summary>
	/// The specified name was not found in the hosts file or Domain Name Services.
	/// </summary>
	public const int HostUnknown = 335544706;
	/// <summary>
	/// Attempt to execute an unprepared dynamic SQL statement.
	/// </summary>
	public const int UnpreparedStatement = 335544711;
	/// <summary>
	/// Service is currently busy: @1.
	/// </summary>
	public const int ServiceInUse = 335544716;
	//335544731 tra_must_sweep skipped (no meaningful message)
	/// <summary>
	/// A fatal exception occurred during the execution of a user defined function.
	/// </summary>
	public const int UserDefinedFunctionException = 335544740;
	/// <summary>
	/// Connection lost to database.
	/// </summary>
	public const int LostDbConnection = 335544741;
	/// <summary>
	/// User cannot write to RDB$USER_PRIVILEGES.
	/// </summary>
	public const int NoWriteUserPrivileges = 335544742;
	/// <summary>
	/// A fatal exception occurred during the execution of a blob filter.
	/// </summary>
	public const int BlobFilterException = 335544767;
	/// <summary>
	/// Access violation. The code attempted to access a virtual address
	/// without privilege to do so.
	/// </summary>
	public const int AccessViolationException = 335544768;
	/// <summary>
	/// Datatype misalignment. The attempted to read or write a value that
	/// was not stored on a memory boundary.
	/// </summary>
	public const int DatatypeMisalignmentException = 335544769;
	/// <summary>
	/// Array bounds exceeded. The code attempted to access an array element
	/// that is out of bounds.
	/// </summary>
	public const int ArrayBoundsExceededException = 335544770;
	/// <summary>
	/// Float denormal operand. One of the floating-point operands is too small
	/// to represent a standard float value. 
	/// </summary>
	public const int FloatDenormalOperandException = 335544771;
	/// <summary>
	/// Floating-point divide by zero. The code attempted to divide a floating-point
	/// value by zero.
	/// </summary>
	public const int FloatDivideByZeroException = 335544772;
	/// <summary>
	/// Floating-point inexact result. The result of a floating-point operation cannot
	/// be represented as a decimal fraction .
	/// </summary>
	public const int FloatInexactResultException = 335544773;
	/// <summary>
	/// Floating-point invalid operand. An indeterminant error occurred during a
	/// floating-point operation.
	/// </summary>
	public const int FloatInvalidOperandException = 335544774;
	/// <summary>
	/// Floating-point overflow. The exponent of a floating-point operation is
	/// greater than the magnitude allowed.
	/// </summary>
	public const int FloatOverflowException = 335544775;
	/// <summary>
	/// Floating-point stack check. The stack overflowed or underflowed as the
	/// result of a floating-point operation.
	/// </summary>
	public const int FloatStackCheckException = 335544776;
	/// <summary>
	/// Floating-point underflow. The exponent of a floating-point operation is
	/// less than the magnitude allowed.
	/// </summary>
	public const int FloatUnderflowException = 335544777;
	/// <summary>
	/// Integer divide by zero. The code attempted to divide an integer value by
	/// an integer divisor of zero.
	/// </summary>
	public const int IntegerDivideByZeroException = 335544778;
	/// <summary>
	/// Integer overflow. The result of an integer operation caused the most
	/// significant bit of the result to carry.
	/// </summary>
	public const int IntegerOverflowException = 335544779;
	/// <summary>
	/// An unspecified exception occurred. Exception number @1.
	/// </summary>
	public const int UnknownException = 335544780;
	/// <summary>
	/// Stack overflow. The resource requirements of the runtime stack have
	/// exceeded the memory available to it.
	/// </summary>
	public const int StackOverflowException = 335544781;
	/// <summary>
	/// Segmentation Fault. The code attempted to access memory without privileges.
	/// </summary>
	public const int SegmentationFaultException = 335544782;
	/// <summary>
	/// Illegal Instruction. The Code attempted to perfrom an illegal operation.
	/// </summary>
	public const int IllegalInstructionException = 335544783;
	/// <summary>
	/// Bus Error. The Code caused a system bus error.
	/// </summary>
	public const int SystemBusException = 335544784;
	/// <summary>
	/// Floating Point Error. The Code caused an Arithmetic
	/// Exception or a floating point exception.
	/// </summary>
	public const int FloatingPointException = 335544785;
	/// <summary>
	/// Cannot delete rows from external files.
	/// </summary>
	public const int ExtFileDeleteError = 335544786;
	/// <summary>
	/// Cannot update rows in external files.
	/// </summary>
	public const int ExtFileModifyError = 335544787;
	/// <summary>
	/// Unable to perform operation. You must be either SYSDBA or owner of the database.
	/// </summary>
	public const int AdminTaskDenied = 335544788;
	/// <summary>
	/// Operation was cancelled.
	/// </summary>
	public const int OperationCancelled = 335544794;
	/// <summary>
	/// User name and password are required while attaching to the services manager.
	/// </summary>
	public const int ServiceNoUser = 335544797;
	/// <summary>
	/// Data type not supported for arithmetic.
	/// </summary>
	public const int DatypeNotSupported = 335544801;
	/// <summary>
	/// Database dialect not changed.
	/// </summary>
	public const int DialectNotChanged = 335544803;
	/// <summary>
	/// Unable to create database @1.
	/// </summary>
	public const int DatabaseCreateFailed = 335544804;
	/// <summary>
	/// Database dialect @1 is not a valid dialect.
	/// </summary>
	public const int InvalidDialectSpecified = 335544805;
	/// <summary>
	/// Valid database dialects are @1.
	/// </summary>
	public const int ValidDbDialects = 335544806;
	/// <summary>
	/// Passed client dialect @1 is not a valid dialect.
	/// </summary>
	public const int InvalidClientDialectSpecified = 335544811;
	/// <summary>
	/// Valid client dialects are @1.
	/// </summary>
	public const int ValidClientDialects = 335544812;
	/// <summary>
	/// Services functionality will be supported in a later version of the product.
	/// </summary>
	public const int ServiceNotSupported = 335544814;
	/// <summary>
	/// Unable to find savepoint with name @1 in transaction context.
	/// </summary>
	public const int SavepointNotFound = 335544820;
	/// <summary>
	/// Target shutdown mode is invalid for database "@1".
	/// </summary>
	public const int BadShutdownMode = 335544835;
	/// <summary>
	/// Cannot update.
	/// </summary>
	public const int NoUpdate = 335544840;
	/// <summary>
	/// @1.
	/// </summary>
	public const int StackTrace = 335544842;
	/// <summary>
	/// Context variable @1 is not found in namespace @2.
	/// </summary>
	public const int ContextVarNotFound = 335544843;
	/// <summary>
	/// Invalid namespace name @1 passed to @2.
	/// </summary>
	public const int ContextNamespaceInvalid = 335544844;
	/// <summary>
	/// Too many context variables.
	/// </summary>
	public const int ContextTooBig = 335544845;
	/// <summary>
	/// Invalid argument passed to @1.
	/// </summary>
	public const int ContextBadArgument = 335544846;
	/// <summary>
	/// BLR syntax error. Identifier @1... is too long.
	/// </summary>
	public const int BlrIdentifierTooLong = 335544847;
	/// <summary>
	/// Time precision exceeds allowed range (0-@1).
	/// </summary>
	public const int InvalidTimePrecision = 335544859;
	/// <summary>
	/// @1 cannot depend on @2.
	/// </summary>
	public const int MetWrongGttScope = 335544866;
	/// <summary>
	/// Procedure @1 is not selectable (it does not contain a SUSPEND statement).
	/// </summary>
	public const int IllegalProcedureType = 335544868;
	/// <summary>
	/// Datatype @1 is not supported for sorting operation.
	/// </summary>
	public const int InvalidSortDatatype = 335544869;
	/// <summary>
	/// COLLATION @1.
	/// </summary>
	public const int CollationName = 335544870;
	/// <summary>
	/// DOMAIN @1.
	/// </summary>
	public const int DomainName = 335544871;
	/// <summary>
	/// A multi database transaction cannot span more than @1 databases.
	/// </summary>
	public const int MaxDbPerTransactionAllowed = 335544874;
	/// <summary>
	/// Error while parsing procedure @1' s BLR.
	/// </summary>
	public const int BadProcBlr = 335544876;
	/// <summary>
	/// Index key too big.
	/// </summary>
	public const int IndexKeyTooBig = 335544877;
	/// <summary>
	/// DSQL: Too many values ( more than @1) in member list to match against.
	/// </summary>
	public const int DsqlTooManyValues = 336397211;
	/// <summary>
	/// DSQL: Feature is not supported in dialect @1.
	/// </summary>
	public const int DsqlUnsupportedFeatureDialect = 336397236;
	/// <summary>
	/// Internal gds software consistency check (@1).
	/// </summary>
	public const int BugCheck = 335544333;
	/// <summary>
	/// Database file appears corrupt (@1).
	/// </summary>
	public const int DbCorrupt = 335544335;
	/// <summary>
	/// I/O error for file "@2".
	/// </summary>
	public const int IoError = 335544344;
	/// <summary>
	/// Corrupt system table.
	/// </summary>
	public const int MetadataCorrupt = 335544346;
	/// <summary>
	/// Operating system directive @1 failed.
	/// </summary>
	public const int SysRequest = 335544373;
	//335544384 badblk skipped (no meaningful message)
	//335544385 invpoolcl skipped (no meaningful message)
	//335544387 relbadblk skipped (no meaningful message)
	/// <summary>
	/// Block size exceeds implementation restriction.
	/// </summary>
	public const int BlockTooBig = 335544388;
	/// <summary>
	/// Incompatible version of on-disk structure.
	/// </summary>
	public const int BadOnDiskStructureVersion = 335544394;
	//335544397 dirtypage skipped (no meaningful message)
	//335544398 waifortra skipped (no meaningful message)
	//335544399 doubleloc skipped (no meaningful message)
	//335544400 nodnotfnd skipped (no meaningful message)
	//335544401 dupnodfnd skipped (no meaningful message)
	//335544402 locnotmar skipped (no meaningful message)
	/// <summary>
	/// Database corrupted.
	/// </summary>
	public const int DatabaseCorrupt = 335544404;
	/// <summary>
	/// Checksum error on database page @1.
	/// </summary>
	public const int BadPage = 335544405;
	/// <summary>
	/// Index is broken.
	/// </summary>
	public const int BrokenIndex = 335544406;
	/// <summary>
	/// Transaction - request mismatch (synchronization error).
	/// </summary>
	public const int TransactionRequestMismatch = 335544409;
	/// <summary>
	/// Bad handle count.
	/// </summary>
	public const int BadHandleCount = 335544410;
	/// <summary>
	/// Wrong version of transaction parameter block.
	/// </summary>
	public const int WrongTransactionParamBlockVersion = 335544411;
	/// <summary>
	/// Unsupported BLR version (expected @1, encountered @2).
	/// </summary>
	public const int WrongBlrVersion = 335544412;
	/// <summary>
	/// Wrong version of database parameter block.
	/// </summary>
	public const int WrongDbParamBlockVersion = 335544413;
	/// <summary>
	/// Database corrupted.
	/// </summary>
	public const int BadDatabase = 335544415;
	//335544416 nodetach skipped (no meaningful message)
	//335544417 notremote skipped (no meaningful message)
	//335544422 dbfile skipped (no meaningful message)
	//335544423 orphan skipped (no meaningful message)
	/// <summary>
	/// Lock manager error.
	/// </summary>
	public const int LockManagerError = 335544432;
	/// <summary>
	/// SQL error code = @1.
	/// </summary>
	public const int SqlError = 335544436;
	/// <summary>
	/// Cache buffer for page @1 invalid.
	/// </summary>
	//335544448 bad_sec_info skipped (no meaningful message)
	//335544449 invalid_sec_info skipped (no meaningful message)
	public const int CacheBufferInvalid = 335544470;
	/// <summary>
	/// There is no index in table @1 with id @2.
	/// </summary>
	public const int IndexNotDefined = 335544471;
	/// <summary>
	/// Your user name and password are not defined.
	/// Ask your database administrator to set up a Firebird login.
	/// </summary>
	public const int LoginAndPasswordNotDefined = 335544472;
	/// <summary>
	/// Database @1 shutdown in progress.
	/// </summary>
	public const int ShutdownInProgress = 335544506;
	/// <summary>
	/// Database @1 shutdown.
	/// </summary>
	public const int DatabaseShutdown = 335544528;
	/// <summary>
	/// Database shutdown unsuccessful.
	/// </summary>
	public const int ShutdownFailed = 335544557;
	/// <summary>
	/// Dynamic SQL Error.
	/// </summary>
	public const int DsqlError = 335544569;
	/// <summary>
	/// Cannot attach to password database.
	/// </summary>
	public const int PasswordAttach = 335544653;
	/// <summary>
	/// Cannot start transaction for password database.
	/// </summary>
	public const int PasswordStartTransaction = 335544654;
	/// <summary>
	/// Stack size insufficent to execute current request.
	/// </summary>
	public const int StackLimitInsufficient = 335544717;
	/// <summary>
	/// Unable to complete network request to host "@1".
	/// </summary>
	public const int NetworkError = 335544721;
	/// <summary>
	/// Failed to establish a connection.
	/// </summary>
	public const int NetConnectError = 335544722;
	/// <summary>
	/// Error while listening for an incoming connection.
	/// </summary>
	public const int NetConnectListenError = 335544723;
	/// <summary>
	/// Failed to establish a secondary connection for event processing.
	/// </summary>
	public const int NetEventConnectError = 335544724;
	/// <summary>
	/// Error while listening for an incoming event connection request.
	/// </summary>
	public const int NetEventListenError = 335544725;
	/// <summary>
	/// Error reading data from the connection.
	/// </summary>
	public const int NetReadError = 335544726;
	/// <summary>
	/// Error writing data to the connection.
	/// </summary>
	public const int NetWriteError = 335544727;
	/// <summary>
	/// Access to databases on file servers is not supported.
	/// </summary>
	public const int UnsupportedNetworkDrive = 335544732;
	/// <summary>
	/// Error while trying to create file.
	/// </summary>
	public const int IoCreateError = 335544733;
	/// <summary>
	/// Error while trying to open file.
	/// </summary>
	public const int IoOpenError = 335544734;
	/// <summary>
	/// Error while trying to close file.
	/// </summary>
	public const int IoCloseError = 335544735;
	/// <summary>
	/// Error while trying to read from file.
	/// </summary>
	public const int IoReadError = 335544736;
	/// <summary>
	/// Error while trying to write to file.
	/// </summary>
	public const int IoWriteError = 335544737;
	/// <summary>
	/// Error while trying to delete file.
	/// </summary>
	public const int IoDeleteError = 335544738;
	/// <summary>
	/// Error while trying to access file.
	/// </summary>
	public const int IoAccessError = 335544739;
	/// <summary>
	/// Your login @1 is same as one of the SQL role name.
	/// Ask your database administrator to set up a valid Firebird login. 
	/// </summary>
	public const int LoginSameAsRoleName = 335544745;
	/// <summary>
	/// The file @1 is currently in use by another process. Try again later.
	/// </summary>
	public const int FileInUse = 335544791;
	/// <summary>
	/// Unexpected item in service parameter block, expected @1.
	/// </summary>
	public const int UnexpectedItemInServiceParamBlock = 335544795;
	/// <summary>
	/// Function @1 is in @2, which is not in a permitted directory for external functions.
	/// </summary>
	public const int ExternFunctionDirectoryError = 335544809;
	/// <summary>
	/// File exceeded maximum size of 2GB. Add another database file or use a 64 bit I/O
	/// version of Firebird.
	/// </summary>
	public const int Io32BitExceededError = 335544819;
	/// <summary>
	/// Access to @1 "@2" is denied by server administrator.
	/// </summary>
	public const int AccessDeniedByAdmin = 335544831;
	/// <summary>
	/// Cursor is not open.
	/// </summary>
	public const int CursorNotOpen = 335544834;
	/// <summary>
	/// Cursor is already open.
	/// </summary>
	public const int CursorAlreadyOpen = 335544841;
	/// <summary>
	/// Connection shutdown.
	/// Symbol: att_shutdown
	/// </summary>
	public const int ConnectionShutdown = 335544856;
	/// <summary>
	/// Login name too long (@1 characters, maximum allowed @2).
	/// </summary>
	public const int LoginTooLong = 335544882;
	/// <summary>
	/// Invalid database handle (no active connection).
	/// </summary>
	public const int BadDbHandle = 335544324;
	/// <summary>
	/// Unavailable database.
	/// </summary>
	public const int DbUnavailable = 335544375;
	/// <summary>
	/// Implementation limit exceeded.
	/// </summary>
	public const int ImplementationExceeded = 335544381;
	/// <summary>
	/// Too many requests
	/// Symbol: nopoolids
	/// </summary>
	public const int TooManyRequests = 335544386;
	/// <summary>
	/// Buffer exhausted.
	/// </summary>
	public const int BufferExhausted = 335544389;
	/// <summary>
	/// Buffer in use.
	/// </summary>
	public const int BufferInUse = 335544391;
	/// <summary>
	/// Request in use
	/// </summary>
	public const int RequestInUse = 335544393;
	/// <summary>
	/// No lock manager available.
	/// </summary>
	public const int NoLockManager = 335544424;
	/// <summary>
	/// Unable to allocate memory from operating system.
	/// </summary>
	public const int OperationSystemMemAllocationError = 335544430;
	/// <summary>
	/// Update conflicts with concurrent update.
	/// </summary>
	public const int UpdateConflict = 335544451;
	/// <summary>
	/// Object @1 is in use.
	/// </summary>
	public const int ObjectInUse = 335544453;
	/// <summary>
	/// Cannot attach active shadow file.
	/// </summary>
	public const int ShadowAccessed = 335544455;
	/// <summary>
	/// A file in manual shadow @1 is unavailable.
	/// </summary>
	public const int ShadowMissing = 335544460;
	/// <summary>
	/// Cannot add index, index root page is full.
	/// </summary>
	public const int IndexRootPageFull = 335544661;
	/// <summary>
	/// Sort error: not enough memory.
	/// </summary>
	public const int SortMemError = 335544676;
	/// <summary>
	/// Request depth exceeded (Recursive definition?).
	/// </summary>
	public const int RequestDepthExceeded = 335544683;
	/// <summary>
	/// Sort record size of @1 bytes is too big.
	/// </summary>
	public const int SortRecordSizeError = 335544758;
	/// <summary>
	/// Too many open handles to database.
	/// </summary>
	public const int TooManyHandles = 335544761;
	/// <summary>
	/// Cannot attach to services manager.
	/// </summary>
	public const int ServiceAttachError = 335544792;
	/// <summary>
	/// The service name was not specified.
	/// </summary>
	public const int ServiceNameMissing = 335544799;
	/// <summary>
	/// Unsupported field type specified in BETWEEN predicate.
	/// </summary>
	public const int OptimizerBetweenError = 335544813;
	/// <summary>
	/// Invalid argument in EXECUTE STATEMENT-cannot convert to string.
	/// </summary>
	public const int ExecSqlInvalidArg = 335544827;
	/// <summary>
	/// Wrong request type in EXECUTE STATEMENT '@1'.
	/// </summary>
	public const int ExecSqlInvalidRequest = 335544828;
	/// <summary>
	/// Variable type (position @1) in EXECUTE STATEMENT '@2' INTO does
	/// not match returned column type.
	/// </summary>
	public const int ExecSqlInvalidVariable = 335544829;
	/// <summary>
	/// Too many recursion levels of EXECUTE STATEMENT.
	/// </summary>
	public const int ExecSqlMaxCallExceeded = 335544830;
	/// <summary>
	/// Cannot change difference file name while database is in backup mode.
	/// </summary>
	public const int WrongBackupState = 335544832;
	/// <summary>
	/// Partner index segment no @1 has incompatible data type
	/// </summary>
	public const int PartnerIndexIncompatibleType = 335544852;
	/// <summary>
	/// Maximum BLOB size exceeded.
	/// </summary>
	public const int BlobTooBig = 335544857;
	/// <summary>
	/// Stream does not support record locking.
	/// </summary>
	public const int RecordLockNotSupported = 335544862;
	/// <summary>
	/// Cannot create foreign key constraint @1. Partner index does not exist or is inactive.
	/// </summary>
	public const int PartnerIndexNotFound = 335544863;
	/// <summary>
	/// Transactions count exceeded. Perform backup and restore to make database
	/// operable again.
	/// </summary>
	public const int TransactionNumExceeded = 335544864;
	/// <summary>
	/// Column has been unexpectedly deleted.
	/// </summary>
	public const int FieldDisappeared = 335544865;
	/// <summary>
	/// Concurrent transaction number is @1.
	/// </summary>
	public const int ConcurrentTransaction = 335544878;
	/// <summary>
	/// Maximum user count exceeded. Contact your database administrator
	/// </summary>
	public const int MaxUsersExceeded = 335544744;
	/// <summary>
	/// Drop database completed with errors.
	/// </summary>
	public const int DropDatabaseWithErrors = 335544667;
	/// <summary>
	/// Record from transaction @1 is stuck in limbo.
	/// </summary>
	public const int RecordInLimbo = 335544459;
	/// <summary>
	/// Deadlock occured.
	/// </summary>
	public const int Deadlock = 335544336;
	/// <summary>
	/// File @1 is not a valid database.
	/// </summary>
	public const int BadDbFormat = 335544323;
	/// <summary>
	/// Connection rejected by remote interface.
	/// </summary>
	public const int ConnectionReject = 335544421;
	/// <summary>
	/// Secondary server attachments cannot validate databases.
	/// </summary>
	public const int CantValidate = 335544461;
	/// <summary>
	/// Secondary server attachments cannot start logging.
	/// </summary>
	public const int CantStartLogging = 335544464;
	/// <summary>
	/// Bad parameters on attach or create database.
	/// Symbol: bad_dpb_content
	/// </summary>
	public const int BadDatabaseCreationParams = 335544325;
	/// <summary>
	/// Database detach completed with errors.
	/// </summary>
	public const int BadDetach = 335544441;
	/// <summary>
	/// Connection lost to pipe server.
	/// </summary>
	public const int ConnectionLost = 335544648;
	/// <summary>
	/// No rollback performed.
	/// </summary>
	public const int NoRollback = 335544447;
	/// <summary>
	/// Firebird error.
	/// </summary>
	public const int FirebirdError = 335544689;
}
