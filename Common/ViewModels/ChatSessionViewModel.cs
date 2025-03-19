namespace Common.ViewModels;

public class ChatSessionViewModel
{
    /// <summary>
    /// 聊天會話id
    /// </summary>
    public int SessionId { get; set; }

    /// <summary>
    /// 聊天會話Name
    /// </summary>
    public string SessionName { get; set; } = null!;
}
