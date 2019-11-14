param(
	[Parameter(Mandatory=$True)]$Configuration)

$ErrorActionPreference = 'Stop'

$baseDir = Split-Path -Parent $PSCommandPath
$outDir = "$baseDir\out"
$version = ''

if ($env:build_wix) {
	$wix = $env:build_wix
}
else {
	$wix = 'I:\devel\bin\wix-binaries'
}

function Clean() {
	if (Test-Path $outDir) {
		rm -Force -Recurse $outDir
	}
	mkdir $outDir | Out-Null
}

function Build() {
	$solutionFile = "$baseDir\src\NETProvider.sln"
	dotnet restore $solutionFile
	dotnet msbuild /t:'Clean,Build' /p:Configuration=$Configuration $solutionFile /v:m /m
	$script:version = (Get-Item $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\$Configuration\net452\FirebirdSql.Data.FirebirdClient.dll).VersionInfo.ProductVersion -replace '(\d+)\.(\d+)\.(\d+)(-[a-z0-9]+)?(.*)','$1.$2.$3$4'
}

function Pack() {
	7z a -mx=9 -bsp0 $outDir\FirebirdSql.Data.FirebirdClient-$version-net452.7z $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\$Configuration\net452\FirebirdSql.Data.FirebirdClient.dll $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\$Configuration\net452\FirebirdSql.Data.FirebirdClient.pdb
	7z a -mx=9 -bsp0 $outDir\FirebirdSql.Data.FirebirdClient-$version-netstandard2.0.7z $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\$Configuration\netstandard2.0\FirebirdSql.Data.FirebirdClient.dll $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\$Configuration\netstandard2.0\FirebirdSql.Data.FirebirdClient.pdb

	7z a -mx=9 -bsp0 $outDir\EntityFramework.Firebird-$version-net452.7z $baseDir\src\EntityFramework.Firebird\bin\$Configuration\net452\EntityFramework.Firebird.dll $baseDir\src\EntityFramework.Firebird\bin\$Configuration\net452\EntityFramework.Firebird.pdb
	7z a -mx=9 -bsp0 $outDir\EntityFramework.Firebird-$version-netstandard2.1.7z $baseDir\src\EntityFramework.Firebird\bin\$Configuration\netstandard2.1\EntityFramework.Firebird.dll $baseDir\src\EntityFramework.Firebird\bin\$Configuration\netstandard2.1\EntityFramework.Firebird.pdb

	7z a -mx=9 -bsp0 $outDir\FirebirdSql.EntityFrameworkCore.Firebird-$version-netstandard2.0.7z $baseDir\src\FirebirdSql.EntityFrameworkCore.Firebird\bin\$Configuration\netstandard2.0\FirebirdSql.EntityFrameworkCore.Firebird.dll $baseDir\src\FirebirdSql.EntityFrameworkCore.Firebird\bin\$Configuration\netstandard2.0\FirebirdSql.EntityFrameworkCore.Firebird.pdb
}

function NuGets() {
	cp $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\$Configuration\FirebirdSql.Data.FirebirdClient.$version.nupkg $outDir
	cp $baseDir\src\EntityFramework.Firebird\bin\$Configuration\EntityFramework.Firebird.$version.nupkg $outDir
	cp $baseDir\src\FirebirdSql.EntityFrameworkCore.Firebird\bin\$Configuration\FirebirdSql.EntityFrameworkCore.Firebird.$version.nupkg $outDir

	cp $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\$Configuration\FirebirdSql.Data.FirebirdClient.$version.snupkg $outDir
	cp $baseDir\src\EntityFramework.Firebird\bin\$Configuration\EntityFramework.Firebird.$version.snupkg $outDir
	cp $baseDir\src\FirebirdSql.EntityFrameworkCore.Firebird\bin\$Configuration\FirebirdSql.EntityFrameworkCore.Firebird.$version.snupkg $outDir
}

function WiX() {
	$wixVersion = $version -replace '(.+?)(-[a-z0-9]+)?','$1'
	& $wix\candle.exe "-dBaseDir=$baseDir" "-dVersion=$wixVersion" "-dConfiguration=$Configuration" -ext $wix\WixUtilExtension.dll -out $outDir\Installer.wixobj $baseDir\installer\Installer.wxs
	& $wix\light.exe -ext $wix\WixUIExtension.dll -ext $wix\WixUtilExtension.dll -out $outDir\FirebirdSql.Data.FirebirdClient-$version.msi $outDir\Installer.wixobj
	rm $outDir\Installer.wixobj
	rm $outDir\FirebirdSql.Data.FirebirdClient-$version.wixpdb
}

Clean
Build
Pack
NuGets
WiX