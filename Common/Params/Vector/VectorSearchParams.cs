using System.Text.Json.Serialization;

namespace Common.Params.Vector;

public class VectorSearchParams
{
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
