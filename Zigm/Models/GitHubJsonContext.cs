namespace Zigm.Models;

using System.Text.Json.Serialization;

/// <summary>
/// System.Text.Json 源生成上下文，用于AOT编译，支持GitHub API相关的JSON类型
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(GitHubRelease))]
[JsonSerializable(typeof(GitHubAsset))]
public partial class GitHubJsonContext : JsonSerializerContext
{
}
