# Zigm - Zig Version Manager Detailed Documentation

## Language Selection
- [中文文档](README.md)
- [English Documentation](README-en.md)

## Project Introduction
Zigm is a Zig programming language version management tool developed in C#, similar to nvm or pyenv. It helps developers easily download, install, and manage different versions of the Zig compiler. With Zigm, you can quickly switch between different Zig versions, making version testing and compatibility verification more convenient.

### Core Features
- ✅ Automatically detects current system architecture and operating system
- ✅ Supports official Zig JSON API for version information
- ✅ Supports detection of system-installed Zig (including Windows non-environment variable installations)
- ✅ Supports installation and management of multiple Zig versions
- ✅ Supports quick version switching via environment variables
- ✅ Supports multiple languages (Chinese, English, Traditional Chinese)
- ✅ AOT-compatible JSON serialization
- ✅ Configuration files localized to program directory
- ✅ Interactive command-line interface
- ✅ Supports user-level and system-level environment variable settings

## System Requirements

| Platform | Minimum Requirements | Recommended Configuration |
|----------|----------------------|---------------------------|
| **Windows** | Windows (Aot supported version, follows .Net Aot) | Windows 11 |
| **Linux** | Ubuntu (Aot supported version, follows .Net Aot) | Ubuntu 22.04+ |
| **macOS** | macOS (Aot supported version, follows .Net Aot) | macOS 14+ |
| **.NET** | .NET 10.0 | .NET 10.0+ |
| **Architecture** | x64 | x64/arm64 |

## Installation Methods

### 1. Install from Release Package (Recommended)

1. Download the release package for your platform from GitHub Releases
2. Extract to any directory
3. Add the extracted directory to your system PATH environment variable
4. Run `zigm help` to verify installation success

### 2. Compile from Source Code

```bash
# Clone the project
git clone <repository-url>
cd Zigm

# Build the project
dotnet build -c Release

# Publish executable files
dotnet publish -c Release -r <runtime-identifier> --self-contained true

# Example: Publish Windows x64 version
dotnet publish -c Release -r win-x64 --self-contained true
```

#### Supported Runtime Identifiers
| Platform | Architecture | Runtime Identifier |
|----------|--------------|--------------------|
| Windows | x64 | win-x64 |
| Windows | arm64 | win-arm64 |
| Linux | x64 | linux-x64 |
| Linux | arm64 | linux-arm64 |
| macOS | x64 | osx-x64 |
| macOS | arm64 | osx-arm64 |

## Quick Start

### 1. View Help Information
```bash
zigm help
```

### 2. List Available Zig Versions
```bash
zigm list
```

### 3. Install a Specific Version
```bash
zigm install 0.12.0
```

### 4. Switch to Installed Version
```bash
# User-level switch (current user only)
zigm use 0.12.0

# System-level switch (all users, requires administrator privileges)
zigm use 0.12.0 --system
```

### 5. View Current Version
```bash
zigm current
```

### 6. Launch Interactive Interface
```bash
zigm start
```

## Detailed Command Line Instructions

### 1. Launch Interactive Interface
```bash
zigm start
```
- Launches a console-based interactive menu
- Supports up/down arrow keys for selection
- Supports visual operation of all command-line functions

### 2. List Available Versions
```bash
zigm list
```
- Fetches and displays all available stable Zig versions
- Automatically filters versions suitable for the current system and architecture
- Displays version numbers and types

### 3. List Local Versions
```bash
zigm ls
```
- Displays all installed Zig versions
- Marks the currently used version
- Displays Zig versions detected on the system (non-Zigm managed)

### 4. Install Specific Version
```bash
zigm install <version>
```
- Downloads and installs the specified Zig version
- Automatically selects the appropriate installation package for the current system
- Supports semantic versioning

**Examples:**
```bash
zigm install 0.12.0
zigm install 0.11.0
```

### 5. Switch Versions
```bash
zigm use <version> [--system]
```
- Switches to the specified Zig version
- Updates the system PATH environment variable to use the specified version
- `--system` option: Sets system-level environment variables (requires administrator privileges)

**Examples:**
```bash
# User-level switch
zigm use 0.12.0

# System-level switch
zigm use 0.12.0 --system
```

### 6. Uninstall Version
```bash
zigm uninstall <version>
```
- Uninstalls the specified Zig version
- Automatically cleans up related files
- Clears version settings if uninstalling the current version

**Example:**
```bash
zigm uninstall 0.11.0
```

### 7. Display Current Version
```bash
zigm current
```
- Displays the currently used Zig version
- Shows a prompt if no version is set

### 8. Update Zigm
```bash
zigm update
```
- Checks for and updates Zigm to the latest version
- Automatically downloads and replaces the current executable file

### 9. Configuration Management
```bash
zigm config [show|set|reset]
```
- Manages Zigm configuration settings

**Subcommands:**
- `show`: Displays current configuration
- `set <key> <value>`: Sets configuration item
- `reset`: Resets configuration to default values

**Examples:**
```bash
# Show configuration
zigm config show

# Set language to English
zigm config set Language en

# Set download timeout to 120 seconds
zigm config set DownloadTimeout 120

# Reset configuration
zigm config reset
```

### 10. Show Help
```bash
zigm help
```
- Displays all supported commands and descriptions
- Provides usage examples

## Configuration Management

### Configuration File Location
The configuration file is located in the `ZigmConfig` folder under the program directory:
- **Windows**: `[Program Directory]\ZigmConfig\config.json`
- **Linux/macOS**: `[Program Directory]/ZigmConfig/config.json`

