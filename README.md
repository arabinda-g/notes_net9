# Notes Application - .NET 9.0

This is a modern Windows Forms note-taking application, fully migrated from .NET Framework 4.8 to .NET 9.0.

## Overview

The Notes application allows you to create, organize, and manage colorful sticky notes on your desktop. Each note can be customized with different colors, fonts, and positions.

## Features

- ✨ Create and manage multiple sticky notes
- 🎨 Customize note colors and fonts
- 📌 Position notes anywhere on screen
- 💾 Auto-save functionality
- 🌙 Light/Dark theme support (follows system theme)
- 🔍 Search and filter notes
- 📂 Import/Export notes
- 🔄 Undo/Redo support
- ⚙️ Comprehensive settings
- 📋 Quick copy to clipboard
- 🔔 System tray integration

## System Requirements

- **Operating System**: Windows 10 or later (64-bit)
- **.NET Runtime**: .NET 9.0 Runtime (Desktop)
- **Display**: Any resolution (optimized for high-DPI displays)

## Building from Source

### Prerequisites

1. Install [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
2. Install Visual Studio 2022 (17.8 or later) with the following workloads:
   - .NET desktop development
   - Or use Visual Studio Code with C# extension

### Build Instructions

#### Using Visual Studio 2022

1. Open `Notes-NET9.sln` in Visual Studio 2022
2. Select **Build > Build Solution** (or press `Ctrl+Shift+B`)
3. The executable will be in `Notes-NET9\bin\Debug\net9.0-windows\` or `Notes-NET9\bin\Release\net9.0-windows\`

#### Using Visual Studio Code

1. Open the project folder in Visual Studio Code
2. Install the **C#** (by Microsoft) — the official extension (ID: `ms-dotnettools.csharp`).
3. The project includes pre-configured debugging support

**To run in debug mode:**
- Press `F5` or select **Run > Start Debugging**
- Set breakpoints by clicking to the left of line numbers
- The application will build automatically and launch with the debugger attached

**Debug Controls:**
- `F5` - Start debugging / Continue
- `F9` - Toggle breakpoint
- `F10` - Step over
- `F11` - Step into
- `Shift+F11` - Step out
- `Shift+F5` - Stop debugging

**To build without debugging:**
- Press `Ctrl+Shift+B` to run the build task
- Or use the terminal commands below

#### Using Command Line

```powershell
# Navigate to the solution directory
cd Notes-NET9

# Restore NuGet packages
dotnet restore

# Build the application (Debug)
dotnet build

# Build the application (Release)
dotnet build -c Release

# Run the application
dotnet run

# Publish self-contained application
dotnet publish -c Release -r win-x64 --self-contained
```

### Build Configurations

- **Debug**: Includes debugging symbols, no optimizations
- **Release**: Optimized build, smaller size, better performance

## Migration from .NET Framework 4.8

This version has been completely migrated from .NET Framework 4.8 to .NET 9.0. Here are the key changes:

### What Changed

1. **Project File Format**
   - Migrated from old `.csproj` format to SDK-style project
   - Removed `packages.config`, now using `PackageReference`
   - Removed `AssemblyInfo.cs` (properties moved to `.csproj`)

2. **Framework APIs**
   - Updated to use `ApplicationConfiguration.Initialize()` instead of `Application.EnableVisualStyles()`
   - Enabled nullable reference types for better null safety
   - Updated NuGet package versions (Newtonsoft.Json 13.0.3)

3. **Language Features**
   - Enabled C# 12 features (implicit usings, nullable reference types)
   - Updated code to use modern C# patterns where applicable

4. **Settings Storage**
   - User settings are still stored in `%LOCALAPPDATA%\Notes`
   - Configuration files remain compatible between versions

### Breaking Changes

- **Settings Location**: While settings are compatible, they are stored in version-specific folders
- **Runtime Requirement**: Requires .NET 9.0 Desktop Runtime instead of .NET Framework 4.8

### Compatibility

✅ **User data is fully compatible** - You can copy your settings from the old version:

**Old location (Framework):**
```
%LOCALAPPDATA%\<Company>\Notes.exe_<hash>\<version>\user.config
```

**New location (.NET 9.0):**
```
%LOCALAPPDATA%\Notes\<version>\user.config
```

Simply copy your `user.config` file to migrate your notes and settings.

## Project Structure

```
Notes-NET9/
├── Notes.csproj              # SDK-style project file
├── Program.cs                # Application entry point
├── Library.cs                # Core library and utilities
├── Win32.cs                  # P/Invoke declarations
├── frmMain.cs                # Main form
├── frmAdd.cs                 # Add note dialog
├── frmEdit.cs                # Edit note dialog
├── frmSettings.cs            # Settings dialog
├── Properties/
│   ├── Settings.settings     # Application settings
│   ├── Settings.Designer.cs
│   ├── Resources.resx        # Embedded resources
│   └── Resources.Designer.cs
└── Resources/
    └── Notes.ico             # Application icon
```

## NuGet Packages

- **Newtonsoft.Json** (13.0.3) - JSON serialization/deserialization

## Development Notes

### Key Technologies

- **Windows Forms** - UI framework
- **Newtonsoft.Json** - Data persistence
- **Registry API** - System integration (startup, theme detection)

### Code Architecture

- **NotesLibrary**: Singleton pattern for configuration management
- **ThemeManager**: Centralized theme handling with system theme monitoring
- **Settings**: User preferences and data persistence
- **UnitStruct**: Note data structure

### Modern .NET Features Used

- Nullable reference types for null safety
- Pattern matching
- String interpolation
- Lambda expressions
- LINQ for data manipulation
- Async/await where applicable

## Configuration

Settings can be accessed via **Edit > Settings** menu:

- **General**: Auto-save, confirmations, theme
- **Hotkeys**: Global hotkey configuration
- **Window**: Startup state, position, always-on-top
- **Backup**: Automatic backup settings
- **Advanced**: Undo levels, animations, optimizations

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+N` | New note |
| `Ctrl+S` | Save |
| `Ctrl+R` | Reload |
| `Ctrl+Q` | Exit |
| `Ctrl+D` | Toggle movable mode |
| `Ctrl+Z` | Undo |
| `Ctrl+Y` | Redo |
| `F2` | Focus title (in edit dialog) |
| `F3` | Focus content (in edit dialog) |
| `Escape` | Cancel dialog |

## Troubleshooting

### Application doesn't start

1. Verify .NET 9.0 Desktop Runtime is installed:
   ```powershell
   dotnet --list-runtimes
   ```
2. Look for `Microsoft.WindowsDesktop.App 9.0.x`

### Settings not saved

- Check write permissions to `%LOCALAPPDATA%\Notes\`
- Run application as administrator (not recommended for regular use)

### High DPI issues

The application automatically handles DPI scaling. If you experience issues:
1. Right-click `Notes.exe` > Properties > Compatibility
2. Verify "Override high DPI scaling behavior" is **not** checked

## License

This application is provided as-is for personal and commercial use.

## Credits

- **Original Version**: .NET Framework 4.8
- **Migrated to**: .NET 9.0
- **JSON Library**: Newtonsoft.Json
- **Icons**: Custom

## Changelog

### Version 2.0.0 (.NET 9.0)
- ✨ Migrated to .NET 9.0
- ✨ Modern SDK-style project
- ✨ Nullable reference types enabled
- ✨ Updated NuGet packages
- ✨ Improved theme support
- 🐛 Various bug fixes and improvements

### Version 1.0.0 (.NET Framework 4.8)
- Initial release

## Support

For issues, feature requests, or questions, please create an issue in the repository.

---

**Happy note-taking! 📝**

