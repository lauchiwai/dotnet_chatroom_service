using Common.Dto;
using MediatR;

public class VectorizeArticleCommand : IRequest<ResultDTO>
{
    /// <summary>
    ///  文章id
    /// </summary>
    public int ArticleId { get; set; }

    /// <summary>
    /// 集合名稱
    /// </summary>
    public string CollectionName { get; set; } = null!; 
}