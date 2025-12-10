using System.Text.Json;
using Zigm.Helpers;
using Zigm.Models;
using Zigm.Languages;

namespace Zigm.Services;

/// <summary>
/// Zig版本服务类，负责获取和管理Zig版本信息
/// </summary>
public class ZigVersionService
{
    private readonly HttpClient _httpClient;
    private const string ZigDownloadIndexUrl = "https://ziglang.org/download/index.json";

    /// <summary>
    /// 构造函数
    /// </summary>
    public ZigVersionService()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// 获取所有可用的稳定版本Zig
    /// </summary>
    /// <returns>Zig版本列表</returns>
    public async Task<List<ZigVersion>> GetStableVersionsAsync()
    {
        try
        {
            // 使用官方JSON API获取版本信息
            var response = await _httpClient.GetAsync(ZigDownloadIndexUrl);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            // 解析JSON内容
            return ParseVersionsFromJson(content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取Zig版本信息失败: {ex.Message}");
            // 失败时返回模拟数据
            return GetMockVersions();
        }
    }

    /// <summary>
    /// 从JSON内容中解析Zig版本信息
    /// </summary>
    /// <param name="jsonContent">JSON内容</param>
    /// <returns>Zig版本列表</returns>
    private List<ZigVersion> ParseVersionsFromJson(string jsonContent)
    {
        var versions = new List<ZigVersion>();
        
        try
        {
            // 解析JSON
            var versionData = JsonSerializer.Deserialize(jsonContent, ZigJsonContext.Default.DictionaryStringJsonElement);
            if (versionData == null)
            {
                return versions;
            }

            // 获取当前系统架构
            var currentArch = SystemHelper.GetSystemArchitecture();
            
            // 遍历所有版本
            foreach (var (versionKey, versionElement) in versionData)
            {
                // 解析版本信息
                string actualVersion = versionKey;
                DateTime releaseDate = DateTime.Now;
                string versionType = "stable";
                
                // 尝试获取version字段（如果存在）
                if (versionElement.TryGetProperty("version", out var versionProp))
                {
                    actualVersion = versionProp.GetString() ?? versionKey;
                }
                
                // 尝试获取date字段
                if (versionElement.TryGetProperty("date", out var dateProp))
                {
                    var dateStr = dateProp.GetString();
                    if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out var parsedDate))
                    {
                        releaseDate = parsedDate;
                    }
                }
                
                // 确定版本类型
                if (versionKey == "master" || actualVersion.Contains("dev"))
                {
                    versionType = "dev";
                }
                else if (versionKey.Contains("nightly"))
                {
                    versionType = "nightly";
                }
                
                // 检查当前系统架构是否在该版本中存在
                if (versionElement.TryGetProperty(currentArch, out var archElement))
                {
                    // 该版本支持当前系统，创建ZigVersion对象
                    var zigVersion = new ZigVersion
                    {
                        Version = actualVersion,
                        ReleaseDate = releaseDate,
                        Type = versionType
                    };
                    
                    // 获取下载链接
                    if (archElement.TryGetProperty("tarball", out var tarballProp))
                    {
                        var tarballUrl = tarballProp.GetString();
                        if (!string.IsNullOrEmpty(tarballUrl))
                        {
                            zigVersion.DownloadUrls.Add(currentArch, tarballUrl);
                        }
                    }
                    
                    versions.Add(zigVersion);
                }
            }

            // 按版本号降序排序
            versions.Sort((a, b) => CompareVersions(b.Version!, a.Version!));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"解析Zig版本JSON失败: {ex.Message}");
        }

        return versions;
    }

    /// <summary>
    /// 获取最新的nightly版本Zig
    /// </summary>
    /// <returns>最新的nightly版本，如果获取失败则返回null</returns>
    public async Task<ZigVersion?> GetLatestNightlyVersionAsync()
    {
        try
        {
            // 使用官方JSON API获取版本信息
            var response = await _httpClient.GetAsync(ZigDownloadIndexUrl);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            // 解析JSON内容，提取nightly版本
            return ParseLatestNightlyFromJson(content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取Zig nightly版本信息失败: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 从JSON内容中解析最新的nightly版本
    /// </summary>
    /// <param name="jsonContent">JSON内容</param>
    /// <returns>最新的nightly版本，如果解析失败则返回null</returns>
    private ZigVersion? ParseLatestNightlyFromJson(string jsonContent)
    {
        try
        {
            // 解析JSON
            var versionData = JsonSerializer.Deserialize(jsonContent, ZigJsonContext.Default.DictionaryStringJsonElement);
            if (versionData == null)
            {
                return null;
            }

            // 获取当前系统架构
            var currentArch = SystemHelper.GetSystemArchitecture();
            
            // 获取master版本（作为最新开发版本）
            if (versionData.TryGetValue("master", out var masterElement))
            {
                string actualVersion = "master";
                DateTime releaseDate = DateTime.Now;
                
                // 尝试获取version字段
                if (masterElement.TryGetProperty("version", out var versionProp))
                {
                    actualVersion = versionProp.GetString() ?? "master";
                }
                
                // 尝试获取date字段
                if (masterElement.TryGetProperty("date", out var dateProp))
                {
                    var dateStr = dateProp.GetString();
                    if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out var parsedDate))
                    {
                        releaseDate = parsedDate;
                    }
                }
                
                // 检查当前系统架构是否在该版本中存在
                if (masterElement.TryGetProperty(currentArch, out var archElement))
                {
                    // 获取下载链接
                    if (archElement.TryGetProperty("tarball", out var tarballProp))
                    {
                        var tarballUrl = tarballProp.GetString();
                        if (!string.IsNullOrEmpty(tarballUrl))
                        {
                            var zigVersion = new ZigVersion
                            {
                                Version = actualVersion,
                                ReleaseDate = releaseDate,
                                Type = "dev"
                            };
                            zigVersion.DownloadUrls.Add(currentArch, tarballUrl);
                            return zigVersion;
                        }
                    }
                }
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"解析Zig nightly版本JSON失败: {ex.Message}");
            return null;
        }
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
            // 先检查稳定版本
            var versions = await GetStableVersionsAsync();
            var version = versions.FirstOrDefault(v => v.Version == versionNumber);
            
            if (version != null)
            {
                return version;
            }
            
            // 如果不是稳定版本，检查master版本
            var masterVersion = await GetLatestNightlyVersionAsync();
            if (masterVersion?.Version == versionNumber || versionNumber == "master")
            {
                return masterVersion;
            }
            
            // 如果都找不到，尝试从JSON API直接获取
            var response = await _httpClient.GetAsync(ZigDownloadIndexUrl);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            // 解析JSON内容
            var versionData = JsonSerializer.Deserialize(content, ZigJsonContext.Default.DictionaryStringJsonElement);
            if (versionData != null)
            {
                // 查找版本（可能是版本号作为键，或者在version字段中）
                foreach (var (key, value) in versionData)
                {
                    // 检查键是否匹配，或者version字段是否匹配
                    string actualVersion = key;
                    if (value.TryGetProperty("version", out var versionProp))
                    {
                        actualVersion = versionProp.GetString() ?? key;
                    }
                    
                    if (key == versionNumber || actualVersion == versionNumber)
                    {
                        // 获取当前系统架构
                        var currentArch = SystemHelper.GetSystemArchitecture();
                        
                        // 检查当前系统架构是否在该版本中存在
                        if (value.TryGetProperty(currentArch, out var archElement))
                        {
                            // 获取下载链接
                            if (archElement.TryGetProperty("tarball", out var tarballProp))
                            {
                                var tarballUrl = tarballProp.GetString();
                                if (!string.IsNullOrEmpty(tarballUrl))
                                {
                                    // 解析版本信息
                                    DateTime releaseDate = DateTime.Now;
                                    string versionType = "stable";
                                    
                                    // 尝试获取date字段
                                    if (value.TryGetProperty("date", out var dateProp))
                                    {
                                        var dateStr = dateProp.GetString();
                                        if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out var parsedDate))
                                        {
                                            releaseDate = parsedDate;
                                        }
                                    }
                                    
                                    // 确定版本类型
                                    if (key == "master" || actualVersion.Contains("dev"))
                                    {
                                        versionType = "dev";
                                    }
                                    else if (key.Contains("nightly"))
                                    {
                                        versionType = "nightly";
                                    }
                                    
                                    var customVersion = new ZigVersion
                                    {
                                        Version = actualVersion,
                                        ReleaseDate = releaseDate,
                                        Type = versionType
                                    };
                                    customVersion.DownloadUrls.Add(currentArch, tarballUrl);
                                    return customVersion;
                                }
                            }
                        }
                        break;
                    }
                }
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取指定版本失败: {ex.Message}");
            return null;
        }
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
            // 移除nightly等后缀
            var v1 = version1.Split('-')[0];
            var v2 = version2.Split('-')[0];

            return Version.Parse(v1).CompareTo(Version.Parse(v2));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"版本比较失败: {ex.Message}");
            // 失败时使用字符串比较
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

        // 为每个模拟版本添加下载链接
        foreach (var version in mockVersions)
        {
            // 只添加当前系统的下载链接
            version.DownloadUrls.Add(currentArch, $"https://ziglang.org/builds/zig-{currentArch}-{version.Version}.tar.xz");
        }

        return mockVersions;
    }
}