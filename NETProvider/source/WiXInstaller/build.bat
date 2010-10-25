set WIX=C:\Program Files\Windows Installer XML v3.0\bin\
set SOLUTION_DIR=%~dp0%..\

"%WIX%candle.exe" -dSolutionDir=%SOLUTION_DIR% -out "%SOLUTION_DIR%WiXInstaller\out\Install.wixobj" "%SOLUTION_DIR%WiXInstaller\Install.wxs"
"%WIX%light.exe" -ext "%WIX%WixUIExtension.dll" -out "%SOLUTION_DIR%WiXInstaller\out\NETProvider.msi" "%SOLUTION_DIR%WiXInstaller\out\Install.wixobj"
