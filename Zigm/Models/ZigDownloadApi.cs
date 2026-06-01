namespace Zigm.Models;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// 下载资源对象，包含 tarball 链接、shasum 和大小
/// </summary>
public class DownloadResource
{
    [JsonPropertyName("tarball")]
    public string? Tarball { get; set; }

    [JsonPropertyName("shasum")]
    public string? Shasum { get; set; }

    [JsonPropertyName("size")]
    public string? Size { get; set; }
}

/// <summary>
/// Zig 版本条目，包含版本信息和各种架构的下载资源
/// </summary>
public class VersionEntry
{
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("docs")]
    public string? Docs { get; set; }

    [JsonPropertyName("stdDocs")]
    public string? StdDocs { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("src")]
    public DownloadResource? Src { get; set; }

    [JsonPropertyName("bootstrap")]
    public DownloadResource? Bootstrap { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ArchitectureDownloads { get; set; }
}
