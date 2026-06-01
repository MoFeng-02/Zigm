using System.Security.Principal;
using Zigm.Languages;
using Zigm.Services.Interfaces;

namespace Zigm.Services;

/// <summary>
/// 环境服务类，负责处理环境变量的修改
/// </summary>
public class EnvironmentService : IEnvironmentService
{
    private readonly ILocalStorageService _localStorageService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="localStorageService">本地存储服务</param>
    public EnvironmentService(ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }

    /// <summary>
    /// 获取系统的PATH环境变量
    /// </summary>
    /// <param name="target">环境变量目标（用户或系统）</param>
    /// <returns>PATH环境变量的值</returns>
    public string GetSystemPath(EnvironmentVariableTarget target = EnvironmentVariableTarget.User)
    {
        return Environment.GetEnvironmentVariable("PATH", target) ?? string.Empty;
    }

    /// <summary>
    /// 设置系统的PATH环境变量
    /// </summary>
    /// <param name="newPath">新的PATH环境变量值</param>
    /// <param name="target">环境变量目标（用户或系统）</param>
    public void SetSystemPath(string newPath, EnvironmentVariableTarget target = EnvironmentVariableTarget.User)
    {
        try
        {
            Environment.SetEnvironmentVariable("PATH", newPath, target);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new UnauthorizedAccessException(
                GetPermissionErrorMessage(target), ex);
        }
        catch (System.Security.SecurityException ex)
        {
            throw new System.Security.SecurityException(
                GetPermissionErrorMessage(target), ex);
        }
    }

    /// <summary>
    /// 使用符号链接将指定版本的Zig设置为当前激活版本
    /// 这种方案只会将一个固定的、短的 bin 目录添加到 PATH，避免 PATH 不断变长
    /// </summary>
    /// <param name="version">Zig版本号</param>
    /// <param name="target">环境变量目标（用户或系统）</param>
    /// <returns>是否成功设置</returns>
    public bool AddZigToPath(string version, EnvironmentVariableTarget target = EnvironmentVariableTarget.User)
    {
        try
        {
            if (target == EnvironmentVariableTarget.Machine && OperatingSystem.IsWindows())
            {
                if (!CheckAdminPrivileges())
                {
                    DisplayAdminPrivilegeError(target);
                    return false;
                }
            }

            var zigPath = _localStorageService.GetVersionPath(version);
            string zigExeName = OperatingSystem.IsWindows() ? "zig.exe" : "zig";
            var zigExePath = Path.Combine(zigPath, zigExeName);
            
            if (!File.Exists(zigExePath))
            {
                Console.WriteLine(string.Format(AppLang.找不到Zig可执行文件, zigExePath));
                return false;
            }
            
            var binPath = _localStorageService.GetBinPath();
            
            // 确保 bin 目录在 PATH 中（只加一次）
            EnsureBinInPath(binPath, target);
            
            // 创建符号链接
            var symlinkPath = Path.Combine(binPath, zigExeName);
            CreateOrUpdateSymlink(symlinkPath, zigExePath);
            
            if (OperatingSystem.IsWindows())
            {
                string targetStr = target == EnvironmentVariableTarget.Machine ? AppLang.系统级 : AppLang.用户级;
                Console.WriteLine(string.Format(AppLang.已将Zig添加到系统PATH中, version, targetStr));
                Console.WriteLine(AppLang.运行完毕后重启PowerShell实例);
            }
            else
            {
                Console.WriteLine(string.Format(AppLang.已将Zig添加到PATH中, version));
                Console.WriteLine(AppLang.完成后请重新加载启动文件或重启Shell);
            }
            
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            DisplayAdminPrivilegeError(target);
            return false;
        }
        catch (System.Security.SecurityException)
        {
            DisplayAdminPrivilegeError(target);
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(string.Format(AppLang.修改环境变量失败, ex.Message));
            return false;
        }
    }

    /// <summary>
    /// 确保 Zigm 的 bin 目录在 PATH 中，避免重复添加
    /// </summary>
    private void EnsureBinInPath(string binPath, EnvironmentVariableTarget target)
    {
        var currentPath = GetSystemPath(target);
        var pathSegments = currentPath.Split(Path.PathSeparator).ToList();
        
        // 检查 binPath 是否已经在 PATH 中
        var binInPath = pathSegments.Any(p => p.Equals(binPath, StringComparison.OrdinalIgnoreCase));
        
        if (!binInPath)
        {
            // 将 binPath 添加到 PATH 开头
            pathSegments.Insert(0, binPath);
            var newPath = string.Join(Path.PathSeparator, pathSegments);
            SetSystemPath(newPath, target);
        }
    }

    /// <summary>
    /// 创建或更新符号链接
    /// </summary>
    private void CreateOrUpdateSymlink(string symlinkPath, string targetPath)
    {
        // 删除旧的符号链接/文件（如果存在）
        if (File.Exists(symlinkPath))
        {
            File.Delete(symlinkPath);
        }
        
        // 创建新的符号链接
        File.CreateSymbolicLink(symlinkPath, targetPath);
    }

