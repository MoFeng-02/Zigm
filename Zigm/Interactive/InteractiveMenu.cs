namespace Zigm.Interactive;

using Zigm.Services;
using Zigm.Languages;

/// <summary>
/// 交互式菜单类，负责处理交互式界面的菜单显示和用户输入
/// </summary>
public class InteractiveMenu
{
    private readonly ZigVersionService _zigVersionService;
    private readonly LocalStorageService _localStorageService;
    private readonly ZigInstallerService _zigInstallerService;

    /// <summary>
    /// 菜单选项枚举
    /// </summary>
    private enum MenuOption
    {
        ListAvailableVersions = 1,
        ListInstalledVersions,
        InstallVersion,
        SwitchVersion,
        UninstallVersion,
        Exit
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="zigVersionService">Zig版本服务</param>
    /// <param name="localStorageService">本地存储服务</param>
    /// <param name="zigInstallerService">Zig安装服务</param>
    public InteractiveMenu(ZigVersionService zigVersionService, LocalStorageService localStorageService, ZigInstallerService zigInstallerService)
    {
        _zigVersionService = zigVersionService;
        _localStorageService = localStorageService;
        _zigInstallerService = zigInstallerService;
    }

    /// <summary>
    /// 显示主菜单
    /// </summary>
    public async Task ShowMainMenuAsync()
    {
        int selectedOption = 1;
        bool exit = false;

        while (!exit)
        {
            Console.Clear();
            DrawHeader();
            DrawMenu(selectedOption);

            // 获取用户输入
            var key = Console.ReadKey(true);

            // 处理用户输入
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    selectedOption = selectedOption > 1 ? selectedOption - 1 : Enum.GetValues<MenuOption>().Length;
                    break;
                case ConsoleKey.DownArrow:
                    selectedOption = selectedOption < Enum.GetValues<MenuOption>().Length ? selectedOption + 1 : 1;
                    break;
                case ConsoleKey.Enter:
                    exit = await HandleMenuOption((MenuOption)selectedOption);
                    break;
                case ConsoleKey.Escape:
                    exit = true;
                    break;
            }
        }
    }

    /// <summary>
    /// 绘制标题
    /// </summary>
    private void DrawHeader()
        {
            Console.WriteLine("=================================");
            Console.WriteLine($"         {AppLang.应用名称} - {AppLang.名称}        ");
            Console.WriteLine("=================================");
            Console.WriteLine();
        }

    /// <summary>
    /// 绘制菜单
    /// </summary>
    /// <param name="selectedOption">当前选中的选项</param>
    private void DrawMenu(int selectedOption)
        {
            var options = Enum.GetValues<MenuOption>();

            Console.WriteLine(AppLang.请选择操作);
            Console.WriteLine();

            for (int i = 1; i <= options.Length; i++)
            {
                var option = (MenuOption)i;
                var isSelected = i == selectedOption;
                var prefix = isSelected ? "> " : "  ";

                Console.WriteLine($"{prefix}{i}. {GetOptionDescription(option)}");
            }

            Console.WriteLine();
            Console.WriteLine(AppLang.使用上下箭头选择);
        }

    /// <summary>
    /// 获取选项描述
    /// </summary>
    /// <param name="option">菜单选项</param>
    /// <returns>选项描述</returns>
    private string GetOptionDescription(MenuOption option)
        {
            switch (option)
            {
                case MenuOption.ListAvailableVersions:
                    return AppLang.查看可用版本;
                case MenuOption.ListInstalledVersions:
                    return AppLang.查看已安装版本;
                case MenuOption.InstallVersion:
                    return AppLang.安装新版本;
                case MenuOption.SwitchVersion:
                    return AppLang.切换版本;
                case MenuOption.UninstallVersion:
                    return AppLang.卸载版本;
                case MenuOption.Exit:
                    return AppLang.退出;
                default:
                    return AppLang.未知选项;
            }
        }

    /// <summary>
    /// 处理菜单选项
    /// </summary>
    /// <param name="option">菜单选项</param>
    /// <returns>是否退出菜单</returns>
    private async Task<bool> HandleMenuOption(MenuOption option)
    {
        switch (option)
        {
            case MenuOption.ListAvailableVersions:
                await ListAvailableVersionsAsync();
                break;
            case MenuOption.ListInstalledVersions:
                ListInstalledVersions();
                break;
            case MenuOption.InstallVersion:
                await InstallVersionAsync();
                break;
            case MenuOption.SwitchVersion:
                SwitchVersion();
                break;
            case MenuOption.UninstallVersion:
                UninstallVersion();
                break;
            case MenuOption.Exit:
                return true;
        }

        return false;
    }

