@echo off

rem DotNet CLI Version
rem 0c31e53bfa562b0231701cf33e9798affeebbf07
rem 1.0.0.001711

set CONFIGURATION=Debug
set FRAMEWORK=net451
set RUNTIME=win7-x64
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
    if not exist %SOLUTION_FOLDER%\%%S\project.lock.json (
        dotnet restore %SOLUTION_FOLDER%\%%S\
    )
    
    dotnet publish %SOLUTION_FOLDER%\%%S\ -c %CONFIGURATION% -f %FRAMEWORK% -r %RUNTIME%

    robocopy /E %SOLUTION_FOLDER%\%%S\PackageRoot\ %PACKAGE_ROOT%\%%S\
    robocopy /E %SOLUTION_FOLDER%\%%S\bin\%CONFIGURATION%\%FRAMEWORK%\%RUNTIME%\publish\ %PACKAGE_ROOT%\%%S\Code\
    
    copy /Y %SOLUTION_FOLDER%\%%S\appsettings.json %PACKAGE_ROOT%\%%S\Code\    
)

copy /Y %SOLUTION_FOLDER%\Hosting\ApplicationPackageRoot\ApplicationManifest.xml %PACKAGE_ROOT%\