using System.Text.Json.Serialization;

namespace Common.Params;

public class UpsertVectorCollectionParams
{
    /// <summary>
    /// 集合名稱
    /// </summary>
    [JsonPropertyName("collection_name")]
    public required string CollectionName { get; set; }

    /// <summary>
    /// 文本點列表
    /// </summary>
    [JsonPropertyName("points")]
    public required List<TextPoint> Points { get; set; }
}

public class TextPoint
{
    /// <summary>
    /// 文本內容
    /// </summary>
    [JsonPropertyName("text")]
    public required string Text { get; set; }

    /// <summary>
    /// 可選的 ID (Python 端默認 null)
    /// </summary>
    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Id { get; set; }
}
