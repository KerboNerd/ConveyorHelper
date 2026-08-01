@echo off
setlocal EnableExtensions EnableDelayedExpansion

REM Parameters: NAME SOURCE [TFM] [PULSAR_OR_BIN64]
if "%~2"=="" (
    echo ERROR: Missing required parameters
    exit /b 1
)

set "NAME=%~1"
set "SOURCE=%~2"
set "TFM=%~3"
set "PULSAR_HINT=%~4"

if "!SOURCE:~-1!"=="\" set "SOURCE=!SOURCE:~0,-1!"
if not "!PULSAR_HINT!"=="" if "!PULSAR_HINT:~-1!"=="\" set "PULSAR_HINT=!PULSAR_HINT:~0,-1!"

set "SRCFILE=!SOURCE!\!NAME!"
if not exist "!SRCFILE!" (
    echo ERROR: Source not found: !SRCFILE!
    exit /b 1
)

REM Resolve Pulsar root:
REM   1) PULSAR env override
REM   2) Bin64 / portable root from MSBuild
REM   3) %AppData%\Pulsar
set "PULSAR_ENV=%PULSAR%"
set "PULSAR="

if not "!PULSAR_ENV!"=="" set "PULSAR=!PULSAR_ENV!"
if "!PULSAR!"=="" if not "!PULSAR_HINT!"=="" if exist "!PULSAR_HINT!\Legacy\" set "PULSAR=!PULSAR_HINT!"
if "!PULSAR!"=="" if not "!PULSAR_HINT!"=="" if exist "!PULSAR_HINT!\Interim\" set "PULSAR=!PULSAR_HINT!"
if "!PULSAR!"=="" if exist "%AppData%\Pulsar\Legacy\" set "PULSAR=%AppData%\Pulsar"
if "!PULSAR!"=="" if exist "%AppData%\Pulsar\Interim\" set "PULSAR=%AppData%\Pulsar"
if "!PULSAR!"=="" if not "!PULSAR_HINT!"=="" set "PULSAR=!PULSAR_HINT!"
if "!PULSAR!"=="" set "PULSAR=%AppData%\Pulsar"

set "EDITION=Interim"
echo(!TFM!| findstr /b /i "net4" >nul && set "EDITION=Legacy"
if "!TFM!"=="" set "EDITION=Legacy"

if /i "!EDITION!"=="Interim" goto deploy_interim
goto deploy_legacy

:deploy_interim
if not exist "!PULSAR!\Interim\" (
    echo Pulsar Interim not installed, skipping !TFM! deploy: !PULSAR!\Interim
    exit /b 0
)
set "PLUGIN_DIR=!PULSAR!\Interim\Local"
if not exist "!PLUGIN_DIR!\" mkdir "!PLUGIN_DIR!"
goto do_copy

:deploy_legacy
if not exist "!PULSAR!\Legacy\" (
    echo Pulsar Legacy not installed, skipping !TFM! deploy: !PULSAR!\Legacy
    exit /b 0
)
set "PLUGIN_DIR=!PULSAR!\Legacy\Local"
if not exist "!PLUGIN_DIR!\" mkdir "!PLUGIN_DIR!"
goto do_copy

:do_copy
echo Copying "!SRCFILE!" to "!PLUGIN_DIR!\"
copy /y "!SRCFILE!" "!PLUGIN_DIR!\"
if errorlevel 1 (
    echo WARNING: Could not copy "!NAME!" — file is probably locked by a running game/Pulsar.
    echo Build succeeded; close the game and rebuild to refresh the deployed plugin.
    exit /b 0
)

exit /b 0
