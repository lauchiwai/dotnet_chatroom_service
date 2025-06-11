using System.Text.Json.Serialization;

namespace Common.Dto;

public class ChatBaseDto
{
    /// <summary>
    /// 聊天會話 id
    /// </summary>
    [JsonPropertyName("chat_session_id")]
    public required int ChatSessionId { get; set; }
}
