@echo off

setlocal ENABLEDELAYEDEXPANSION
set build_number=%1
if "%build_number%" == "" goto missing_build_number

REM create output folder
if not exist ..\cireport\testcoveragereport mkdir ..\cireport\testcoveragereport
if not exist ..\cireport\coberturareport mkdir ..\cireport\coberturareport

REM Set working dir
pushd ..\

set mypath=%cd%
echo %mypath%

call CodeCoverage\run_netcore.bat %build_number% GameUser.Tests

REM call CodeCoverage\run_netframework.bat %build_number% Fanex.NCB.Swagger\Fanex.NCB.Swagger.AuthGate.UnitTests\bin\Debug\Fanex.NCB.Swagger.AuthGate.UnitTests.dll

REM ReportGenerator
set command=CodeCoverage\tools\ReportGenerator.3.0.2\tools\reportgenerator.exe -reports:"UnitTestCoverage_%build_number%.xml" -targetdir:"cireport\testcoveragereport\"
call %command%
if %errorlevel% neq 0 goto error

REM CoberturaReport
set command=CodeCoverage\tools\OpenCoverToCoberturaConverter.0.3.4\tools\OpenCoverToCoberturaConverter.exe -input:"UnitTestCoverage_%build_number%.xml" -output:"cireport\coberturareport\UnitTestCobertura.xml" -sources:".\"
call %command%
if %errorlevel% neq 0 goto error

echo Success.
goto end

:missing_build_number
echo Please provide name of your test build number.
goto end

:error
echo Failure.

:end
set /p temp="Hit enter to continue "

popd