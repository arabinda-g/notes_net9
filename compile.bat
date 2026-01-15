@echo off
echo Notes Application - .NET 9 Build Script
echo ========================================
echo.

REM Find dotnet CLI
set DOTNET_PATH=""

REM Check PATH first
where dotnet >nul 2>nul
if %ERRORLEVEL% equ 0 (
    set DOTNET_PATH=dotnet
    echo Found: dotnet CLI in PATH
    goto :use_dotnet
)

REM Check standard installation locations
if exist "C:\Program Files\dotnet\dotnet.exe" (
    set DOTNET_PATH="C:\Program Files\dotnet\dotnet.exe"
    echo Found: dotnet CLI at C:\Program Files\dotnet
    goto :use_dotnet
)

if exist "C:\Program Files (x86)\dotnet\dotnet.exe" (
    set DOTNET_PATH="C:\Program Files (x86)\dotnet\dotnet.exe"
    echo Found: dotnet CLI at C:\Program Files (x86)\dotnet
    goto :use_dotnet
)

echo ERROR: .NET SDK not found!
echo Please install .NET 9 SDK from https://dotnet.microsoft.com/download
pause
exit /b 1

:use_dotnet
echo.
echo Checking .NET SDK version...
%DOTNET_PATH% --version
echo.

REM Check if icon exists
if not exist "Resources\Notes.ico" (
    echo WARNING: Icon file not found at Resources\Notes.ico
    echo.
)

REM Clean previous build artifacts
echo Cleaning previous build artifacts...
if exist "bin\Release" rmdir /s /q "bin\Release"
if exist "obj\Release" rmdir /s /q "obj\Release"
echo.

REM Build the project in Release mode
echo Building Notes application...
%DOTNET_PATH% build Notes.csproj --configuration Release --verbosity minimal

if %ERRORLEVEL% neq 0 (
    echo.
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo Publishing self-contained executable...
echo.

if not exist "publish" mkdir publish

%DOTNET_PATH% publish Notes.csproj ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    --output publish ^
    /p:PublishSingleFile=true ^
    /p:PublishTrimmed=false ^
    /p:IncludeNativeLibrariesForSelfExtract=true ^
    /p:EnableCompressionInSingleFile=true ^
    --verbosity minimal

if %ERRORLEVEL% neq 0 (
    echo.
    echo Publishing failed!
    pause
    exit /b 1
)

echo.
echo ========================================
echo        BUILD SUCCESSFUL!
echo ========================================
echo.
echo Output locations:
echo   Release build: bin\Release\net9.0-windows\Notes.exe
echo   Published:     publish\Notes.exe (self-contained)
echo.
for %%F in ("publish\Notes.exe") do echo File size: %%~zF bytes
echo.
pause
