# Zigm - Zig版本管理工具

## 项目简介
Zigm是一个用C#开发的Zig语言版本管理工具，类似于nvm或pyenv，旨在帮助开发者轻松下载、安装和管理不同版本的Zig编译器。通过Zigm，你可以快速切换不同版本的Zig，方便进行版本测试和兼容性验证。

## 系统要求
- **操作系统**：Windows 10/11、Linux（Ubuntu 20.04+、CentOS 8+等）
- **.NET版本**：.NET 10.0或更高版本
- **MacOS**：需自行编译（项目代码支持MacOS）

## 安装方法

### 从源代码编译
1. 克隆项目仓库
   ```bash
   git clone <repository-url>
   cd Zigm
   ```

2. 编译项目
   ```bash
   dotnet build -c Release
   ```

3. 发布可执行文件
   ```bash
   dotnet publish -c Release -r <runtime-identifier> --self-contained true
   ```
   其中`<runtime-identifier>`可以是：
   - Windows: win-x64, win-arm64
   - Linux: linux-x64, linux-arm64
   - MacOS: osx-x64, osx-arm64

4. 将发布目录添加到系统PATH，或在下载后在存放目录第一次使用时自动处理，添加到环境变量中（Windows可选择用户变量还是系统变量path）。
## 使用方法

### 命令行操作
Zigm支持以下命令行参数：

| 命令 | 描述 |
|------|------|
| `zigm start` | 启动控制台交互式操作界面 |
| `zigm list` | 列出所有可用的Zig版本（可下载） |
| `zigm ls` | 列出本地已安装的Zig版本 |
| `zigm install <version>` | 安装指定版本的Zig |
| `zigm use <version>` | 切换到指定版本的Zig |
| `zigm uninstall <version>` | 卸载指定版本的Zig |
| `zigm current` | 显示当前使用的Zig版本 |
| `zigm update` | 更新Zigm到最新版本 |
| `zigm help` | 显示帮助信息 |

### 交互式操作
通过`zigm start`命令启动交互式界面，你可以使用上下箭头键选择操作，按Enter键执行。

交互式界面提供以下选项：
- 查看可用Zig版本
- 查看本地已安装版本
- 安装新版本
- 切换版本
- 卸载版本
- 退出

## 项目结构

```
Zigm/
├── ArgsConstant/          # 命令行参数定义
│   └── ArgsFirst.cs       # 主参数处理类
├── Doc/                   # 文档目录
│   └── README.md          # 项目文档
├── Program.cs             # 主入口文件
└── Zigm.csproj            # 项目配置文件
```

## 贡献指南

欢迎提交Issue和Pull Request来帮助改进Zigm！

1. Fork项目仓库
2. 创建特性分支
3. 提交更改
4. 推送分支
5. 提交Pull Request

## 许可证

本项目采用MIT许可证，详见LICENSE文件。

## 联系方式

如有问题或建议，欢迎通过以下方式联系：
- GitHub Issues：<repository-issues-url>

## 更新日志

### 0.1.0 (开发中)
- 初始版本
- 支持基本的命令行参数处理
- 交互式界面框架

---

**Zigm - 让Zig版本管理更简单！**