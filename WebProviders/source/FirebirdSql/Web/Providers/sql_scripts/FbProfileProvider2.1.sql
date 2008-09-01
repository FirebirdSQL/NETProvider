SET SQL DIALECT 3;

SET NAMES UTF8;

/******************************************************************************/
/*                                   Tables                                   */
/******************************************************************************/



CREATE TABLE PROFILES (
    PKID                  WP_CHAR16_OCTETS,
    PROPERTYNAMES         WP_BLOB_TEXT,
    PROPERTYVALUESSTRING  WP_BLOB_TEXT,
    PROPERTYVALUESBINARY  WP_BLOB_BINARY,
    LASTUPDATEDDATE       WP_TIMESTAMP,
    LASTACTIVITYDATE      WP_TIMESTAMP,
    ISUSERANONYMOUS       WP_BOOL,
    APPLICATIONNAME       WP_VARCHAR100
);



/******************************************************************************/
/*                                Primary Keys                                */
/******************************************************************************/

ALTER TABLE PROFILES ADD CONSTRAINT PK_PROFILES PRIMARY KEY (PKID);


/******************************************************************************/
/*                                Foreign Keys                                */
/******************************************************************************/

ALTER TABLE PROFILES ADD CONSTRAINT FK_PROFILES_USERS FOREIGN KEY (PKID) REFERENCES USERS (PKID)
  USING INDEX FK_PROFILES;


/******************************************************************************/
/*                             Stored Procedures                              */
/******************************************************************************/


SET TERM ^ ;

CREATE OR ALTER PROCEDURE PROFILES_DELETEINACTPROFILES (
    APPLICATIONNAME TYPE OF WP_VARCHAR100,
    PROFILEAUTHOPTIONS TYPE OF WP_INTEGER,
    INACTIVESINCEDATE TYPE OF WP_TIMESTAMP)
AS
begin
 DELETE FROM  Profiles  WHERE APPLICATIONNAME = :applicationname AND (LastActivityDate <= :InactiveSinceDate)
                        AND (
                                (:ProfileAuthOptions = 2)
                             OR (:ProfileAuthOptions = 0 AND IsUserAnonymous = 1)
                             OR (:ProfileAuthOptions = 1 AND IsUserAnonymous = 0)
                            ) ;
end
^

CREATE OR ALTER PROCEDURE PROFILES_DELETEPROFILE (
    APPLICATIONNAME VARCHAR(100),
    USERNAME VARCHAR(100))
AS
DECLARE VARIABLE USERID WP_CHAR16_OCTETS;
begin
  userid = null;
  select pkid from users where applicationname = :applicationname and username = :username into :userid;
  if (userid is null) then
   userid = :username;
  delete from profiles where pkid = :userid;
end
^

CREATE OR ALTER PROCEDURE PROFILES_GETCOUNTPROFILES (
    APPLICATIONNAME TYPE OF WP_VARCHAR100,
    PROFILEAUTHOPTIONS TYPE OF WP_INTEGER,
    USERNAMETOMATCH TYPE OF WP_VARCHAR100,
    INACTIVESINCEDATE TYPE OF WP_TIMESTAMP)
RETURNS (
    TOTALRECORDS TYPE OF WP_INTEGER)
AS
DECLARE VARIABLE PKID WP_CHAR16_OCTETS;
DECLARE VARIABLE UPPERUSERNAME TYPE OF WP_VARCHAR100;
DECLARE VARIABLE ISANONYMOUS TYPE OF WP_BOOL;
DECLARE VARIABLE USERNAME TYPE OF WP_VARCHAR100;
BEGIN
  totalrecords = 0;

  IF (usernametomatch IS NOT NULL) THEN
   FOR SELECT pkid,isuseranonymous FROM Profiles
       WHERE (applicationname = :applicationname)
       AND(:inactivesincedate IS NULL OR LastActivityDate <= :InactiveSinceDate)
       AND ((:ProfileAuthOptions = 2) OR (:ProfileAuthOptions = 0 AND ISUSERANONYMOUS = 1)
       OR (:ProfileAuthOptions = 1 AND ISUSERANONYMOUS = 0))
   INTO :pkid , :isanonymous
   DO
   BEGIN
     username = NULL;
     IF (:isanonymous = 1) THEN
        username = pkid;
     ELSE
     BEGIN
      SELECT userName, upperusername FROM users WHERE pkid = :pkid INTO :username, :upperusername;
     END
     IF (upperusername LIKE :usernametomatch) THEN
      totalrecords = totalrecords + 1;
   END
  ELSE
   SELECT COUNT(1) FROM Profiles
                   WHERE (applicationname = :applicationname)
                   AND (:inactivesincedate IS NULL OR LastActivityDate <= :InactiveSinceDate)
                   AND ((:ProfileAuthOptions = 2) OR (:ProfileAuthOptions = 0 AND ISUSERANONYMOUS = 1)
                   OR (:ProfileAuthOptions = 1 AND ISUSERANONYMOUS = 0))
   INTO :totalrecords;
  SUSPEND;
