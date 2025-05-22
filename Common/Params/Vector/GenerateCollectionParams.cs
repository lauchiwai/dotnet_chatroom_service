using System.Text.Json.Serialization;

namespace Common.Params.Vector;

public class GenerateCollectionParams
{
    /// <summary>
    /// 集合名稱
    /// </summary>
    [JsonPropertyName("collection_name")]
    public required string CollectionName { get; set; }
}
