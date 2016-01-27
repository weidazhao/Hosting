set CURRENT_FOLDER=%~dp0
set CONFIGURATION=Debug
set FRAMEWORK=dnx451

dotnet restore %CURRENT_FOLDER%..\Microsoft.ServiceFabric.AspNetCore\

dotnet restore %CURRENT_FOLDER%..\Counter\
dotnet publish %CURRENT_FOLDER%..\Counter\ -c %CONFIGURATION% -f %FRAMEWORK%
robocopy /MIR %CURRENT_FOLDER%..\Counter\bin\%CONFIGURATION%\%FRAMEWORK%\ %CURRENT_FOLDER%\pkg\%CONFIGURATION%\Counter\Code\
copy /Y %CURRENT_FOLDER%..\Counter\PackageRoot\ServiceManifest.xml %CURRENT_FOLDER%\pkg\%CONFIGURATION%\Counter\

dotnet restore %CURRENT_FOLDER%..\Gateway\
dotnet publish %CURRENT_FOLDER%..\Gateway\ -c %CONFIGURATION% -f %FRAMEWORK%
robocopy /MIR %CURRENT_FOLDER%..\Gateway\bin\%CONFIGURATION%\%FRAMEWORK%\ %CURRENT_FOLDER%\pkg\%CONFIGURATION%\Gateway\Code\
copy /Y %CURRENT_FOLDER%..\Gateway\PackageRoot\ServiceManifest.xml %CURRENT_FOLDER%\pkg\%CONFIGURATION%\Gateway\

dotnet restore %CURRENT_FOLDER%..\Sms\
dotnet publish %CURRENT_FOLDER%..\Sms\ -c %CONFIGURATION% -f %FRAMEWORK%
robocopy /MIR %CURRENT_FOLDER%..\Sms\bin\%CONFIGURATION%\%FRAMEWORK%\ %CURRENT_FOLDER%\pkg\%CONFIGURATION%\Sms\Code\
copy /Y %CURRENT_FOLDER%..\Sms\PackageRoot\ServiceManifest.xml %CURRENT_FOLDER%\pkg\%CONFIGURATION%\Sms\

copy /Y %CURRENT_FOLDER%ApplicationManifest.xml %CURRENT_FOLDER%\pkg\%CONFIGURATION%\