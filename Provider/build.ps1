param([Parameter(Mandatory=$True)]$Configuration)

$baseDir = Split-Path -parent $PSCommandPath

msbuild /t:Clean,Build /p:Configuration=$Configuration $baseDir\src\NETProvider.sln /v:m /m