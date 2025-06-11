using System.Text.Json.Serialization;

namespace Common.Params.EnglishAssistant;

public class WordAssistanParams
{
    /// <summary>
    /// 要分析的英文單詞
    /// </summary>
    /// <example>ubiquitous</example>
    [JsonPropertyName("word")]
    public required string Word { get; init; }

    /// <summary>
    /// 附加指令資訊
    /// </summary>
    /// <example>請包含詞源分析</example>
    [JsonPropertyName("message")]
    public required string Message { get; init; }
} 