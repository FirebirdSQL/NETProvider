param(
	[Parameter(Mandatory=$True)]$Configuration,
	[Parameter(Mandatory=$True)]$Version)

$baseDir = Split-Path -parent $PSCommandPath
$outDir = "$baseDir\out"

if ($env:build_nuget) {
	$nuget = $env:build_nuget
}
else {
	$nuget = 'I:\devel\bin\NuGet\nuget.exe'
}
if ($env:build_wix) {
	$wix = $env:build_wix
}
else {
	$wix = 'I:\devel\bin\wix-binaries'
}

function Clean() {
	if (Test-Path $outDir) {
	rm -Recurse -Force $outDir\*
	}
	else {
		mkdir $outDir | Out-Null
	}
}

function Build() {
	$solutionFile = "$baseDir\src\NETProvider.sln"
	& $nuget restore $solutionFile
	msbuild /t:Clean,Build /p:Configuration=$Configuration $solutionFile /v:m /m
}

function Pack() {
	7z a -mx=9 $outDir\FirebirdSql.Data.FirebirdClient-$Version-net452.7z $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\Release\net452\FirebirdSql.Data.FirebirdClient.dll $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\Release\net452\FirebirdSql.Data.FirebirdClient.pdb
	7z a -mx=9 $outDir\FirebirdSql.Data.FirebirdClient-$Version-netstandard1.6.7z $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\Release\netstandard1.6\FirebirdSql.Data.FirebirdClient.dll $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\Release\netstandard1.6\FirebirdSql.Data.FirebirdClient.pdb
	7z a -mx=9 $outDir\FirebirdSql.Data.FirebirdClient-$Version-netstandard2.0.7z $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\Release\netstandard2.0\FirebirdSql.Data.FirebirdClient.dll $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\Release\netstandard2.0\FirebirdSql.Data.FirebirdClient.pdb

	7z a -mx=9 $outDir\EntityFramework.Firebird-$Version-net452.7z $baseDir\src\EntityFramework.Firebird\bin\Release\net452\EntityFramework.Firebird.dll $baseDir\src\EntityFramework.Firebird\bin\Release\net452\EntityFramework.Firebird.pdb

	7z a -mx=9 $outDir\FirebirdSql.EntityFrameworkCore.Firebird-$Version-net452.7z $baseDir\src\FirebirdSql.EntityFrameworkCore.Firebird\bin\Release\netstandard2.0\FirebirdSql.EntityFrameworkCore.Firebird.dll $baseDir\src\FirebirdSql.EntityFrameworkCore.Firebird\bin\Release\netstandard2.0\FirebirdSql.EntityFrameworkCore.Firebird.pdb
}

function NuGet() {
	& $nuget pack $baseDir\nuget\FirebirdSql.Data.FirebirdClient\FirebirdSql.Data.FirebirdClient.nuspec -OutputDirectory $outDir -Version $Version
	& $nuget pack $baseDir\nuget\EntityFramework.Firebird\EntityFramework.Firebird.nuspec -OutputDirectory $outDir -Version $Version
}

function WiX() {
	& $wix\candle.exe "-dBaseDir=$baseDir" "-dVersion=$Version" -ext $wix\WixUtilExtension.dll -out $outDir\Installer.wixobj $baseDir\installer\Installer.wxs
	& $wix\light.exe -ext $wix\WixUIExtension.dll -ext $wix\WixUtilExtension.dll -out $outDir\FirebirdSql.Data.FirebirdClient-$Version.msi $outDir\Installer.wixobj
	rm $outDir\Installer.wixobj
	rm $outDir\FirebirdSql.Data.FirebirdClient-$Version.wixpdb
}

Clean
Build
Pack
NuGet
WiX
