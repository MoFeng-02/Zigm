using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text.Json;
using Zigm.Languages;
using Zigm.Models;
using Zigm.Services.Interfaces;

namespace Zigm.Services;

/// <summary>
/// Zigm更新服务类
/// </summary>
public class ZigmUpdaterService : IZigmUpdaterService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _currentVersion;
    private const string GitHubApiUrl = "https://api.github.com/repos/MoFeng-02/Zigm/releases/latest";
    private readonly string? _currentExePath;
    private readonly string _currentDir;
    private readonly string _tempDir;

    /// <summary>
    /// 构造函数
    /// </summary>
    public ZigmUpdaterService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _currentVersion = GetCurrentVersion();
        var process = Process.GetCurrentProcess();
        _currentExePath = process.MainModule?.FileName;

        // 如果失败，使用命令行参数（Linux/macOS 常用）
        if (string.IsNullOrEmpty(_currentExePath))
        {
            _currentExePath = Environment.GetCommandLineArgs()[0];
        }

        // 如果是相对路径，转为绝对路径
        if (!Path.IsPathRooted(_currentExePath))
        {
            _currentExePath = Path.Combine(Environment.CurrentDirectory, _currentExePath);
        }
        _currentDir = Path.GetDirectoryName(_currentExePath)
            ?? AppDomain.CurrentDomain.BaseDirectory;

        _tempDir = Path.Combine(Path.GetTempPath(), $"ZigmUpdate_{Guid.NewGuid():N}");
    }

    private static string GetCurrentVersion()
    {
        var assembly = typeof(ZigmUpdaterService).Assembly;
        using var stream = assembly.GetManifestResourceStream("Zigm.version.json");
        if (stream == null)
        {
            return "1.0.0";
        }
        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();
        using var doc = JsonDocument.Parse(content);
        if (doc.RootElement.TryGetProperty("version", out var versionElement))
        {
            return versionElement.GetString() ?? "1.0.0";
        }
        return "1.0.0";
    }

    private HttpClient CreateHttpClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("ZigmUpdater", _currentVersion));

        // 对于私有仓库或避免限流，可以添加 Token
        // var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        // if (!string.IsNullOrEmpty(token))
        // {
        //     client.DefaultRequestHeaders.Authorization = 
        //         new AuthenticationHeaderValue("Bearer", token);
        // }
        return client;
    }

    /// <summary>
    /// 检查更新
    /// </summary>
    public async Task<(bool HasUpdate, GitHubRelease? Release, string? Error)> CheckForUpdatesAsync()
    {
        try
        {
            Console.WriteLine(AppLang.检查更新中);

            using var client = CreateHttpClient();
            var response = await client.GetAsync(GitHubApiUrl);
            if (!response.IsSuccessStatusCode)
            {
                return (false, null, $"API请求失败: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var latestRelease = JsonSerializer.Deserialize(
                content, GitHubJsonContext.Default.GitHubRelease);

            if (latestRelease == null || string.IsNullOrEmpty(latestRelease.TagName))
            {
                return (false, null, "无法解析发布信息");
            }

            var latestVersion = latestRelease.TagName.TrimStart('v', 'V');
            var currentVersionObj = ParseVersion(_currentVersion);
            var latestVersionObj = ParseVersion(latestVersion);

            if (latestVersionObj > currentVersionObj)
            {
                Console.WriteLine(string.Format(AppLang.发现新版本, latestRelease.TagName));
                return (true, latestRelease, null);
            }

            Console.WriteLine(AppLang.当前已是最新版本);
            return (false, null, null);
        }
        catch (Exception ex)
        {
            return (false, null, $"检查更新失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 执行更新
    /// </summary>
    public async Task<bool> UpdateAsync(GitHubRelease release)
    {
        try
        {
            // 1. 匹配资产包
            var runtimeId = GetCurrentRuntimeId();
            var asset = release.Assets?.FirstOrDefault(a =>
                a?.Name != null && a.Name.Contains(runtimeId, StringComparison.OrdinalIgnoreCase));

            if (asset == null)
            {
                Console.WriteLine(string.Format(AppLang.未找到兼容的包, runtimeId));
                return false;
            }

            if (asset.Name == null)
            {
                Console.WriteLine(AppLang.资产包名称为空);
                return false;
            }

            if (asset.BrowserDownloadUrl == null)
            {
                Console.WriteLine(AppLang.资产包下载链接为空);
                return false;
            }

            // 2. 创建临时目录
            Directory.CreateDirectory(_tempDir);

            // 3. 下载文件
            var downloadPath = Path.Combine(_tempDir, asset.Name);
            Console.WriteLine(string.Format(AppLang.正在下载, asset.Name));
            await DownloadFileAsync(asset.BrowserDownloadUrl, downloadPath);

            // 4. 提取文件
            var extractDir = Path.Combine(_tempDir, "extracted");

            if (asset.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                ZipFile.ExtractToDirectory(downloadPath, extractDir);
            }
            else if (asset.Name.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase))
            {
                await ExtractTarGzAsync(downloadPath, extractDir);
            }
            else
            {
                throw new NotSupportedException($"不支持的压缩格式: {Path.GetExtension(asset.Name)}");
            }

            // 5. 替换文件
            await ReplaceFilesAsync(extractDir);

            Console.WriteLine(AppLang.更新完成请重启应用程序);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(string.Format(AppLang.更新失败, ex.Message));
            return false;
        }
        finally
        {
            CleanupTempFiles();
        }
    }

    /// <summary>
    /// 替换文件（更安全的实现）
    /// </summary>
    private async Task ReplaceFilesAsync(string extractDir)
    {
        var exeExtension = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "";
        var newExeFile = Directory.EnumerateFiles(extractDir, $"*{exeExtension}", SearchOption.AllDirectories)
            .FirstOrDefault(f => Path.GetFileName(f).StartsWith("zigm", StringComparison.OrdinalIgnoreCase));

        if (newExeFile == null)
        {
            throw new FileNotFoundException("未找到可执行文件");
        }

        // 检查写入权限
        if (!HasWriteAccess(_currentDir))
        {
            throw new UnauthorizedAccessException("没有写入权限，请以管理员/root权限运行");
        }

        // Windows: 创建批处理文件来延迟替换
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            await ReplaceOnWindowsAsync(newExeFile);
        }
        else
        {
            await ReplaceOnUnixAsync(newExeFile);
        }
    }

    /// <summary>
    /// Windows 下的替换逻辑（使用批处理）
    /// </summary>
    private async Task ReplaceOnWindowsAsync(string newExeFile)
    {
        var backupPath = _currentExePath + ".old";
        var batPath = Path.Combine(_tempDir, "replace.bat");

        // 创建批处理文件
        var batContent = $@"
@echo off
timeout /t 2 /nobreak >nul
del ""{backupPath}"" 2>nul
move ""{_currentExePath}"" ""{backupPath}"" 2>nul
copy ""{newExeFile}"" ""{_currentExePath}"" 2>nul
start """" ""{_currentExePath}""
del ""{backupPath}"" 2>nul
del ""%~f0""
";

        await File.WriteAllTextAsync(batPath, batContent);

        // 启动批处理
        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/C \"{batPath}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        });

        Environment.Exit(0); // 退出当前进程
    }

    /// <summary>
    /// Unix 系统下的替换逻辑
    /// </summary>
    private async Task ReplaceOnUnixAsync(string newExeFile)
    {
        // 复制文件
        File.Copy(newExeFile, _currentExePath!, true);

        // 设置可执行权限
        await SetExecutablePermissionAsync(_currentExePath!);
    }

    /// <summary>
    /// 设置可执行权限
    /// </summary>
    private async Task SetExecutablePermissionAsync(string filePath)
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "chmod",
                Arguments = $"+x \"{filePath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            });

            if (process != null)
            {
                await process.WaitForExitAsync();
            }
        }
        catch
        {
            // 如果 chmod 失败，尝试其他方法
            Console.WriteLine(string.Format(AppLang.警告无法设置可执行权限, filePath));
        }
    }

    /// <summary>
    /// 检查写入权限
    /// </summary>
    private bool HasWriteAccess(string directory)
    {
        try
        {
            var testFile = Path.Combine(directory, $"test_{Guid.NewGuid():N}.tmp");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 解析版本号
    /// </summary>
    private Version ParseVersion(string version)
    {
        if (Version.TryParse(version, out var result))
            return result;

        // 简化处理：只取数字部分
        var numbers = System.Text.RegularExpressions.Regex.Replace(version, "[^0-9.]", "");
        return Version.TryParse(numbers, out result) ? result : new Version(0, 0, 0);
    }

    /// <summary>
    /// 获取 Runtime ID
    /// </summary>
    private string GetCurrentRuntimeId()
    {
        var architecture = RuntimeInformation.ProcessArchitecture;
        var os = "unknown";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            os = "win";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            os = "linux";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            os = "osx";

        var arch = architecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            Architecture.X86 => "x86",
            _ => "x64"
        };

        return $"{os}-{arch}";
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    private async Task DownloadFileAsync(string url, string savePath)
    {
        using var client = CreateHttpClient();
        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var fileStream = File.Create(savePath);

        await stream.CopyToAsync(fileStream);
    }

    /// <summary>
    /// 解压 tar.gz（简化示例）
    /// </summary>
    private async Task ExtractTarGzAsync(string archivePath, string extractDir)
    {
        // 这里需要实现 tar.gz 解压逻辑
        // 可以使用 SharpCompress 或其他库
        throw new NotImplementedException("tar.gz 解压功能待实现");
    }

    /// <summary>
    /// 清理临时文件
    /// </summary>
    private void CleanupTempFiles()
    {
        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(string.Format(AppLang.清理临时文件失败, ex.Message));
        }
    }
}