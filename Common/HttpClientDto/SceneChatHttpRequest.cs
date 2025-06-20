using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.HttpClientDto;

public class SceneChatHttpRequest
{
    /// <summary>
    /// 聊天會話 id
    /// </summary>
    [JsonPropertyName("chat_session_id")]
    public required int ChatSessionId { get; set; }

    /// <summary>
    /// 使用者 id
    /// </summary>
    [JsonPropertyName("user_id")]
    public required int UserId { get; set; }

    /// <summary>
    /// 新訊息
    /// </summary>
    [JsonPropertyName("message")]
    public required string Message { get; set; }
}