END
^

CREATE OR ALTER PROCEDURE PROFILES_GETNBOFINACTPROFILES (
    APPLICATIONNAME TYPE OF WP_VARCHAR100,
    PROFILEAUTHOPTIONS TYPE OF WP_INTEGER,
    INACTIVESINCEDATE TYPE OF WP_TIMESTAMP)
RETURNS (
    NB TYPE OF WP_INTEGER)
AS
begin
 nb = 0;
  SELECT  COUNT(*) FROM    Profiles
  WHERE Applicationname = :applicationname
        AND (LastActivityDate <= :InactiveSinceDate)
        AND ((:ProfileAuthOptions = 2)
              OR (:ProfileAuthOptions = 0 AND IsUserAnonymous = 1)
              OR (:ProfileAuthOptions = 1 AND IsUserAnonymous = 0))
  INTO :nb;
  suspend;
end
^

CREATE OR ALTER PROCEDURE PROFILES_GETPROFILES (
    APPLICATIONNAME TYPE OF WP_VARCHAR100,
    PROFILEAUTHOPTIONS TYPE OF WP_INTEGER,
    USERNAMETOMATCH TYPE OF WP_VARCHAR100,
    INACTIVESINCEDATE TYPE OF WP_TIMESTAMP,
    PAGEINDEX TYPE OF WP_INTEGER,
    PAGESIZE TYPE OF WP_INTEGER)
RETURNS (
    USERNAME TYPE OF WP_VARCHAR100,
    ISANONYMOUS TYPE OF WP_BOOL,
    LASTACTIVITYDATE TYPE OF WP_TIMESTAMP,
    LASTUPDATEDDATE TYPE OF WP_TIMESTAMP)
AS
DECLARE VARIABLE PKID TYPE OF WP_CHAR16_OCTETS;
DECLARE VARIABLE UPPERUSERNAME TYPE OF WP_VARCHAR100;
DECLARE VARIABLE PAGELOWERBOUND TYPE OF WP_INTEGER;
DECLARE VARIABLE PAGEUPPERBOUND TYPE OF WP_INTEGER;
BEGIN
  pagelowerbound = pagesize * pageindex;
  PageUpperBound = pagesize;

  FOR SELECT  FIRST(:pageupperbound) SKIP(:pagelowerbound) pkid,isuseranonymous,lastactivitydate,lastupdateddate FROM Profiles
  WHERE (applicationname = :applicationname)
  AND (:inactivesincedate IS NULL OR LastActivityDate <= :InactiveSinceDate)
  AND ((:ProfileAuthOptions = 2) OR (:ProfileAuthOptions = 0 AND ISUSERANONYMOUS = 1)
  OR (:ProfileAuthOptions = 1 AND ISUSERANONYMOUS = 0))
  INTO :pkid,:IsAnonymous,:lastactivitydate,:lastupdateddate
  DO
  BEGIN
   username = NULL;
   IF (:IsAnonymous = 1) THEN
    username = pkid;
   ELSE
   BEGIN
    SELECT username, upperusername FROM users WHERE pkid = :pkid INTO :username,:upperusername;
   END
   IF (usernametomatch IS NOT NULL) THEN
   BEGIN
    IF (upperusername LIKE :usernametomatch) THEN
     SUSPEND;
   END
   ELSE
    SUSPEND;
  END
END
^

CREATE OR ALTER PROCEDURE PROFILES_GETPROPERTIES (
    APPLICATIONNAME TYPE OF WP_VARCHAR100,
    USERNAME TYPE OF WP_VARCHAR100,
    CURRENTTIMEUTC TYPE OF WP_TIMESTAMP)
