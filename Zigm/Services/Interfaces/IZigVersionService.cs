using Zigm.Models;

namespace Zigm.Services.Interfaces;

/// <summary>
/// Zig 版本服务接口，负责获取和管理 Zig 版本信息
/// </summary>
public interface IZigVersionService
{
    /// <summary>
    /// 获取所有可用的稳定版本 Zig
    /// </summary>
    /// <returns>Zig 版本列表</returns>
    Task<List<ZigVersion>> GetStableVersionsAsync();

    /// <summary>
    /// 获取最新的 nightly 版本 Zig
    /// </summary>
    /// <returns>最新的 nightly 版本，如果获取失败则返回 null</returns>
    Task<ZigVersion?> GetLatestNightlyVersionAsync();

    /// <summary>
    /// 根据版本号获取特定版本的 ZigVersion 对象
    /// </summary>
    /// <param name="versionNumber">版本号</param>
    /// <returns>ZigVersion 对象，如果找不到则返回 null</returns>
    Task<ZigVersion?> GetVersionAsync(string versionNumber);

    /// <summary>
    /// 比较两个版本号的大小
    /// </summary>
    /// <param name="version1">第一个版本号</param>
    /// <param name="version2">第二个版本号</param>
    /// <returns>如果 version1 大于 version2 返回 1，等于返回 0，小于返回 -1</returns>
    int CompareVersions(string version1, string version2);
}
