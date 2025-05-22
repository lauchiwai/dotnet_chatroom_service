using System.Text.Json.Serialization;

namespace Common.Params.Vector;

public class VectorSearchParams
{
    /// <summary>
    /// 集合内的id
    /// </summary>
    [JsonPropertyName("id")]
    public required int Id { get; set; }

    /// <summary>
    /// 向量資料集 名稱
    /// </summary>
    [JsonPropertyName("collection_name")]
    public required string CollectionName { get; set; }

    /// <summary>
    /// 詢問條件
    /// </summary>
    [JsonPropertyName("query_text")]
    public required string QueryText { get; set; }
}
