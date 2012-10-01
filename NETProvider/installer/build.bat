set WIX=C:\Program Files\Windows Installer XML v3.0\bin\
set SOLUTION_DIR=%~dp0%..\

"%WIX%candle.exe" -dSolutionDir=%SOLUTION_DIR% -out "%SOLUTION_DIR%installer\out\Install.wixobj" "%SOLUTION_DIR%installer\Install.wxs"
"%WIX%light.exe" -ext "%WIX%WixUIExtension.dll" -out "%SOLUTION_DIR%installer\out\NETProvider.msi" "%SOLUTION_DIR%installer\out\Install.wixobj"
