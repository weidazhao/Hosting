@echo off

rem DotNet CLI Version
rem 2be5e84f874a2d15ee3f091130a3e22e479eb700
rem 1.0.0-rc2-002339

set CONFIGURATION=Debug
set SOLUTION_FOLDER=%~dp0..
set PACKAGE_ROOT=%SOLUTION_FOLDER%\Hosting\pkg\%CONFIGURATION%

if exist %PACKAGE_ROOT%\ (
    rmdir /q /s %PACKAGE_ROOT%\
)

if not exist %SOLUTION_FOLDER%\Microsoft.ServiceFabric.AspNetCore.Hosting\project.lock.json (
    dotnet restore %SOLUTION_FOLDER%\Microsoft.ServiceFabric.AspNetCore.Hosting\
)

if not exist %SOLUTION_FOLDER%\Microsoft.ServiceFabric.AspNetCore.Gateway\project.lock.json (
    dotnet restore %SOLUTION_FOLDER%\Microsoft.ServiceFabric.AspNetCore.Gateway\
)

for %%S in (Counter,Gateway,Sms) do (
    robocopy /E %SOLUTION_FOLDER%\%%S\PackageRoot\ %PACKAGE_ROOT%\%%S\

    if not exist %SOLUTION_FOLDER%\%%S\project.lock.json (
        dotnet restore %SOLUTION_FOLDER%\%%S\
    )
    dotnet publish %SOLUTION_FOLDER%\%%S\ -c %CONFIGURATION% -o %PACKAGE_ROOT%\%%S\Code\
)

copy /Y %SOLUTION_FOLDER%\Hosting\ApplicationPackageRoot\ApplicationManifest.xml %PACKAGE_ROOT%\
