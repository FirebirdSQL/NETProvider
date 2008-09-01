/* This provider don't work without the membership provider */

SET SQL DIALECT 3;

CREATE TABLE PROFILES (
    PKID                  CHAR(16) CHARACTER SET OCTETS NOT NULL,
    PROPERTYNAMES         BLOB SUB_TYPE TEXT SEGMENT SIZE 80 CHARACTER SET UTF8,
    PROPERTYVALUESSTRING  BLOB SUB_TYPE TEXT SEGMENT SIZE 80 CHARACTER SET UTF8,
    PROPERTYVALUESBINARY  BLOB SUB_TYPE BINARY SEGMENT SIZE 80,
    LASTUPDATEDDATE       TIMESTAMP,
    LASTACTIVITYDATE      TIMESTAMP,
    ISUSERANONYMOUS       BOOL,
    APPLICATIONNAME       VARCHAR(100) CHARACTER SET UTF8
);

ALTER TABLE PROFILES ADD CONSTRAINT PK_PROFILES PRIMARY KEY (PKID);

SET TERM ^ ;

CREATE OR ALTER PROCEDURE PROFILES_DELETEINACTPROFILES (
    APPLICATIONNAME VARCHAR(100) CHARACTER SET UTF8,
    PROFILEAUTHOPTIONS INTEGER,
    INACTIVESINCEDATE TIMESTAMP)
AS
begin
 DELETE FROM  Profiles  WHERE APPLICATIONNAME = :applicationname AND (LastActivityDate <= :InactiveSinceDate)
                        AND (
                                (:ProfileAuthOptions = 2)
                             OR (:ProfileAuthOptions = 0 AND IsUserAnonymous = 1)
                             OR (:ProfileAuthOptions = 1 AND IsUserAnonymous = 0)
                            ) ;
end^

CREATE OR ALTER PROCEDURE PROFILES_DELETEPROFILE (
    APPLICATIONNAME VARCHAR(100) CHARACTER SET UTF8,
    USERNAME VARCHAR(100) CHARACTER SET UTF8)
AS
DECLARE VARIABLE USERID CHAR(16) CHARACTER SET OCTETS;
begin
  userid = null;
  select pkid from users where applicationname = :applicationname and username = :username into :userid;
  if (userid is null) then
   userid = :username;
  delete from profiles where pkid = :userid;
end^

CREATE OR ALTER PROCEDURE PROFILES_GETNBOFINACTPROFILES (
    APPLICATIONNAME VARCHAR(100) CHARACTER SET UTF8,
    PROFILEAUTHOPTIONS INTEGER,
    INACTIVESINCEDATE TIMESTAMP)
RETURNS (
    NB INTEGER)
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
end^

CREATE OR ALTER PROCEDURE PROFILES_GETPROFILES (
    APPLICATIONNAME VARCHAR(100) CHARACTER SET UTF8,
    PROFILEAUTHOPTIONS INTEGER,
    USERNAMETOMATCH VARCHAR(100) CHARACTER SET UTF8,
    INACTIVESINCEDATE TIMESTAMP,
    PAGEINDEX INTEGER,
    PAGESIZE INTEGER)
RETURNS (
    USERNAME VARCHAR(100) CHARACTER SET UTF8,
    ISANONYMOUS SMALLINT,
    LASTACTIVITYDATE TIMESTAMP,
    LASTUPDATEDDATE TIMESTAMP)
AS
DECLARE VARIABLE PKID CHAR(16) CHARACTER SET OCTETS;
DECLARE VARIABLE UPPERUSERNAME VARCHAR(100) CHARACTER SET UTF8;
DECLARE VARIABLE PAGELOWERBOUND INTEGER;
DECLARE VARIABLE PAGEUPPERBOUND INTEGER;
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
END^

