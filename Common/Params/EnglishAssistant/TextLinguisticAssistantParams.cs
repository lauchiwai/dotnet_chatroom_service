using System.Text.Json.Serialization;

namespace Common.Params.EnglishAssistant;

public class TextLinguisticAssistantParams
{
    /// <summary>
    /// 要分析的文本內容 
    /// </summary>
    [JsonPropertyName("text")]
    public required string Text { get; init; }

    /// <summary>
    /// 附加指令資訊
    /// </summary>
    [JsonPropertyName("message")]
    public required string Message { get; init; }
}
