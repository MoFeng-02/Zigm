namespace Zigm.Models;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// System.Text.Json 源生成上下文，用于AOT编译，支持Zig版本服务中的JSON类型
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Dictionary<string, JsonElement>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
public partial class ZigJsonContext : JsonSerializerContext
{
    // 自动生成实现
}