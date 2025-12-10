﻿﻿﻿using System.Globalization;
using Zigm.ArgsConstant;
using Zigm.Interactive;
using Zigm.Languages;
using Zigm.Services;

// 解析命令行参数
var (command, parameters) = ArgsFirst.ParseArgs(args);

// 创建服务实例
var configService = new ConfigService();
var config = configService.GetConfig();

// 设置应用程序语言 - 仅支持明确的资源文件语言代码
if (!string.IsNullOrEmpty(config.Language))
{
    // 在全球化不变模式下，我们直接设置CultureInfo，而不进行验证
    // 支持的语言代码：zh-CN (默认), en, en-US, zh-Hant
    try
    {
        // 直接设置文化，不进行验证，因为我们知道支持哪些语言
        AppLang.Culture = CultureInfo.GetCultureInfo(config.Language);
    }
    catch (Exception)
    {
    }
}

var systemZigService = new SystemZigService();
var zigVersionService = new ZigVersionService();
var localStorageService = new LocalStorageService(config);
var zigInstallerService = new ZigInstallerService(localStorageService, zigVersionService, config);
var zigmUpdaterService = new ZigmUpdaterService();

// 根据命令执行相应的操作
switch (command)
{
    case "start":
        Console.WriteLine(AppLang.启动交互式界面);
        var interactiveMenu = new InteractiveMenu(zigVersionService, localStorageService, zigInstallerService);
        await interactiveMenu.ShowMainMenuAsync();
        break;
    case "list":
        Console.WriteLine(AppLang.列出可用版本);
        Console.WriteLine(AppLang.获取版本信息);
        var versions = await zigVersionService.GetStableVersionsAsync();
        if (versions.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine(AppLang.可用版本);
            Console.WriteLine("---------------------------------");
            foreach (var version in versions)
            {
                Console.WriteLine($"  {version.Version} ({version.Type})");
            }
            Console.WriteLine("---------------------------------");
            Console.WriteLine(string.Format(AppLang.共找到版本, versions.Count));
        }
        else
        {
            Console.WriteLine(AppLang.未获取到版本);
        }
        break;
    case "ls":
        Console.WriteLine(AppLang.列出本地版本);
        var installedVersions = localStorageService.ListInstalledVersions();
        var currentVersion = localStorageService.GetCurrentVersion();

        // 显示系统中已安装的Zig信息
        systemZigService.ShowSystemZigInfo();
        Console.WriteLine();

        if (installedVersions.Count > 0)
        {
            Console.WriteLine(AppLang.Zigm管理的本地Zig版本);
            Console.WriteLine("---------------------------------");
            foreach (var version in installedVersions)
            {
                var isCurrent = version == currentVersion ? " *" : "";
                Console.WriteLine($"  {version}{isCurrent}");
            }
            Console.WriteLine("---------------------------------");
            Console.WriteLine(string.Format(AppLang.共安装了版本, installedVersions.Count));
            if (!string.IsNullOrEmpty(currentVersion))
            {
                Console.WriteLine(string.Format(AppLang.当前使用版本, currentVersion));
            }
        }
        else
        {
            Console.WriteLine(AppLang.Zigm未管理本地版本);
        }
        break;
    case "install":
        if (parameters.Count == 0)
        {
            Console.WriteLine(AppLang.请指定安装版本);
        }
        else
        {
            await zigInstallerService.InstallAsync(parameters[0]);
        }
        break;
    case "use":
        if (parameters.Count == 0)
        {
            Console.WriteLine(AppLang.请指定使用版本);
            Console.WriteLine(AppLang.可选参数系统级);
        }
        else
        {
            // 检查是否需要系统级安装
            EnvironmentVariableTarget target = EnvironmentVariableTarget.User;
            if (parameters.Count >= 2 && parameters[1].Equals("--system", StringComparison.OrdinalIgnoreCase))
            {
                target = EnvironmentVariableTarget.Machine;
            }
            zigInstallerService.SwitchToVersion(parameters[0], target);
        }
        break;
    case "uninstall":
        if (parameters.Count == 0)
        {
            Console.WriteLine(AppLang.请指定卸载版本);
        }
        else
        {
            zigInstallerService.Uninstall(parameters[0]);
        }
        break;
    case "current":
        Console.WriteLine(AppLang.显示当前版本);
        var current = localStorageService.GetCurrentVersion();
        if (!string.IsNullOrEmpty(current))
        {
            Console.WriteLine(string.Format(AppLang.当前使用版本, current));
        }
        else
        {
            Console.WriteLine(AppLang.未设置当前版本);
        }
        break;
    case "update":
        Console.WriteLine(AppLang.更新到最新版本);
        await zigmUpdaterService.UpdateAsync();
        break;
    case "config":
        if (parameters.Count == 0 || parameters[0] == "show")
        {
            configService.ShowConfig();
        }
        else if (parameters[0] == "set" && parameters.Count >= 3)
        {
            configService.SetConfigValue(parameters[1], parameters[2]);
        }
        else if (parameters[0] == "reset")
        {
            configService.ResetConfig();
        }
        else
        {
            Console.WriteLine(AppLang.无效的config命令用法);
            Console.WriteLine(AppLang.用法);
            Console.WriteLine("  zigm config show       - " + AppLang.显示当前配置);
            Console.WriteLine("  zigm config set <key> <value> - " + AppLang.设置配置项);
            Console.WriteLine("  zigm config reset      - " + AppLang.重置配置为默认值);
        }
        break;
    case "help":
        ArgsFirst.ShowHelp();
        break;
    default:
        ArgsFirst.ShowHelp();
        break;
}

// 防止控制台窗口立即关闭（仅在直接运行可执行文件时）
//if (Environment.UserInteractive && args.Length > 0)
//{
//    Console.WriteLine();
//    Console.WriteLine(AppLang.按任意键退出);
//    Console.ReadKey();
//}