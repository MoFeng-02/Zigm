namespace Zigm.Models;

/// <summary>
/// 应用程序配置类，用于存储Zigm的配置信息
/// </summary>
public class Config
{
    /// <summary>
    /// Zig版本存储目录
    /// </summary>
    public string? StoragePath { get; set; }

    /// <summary>
    /// 是否自动检查更新
    /// </summary>
    public bool AutoCheckUpdates { get; set; } = true;

    /// <summary>
    /// 当前使用的Zig版本
    /// </summary>
    public string? CurrentVersion { get; set; }

    /// <summary>
    /// 下载超时时间（秒）
    /// </summary>
    public int DownloadTimeout { get; set; } = 300;

    /// <summary>
    /// 默认的Zig版本源
    /// </summary>
    public string? DefaultSource { get; set; } = "official";

    /// <summary>
    /// 常量下载源
    /// </summary>
    public const string ConstantDownloadSource = "https://ziglang.org/download/";

    /// <summary>
    /// 下载源（可为镜像源）
    /// </summary>
    public string? DownloadSource { get; set; } = ConstantDownloadSource;

    /// <summary>
    /// 本地化语言设置
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    public Config()
    {
    }
}