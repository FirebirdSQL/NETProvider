Visual Studio 2005 DDEX Provider for Firebird
------ ------ ---- ---- -------- --- --------

The DDEX Provider for Firebird provides integration of FirebirdClient 
into Visual Studio 2005.

In order to use it FirebirdClient should be iun the GAC and registrered in the machine.config file,
and the information of the FirebirdDDEXProvider.reg file should be added to the
Windows registry.

To register the provider in the machine.config file add this:

<configuration>
    <configSections>
        ...
        <section name="firebirdsql.data.firebirdclient" type="System.Data.Common.DbProviderConfigurationHandler, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
        ...
    <configSections>

    <system.data>
        <DbProviderFactories>
		...
		<add name="FirebirdClient Data Provider" invariant="FirebirdSql.Data.FirebirdClient" description=".Net Framework Data Provider for Firebird" type="FirebirdSql.Data.FirebirdClient.FirebirdClientFactory, FirebirdSql.Data.FirebirdClient, Version=2.0.0.0, Culture=neutral, PublicKeyToken=3750abcc3150b00c" />
		...
        </DbProviderFactories>
	</system.data>
	....
</configuration>	

The FirebirdDDEXProvider.reg should be modified to set the correct CodeBase and Path for the 
DDEX provider files (by replace the %Path% in the file with the correct paths).

It's possible to install the DDEX provider in package less mode by using the 
FirebirdDDEXProviderPackageLess.reg file, that way the Visual Studio SDK shouldn't be needed.

To use the DDEX Provider:

	- Visual Studio 2005
	- Visual Studio 2005 SDK (http://msdn.microsoft.com/vstudio/extend/) IMPORTANT: This is not the .NET Framework SDK.

IMPORTANT: The DDEX provider is package based that means the Visual Studio 2005 SDK is needed and that it may not work with the Express Editions. The Visual Studio 2005 SDK is NOT the .NET Framework SDK , so make sure you have it correctly installed.

( If you want to build the sources you will need to have C# and C++ installed. )