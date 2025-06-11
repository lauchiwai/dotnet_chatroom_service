using System.Text.Json.Serialization;

namespace Common.Params.Vector;

public class CheckVectorDataExistParams
{
    /// <summary>
    /// 集合名稱
    /// </summary>
    [JsonPropertyName("collection_name")]
    public required string CollectionName { get; set; }

    /// <summary>
    /// id
    /// </summary>
    [JsonPropertyName("id")]
    public required int Id { get; set; }
}
