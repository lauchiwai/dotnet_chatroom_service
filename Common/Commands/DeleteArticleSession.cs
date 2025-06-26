using Common.Dto;
using MediatR;

namespace Common.Commands;
public class DeleteArticleSession : IRequest<ResultDTO>
{
    /// <summary>
    /// 文章ID
    /// </summary>
    public int ArticleId { get; set; }

    /// <summary>
    /// 關聯的聊天對話 Id
    /// </summary>
    public List<int> SessionIds { get; set; } = new List<int>();

}
