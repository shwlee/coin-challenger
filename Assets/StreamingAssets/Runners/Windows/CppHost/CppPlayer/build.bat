@echo off

set SCRIPT_DIR=%~dp0
set OUTPUT_NAME=%1

if not exist "%SCRIPT_DIR%result" (
    mkdir "%SCRIPT_DIR%result"
)

cd /d "%SCRIPT_DIR%result"

del "%SCRIPT_DIR%result\CppBuilder.dir\Release\CppPlayer.obj"
del "%SCRIPT_DIR%result\libCppBuilder.dll"

cmake ..
cmake --build . --config Release

copy "%SCRIPT_DIR%result\Release\CppBuilder.dll" "%SCRIPT_DIR%result\Release\%OUTPUT_NAME%.dll"
