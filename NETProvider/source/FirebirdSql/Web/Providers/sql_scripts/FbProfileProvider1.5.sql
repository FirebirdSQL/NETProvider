/* This provider don't work without the membership provider */

SET SQL DIALECT 3;

SET NAMES NONE;

CREATE DOMAIN BOOL AS
SMALLINT
NOT NULL
CHECK (value=1 or value=0 or value is null);

CREATE TABLE PROFILES (
    PKID                  CHAR(36) CHARACTER SET OCTETS NOT NULL,
    PROPERTYNAMES         BLOB SUB_TYPE 1 SEGMENT SIZE 80 CHARACTER SET UNICODE_FSS,
    PROPERTYVALUESSTRING  BLOB SUB_TYPE 1 SEGMENT SIZE 80 CHARACTER SET UNICODE_FSS,
    PROPERTYVALUESBINARY  BLOB SUB_TYPE 0 SEGMENT SIZE 80,
    LASTUPDATEDDATE       TIMESTAMP,
    LASTACTIVITYDATE      TIMESTAMP,
    ISUSERANONYMOUS       BOOL,
    APPLICATIONNAME       VARCHAR(252) CHARACTER SET NONE
);

ALTER TABLE PROFILES ADD CONSTRAINT PK_PROFILES PRIMARY KEY (PKID);

SET TERM ^ ;

CREATE PROCEDURE PROFILES_DELETEINACTPROFILES (
    APPLICATIONNAME VARCHAR(252) CHARACTER SET NONE,
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

CREATE PROCEDURE PROFILES_DELETEPROFILE (
    APPLICATIONNAME VARCHAR(255) CHARACTER SET NONE,
    USERNAME VARCHAR(252) CHARACTER SET NONE)
AS
declare variable userid char(36) character set octets;
begin
  userid = null;
  select pkid from users where applicationname = :applicationname and username = :username into :userid;
  if (userid is null) then
   userid = :username;
  delete from profiles where pkid = :userid;
end^

CREATE PROCEDURE PROFILES_GETNBOFINACTPROFILES (
    APPLICATIONNAME VARCHAR(252) CHARACTER SET OCTETS,
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

CREATE PROCEDURE PROFILES_GETPROFILES (
    APPLICATIONNAME VARCHAR(252) CHARACTER SET NONE,
    PROFILEAUTHOPTIONS INTEGER,
    USERNAMETOMATCH VARCHAR(252) CHARACTER SET NONE,
    INACTIVESINCEDATE TIMESTAMP,
    PAGEINDEX INTEGER,
    PAGESIZE INTEGER)
RETURNS (
    USERNAME VARCHAR(252) CHARACTER SET NONE,
    ISANONYMOUS SMALLINT,
    LASTACTIVITYDATE TIMESTAMP,
    LASTUPDATEDDATE TIMESTAMP)
AS
declare variable pkid char(36) character set octets;
declare variable spkid char(16) character set octets;
declare variable upperusername varchar(252) character set NONE;
declare variable pagelowerbound integer;
declare variable pageupperbound integer;
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
    spkid = CAST(:pkid AS CHAR(16) character set octets);
    SELECT username, upperusername FROM users WHERE pkid = :spkid INTO :username,:upperusername;
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

CREATE PROCEDURE PROFILES_GETCOUNTPROFILES (
    APPLICATIONNAME VARCHAR(252) CHARACTER SET NONE,
    PROFILEAUTHOPTIONS INTEGER,
    USERNAMETOMATCH VARCHAR(252) CHARACTER SET NONE,
    INACTIVESINCEDATE TIMESTAMP)
RETURNS (
    TOTALRECORDS INTEGER)
AS
declare variable pkid char(36) character set octets;
declare variable spkid char(16) character set octets;
declare variable upperusername varchar(252) character set NONE;
declare variable isanonymous smallint;
declare variable username varchar(252) character set NONE;
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
      spkid = CAST(:pkid AS CHAR(16) character set octets);
      SELECT userName, upperusername FROM users WHERE pkid = :spkid INTO :username, :upperusername;
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

CREATE PROCEDURE PROFILES_GETPROPERTIES (
    APPLICATIONNAME VARCHAR(252) CHARACTER SET NONE,
    USERNAME VARCHAR(252) CHARACTER SET NONE,
    CURRENTTIMEUTC TIMESTAMP)
RETURNS (
    PROPERTYNAMES BLOB SUB_TYPE 1 SEGMENT SIZE 80,
    PROPERTYVALUESSTRING BLOB SUB_TYPE 1 SEGMENT SIZE 80,
    PROPERTYVALUESBINARY BLOB SUB_TYPE 0 SEGMENT SIZE 80)
AS
declare variable userid char(36) character set octets;
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

CREATE PROCEDURE PROFILES_SETPROPERTIES (
    APPLICATIONNAME VARCHAR(252) CHARACTER SET NONE,
    PROPERTYNAMES BLOB SUB_TYPE 1 SEGMENT SIZE 80,
    PROPERTYVALUESSTRING BLOB SUB_TYPE 1 SEGMENT SIZE 80,
    PROPERTYVALUESBINARY BLOB SUB_TYPE 0 SEGMENT SIZE 80,
    USERNAME VARCHAR(252) CHARACTER SET NONE,
    ISUSERANONYMOUS SMALLINT,
    CURRENTTIMEUTC TIMESTAMP)
RETURNS (
    ERRORCODE INTEGER)
AS
declare variable userid char(36) character set octets;
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
GRANT EXECUTE ON PROCEDURE PROFILES_DELETEINACTPROFILES TO SYSDBA;
GRANT SELECT ON USERS TO PROCEDURE PROFILES_DELETEPROFILE;
GRANT SELECT,DELETE ON PROFILES TO PROCEDURE PROFILES_DELETEPROFILE;
GRANT EXECUTE ON PROCEDURE PROFILES_DELETEPROFILE TO SYSDBA;
GRANT SELECT ON PROFILES TO PROCEDURE PROFILES_GETNBOFINACTPROFILES;
GRANT EXECUTE ON PROCEDURE PROFILES_GETNBOFINACTPROFILES TO SYSDBA;
GRANT SELECT ON PROFILES TO PROCEDURE PROFILES_GETPROFILES;
GRANT SELECT ON USERS TO PROCEDURE PROFILES_GETPROFILES;
GRANT EXECUTE ON PROCEDURE PROFILES_GETPROFILES TO SYSDBA;
GRANT SELECT ON USERS TO PROCEDURE PROFILES_GETPROPERTIES;
GRANT SELECT,UPDATE ON PROFILES TO PROCEDURE PROFILES_GETPROPERTIES;
GRANT EXECUTE ON PROCEDURE PROFILES_GETPROPERTIES TO SYSDBA;
GRANT SELECT ON USERS TO PROCEDURE PROFILES_SETPROPERTIES;
GRANT SELECT,INSERT,UPDATE ON PROFILES TO PROCEDURE PROFILES_SETPROPERTIES;
GRANT EXECUTE ON PROCEDURE PROFILES_SETPROPERTIES TO SYSDBA;
GRANT SELECT,DELETE ON PROFILES TO PROCEDURE MEMBERSHIP_DELETEUSER;