using System.Text.Json.Serialization;

namespace Common.Params.Vector;

public class UpsertVectorCollectionParams
{
    /// <summary>
    /// 集合内的id
    /// </summary>
    [JsonPropertyName("id")]
    public required int Id { get; set; }

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
    /// ID
    /// </summary>
    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Id { get; set; }
}