RETURNS (
    PROPERTYNAMES TYPE OF WP_BLOB_TEXT,
    PROPERTYVALUESSTRING TYPE OF WP_BLOB_TEXT,
    PROPERTYVALUESBINARY TYPE OF WP_BLOB_BINARY)
AS
DECLARE VARIABLE USERID TYPE OF WP_CHAR16_OCTETS;
begin
  userid = null;
  PropertyNames = null;
  PropertyValuesString = null;
  PropertyValuesBinary = null;                                          
  userid = null;
  select pkid from users where applicationname = :applicationname and upperusername = upper(:username) into :userid;
  if (userid is null) then
   userid = :username;
  if (userid is not null) then
  begin
   select first(1) PropertyNames, PropertyValuesString, PropertyValuesBinary from profiles
   where pkid = :userid into :propertynames,:propertyvaluesstring,:propertyvaluesbinary;
   if (propertynames is not null) then
   begin
    suspend;
    UPDATE profiles set profiles.lastactivitydate = :currenttimeutc where profiles.pkid = :userid;
   end
  end
end
^

CREATE OR ALTER PROCEDURE PROFILES_SETPROPERTIES (
    APPLICATIONNAME TYPE OF WP_VARCHAR100,
    PROPERTYNAMES TYPE OF WP_BLOB_TEXT,
    PROPERTYVALUESSTRING TYPE OF WP_BLOB_TEXT,
    PROPERTYVALUESBINARY TYPE OF WP_BLOB_BINARY,
    USERNAME TYPE OF WP_VARCHAR100,
    ISUSERANONYMOUS TYPE OF WP_BOOL,
    CURRENTTIMEUTC TYPE OF WP_TIMESTAMP)
RETURNS (
    ERRORCODE TYPE OF WP_INTEGER)
AS
DECLARE VARIABLE USERID TYPE OF WP_CHAR16_OCTETS;
begin
  userid = null;
  errorcode = 0;
  userid = null;
  select pkid from users where applicationname = :applicationname and upperusername = upper(:username) into :userid;
  if (userid is null) then
   userid = :username;
  if (userid is not null) then
  begin
   IF (EXISTS( SELECT 1 FROM   Profiles WHERE  profiles.pkid = :UserId)) then
        UPDATE Profiles
        SET    PropertyNames=:PropertyNames, PropertyValuesString = :PropertyValuesString,
               PropertyValuesBinary = :PropertyValuesBinary, LastUpdatedDate=:CurrentTimeUtc , LastActivityDate=:currenttimeutc
        WHERE  pkid = :UserId;
   ELSE
        INSERT INTO Profiles(pkid, PropertyNames, PropertyValuesString, PropertyValuesBinary, LastUpdatedDate,ISUSERANONYMOUS,APPLICATIONNAME,LastActivityDate)
             VALUES (:UserId, :PropertyNames, :PropertyValuesString, :PropertyValuesBinary, :CurrentTimeUtc,:isuseranonymous,:applicationname,:CurrentTimeUtc);
  end
  else
   errorcode = 1;
  suspend;
end
^


SET TERM ; ^


/******************************************************************************/
/*                                 Privileges                                 */
/******************************************************************************/

/* Privileges of procedures */

GRANT SELECT, DELETE ON PROFILES TO PROCEDURE MEMBERSHIP_DELETEUSER;
GRANT SELECT, DELETE ON PROFILES TO PROCEDURE PROFILES_DELETEINACTPROFILES;
GRANT SELECT, DELETE ON PROFILES TO PROCEDURE PROFILES_DELETEPROFILE;
GRANT SELECT ON USERS TO PROCEDURE PROFILES_DELETEPROFILE;
GRANT SELECT ON PROFILES TO PROCEDURE PROFILES_GETNBOFINACTPROFILES;
GRANT SELECT ON PROFILES TO PROCEDURE PROFILES_GETPROFILES;
GRANT SELECT ON USERS TO PROCEDURE PROFILES_GETPROFILES;
GRANT SELECT, UPDATE ON PROFILES TO PROCEDURE PROFILES_GETPROPERTIES;
GRANT SELECT ON USERS TO PROCEDURE PROFILES_GETPROPERTIES;
GRANT SELECT, INSERT, UPDATE ON PROFILES TO PROCEDURE PROFILES_SETPROPERTIES;
GRANT SELECT ON USERS TO PROCEDURE PROFILES_SETPROPERTIES;
