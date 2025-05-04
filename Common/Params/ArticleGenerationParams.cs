using System.Text.Json.Serialization;

namespace Common.Params;

public class ArticleGenerationParams
{
    /// <summary>
    /// 向量資料集 名稱
    /// </summary>
    [JsonPropertyName("prompt")]
    public required string Prompt { get; set; }
}
