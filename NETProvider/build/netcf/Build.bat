set CSC="csc.exe"
set NETCFLIBS="C:\Program Files (x86)\Microsoft.NET\SDK\CompactFramework\v2.0\WindowsCE"

rem ********** PATHS **********

set BUILD_DIR=.
set SOURCE_DIR=..\..\source
set COMMON_SOURCES=%SOURCE_DIR%\FirebirdSql\Data\Common
set GDS_SOURCES=%SOURCE_DIR%\FirebirdSql\Data\Client\Managed
set PROVIDER_SOURCES=%SOURCE_DIR%\FirebirdSql\Data\FirebirdClient
set SCHEMA_SOURCES=%SOURCE_DIR%\FirebirdSql\Data\Schema
set SERVICES_SOURCES=%SOURCE_DIR%\FirebirdSql\Data\Services

rem ********** DEFINES **********

set DEFINES=-define:DEBUG -define:NETCF

rem ********** RESOURCES **********

set RESOURCES=-resource:%SOURCE_DIR%\FirebirdSql\Data\Resources\isc_error_msg.resources,FirebirdSql.Resources.isc_error_msg.resources

rem ********** References **********

set REFERENCES=/r:%NETCFLIBS%\mscorlib.dll /r:%NETCFLIBS%\system.dll /r:%NETCFLIBS%\system.data.dll /r:%NETCFLIBS%\system.drawing.dll /r:%NETCFLIBS%\system.xml.dll /r:%NETCFLIBS%\system.windows.forms.dll

rem ********** Build **********

copy %SOURCE_DIR%\FirebirdSql\Data\Properties\*.snk .

%CSC% /noconfig /nostdlib /target:library /out:%BUILD_DIR%\FirebirdSql.Data.FirebirdClient.dll %REFERENCES% %DEFINES% %COMMON_RESOURCES% /recurse:%COMMON_SOURCES%\*.cs /recurse:%GDS_SOURCES%\*.cs /recurse:%PROVIDER_SOURCES%\*.cs /recurse:%SCHEMA_SOURCES%\*.cs /recurse:%SERVICES_SOURCES%\*.cs