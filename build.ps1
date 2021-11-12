param(
	[Parameter(Mandatory=$True)]$Configuration)

$ErrorActionPreference = 'Stop'

$baseDir = Split-Path -Parent $PSCommandPath

. "$baseDir\include.ps1"

$outDir = "$baseDir\out"
$version = ''

function Clean() {
	if (Test-Path $outDir) {
		rm -Force -Recurse $outDir
	}
	mkdir $outDir | Out-Null
}

function Build() {
	function b($target, $check=$True) {
		dotnet msbuild /t:$target /p:Configuration=$Configuration /p:ContinuousIntegrationBuild=true "$baseDir\src\NETProvider.sln" /v:m /m
		if ($check) {
			Check-ExitCode
		}
	}
	b 'Clean'
	# this sometimes fails on CI
	b 'Restore' $False
	b 'Restore'
	b 'Build'
	$script:version = (Get-Item $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\$Configuration\$(Get-UsedTargetFramework)\FirebirdSql.Data.FirebirdClient.dll).VersionInfo.ProductVersion -replace '(\d+)\.(\d+)\.(\d+)(-[a-z0-9]+)?(.*)','$1.$2.$3$4'
}

function NuGets() {
	cp $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\$Configuration\FirebirdSql.Data.FirebirdClient.$version.nupkg $outDir
	cp $baseDir\src\EntityFramework.Firebird\bin\$Configuration\EntityFramework.Firebird.$version.nupkg $outDir
	cp $baseDir\src\FirebirdSql.EntityFrameworkCore.Firebird\bin\$Configuration\FirebirdSql.EntityFrameworkCore.Firebird.$version.nupkg $outDir

	cp $baseDir\src\FirebirdSql.Data.FirebirdClient\bin\$Configuration\FirebirdSql.Data.FirebirdClient.$version.snupkg $outDir
	cp $baseDir\src\EntityFramework.Firebird\bin\$Configuration\EntityFramework.Firebird.$version.snupkg $outDir
	cp $baseDir\src\FirebirdSql.EntityFrameworkCore.Firebird\bin\$Configuration\FirebirdSql.EntityFrameworkCore.Firebird.$version.snupkg $outDir
}

Clean
Build
NuGets
