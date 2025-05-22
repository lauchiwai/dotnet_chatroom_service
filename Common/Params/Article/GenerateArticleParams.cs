namespace Common.Params.Article;

public class GenerateArticleParams
{
    /// <summary>
    /// 文章標題
    /// </summary>
    public string ArticleTitle { get; set; } = null!;

    /// <summary>
    /// 文章内容
    /// </summary>
    public string ArticleContent { get; set; } = null!;
}
