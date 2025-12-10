using System.IO.Compression;
using Zigm.Models;
using Zigm.Languages;

namespace Zigm.Services;

/// <summary>
/// 本地存储服务类，负责管理Zig版本的存储路径和文件操作
/// </summary>
public class LocalStorageService
{
    private readonly string _basePath;
    private readonly string _currentVersionPath;
    private readonly string _versionsPath;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="config">配置对象</param>
    public LocalStorageService(Config config)
    {
        // 使用配置中的存储路径
        _basePath = config.StoragePath ?? GetDefaultBasePath();
        _versionsPath = Path.Combine(_basePath, "versions");
        _currentVersionPath = Path.Combine(_basePath, "current");

        // 确保目录存在
        Directory.CreateDirectory(_basePath);
        Directory.CreateDirectory(_versionsPath);
    }

    /// <summary>
    /// 获取默认基础存储目录
    /// </summary>
    /// <returns>默认基础存储目录路径</returns>
    private string GetDefaultBasePath()
    {
        // 根据不同操作系统确定默认存储目录
        if (OperatingSystem.IsWindows())
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Zigm", "zig-versions");
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".zigm", "zig-versions");
        }
        else
        {
            throw new PlatformNotSupportedException("不支持的操作系统");
        }
    }

    /// <summary>
    /// 获取指定版本的存储路径
    /// </summary>
    /// <param name="version">版本号</param>
    /// <returns>存储路径</returns>
    public string GetVersionPath(string version)
    {
        return Path.Combine(_versionsPath, version);
    }

    /// <summary>
    /// 获取当前版本的存储路径
    /// </summary>
    /// <returns>当前版本的存储路径</returns>
    public string GetCurrentVersionPath()
    {
        return _currentVersionPath;
    }

    /// <summary>
    /// 列出本地已安装的版本
    /// </summary>
    /// <returns>已安装版本列表</returns>
    public List<string> ListInstalledVersions()
    {
        if (!Directory.Exists(_versionsPath))
        {
            return new List<string>();
        }

        return Directory.GetDirectories(_versionsPath)
            .Select(Path.GetFileName)
            .Where(name => name != null)
            .OrderByDescending(v => v)
            .Select(name => name!)
            .ToList();
    }

    /// <summary>
    /// 检查指定版本是否已安装
    /// </summary>
    /// <param name="version">版本号</param>
    /// <returns>如果已安装返回true，否则返回false</returns>
    public bool IsVersionInstalled(string version)
    {
        return Directory.Exists(GetVersionPath(version));
    }

    /// <summary>
    /// 获取当前使用的版本
    /// </summary>
    /// <returns>当前版本号，如果没有设置则返回null</returns>
    public string? GetCurrentVersion()
    {
        if (File.Exists(_currentVersionPath))
        {
            return File.ReadAllText(_currentVersionPath).Trim();
        }
        return null;
    }

    /// <summary>
    /// 设置当前使用的版本
    /// </summary>
    /// <param name="version">版本号</param>
    public void SetCurrentVersion(string version)
    {
        File.WriteAllText(_currentVersionPath, version);
    }

    /// <summary>
    /// 安装Zig版本（解压下载的文件）
    /// </summary>
    /// <param name="version">版本号</param>
    /// <param name="downloadPath">下载文件的路径</param>
    /// <returns>安装是否成功</returns>
    public bool InstallVersion(string version, string downloadPath)
    {
        try
        {
            var versionPath = GetVersionPath(version);
            
            // 确保版本目录不存在
            if (Directory.Exists(versionPath))
            {
                Directory.Delete(versionPath, true);
            }
            Directory.CreateDirectory(versionPath);

            // 根据文件扩展名选择解压方式
            var extension = Path.GetExtension(downloadPath).ToLower();
            if (extension == ".zip")
            {
                // 解压ZIP文件
                ZipFile.ExtractToDirectory(downloadPath, versionPath);
                
                // 移动解压后的文件到版本目录根目录
                MoveExtractedFiles(versionPath);
            }
            else if (extension == ".xz" || extension == ".tar.xz")
            {
                // 这里需要处理tar.xz文件，暂时使用占位符
                Console.WriteLine("tar.xz文件解压功能将在后续实现");
                return false;
            }
            else
            {
                Console.WriteLine($"不支持的文件格式: {extension}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(string.Format(AppLang.版本安装失败, version));
            Console.WriteLine($"错误信息: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 移动解压后的文件到版本目录根目录
    /// </summary>
    /// <param name="versionPath">版本目录路径</param>
    private void MoveExtractedFiles(string versionPath)
    {
        // 获取解压后的第一个目录
        var subDirectories = Directory.GetDirectories(versionPath);
        if (subDirectories.Length > 0)
        {
            var extractedDir = subDirectories[0];
            
            // 移动所有文件和子目录到版本目录根目录
            foreach (var file in Directory.GetFiles(extractedDir))
            {
                var destFile = Path.Combine(versionPath, Path.GetFileName(file));
                File.Move(file, destFile, true);
            }
            
            foreach (var dir in Directory.GetDirectories(extractedDir))
            {
                var destDir = Path.Combine(versionPath, Path.GetFileName(dir));
                Directory.Move(dir, destDir);
            }
            
            // 删除空的解压目录
            Directory.Delete(extractedDir);
        }
    }

    /// <summary>
    /// 卸载指定版本
    /// </summary>
    /// <param name="version">版本号</param>
    /// <returns>卸载是否成功</returns>
    public bool UninstallVersion(string version)
    {
        try
        {
            var versionPath = GetVersionPath(version);
            
            if (Directory.Exists(versionPath))
            {
                Directory.Delete(versionPath, true);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(string.Format(AppLang.版本卸载失败, version));
            Console.WriteLine($"错误信息: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 清理临时文件
    /// </summary>
    /// <param name="filePath">要清理的文件路径</param>
    public void CleanupTempFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清理临时文件失败: {ex.Message}");
            }
        }
    }
}
