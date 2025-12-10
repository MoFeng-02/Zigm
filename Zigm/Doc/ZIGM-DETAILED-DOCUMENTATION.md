# Zigm - Zig版本管理工具详细文档

## 项目简介
Zigm是一个用C#开发的Zig语言版本管理工具，类似于nvm或pyenv，旨在帮助开发者轻松下载、安装和管理不同版本的Zig编译器。通过Zigm，你可以快速切换不同版本的Zig，方便进行版本测试和兼容性验证。

### 核心功能
- ✅ 自动检测当前系统架构和操作系统
- ✅ 支持官方Zig JSON API获取版本信息
- ✅ 支持系统已安装Zig的检测（包括Windows非环境变量安装）
- ✅ 支持多版本Zig的安装和管理
- ✅ 支持通过环境变量快速切换Zig版本
- ✅ 支持多语言（中文、英文、繁体中文）
- ✅ AOT兼容的JSON序列化
- ✅ 配置文件本地化到程序目录
- ✅ 交互式命令行界面
- ✅ 支持系统级和用户级环境变量设置

## 系统要求

| 平台 | 最低要求 | 推荐配置 |
|------|----------|----------|
| **Windows** | Windows 10/11 | Windows 11 |
| **Linux** | Ubuntu 20.04+/CentOS 8+ | Ubuntu 22.04+ |
| **macOS** | macOS 12+ | macOS 14+ |
| **.NET** | .NET 10.0 | .NET 10.0+ |
| **架构** | x64 | x64/arm64 |

## 安装方法

### 1. 从发布包安装（推荐）

1. 从GitHub Releases下载对应平台的发布包
2. 解压到任意目录
3. 将解压目录添加到系统PATH环境变量
4. 运行 `zigm help` 验证安装成功

### 2. 从源代码编译

```bash
# 克隆项目
git clone <repository-url>
cd Zigm

# 编译项目
dotnet build -c Release

# 发布可执行文件
dotnet publish -c Release -r <runtime-identifier> --self-contained true

# 示例：发布Windows x64版本
dotnet publish -c Release -r win-x64 --self-contained true
```

#### 支持的运行时标识符
| 平台 | 架构 | 运行时标识符 |
|------|------|--------------|
| Windows | x64 | win-x64 |
| Windows | arm64 | win-arm64 |
| Linux | x64 | linux-x64 |
| Linux | arm64 | linux-arm64 |
| macOS | x64 | osx-x64 |
| macOS | arm64 | osx-arm64 |

## 快速开始

### 1. 查看帮助信息
```bash
zigm help
```

### 2. 列出可用的Zig版本
```bash
zigm list
```

### 3. 安装指定版本
```bash
zigm install 0.12.0
```

### 4. 切换到已安装版本
```bash
# 用户级切换（仅当前用户）
zigm use 0.12.0

# 系统级切换（所有用户，需要管理员权限）
zigm use 0.12.0 --system
```

### 5. 查看当前使用的版本
```bash
zigm current
```

### 6. 启动交互式界面
```bash
zigm start
```

## 命令行指令详解

### 1. 启动交互式界面
```bash
zigm start
```
- 启动基于控制台的交互式菜单
- 支持使用上下箭头键选择操作
- 支持所有命令行功能的可视化操作

### 2. 列出可用版本
```bash
zigm list
```
- 获取并显示所有可用的Zig稳定版本
- 自动过滤当前系统和架构支持的版本
- 显示版本号和版本类型

### 3. 列出本地版本
```bash
zigm ls
```
- 显示所有已安装的Zig版本
- 标记当前正在使用的版本
- 显示系统中检测到的Zig版本（非Zigm管理）

### 4. 安装指定版本
```bash
zigm install <version>
```
- 下载并安装指定版本的Zig
- 自动选择适合当前系统的安装包
- 支持语义化版本号

**示例：**
```bash
zigm install 0.12.0
zigm install 0.11.0
```

### 5. 切换版本
```bash
zigm use <version> [--system]
```
- 切换到指定的Zig版本
- 更新环境变量PATH以使用指定版本
- `--system` 选项：设置系统级环境变量（需要管理员权限）

**示例：**
```bash
# 用户级切换
zigm use 0.12.0

# 系统级切换
zigm use 0.12.0 --system
```

### 6. 卸载版本
```bash
zigm uninstall <version>
```
- 卸载指定版本的Zig
- 自动清理相关文件
- 如果卸载当前版本，会清除版本设置

**示例：**
```bash
zigm uninstall 0.11.0
```

### 7. 显示当前版本
```bash
zigm current
```
- 显示当前正在使用的Zig版本
- 如果未设置，显示提示信息

### 8. 更新Zigm
```bash
zigm update
```
- 检查并更新Zigm到最新版本
- 自动下载并替换当前可执行文件

### 9. 配置管理
```bash
zigm config [show|set|reset]
```
- 管理Zigm的配置设置

**子命令：**
- `show`：显示当前配置
- `set <key> <value>`：设置配置项
- `reset`：重置配置为默认值

**示例：**
```bash
# 显示配置
zigm config show

# 设置语言为英文
zigm config set Language en

# 设置下载超时为120秒
zigm config set DownloadTimeout 120

# 重置配置
zigm config reset
```

### 10. 显示帮助
```bash
zigm help
```
- 显示所有支持的命令和描述
- 提供使用示例

## 配置管理

