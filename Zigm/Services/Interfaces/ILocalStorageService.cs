namespace Zigm.Services.Interfaces;

/// <summary>
/// 本地存储服务接口，负责管理 Zig 版本在本地的存储和管理
/// </summary>
public interface ILocalStorageService
{
    /// <summary>
    /// 获取指定版本 Zig 的安装路径
    /// </summary>
    /// <param name="version">版本号</param>
    /// <returns>版本安装路径</returns>
    string GetVersionPath(string version);

    /// <summary>
    /// 获取当前使用版本 Zig 的安装路径
    /// </summary>
    /// <returns>当前版本安装路径</returns>
    string GetCurrentVersionPath();

    /// <summary>
    /// 获取 Zigm 的 bin 目录路径（用于符号链接）
    /// </summary>
    /// <returns>bin 目录路径</returns>
    string GetBinPath();

    /// <summary>
    /// 列出所有已安装的 Zig 版本
    /// </summary>
    /// <returns>已安装版本列表</returns>
    List<string> ListInstalledVersions();

    /// <summary>
    /// 检查指定版本是否已安装
    /// </summary>
    /// <param name="version">版本号</param>
    /// <returns>如果已安装返回 true，否则返回 false</returns>
    bool IsVersionInstalled(string version);

    /// <summary>
    /// 获取当前使用的 Zig 版本
    /// </summary>
    /// <returns>当前版本号，如果未设置则返回 null</returns>
    string? GetCurrentVersion();

    /// <summary>
    /// 异步获取当前使用的 Zig 版本
    /// </summary>
    /// <returns>当前版本号，如果未设置则返回 null</returns>
    Task<string?> GetCurrentVersionAsync();

    /// <summary>
    /// 设置当前使用的 Zig 版本
    /// </summary>
    /// <param name="version">版本号</param>
    void SetCurrentVersion(string version);

    /// <summary>
    /// 异步设置当前使用的 Zig 版本
    /// </summary>
    /// <param name="version">版本号</param>
    Task SetCurrentVersionAsync(string version);

    /// <summary>
    /// 安装指定版本的 Zig
    /// </summary>
    /// <param name="version">版本号</param>
    /// <param name="downloadPath">下载文件的路径</param>
    /// <returns>安装是否成功</returns>
    bool InstallVersion(string version, string downloadPath);

    /// <summary>
    /// 异步安装指定版本的 Zig
    /// </summary>
    /// <param name="version">版本号</param>
    /// <param name="downloadPath">下载文件的路径</param>
    /// <returns>安装是否成功</returns>
    Task<bool> InstallVersionAsync(string version, string downloadPath);

    /// <summary>
    /// 卸载指定版本的 Zig
    /// </summary>
    /// <param name="version">版本号</param>
    /// <returns>卸载是否成功</returns>
    bool UninstallVersion(string version);

    /// <summary>
    /// 异步卸载指定版本的 Zig
    /// </summary>
    /// <param name="version">版本号</param>
    /// <returns>卸载是否成功</returns>
    Task<bool> UninstallVersionAsync(string version);

    /// <summary>
    /// 清理临时文件
    /// </summary>
    /// <param name="filePath">要清理的文件路径</param>
    void CleanupTempFile(string filePath);

    /// <summary>
    /// 异步清理临时文件
    /// </summary>
    /// <param name="filePath">要清理的文件路径</param>
    Task CleanupTempFileAsync(string filePath);
}
