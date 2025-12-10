using Zigm.Languages;

namespace Zigm.Services;

/// <summary>
/// 系统Zig服务类，负责检测系统中已安装的Zig版本
/// </summary>
public class SystemZigService
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public SystemZigService()
    {
    }

    /// <summary>
    /// 检查系统中是否安装了Zig
    /// </summary>
    /// <returns>如果安装了Zig返回true，否则返回false</returns>
    public bool IsZigInstalled()
    {
        try
        {
            // 先尝试通过命令行检查
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "zig",
                        Arguments = "version",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit(2000);
                
                if (process.ExitCode == 0)
                {
                    return true;
                }
            }
            catch
            {
                // 命令行检查失败，尝试查找本地文件
            }
            
            // 查找本地文件
            var zigPath = GetSystemZigPath();
            return !string.IsNullOrEmpty(zigPath);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 获取系统中安装的Zig版本
    /// </summary>
    /// <returns>Zig版本号，如果未安装则返回null</returns>
    public string? GetSystemZigVersion()
    {
        try
        {
            // 先尝试通过命令行获取
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "zig",
                        Arguments = "version",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(2000);
                
                if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                {
                    // 解析版本号，输出格式类似于 "zig 0.12.0"
                    return output;
                }
            }
            catch
            {
                // 命令行获取失败，尝试通过本地文件获取
            }
            
            // 通过本地文件获取版本号
            var zigPath = GetSystemZigPath();
            if (!string.IsNullOrEmpty(zigPath))
            {
                // 尝试执行找到的zig.exe获取版本
                try
                {
                    var process = new System.Diagnostics.Process
                    {
                        StartInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = zigPath,
                            Arguments = "version",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };

                    process.Start();
                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(2000);
                    
                    if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                    {
                        // 解析版本号，输出格式类似于 "zig 0.12.0"
                        var parts = output.Trim().Split(' ');
                        if (parts.Length >= 2)
                        {
                            return parts[1];
                        }
                    }
                }
                catch
                {
                    // 执行失败，尝试从文件名中解析版本号
                    var dirName = Path.GetFileName(Path.GetDirectoryName(zigPath));
                    if (!string.IsNullOrEmpty(dirName))
                    {
                        // 尝试从目录名中提取版本号，例如 "zig-windows-x86_64-0.15.2"
                        var match = System.Text.RegularExpressions.Regex.Match(dirName, @"(\d+\.\d+\.\d+)");
                        if (match.Success)
                        {
                            return match.Groups[1].Value;
                        }
                    }
                }
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 获取系统中Zig的安装路径
    /// </summary>
    /// <returns>Zig的安装路径，如果未安装则返回null</returns>
    public string? GetSystemZigPath()
    {
        try
        {
            // 先尝试使用命令行工具查找（环境变量中的Zig）
            string? zigPath = null;
            
            if (OperatingSystem.IsWindows())
            {
                // 在Windows上使用where命令
                var whereProcess = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "where",
                        Arguments = "zig",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                whereProcess.Start();
                var whereOutput = whereProcess.StandardOutput.ReadToEnd();
                whereProcess.WaitForExit(2000);
                
                if (whereProcess.ExitCode == 0 && !string.IsNullOrEmpty(whereOutput))
                {
                    zigPath = whereOutput.Trim().Split('\n')[0];
                }
                else
                {
                    // 如果where命令找不到，尝试查找常见的安装位置
                    zigPath = FindZigInCommonLocations();
                }
            }
            // 在Linux/macOS上使用which命令
            else
            {
                var whichProcess = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "which",
                        Arguments = "zig",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                whichProcess.Start();
                var whichOutput = whichProcess.StandardOutput.ReadToEnd();
                whichProcess.WaitForExit(2000);
                
                if (whichProcess.ExitCode == 0 && !string.IsNullOrEmpty(whichOutput))
                {
                    zigPath = whichOutput.Trim();
                }
            }
            
            return zigPath;
        }
        catch
        {
            // 如果发生异常，尝试查找常见位置
            if (OperatingSystem.IsWindows())
            {
                return FindZigInCommonLocations();
            }
            return null;
        }
    }
    
    /// <summary>
    /// 在Windows上查找常见的Zig安装位置
    /// </summary>
    /// <returns>Zig的安装路径，如果未找到则返回null</returns>
    private string? FindZigInCommonLocations()
    {
        try
        {
            // 常见的Zig安装目录
            var commonPaths = new List<string>
            {
                // Program Files目录
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "zig"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "zig"),
                
                // User目录
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "zig"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "bin", "zig"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Local", "zig"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Roaming", "zig"),
                
                // 当前目录和父目录
                Directory.GetCurrentDirectory(),
                Path.GetDirectoryName(Environment.ProcessPath) ?? string.Empty
            };
            
            // 遍历常见目录，查找zig.exe
            foreach (var basePath in commonPaths)
            {
                if (string.IsNullOrEmpty(basePath)) continue;
                
                // 检查直接在目录下的zig.exe
                var zigExePath = Path.Combine(basePath, "zig.exe");
                if (File.Exists(zigExePath))
                {
                    return zigExePath;
                }
                
                // 检查子目录中的zig.exe（可能是解压后的目录结构）
                if (Directory.Exists(basePath))
                {
                    // 查找所有包含zig.exe的子目录
                    var zigFiles = Directory.GetFiles(basePath, "zig.exe", SearchOption.AllDirectories);
                    if (zigFiles.Length > 0)
                    {
                        return zigFiles[0];
                    }
                }
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 显示系统中已安装的Zig信息
    /// </summary>
    public void ShowSystemZigInfo()
        {
            Console.WriteLine(AppLang.系统已安装信息);
            Console.WriteLine("---------------------------------");
            
            if (IsZigInstalled())
            {
                var version = GetSystemZigVersion();
                var path = GetSystemZigPath();
                
                Console.WriteLine(AppLang.已安装是);
                Console.WriteLine(string.Format(AppLang.版本, version));
                Console.WriteLine(string.Format(AppLang.路径, path));
            }
            else
            {
                Console.WriteLine(AppLang.已安装否);
            }
            
            Console.WriteLine("---------------------------------");
        }
}