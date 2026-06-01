using System.Text.Json;
using Zigm.Helpers;
using Zigm.Languages;
using Zigm.Models;
using Zigm.Services.Interfaces;

namespace Zigm.Services;

/// <summary>
/// Zig版本服务类，负责获取和管理Zig版本信息
/// </summary>
public class ZigVersionService : IZigVersionService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const string ZigDownloadIndexUrl = "https://ziglang.org/download/index.json";

    /// <summary>
    /// 构造函数
    /// </summary>
    public ZigVersionService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient CreateHttpClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(30);
        return client;
    }

    /// <summary>
    /// 获取所有可用的稳定版本Zig
    /// </summary>
    /// <returns>Zig版本列表</returns>
    public async Task<List<ZigVersion>> GetStableVersionsAsync()
    {
        try
        {
            using var client = CreateHttpClient();
            var response = await client.GetAsync(ZigDownloadIndexUrl);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            return await ParseVersionsFromJsonAsync(content);
        }
        catch (Exception ex)
        {
            Console.WriteLine(string.Format(AppLang.获取Zig版本信息失败, ex.Message));
            return GetMockVersions();
        }
    }

    /// <summary>
    /// 从JSON内容中解析Zig版本信息
    /// </summary>
    /// <param name="jsonContent">JSON内容</param>
    /// <returns>Zig版本列表</returns>
    private Task<List<ZigVersion>> ParseVersionsFromJsonAsync(string jsonContent)
    {
        return Task.Run(() =>
        {
            var versions = new List<ZigVersion>();

            try
            {
                var versionData = JsonSerializer.Deserialize(jsonContent, ZigJsonContext.Default.DictionaryStringVersionEntry);
                if (versionData == null)
                {
                    return versions;
                }

                var currentArch = SystemHelper.GetSystemArchitecture();

                foreach (var (versionKey, versionEntry) in versionData)
                {
                    var zigVersion = MapToZigVersion(versionKey, versionEntry, currentArch);
                    if (zigVersion != null)
                    {
                        versions.Add(zigVersion);
                    }
                }

                versions.Sort((a, b) => CompareVersions(b.Version!, a.Version!));
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format(AppLang.解析Zig版本JSON失败, ex.Message));
            }

            return versions;
        });
    }

    /// <summary>
    /// 将 VersionEntry 映射到 ZigVersion
    /// </summary>
    private ZigVersion? MapToZigVersion(string versionKey, VersionEntry versionEntry, string currentArch)
    {
        var actualVersion = versionEntry.Version ?? versionKey;
        var releaseDate = ParseDate(versionEntry.Date);
        var versionType = DetermineVersionType(versionKey, actualVersion);

        if (versionEntry.ArchitectureDownloads != null && 
            versionEntry.ArchitectureDownloads.TryGetValue(currentArch, out var archElement))
        {
            var downloadResource = archElement.Deserialize(ZigJsonContext.Default.DownloadResource);
            if (downloadResource != null && !string.IsNullOrEmpty(downloadResource.Tarball))
            {
                var zigVersion = new ZigVersion
                {
                    Version = actualVersion,
                    ReleaseDate = releaseDate,
                    Type = versionType
                };

                zigVersion.DownloadUrls.Add(currentArch, downloadResource.Tarball);
                return zigVersion;
            }
        }

        return null;
    }

    /// <summary>
    /// 解析日期字符串
    /// </summary>
    private DateTime ParseDate(string? dateStr)
    {
        if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out var parsedDate))
        {
            return parsedDate;
        }
        return DateTime.Now;
    }

    /// <summary>
    /// 确定版本类型
    /// </summary>
    private string DetermineVersionType(string versionKey, string actualVersion)
    {
        if (versionKey == "master" || actualVersion.Contains("dev"))
        {
            return "dev";
        }
        else if (versionKey.Contains("nightly"))
        {
            return "nightly";
        }
        return "stable";
    }

    /// <summary>
    /// 获取最新的nightly版本Zig
    /// </summary>
    /// <returns>最新的nightly版本，如果获取失败则返回null</returns>
    public async Task<ZigVersion?> GetLatestNightlyVersionAsync()
    {
        try
        {
            using var client = CreateHttpClient();
            var response = await client.GetAsync(ZigDownloadIndexUrl);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            return await ParseLatestNightlyFromJsonAsync(content);
        }
        catch (Exception ex)
        {
            Console.WriteLine(string.Format(AppLang.获取ZigNightly版本信息失败, ex.Message));
            return null;
        }
    }

    /// <summary>
    /// 从JSON内容中解析最新的nightly版本
    /// </summary>
    /// <param name="jsonContent">JSON内容</param>
    /// <returns>最新的nightly版本，如果解析失败则返回null</returns>
    private Task<ZigVersion?> ParseLatestNightlyFromJsonAsync(string jsonContent)
    {
        return Task.Run(() =>
        {
            try
            {
                var versionData = JsonSerializer.Deserialize(jsonContent, ZigJsonContext.Default.DictionaryStringVersionEntry);
                if (versionData == null)
                {
                    return null;
                }

                var currentArch = SystemHelper.GetSystemArchitecture();

                if (versionData.TryGetValue("master", out var masterEntry))
                {
                    var actualVersion = masterEntry.Version ?? "master";
                    var releaseDate = ParseDate(masterEntry.Date);

                    if (masterEntry.ArchitectureDownloads != null && 
                        masterEntry.ArchitectureDownloads.TryGetValue(currentArch, out var archElement))
                    {
                        var downloadResource = archElement.Deserialize(ZigJsonContext.Default.DownloadResource);
                        if (downloadResource != null && !string.IsNullOrEmpty(downloadResource.Tarball))
                        {
                            var zigVersion = new ZigVersion
                            {
                                Version = actualVersion,
                                ReleaseDate = releaseDate,
                                Type = "dev"
                            };
                            zigVersion.DownloadUrls.Add(currentArch, downloadResource.Tarball);
                            return zigVersion;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format(AppLang.解析ZigNightly版本JSON失败, ex.Message));
                return null;
            }
        });
    }

    /// <summary>
    /// 根据版本号获取特定版本的ZigVersion对象
    /// </summary>
    /// <param name="versionNumber">版本号</param>
    /// <returns>ZigVersion对象，如果找不到则返回null</returns>
    public async Task<ZigVersion?> GetVersionAsync(string versionNumber)
    {
        try
        {
            var versions = await GetStableVersionsAsync();
            var version = versions.FirstOrDefault(v => v.Version == versionNumber);

            if (version != null)
            {
                return version;
            }

            var masterVersion = await GetLatestNightlyVersionAsync();
            if (masterVersion?.Version == versionNumber || versionNumber == "master")
            {
                return masterVersion;
            }

            using var client = CreateHttpClient();
            var response = await client.GetAsync(ZigDownloadIndexUrl);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            return await FindVersionInJsonAsync(content, versionNumber);
        }
        catch (Exception ex)
        {
            Console.WriteLine(string.Format(AppLang.获取指定版本失败, ex.Message));
            return null;
        }
    }

    /// <summary>
    /// 在JSON中查找特定版本
    /// </summary>
    private Task<ZigVersion?> FindVersionInJsonAsync(string jsonContent, string versionNumber)
    {
        return Task.Run(() =>
        {
            try
            {
                var versionData = JsonSerializer.Deserialize(jsonContent, ZigJsonContext.Default.DictionaryStringVersionEntry);
                if (versionData == null)
                {
                    return null;
                }

                var currentArch = SystemHelper.GetSystemArchitecture();

                foreach (var (key, versionEntry) in versionData)
                {
                    var actualVersion = versionEntry.Version ?? key;

                    if (key == versionNumber || actualVersion == versionNumber)
                    {
                        var zigVersion = MapToZigVersion(key, versionEntry, currentArch);
                        return zigVersion;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format(AppLang.在JSON中查找版本失败, ex.Message));
                return null;
            }
        });
    }

    /// <summary>
    /// 比较两个版本号的大小
    /// </summary>
    /// <param name="version1">第一个版本号</param>
    /// <param name="version2">第二个版本号</param>
    /// <returns>如果version1大于version2返回1，等于返回0，小于返回-1</returns>
    public int CompareVersions(string version1, string version2)
    {
        if (string.IsNullOrEmpty(version1) || string.IsNullOrEmpty(version2))
        {
            return string.Compare(version1, version2, StringComparison.Ordinal);
        }

        try
        {
            var v1 = version1.Split('-')[0];
            var v2 = version2.Split('-')[0];

            return Version.Parse(v1).CompareTo(Version.Parse(v2));
        }
        catch (Exception ex)
        {
            Console.WriteLine(string.Format(AppLang.版本比较失败, ex.Message));
            return string.Compare(version1, version2, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// 获取模拟版本数据，当网络请求失败时使用
    /// </summary>
    /// <returns>模拟的Zig版本列表</returns>
    private List<ZigVersion> GetMockVersions()
    {
        var currentArch = SystemHelper.GetSystemArchitecture();
        var mockVersions = new List<ZigVersion>
        {
            new ZigVersion { Version = "0.12.0", ReleaseDate = new DateTime(2024, 12, 2), Type = "stable" },
            new ZigVersion { Version = "0.11.0", ReleaseDate = new DateTime(2024, 7, 15), Type = "stable" },
            new ZigVersion { Version = "0.10.1", ReleaseDate = new DateTime(2024, 2, 28), Type = "stable" },
            new ZigVersion { Version = "0.9.1", ReleaseDate = new DateTime(2023, 10, 10), Type = "stable" }
        };

        foreach (var version in mockVersions)
        {
            version.DownloadUrls.Add(currentArch, $"https://ziglang.org/builds/zig-{currentArch}-{version.Version}.tar.xz");
        }

        return mockVersions;
    }
}
