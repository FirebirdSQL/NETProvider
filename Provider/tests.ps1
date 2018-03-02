param(
	[Parameter(Mandatory=$True)]$Configuration,
	[Parameter(Mandatory=$True)]$FbDownload,
	[Parameter(Mandatory=$True)]$FbStart)

$ErrorActionPreference = 'Stop'

$baseDir = Resolve-Path .
$testsBaseDir = "$baseDir\Provider\src\FirebirdSql.Data.FirebirdClient.Tests"
$testsNETDir = "$testsBaseDir\bin\$Configuration\net452"
$testsCOREDir = "$testsBaseDir\bin\$Configuration\netcoreapp2.0"

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

cd $testsNETDir
nunit3-console FirebirdSql.Data.FirebirdClient.Tests.dll --framework=net-4.5 --result="dummy.xml;format=$env:tests_result_format"

cd $testsBaseDir
dotnet test FirebirdSql.Data.FirebirdClient.Tests.csproj -c $Configuration -f netcoreapp2.0 --no-build --no-restore

cd "$baseDir\Provider\src\EntityFramework.Firebird.Tests\bin\$Configuration\net452"
nunit3-console EntityFramework.Firebird.Tests.dll --framework=net-4.5 --result="dummy.xml;format=$env:tests_result_format"

cd "$baseDir\Provider\src\FirebirdSql.EntityFrameworkCore.Firebird.Tests"
dotnet test FirebirdSql.EntityFrameworkCore.Firebird.Tests.csproj -c $Configuration -f netcoreapp2.0 --no-build --no-restore