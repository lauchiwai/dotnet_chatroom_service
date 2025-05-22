namespace Common.ViewModels.Article;

public class ArticleViewModel
{
    /// <summary>
    /// 文章id
    /// </summary>
    public int ArticleId { get; set; }

    /// <summary>
    /// 文章内容
    /// </summary>
    public string ArticleContent { get; set; } = null!;
}
