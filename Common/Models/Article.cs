namespace Common.Models;

public partial class Article
{
    /// <summary>
    /// 文章編號
    /// </summary>
    public int ArticleID { get; set; }

    /// <summary>
    /// 文章標題
    /// </summary>
    public string ArticleTitle { get; set; } = null!;

    /// <summary>
    /// 文章内容
    /// </summary>
    public string ArticleContent { get; set; } = null!;

    /// <summary>
    /// 使用者id
    /// </summary>
    public string UserId { get; set; } = null!;

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdateTime { get; set; }
}
