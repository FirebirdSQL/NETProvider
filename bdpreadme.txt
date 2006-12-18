Firebird BDP (Borland Data Provider)
======== ===  =====================

This project is supported by:
---- ------- -- --------- ---

	Sean Leyne ( Broadview Software )


Developement list
-----------------

You can subscribe to the developement list at:

	http://lists.sourceforge.net/lists/listinfo/firebird-net-provider


You can access to the lastest developement sources through CVS, see:

	http://sourceforge.net/cvs/?group_id=9028


Reporting Bugs
--------------

Yo can report bugs using two ways:

1. Sending it to the developement list.
2. If you have a Sourceforge ID you can send it using the Bugs section of the Firebird Project web page 
(category .Net Provider):


	http://sourceforge.net/tracker/?group_id=9028&atid=109028


1. Build instructions

* You need the Microsoft .NET Framework 1.1.

* You need the Borland Data provider (Ships with Borland Delphi .NET and C# Builder).

* You will need NAnt (nant.sourceforge.net)

	- The nant build file for the BDP is called FirebirdBdp.build

2. Installation instructions.

You will need to install the Firebird BDP assembly in the GAC and 
modify the Borland Data Provider configuration files.

* Add the following lines to BdpDataSources.xml:

	<provider name="Firebird" connectionStringType="FirebirdSql.Data.Bdp.FbConnectionString, FirebirdSql.Data.Bdp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=c7d0a028dd9e545b">
		<objectTypes>
			<objectType>Tables</objectType>
			<objectType>Procedures</objectType>
			<objectType>Views</objectType>
		</objectTypes>
	</provider>

* Add the following lines to bdpConnections.xml to configure a permanent connection:

	<BdpConnectionString xsi:type="FbConnectionString">
		<Name>FbConn1</Name>
		<Database>localhost/3050:employee.fdb</Database>
		<UserName>sysdba</UserName>
		<Password>masterkey</Password>
		<Assembly>FirebirdSql.Data.Bdp,Version=1.0.0.0,Culture=neutral,PublicKeyToken=c7d0a028dd9e545b</Assembly>
	</BdpConnectionString>

