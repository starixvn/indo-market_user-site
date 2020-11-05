@echo off

echo ""
echo "--run_framework--"

setlocal ENABLEDELAYEDEXPANSION

set build_number=%1
if "%build_number%" == "" goto missing_params

set project=%2
if "%project%" == "" goto missing_params

REM OpenCover
set command=CodeCoverage\tools\OpenCover.4.6.519\tools\OpenCover.Console.exe -register:user -target:"CodeCoverage\tools\xunit.runner.console.2.3.0\tools\net452\xunit.console.exe"  -targetargs:"%project% -nologo -noshadow -quiet" -filter:"+[GamesAdmin*]* -[GamesAdmin.*.UnitTests*]*" -output:"UnitTestCoverage_%build_number%.xml" -mergeoutput
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