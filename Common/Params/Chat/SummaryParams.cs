using Common.Dto;
using System.Text.Json.Serialization;

namespace Common.Params.Chat;

public class SummaryParams: ChatBaseDto
{
    /// <summary>
    /// 向量數據集名稱
    /// </summary>
    [JsonPropertyName("collection_name")]
    public string CollectionName { get; set; } = "articles";

    /// <summary>
    /// 文章 id
    /// </summary>
    [JsonPropertyName("article_id")]
    public string? ArticleId { get; set; }
}
