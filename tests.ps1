param(
	[Parameter(Mandatory=$True)]$Configuration,
	[Parameter(Mandatory=$True)]$FirebirdSelection,
	[Parameter(Mandatory=$True)]$TestSuite)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue' # Faster downloads with Invoke-RestMethod -- https://stackoverflow.com/a/43477248/33244

$baseDir = Split-Path -Parent $PSCommandPath

. "$baseDir\include.ps1"

$FirebirdConfiguration = @{
	FB50 = @{
		Download = 'https://github.com/FirebirdSQL/NETProvider-tests-infrastructure/raw/master/fb50.7z'
		Executable = '.\firebird.exe'
		Args = @('-a')
	}
	FB40 = @{
		Download = 'https://github.com/FirebirdSQL/NETProvider-tests-infrastructure/raw/master/fb40.7z'
		Executable = '.\firebird.exe'
		Args = @('-a')
	}
	FB30 = @{
		Download = 'https://github.com/FirebirdSQL/NETProvider-tests-infrastructure/raw/master/fb30.7z'
		Executable = '.\firebird.exe'
		Args = @('-a')
	}
}

$frameworkVersion = 'net10.0'
$testsBaseDir = "$baseDir\src\FirebirdSql.Data.FirebirdClient.Tests"
$testsProviderDir = "$testsBaseDir\bin\$Configuration\$frameworkVersion"

$firebirdProcess = $null

if ($env:tests_firebird_dir) {
	$firebirdDir = $env:tests_firebird_dir
}
else {
	$firebirdDir = "$env:TEMP\fb_tests\$FirebirdSelection\"
}

function Prepare() {
	echo "=== $($MyInvocation.MyCommand.Name) ==="

	$selectedConfiguration = $FirebirdConfiguration[$FirebirdSelection]
	$fbDownload = $selectedConfiguration.Download
	$fbDownloadName = $fbDownload -Replace '.+/(.+)$','$1'
	if (Test-Path $firebirdDir) {
		rm -Force -Recurse $firebirdDir
	}
	mkdir $firebirdDir | Out-Null

	pushd $firebirdDir
	try {
		echo "Downloading $fbDownload"
		Invoke-RestMethod -Uri $fbDownload -OutFile $fbDownloadName 
		echo "Extracting $fbDownloadName"
		7z x -bsp0 -bso0 $fbDownloadName
		rm $fbDownloadName
		cp -Recurse -Force .\* $testsProviderDir

		ni firebird.log -ItemType File | Out-Null

		echo "Starting Firebird"
		$process = Start-Process -FilePath $selectedConfiguration.Executable -ArgumentList $selectedConfiguration.Args -PassThru
		echo "Version: $($process.MainModule.FileVersionInfo.FileVersion)"
		$script:firebirdProcess = $process

		echo "=== END ==="
	}
	finally {
		popd
	}
}

function Cleanup() {
	# Do not write to output (Write-Output, echo) here. Write-Host is ok.
	# -- https://stackoverflow.com/a/45105609

	Write-Host "=== $($MyInvocation.MyCommand.Name) ==="

	$process = $script:firebirdProcess
	if ($process) {
		$process.Kill()
		$process.WaitForExit()
		# give OS time to release all files
		sleep -Milliseconds 100
	}
	rm -Force -Recurse $firebirdDir

	Write-Host "=== END ==="
}

function Tests-All() {
	Tests-FirebirdClient-Default-Compression-CryptRequired
	Tests-FirebirdClient-Default-NoCompression-CryptRequired
	Tests-FirebirdClient-Default-Compression-CryptDisabled
	Tests-FirebirdClient-Default-NoCompression-CryptDisabled
	Tests-FirebirdClient-Embedded
	Tests-EFCore
	Tests-EFCore-Functional
	Tests-EF6
}

function Tests-FirebirdClient-Default-Compression-CryptRequired() {
	Tests-FirebirdClient 'Default' $True 'Required'
}
function Tests-FirebirdClient-Default-NoCompression-CryptRequired() {
	Tests-FirebirdClient 'Default' $False 'Required'
}
function Tests-FirebirdClient-Default-Compression-CryptDisabled() {
	Tests-FirebirdClient 'Default' $True 'Disabled'
}
function Tests-FirebirdClient-Default-NoCompression-CryptDisabled() {
	Tests-FirebirdClient 'Default' $False 'Disabled'
}
function Tests-FirebirdClient-Embedded() {
	Tests-FirebirdClient 'Embedded' $False 'Disabled'
}
function Tests-FirebirdClient($serverType, $compression, $wireCrypt) {
	pushd $testsProviderDir
	try {
		.\FirebirdSql.Data.FirebirdClient.Tests.exe --labels=All "--where=(ServerType==$serverType && Compression==$compression && WireCrypt==$wireCrypt) || Category==NoServer"
		Check-ExitCode
	}
	finally {
		popd
	}
}

function Tests-EF6() {
	pushd "$baseDir\src\EntityFramework.Firebird.Tests\bin\$Configuration\$frameworkVersion"
	try {
		.\EntityFramework.Firebird.Tests.exe --labels=All
		Check-ExitCode
	}
	finally {
		popd
	}
}

function Tests-EFCore() {
	pushd "$baseDir\src\FirebirdSql.EntityFrameworkCore.Firebird.Tests\bin\$Configuration\$frameworkVersion"
	try {
		.\FirebirdSql.EntityFrameworkCore.Firebird.Tests.exe --labels=All
		Check-ExitCode
	}
	finally {
		popd
	}
}
function Tests-EFCore-Functional() {
	pushd "$baseDir\src\FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests"
	try {
		dotnet test --no-build -c $Configuration
		Check-ExitCode
	}
	finally {
		popd
	}
}

try {
	Prepare
	& $TestSuite
}
finally {
	Cleanup
}
