param(
	[Parameter(Mandatory=$True)]$Configuration)

$ErrorActionPreference = 'Stop'

$baseDir = Split-Path -Parent $PSCommandPath

. "$baseDir\include.ps1"

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
	function b($target, $check=$True) {
		dotnet msbuild /t:$target /p:Configuration=$Configuration "$baseDir\src\NETProvider.sln" /v:m /m
		if ($check) {
			Check-ExitCode
		}
	}
	b 'Clean'
	# this sometimes fails on CI
	b 'Restore' $False
	b 'Restore'
	b 'Build'
	$script:version = (Get-Item $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\$Configuration\net452\FirebirdSql.Data.FirebirdClient.dll).VersionInfo.ProductVersion -replace '(\d+)\.(\d+)\.(\d+)(-[a-z0-9]+)?(.*)','$1.$2.$3$4'
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
NuGets
WiX