### 配置文件位置
配置文件位于程序目录下的 `ZigmConfig` 文件夹中：
- **Windows**：`[程序目录]\ZigmConfig\config.json`
- **Linux/macOS**：`[程序目录]/ZigmConfig/config.json`

### 配置项详解

| 配置项 | 类型 | 默认值 | 描述 |
|--------|------|--------|------|
| StoragePath | string | `ZigmConfig/zig-versions` | Zig版本存储目录 |
| AutoCheckUpdates | bool | `true` | 是否自动检查Zigm更新 |
| CurrentVersion | string | `null` | 当前使用的Zig版本 |
| DownloadTimeout | int | `300` | 下载超时时间（秒） |
| DefaultSource | string | `official` | 默认的Zig版本源 |
| DownloadSource | string | `https://ziglang.org/download/` | 下载源URL（可设置镜像源） |
| Language | string | `null` | 界面语言设置 |

### 配置示例
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

## 多语言支持

### 支持的语言
- **中文**（默认，zh-CN）
- **英文**（en, en-US）
- **繁体中文**（zh-Hant）

### 切换语言
```bash
# 切换到英文
zigm config set Language en

# 切换到繁体中文
zigm config set Language zh-Hant

# 恢复默认语言（中文）
zigm config set Language zh-CN
```

### 自动语言检测
- 如果未设置Language配置项，系统会自动使用当前操作系统的语言设置
- 支持的语言会自动切换，不支持的语言会使用默认语言（中文）

## 高级功能

### 1. 系统已安装Zig检测
- 自动检测系统中已安装的Zig版本
- 显示在 `zigm ls` 命令的输出中
- 支持检测Windows注册表中的Zig安装（非环境变量）

### 2. 环境变量管理
- 自动更新系统PATH环境变量
- 支持用户级和系统级环境变量设置
- 安装完成后立即生效

### 3. 官方JSON API支持
- 使用Zig官方提供的JSON API获取版本信息
- 确保版本信息的准确性和可靠性
- 自动过滤适合当前系统的版本

### 4. AOT兼容
- 使用System.Text.Json源生成实现AOT兼容
- 支持发布独立可执行文件
- 减少运行时依赖

### 5. 配置文件本地化
- 配置文件存储在程序目录下，不占用系统磁盘空间
- 便于迁移和备份
- 避免用户C盘堆积

## 项目结构

```
Zigm/
├── ArgsConstant/          # 命令行参数定义
│   └── ArgsFirst.cs       # 主参数处理类
├── Doc/                   # 文档目录
│   └── README.md          # 项目文档
├── Helpers/               # 工具类
│   └── SystemHelper.cs    # 系统辅助功能
├── Interactive/           # 交互式界面
│   └── InteractiveMenu.cs # 交互式菜单实现
├── Languages/             # 多语言资源
│   ├── AppLang.Designer.cs       # 资源设计器
│   ├── AppLang.en-US.resx        # 英文资源
│   ├── AppLang.en.resx           # 英文资源
│   ├── AppLang.resx              # 中文资源
│   └── AppLang.zh-Hant.resx      # 繁体中文资源
├── Models/                # 数据模型
│   ├── Config.cs           # 配置模型
│   ├── ConfigJsonContext.cs      # 配置JSON上下文（AOT）
│   ├── ZigJsonContext.cs         # Zig版本JSON上下文（AOT）
│   └── ZigVersion.cs       # Zig版本模型
├── Services/              # 业务服务
│   ├── ConfigService.cs           # 配置服务
│   ├── EnvironmentService.cs      # 环境变量服务
│   ├── LocalStorageService.cs     # 本地存储服务
│   ├── SystemZigService.cs        # 系统Zig检测服务
│   ├── ZigInstallerService.cs     # Zig安装服务
│   ├── ZigVersionService.cs       # Zig版本服务
│   └── ZigmUpdaterService.cs      # Zigm更新服务
├── Program.cs             # 主入口文件
└── Zigm.csproj            # 项目配置文件
```

## 常见问题

### 1. 安装失败怎么办？
- 检查网络连接是否正常
- 确保有权限写入安装目录
- 尝试修改DownloadTimeout配置项增加超时时间
- 查看详细错误信息并报告

### 2. 切换版本后不生效？
- 关闭并重新打开终端窗口
- 检查环境变量是否正确设置
- 尝试使用管理员权限运行切换命令

### 3. 如何使用镜像源？
```bash
zigm config set DownloadSource <mirror-url>
```

### 4. 配置文件丢失怎么办？
- 重新运行Zigm，会自动生成默认配置文件
- 使用`zigm config reset`重置配置

### 5. 如何卸载Zigm？
- 删除Zigm程序目录
- 删除环境变量中的Zigm路径
- 删除ZigmConfig文件夹

## 贡献指南

欢迎提交Issue和Pull Request来帮助改进Zigm！

1. Fork项目仓库
2. 创建特性分支
3. 提交更改
4. 推送分支
5. 提交Pull Request

## 许可证

本项目采用MIT许可证，详见LICENSE文件。

## 更新日志

### 0.1.0（开发中）
- 初始版本
- 支持基本的Zig版本管理功能
- 交互式界面支持
- 多语言支持
- 系统Zig检测
- AOT兼容

## 联系方式

如有问题或建议，欢迎通过以下方式联系：
- GitHub Issues：<repository-issues-url>

---

**Zigm - 让Zig版本管理更简单！**