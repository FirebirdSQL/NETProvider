Visual Studio 2005/2008/2010 DDEX Provider for Firebird
=======================================================

This project is supported by:
=============================

  Sean Leyne (Broadview Software)

The DDEX Provider for Firebird provides integration of FirebirdClient into Visual Studio. In order to use Firebird Client in Visual Studio, you have to perform these steps.

0. Preinstallation steps.
-------------------------
Copy files from this archive to some place (must be accessible for VS). 

1. Install FirebirdClient into the GAC.
---------------------------------------
You can use gacutil utility to do this or to check whether it's correctly installed. The gacutil show you also the signature for assembly, that will be used later.

2. Modify machine.config file.
------------------------------
Modify it like this (for 64bit systems you have to edit "32bit version" of this file, because Visual Studio is 32bit, but there's no problem with editing the "64bit version" too):

<configuration>
  <configSections>
    ...
    <section name="firebirdsql.data.firebirdclient" type="System.Data.Common.DbProviderConfigurationHandler, System.Data, Version=%RuntimeVersion%, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
    ...
  <configSections>
...
  <system.data>
    <DbProviderFactories>
      ...
      <add name="FirebirdClient Data Provider" invariant="FirebirdSql.Data.FirebirdClient" description=".Net Framework Data Provider for Firebird" type="FirebirdSql.Data.FirebirdClient.FirebirdClientFactory, FirebirdSql.Data.FirebirdClient, Version=%Version%, Culture=%Culture%, PublicKeyToken=%PublicKeyToken%" />
      ...
    </DbProviderFactories>
  </system.data>
</configuration>

And substitute (these informations you can find using gacutil):
  - %RuntimeVersion% with either 2.0.0.0 (.NET 2.0, .NET 3.0, .NET 3.5) or 4.0.0.0 (.NET 4). 
  - %Version% with the version of the provider assembly that you have in the GAC.
  - %Culture% with the culture of the provider assembly that you have in the GAC.
  - %PublicKeyToken% with the PublicKeyToken of the provider assembly that you have in the GAC.

Note:
  Notice, that in configSections there isn't signature of FirebirdClient but the signature of assembly from framework.

3. Import registry file.
------------------------
There's a couple of *.reg files in installation. There are files for 32bit and for 64bit system, so select appropriate version for your system. There are also files in "withSDK" directory. These can be used for Visual Studio with VS SDK installed. The files not in this directory are for systems without Visual Studio SDK (it's *not* the .NET FW SDK!) and it's probably the best choice for a lot of developers. The selected registry file needs be modified to set the correct paths. To do this, substitute %Path% in the file with path for the DDEX files where you copyied them in step 0 (remember to backslash the backslash character).

IMPORTANT: The DDEX provider didn't work with Express editions of Visual Studio.

(If you want to build the sources you will need to have C# and C++ installed and VS SDK.)