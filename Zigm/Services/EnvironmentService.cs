using Zigm.Languages;

namespace Zigm.Services;

/// <summary>
/// 环境服务类，负责处理环境变量的修改
/// </summary>
public class EnvironmentService
{
    private readonly LocalStorageService _localStorageService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="localStorageService">本地存储服务</param>
    public EnvironmentService(LocalStorageService localStorageService)
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
        Environment.SetEnvironmentVariable("PATH", newPath, target);
    }

    /// <summary>
    /// 将指定版本的Zig添加到系统PATH中
    /// </summary>
    /// <param name="version">Zig版本号</param>
    /// <param name="target">环境变量目标（用户或系统）</param>
    /// <returns>是否成功添加</returns>
    public bool AddZigToPath(string version, EnvironmentVariableTarget target = EnvironmentVariableTarget.User)
    {
        try
        {
            // 获取指定版本的Zig路径
            var zigPath = _localStorageService.GetVersionPath(version);
            string zigExeName = OperatingSystem.IsWindows() ? "zig.exe" : "zig";
            var zigExePath = Path.Combine(zigPath, zigExeName);
            
            // 检查Zig可执行文件是否存在
            if (!File.Exists(zigExePath))
            {
                Console.WriteLine(string.Format(AppLang.找不到Zig可执行文件, zigExePath));
                return false;
            }
            
            // 获取当前PATH
            var currentPath = GetSystemPath(target);
            var pathSegments = currentPath.Split(Path.PathSeparator).ToList();
            
            // 移除旧的Zigm路径
            pathSegments = pathSegments.Where(p => !IsZigmPath(p)).ToList();
            
            // 添加新的Zig路径
            pathSegments.Insert(0, zigPath);
            
            // 设置新的PATH
            var newPath = string.Join(Path.PathSeparator, pathSegments);
            SetSystemPath(newPath, target);
            
            // 根据操作系统给出不同的提示
            if (OperatingSystem.IsWindows())
            {
                string targetStr = target == EnvironmentVariableTarget.Machine ? "系统级" : "用户级";
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
            Console.WriteLine(AppLang.修改系统环境变量需要管理员权限);
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(string.Format(AppLang.修改环境变量失败, ex.Message));
            return false;
        }
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
}