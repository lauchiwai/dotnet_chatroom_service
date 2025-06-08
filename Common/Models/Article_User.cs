namespace Common.Models;

public partial class Article_User
{
    public int UserId { get; set; }

    public int ArticleId { get; set; }

    public int Progress { get; set; }

    public virtual Article Article { get; set; } = null!;

    public virtual Authenticate User { get; set; } = null!;
}
