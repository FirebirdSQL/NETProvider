set WIX=I:\devel\bin\wix35-binaries\
set BASE_DIR=%~dp0%..\

"%WIX%candle.exe" -dBaseDir=%BASE_DIR% -out "%BASE_DIR%installer\out\Install.wixobj" "%BASE_DIR%installer\Install.wxs"
"%WIX%light.exe" -ext "%WIX%WixUIExtension.dll" -ext "%WIX%WixUtilExtension.dll" -out "%BASE_DIR%installer\out\DDEX.msi" "%BASE_DIR%installer\out\Install.wixobj"
