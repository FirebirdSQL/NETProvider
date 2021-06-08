param(
	[Parameter(Mandatory=$True)]$Configuration,
	[Parameter(Mandatory=$True)]$FirebirdSelection,
	[Parameter(Mandatory=$True)]$TestSuite)

$ErrorActionPreference = 'Stop'

$baseDir = Split-Path -Parent $PSCommandPath

. "$baseDir\include.ps1"

$FirebirdConfiguration = @{
	FB40 = @{
		Download = 'https://www.dropbox.com/s/72j823pvdfdrvww/fb40.7z?dl=1';
		Executable = '.\firebird.exe';
		Args = @('-a');
	};
	FB30 = @{
		Download = 'https://www.dropbox.com/s/x46uy7e5zrtsnux/fb30.7z?dl=1';
		Executable = '.\firebird.exe';
		Args = @('-a');
	};
}

$testsBaseDir = "$baseDir\src\FirebirdSql.Data.FirebirdClient.Tests"
$testsProviderDir = "$testsBaseDir\bin\$Configuration\$(Get-UsedTargetFramework)"

$startDir = $null
$firebirdProcess = $null

if ($env:tests_firebird_dir) {
	$firebirdDir = $env:tests_firebird_dir
}
else {
	$firebirdDir = 'I:\Downloads\fb_tests'
}

function Prepare() {
	echo "=== $($MyInvocation.MyCommand.Name) ==="

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
	7z x -bsp0 -bso0 $fbDownloadName
	cp -Recurse -Force .\embedded\* $testsProviderDir
	rmdir -Recurse .\embedded
	rm $fbDownloadName
	mv .\server\* .
	rmdir .\server

	ni firebird.log -ItemType File | Out-Null

	echo "Starting Firebird"
	$process = Start-Process -FilePath $selectedConfiguration.Executable -ArgumentList $selectedConfiguration.Args -PassThru
	echo "Version: $($process.MainModule.FileVersionInfo.FileVersion)"
	$script:firebirdProcess = $process

	echo "=== END ==="
}

function Cleanup() {
	echo "=== $($MyInvocation.MyCommand.Name) ==="

	cd $script:startDir
	$process = $script:firebirdProcess
	$process.Kill()
	$process.WaitForExit()
	# give OS time to release all files
	sleep -Milliseconds 100
	rm -Force -Recurse $firebirdDir

	echo "=== END ==="
}

function Tests-All() {
	Tests-FirebirdClient-Default-C-R
	Tests-FirebirdClient-Default-NC-R
	Tests-FirebirdClient-Default-C-D
	Tests-FirebirdClient-Default-NC-D
	Tests-FirebirdClient-Embedded
	Tests-EFCore
	Tests-EFCore-Functional
	Tests-EF6
}

function Tests-FirebirdClient-Default-C-R() {
	Tests-FirebirdClient 'Default' $True 'Required'
}
function Tests-FirebirdClient-Default-NC-R() {
	Tests-FirebirdClient 'Default' $False 'Required'
}
function Tests-FirebirdClient-Default-C-D() {
	Tests-FirebirdClient 'Default' $True 'Disabled'
}
function Tests-FirebirdClient-Default-NC-D() {
	Tests-FirebirdClient 'Default' $False 'Disabled'
}
function Tests-FirebirdClient-Embedded() {
	Tests-FirebirdClient 'Embedded' $False 'Disabled'
}
function Tests-FirebirdClient($serverType, $compression, $wireCrypt) {
	cd $testsProviderDir
	.\FirebirdSql.Data.FirebirdClient.Tests.exe --labels=All "--where=(ServerType==$serverType && Compression==$compression && WireCrypt==$wireCrypt) || Category==NoServer"
	Check-ExitCode
}

function Tests-EF6() {
	cd "$baseDir\src\EntityFramework.Firebird.Tests\bin\$Configuration\$(Get-UsedTargetFramework)"
	.\EntityFramework.Firebird.Tests.exe --labels=All
	Check-ExitCode
}

function Tests-EFCore() {
	cd "$baseDir\src\FirebirdSql.EntityFrameworkCore.Firebird.Tests\bin\$Configuration\$(Get-UsedTargetFramework)"
	.\FirebirdSql.EntityFrameworkCore.Firebird.Tests.exe --labels=All
	Check-ExitCode
}
function Tests-EFCore-Functional() {
	cd "$baseDir\src\FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests"
	dotnet test --no-build -c $Configuration
	Check-ExitCode
}

try {
	Prepare
	& $TestSuite
}
finally {
	Cleanup
}
