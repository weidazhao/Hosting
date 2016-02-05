@echo off

rem DotNet CLI Version
rem d53b405ebd451b537636c7fcafa8697b69c85cf2
rem 1.0.1.001134

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
dotnet compile %SOLUTION_FOLDER%\Microsoft.ServiceFabric.AspNetCore.Hosting\ -c %CONFIGURATION% -f %FRAMEWORK%

if not exist %SOLUTION_FOLDER%\Microsoft.ServiceFabric.AspNetCore.Gateway\project.lock.json (
    dotnet restore %SOLUTION_FOLDER%\Microsoft.ServiceFabric.AspNetCore.Gateway\
)
dotnet compile %SOLUTION_FOLDER%\Microsoft.ServiceFabric.AspNetCore.Gateway\ -c %CONFIGURATION% -f %FRAMEWORK%

for %%S in (Counter,Gateway,Sms) do (
    if not exist %SOLUTION_FOLDER%\%%S\project.lock.json (
        dotnet restore %SOLUTION_FOLDER%\%%S\
    )

    dotnet compile %SOLUTION_FOLDER%\%%S\ -c %CONFIGURATION% -f %FRAMEWORK%
    dotnet publish %SOLUTION_FOLDER%\%%S\ -c %CONFIGURATION% -f %FRAMEWORK% -r %RUNTIME%

    robocopy /E %SOLUTION_FOLDER%\%%S\PackageRoot\ %PACKAGE_ROOT%\%%S\
    robocopy /E %SOLUTION_FOLDER%\%%S\bin\%CONFIGURATION%\%FRAMEWORK%\%RUNTIME%\ %PACKAGE_ROOT%\%%S\Code\
    
    copy /Y %SOLUTION_FOLDER%\%%S\appsettings.json %PACKAGE_ROOT%\%%S\Code\    
)

copy /Y %SOLUTION_FOLDER%\Hosting\ApplicationManifest.xml %PACKAGE_ROOT%\