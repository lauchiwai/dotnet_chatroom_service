namespace Common.Models;

public partial class Article_Chat_Session
{
    public int SessionID { get; set; }

    public int ArticleId { get; set; }

    public virtual Article Article { get; set; } = null!;

    public virtual ChatSession Session { get; set; } = null!;
}
