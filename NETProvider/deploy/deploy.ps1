param([Parameter(Mandatory=$True)]$Version)

$wix = 'I:\devel\bin\wix-binaries'
$nuget = 'I:\devel\bin\NuGet\nuget.exe'
$baseDir = Split-Path -Parent (Split-Path -parent $MyInvocation.MyCommand.Definition)

rm -Recurse -Force $baseDir\deploy\out\*

7z a -mx=9 $baseDir\deploy\out\FirebirdSql.Data.FirebirdClient-$Version-NET45.7z $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\Release_45\FirebirdSql.Data.FirebirdClient.dll $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\Release_45\FirebirdSql.Data.FirebirdClient.pdb
7z a -mx=9 $baseDir\deploy\out\FirebirdSql.Data.FirebirdClient-$Version-NET40.7z $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\Release_40\FirebirdSql.Data.FirebirdClient.dll $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\Release_40\FirebirdSql.Data.FirebirdClient.pdb
7z a -mx=9 $baseDir\deploy\out\FirebirdSql.Data.FirebirdClient-$Version-MONO_LINUX.7z $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\Release_Mono_w_Linux\FirebirdSql.Data.FirebirdClient.dll $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\Release_Mono_w_Linux\FirebirdSql.Data.FirebirdClient.pdb 

7z a -mx=9 $baseDir\deploy\out\EntityFramework.Firebird-$Version-NET45.7z $baseDir\src\EntityFramework.Firebird\bin\Release_45\EntityFramework.Firebird.dll $baseDir\src\EntityFramework.Firebird\bin\Release_45\EntityFramework.Firebird.pdb
7z a -mx=9 $baseDir\deploy\out\EntityFramework.Firebird-$Version-NET40.7z $baseDir\src\EntityFramework.Firebird\bin\Release_40\EntityFramework.Firebird.dll $baseDir\src\EntityFramework.Firebird\bin\Release_40\EntityFramework.Firebird.pdb

& $nuget pack $baseDir\nuget\FirebirdSql.Data.FirebirdClient\FirebirdSql.Data.FirebirdClient.nuspec -Version $Version
& $nuget pack $baseDir\nuget\EntityFramework.Firebird\EntityFramework.Firebird.nuspec -Version $Version

& $wix\candle.exe "-dBaseDir=$baseDir" "-dVersion=$Version" -ext $wix\WixUtilExtension.dll -out $baseDir\deploy\out\Installer.wixobj $baseDir\installer\Installer.wxs
& $wix\light.exe -ext $wix\WixUIExtension.dll -ext $wix\WixUtilExtension.dll -out $baseDir\deploy\out\FirebirdSql.Data.FirebirdClient-$Version.msi $baseDir\deploy\out\Installer.wixobj
rm $baseDir\deploy\out\Installer.wixobj
rm $baseDir\deploy\out\FirebirdSql.Data.FirebirdClient-$Version.wixpdb