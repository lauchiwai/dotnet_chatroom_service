using System.Text.Json.Serialization;

namespace Common.Dto;

public class ChatBaseDto
{
    /// <summary>
    /// 聊天會話 id
    /// </summary>
    [JsonPropertyName("chat_session_id")]
    public required string ChatSessionId { get; set; }

    /// <summary>
    /// 使用者 id
    /// </summary>
    [JsonPropertyName("user_id")]
    public required string UserId { get; set; }
}