CREATE OR ALTER PROCEDURE PROFILES_GETCOUNTPROFILES (
    APPLICATIONNAME VARCHAR(100) CHARACTER SET UTF8,
    PROFILEAUTHOPTIONS INTEGER,
    USERNAMETOMATCH VARCHAR(100) CHARACTER SET UTF8,
    INACTIVESINCEDATE TIMESTAMP)
RETURNS (
    TOTALRECORDS INTEGER)
AS
DECLARE VARIABLE PKID CHAR(16) CHARACTER SET OCTETS;
DECLARE VARIABLE UPPERUSERNAME VARCHAR(100) CHARACTER SET UTF8;
DECLARE VARIABLE ISANONYMOUS SMALLINT;
DECLARE VARIABLE USERNAME VARCHAR(100) CHARACTER SET UTF8;
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
END^

CREATE OR ALTER PROCEDURE PROFILES_GETPROPERTIES (
    APPLICATIONNAME VARCHAR(100) CHARACTER SET UTF8,
    USERNAME VARCHAR(100) CHARACTER SET UTF8,
    CURRENTTIMEUTC TIMESTAMP)
RETURNS (
    PROPERTYNAMES BLOB SUB_TYPE 1 SEGMENT SIZE 80,
    PROPERTYVALUESSTRING BLOB SUB_TYPE 1 SEGMENT SIZE 80,
    PROPERTYVALUESBINARY BLOB SUB_TYPE 0 SEGMENT SIZE 80)
AS
DECLARE VARIABLE USERID CHAR(16) CHARACTER SET OCTETS;
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
end^

CREATE OR ALTER PROCEDURE PROFILES_SETPROPERTIES (
    APPLICATIONNAME VARCHAR(100) CHARACTER SET UTF8,
    PROPERTYNAMES BLOB SUB_TYPE 1 SEGMENT SIZE 80,
    PROPERTYVALUESSTRING BLOB SUB_TYPE 1 SEGMENT SIZE 80,
    PROPERTYVALUESBINARY BLOB SUB_TYPE 0 SEGMENT SIZE 80,
    USERNAME VARCHAR(100) CHARACTER SET UTF8,
    ISUSERANONYMOUS SMALLINT,
    CURRENTTIMEUTC TIMESTAMP)
RETURNS (
    ERRORCODE INTEGER)
AS
DECLARE VARIABLE USERID CHAR(16) CHARACTER SET OCTETS;
begin
  userid = null;
  errorcode = 0;
  userid = null;
  select pkid from users where applicationname = :applicationname and upperusername = upper(:username) into :userid;
  if (userid is null) then
   userid = :username;
  if (userid is not null) then
  begin
   IF (EXISTS( SELECT * FROM   Profiles WHERE  profiles.pkid = :UserId)) then
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
end^

SET TERM ; ^

GRANT SELECT,DELETE ON PROFILES TO PROCEDURE PROFILES_DELETEINACTPROFILES;
GRANT SELECT ON USERS TO PROCEDURE PROFILES_DELETEPROFILE;
GRANT SELECT,DELETE ON PROFILES TO PROCEDURE PROFILES_DELETEPROFILE;
GRANT SELECT ON PROFILES TO PROCEDURE PROFILES_GETNBOFINACTPROFILES;
GRANT SELECT ON PROFILES TO PROCEDURE PROFILES_GETPROFILES;
GRANT SELECT ON USERS TO PROCEDURE PROFILES_GETPROFILES;
GRANT SELECT ON USERS TO PROCEDURE PROFILES_GETPROPERTIES;
GRANT SELECT,UPDATE ON PROFILES TO PROCEDURE PROFILES_GETPROPERTIES;
GRANT SELECT ON USERS TO PROCEDURE PROFILES_SETPROPERTIES;
GRANT SELECT,INSERT,UPDATE ON PROFILES TO PROCEDURE PROFILES_SETPROPERTIES;
GRANT SELECT,DELETE ON PROFILES TO PROCEDURE MEMBERSHIP_DELETEUSER;