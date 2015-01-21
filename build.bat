@echo Off
set config=%1
if "%config%" == "" (
   set config=Release
)

if "%MsBuildExe%" == "" (
   set MsBuildExe=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe
)

if "%XUnit192Path%" == "" (
   set XUnit192Path=packages\xunit.runners.1.9.2\tools
)

if "%nuget%" == "" (
   set nuget=%USERPROFILE%\OneDrive\software\nuget.exe
)

set version=
rem if not "%PackageVersion%" == "" (
rem    set version=-Version %PackageVersion%
rem )

REM Package restore
%nuget% install .nuget\packages.config -OutputDirectory %cd%\packages -NonInteractive
%nuget% install ThinkTel.uControl.Api.Tests\packages.config -OutputDirectory %cd%\packages -NonInteractive

REM Build
mkdir build
"%MsBuildExe%" ThinkTel.uControl.Api.sln /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=build\msbuild.log;Verbosity=Normal /nr:false
if not "%errorlevel%"=="0" goto failure

REM Unit tests
%XUnit192Path%\xunit.console.clr4.exe ThinkTel.uControl.Api.Tests\bin\%config%\ThinkTel.uControl.Api.Tests.dll
if not "%errorlevel%"=="0" goto failure

REM Package
cmd /c %nuget% pack "ThinkTel.uControl.Api\ThinkTel.uControl.Api.nuspec" -symbols -o build -p Configuration=%config% %version%
if not "%errorlevel%"=="0" goto failure

:success
exit 0

:failure
exit -1
