namespace Common.Params.Article;

public class VectorizeArticleParams
{
    /// <summary>
    /// 文章編號
    /// </summary>
    public int ArticleId { get; set; }

    /// <summary>
    /// 集合名稱
    /// </summary>
    public string CollectionName { get; set; } = null!;
}
