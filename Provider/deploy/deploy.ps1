param([Parameter(Mandatory=$True)]$Version)

$wix = 'I:\devel\bin\wix-binaries'
$nuget = 'I:\devel\bin\NuGet\nuget.exe'
$baseDir = Split-Path -Parent (Split-Path -parent $PSCommandPath)
$outDir = "$baseDir\deploy\out"

if (Test-Path $outDir) {
	rm -Recurse -Force $outDir\*
}
else {
	mkdir $outDir | Out-Null
}

7z a -mx=9 $outDir\FirebirdSql.Data.FirebirdClient-$Version-NET45.7z $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\Release_45\FirebirdSql.Data.FirebirdClient.dll $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\Release_45\FirebirdSql.Data.FirebirdClient.pdb
7z a -mx=9 $outDir\FirebirdSql.Data.FirebirdClient-$Version-NET40.7z $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\Release_40\FirebirdSql.Data.FirebirdClient.dll $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\Release_40\FirebirdSql.Data.FirebirdClient.pdb 

7z a -mx=9 $outDir\EntityFramework.Firebird-$Version-NET45.7z $baseDir\src\EntityFramework.Firebird\bin\Release_45\EntityFramework.Firebird.dll $baseDir\src\EntityFramework.Firebird\bin\Release_45\EntityFramework.Firebird.pdb
7z a -mx=9 $outDir\EntityFramework.Firebird-$Version-NET40.7z $baseDir\src\EntityFramework.Firebird\bin\Release_40\EntityFramework.Firebird.dll $baseDir\src\EntityFramework.Firebird\bin\Release_40\EntityFramework.Firebird.pdb

& $nuget pack $baseDir\nuget\FirebirdSql.Data.FirebirdClient\FirebirdSql.Data.FirebirdClient.nuspec -OutputDirectory $outDir -Version $Version
& $nuget pack $baseDir\nuget\EntityFramework.Firebird\EntityFramework.Firebird.nuspec -OutputDirectory $outDir -Version $Version

& $wix\candle.exe "-dBaseDir=$baseDir" "-dVersion=$Version" -ext $wix\WixUtilExtension.dll -out $outDir\Installer.wixobj $baseDir\installer\Installer.wxs
& $wix\light.exe -ext $wix\WixUIExtension.dll -ext $wix\WixUtilExtension.dll -out $outDir\FirebirdSql.Data.FirebirdClient-$Version.msi $outDir\Installer.wixobj
rm $outDir\Installer.wixobj
rm $outDir\FirebirdSql.Data.FirebirdClient-$Version.wixpdb