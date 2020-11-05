@echo off

echo ""
echo "--run_netcore--"

setlocal ENABLEDELAYEDEXPANSION

set build_number=%1
if "%build_number%" == "" goto missing_params

set project=%2
if "%project%" == "" goto missing_params

set mypath=%cd%
echo %mypath%

REM OpenCover
set command=CodeCoverage\tools\OpenCover.4.6.519\tools\OpenCover.Console.exe -register:user -target:"dotnet.exe" -targetargs:"test %project% --no-build /p:DebugType=full" -filter:"+[GameUser*]* -[GameUser.*.Tests*]* -[GameUser.Tests*]*" -output:"UnitTestCoverage_%build_number%.xml" -oldstyle -mergeoutput
call %command%
if %errorlevel% neq 0 goto error

goto end

:missing_params
echo "missing params"
set /p temp="Hit enter to continue "
exit /b 1

:error
echo "error executing"
set /p temp="Hit enter to continue "
exit /b 1

:end
exit /b 0
