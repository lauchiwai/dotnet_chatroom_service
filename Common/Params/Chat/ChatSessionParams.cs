using System.Text.Json.Serialization;

namespace Common.Params.Chat;

public class ChatSessionParams
{
    /// <summary>
    /// 聊天室名稱
    /// </summary>
    [JsonPropertyName("chat_session_name")]
    public string? ChatSessionName { get; set; } = null!;
}
