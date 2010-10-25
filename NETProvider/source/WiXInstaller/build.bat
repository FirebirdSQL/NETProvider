set WIX=C:\Program Files\Windows Installer XML v3.0\
set SOLUTION_DIR=%~dp0%..\

"%WIX%bin\candle.exe" -dSolutionDir=%SOLUTION_DIR% -out "%SOLUTION_DIR%WixInstaller\out\Install.wixobj" "%SOLUTION_DIR%WixInstaller\Install.wxs"
"%WIX%bin\light.exe" -ext "%WIX%bin\WixUIExtension.dll" -out "%SOLUTION_DIR%WixInstaller\out\NETProvider.msi" "%SOLUTION_DIR%WixInstaller\out\Install.wixobj"
