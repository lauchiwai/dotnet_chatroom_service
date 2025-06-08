namespace Common.Models;

public partial class Chat_Session
{
    public int SessionID { get; set; }

    public string? SessionName { get; set; }

    public int UserId { get; set; }

    public DateTime UpdateTime { get; set; }

    public virtual Article_Chat_Session? Article_Chat_Session { get; set; }

    public virtual Authenticate User { get; set; } = null!;
}
