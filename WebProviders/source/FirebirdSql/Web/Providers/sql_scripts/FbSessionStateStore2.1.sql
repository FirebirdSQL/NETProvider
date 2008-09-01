SET SQL DIALECT 3;

SET NAMES UTF8;



/******************************************************************************/
/*                                   Tables                                   */
/******************************************************************************/



CREATE TABLE SESSIONS (
    SESSIONID        wp_varchar80_octets,
    APPLICATIONNAME  wp_varchar100  NOT NULL /* VARCHAR100 = VARCHAR(100) */,
    CREATED          WP_TIMESTAMP /* TIMESTAMP_NOTREQ = TIMESTAMP */,
    EXPIRES          WP_TIMESTAMP /* TIMESTAMP_NOTREQ = TIMESTAMP */,
    LOCKDATE         WP_TIMESTAMP /* TIMESTAMP_NOTREQ = TIMESTAMP */,
    LOCKID           WP_INTEGER /* INTEGER_NOTREQ = INTEGER */,
    TIMEOUT          WP_INTEGER /* INTEGER_NOTREQ = INTEGER */,
    LOCKED           WP_BOOL /* BOOL = SMALLINT NOT NULL CHECK (value=1 or value=0 or value is null) */,
    SESSIONITEMS     WP_BLOB_TEXT /* BLOB4096_TEXT = BLOB SUB_TYPE 1 SEGMENT SIZE 4096 */,
    FLAGS            WP_INTEGER /* INTEGER_NOTREQ = INTEGER */
);




/******************************************************************************/
/*                                  Indices                                   */
/******************************************************************************/

CREATE UNIQUE INDEX SESSIONS_ID_APPLICATIONNAME ON SESSIONS (SESSIONID, APPLICATIONNAME);


/******************************************************************************/
/*                                 Privileges                                 */
/******************************************************************************/

