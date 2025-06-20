using System.Text.Json.Serialization;

namespace Common.Params.Chat;

public class SceneChatParams
{
    /// <summary>
    /// 聊天會話 id
    /// </summary>
    [JsonPropertyName("chat_session_id")]
    public required int ChatSessionId { get; set; }

    /// <summary>
    /// 新訊息
    /// </summary>
    [JsonPropertyName("message")]
    public required string Message { get; set; }
}
