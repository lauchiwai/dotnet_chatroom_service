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
    /// 擁有者id
    /// </summary>
    public int OwnerId { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdateTime { get; set; }

    public virtual ICollection<Article_Chat_Session> Article_Chat_Session { get; set; } = new List<Article_Chat_Session>();

    public virtual ICollection<Article_User> Article_User { get; set; } = new List<Article_User>();

    public virtual Authenticate Owner { get; set; } = null!;
}
