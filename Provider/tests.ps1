param(
	[Parameter(Mandatory=$True)]$Configuration,
	[Parameter(Mandatory=$True)]$FbDownload,
	[Parameter(Mandatory=$True)]$FbStart)

$ErrorActionPreference = 'Stop'

$baseDir = Resolve-Path .
$testsBaseDir = "$baseDir\Provider\src\FirebirdSql.Data.FirebirdClient.Tests"
$testsNETDir = "$testsBaseDir\bin\$Configuration\net452"
$testsCOREDir = "$testsBaseDir\bin\$Configuration\netcoreapp2.0"

function Exec($command) {
	& $command
	if (-not $?) {
		exit 1
	}
}

function Prepare() {
	$fbDownloadName = $FbDownload -Replace '.+/([^/]+)\?dl=1','$1'
	mkdir $env:tests_firebird_dir | Out-Null
	cd $env:tests_firebird_dir
	(New-Object System.Net.WebClient).DownloadFile($FbDownload, (Join-Path (pwd) $fbDownloadName))
	7z x $fbDownloadName | Out-Null
	cp -Recurse .\embedded\* $testsNETDir
	cp -Recurse .\embedded\* $testsCOREDir
	rmdir -Recurse .\embedded
	mv .\server\* .
	rmdir .\server

	iex $FbStart
	ni firebird.log -ItemType File | Out-Null
}

function Tests-FirebirdClient() {
	echo "=== $($MyInvocation.MyCommand.Name) ==="

	cd $testsNETDir
	Exec { .\FirebirdSql.Data.FirebirdClient.Tests.exe --result=tests.xml }
	cd $testsCOREDir
	Exec { dotnet FirebirdSql.Data.FirebirdClient.Tests.dll --result=tests.xml }

	echo "=== END ==="
}

function Tests-EF() {
	echo "=== $($MyInvocation.MyCommand.Name) ==="

	cd "$baseDir\Provider\src\EntityFramework.Firebird.Tests\bin\$Configuration\net452"
	Exec { .\EntityFramework.Firebird.Tests.exe --result=tests.xml }

	echo "=== END ==="
}

function Tests-EFCore() {
	echo "=== $($MyInvocation.MyCommand.Name) ==="

	cd "$baseDir\Provider\src\FirebirdSql.EntityFrameworkCore.Firebird.Tests\bin\$Configuration\netcoreapp2.0"
	Exec { dotnet FirebirdSql.EntityFrameworkCore.Firebird.Tests.dll --result=tests.xml }

	echo "=== END ==="
}

Prepare
Tests-FirebirdClient
Tests-EF
Tests-EFCore
