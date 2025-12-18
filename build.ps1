param(
	[Parameter(Mandatory=$True)]$Configuration)

$ErrorActionPreference = 'Stop'

$baseDir = Split-Path -Parent $PSCommandPath

. "$baseDir\include.ps1"

$outDir = "$baseDir\out"
$versionProvider = ''
$versionEFCore = ''
$versionEF6 = ''

function Clean() {
	if (Test-Path $outDir) {
		rm -Force -Recurse $outDir
	}
	mkdir $outDir | Out-Null
}

function Build() {
	dotnet clean "$baseDir\src\NETProvider.slnx" -c $Configuration -v m
	dotnet build "$baseDir\src\NETProvider.slnx" -c $Configuration -p:ContinuousIntegrationBuild=true -v m
}

function Versions() {
	function v($file) {
		return (Get-Item $file).VersionInfo.ProductVersion -replace '(\d+)\.(\d+)\.(\d+)(-[a-z0-9]+)?.*','$1.$2.$3$4'
	}
	$script:versionProvider = v $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\$Configuration\net10.0\FirebirdSql.Data.FirebirdClient.dll
	$script:versionEFCore = v $baseDir\src\FirebirdSql.EntityFrameworkCore.Firebird\bin\$Configuration\net10.0\FirebirdSql.EntityFrameworkCore.Firebird.dll
	$script:versionEF6 = v $baseDir\src\EntityFramework.Firebird\bin\$Configuration\net48\EntityFramework.Firebird.dll
}

function NuGets() {
	cp $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\$Configuration\FirebirdSql.Data.FirebirdClient.$versionProvider.nupkg $outDir
	cp $baseDir\src\FirebirdSql.EntityFrameworkCore.Firebird\bin\$Configuration\FirebirdSql.EntityFrameworkCore.Firebird.$versionEFCore.nupkg $outDir
	cp $baseDir\src\EntityFramework.Firebird\bin\$Configuration\EntityFramework.Firebird.$versionEF6.nupkg $outDir

	cp $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\$Configuration\FirebirdSql.Data.FirebirdClient.$versionProvider.snupkg $outDir
	cp $baseDir\src\FirebirdSql.EntityFrameworkCore.Firebird\bin\$Configuration\FirebirdSql.EntityFrameworkCore.Firebird.$versionEFCore.snupkg $outDir
	cp $baseDir\src\EntityFramework.Firebird\bin\$Configuration\EntityFramework.Firebird.$versionEF6.snupkg $outDir
}

Clean
Build
Versions
NuGets
