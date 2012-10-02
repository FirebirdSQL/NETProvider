$wix = 'I:\devel\bin\wix-binaries'
$baseDir = Split-Path -Parent (Split-Path -parent $MyInvocation.MyCommand.Definition)

& $wix\candle.exe "-dBaseDir=$baseDir" -ext $wix\WixUtilExtension.dll -out $baseDir\installer\out\Install.wixobj $baseDir\installer\Install.wxs
& $wix\light.exe -ext $wix\WixUIExtension.dll -ext $wix\WixUtilExtension.dll -out $baseDir\installer\out\NETProvider.msi $baseDir\installer\out\Install.wixobj