    /// <summary>
    /// 检查指定路径是否是Zigm管理的路径
    /// </summary>
    /// <param name="path">要检查的路径</param>
    /// <returns>如果是Zigm管理的路径返回true，否则返回false</returns>
    private bool IsZigmPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }
        
        // 检查路径是否包含Zigm的存储目录
        var zigmBasePath = Path.GetDirectoryName(_localStorageService.GetCurrentVersionPath());
        if (zigmBasePath != null && path.StartsWith(zigmBasePath, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// 刷新当前进程的环境变量
    /// </summary>
    /// <param name="target">环境变量目标（用户或系统）</param>
    public void RefreshEnvironment(EnvironmentVariableTarget target = EnvironmentVariableTarget.User)
    {
        // 刷新当前进程的环境变量
        var path = Environment.GetEnvironmentVariable("PATH", target);
        Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Process);
        
        Console.WriteLine(AppLang.环境变量已刷新);
    }
    
    /// <summary>
    /// 显示设置PATH环境变量的官方建议
    /// </summary>
    public void ShowOfficialPathSetupInfo()
    {
        Console.WriteLine(AppLang.官方PATH设置建议);
        Console.WriteLine("---------------------------------");
        
        if (OperatingSystem.IsWindows())
        {
            Console.WriteLine(AppLang.在Windows上设置PATH);
            Console.WriteLine(AppLang.要在Windows上设置PATH请在PowerShell中运行以下任一代码片段);
            Console.WriteLine();
            Console.WriteLine(AppLang.系统级安装管理员PowerShell);
            Console.WriteLine("[Environment]::SetEnvironmentVariable(");
            Console.WriteLine("   \"Path\",");
            Console.WriteLine("   [Environment]::GetEnvironmentVariable(\"Path\", \"Machine\") + \";C:\\\\your-path\\\\zig-windows-x86_64-your-version\",");
            Console.WriteLine("   \"Machine\"");
            Console.WriteLine(")");
            Console.WriteLine();
            Console.WriteLine(AppLang.用户级安装PowerShell);
            Console.WriteLine("[Environment]::SetEnvironmentVariable(");
            Console.WriteLine("   \"Path\",");
            Console.WriteLine("   [Environment]::GetEnvironmentVariable(\"Path\", \"User\") + \";C:\\\\your-path\\\\zig-windows-x86_64-your-version\",");
            Console.WriteLine("   \"User\"");
            Console.WriteLine(")");
            Console.WriteLine();
            Console.WriteLine(AppLang.运行完毕后重启PowerShell实例);
        }
        else
        {
            Console.WriteLine(AppLang.在LinuxMacOSBSD上设置PATH);
            Console.WriteLine(AppLang.将zig二进制镜像的位置添加到PATH环境变量中);
            Console.WriteLine(AppLang.这通常通过将export行添加到你的shell启动脚本);
            Console.WriteLine();
            Console.WriteLine("export PATH=$PATH:~/path/to/zig");
            Console.WriteLine();
            Console.WriteLine(AppLang.完成后请重新加载启动文件或重启Shell);
        }
        
        Console.WriteLine("---------------------------------");
    }

    /// <summary>
    /// 检查当前进程是否具有管理员权限（Windows）
    /// </summary>
    /// <returns>如果是管理员返回true，否则返回false</returns>
    public bool CheckAdminPrivileges()
    {
        if (!OperatingSystem.IsWindows())
        {
            return true;
        }

        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch (PlatformNotSupportedException)
        {
            return true;
        }
    }

    /// <summary>
    /// 检查是否具有修改指定目标环境变量的权限
    /// </summary>
    /// <param name="target">环境变量目标</param>
    /// <returns>可以修改返回true，否则返回false</returns>
    public bool CanModifyEnvironmentVariable(EnvironmentVariableTarget target)
    {
        if (target == EnvironmentVariableTarget.User)
        {
            return true;
        }

        if (target == EnvironmentVariableTarget.Machine)
        {
            if (!OperatingSystem.IsWindows())
            {
                return true;
            }

            return CheckAdminPrivileges();
        }

        return true;
    }

    /// <summary>
    /// 显示管理员权限错误消息和解决建议
    /// </summary>
    /// <param name="target">环境变量目标</param>
    private void DisplayAdminPrivilegeError(EnvironmentVariableTarget target)
    {
        Console.WriteLine();
        Console.WriteLine("=================================");
        Console.WriteLine(AppLang.错误权限不足);

        if (target == EnvironmentVariableTarget.Machine && OperatingSystem.IsWindows())
        {
            Console.WriteLine(AppLang.修改系统级环境变量需要管理员权限);
            Console.WriteLine();
            Console.WriteLine(AppLang.解决方案);
            Console.WriteLine(AppLang.以管理员身份运行提示);
            Console.WriteLine(AppLang.重新运行此命令);
            Console.WriteLine();
            Console.WriteLine(AppLang.或者);
            Console.WriteLine(AppLang.使用用户级安装提示);
            Console.WriteLine(AppLang.用户级安装说明);
        }
        else
        {
            Console.WriteLine(AppLang.修改系统环境变量需要管理员权限);
        }

        Console.WriteLine("=================================");
        Console.WriteLine();
    }

    /// <summary>
    /// 获取权限错误消息
    /// </summary>
    /// <param name="target">环境变量目标</param>
    /// <returns>错误消息</returns>
    private string GetPermissionErrorMessage(EnvironmentVariableTarget target)
    {
        if (target == EnvironmentVariableTarget.Machine && OperatingSystem.IsWindows())
        {
            return AppLang.修改系统级环境变量需要管理员权限;
        }

        return AppLang.修改系统环境变量需要管理员权限;
    }
}