    /// <summary>
    /// 查看可用Zig版本
    /// </summary>
    private async Task ListAvailableVersionsAsync()
        {
            Console.Clear();
            DrawHeader();
            Console.WriteLine(AppLang.可用版本);
            Console.WriteLine("---------------------------------");

            var versions = await _zigVersionService.GetStableVersionsAsync();

            if (versions.Count > 0)
            {
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

            Console.WriteLine();
            Console.WriteLine(AppLang.按任意键返回主菜单);
            Console.ReadKey();
        }

    /// <summary>
    /// 查看本地已安装版本
    /// </summary>
    private void ListInstalledVersions()
        {
            Console.Clear();
            DrawHeader();
            Console.WriteLine(AppLang.Zigm管理的本地Zig版本);
            Console.WriteLine("---------------------------------");

            var installedVersions = _localStorageService.ListInstalledVersions();
            var currentVersion = _localStorageService.GetCurrentVersion();

            if (installedVersions.Count > 0)
            {
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
                Console.WriteLine(AppLang.本地未安装任何版本);
            }

            Console.WriteLine();
            Console.WriteLine(AppLang.按任意键返回主菜单);
            Console.ReadKey();
        }

    /// <summary>
    /// 安装新版本
    /// </summary>
    private async Task InstallVersionAsync()
        {
            Console.Clear();
            DrawHeader();
            Console.WriteLine(AppLang.安装Zig版本);
            Console.WriteLine("---------------------------------");
            Console.Write(AppLang.请输入要安装的版本号);

            var version = Console.ReadLine()?.Trim();

            if (!string.IsNullOrEmpty(version))
            {
                Console.WriteLine();
                await _zigInstallerService.InstallAsync(version);
            }
            else
            {
                Console.WriteLine(AppLang.版本号不能为空);
            }

            Console.WriteLine();
            Console.WriteLine(AppLang.按任意键返回主菜单);
            Console.ReadKey();
        }

    /// <summary>
    /// 切换版本
    /// </summary>
    private void SwitchVersion()
        {
            Console.Clear();
            DrawHeader();
            Console.WriteLine(AppLang.切换版本);
            Console.WriteLine("---------------------------------");

            var installedVersions = _localStorageService.ListInstalledVersions();

            if (installedVersions.Count == 0)
            {
                Console.WriteLine(AppLang.本地未安装任何版本);
            }
            else
            {
                Console.WriteLine(AppLang.可用的版本);
                for (int i = 0; i < installedVersions.Count; i++)
                {
                    Console.WriteLine($"  {i + 1}. {installedVersions[i]}");
                }

                Console.Write(AppLang.请选择要切换的版本序号);
                var input = Console.ReadLine();

                if (int.TryParse(input, out int index) && index >= 1 && index <= installedVersions.Count)
                {
                    var version = installedVersions[index - 1];
                    _zigInstallerService.SwitchToVersion(version);
                }
                else
                {
                    Console.WriteLine(AppLang.无效的选择);
                }
            }

            Console.WriteLine();
            Console.WriteLine(AppLang.按任意键返回主菜单);
            Console.ReadKey();
        }

    /// <summary>
    /// 卸载版本
    /// </summary>
    private void UninstallVersion()
    {
        Console.Clear();
        DrawHeader();
        Console.WriteLine(AppLang.卸载Zig版本);
        Console.WriteLine("---------------------------------");

        var installedVersions = _localStorageService.ListInstalledVersions();
        var currentVersion = _localStorageService.GetCurrentVersion();

        if (installedVersions.Count == 0)
        {
            Console.WriteLine(AppLang.本地未安装任何版本无法卸载);
        }
        else
        {
            Console.WriteLine(AppLang.可用的版本);
            for (int i = 0; i < installedVersions.Count; i++)
            {
                var version = installedVersions[i];
                var isCurrent = version == currentVersion ? $" ({AppLang.当前版本})" : "";
                Console.WriteLine($"  {i + 1}. {version}{isCurrent}");
            }

            Console.Write(AppLang.请选择要卸载的版本序号);
            var input = Console.ReadLine();

            if (int.TryParse(input, out int index) && index >= 1 && index <= installedVersions.Count)
            {
                var version = installedVersions[index - 1];
                _zigInstallerService.Uninstall(version);
            }
            else
            {
                Console.WriteLine(AppLang.无效的选择);
            }
        }

        Console.WriteLine();
        Console.WriteLine(AppLang.按任意键返回主菜单);
        Console.ReadKey();
    }
}
