using Common.Dto;
using System.Text.Json.Serialization;

namespace Common.HttpClientDto;

public class ChatHttpRequest : ChatBaseDto
{
    /// <summary>
    /// 新訊息
    /// </summary>
    [JsonPropertyName("message")]
    public required string Message { get; set; }

    /// <summary>
    /// 向量數據集名稱
    /// </summary>
    [JsonPropertyName("collection_name")]
    public string? CollectionName { get; set; }

    /// <summary>
    /// 文章 id
    /// </summary>
    [JsonPropertyName("article_id")]
    public int? ArticleId { get; set; }

    /// <summary>
    /// 使用者 id
    /// </summary>
    [JsonPropertyName("user_id")]
    public required int UserId { get; set; }
}