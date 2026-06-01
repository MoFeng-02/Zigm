namespace Zigm.Services.Interfaces;

/// <summary>
/// Zig 安装服务接口，负责下载、安装、卸载和切换 Zig 版本
/// </summary>
public interface IZigInstallerService
{
    /// <summary>
    /// 安装指定版本的 Zig
    /// </summary>
    /// <param name="version">版本号</param>
    /// <returns>安装是否成功</returns>
    Task<bool> InstallAsync(string version);

    /// <summary>
    /// 卸载指定版本的 Zig
    /// </summary>
    /// <param name="version">版本号</param>
    /// <returns>卸载是否成功</returns>
    bool Uninstall(string version);

    /// <summary>
    /// 切换到指定版本的 Zig
    /// </summary>
    /// <param name="version">版本号</param>
    /// <param name="target">环境变量目标（用户级或系统级）</param>
    /// <returns>切换是否成功</returns>
    bool SwitchToVersion(string version, EnvironmentVariableTarget target = EnvironmentVariableTarget.User);
}
