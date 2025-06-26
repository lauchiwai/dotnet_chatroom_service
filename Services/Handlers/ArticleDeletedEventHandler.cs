using Common.Commands;
using Common.Dto;
using MediatR;
using Services.Interfaces;

public class ArticleDeletedEventHandler : IRequestHandler<DeleteArticleSession, ResultDTO>
{
    private readonly IArticleService _articleService;
    private readonly IChatService _chatService;
    public ArticleDeletedEventHandler(
        IArticleService articleService,
        IChatService chatService)
    {
        _articleService = articleService;
        _chatService = chatService;
    }
    public async Task<ResultDTO> Handle(DeleteArticleSession request, CancellationToken ct)
    {

        var articleResult = await _articleService.DeleteArticle(request.ArticleId);
        if (!articleResult.IsSuccess)
        {
            return articleResult;
        }

        var failedSessions = new List<int>();
        foreach (var sessionId in request.SessionIds)
        {
            var sessionResult = await _chatService.DeleteChatData(sessionId);
            if (!sessionResult.IsSuccess)
            {
                failedSessions.Add(sessionId);
            }
        }

        if (failedSessions.Any())
        {
            return new ResultDTO
            {
                IsSuccess = false,
                Code = 207,
                Message = $"文章刪除成功，但有{failedSessions.Count}個會話刪除失敗。失敗會話ID: {string.Join(", ", failedSessions)}"
            };
        }
        return new ResultDTO
        {
            IsSuccess = true,
            Code = 200,
            Message = "文章及所有關聯會話已成功刪除"
        };
    }
}