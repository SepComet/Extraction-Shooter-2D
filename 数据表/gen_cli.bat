@echo off
setlocal EnableExtensions

set "SCRIPT_DIR=%~dp0"
pushd "%SCRIPT_DIR%" >nul || exit /b 1

set "PATH_FILE=%SCRIPT_DIR%path.txt"
set "LUBAN_DLL=%SCRIPT_DIR%Tools\Luban\Luban.dll"
set "CONF_FILE=%SCRIPT_DIR%luban.conf"
set "CUSTOM_TEMPLATE_DIR=%SCRIPT_DIR%CustomTemplates"

if not exist "%PATH_FILE%" (
    echo [ERROR] Missing output path file: %PATH_FILE%
    popd
    exit /b 1
)

if not exist "%LUBAN_DLL%" (
    echo [ERROR] Missing Luban executable: %LUBAN_DLL%
    popd
    exit /b 1
)

set /p "path_content="<"%PATH_FILE%"
if not defined path_content (
    echo [ERROR] path.txt must contain a non-empty output directory.
    popd
    exit /b 1
)

if defined LUBAN_OUTPUT_ROOT (
    set "output_root_input=%LUBAN_OUTPUT_ROOT%"
) else (
    set "output_root_input=%path_content%"
)

for %%I in ("%SCRIPT_DIR%..") do set "PROJECT_ROOT=%%~fI"
for %%I in ("%output_root_input%") do set "OUTPUT_ROOT=%%~fI"

set "ASSET_ROOT=%PROJECT_ROOT%\Assets"
set "PATH_GEN_CSHARP=%OUTPUT_ROOT%\CSharp"
set "PATH_DATA_JSON=%OUTPUT_ROOT%\Json"
set "PATH_DATA_BIN=%OUTPUT_ROOT%\Bin"

if not exist "%PATH_GEN_CSHARP%" mkdir "%PATH_GEN_CSHARP%"
if not exist "%PATH_DATA_JSON%" mkdir "%PATH_DATA_JSON%"
if not exist "%PATH_DATA_BIN%" mkdir "%PATH_DATA_BIN%"

echo [INFO] Project root: %PROJECT_ROOT%
echo [INFO] C# output:   %PATH_GEN_CSHARP%
echo [INFO] JSON output: %PATH_DATA_JSON%
echo [INFO] Bin output:  %PATH_DATA_BIN%

dotnet "%LUBAN_DLL%" ^
    -t client ^
    -c cs-bin ^
    -d json ^
    -d bin ^
    --conf "%CONF_FILE%" ^
    --customTemplateDir "%CUSTOM_TEMPLATE_DIR%" ^
    -x "cs-bin.outputCodeDir=%PATH_GEN_CSHARP%" ^
    -x "json.outputDataDir=%PATH_DATA_JSON%" ^
    -x "bin.outputDataDir=%PATH_DATA_BIN%" ^
    -x "pathValidator.rootDir=%ASSET_ROOT%"

set "exit_code=%ERRORLEVEL%"
if not "%exit_code%"=="0" (
    echo [ERROR] Luban export failed with exit code %exit_code%.
    popd
    exit /b %exit_code%
)

echo [INFO] Luban export completed.
popd
exit /b 0
