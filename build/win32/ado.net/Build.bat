@echo off

IF "%USERNAME%" == "Jiri" ( rem Jiri
  SET NANT=I:\Downloads\nant-0.85\bin\NAnt.exe
) ELSE ( rem Carlos
  SET NANT=d:\desarrollo\herramientas\nant\bin\nant
)

%NANT% -verbose > Build.log