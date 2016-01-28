set CONFIGURATION=Debug
set FRAMEWORK=dnx451
set SOLUTION_FOLDER=%~dp0..
set PACKAGE_ROOT=%SOLUTION_FOLDER%\Hosting\pkg\%CONFIGURATION%

if exist %PACKAGE_ROOT%\ (
    rmdir /q /s %PACKAGE_ROOT%\
)

if not exist %SOLUTION_FOLDER%\Microsoft.ServiceFabric.AspNetCore\project.lock.json (
    dotnet restore %SOLUTION_FOLDER%\Microsoft.ServiceFabric.AspNetCore\
)

for %%S in (Counter,Gateway,Sms) do (
    if not exist %SOLUTION_FOLDER%\%%S\project.lock.json (
        dotnet restore %SOLUTION_FOLDER%\%%S\
    )

    dotnet publish %SOLUTION_FOLDER%\%%S\ -c %CONFIGURATION% -f %FRAMEWORK%

    robocopy /E %SOLUTION_FOLDER%\%%S\PackageRoot\ %PACKAGE_ROOT%\%%S\
    robocopy /E %SOLUTION_FOLDER%\%%S\bin\%CONFIGURATION%\%FRAMEWORK%\ %PACKAGE_ROOT%\%%S\Code\
    
    copy /Y %SOLUTION_FOLDER%\%%S\appsettings.json %PACKAGE_ROOT%\%%S\Code\
    copy /Y %SOLUTION_FOLDER%\%%S\hosting.json %PACKAGE_ROOT%\%%S\Code\
)

copy /Y %SOLUTION_FOLDER%\Hosting\ApplicationManifest.xml %PACKAGE_ROOT%\