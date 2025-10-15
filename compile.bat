@echo off
echo Notes Application - .NET 9 Build Script
echo ========================================
echo.

REM Check if dotnet SDK is available
where dotnet >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo ERROR: .NET SDK not found!
    echo Please install .NET 9 SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

REM Display .NET version
echo Checking .NET SDK version...
dotnet --version
echo.

REM Check if icon exists
if not exist "Resources\Notes.ico" (
    echo WARNING: Icon file not found at Resources\Notes.ico
    echo The build will continue without a custom icon.
    echo.
)

REM Clean previous build artifacts
echo Cleaning previous build artifacts...
if exist "bin\Release" rmdir /s /q "bin\Release"
if exist "obj\Release" rmdir /s /q "obj\Release"
echo.

REM Build the project in Release mode
echo Building Notes application...
echo Configuration: Release
echo Platform: x64
echo Target Framework: net9.0-windows
echo.

dotnet build Notes.csproj --configuration Release --verbosity minimal

if %ERRORLEVEL% neq 0 (
    echo.
    echo ✗ Build failed!
    echo Check the error messages above for details.
    pause
    exit /b 1
)

echo.
echo ========================================
echo Publishing self-contained executable...
echo ========================================
echo.

REM Create publish directory
if not exist "publish" mkdir publish

REM Publish as a single-file, self-contained executable
dotnet publish Notes.csproj ^
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
    echo ✗ Publishing failed!
    echo Check the error messages above for details.
    pause
    exit /b 1
)

echo.
echo ╔════════════════════════════════════════════════════════════════╗
echo ║                   ✓ BUILD SUCCESSFUL!                          ║
echo ╠════════════════════════════════════════════════════════════════╣
echo ║                                                                ║
echo ║  Output locations:                                             ║
echo ║  • Debug build:   bin\Debug\net9.0-windows\Notes.exe           ║
echo ║  • Release build: bin\Release\net9.0-windows\Notes.exe         ║
echo ║  • Published:     publish\Notes.exe (self-contained)           ║
echo ║                                                                ║
echo ║  The published executable includes:                            ║
echo ║  ✓ Embedded application icon                                   ║
echo ║  ✓ All dependencies bundled                                    ║
echo ║  ✓ Ready to run on any Windows 10+ x64 machine                 ║
echo ║  ✓ No .NET runtime installation required                       ║
echo ║                                                                ║
echo ╚════════════════════════════════════════════════════════════════╝
echo.

REM Display file sizes
echo File Information:
for %%F in ("publish\Notes.exe") do echo  • publish\Notes.exe - %%~zF bytes
echo.

echo Ready to distribute: publish\Notes.exe
echo.
pause
