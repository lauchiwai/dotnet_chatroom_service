namespace Common.Params.Article;

public class UpdateArticleReadingProgressParams
{
    /// <summary>
    /// 文章 編號
    /// </summary>
    public int ArticleId { get; set; }

    /// <summary>
    /// 觀看 進度
    /// </summary>
    public int Progress { get; set; }

}
