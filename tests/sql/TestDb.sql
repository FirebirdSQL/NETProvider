SET SQL DIALECT 3;

SET NAMES ISO8859_1;



SET TERM ^ ; 



/******************************************************************************/
/***                           Stored Procedures                            ***/
/******************************************************************************/

CREATE PROCEDURE GETVARCHARFIELD (
    ID INTEGER)
RETURNS (
    VARCHAR_FIELD VARCHAR(100))
AS
BEGIN
  EXIT;
END^



SET TERM ; ^


/******************************************************************************/
/***                                 Tables                                 ***/
/******************************************************************************/

CREATE TABLE TEST_TABLE_01 (
    INT_FIELD        INTEGER DEFAULT 0 NOT NULL,
    CHAR_FIELD       CHAR(30),
    VARCHAR_FIELD    VARCHAR(100),
    BIGINT_FIELD     BIGINT,
    SMALLINT_FIELD   SMALLINT,
    DOUBLE_FIELD     DOUBLE PRECISION,
    NUMERIC_FIELD    NUMERIC(15,2),
    DECIMAL_FIELD    DECIMAL(15,2),
    DATE_FIELD       DATE,
    TIME_FIELD       TIME,
    TIMESTAMP_FIELD  TIMESTAMP,
    CLOB_FIELD       BLOB SUB_TYPE 1 SEGMENT SIZE 80,
    BLOB_FIELD       BLOB SUB_TYPE 0 SEGMENT SIZE 80
);





/******************************************************************************/
/***                              Primary Keys                              ***/
/******************************************************************************/

ALTER TABLE TEST_TABLE_01 ADD CONSTRAINT PK_TEST_TABLE_01 PRIMARY KEY (INT_FIELD);


/******************************************************************************/
/***                           Stored Procedures                            ***/
/******************************************************************************/


SET TERM ^ ;

ALTER PROCEDURE GETVARCHARFIELD (
    ID INTEGER)
RETURNS (
    VARCHAR_FIELD VARCHAR(100))
AS
begin
    for select varchar_field from test_table_01 where int_field = :id into :varchar_field
    do
        suspend;
end
^


SET TERM ; ^