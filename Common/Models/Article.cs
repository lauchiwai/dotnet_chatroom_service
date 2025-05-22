namespace Common.Models;

public partial class Article
{
    public int ArticleID { get; set; }

    public string ArticleTitle { get; set; } = null!;

    public string ArticleContent { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public DateTime UpdateTime { get; set; }
}
