# Zigm - Zig Version Manager

## Language Select
- [中文文档](README.md)
- [English Documentation](README-en.md)

## Project Introduction
Zigm is a Zig programming language version management tool developed in C#, similar to nvm or pyenv. It helps developers easily download, install, and manage different versions of the Zig compiler. With Zigm, you can quickly switch between different Zig versions, making version testing and compatibility verification more convenient.

## System Requirements
- **Operating System**: Windows 10/11, Linux (Ubuntu 20.04+, CentOS 8+, etc.)
- **.NET Version**: .NET 10.0 or higher
- **MacOS**: Requires manual compilation (project code supports MacOS)

## Installation Methods

### From Source Code Compilation
1. Clone the project repository
   ```bash
   git clone <repository-url>
   cd Zigm
   ```

2. Build the project
   ```bash
   dotnet build -c Release
   ```

3. Publish executable files
   ```bash
   dotnet publish -c Release -r <runtime-identifier> --self-contained true
   ```
   Where `<runtime-identifier>` can be:
   - Windows: win-x64, win-arm64
   - Linux: linux-x64, linux-arm64
   - MacOS: osx-x64, osx-arm64

4. Add the publish directory to the system PATH, or let Zigm automatically handle it when first used in the downloaded directory (Windows allows choosing between user variables or system variables for PATH).

## Usage

### Command Line Operations
Zigm supports the following command line parameters:

| Command | Description |
|---------|-------------|
| `zigm start` | Launch console interactive operation interface |
| `zigm list` | List all available Zig versions (downloadable) |
| `zigm ls` | List locally installed Zig versions |
| `zigm install <version>` | Install specified version of Zig |
| `zigm use <version>` | Switch to specified version of Zig |
| `zigm uninstall <version>` | Uninstall specified version of Zig |
| `zigm current` | Show currently used Zig version |
| `zigm update` | Update Zigm to the latest version |
| `zigm config [show\|set\|reset]` | Manage Zigm configuration |
| `zigm help` | Show help information |

### Interactive Operations
Launch the interactive interface via `zigm start` command. You can use up/down arrow keys to select operations and press Enter to execute.

The interactive interface provides the following options:
- View available Zig versions
- View locally installed versions
- Install new versions
- Switch versions
- Uninstall versions
- Exit

## Configuration Management

### Configuration File Location
The configuration file is located in the `ZigmConfig` folder under the program directory:
- **Windows**: `[Program Directory]\ZigmConfig\config.json`
- **Linux/macOS**: `[Program Directory]/ZigmConfig/config.json`

### Configuration Options

| Configuration Item | Type | Default Value | Description |
|-------------------|------|---------------|-------------|
| StoragePath | string | `ZigmConfig/zig-versions` | Zig version storage directory |
| AutoCheckUpdates | bool | `true` | Whether to automatically check for Zigm updates |
| CurrentVersion | string | `null` | Currently used Zig version |
| DownloadTimeout | int | `300` | Download timeout in seconds |
| DefaultSource | string | `official` | Default Zig version source |
| DownloadSource | string | `https://ziglang.org/download/` | Download source URL (can be a mirror) |
| Language | string | `null` | Localization language setting |

### Configuration Examples
```json
{
  "StoragePath": "C:\\Program Files\\Zigm\\ZigmConfig\\zig-versions",
  "AutoCheckUpdates": true,
  "CurrentVersion": "0.12.0",
  "DownloadTimeout": 300,
  "DefaultSource": "official",
  "DownloadSource": "https://ziglang.org/download/",
  "Language": "en"
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

## Advanced Features

### 1. System Zig Detection
- Automatically detects Zig versions installed on the system
- Displays in the output of `zigm ls` command
- Supports detecting Zig installations from Windows registry (non-environment variable)

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

## Contribution Guide

Contributions to Zigm are welcome! Please follow these steps:

1. Fork the project repository
2. Create a feature branch
3. Commit your changes
4. Push the branch
5. Submit a Pull Request

## License

This project is licensed under the MIT License. See the LICENSE file for details.

## Contact Information

For questions or suggestions, please contact via:
- GitHub Issues: <repository-issues-url>

## Changelog

### 0.1.0 (In Development)
- Initial version
- Basic command line parameter processing
- Interactive interface framework
- Multi-language support
- System Zig detection
- AOT compatibility

---

**Zigm - Making Zig Version Management Easier!**