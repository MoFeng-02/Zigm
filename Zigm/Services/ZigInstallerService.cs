using Zigm.Helpers;
using Zigm.Models;
using Zigm.Languages;
using Zigm.Services.Interfaces;

namespace Zigm.Services;

public class ZigInstallerService : IZigInstallerService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILocalStorageService _localStorageService;
    private readonly IZigVersionService _zigVersionService;
    private readonly IEnvironmentService _environmentService;
    private readonly Config _config;

    public ZigInstallerService(
        ILocalStorageService localStorageService, 
        IZigVersionService zigVersionService, 
        IEnvironmentService environmentService,
        Config config,
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _localStorageService = localStorageService;
        _zigVersionService = zigVersionService;
        _environmentService = environmentService;
        _config = config;
    }

    private HttpClient CreateHttpClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromMinutes(10);
        return client;
    }

    private async Task<string?> DownloadVersionAsync(string version)
    {
        try
        {
            var zigVersion = await _zigVersionService.GetVersionAsync(version);
            if (zigVersion == null)
            {
                Console.WriteLine(string.Format(AppLang.找不到指定版本, version));
                return null;
            }

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

            using var httpClient = CreateHttpClient();
            using (var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
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

                    if (totalBytes > 0)
                    {
                        var progress = (double)downloadedBytes / totalBytes * 100;
                        Console.Write($"\r{string.Format(AppLang.下载进度, progress, downloadedBytes, totalBytes)}");
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine(AppLang.下载完成);
            return tempPath;
        }
        catch (Exception ex)
        {
            Console.WriteLine(string.Format(AppLang.下载Zig版本失败, ex.Message));
            return null;
        }
    }

    public async Task<bool> InstallAsync(string version)
    {
        if (_localStorageService.IsVersionInstalled(version))
        {
            Console.WriteLine(string.Format(AppLang.版本已安装, version, AppLang.已安装是));
            return true;
        }

        var downloadPath = await DownloadVersionAsync(version);
        if (string.IsNullOrEmpty(downloadPath))
        {
            return false;
        }

        try
        {
            Console.WriteLine(string.Format(AppLang.正在安装版本, version));
            var success = _localStorageService.InstallVersion(version, downloadPath);

            if (success)
            {
                Console.WriteLine(string.Format(AppLang.版本安装成功, version));

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
            _localStorageService.CleanupTempFile(downloadPath);
        }
    }

    public bool Uninstall(string version)
    {
        if (!_localStorageService.IsVersionInstalled(version))
        {
            Console.WriteLine(string.Format(AppLang.版本未安装, version));
            return false;
        }

        var currentVersion = _localStorageService.GetCurrentVersion();
        if (currentVersion == version)
        {
            Console.WriteLine(string.Format(AppLang.版本是当前使用的版本无法卸载, version));
            return false;
        }

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

    public bool SwitchToVersion(string version, EnvironmentVariableTarget target = EnvironmentVariableTarget.User)
    {
        if (!_localStorageService.IsVersionInstalled(version))
        {
            Console.WriteLine(string.Format(AppLang.版本未安装, version));
            return false;
        }

        _localStorageService.SetCurrentVersion(version);

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
