Firebird BDP (Borland Data Provider)
====================================

This project is supported by:
-----------------------------

	Sean Leyne ( Broadview Software )


Developement list
-----------------

You can subscribe to the developement list at:

	http://lists.sourceforge.net/lists/listinfo/firebird-net-provider


You can access to the lastest developement sources through Git, see:

	https://sourceforge.net/p/firebird/NETProvider/ci/master/tree/


Methods Missing for BDP
-----------------------
FbSQLSchemaCreate:
 - CreateObject
 - GetDDL
 - ReadSQLTypeMapping
 - WriteSQLTypeMapping

FbMetaData:
 - GetObjectList
 - GetExtendedMetaData

ISQLExtendedMetaData:
 - GetRelatedObjects
 - GetForeignKeys

Of the methods listed above, ReadSQLTypeMapping, WriteSQLTypeMapping,
GetExtendedMetaData, GetRelatedObjects, and GetForeignKeys are new
for the version of BDP present in Delphi 2006.  None of these methods
appear to be critical to using the bulk of BDD; including the
capabilities provided through the IDE.


Reporting Bugs
--------------

You can report bugs using two ways:

1. Sending it to the developement list.
2. If you have a Sourceforge ID you can send it using the Bugs section of the Firebird Project web page 
(category .Net Provider):



	http://sourceforge.net/tracker/?group_id=9028&atid=109028


Building and Installation
-------------------------

1. Build instructions

* You need the Microsoft .NET Framework 1.1.

* You need the Borland Data provider (Ships with Borland Delphi .NET and C# Builder).
	- Copy Borland.Data.Common.dll to .\source\FirebirdSql.Data.Bdp\Bdp
		- File is in C:\Program Files\Common Files\Borland Shared\BDS\Shared Assemblies\4.0 by default
	- Copy Borland.Data.Provider.dll to .\source\FirebirdSQL.Data.Bdp\Bdp
		- File is in C:\Program Files\Common Files\Borland Shared\BDS\Shared Assemblies\4.0 by default

* You will need NAnt (nant.sourceforge.net)

	- The nant build file for the BDP is called FirebirdBdp.build

2. Installation instructions.

You will need to install the Firebird BDP assembly in the GAC and 
modify the Borland Data Provider configuration files.

* Add the following lines to BdpDataSources.xml:

<provider name="Firebird" connectionStringType="FirebirdSql.Data.Bdp.FbConnectionString,  FirebirdSql.Data.Bdp, Version=1.0.1.0, Culture=neutral, PublicKeyToken=c7d0a028dd9e545b">
  <objectTypes>
    <objectType>Tables</objectType>
    <objectType>Procedures</objectType>
    <objectType>Views</objectType>
  </objectTypes>
</provider>

* Add the following lines to bdpConnections.xml to configure a permanent connection:

  <BdpConnectionString xsi:type="FbConnectionString">
    <Name>FBConn1</Name>
    <VendorClient>fbclient.dll</VendorClient>
    <Database>localhost/3050:employee.fdb</Database>
    <UserName>sysdba</UserName>
    <Password>masterkey</Password>
    <Assembly>FirebirdSql.Data.Bdp,Version=1.0.1.0,Culture=neutral,PublicKeyToken=c7d0a028dd9e545b</Assembly>
  </BdpConnectionString>

