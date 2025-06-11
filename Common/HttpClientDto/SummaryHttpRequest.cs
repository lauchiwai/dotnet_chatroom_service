using Common.Dto;
using System.Text.Json.Serialization;

namespace Common.HttpClientDto;

public class SummaryHttpRequest : ChatBaseDto
{
    /// <summary>
    /// 向量數據集名稱
    /// </summary>
    [JsonPropertyName("collection_name")]
    public string CollectionName { get; set; } = null!;

    /// <summary>
    /// 文章 id
    /// </summary>
    [JsonPropertyName("article_id")]
    public int ArticleId { get; set; }

    /// <summary>
    /// 使用者 id
    /// </summary>
    [JsonPropertyName("user_id")]
    public required int UserId { get; set; }
}