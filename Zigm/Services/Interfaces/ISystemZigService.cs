namespace Zigm.Services.Interfaces;

/// <summary>
/// 系统 Zig 服务接口，负责检测和显示系统中已安装的 Zig 信息
/// </summary>
public interface ISystemZigService
{
    /// <summary>
    /// 检查系统中是否安装了 Zig
    /// </summary>
    /// <returns>如果安装了返回 true，否则返回 false</returns>
    bool IsZigInstalled();

    /// <summary>
    /// 获取系统中安装的 Zig 版本
    /// </summary>
    /// <returns>Zig 版本号，如果未安装则返回 null</returns>
    string? GetSystemZigVersion();

    /// <summary>
    /// 获取系统中 Zig 的安装路径
    /// </summary>
    /// <returns>Zig 安装路径，如果未安装则返回 null</returns>
    string? GetSystemZigPath();

    /// <summary>
    /// 在控制台显示系统中 Zig 的信息
    /// </summary>
    void ShowSystemZigInfo();
}
