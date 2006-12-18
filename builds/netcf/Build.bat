set NETCFLIBS="C:\Archivos de programa\Microsoft Visual Studio 8\SDK\v2.0\CompactFramework\WindowsCE"

rem Paths

set BUILD_DIR=.
set SOURCE_DIR=..\..\source
set COMMON_SOURCES=%SOURCE_DIR%\FirebirdSql.Data.Common
set GDS_SOURCES=%SOURCE_DIR%\FirebirdSql.Data.Gds
set PROVIDER_SOURCES=%SOURCE_DIR%\FirebirdSql.Data.Firebird

rem DEFINES

set DEFINES=-define:DEBUG -define:NETCF -define:SINGLE_DLL

rem RESOURCES

set COMMON_RESOURCES=-resource:%COMMON_SOURCES%\Resources\isc_error_msg.resources,FirebirdSql.Data.Common.Resources.isc_error_msg.resources

rem References

set REFERENCES=/r:%NETCFLIBS%\mscorlib.dll /r:%NETCFLIBS%\system.dll /r:%NETCFLIBS%\system.data.dll /r:%NETCFLIBS%\system.drawing.dll /r:%NETCFLIBS%\system.xml.dll /r:%NETCFLIBS%\system.windows.forms.dll

rem Build

copy %SOURCE_DIR%\FirebirdSql.Data.Firebird\*.snk .

csc.exe /noconfig /nostdlib /target:library /out:%BUILD_DIR%\FirebirdSql.Data.Firebird.dll %REFERENCES% %DEFINES% %COMMON_RESOURCES% /recurse:%COMMON_SOURCES%\*.cs /recurse:%GDS_SOURCES%\*.cs /recurse:%PROVIDER_SOURCES%\*.cs