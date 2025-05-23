﻿using System.Text.Json.Serialization;

namespace Common.Params;

public class ChatParams
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

    /// <summary>
    /// 新訊息
    /// </summary>
    [JsonPropertyName("message")]
    public required string Message { get; set; }

    /// <summary>
    /// 向量數據集名稱
    /// </summary>
    [JsonPropertyName("collection_name")]
    public string? CollectionName { get; set; }
}