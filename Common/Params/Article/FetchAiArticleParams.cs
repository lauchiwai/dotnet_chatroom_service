using System.Text.Json.Serialization;

namespace Common.Params.Article;

public class FetchAiArticleParams
{
    /// <summary>
    /// 提示詞
    /// </summary>
    [JsonPropertyName("prompt")]
    public required string Prompt { get; set; }
}
