param(
	[Parameter(Mandatory=$True)]$Configuration,
	[Parameter(Mandatory=$True)]$FbDownload,
	[Parameter(Mandatory=$True)]$FbStart)

$ErrorActionPreference = 'Stop'

$baseDir = Split-Path -Parent $PSCommandPath
$testsBaseDir = "$baseDir\src\FirebirdSql.Data.FirebirdClient.Tests"
$testsNETDir = "$testsBaseDir\bin\$Configuration\net452"
$testsCOREDir = "$testsBaseDir\bin\$Configuration\netcoreapp2.0"

function Check-ExitCode($command) {
	& $command
	$exitCode = $LASTEXITCODE
	if ($exitCode -ne 0) {
		echo "Non-zero ($exitCode) exit code. Exiting..."
		exit $exitCode
	}
}

function Prepare() {
	$fbDownloadName = $FbDownload -Replace '.+/([^/]+)\?dl=1','$1'
	mkdir $env:tests_firebird_dir | Out-Null
	cd $env:tests_firebird_dir
	(New-Object System.Net.WebClient).DownloadFile($FbDownload, (Join-Path (pwd) $fbDownloadName))
	7z x $fbDownloadName | Out-Null
	cp -Recurse -Force .\embedded\* $testsNETDir
	cp -Recurse -Force .\embedded\* $testsCOREDir
	rmdir -Recurse .\embedded
	rm $fbDownloadName
	mv .\server\* .
	rmdir .\server

	iex $FbStart
	ni firebird.log -ItemType File | Out-Null
}

function Tests-FirebirdClient() {
	echo "=== $($MyInvocation.MyCommand.Name) ==="

	cd $testsNETDir
	Check-ExitCode { .\FirebirdSql.Data.FirebirdClient.Tests.exe --result=tests.xml --labels=All }
	cd $testsCOREDir
	Check-ExitCode { dotnet FirebirdSql.Data.FirebirdClient.Tests.dll --result=tests.xml --labels=All }

	echo "=== END ==="
}

function Tests-EF() {
	echo "=== $($MyInvocation.MyCommand.Name) ==="

	cd "$baseDir\src\EntityFramework.Firebird.Tests\bin\$Configuration\net452"
	Check-ExitCode { .\EntityFramework.Firebird.Tests.exe --result=tests.xml --labels=All }

	echo "=== END ==="
}

function Tests-EFCore() {
	echo "=== $($MyInvocation.MyCommand.Name) ==="

	cd "$baseDir\src\FirebirdSql.EntityFrameworkCore.Firebird.Tests\bin\$Configuration\netcoreapp2.0"
	Check-ExitCode { dotnet FirebirdSql.EntityFrameworkCore.Firebird.Tests.dll --result=tests.xml --labels=All }

	echo "=== END ==="
}

Prepare
Tests-FirebirdClient
Tests-EF
Tests-EFCore
