namespace Zigm.Models;

/// <summary>
/// Zig版本信息类，用于表示一个Zig版本的详细信息
/// </summary>
public class ZigVersion
{
    /// <summary>
    /// 版本号，例如：0.12.0
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// 发布日期
    /// </summary>
    public DateTime ReleaseDate { get; set; }

    /// <summary>
    /// 下载链接
    /// </summary>
    public Dictionary<string, string> DownloadUrls { get; set; }

    /// <summary>
    /// 版本类型，例如：stable、dev、nightly
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    public ZigVersion()
    {
        DownloadUrls = new Dictionary<string, string>();
    }

    /// <summary>
    /// 重写ToString方法，方便显示
    /// </summary>
    /// <returns>版本字符串</returns>
    public override string ToString()
    {
        return Version ?? string.Empty;
    }
}