### Configuration Item Details

| Configuration Item | Type | Default Value | Description |
|-------------------|------|---------------|-------------|
| StoragePath | string | `ZigmConfig/zig-versions` | Zig version storage directory |
| AutoCheckUpdates | bool | `true` | Whether to automatically check for Zigm updates |
| CurrentVersion | string | `null` | Currently used Zig version |
| DownloadTimeout | int | `300` | Download timeout in seconds |
| DefaultSource | string | `official` | Default Zig version source |
| DownloadSource | string | `https://ziglang.org/download/` | Download source URL (can be a mirror) |
| Language | string | `null` | Interface language setting |

### Configuration Example
```json
{
  "StoragePath": "C:\\Program Files\\Zigm\\ZigmConfig\\zig-versions",
  "AutoCheckUpdates": true,
  "CurrentVersion": "0.12.0",
  "DownloadTimeout": 300,
  "DefaultSource": "official",
  "DownloadSource": "https://ziglang.org/download/",
  "Language": "zh-Hant"
}
```

## Multi-language Support

### Supported Languages
- **Chinese** (default, zh-CN)
- **English** (en, en-US)
- **Traditional Chinese** (zh-Hant)

### Switching Languages
```bash
# Switch to English
zigm config set Language en

# Switch to Traditional Chinese
zigm config set Language zh-Hant

# Restore default language (Chinese)
zigm config set Language zh-CN
```

### Automatic Language Detection
- If no Language configuration is set, the system automatically uses the current operating system language setting
- Supported languages are automatically switched, unsupported languages use the default language (Chinese)

## Advanced Features

### 1. System Zig Detection
- Automatically detects Zig versions installed on the system
- Displays in the output of `zigm ls` command
- Supports detection of Zig installations from Windows registry (non-environment variable)

### 2. Environment Variable Management
- Automatically updates system PATH environment variable
- Supports user-level and system-level environment variable settings
- Takes effect immediately after installation

### 3. Official JSON API Support
- Uses Zig's official JSON API to fetch version information
- Ensures accuracy and reliability of version information
- Automatically filters versions suitable for the current system

### 4. AOT Compatibility
- Uses System.Text.Json source generation for AOT compatibility
- Supports publishing as standalone executable files
- Reduces runtime dependencies

### 5. Configuration File Localization
- Configuration files are stored in the program directory, not occupying system disk space
- Easy to migrate and backup
- Avoids user C drive accumulation

## Project Structure

```
Zigm/
├── ArgsConstant/          # Command line parameter definitions
│   └── ArgsFirst.cs       # Main parameter processing class
├── Doc/                   # Documentation directory
│   ├── README.md          # Chinese documentation
│   └── README-en.md       # English documentation
├── Helpers/               # Utility classes
│   └── SystemHelper.cs    # System auxiliary functions
├── Interactive/           # Interactive interface
│   └── InteractiveMenu.cs # Interactive menu implementation
├── Languages/             # Multi-language resources
│   ├── AppLang.Designer.cs       # Resource designer
│   ├── AppLang.en-US.resx        # English resources (US)
│   ├── AppLang.en.resx           # English resources
│   ├── AppLang.resx              # Chinese resources
│   └── AppLang.zh-Hant.resx      # Traditional Chinese resources
├── Models/                # Data models
│   ├── Config.cs           # Configuration model
│   ├── ConfigJsonContext.cs      # Configuration JSON context (AOT)
│   ├── ZigJsonContext.cs         # Zig version JSON context (AOT)
│   └── ZigVersion.cs       # Zig version model
├── Services/              # Business services
│   ├── ConfigService.cs           # Configuration service
│   ├── EnvironmentService.cs      # Environment variable service
│   ├── LocalStorageService.cs     # Local storage service
│   ├── SystemZigService.cs        # System Zig detection service
│   ├── ZigInstallerService.cs     # Zig installation service
│   ├── ZigVersionService.cs       # Zig version service
│   └── ZigmUpdaterService.cs      # Zigm update service
├── Program.cs             # Main entry file
└── Zigm.csproj            # Project configuration file
```

## Common Questions

### 1. What to do if installation fails?
- Check if network connection is normal
- Ensure write permissions to the installation directory
- Try increasing the download timeout by modifying the DownloadTimeout configuration item
- View detailed error messages and report

### 2. Why doesn't version switching take effect?
- Close and reopen the terminal window
- Check if environment variables are set correctly
- Try running the switch command with administrator privileges

### 3. How to use a mirror source?
```bash
zigm config set DownloadSource <mirror-url>
```

### 4. What to do if the configuration file is lost?
- Rerun Zigm, which will automatically generate a default configuration file
- Use `zigm config reset` to reset the configuration

### 5. How to uninstall Zigm?
- Delete the Zigm program directory
- Remove the Zigm path from environment variables
- Delete the ZigmConfig folder

## Contribution Guide

Contributions to Zigm are welcome! Please follow these steps:

1. Fork the project repository
2. Create a feature branch
3. Commit your changes
4. Push the branch
5. Submit a Pull Request

## License

This project is licensed under the MIT License. See the LICENSE file for details.

## Changelog

### 0.1.0 (In Development)
- Initial version
- Basic Zig version management functionality
- Interactive interface support
- Multi-language support
- System Zig detection
- AOT compatibility

## Contact Information

For questions or suggestions, please contact via:
- GitHub Issues: <repository-issues-url>

---

**Zigm - Making Zig Version Management Easier!**