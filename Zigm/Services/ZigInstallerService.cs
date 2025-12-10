using Zigm.Helpers;
using Zigm.Models;
using Zigm.Languages;

namespace Zigm.Services;

/// <summary>
/// Zig安装服务类，负责下载和安装Zig版本
/// </summary>
public class ZigInstallerService
{
    private readonly HttpClient _httpClient;
    private readonly LocalStorageService _localStorageService;
    private readonly ZigVersionService _zigVersionService;
    private readonly EnvironmentService _environmentService;
    private readonly Config _config;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="localStorageService">本地存储服务</param>
    /// <param name="zigVersionService">Zig版本服务</param>
    public ZigInstallerService(LocalStorageService localStorageService, ZigVersionService zigVersionService, Config config)
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromMinutes(10); // 下载大文件需要更长时间
        _localStorageService = localStorageService;
        _zigVersionService = zigVersionService;
        _environmentService = new EnvironmentService(localStorageService);
        _config = config;
    }

    /// <summary>
    /// 下载Zig版本
    /// </summary>
    /// <param name="version">版本号</param>
    /// <returns>下载文件的路径，如果下载失败则返回null</returns>
    private async Task<string?> DownloadVersionAsync(string version)
    {
        try
        {
            // 获取版本信息
            var zigVersion = await _zigVersionService.GetVersionAsync(version);
            if (zigVersion == null)
            {
                Console.WriteLine(string.Format(AppLang.找不到指定版本, version));
                return null;
            }

            // 获取当前系统架构
            var architecture = SystemHelper.GetSystemArchitecture();
            if (!zigVersion.DownloadUrls.ContainsKey(architecture))
            {
                Console.WriteLine(string.Format(AppLang.不支持当前系统架构, architecture));
                return null;
            }

            var downloadUrl = zigVersion.DownloadUrls[architecture]?.Replace(Config.ConstantDownloadSource, _config.DownloadSource);
            var fileName = Path.GetFileName(downloadUrl);
            if (fileName == null)
            {
                Console.WriteLine(string.Format(AppLang.无效的下载链接, downloadUrl));
                return null;
            }
            var tempPath = Path.Combine(Path.GetTempPath(), fileName);

            Console.WriteLine(string.Format(AppLang.正在下载, fileName));
            Console.WriteLine(string.Format(AppLang.从, downloadUrl));
            Console.WriteLine(string.Format(AppLang.到, tempPath));

            // 下载文件
            using (var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? 0;
                var downloadedBytes = 0;

                using var contentStream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
                var buffer = new byte[8192];
                var bytesRead = 0;

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    downloadedBytes += bytesRead;

                    // 显示下载进度
                    if (totalBytes > 0)
                    {
                        var progress = (double)downloadedBytes / totalBytes * 100;
                        Console.Write($"\r下载进度: {progress:F2}% ({downloadedBytes:N0}/{totalBytes:N0} 字节)");
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine(AppLang.下载完成);
            return tempPath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"下载Zig版本失败: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 安装指定版本的Zig
    /// </summary>
    /// <param name="version">版本号</param>
    /// <returns>安装是否成功</returns>
    public async Task<bool> InstallAsync(string version)
    {
        // 检查版本是否已安装
            if (_localStorageService.IsVersionInstalled(version))
            {
                Console.WriteLine($"版本 {version} {AppLang.已安装是}");
                return true;
            }

        // 下载版本
        var downloadPath = await DownloadVersionAsync(version);
        if (string.IsNullOrEmpty(downloadPath))
        {
            return false;
        }

        try
        {
            // 安装版本
            Console.WriteLine(string.Format(AppLang.正在安装版本, version));
            var success = _localStorageService.InstallVersion(version, downloadPath);

            if (success)
            {
                Console.WriteLine(string.Format(AppLang.版本安装成功, version));

                // 如果是第一次安装，自动设置为当前版本
                if (string.IsNullOrEmpty(_localStorageService.GetCurrentVersion()))
                {
                    _localStorageService.SetCurrentVersion(version);
                    Console.WriteLine(string.Format(AppLang.已将版本设置为当前版本, version));
                }
            }
            else
            {
                Console.WriteLine(string.Format(AppLang.版本安装失败, version));
            }

            return success;
        }
        finally
        {
            // 清理临时文件
            _localStorageService.CleanupTempFile(downloadPath);
        }
    }

    /// <summary>
    /// 卸载指定版本的Zig
    /// </summary>
    /// <param name="version">版本号</param>
    /// <returns>卸载是否成功</returns>
    public bool Uninstall(string version)
    {
        // 检查版本是否已安装
        if (!_localStorageService.IsVersionInstalled(version))
        {
            Console.WriteLine(string.Format(AppLang.版本未安装, version));
            return false;
        }

        // 检查是否是当前使用的版本
        var currentVersion = _localStorageService.GetCurrentVersion();
        if (currentVersion == version)
        {
            Console.WriteLine(string.Format(AppLang.版本是当前使用的版本无法卸载, version));
            return false;
        }

        // 卸载版本
        var success = _localStorageService.UninstallVersion(version);
        if (success)
        {
            Console.WriteLine(string.Format(AppLang.版本卸载成功, version));
        }
        else
        {
            Console.WriteLine(string.Format(AppLang.版本卸载失败, version));
        }

        return success;
    }

    /// <summary>
    /// 切换到指定版本的Zig
    /// </summary>
    /// <param name="version">版本号</param>
    /// <param name="target">环境变量目标（用户或系统）</param>
    /// <returns>切换是否成功</returns>
    public bool SwitchToVersion(string version, EnvironmentVariableTarget target = EnvironmentVariableTarget.User)
    {
        // 检查版本是否已安装
        if (!_localStorageService.IsVersionInstalled(version))
        {
            Console.WriteLine(string.Format(AppLang.版本未安装, version));
            return false;
        }

        // 设置当前版本
        _localStorageService.SetCurrentVersion(version);

        // 更新系统环境变量
        if (_environmentService.AddZigToPath(version, target))
        {
            Console.WriteLine(string.Format(AppLang.已成功切换到版本, version));
            return true;
        }
        else
        {
            Console.WriteLine(string.Format(AppLang.切换到版本失败, version));
            return false;
        }
    }
}
