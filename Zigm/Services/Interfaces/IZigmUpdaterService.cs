using Zigm.Models;

namespace Zigm.Services.Interfaces;


/// <summary>
/// Zigm 更新服务接口，负责检查和更新 Zigm 自身
/// </summary>
public interface IZigmUpdaterService
{
    /// <summary>
    /// 检查是否有可用的更新
    /// </summary>
    /// <returns>包含是否有更新、发布信息和错误信息的元组</returns>
    Task<(bool HasUpdate, GitHubRelease? Release, string? Error)> CheckForUpdatesAsync();

    /// <summary>
    /// 执行更新操作
    /// </summary>
    /// <param name="release">GitHub 发布信息</param>
    /// <returns>更新是否成功</returns>
    Task<bool> UpdateAsync(GitHubRelease release);
}
