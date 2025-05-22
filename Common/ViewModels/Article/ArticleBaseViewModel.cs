namespace Common.ViewModels.Article;

public class ArticleBaseViewModel
{
    /// <summary>
    /// 文章id
    /// </summary>
    public int ArticleId { get; set; }

    /// <summary>
    /// 文章標題
    /// </summary>
    public string ArticleTitle { get; set; } = null!;
}
