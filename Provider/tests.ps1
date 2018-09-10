param(
	[Parameter(Mandatory=$True)]$Configuration,
	[Parameter(Mandatory=$True)]$FirebirdSelection,
	[Parameter(Mandatory=$True)]$TestSuite)

$ErrorActionPreference = 'Stop'

$FirebirdConfiguration = @{
	FB30_Default = @{
		Download = 'https://www.dropbox.com/s/x46uy7e5zrtsnux/fb30.7z?dl=1';
		Executable = '.\firebird.exe';
		Args = @('-a');
	};
	FB25_SC = @{
		Download = 'https://www.dropbox.com/s/ayzjnxjx20vb7s5/fb25.7z?dl=1';
		Executable = '.\bin\fb_inet_server.exe';
		Args = @('-a', '-m');
	};
}

$baseDir = Split-Path -Parent $PSCommandPath
$testsBaseDir = "$baseDir\src\FirebirdSql.Data.FirebirdClient.Tests"
$testsNETDir = "$testsBaseDir\bin\$Configuration\net452"
$testsCOREDir = "$testsBaseDir\bin\$Configuration\netcoreapp2.0"

$startDir = $null
$firebirdProcess = $null

if ($env:tests_firebird_dir) {
	$firebirdDir = $env:tests_firebird_dir
}
else {
	$firebirdDir = 'I:\Downloads\fb_tests'
}

function Check-ExitCode($command) {
	& $command
	$exitCode = $LASTEXITCODE
	if ($exitCode -ne 0) {
		echo "Non-zero ($exitCode) exit code. Exiting..."
		exit $exitCode
	}
}

function Prepare() {
	$script:startDir = $pwd
	$selectedConfiguration = $FirebirdConfiguration[$FirebirdSelection]
	$fbDownload = $selectedConfiguration.Download
	$fbDownloadName = $fbDownload -Replace '.+/([^/]+)\?dl=1','$1'
	if (Test-Path $firebirdDir) {
		rm -Force -Recurse $firebirdDir
	}
	mkdir $firebirdDir | Out-Null
	cd $firebirdDir
	echo "Downloading $fbDownload"
	(New-Object System.Net.WebClient).DownloadFile($fbDownload, (Join-Path (pwd) $fbDownloadName))
	echo "Extracting $fbDownloadName"
	7z x $fbDownloadName | Out-Null
	cp -Recurse -Force .\embedded\* $testsNETDir
	cp -Recurse -Force .\embedded\* $testsCOREDir
	rmdir -Recurse .\embedded
	rm $fbDownloadName
	mv .\server\* .
	rmdir .\server

	ni firebird.log -ItemType File | Out-Null

	echo "Starting Firebird"
	$script:firebirdProcess = Start-Process -FilePath $selectedConfiguration.Executable -ArgumentList $selectedConfiguration.Args -PassThru
}

function Cleanup() {
	cd $script:startDir
	$process = $script:firebirdProcess
	$process.Kill()
	$process.WaitForExit()
	rm -Force -Recurse $firebirdDir
}

function Tests-All() {
	Tests-FirebirdClient
	Tests-EF6
	Tests-EFCore
}

function Tests-FirebirdClient() {
	echo "=== $($MyInvocation.MyCommand.Name) ==="

	cd $testsNETDir
	Check-ExitCode { .\FirebirdSql.Data.FirebirdClient.Tests.exe --result=tests.xml --labels=All }
	cd $testsCOREDir
	Check-ExitCode { dotnet FirebirdSql.Data.FirebirdClient.Tests.dll --result=tests.xml --labels=All }

	echo "=== END ==="
}

function Tests-EF6() {
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
try {
	& $TestSuite
}
finally {
	Cleanup
}
