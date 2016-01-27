set CURRENT_FOLDER=%~dp0
set CONFIGURATION=Debug
set FRAMEWORK=dnx451

if not exist %CURRENT_FOLDER%..\Microsoft.ServiceFabric.AspNetCore\project.lock.json (
    dotnet restore %CURRENT_FOLDER%..\Microsoft.ServiceFabric.AspNetCore\
)

if not exist %CURRENT_FOLDER%..\Counter\project.lock.json (
    dotnet restore %CURRENT_FOLDER%..\Counter\
)
dotnet publish %CURRENT_FOLDER%..\Counter\ -c %CONFIGURATION% -f %FRAMEWORK%
robocopy /MIR %CURRENT_FOLDER%..\Counter\bin\%CONFIGURATION%\%FRAMEWORK%\ %CURRENT_FOLDER%\pkg\%CONFIGURATION%\Counter\Code\
copy /Y %CURRENT_FOLDER%..\Counter\appsettings.json %CURRENT_FOLDER%\pkg\%CONFIGURATION%\Counter\Code\
copy /Y %CURRENT_FOLDER%..\Counter\hosting.json %CURRENT_FOLDER%\pkg\%CONFIGURATION%\Counter\Code\
copy /Y %CURRENT_FOLDER%..\Counter\PackageRoot\ServiceManifest.xml %CURRENT_FOLDER%\pkg\%CONFIGURATION%\Counter\

if not exist %CURRENT_FOLDER%..\Gateway\project.lock.json (
    dotnet restore %CURRENT_FOLDER%..\Gateway\
)
dotnet publish %CURRENT_FOLDER%..\Gateway\ -c %CONFIGURATION% -f %FRAMEWORK%
robocopy /MIR %CURRENT_FOLDER%..\Gateway\bin\%CONFIGURATION%\%FRAMEWORK%\ %CURRENT_FOLDER%\pkg\%CONFIGURATION%\Gateway\Code\
copy /Y %CURRENT_FOLDER%..\Gateway\appsettings.json %CURRENT_FOLDER%\pkg\%CONFIGURATION%\Gateway\Code\
copy /Y %CURRENT_FOLDER%..\Gateway\hosting.json %CURRENT_FOLDER%\pkg\%CONFIGURATION%\Gateway\Code\
copy /Y %CURRENT_FOLDER%..\Gateway\PackageRoot\ServiceManifest.xml %CURRENT_FOLDER%\pkg\%CONFIGURATION%\Gateway\

if not exist %CURRENT_FOLDER%..\Sms\project.lock.json (
    dotnet restore %CURRENT_FOLDER%..\Sms\
)
dotnet publish %CURRENT_FOLDER%..\Sms\ -c %CONFIGURATION% -f %FRAMEWORK%
robocopy /MIR %CURRENT_FOLDER%..\Sms\bin\%CONFIGURATION%\%FRAMEWORK%\ %CURRENT_FOLDER%\pkg\%CONFIGURATION%\Sms\Code\
copy /Y %CURRENT_FOLDER%..\Sms\appsettings.json %CURRENT_FOLDER%\pkg\%CONFIGURATION%\Sms\Code\
copy /Y %CURRENT_FOLDER%..\Sms\hosting.json %CURRENT_FOLDER%\pkg\%CONFIGURATION%\Sms\Code\
copy /Y %CURRENT_FOLDER%..\Sms\PackageRoot\ServiceManifest.xml %CURRENT_FOLDER%\pkg\%CONFIGURATION%\Sms\

copy /Y %CURRENT_FOLDER%ApplicationManifest.xml %CURRENT_FOLDER%\pkg\%CONFIGURATION%\