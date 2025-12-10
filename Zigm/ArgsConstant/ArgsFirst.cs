﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using Zigm.Languages;

namespace Zigm.ArgsConstant;

/// <summary>
/// 命令行参数处理类，负责定义和解析命令行参数
/// </summary>
public class ArgsFirst
{
    /// <summary>
    /// 支持的命令列表
    /// </summary>
    public static readonly Dictionary<string, string> SupportedCommands = new Dictionary<string, string>
    {
        { "start", AppLang.启动交互式界面 },
        { "list", AppLang.列出可用版本 },
        { "ls", AppLang.列出本地版本 },
        { "install", "安装指定版本的Zig" },
        { "use", "切换到指定版本的Zig" },
        { "uninstall", "卸载指定版本的Zig" },
        { "current", AppLang.显示当前版本 },
        { "update", AppLang.更新到最新版本 },
        { "config", "管理Zigm配置" },
        { "help", "显示帮助信息" }
    };

    /// <summary>
    /// 解析命令行参数
    /// </summary>
    /// <param name="args">命令行参数数组</param>
    /// <returns>解析结果，包含命令和参数</returns>
    public static (string Command, List<string> Parameters) ParseArgs(string[] args)
    {
        if (args.Length == 0)
        {
            return ("help", new List<string>());
        }

        var command = args[0].ToLower();
        var parameters = args.Skip(1).ToList();

        // 验证命令是否支持
        if (!SupportedCommands.ContainsKey(command))
        {
            Console.WriteLine($"未知命令: {command}");
            return ("help", new List<string>());
        }

        return (command, parameters);
    }

    /// <summary>
    /// 显示帮助信息
    /// </summary>
    public static void ShowHelp()
    {
        Console.WriteLine($"{AppLang.应用名称} - Zig版本管理工具");
        Console.WriteLine();
        Console.WriteLine("使用方法: zigm [命令] [参数]");
        Console.WriteLine();
        Console.WriteLine("支持的命令:");
        Console.WriteLine();
        
        // 计算最大命令长度，用于对齐
        int maxCommandLength = SupportedCommands.Keys.Max(cmd => cmd.Length);
        
        foreach (var (command, description) in SupportedCommands)
        {
            Console.WriteLine($"  {command.PadRight(maxCommandLength)}  {description}");
        }
        
        Console.WriteLine();
        Console.WriteLine("示例:");
        Console.WriteLine("  zigm install 0.12.0    # 安装Zig 0.12.0版本");
        Console.WriteLine("  zigm use 0.11.0        # 切换到Zig 0.11.0版本");
        Console.WriteLine("  zigm list              # 列出所有可用的Zig版本");
        Console.WriteLine();
        Console.WriteLine("更多信息请访问项目仓库。");
    }
}
